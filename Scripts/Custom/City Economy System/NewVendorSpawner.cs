using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public static class NewVendorSpawner
    {
        public static void Initialize()
        {
            CommandSystem.Register("AutoVendorSpawn", AccessLevel.Administrator, new CommandEventHandler(AutoVendorSpawn_OnCommand));
            CommandSystem.Register("InitTowns", AccessLevel.GameMaster, new CommandEventHandler(InitTowns_OnCommand));
            
            // [★ 추가] 벤더 스폰 없이 재고만 다시 계산하고 싶을 때 쓰는 명령어
            CommandSystem.Register("SyncTownStock", AccessLevel.Administrator, new CommandEventHandler(SyncTownStock_OnCommand));
        }

        [Usage("InitTowns")]
        [Description("수동으로 기본 마을 경제 시스템을 생성합니다.")]
        private static void InitTowns_OnCommand(CommandEventArgs e)
        {
            string[] defaultTowns = { "Cove", "Moonglow", "Yew", "Minoc", "Skara Brae", "Jhelom", "New Magincia", "Vesper", "Trinsic", "Britain" };
            int count = 0;

            foreach (string t in defaultTowns)
            {
                if (!TownEconomyManager.Towns.ContainsKey(t))
                {
                    TownEconomyManager.Towns[t] = new TownEconomy(t, Point3D.Zero, Map.Trammel, 100000); 
                    count++;
                }
            }
            e.Mobile.SendMessage(88, $"총 {count}개의 기본 마을 경제 DB가 수동 구축되었습니다.");
        }

        [Usage("SyncTownStock")]
        [Description("현재 월드에 배치된 상인 수를 계산하여 창고 재고를 500개씩 동기화합니다.")]
        private static void SyncTownStock_OnCommand(CommandEventArgs e)
        {
            SyncTownStock_Logic(e.Mobile);
        }

        // [★ 핵심 로직] 월드의 모든 상인을 스캔해서 창고에 500개씩 물건을 쌓아주는 함수
        private static void SyncTownStock_Logic(Mobile from)
        {
            from.SendMessage(88, "마을별 상인 수를 집계하여 창고 초기 재고를 설정합니다...");
            
            int totalStockAdded = 0;

            // 1. 모든 도시 창고 재고 초기화 (새로 집계하기 위함)
            foreach (var town in TownEconomyManager.Towns.Values)
            {
                town.Warehouse.Clear();
            }

            // 2. 월드에 있는 모든 TownVendor 순회
            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m is TownVendor tv && !tv.Deleted)
                {
                    string tName = tv.TownName;
                    if (!string.IsNullOrEmpty(tName) && TownEconomyManager.Towns.TryGetValue(tName, out var town))
                    {
                        // 리플렉션을 통해 상인이 배정받은 원본 아이템 목록(m_CurrentEntries)을 훔쳐옵니다!
                        FieldInfo entriesField = typeof(TownVendor).GetField("m_CurrentEntries", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (entriesField != null)
                        {
                            if (entriesField.GetValue(tv) is List<TownInventoryEntry> entries)
                            {
                                foreach (var entry in entries)
                                {
                                    if (!town.Warehouse.ContainsKey(entry.ItemType))
                                    {
                                        // 창고에 처음 등록하는 물품이면 500개로 신규 생성
                                        town.Warehouse[entry.ItemType] = new WarehouseItem(entry.ItemType, 500, entry.BasePrice);
                                    }
                                    else
                                    {
                                        // 이미 다른 상인이 올려둔 물품이면 재고에 500개 추가 누적! (ex: 상인 10명이면 5000개)
                                        town.Warehouse[entry.ItemType].Stock += 500;
                                    }
                                    totalStockAdded += 500;
                                }
                            }
                        }
                    }
                }
            }
            from.SendMessage(68, $"집계 완료! 총 {totalStockAdded:#,0}개의 물품이 각 마을 창고에 분배되었습니다.");
        }

        [Usage("AutoVendorSpawn")]
        [Description("기존 스포너와 VendorNode를 읽어와 경제 시스템(Town)을 자동 등록하고 상인을 갱신합니다.")]
        private static void AutoVendorSpawn_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendMessage(88, "상인 자동 스폰 및 마을 DB 자동 구축을 시작합니다...");

            int vendorWiped = 0;
            int spawnerConverted = 0;
            int nodeRespawned = 0;
            int newTownsRegistered = 0;

            TownInventoryData.LoadFromXml();

            try
            {
                var oldVendors = World.Mobiles.Values.Where(m => 
                    m is BaseVendor && 
                    !(m is TownVendor) && 
                    !(m is PlayerVendor) && 
                    !m.GetType().Name.Contains("Player") && 
                    !m.GetType().Name.Contains("Rented")
                ).ToList();

                var allItems = World.Items.Values.ToList();

                foreach (Item item in allItems)
                {
                    if (item.Deleted || item.Map == null || item.Map == Map.Internal) continue;

                    if (item is VendorNode node)
                    {
                        string zone = node.ZoneId;
                        
                        if (!string.IsNullOrEmpty(zone) && zone != "Unknown" && !TownEconomyManager.Towns.ContainsKey(zone))
                        {
                            TownEconomyManager.Towns[zone] = new TownEconomy(zone, node.Location, node.Map, 100000);
                            newTownsRegistered++;
                        }
                        
                        node.Respawn();
                        nodeRespawned++;
                        continue;
                    }

                    string itemName = item.GetType().Name;
                    if (itemName == "Spawner" || itemName == "XmlSpawner")
                    {
                        int maxCount = 1;
                        var spawnList = new List<string>();
                        Type t = item.GetType();

                        var countProp = t.GetProperty("MaxCount") ?? t.GetProperty("Count");
                        if (countProp != null) 
                        {
                            try { maxCount = Convert.ToInt32(countProp.GetValue(item)); } catch { }
                        }

                        var namesProp = t.GetProperty("SpawnNames") ?? t.GetProperty("NameList") ?? t.GetProperty("Spawns");
                        if (namesProp != null)
                        {
                            object val = namesProp.GetValue(item);
                            if (val is List<string> list) spawnList.AddRange(list);
                            else if (val is string[] arr) spawnList.AddRange(arr);
                        }

                        var fld = t.GetField("m_SpawnObjects", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fld != null && fld.GetValue(item) is IEnumerable arrObjects)
                        {
                            foreach (var obj in arrObjects)
                            {
                                if (obj == null) continue;
                                var nameFld = obj.GetType().GetField("TypeName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                var nameProp = obj.GetType().GetProperty("TypeName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                
                                object nameVal = null;
                                if (nameFld != null) nameVal = nameFld.GetValue(obj);
                                else if (nameProp != null) nameVal = nameProp.GetValue(obj);

                                if (nameVal is string s && !string.IsNullOrEmpty(s))
                                    spawnList.Add(s);
                            }
                        }

                        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (prop.PropertyType == typeof(string) && prop.Name.StartsWith("Spawn"))
                            {
                                if (prop.GetValue(item) is string s && !string.IsNullOrEmpty(s))
                                    spawnList.Add(s);
                            }
                        }

                        if (spawnList.Count == 0) continue;

                        bool isVendorSpawner = false;
                        var cleanSpawnTypes = new List<string>();

                        foreach (string rawName in spawnList)
                        {
                            string typeName = rawName.Split('/')[0].Trim(); 
                            if (string.IsNullOrEmpty(typeName)) continue;

                            Type mobType = ScriptCompiler.FindTypeByName(typeName) ?? ScriptCompiler.FindTypeByFullName("Server.Mobiles." + typeName);
                            
                            if (mobType != null && (mobType.IsSubclassOf(typeof(BaseVendor)) || mobType == typeof(TownVendor)))
                            {
                                isVendorSpawner = true;
                                cleanSpawnTypes.Add(typeName);
                            }
                        }

                        if (isVendorSpawner)
                        {
                            VendorNode newNode = new VendorNode();
                            newNode.MoveToWorld(item.Location, item.Map);
                            
                            Region reg = Region.Find(item.Location, item.Map);
                            string zone = reg?.Name ?? "Unknown";
                            newNode.ZoneId = zone;
                            
                            newNode.MaxCount = maxCount;
                            newNode.HomeRange = 5;
                            newNode.SpawnTypes.AddRange(cleanSpawnTypes.Distinct());

                            if (zone != "Unknown" && !TownEconomyManager.Towns.ContainsKey(zone))
                            {
                                TownEconomyManager.Towns[zone] = new TownEconomy(zone, newNode.Location, newNode.Map, 100000);
                                newTownsRegistered++;
                            }

                            newNode.Respawn(); 
                            item.Delete(); 
                            spawnerConverted++;
                        }
                    }
                }

                foreach (Mobile v in oldVendors)
                {
                    v.Delete();
                    vendorWiped++;
                }

                from.SendMessage(68, "=============== [스폰 정리 결과] ===============");
                from.SendMessage(68, $"구형 상인 삭제: {vendorWiped}명");
                from.SendMessage(68, $"기존 VendorNode 리스폰: {nodeRespawned}개");
                from.SendMessage(68, $"구형 스포너 -> VendorNode 변환: {spawnerConverted}개");
                from.SendMessage(1152, $"★ 동적 등록된 새 마을 DB: {newTownsRegistered}곳");
                from.SendMessage(68, "==============================================");

                // [★ 자동 집계] 스폰이 다 끝나면 상인 수를 세어서 창고에 500개씩 자동으로 채워줍니다!
                SyncTownStock_Logic(from);
            }
            catch (Exception ex)
            {
                from.SendMessage(33, $"에러 발생: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}