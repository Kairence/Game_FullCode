using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    public class NodeRegistrySystem
    {
        private static string RegistryDir => Path.Combine(Core.BaseDirectory, "Data", "NodeRegistry");

        public static void Configure()
        {
            CommandSystem.Register("ns_convert", AccessLevel.Administrator, new CommandEventHandler(OnConvertSpawns));
            CommandSystem.Register("ns_save", AccessLevel.Administrator, new CommandEventHandler(OnSaveRegistry));
            CommandSystem.Register("ns_load", AccessLevel.Administrator, new CommandEventHandler(OnLoadRegistry));
        }

        #region [1] Convert: 기존 XML -> Kairence 설계도 변환
        [Usage("ns_convert")]
        public static void OnConvertSpawns(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            string sourcePath = Path.Combine(Core.BaseDirectory, "Data", "Spawns_Merged.txt");

            if (!File.Exists(sourcePath))
            {
                from.SendMessage(33, "Data 폴더에 Spawns_Merged.txt 파일이 없습니다.");
                return;
            }

            from.SendMessage(68, "XML 데이터 추출 및 설계도 생성을 시작합니다...");

            Dictionary<Map, XmlDocument> registryDocs = new Dictionary<Map, XmlDocument>();

            try
            {
                string xmlContent = File.ReadAllText(sourcePath);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<Root>" + xmlContent + "</Root>");

                XmlNodeList pointsList = doc.SelectNodes("//Points");
                int processed = 0;

                for (int i = 0; i < pointsList.Count; i++)
                {
                    XmlNode node = pointsList[i];
                    string mapName = node["Map"]?.InnerText;
                    Map map = DungeonManager.ResolveMapByName(mapName);
                    if (map == null || map == Map.Internal) continue;

                    int x = int.Parse(node["CentreX"]?.InnerText ?? "0");
                    int y = int.Parse(node["CentreY"]?.InnerText ?? "0");
                    int z = int.Parse(node["CentreZ"]?.InnerText ?? "0");
                    int range = int.Parse(node["Range"]?.InnerText ?? "10");
                    int maxCount = int.Parse(node["MaxCount"]?.InnerText ?? "1");
                    string objectsData = node["Objects2"]?.InnerText ?? "";

                    RegionCode rCode = RegionSaver.GetRegionCode(map, x, y, z);
                    int category = ((int)rCode / 10000) % 10;
                    bool isDungeon = (category == 2);

                    if (map == Map.Trammel && !isDungeon) continue;

                    List<string> rawNames = new List<string>();
                    List<Type> spawnTypes = ExtractTypes(objectsData, rawNames);

                    if (!registryDocs.ContainsKey(map))
                    {
                        XmlDocument newDoc = new XmlDocument();
                        newDoc.AppendChild(newDoc.CreateElement("NodeRegistry"));
                        registryDocs[map] = newDoc;
                    }

                    XmlElement regRoot = registryDocs[map].DocumentElement;
                    XmlElement newNode = null;

                    bool isVendor = IsVendorSpawn(spawnTypes, rawNames);

                    // 🌟 농장 빼고 전부 CSV에 있으므로 구형 야생 데이터는 전면 무시(폐기)
                    if (isVendor)
                    {
                        newNode = registryDocs[map].CreateElement("VendorNode");
                        newNode.SetAttribute("TownID", TownNumber.GetID(new Point3D(x, y, z), map).ToString());
                        newNode.SetAttribute("MaxCount", maxCount.ToString());
                        newNode.SetAttribute("Spawns", string.Join(",", rawNames));
                    }
                    else if (isDungeon)
                    {
                        newNode = registryDocs[map].CreateElement("DungeonNode");
                        int avgFame = GetAverageFame(spawnTypes);
                        newNode.SetAttribute("Depth", ((int)CalculateDepth(avgFame)).ToString());
                        
                        if (HasItemInTypes(spawnTypes)) newNode.SetAttribute("HasItem", "true");
                    }
                    else
                    {
                        // 야생 자원은 CSV 스캐너가 담당하므로 스킵
                        continue;
                    }

                    newNode.SetAttribute("X", x.ToString());
                    newNode.SetAttribute("Y", y.ToString());
                    newNode.SetAttribute("Z", z.ToString());
                    newNode.SetAttribute("Range", range.ToString());
                    newNode.SetAttribute("RCode", ((int)rCode).ToString());

                    regRoot.AppendChild(newNode);
                    processed++;
                }

                if (!Directory.Exists(RegistryDir)) Directory.CreateDirectory(RegistryDir);

                // 타일 스캐너가 순수 농장 노드만 별도로 100% 주입
                ScanFarmsAndInject(from, registryDocs);

                foreach (KeyValuePair<Map, XmlDocument> kvp in registryDocs)
                {
                    string savePath = Path.Combine(RegistryDir, $"{kvp.Key.Name}.xml");
                    kvp.Value.Save(savePath);
                }

                from.SendMessage(68, $"설계도 변환 완료: 총 {processed}개 노드 데이터가 저장되었습니다.");
            }
            catch (Exception ex)
            {
                from.SendMessage(33, "변환 오류: " + ex.Message);
            }
        }

        private static List<Type> ExtractTypes(string objectsData, List<string> rawNames)
        {
            List<Type> types = new List<Type>();
            string[] parts = objectsData.Split(':');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("OBJ="))
                {
                    string name = parts[i].Substring(4);
                    rawNames.Add(name);
                    Type t = ScriptCompiler.FindTypeByName(name);
                    if (t != null) types.Add(t);
                }
            }
            return types;
        }

        private static bool IsVendorSpawn(List<Type> types, List<string> rawNames)
        {
            for (int i = 0; i < types.Count; i++)
            {
                Type t = types[i];
                if (t.IsSubclassOf(typeof(BaseVendor))) return true;
                
                string tName = t.Name.ToLower();
                if (tName.Contains("banker") || tName.Contains("healer") || tName.Contains("minter")) 
                    return true;
            }

            for (int i = 0; i < rawNames.Count; i++)
            {
                string n = rawNames[i].ToLower();
                if (n.Contains("vendor") || n.Contains("merchant") || n.Contains("banker") || 
                    n.Contains("tailor") || n.Contains("smith") || n.Contains("mage") || 
                    n.Contains("innkeeper") || n.Contains("barkeep") || n.Contains("tinker") || 
                    n.Contains("cook") || n.Contains("waiter") || n.Contains("weaver") || 
                    n.Contains("bowyer") || n.Contains("butcher") || n.Contains("baker") ||
                    n.Contains("jeweler") || n.Contains("provisioner") || n.Contains("alchemist") ||
                    n.Contains("armorer") || n.Contains("cobbler") || n.Contains("furtrader") ||
                    n.Contains("healer")) 
                    return true;
            }
            return false;
        }

        private static bool HasItemInTypes(List<Type> types)
        {
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i].IsSubclassOf(typeof(Item))) return true;
            }
            return false;
        }

        private static int GetAverageFame(List<Type> types)
        {
            int total = 0, count = 0;
            for (int i = 0; i < types.Count; i++)
            {
                if (!types[i].IsSubclassOf(typeof(BaseCreature))) continue;
                try
                {
                    BaseCreature bc = (BaseCreature)Activator.CreateInstance(types[i]);
                    total += bc.Fame;
                    count++;
                    bc.Delete();
                }
                catch { }
            }
            return count > 0 ? total / count : 0;
        }

        private static DungeonDepth CalculateDepth(int fame)
        {
            if (fame <= 3000) return DungeonDepth.Entrance;
            if (fame <= 8500) return DungeonDepth.Middle;
            if (fame <= 18000) return DungeonDepth.Deep;
            return DungeonDepth.BossRoom;
        }

        private static bool IsFarmTile(int id)
        {
            return (id >= 0x0009 && id <= 0x0015) || (id >= 0x0150 && id <= 0x015C);
        }
        #endregion

        #region [2] Save/Load: 설계도 기반 월드 배치 및 상태 보존
        [Usage("ns_save")]
        public static void OnSaveRegistry(CommandEventArgs e)
        {
            Map map = e.Mobile.Map;
            if (map == null || map == Map.Internal) return;

            string savePath = Path.Combine(RegistryDir, $"{map.Name}.xml");
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("NodeRegistry");
            doc.AppendChild(root);

            foreach (Item item in World.Items.Values)
            {
                if (item.Deleted || item.Map != map) continue;

                XmlElement newNode = null;
                if (item is DungeonNode dn)
                {
                    newNode = doc.CreateElement("DungeonNode");
                    newNode.SetAttribute("Depth", ((int)dn.Depth).ToString());
                }
                else if (item is VendorNode vn)
                {
                    newNode = doc.CreateElement("VendorNode");
                    newNode.SetAttribute("TownID", vn.TownID.ToString());
                    newNode.SetAttribute("MaxCount", vn.MaxCount.ToString());
                    newNode.SetAttribute("Spawns", vn.SpawnList);
                }
                else if (item is EcoNode en)
                {
                    newNode = doc.CreateElement("EcoNode");
                    if (en.Name == "FarmNode") newNode.SetAttribute("EnvType", "Farm");
                    else if (en.Name == "FishingNode") newNode.SetAttribute("EnvType", "Water");
                    else if (en.Name == "MiningNode") newNode.SetAttribute("EnvType", "Mine");
                    else newNode.SetAttribute("EnvType", "Forest");
                }

                if (newNode != null)
                {
                    newNode.SetAttribute("X", item.X.ToString());
                    newNode.SetAttribute("Y", item.Y.ToString());
                    newNode.SetAttribute("Z", item.Z.ToString());
                    newNode.SetAttribute("Range", (item is EcoNode ? ((EcoNode)item).SpawnRange : (item is DungeonNode ? ((DungeonNode)item).SpawnRange : ((VendorNode)item).HomeRange)).ToString());
                    newNode.SetAttribute("RCode", ((int)RegionSaver.GetRegionCode(map, item.X, item.Y, item.Z)).ToString());
                    root.AppendChild(newNode);
                }
            }
            doc.Save(savePath);
            e.Mobile.SendMessage(68, $"{map.Name} 대륙의 현재 노드 상태를 설계도에 반영했습니다.");
        }

        [Usage("ns_load")]
        public static void OnLoadRegistry(CommandEventArgs e)
        {
            Map map = e.Mobile.Map;
            string loadPath = Path.Combine(RegistryDir, $"{map.Name}.xml");

            if (!File.Exists(loadPath))
            {
                e.Mobile.SendMessage(33, $"{map.Name}.xml 설계도 파일이 없습니다.");
                return;
            }

            List<Item> toDelete = new List<Item>();
            foreach (Item item in World.Items.Values)
            {
                if (item.Deleted || item.Map != map) continue;

                Type t = item.GetType();
                
                bool isOldSpawner = t.Name.IndexOf("Spawner", StringComparison.OrdinalIgnoreCase) >= 0; 
                bool isNewNode = (item is DungeonNode || item is EcoNode || item is VendorNode);

                if (isOldSpawner || isNewNode)
                {
                    if (map == Map.Trammel)
                    {
                        RegionCode code = RegionSaver.GetRegionCode(map, item.X, item.Y, item.Z);
                        if (((int)code / 10000) % 10 != 2) continue;
                    }
                    toDelete.Add(item);
                }
            }

            for (int i = 0; i < toDelete.Count; i++) toDelete[i].Delete();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(loadPath);
                int count = 0;

                XmlNodeList list = doc.DocumentElement.ChildNodes;
                for (int i = 0; i < list.Count; i++)
                {
                    XmlNode node = list[i];
                    int x = int.Parse(node.Attributes["X"]?.Value ?? "0");
                    int y = int.Parse(node.Attributes["Y"]?.Value ?? "0");
                    int z = int.Parse(node.Attributes["Z"]?.Value ?? "0");
                    int range = int.Parse(node.Attributes["Range"]?.Value ?? "10");
                    RegionCode code = (RegionCode)int.Parse(node.Attributes["RCode"]?.Value ?? "0");

                    if (node.Name == "DungeonNode")
                    {
                        DungeonNode dn = new DungeonNode { SpawnRange = range, HomeRange = range + 20, RCode = code };
                        dn.Depth = (DungeonDepth)int.Parse(node.Attributes["Depth"]?.Value ?? "1");
                        dn.MoveToWorld(new Point3D(x, y, z), map);
                    }
                    else if (node.Name == "VendorNode")
                    {
                        VendorNode vn = new VendorNode { HomeRange = range, TownID = int.Parse(node.Attributes["TownID"]?.Value ?? "0"), MaxCount = int.Parse(node.Attributes["MaxCount"]?.Value ?? "1") };
                        string[] spawns = (node.Attributes["Spawns"]?.Value ?? "").Split(',');
                        for (int s = 0; s < spawns.Length; s++) if (!string.IsNullOrEmpty(spawns[s])) vn.SpawnTypes.Add(spawns[s]);
                        vn.MoveToWorld(new Point3D(x, y, z), map);
                    }
                    else if (node.Name == "EcoNode")
                    {
                        EcoNode en = new EcoNode { SpawnRange = range, HomeRange = range + 20, RCode = code };
                        NewSpawnManager.ApplyDefaultSettings(en);

                        XmlAttribute envAttr = node.Attributes["EnvType"];
                        if (envAttr != null)
                        {
                            if (envAttr.Value == "Farm") en.Name = "FarmNode";
                            else if (envAttr.Value == "Water") en.Name = "FishingNode";
                            else if (envAttr.Value == "Mine") en.Name = "MiningNode";
                            else en.Name = "ForestNode";
                        }
                        else 
                        {
                            en.Name = "ForestNode";
                        }

                        XmlAttribute capAttr = node.Attributes["FarmCap"];
                        if (capAttr != null && int.TryParse(capAttr.Value, out int cap)) { }

                        en.MoveToWorld(new Point3D(x, y, z), map);
                    }
                    count++;
                }

                for (int i = 0; i < DungeonManager.ZoneList.Count; i++) DungeonManager.ZoneList[i].CacheNodes();
                EcosystemManager.RebuildZones();

                e.Mobile.SendMessage(68, $"{map.Name} 대륙 재구축 완료: {count}개의 Kairence 노드 설치.");
            }
            catch (Exception ex) { e.Mobile.SendMessage(33, "로드 오류: " + ex.Message); }
        }
        #endregion

        #region [3] 타일 정밀 스캔 (Flood Fill) 및 농장 노드 생성
        private static void ScanFarmsAndInject(Mobile from, Dictionary<Map, XmlDocument> registryDocs)
        {
            from.SendMessage(68, "텍스트 파싱 완료. 농장 구역 병합을 위한 정밀 타일 스캔을 시작합니다...");

            Map[] maps = new Map[] { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };

            for (int m = 0; m < maps.Length; m++)
            {
                Map map = maps[m];
                if (map == null || map == Map.Internal || !registryDocs.ContainsKey(map)) continue;

                int width = map.Width;
                int height = map.Height;
                bool[,] visited = new bool[width, height];
                XmlDocument doc = registryDocs[map];
                XmlElement root = doc.DocumentElement;
                int addedCount = 0;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (visited[x, y]) continue;

                        int landID = map.Tiles.GetLandTile(x, y).ID & 0x3FFF;
                        if (IsFarmTile(landID))
                        {
                            Queue<Point2D> q = new Queue<Point2D>();
                            List<Point2D> farmTiles = new List<Point2D>();

                            q.Enqueue(new Point2D(x, y));
                            visited[x, y] = true;
                            farmTiles.Add(new Point2D(x, y));

                            long sumX = 0;
                            long sumY = 0;
                            int minX = x, maxX = x, minY = y, maxY = y;

                            int[] dx = { 0, 0, -1, 1 };
                            int[] dy = { -1, 1, 0, 0 };

                            while (q.Count > 0)
                            {
                                Point2D p = q.Dequeue();
                                sumX += p.X;
                                sumY += p.Y;

                                if (p.X < minX) minX = p.X;
                                if (p.X > maxX) maxX = p.X;
                                if (p.Y < minY) minY = p.Y;
                                if (p.Y > maxY) maxY = p.Y;

                                for (int i = 0; i < 4; i++)
                                {
                                    int nx = p.X + dx[i];
                                    int ny = p.Y + dy[i];

                                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                                    {
                                        int nID = map.Tiles.GetLandTile(nx, ny).ID & 0x3FFF;
                                        if (IsFarmTile(nID))
                                        {
                                            visited[nx, ny] = true;
                                            q.Enqueue(new Point2D(nx, ny));
                                            farmTiles.Add(new Point2D(nx, ny));
                                        }
                                    }
                                }
                            }

                            if (farmTiles.Count >= 4)
                            {
                                int avgX = (int)(sumX / farmTiles.Count);
                                int avgY = (int)(sumY / farmTiles.Count);
                                int cx = avgX, cy = avgY;
                                
                                double minDst = double.MaxValue;
                                foreach (Point2D p in farmTiles)
                                {
                                    double dst = Math.Pow(p.X - avgX, 2) + Math.Pow(p.Y - avgY, 2);
                                    if (dst < minDst) { minDst = dst; cx = p.X; cy = p.Y; }
                                }

                                int cz = map.GetAverageZ(cx, cy);
                                Region reg = Region.Find(new Point3D(cx, cy, cz), map);
                                string regName = reg.Name != null ? reg.Name.ToLower() : string.Empty;
                                
                                RegionCode rCode = RegionSaver.GetRegionCode(map, cx, cy, cz);

                                // 농장도 던전 구역(category 2)이면 절대 생성 불가
                                if (((int)rCode / 10000) % 10 == 2) continue;

                                if (!(reg is Server.Regions.HouseRegion) && !regName.Contains("house") && !regName.Contains("private"))
                                {
                                    XmlElement newNode = doc.CreateElement("EcoNode");
                                    newNode.SetAttribute("X", cx.ToString());
                                    newNode.SetAttribute("Y", cy.ToString());
                                    newNode.SetAttribute("Z", cz.ToString());
                                    
                                    int range = Math.Max(10, Math.Max(maxX - minX, maxY - minY) / 2 + 5);
                                    newNode.SetAttribute("Range", range.ToString());
                                    
                                    int farmCap = farmTiles.Count * 10;
                                    newNode.SetAttribute("FarmCap", farmCap.ToString());
                                    
                                    newNode.SetAttribute("EnvType", "Farm");
                                    newNode.SetAttribute("RCode", ((int)rCode).ToString());
                                    root.AppendChild(newNode);
                                    addedCount++;
                                }
                            }
                        }
                    }
                }

                if (addedCount > 0)
                {
                    from.SendMessage(68, $"[{map.Name}] 농장 구역 {addedCount}개가 병합되어 레지스트리에 저장되었습니다.");
                }
            }
        }
        #endregion
    }
}