using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public static class VendorNodeManager
    {
        public static void Initialize()
        {
            CommandSystem.Register("VendorWipe", AccessLevel.Administrator, new CommandEventHandler(VendorWipe_OnCommand));
            CommandSystem.Register("VendorImport", AccessLevel.Administrator, new CommandEventHandler(VendorImport_OnCommand));
            CommandSystem.Register("VendorExport", AccessLevel.Administrator, new CommandEventHandler(VendorExport_OnCommand));
            CommandSystem.Register("VendorGrowth", AccessLevel.Administrator, new CommandEventHandler(VendorGrowth_OnCommand));
        }

        // =========================================================================
        // 1. VendorNode 삭제 (월드 초기화)
        // =========================================================================
        [Usage("VendorWipe")]
        [Description("월드에 존재하는 모든 VendorNode를 삭제합니다.")]
        public static void VendorWipe_OnCommand(CommandEventArgs e)
        {
            int count = 0;
            List<VendorNode> toDelete = new List<VendorNode>();

            foreach (Item item in World.Items.Values)
            {
                if (item is VendorNode node) toDelete.Add(node);
            }

            foreach (VendorNode node in toDelete)
            {
                node.Delete();
                count++;
            }
            e.Mobile.SendMessage(33, $"완료: 총 {count}개의 VendorNode가 월드에서 삭제되었습니다.");
        }

        // =========================================================================
        // 2. NewVendor.xml Import (배치)
        // =========================================================================
        [Usage("VendorImport")]
        [Description("NewVendor.xml의 좌표를 읽어 VendorNode를 배치합니다.")]
        public static void VendorImport_OnCommand(CommandEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "NewVendor.xml");
            if (!File.Exists(path))
            {
                e.Mobile.SendMessage(33, $"파일을 찾을 수 없습니다: {path}");
                return;
            }

            int count = 0;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                foreach (XmlNode vNode in doc.SelectNodes("//Vendor"))
                {
                    string mapName = vNode.Attributes["Map"]?.Value ?? "Trammel";
                    Map map = Map.Parse(mapName);

                    XmlNode posNode = vNode.SelectSingleNode("Position");
                    if (posNode == null) continue;

                    int x = int.Parse(posNode.SelectSingleNode("X").InnerText);
                    int y = int.Parse(posNode.SelectSingleNode("Y").InnerText);
                    int z = posNode.SelectSingleNode("Z") != null ? int.Parse(posNode.SelectSingleNode("Z").InnerText) : 0;

                    VendorNode node = new VendorNode();
                    node.MoveToWorld(new Point3D(x, y, z), map);

                    // [핵심] XML에 직업이 없으므로, 우리가 만든 만능 상인(TownVendor)을 기본으로 소환하게 설정
                    node.SpawnList = "TownVendor";
                    node.MaxCount = 1;
                    node.HomeRange = 5;

                    // TownID는 MoveToWorld 될 때 내부 로직에 의해 0.001초만에 자동 획득됩니다.
                    count++;
                }
                e.Mobile.SendMessage(68, $"성공: {count}개의 VendorNode가 성공적으로 배치되었습니다.");
            }
            catch (Exception ex) { e.Mobile.SendMessage(33, $"오류: {ex.Message}"); }
        }

        // =========================================================================
        // 3. NewVendor.xml Export (기존 XML 구조와 동일하게 추출)
        // =========================================================================
        [Usage("VendorExport")]
        [Description("현재 월드에 배치된 VendorNode들을 NewVendor.xml 형식으로 추출합니다.")]
        public static void VendorExport_OnCommand(CommandEventArgs e)
        {
            string dir = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "NewVendor_Exported.xml"); 

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<Vendors>");
                int count = 0;

                foreach (Item item in World.Items.Values)
                {
                    if (item is VendorNode node)
                    {
                        // TownNumber 엔진을 통해 현재 속한 마을 이름 역추적
                        string zoneName = TownNumber.GetName(node.TownID).Replace(" (F)", ""); 
                        string mapName = node.Map.Name;

                        writer.WriteLine($"  <Vendor Name=\"Merchant\" Map=\"{mapName}\" ZoneId=\"{zoneName}\">");
                        writer.WriteLine($"    <Position>");
                        writer.WriteLine($"      <X>{node.X}</X>");
                        writer.WriteLine($"      <Y>{node.Y}</Y>");
                        writer.WriteLine($"      <Z>{node.Z}</Z>");
                        writer.WriteLine($"    </Position>");
                        writer.WriteLine($"    <Inventory>");

                        // [핵심] TownEconomyManager에 저장된 이 마을의 실제 판매 물품 목록을 그대로 출력!
                        if (TownEconomyManager.Towns.TryGetValue(node.TownID, out var town))
                        {
                            foreach (var kvp in town.Warehouse)
                            {
                                writer.WriteLine($"      <Item Type=\"{kvp.Key.Name}\" Price=\"{kvp.Value.BasePrice}\" />");
                            }
                        }

                        writer.WriteLine($"    </Inventory>");
                        writer.WriteLine($"  </Vendor>");
                        count++;
                    }
                }
                writer.WriteLine("</Vendors>");
                e.Mobile.SendMessage(68, $"성공: {count}개의 노드가 NewVendor_Exported.xml에 저장되었습니다.");
            }
        }

        // =========================================================================
        // 4. 상인 자동 증식 알고리즘
        // =========================================================================
        [Usage("VendorGrowth")]
        [Description("마을 경제 상태에 따라 상인(MaxCount)을 증식시킵니다.")]
        public static void VendorGrowth_OnCommand(CommandEventArgs e)
        {
            int growCount = 0;

            foreach (Item item in World.Items.Values)
            {
                if (item is VendorNode node)
                {
                    if (node.TownID <= 0) continue; // 야생 제외
                    if (IsGenericVendor(node)) continue; // 일반 NPC 제외

                    if (TownEconomyManager.Towns.TryGetValue(node.TownID, out var town))
                    {
                        // 자본금이 120%를 초과했고 현재 상인 스폰 제한이 5명 미만일 때 증식
                        if (town.Wealth > (town.BaseWealth * 1.2) && node.MaxCount < 5)
                        {
                            node.MaxCount++;
                            node.Respawn(); 
                            growCount++;
                        }
                    }
                }
            }
            e.Mobile.SendMessage(68, $"경제 성장 완료: 총 {growCount}개의 상인 노드가 증식했습니다.");
        }

        private static bool IsGenericVendor(VendorNode node)
        {
            string[] nonCommercial = { "gypsy", "peasant", "noble", "towncrier", "guard", "waiter", "barkeep", "escort" };
            foreach (string spawnType in node.SpawnTypes)
            {
                string lowerType = spawnType.ToLower();
                foreach (string nc in nonCommercial)
                    if (lowerType.Contains(nc)) return true; 
            }
            return false;
        }
    }
}