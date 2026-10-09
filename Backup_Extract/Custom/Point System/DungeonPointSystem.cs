using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public class DungeonPointInfo
    {
        public string Name { get; set; }
        public int SilverPointIndex { get; set; }
        public int Multiplier { get; set; }
        public Map Facet { get; set; }
        public bool IsField { get; set; }
    }

    public static class DungeonPointSystem
    {
        public static Dictionary<RegionCode, DungeonPointInfo> ActiveRegions { get; private set; }

        public static void Configure()
        {
            ActiveRegions = new Dictionary<RegionCode, DungeonPointInfo>();
            EventSink.ServerStarted += Initialize;
        }

        private static void Initialize()
        {
            int[] baseIndices = { 1, 26, 51, 66, 81, 91 }; 
            int[] dungeonCap = { 17, 19, 9, 9, 4, 4 }; 

            ActiveRegions[(RegionCode)199999] = new DungeonPointInfo
            {
                Name = "트라멜 통합 기록",
                SilverPointIndex = 1,
                Multiplier = 1,
                Facet = Map.Trammel,
                IsField = true
            };

            var groupedDungeons = DungeonManager.ZoneList.GroupBy(z => (int)z.RCode / 100000);

            foreach (var group in groupedDungeons)
            {
                int continent = group.Key;
                if (continent < 1 || continent > 6) continue;

                int baseIdx = baseIndices[continent - 1];
                int maxDungeons = dungeonCap[continent - 1];
                int offset = (continent == 1) ? 1 : 0; 
                
                // 🌟 [오류 해결] 캐스팅 대신 명시적인 Map 할당
                Map dungeonFacet = continent switch { 
                    1 => Map.Trammel, 
                    2 => Map.Felucca, 
                    3 => Map.Ilshenar, 
                    4 => Map.Malas, 
                    5 => Map.Tokuno, 
                    _ => Map.TerMur 
                };

                var dungeons = group.GroupBy(z => ((int)z.RCode / 100) * 100);
                foreach (var dGroup in dungeons)
                {
                    if (offset >= (continent == 1 ? maxDungeons + 1 : maxDungeons)) break;

                    RegionCode majorCode = (RegionCode)dGroup.Key;
                    int codeID = (int)majorCode;
                    
                    if (codeID % 10000 == 900 && codeID != 220900 && codeID != 221900) continue; 

                    bool isDungeon = (codeID / 10000 % 10 == 2);
                    bool isWind = (codeID == 111400);

                    if (!isDungeon && !isWind) continue;

                    string dName = isWind ? "Wind Park" : NewSpawnManager.GetDisplayName(majorCode).Replace("Level 1", "").Trim();
                    if (codeID == 220900 || codeID == 221900) dName = "Khaldun";

                    ActiveRegions[majorCode] = new DungeonPointInfo
                    {
                        Name = dName,
                        SilverPointIndex = baseIdx + offset++, 
                        Multiplier = Math.Clamp((int)(dGroup.Sum(z => (long)z.TargetHeat) / 500000) + 1, 1, 10),
                        Facet = dungeonFacet, // 🌟 수정된 부분 적용
                        IsField = false
                    };
                }
            }

            string[] fieldTypes = { "숲/평원", "사막/황무지", "설원/빙하", "늪지대", "화산/특수", "해양/심해" };
            for (int i = 1; i <= 6; i++)
            {
                int bIdx = baseIndices[i - 1];
                int fieldStartOffset = (i == 1) ? 18 : dungeonCap[i - 1]; 
                Map facet = i switch { 1 => Map.Trammel, 2 => Map.Felucca, 3 => Map.Ilshenar, 4 => Map.Malas, 5 => Map.Tokuno, _ => Map.TerMur };

                for (int f = 0; f < 6; f++)
                {
                    RegionCode fieldCode = (RegionCode)(i * 100000 + 40000 + f);
                    ActiveRegions[fieldCode] = new DungeonPointInfo
                    {
                        Name = fieldTypes[f],
                        SilverPointIndex = bIdx + fieldStartOffset + f, 
                        Multiplier = 1,
                        Facet = facet,
                        IsField = true
                    };
                }
            }
        }

        public static int GetOptionIDFromIndex(int index)
        {
            return index switch
            {
                1 => 0, 2 => 1, 3 => 2, 4 => 3, 5 => 5, 6 => 6, 7 => 7, 8 => 4,
                9 => 9, 10 => 10, 11 => 11, 12 => 12, 13 => 13, 14 => 14, 15 => 15, 16 => 16,
                17 => 21, 18 => 22, 19 => 23, 20 => 24, 21 => 25, 22 => 27,
                23 => 31, 24 => 32, 25 => 33, 26 => 34,
                27 => 35, 28 => 36, 29 => 37, 30 => 38, 31 => 39,
                32 => 45, 33 => 46, 34 => 47,
                35 => 49, 36 => 50, 37 => 51, 38 => 52,
                39 => 53, 40 => 55, 41 => 57, 42 => 58,
                43 => 60, 44 => 62, 45 => 63, 46 => 64, 47 => 65, 48 => 66,
                _ => -1
            };
        }

        public static string GetStatName(int index)
        {
            int optID = GetOptionIDFromIndex(index);
            return optID switch
            {
                0 => "힘 증가", 1 => "민첩 증가", 2 => "지능 증가", 3 => "모든 스탯", 4 => "운",
                5 => "최대 체력", 6 => "최대 기력", 7 => "최대 마나",
                9 => "무기 피해%", 10 => "주문 피해%", 11 => "모든 피해%",
                12 => "공격 속도%", 13 => "시전 속도%", 14 => "모든 속도%", 15 => "명중률%", 16 => "방어율%",
                21 => "물리 저항%", 22 => "화염 저항%", 23 => "냉기 저항%", 24 => "독 저항%", 25 => "에너지 저항%", 27 => "모든 저항%",
                31 => "물리 치명타%", 32 => "마법 치명타%", 33 => "물리 치명피해%", 34 => "마법 치명피해%",
                35 => "추가 물리피해", 36 => "추가 화염피해", 37 => "추가 냉기피해", 38 => "추가 독피해", 39 => "추가 에너지피해",
                45 => "체력 재생", 46 => "기력 재생", 47 => "마나 재생",
                49 => "체력 흡수%", 50 => "기력 흡수%", 51 => "마나 흡수%", 52 => "모든 흡수%",
                53 => "체력 획득", 55 => "마나 획득", 57 => "치유량 증가", 58 => "치유량 증가%",
                60 => "무기공격 반사%", 62 => "금화 획득%", 63 => "매직 획득%", 64 => "마나소모 감소%", 65 => "기력소모 감소%", 66 => "모든소모 감소%",
                _ => "미확인"
            };
        }

        public static int GetIncrementValue(int index)
        {
            int optID = GetOptionIDFromIndex(index);
            return optID switch
            {
                0 or 1 or 2 or 4 or 5 or 6 or 7 => 20000, 3 => 10000,
                9 => 1000, 10 => 1500, 11 => 600, 12 => 500, 13 => 1000, 14 => 300, 15 or 16 => 500,
                21 or 22 or 23 or 24 or 25 => 400, 27 => 200, 31 or 32 => 250, 33 or 34 => 750,
                >= 35 and <= 39 => 300, 45 or 46 or 47 => 300, 49 or 50 or 51 => 50, 52 => 20,
                53 or 55 => 300, 57 => 2000, 58 => 750, 60 => 2000, 62 or 63 => 400,
                64 or 65 => 250, 66 => 150, _ => 10000
            };
        }

        public static int GetRequiredSilverIdx(int index)
        {
            return (index + 1) / 2;
        }

        public static void DoTrain(PlayerMobile pm, int index)
        {
            if (pm == null || index <= 0 || index > 48) return;
            int silverIdx = GetRequiredSilverIdx(index);
            int perLevel = GetIncrementValue(index);
            int curLevel = pm.GoldPoint[index] / perLevel;
            if (curLevel >= 1000) { pm.SendMessage(0x22, "최대 단계입니다."); return; }
            int cost = (curLevel * 100) + 1000;

            if (pm.SilverPoint[silverIdx] >= cost)
            {
                pm.SilverPoint[silverIdx] -= cost;
                pm.GoldPoint[index] += perLevel;
                pm.UpdateEquipOptions();
                pm.SendMessage(0x42, $"{GetStatName(index)} 강화 성공!");
            }
            else pm.SendMessage(0x22, "포인트가 부족합니다.");
        }

        public static void ApplyGoldPointOptions(PlayerMobile pm, int[] totalOptions)
        {
            if (pm == null || pm.GoldPoint == null || totalOptions == null) return;
            for (int i = 1; i <= 48; i++)
            {
                int val = pm.GoldPoint[i];
                if (val <= 0) continue;
                int optID = GetOptionIDFromIndex(i); 
                if (optID >= 0 && optID < totalOptions.Length) totalOptions[optID] += val;
            }
        }
    }
}