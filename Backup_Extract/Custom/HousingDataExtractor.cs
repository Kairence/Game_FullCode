using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Commands;
using Server.Items;
using Server.Regions;

namespace Server.Misc
{
    public class HousingDataExtractor
    {
        private const int ChunkSize = 128;

        // 에코그리드 장부 데이터를 담을 레코드
        private class GridChunk
        {
            public Map Map { get; set; }
            public int CX { get; set; }
            public int CY { get; set; }
            public int BaseRegionCode { get; set; }
            public int ZoneType { get; set; } // 1:도시, 2:근교, 3:영토, 0:외곽
            public int FinalZoneID { get; set; }
            public List<Rectangle2D> Lots { get; set; } = [];
        }

        public static void Initialize()
        {
            CommandSystem.Register("ExHousing", AccessLevel.Administrator, OnExtractHousingData);
        }

        private static void OnExtractHousingData(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendMessage(68, "현미경 타일 전수조사 및 에코그리드 영토 병합을 시작합니다...");
            from.SendMessage(33, "경고: 맵 전체를 스캔하므로 서버가 수 분간 멈출 수 있습니다!"); // 🌟 진짜 오래 걸릴 겁니다.

            string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string masterGridPath = Path.Combine(Core.BaseDirectory, "Data", "EcoGrid_Master_AllMaps.csv");
            string outStatusPath = Path.Combine(Core.BaseDirectory, "Data", $"Housing_Chunk_Status_{ts}.csv");
            string outDataPath = Path.Combine(Core.BaseDirectory, "Data", $"Housing_Master_Data_{ts}.csv");

            // 1. 에코그리드 마스터 장부는 '참고용(BaseRegionCode 매핑용)'으로만 로드
            Dictionary<(Map, int, int), int> baseRegions = LoadBaseRegionCodes(masterGridPath);

            // 2. 🌟 [핵심 수정] 타겟 맵 전체의 모든 청크를 강제로 메모리에 생성 (야외 0번 포함)
            Map[] targetMaps = { Map.Felucca, Map.Trammel, Map.Malas, Map.Tokuno, Map.TerMur };
            Dictionary<(Map, int, int), GridChunk> allChunks = new();

            foreach (Map map in targetMaps)
            {
                if (map == null || map == Map.Internal) continue;
                
                int maxCX = map.Width / ChunkSize;
                int maxCY = map.Height / ChunkSize;

                for (int cx = 0; cx <= maxCX; cx++)
                {
                    for (int cy = 0; cy <= maxCY; cy++)
                    {
                        // 🌟 [추가] 청크의 중심부(Center)를 찔러서 소속 지역(Region)을 확인합니다.
                        int centerX = (cx * ChunkSize) + (ChunkSize / 2);
                        int centerY = (cy * ChunkSize) + (ChunkSize / 2);
                        Region chunkRegion = Region.Find(new Point3D(centerX, centerY, 0), map);

                        // 그린 에이커, 감옥, 던전 등은 아예 청크 목록에 올리지도 않고 버립니다!
                        if (chunkRegion != null)
                        {
                            string regName = chunkRegion.Name?.ToLower() ?? "";
                            if (regName.Contains("green acres") || 
                                regName.Contains("jail") || 
                                chunkRegion.IsPartOf(typeof(Server.Regions.DungeonRegion)) ||
                                chunkRegion.IsPartOf(typeof(Server.Regions.GuardedRegion))) // 마을 내부 강제 스킵 원할 시
                            {
                                continue; 
                            }
                        }

                        // 통과된 청크만 장부에 코드가 있으면 가져오고, 없으면 0(야생)으로 세팅
                        int regionCode = baseRegions.TryGetValue((map, cx, cy), out int code) ? code : 0;
                        allChunks[(map, cx, cy)] = new GridChunk { Map = map, CX = cx, CY = cy, BaseRegionCode = regionCode };
                    }
                }
            }

            from.SendMessage(50, $"맵 그리드 생성 완료: 총 {allChunks.Count}개의 청크를 샅샅이 뒤집니다.");

            int totalLots = 0;

            // 3. 1x1 현미경 전수조사 (가장 오래 걸리는 작업)
            foreach (var chunk in allChunks.Values)
            {
                chunk.Lots = ScanChunkForMaximalLots(chunk.Map, chunk.CX * ChunkSize, chunk.CY * ChunkSize);
                totalLots += chunk.Lots.Count;
            }

            from.SendMessage(50, $"전수조사 완료: 총 {totalLots}개의 순수 공터 확보. 영토(Belt) 판정을 시작합니다...");

            // 4. 구역(Zone) 1차 판정
            foreach (var chunk in allChunks.Values)
            {
                if (chunk.BaseRegionCode > 0)
                {
                    chunk.ZoneType = chunk.Lots.Count > 0 ? 2 : 1; 
                    chunk.FinalZoneID = (chunk.BaseRegionCode * 10) + chunk.ZoneType;
                }
                else
                {
                    chunk.ZoneType = 0; // 완전 야생
                    chunk.FinalZoneID = 0; // 유저님 기획대로 인덱스는 0
                }
            }

            // 5. 구역(Zone) 2차 판정: 도시/근교 바로 옆의 야생(0)은 영토(3) 벨트로 편입
            foreach (var chunk in allChunks.Values.Where(c => c.ZoneType == 0 && c.Lots.Count > 0))
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        if (allChunks.TryGetValue((chunk.Map, chunk.CX + dx, chunk.CY + dy), out var neighbor))
                        {
                            // 이웃이 도시(1)거나 근교(2)면 나는 영토(3)가 된다.
                            if (neighbor.ZoneType == 1 || neighbor.ZoneType == 2)
                            {
                                chunk.ZoneType = 3;
                                chunk.FinalZoneID = (neighbor.BaseRegionCode * 10) + 3;
                                break; 
                            }
                        }
                    }
                    if (chunk.ZoneType == 3) break;
                }
            }

            // 6. 결과물 파일(2개) 추출
            using StreamWriter swStatus = new(outStatusPath);
            using StreamWriter swData = new(outDataPath);

            swStatus.WriteLine("MapName,ChunkX,ChunkY,BaseRegionCode,ZoneType,FinalZoneID,LotsCount");
            swData.WriteLine("MapName,X,Y,Z,Width,Height,ZoneID");

            int finalLotsWritten = 0;

            foreach (var chunk in allChunks.Values)
            {
                swStatus.WriteLine($"{chunk.Map.Name},{chunk.CX},{chunk.CY},{chunk.BaseRegionCode},{chunk.ZoneType},{chunk.FinalZoneID},{chunk.Lots.Count}");

                // 🌟 [핵심] ZoneType 필터링 삭제. 집터(Lots)가 1개라도 있으면 Zone 0(야생)이든 뭐든 무조건 다 기록!
                if (chunk.Lots.Count > 0)
                {
                    foreach (var lot in chunk.Lots)
                    {
                        int z = chunk.Map.GetAverageZ(lot.X, lot.Y);
                        // FinalZoneID가 0으로 찍혀서 나감 (나중에 시스템에서 갈아끼우기 완벽 대응)
                        swData.WriteLine($"{chunk.Map.Name},{lot.X},{lot.Y},{z},{lot.Width},{lot.Height},{chunk.FinalZoneID}");
                        finalLotsWritten++;
                    }
                }
            }

            from.SendMessage(66, $"추출 완료! 맵 전역에 분양 가능한 집터 {finalLotsWritten}개가 영구 기록되었습니다.");
        }
        // ==============================================================================
        // 🌟 [핵심] 현미경 스캔 알고리즘
        // ==============================================================================
        private static List<Rectangle2D> ScanChunkForMaximalLots(Map map, int startX, int startY)
        {
            List<Rectangle2D> lots = [];
            bool[,] validTile = new bool[ChunkSize, ChunkSize];
            int[,] zMap = new int[ChunkSize, ChunkSize];
            bool[,] visited = new bool[ChunkSize, ChunkSize];

            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < ChunkSize; y++)
                {
                    int wx = startX + x;
                    int wy = startY + y;
                    
                    if (wx >= map.Width || wy >= map.Height) continue;

                    var (isSafe, tileZ) = IsSingleTileSafe(map, wx, wy);
                    validTile[x, y] = isSafe;
                    zMap[x, y] = tileZ;
                }
            }

            // Greedy Expansion
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    if (!validTile[x, y] || visited[x, y]) continue;

                    int bestW = 0, bestH = 0, maxArea = 0;

                    for (int w = 9; x + w <= ChunkSize; w++)
                    {
                        if (!validTile[x + w - 1, y] || visited[x + w - 1, y]) break;

                        int h = 1;
                        bool canExpand = true;
                        while (y + h < ChunkSize && canExpand)
                        {
                            for (int dx = 0; dx < w; dx++)
                            {
                                if (!validTile[x + dx, y + h] || visited[x + dx, y + h])
                                {
                                    canExpand = false;
                                    break;
                                }
                            }
                            if (canExpand) h++;
                        }

                        if (h >= 9 && w * h > maxArea)
                        {
                            if (IsFlatArea(zMap, x, y, w, h))
                            {
                                maxArea = w * h;
                                bestW = w;
                                bestH = h;
                            }
                        }
                    }

                    if (maxArea >= 81)
                    {
                        lots.Add(new Rectangle2D(startX + x, startY + y, bestW, bestH));
                        for (int i = 0; i < bestW; i++)
                            for (int j = 0; j < bestH; j++)
                                visited[x + i, y + j] = true;
                    }
                }
            }

            return lots;
        }

        // ==============================================================================
        // 🌟 낚시 코드 기반의 완벽한 물 검증 헬퍼 메서드
        // ==============================================================================
        private static bool IsWaterTile(int id)
        {
            // Fishing.cs에서 가져온 물 타일 검증 로직
            return (id >= 0x00A8 && id <= 0x00AB) || 
                   (id >= 0x0136 && id <= 0x0137) || 
                   (id >= 0x5797 && id <= 0x579C) || 
                   (id >= 0x746E && id <= 0x7485) || 
                   (id >= 0x7490 && id <= 0x74AB) || 
                   (id >= 0x74B5 && id <= 0x75D5) || 
                   (id >= 0x1796 && id <= 0x17B2); // 얕은 해안가 스태틱 타일 추가
        }

        // ==============================================================================
        // 🌟 [통합 물 검증 헬퍼] 낚시 코드 + 플래그 + 이름 무차별 필터링
        // ==============================================================================
        private static bool IsWater(int id, TileFlag flags, string name)
        {
            // 1. Wet(물) 플래그가 있으면 무조건 컷
            if ((flags & TileFlag.Wet) != 0) return true;

            // 2. 낚시가 가능한 물/해안가 타일 하드코딩 블랙리스트
            bool isHardcodedWater = 
                   (id >= 0x00A8 && id <= 0x00AB) || 
                   (id >= 0x0136 && id <= 0x0137) || 
                   (id >= 0x5797 && id <= 0x579C) || 
                   (id >= 0x746E && id <= 0x7485) || 
                   (id >= 0x7490 && id <= 0x74AB) || 
                   (id >= 0x74B5 && id <= 0x75D5) || 
                   (id >= 0x1796 && id <= 0x17B2);

            if (isHardcodedWater) return true;

            // 3. 이름에 물, 바다, 강, 늪, 용암이 들어가면 컷
            if (!string.IsNullOrEmpty(name))
            {
                string lowerName = name.ToLower();
                if (lowerName.Contains("water") || lowerName.Contains("ocean") || 
                    lowerName.Contains("sea") || lowerName.Contains("river") || 
                    lowerName.Contains("swamp") || lowerName.Contains("lava") ||
                    lowerName.Contains("lake") || lowerName.Contains("pond"))
                {
                    return true;
                }
            }

            return false;
        }

        // ==============================================================================
        // 🌟 [최종 스캔 필터] 오리지널 UO 룰 + Z축 융단폭격 차단
        // ==============================================================================
        private static (bool IsSafe, int Z) IsSingleTileSafe(Map map, int x, int y)
        {
            int z = 0;
            LandTile landTile = map.Tiles.GetLandTile(x, y);
            int landID = landTile.ID & TileData.MaxLandValue;
            LandData landData = TileData.LandTable[landID];
            
            int landStartZ = 0, landAvgZ = 0, landTopZ = 0;
            map.GetAverageZ(x, y, ref landStartZ, ref landAvgZ, ref landTopZ);
            z = landAvgZ; // UO 오리지널과 동일하게 평균 Z값 사용

            // 🌟 [절대 원칙] 울티마 온라인의 바다/강 표면은 정확히 '-5' 입니다. 
            // -5 이하의 땅은 물속이므로 이유 불문하고 무조건 잘라냅니다. (이전 버그의 핵심 원인 해결)
            if (z <= -5) return (false, z);

            // 1. [Land 규칙] 이동불가거나, 통합 물 검증에 걸리면 차단
            if (landTile.Ignored || (landData.Flags & TileFlag.Impassable) != 0 || IsWater(landID, landData.Flags, landData.Name)) 
                return (false, z);

            // HousePlacement.cs 기반의 길바닥 차단
            bool isRoadLand = (landID >= 0x0071 && landID <= 0x0078) || 
                              (landID >= 0x00E8 && landID <= 0x00EB) || 
                              (landID >= 0x07AE && landID <= 0x07B1) || 
                              (landID == 0x3FF4) || 
                              (landID >= 0x3FF8 && landID <= 0x3FFB) || 
                              (landID >= 0x0442 && landID <= 0x0479) || 
                              (landID >= 0x0501 && landID <= 0x0510) || 
                              (landID >= 0x0009 && landID <= 0x0015) || 
                              (landID >= 0x0150 && landID <= 0x015C) || 
                              (landID >= 0x011E && landID <= 0x0125) || 
                              (landID >= 0x0088 && landID <= 0x008B) || 
                              (landID >= 0x0141 && landID <= 0x0144) || 
                              (landID >= 0x028A && landID <= 0x0291) || 
                              (landID >= 0x0335 && landID <= 0x035C);
            if (isRoadLand) return (false, z);

            // 2. [Static 규칙] 
            StaticTile[] statics = map.Tiles.GetStaticTiles(x, y, true);
            foreach (var st in statics)
            {
                int staticID = st.ID & TileData.MaxItemValue;
                ItemData id = TileData.ItemTable[staticID];

                // 🌟 스태틱 타일에도 통합 물/늪 검증 들이대기
                if (IsWater(staticID, id.Flags, id.Name)) return (false, z);

                // 스캐너의 특권: 풀/나무, Z축 2 이하의 잡동사니 무시
                if ((id.Flags & TileFlag.Foliage) != 0) continue;
                if (id.Height > 0 && id.Height <= 2 && !id.Impassable) continue;

                // 해당 사물이 이동 불가(Impassable)이거나, 표면(Surface)이면서 바닥재(Background)가 아니면 차단!
                if (id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0))
                {
                    return (false, z);
                }
                
                z = Math.Max(z, st.Z + id.Height);
            }

            // 사물 높이 반영 후 혹시라도 바다 높이(-5) 이하면 다시 컷
            if (z <= -5) return (false, z); 
            
            Region reg = Region.Find(new Point3D(x, y, z), map);
            if (reg != null && !reg.IsDefault) 
            {
                if (!reg.AllowHousing(null, new Point3D(x, y, z))) return (false, z);
                if (reg is Server.Regions.GuardedRegion || reg.IsPartOf(typeof(Server.Regions.GuardedRegion))) 
                    return (false, z);
            }

            return (true, z);
        }

        private static bool IsFlatArea(int[,] zMap, int x, int y, int w, int h)
        {
            int minZ = 127, maxZ = -128;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int z = zMap[x + i, y + j];
                    if (z < minZ) minZ = z;
                    if (z > maxZ) maxZ = z;
                    
                    if (maxZ - minZ > 8) return false;
                }
            }
            return true;
        }

        private static Dictionary<(Map, int, int), int> LoadBaseRegionCodes(string path)
        {
            var dict = new Dictionary<(Map, int, int), int>();
            if (!File.Exists(path)) return dict;

            string[] lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] p = lines[i].Split(',');
                if (p.Length < 6) continue;

                string mapStr = p[0].Trim();
                Map m = mapStr switch { "Trammel" => Map.Trammel, "Felucca" => Map.Felucca, "Malas" => Map.Malas, "Tokuno" => Map.Tokuno, "TerMur" => Map.TerMur, _ => null };
                if (m == null) continue;

                int cx = int.Parse(p[1]), cy = int.Parse(p[2]);
                int regionCode = int.Parse(p[5]);

                dict[(m, cx, cy)] = regionCode;
            }
            return dict;
        }
    }
}