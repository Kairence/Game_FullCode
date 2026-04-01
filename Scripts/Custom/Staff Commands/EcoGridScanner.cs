using System;
using System.IO;
using Server;
using Server.Commands;
using Server.Misc; // RegionSaver 연동

namespace Server.Custom
{
    public class EcoGridScanner
    {
        public static void Initialize()
        {
            CommandSystem.Register("SEG", AccessLevel.Administrator, new CommandEventHandler(OnScanAllEcoGrids));
        }

        private static void OnScanAllEcoGrids(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendMessage(68, "전 대륙 생태계 마스터 데이터 추출을 시작합니다. (수 초 소요될 수 있습니다...)");

            // 모든 맵의 데이터를 하나의 CSV 파일로 통합
            string outputPath = Path.Combine(Core.BaseDirectory, "Logs", $"EcoGrid_Master_AllMaps_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using StreamWriter writer = new(outputPath);
            // 🌟 CS 파일 변환을 위한 최종 헤더
            writer.WriteLine("MapName,ChunkX,ChunkY,CenterX,CenterY,RegionCode,OreCap,WoodCap,FishCap,FarmCap,TanCap");

            int chunkSize = 128;
            int totalValidCount = 0;
            int totalExcludedCount = 0;

            // 스캔할 맵 목록 (Internal 제외)
            Map[] mapsToScan = [Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur];

            foreach (Map map in mapsToScan)
            {
                if (map == null || map == Map.Internal) continue;

                int totalChunksX = map.Width / chunkSize;
                int totalChunksY = map.Height / chunkSize;
                int totalTilesPerChunk = chunkSize * chunkSize;

                for (int cx = 0; cx < totalChunksX; cx++)
                {
                    for (int cy = 0; cy < totalChunksY; cy++)
                    {
                        int startX = cx * chunkSize;
                        int startY = cy * chunkSize;
                        int centerX = startX + (chunkSize / 2);
                        int centerY = startY + (chunkSize / 2);

                        // ==========================================
                        // 🚨 [하드코딩 컷오프] 터머(Ter Mur) 특수 구역 즉시 제외
                        // ==========================================
                        if (map == Map.TerMur)
                        {
                            // 1. 에오돈 특수 공간 (32, 1976 ~ 574, 2239)
                            if (centerX >= 32 && centerX <= 574 && centerY >= 1976 && centerY <= 2239)
                            {
                                totalExcludedCount++;
                                continue;
                            }
                            // 2. 지하/어비스 등 던전 공간 (0, 2270 ~ 925, 2835)
                            if (centerX >= 0 && centerX <= 925 && centerY >= 2270 && centerY <= 2835)
                            {
                                totalExcludedCount++;
                                continue;
                            }
                        }

                        int voidCount = 0;
                        int waterCount = 0;
                        int treeCount = 0;
                        int rockCount = 0;
                        int sandCount = 0;

                        // 128x128 타일 정밀 스캔
                        for (int x = startX; x < startX + chunkSize; x++)
                        {
                            for (int y = startY; y < startY + chunkSize; y++)
                            {
                                LandTile landTile = map.Tiles.GetLandTile(x, y);
                                int landId = landTile.ID & TileData.MaxLandValue;
                                TileFlag landFlags = TileData.LandTable[landId].Flags;
                                string landName = TileData.LandTable[landId].Name ?? "";

                                // 1. 검은 구역(Void) 및 말라스 별(Star) 판정
                                if (landId == 0 || landId == 2 || landId == 0x244 || landTile.Ignored ||
                                    landName.Contains("star", StringComparison.OrdinalIgnoreCase) ||
                                    landName.Contains("void", StringComparison.OrdinalIgnoreCase))
                                {
                                    voidCount++;
                                    continue; 
                                }

                                // 2. 물(Water) 판정
                                if ((landFlags & TileFlag.Wet) != 0) waterCount++;

                                // 3. 바위/산맥(Rock/Mountain) 판정
                                if (landName.Contains("rock", StringComparison.OrdinalIgnoreCase) || 
                                    landName.Contains("mountain", StringComparison.OrdinalIgnoreCase) || 
                                    landName.Contains("cave", StringComparison.OrdinalIgnoreCase))
                                {
                                    rockCount++;
                                }

                                // 4. 모래/사막(Sand) 판정
                                if (landName.Contains("sand", StringComparison.OrdinalIgnoreCase)) sandCount++;

                                // 5. 나무/숲(Tree/Foliage) 판정 (스태틱 아이템 스캔)
                                StaticTile[] staticTiles = map.Tiles.GetStaticTiles(x, y);
                                foreach (var st in staticTiles)
                                {
                                    int staticId = st.ID & TileData.MaxItemValue;
                                    TileFlag itemFlags = TileData.ItemTable[staticId].Flags;
                                    string itemName = TileData.ItemTable[staticId].Name ?? "";

                                    if ((itemFlags & TileFlag.Foliage) != 0 || itemName.Contains("tree", StringComparison.OrdinalIgnoreCase))
                                    {
                                        treeCount++;
                                    }
                                }
                            }
                        }

                        // 🌟 [필터 1] 빈 공간/별밭이 50% 이상이면 가차없이 버림
                        if (voidCount >= (totalTilesPerChunk / 2))
                        {
                            totalExcludedCount++;
                            continue;
                        }

                        // 중심점 RegionCode 획득
                        int centerZ = map.GetAverageZ(centerX, centerY);
                        var (major, minor) = RegionSaver.GetRegionCodes(map, centerX, centerY, centerZ);
                        RegionCode displayCode = minor != RegionCode.None ? minor : major;
                        string codeName = displayCode.ToString();

                        // 🌟 [필터 2] 던전 및 특수 구역(Green Acres 등) 버림
                        if (codeName.Contains("Dungeon") || codeName.Contains("Internal"))
                        {
                            totalExcludedCount++;
                            continue;
                        }

                        // ==========================================
                        // 🌟 5대 자원 수학적 산출 공식
                        // ==========================================
                        
                        // 1. 광산: 바위 타일 * 5 + (모래 100타일 이상이면 보너스 200)
                        int oreCap = (rockCount * 5) + (sandCount >= 100 ? 200 : 0);

                        // 2. 벌목: 나무 타일 * 2
                        int woodCap = treeCount * 2;

                        // 3. 낚시: 수질 등급에 따른 용량 부여
                        int fishCap = 0;
                        if (waterCount >= 15000) fishCap = 4000;      // 90% 이상: 심해 (Deep Sea)
                        else if (waterCount >= 5000) fishCap = 2000;  // 30% 이상: 해안선 (Coastal)
                        else if (waterCount >= 100) fishCap = 1000;   // 약간: 강/호수 (River)

                        // 4. 농장: 파밍랜드 지정 구역이면 1000, 아니면 0
                        int farmCap = codeName.Contains("Farmlands") ? 1000 : 0;

                        // 🌟 [기획 연동] 마을 인접성 페널티 (가로수 및 마을 내 동물 억제)
                        if (codeName.Contains("Town") && !codeName.Contains("Farmlands"))
                        {
                            woodCap = (int)(woodCap * 0.3); // 벌목량 30%로 하향
                        }

                        // 5. 무두질(동물 스폰): 나무 용량을 따라감
                        int tanCap = woodCap;

                        // 최종 유효 데이터 CSV 기록
                        writer.WriteLine($"{map.Name},{cx},{cy},{centerX},{centerY},{displayCode},{oreCap},{woodCap},{fishCap},{farmCap},{tanCap}");
                        totalValidCount++;
                    }
                }
            }

            from.SendMessage(68, $"전 대륙 마스터 추출 완료! 고정 야생 청크: {totalValidCount}개 / 제거된 잉여 청크: {totalExcludedCount}개");
            from.SendMessage(68, "이 CSV 파일을 확인하신 후, 다시 코드로 변환(CS 추출)하는 단계를 진행합시다.");
        }
    }
}