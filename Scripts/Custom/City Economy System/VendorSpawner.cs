using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Server;
using Server.Mobiles;
using Server.Misc;

namespace Server.Misc
{
    public static class VendorSpawner
    {
        // [1] 가장 가까운 마을 탐색 및 거리 반환
        public static string FindNearestTown(Point3D loc, Map map, out double distance)
        {
            string nearest = "Wilderness";
            distance = 5000.0; // 기본 아주 먼 거리

            foreach (var kvp in TownEconomyManager.Towns)
            {
                if (kvp.Value.Facet == map)
                {
                    double d = GetDistance(loc, kvp.Value.Center);
                    if (d < distance)
                    {
                        distance = d;
                        nearest = kvp.Key;
                    }
                }
            }
            return nearest;
        }

        private static double GetDistance(Point3D p1, Point3D p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        // [2] 상인 원형으로부터 판매 품목(유전자) 추출
        public static HashSet<Type> ExtractItemTypes(Type originalType)
        {
            HashSet<Type> types = new HashSet<Type>();
            try
            {
                if (Activator.CreateInstance(originalType) is BaseVendor temp)
                {
                    temp.InitSBInfo();
                    PropertyInfo sbProp = typeof(BaseVendor).GetProperty("SBInfos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var sbList = sbProp?.GetValue(temp) as List<SBInfo>;
                    if (sbList != null)
                    {
                        foreach (var sb in sbList)
                        {
                            if (sb.BuyInfo == null) continue;
                            foreach (var buyInfo in sb.BuyInfo)
                                if (buyInfo?.Type != null) types.Add(buyInfo.Type);
                        }
                    }
                    temp.Delete();
                }
            }
            catch { }
            return types;
        }

        // [3] 통합 스폰 프로세스
        public static Mobile PerformSpawn(string typeName, string zoneId, Point3D loc, Map map, int range)
        {
            Type originalType = ScriptCompiler.FindTypeByName(typeName) ?? 
                                ScriptCompiler.FindTypeByFullName("Server.Mobiles." + typeName);

            if (originalType == null) return null;

            // 품목 추출
            HashSet<Type> extractedTypes = ExtractItemTypes(originalType);

            // [케이스 1] 서비스 NPC (힐러 등) -> 원본 소환
            if (extractedTypes.Count == 0)
            {
                Mobile m = Activator.CreateInstance(originalType) as Mobile;
                SetupMobile(m, loc, map, range);
                Console.WriteLine($"[Spawner] Service NPC {typeName} spawned.");
                return m;
            }

            // [핵심 수정] 마을 소속 확인 (대소문자 무시, 공백 제거 유연한 검사)
            string actualTownName = zoneId.Trim();
            bool isTownMerchant = false;

            if (!string.IsNullOrEmpty(actualTownName) && actualTownName.ToLower() != "unknown")
            {
                // 먼저 정확히 일치하는지 검사
                if (TownEconomyManager.Towns.ContainsKey(actualTownName))
                {
                    isTownMerchant = true;
                }
                else
                {
                    // 정확히 안 맞으면 대소문자 무시하고 싹 다 뒤져서 매칭!
                    string matchedKey = TownEconomyManager.Towns.Keys.FirstOrDefault(k => k.Equals(actualTownName, StringComparison.OrdinalIgnoreCase));
                    if (matchedKey != null)
                    {
                        actualTownName = matchedKey; // 진짜 저장된 이름으로 교체
                        isTownMerchant = true;
                    }
                }
            }

            // 디버그 출력: 마을 판정 결과 확인
            Console.WriteLine($"\n[Spawner Debug] Request ZoneId: '{zoneId}' -> Matched Town: '{actualTownName}', isTownMerchant: {isTownMerchant}");

            if (isTownMerchant)
            {
                // [케이스 2] 도시 상인
                TownVendor tv = new TownVendor(actualTownName);
                var townEntries = TownInventoryData.GetSetupData(actualTownName);
                var filtered = townEntries.Where(e => extractedTypes.Contains(e.ItemType)).ToList();
                
                tv.SetInventory(typeName, filtered);
                SetupMobile(tv, loc, map, range);
                Console.WriteLine($"[Spawner Debug] Spawned as TownVendor for {actualTownName}.");
                return tv;
            }
            else
            {
                // [마을 매칭 실패 원인 추적기] 대체 등록된 마을 이름이 뭔지 전부 출력!
                Console.WriteLine($"[WARNING] '{zoneId}' is NOT registered as a valid Town. Registered Towns are: {string.Join(", ", TownEconomyManager.Towns.Keys)}");
                
                // [케이스 3] 개인 상인 (Private Vendor)
                double dist;
                FindNearestTown(loc, map, out dist);
                
                PrivateVendor pv = new PrivateVendor();
                pv.LogisticsSurcharge = 1.1 + (dist / 1000.0); // 100타일당 0.1 할증

                // 개인 창고용 초기 데이터 생성
                List<TownInventoryEntry> pEntries = new List<TownInventoryEntry>();
                foreach (Type t in extractedTypes)
                    pEntries.Add(new TownInventoryEntry(t, 50, 10)); // 기본 재고 50개

                pv.SetInventory(typeName, pEntries);
                SetupMobile(pv, loc, map, range);
                Console.WriteLine($"[Spawner Debug] Spawned as PrivateVendor (LogisticsSurcharge: {pv.LogisticsSurcharge:F2}x).");
                return pv;
            }
        }

        private static void SetupMobile(Mobile m, Point3D loc, Map map, int range)
        {
            if (m == null) return;
            
            m.MoveToWorld(loc, map);

            if (m is BaseVendor v)
            {
                v.Home = loc;
                v.RangeHome = range;
            }
            else if (m is BaseCreature bc)
            {
                bc.Home = loc;
                bc.RangeHome = range;
            }
        }
    }
}