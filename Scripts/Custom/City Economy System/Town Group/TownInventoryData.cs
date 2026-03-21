using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq; // 추가됨
using Server;

namespace Server.Misc
{
    public class TownInventoryEntry
    {
        public Type ItemType { get; set; }
        public int InitialStock { get; set; }
        public int BasePrice { get; set; }

        public TownInventoryEntry(Type type, int stock, int price)
        {
            ItemType = type;
            InitialStock = stock;
            BasePrice = price;
        }
    }

    public static class TownInventoryData
    {
        private static readonly Dictionary<string, List<TownInventoryEntry>> m_Data = new Dictionary<string, List<TownInventoryEntry>>();
        private static bool m_IsLoaded = false;

        private static readonly HashSet<string> MajorTowns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Britain", "Moonglow", "Magincia", "Zento", "Skara Brae", 
            "Minoc", "Yew", "Trinsic", "Jhelom", "New Magincia", "Royal City"
        };

        public static void LoadFromXml()
        {
            if (m_IsLoaded) return;

            string path = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "NewVendor.xml");
            if (!File.Exists(path)) { m_IsLoaded = true; return; }

            try
            {
                m_Data.Clear();
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                
                var tempTownData = new Dictionary<string, Dictionary<string, int>>();
                var tempTownLocs = new Dictionary<string, (Point3D, Map)>();
                var tempTownVendorCount = new Dictionary<string, int>();

                XmlNodeList vendorNodes = doc.SelectNodes("//Vendor");
                if (vendorNodes == null) return;

                foreach (XmlNode vendor in vendorNodes)
                {
                    string zoneId = vendor.Attributes["ZoneId"]?.Value?.Trim();
                    string mapName = vendor.Attributes["Map"]?.Value?.Trim();

                    if (string.IsNullOrEmpty(zoneId) || zoneId == "Unknown" || zoneId == "None") continue;

                    if (!tempTownVendorCount.ContainsKey(zoneId)) tempTownVendorCount[zoneId] = 0;
                    tempTownVendorCount[zoneId]++;

                    if (!tempTownLocs.ContainsKey(zoneId))
                    {
                        XmlNode posNode = vendor.SelectSingleNode("Position");
                        if (posNode != null)
                        {
                            int x = int.Parse(posNode.SelectSingleNode("X").InnerText);
                            int y = int.Parse(posNode.SelectSingleNode("Y").InnerText);
                            int z = int.Parse(posNode.SelectSingleNode("Z").InnerText);
                            Map mapObj = Map.Parse(mapName ?? "Trammel");
                            tempTownLocs[zoneId] = (new Point3D(x, y, z), mapObj);
                        }
                    }

                    XmlNode inventory = vendor.SelectSingleNode("Inventory");
                    if (inventory != null)
                    {
                        XmlNodeList itemNodes = inventory.SelectNodes("Item");
                        if (itemNodes == null || itemNodes.Count == 0) continue;

                        if (!tempTownData.ContainsKey(zoneId))
                            tempTownData[zoneId] = new Dictionary<string, int>();

                        foreach (XmlNode item in itemNodes)
                        {
                            string typeName = item.Attributes["Type"]?.Value?.Trim();
                            string priceStr = item.Attributes["Price"]?.Value?.Trim();

                            if (!string.IsNullOrEmpty(typeName) && int.TryParse(priceStr, out int price))
                            {
                                if (!tempTownData[zoneId].ContainsKey(typeName) || tempTownData[zoneId][typeName] < price)
                                    tempTownData[zoneId][typeName] = price;
                            }
                        }
                    }
                }

                foreach (var townKvp in tempTownData)
				{
					string townName = townKvp.Key;
					if (townKvp.Value.Count == 0) continue;

					// [수정] RankAndTerritorySystem.Towns -> TownEconomyManager.Towns
					if (!TownEconomyManager.Towns.ContainsKey(townName))
					{
						Point3D center = Point3D.Zero;
						Map townMap = Map.Trammel;
						if (tempTownLocs.TryGetValue(townName, out var loc)) { center = loc.Item1; townMap = loc.Item2; }

						long initialWealth = MajorTowns.Contains(townName) ? 1000000 : 
							Math.Min(300000, 50000 + ((tempTownVendorCount.ContainsKey(townName) ? tempTownVendorCount[townName] : 1) * 20000));

						// 새로운 매니저의 Towns 딕셔너리에 등록
						TownEconomyManager.Towns[townName] = new TownEconomy(townName, center, townMap, initialWealth);
					}

					var townObj = TownEconomyManager.Towns[townName];
                    
                    // 초기 MaxCapacity 설정 (기본 2000 + 품목당 500)
                    townObj.MaxInventoryCapacity = 2000; 

                    foreach (var itemKvp in townKvp.Value)
                    {
                        Type type = ScriptCompiler.FindTypeByName(itemKvp.Key) ??
                                   ScriptCompiler.FindTypeByFullName("Server.Items." + itemKvp.Key) ??
                                   ScriptCompiler.FindTypeByFullName("Server.Mobiles." + itemKvp.Key);

                        if (type != null)
                        {
                            // 창고 데이터 주입
                            if (!townObj.Warehouse.ContainsKey(type))
                            {
                                townObj.Warehouse[type] = new WarehouseItem(type, 2000, itemKvp.Value);
                                townObj.MaxInventoryCapacity += 500; // XML 로드 시에도 한도 확장
                            }
                        }
                    }
                }
                m_IsLoaded = true;
                Console.WriteLine($"[{DateTime.Now}] [Economy] {tempTownData.Count}개 지역 데이터 로드 및 창고 동기화 완료.");
            }
            catch (Exception ex) { Console.WriteLine($"[Economy Error] XML 로드 실패: {ex.Message}"); }
        }

        // [★ 핵심 수정] 실시간 데이터를 반환하도록 변경
        public static List<TownInventoryEntry> GetSetupData(string townName)
        {
            if (!m_IsLoaded) LoadFromXml();

            // 마을이 시스템에 등록되어 있다면, 'm_Data' 대신 실제 'Warehouse' 데이터를 읽어 리스트를 만듭니다.
            if (TownEconomyManager.Towns.TryGetValue(townName, out var town))
            {
                List<TownInventoryEntry> realTimeList = new List<TownInventoryEntry>();
                foreach (var kvp in town.Warehouse)
                {
                    // 실시간 재고와 실시간 기준가를 반영하여 리스트 생성
                    realTimeList.Add(new TownInventoryEntry(kvp.Key, kvp.Value.Stock, kvp.Value.BasePrice));
                }
                return realTimeList;
            }

            return new List<TownInventoryEntry>();
        }
    }
}