using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Misc
{
    public class NewSpawnManager
    {
        public static Dictionary<Map, bool> ActiveMaps = new Dictionary<Map, bool>
        {
            { Map.Felucca, true }, { Map.Trammel, true }, { Map.Ilshenar, true },
            { Map.Malas, true }, { Map.Tokuno, true }, { Map.TerMur, true }
        };

        // 🌟 대륙 시스템 ON/OFF 저장 경로
        private static string MapSavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "ActiveMaps.bin");

        public static void Configure()
        {
            EventSink.WorldSave += OnSaveActiveMaps;
            EventSink.WorldLoad += OnLoadActiveMaps;

            CommandSystem.Register("ns", AccessLevel.Administrator, new CommandEventHandler(OnNewSpawn));
            CommandSystem.Register("zonemonitor", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("zm", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("fixallnodes", AccessLevel.Administrator, new CommandEventHandler(OnFixAllNodes));
            CommandSystem.Register("wipeworldspawns", AccessLevel.Administrator, new CommandEventHandler(OnWipeWorldSpawns));
            CommandSystem.Register("wipewildcrops", AccessLevel.Administrator, new CommandEventHandler(OnWipeWildCrops));
            
            CommandSystem.Register("NewGen", AccessLevel.Administrator, new CommandEventHandler(OnNewGen));
            
            Timer.DelayCall(TimeSpan.FromSeconds(5.0), () =>
            {
                Console.WriteLine("Ecosystem & Dungeon Monitor Data Rebuilding...");
                EcosystemManager.RebuildZones(); 
                foreach (var dz in DungeonManager.ZoneList) dz.CacheNodes();
                Console.WriteLine($"Monitor Ready: Eco({EcosystemManager.ZoneList.Count}) Dungeon({DungeonManager.ZoneList.Count})");
            });
        }

        private static void OnSaveActiveMaps(WorldSaveEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(MapSavePath))) Directory.CreateDirectory(Path.GetDirectoryName(MapSavePath));
            using (FileStream bin = new FileStream(MapSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                GenericWriter writer = new BinaryFileWriter(bin, true);
                writer.Write(0); 
                writer.Write(ActiveMaps.Count);
                foreach (var kvp in ActiveMaps) 
                { 
                    writer.Write(kvp.Key.MapID); 
                    writer.Write(kvp.Value); 
                }
                writer.Close(); // 누락되었던 핵심 저장 처리
            }
        }

        private static void OnLoadActiveMaps()
        {
            if (!File.Exists(MapSavePath)) return;

            try
            {
                using (FileStream bin = new FileStream(MapSavePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // 1. 파일이 비어있는지(0바이트) 체크하여 예외 방지
                    if (bin.Length == 0)
                    {
                        Console.WriteLine("[NewSpawnManager] ActiveMaps.bin 파일이 비어 있습니다. 기본 설정을 사용합니다.");
                        return;
                    }

                    using (BinaryReader br = new BinaryReader(bin))
                    {
                        GenericReader reader = new BinaryFileReader(br);
                        int version = reader.ReadInt();
                        int count = reader.ReadInt();
                        
                        for (int i = 0; i < count; i++)
                        {
                            int mapID = reader.ReadInt(); 
                            bool isActive = reader.ReadBool();
                            
                            try
                            {
                                Map map = Map.Maps[mapID];
                                if (map != null && map != Map.Internal) 
                                    ActiveMaps[map] = isActive;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 2. 파일이 깨져서 읽기 실패해도 서버 크래시를 막고 오류 메시지만 출력
                Console.WriteLine($"[NewSpawnManager] ActiveMaps.bin 로드 실패: {ex.Message}. 기본 설정을 유지합니다.");
            }
        }

        [Usage("NewGen")]
        public static void OnNewGen(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            int deletedEco = 0, deletedDungeon = 0;
            int newEco = 0, newDungeon = 0;
            
            from.SendMessage(68, "[NewGen] 생태계 및 던전 맵핑을 초기화합니다. 잠시 렉이 발생할 수 있습니다...");

            foreach (var node in World.Items.Values.OfType<EcoNode>().ToList()) { node.Delete(); deletedEco++; }
            foreach (var node in World.Items.Values.OfType<DungeonNode>().ToList()) { node.Delete(); deletedDungeon++; }

            foreach (var kvp in EcoGridDatabase.Chunks)
            {
                EcoNode node = new EcoNode();
                node.RCode = kvp.Value.Code; 
                ApplyDefaultSettings(node);

                if (node.RCode == RegionCode.None)
                {
                    int mapId = kvp.Key.Facet.MapID;
                    int cx = kvp.Value.CenterX / 128;
                    int cy = kvp.Value.CenterY / 128;
                    int pseudoCode = ((mapId + 1) * 1000000) + (cx * 1000) + cy;
                    node.RCode = (RegionCode)pseudoCode;
                }

                int z = kvp.Key.Facet.GetAverageZ(kvp.Value.CenterX, kvp.Value.CenterY);
                node.MoveToWorld(new Point3D(kvp.Value.CenterX, kvp.Value.CenterY, z), kvp.Key.Facet);
                newEco++;
            }
            
            foreach (var dz in DungeonManager.ZoneList)
            {
                if (dz.Facet == null || !ActiveMaps.GetValueOrDefault(dz.Facet, true)) continue;

                Point3D centerLoc = RegionSaver.GetRegionCenter(dz.RCode, dz.Facet);
                if (centerLoc == Point3D.Zero) continue;

                DungeonNode node = new DungeonNode();
                node.RCode = dz.RCode;
                node.SpawnRange = 30;
                node.HomeRange = 50;
                node.Depth = DungeonDepth.Entrance;
                node.MoveToWorld(centerLoc, dz.Facet);
                newDungeon++;
            }

            EcosystemManager.RebuildZones();
            foreach (var dz in DungeonManager.ZoneList) dz.CacheNodes();

            from.SendMessage(68, $"[NewGen 완료] 기존 (Eco:{deletedEco}, Dungeon:{deletedDungeon}) 삭제.");
            from.SendMessage(68, $"[NewGen 완료] 신규 (Eco:{newEco}, Dungeon:{newDungeon}) 노드 배치 및 캐싱 완료!");
        }

        public static string GetDisplayName(RegionCode code)
        {
            int val = (int)code;
            if (val == 0) return "Unknown";
            
            if (val >= 1000000)
            {
                int mapId = (val / 1000000) - 1;
                string mapName = mapId switch { 0 => "Felucca", 1 => "Trammel", 2 => "Ilshenar", 3 => "Malas", 4 => "Tokuno", 5 => "TerMur", _ => "Unknown" };
                int rem = val % 1000000;
                int cx = rem / 1000;
                int cy = rem % 1000;
                return $"[{mapName}] 야생 ({cx * 128}, {cy * 128})"; 
            }
            
            if ((val % 100000) >= 99000)
            {
                int mapId = (val / 100000) - 1;
                string mapName = mapId switch { 0 => "Felucca", 1 => "Trammel", 2 => "Ilshenar", 3 => "Malas", 4 => "Tokuno", 5 => "TerMur", _ => "Unknown" };
                int area = (val % 100) / 10;
                int climate = val % 10;
                return $"[{mapName}] 미개척 야생 ({(EcoAreaType)area}/{(EcoClimateType)climate})";
            }

            string full = code.ToString(); string[] parts = full.Split('_');
            return parts.Length >= 3 ? string.Join(" ", parts.Skip(2)).Replace("Level", "Level ") : full.Replace("_", " ");
        }

        public static string GetGoGumpZoneName(Point3D loc, Map map)
        {
            RegionCode code = RegionSaver.GetRegionCode(map, loc.X, loc.Y, loc.Z);
            return code == RegionCode.None ? "Unknown" : code.ToString();
        }

        // 🌟 외부 재생성 로직에서 접근할 수 있도록 public으로 변경했습니다.
        public static void ApplyDefaultSettings(EcoNode node)
        {
            int calculatedRange = Math.Max(30, (int)Math.Sqrt(20 * 100));
            node.SpawnRange = calculatedRange; node.HomeRange = calculatedRange + 20; 
            node.AreaType = calculatedRange >= 80 ? EcoAreaType.Forest : (calculatedRange >= 50 ? EcoAreaType.Hunting : EcoAreaType.Town);

            string codeName = node.RCode.ToString().ToLower();
            if (codeName.Contains("desert")) node.ClimateType = EcoClimateType.Desert;
            else if (codeName.Contains("snow") || codeName.Contains("arctic") || codeName.Contains("ice")) node.ClimateType = EcoClimateType.Arctic;
            else if (codeName.Contains("swamp") || codeName.Contains("bog")) node.ClimateType = EcoClimateType.Swamp;
            else node.ClimateType = EcoClimateType.Temperate;
        }

        #region [Commands & Utilities]
        [Usage("wipewildcrops")]
        public static void OnWipeWildCrops(CommandEventArgs e)
        {
            Mobile from = e.Mobile; Map targetMap = from.Map;
            if (targetMap == null || targetMap == Map.Internal) return;

            List<Item> toDelete = World.Items.Values.Where(item => item.Map == targetMap && item is BaseFarmItem).ToList();
            foreach (Item i in toDelete) i.Delete();

            int poolReset = 0;
            foreach (var pool in ResourceManager.PoolList) if (pool.Facet == targetMap && pool.Type == ResourceType.Farming) { pool.CurrentCapacity = 0; poolReset++; }
            from.SendMessage(68, $"{targetMap.Name}: 작물 {toDelete.Count}개 삭제 및 {poolReset}개 구역 초기화 완료.");
        }

        [Usage("fixallnodes")]
        public static void OnFixAllNodes(CommandEventArgs e)
        {
            int count = 0;
            foreach (Item item in World.Items.Values)
            {
                if (item is DungeonNode dNode && dNode.RCode == RegionCode.None) { dNode.RCode = RegionSaver.GetRegionCode(dNode.Map, dNode.X, dNode.Y, dNode.Z); count++; }
                else if (item is EcoNode eNode && eNode.RCode == RegionCode.None) { eNode.RCode = RegionSaver.GetRegionCode(eNode.Map, eNode.X, eNode.Y, eNode.Z); count++; }
            }
            e.Mobile.SendMessage(68, $"총 {count}개의 누락된 노드 인덱스를 갱신했습니다.");
        }

        [Usage("wipeworldspawns")] public static void OnWipeWorldSpawns(CommandEventArgs e) => DoReset(e.Mobile);

        public static Point3D? FindLocationByRegionCode(RegionCode code, Map map)
        {
            if (code == RegionCode.None) return null;
            if (DungeonManager.Zones.TryGetValue(code, out var dz) && dz.Nodes.Count > 0) return dz.Nodes[0].Location;
            if (EcosystemManager.Zones.TryGetValue(code, out var ez) && ez.Nodes.Count > 0) return ez.Nodes[0].Location;
            return null;
        }

        public static bool IsManaged(RegionCode code) { return code != RegionCode.None && (DungeonManager.Zones.ContainsKey(code) || EcosystemManager.Zones.ContainsKey(code)); }

        [Usage("ns")] public static void OnNewSpawn(CommandEventArgs e) => e.Mobile.SendGump(new NewSpawnGump());
        [Usage("zonemonitor")] public static void OnMonitor(CommandEventArgs e) => e.Mobile.SendGump(new ZoneMonitorGump(0, 0));
        #endregion

        #region [Export & Import System]
        public static void DoExport(Mobile from, int mode) 
        {
            int dCount = 0, eCount = 0, vCount = 0, popCount = 0;
            if (mode == 0 || mode == 1)
            {
                string dPath = Path.Combine(Core.BaseDirectory, "Data", "NewRespawn.xml");
                using (XmlTextWriter xml = new XmlTextWriter(dPath, System.Text.Encoding.UTF8))
                {
                    xml.Formatting = Formatting.Indented; xml.WriteStartDocument(); xml.WriteStartElement("NewRespawn");
                    xml.WriteStartElement("DungeonNodes");
                    foreach (Item item in World.Items.Values)
                    {
                        if (item is DungeonNode n)
                        {
                            xml.WriteStartElement("Node"); xml.WriteAttributeString("Map", n.Map.Name); xml.WriteAttributeString("X", n.X.ToString()); xml.WriteAttributeString("Y", n.Y.ToString()); xml.WriteAttributeString("Z", n.Z.ToString());
                            xml.WriteAttributeString("RCode", ((int)n.RCode).ToString()); xml.WriteAttributeString("Depth", ((int)n.Depth).ToString()); xml.WriteAttributeString("SpawnRange", n.SpawnRange.ToString()); xml.WriteAttributeString("HomeRange", n.HomeRange.ToString()); xml.WriteEndElement(); dCount++;
                        }
                    }
                    xml.WriteEndElement();

                    xml.WriteStartElement("EcoNodes");
                    foreach (Item item in World.Items.Values)
                    {
                        if (item is EcoNode en)
                        {
                            xml.WriteStartElement("Node"); xml.WriteAttributeString("Map", en.Map.Name); xml.WriteAttributeString("X", en.X.ToString()); xml.WriteAttributeString("Y", en.Y.ToString()); xml.WriteAttributeString("Z", en.Z.ToString());
                            xml.WriteAttributeString("RCode", ((int)en.RCode).ToString()); xml.WriteAttributeString("AreaType", ((int)en.AreaType).ToString()); xml.WriteAttributeString("ClimateType", ((int)en.ClimateType).ToString()); xml.WriteAttributeString("SpawnRange", en.SpawnRange.ToString()); xml.WriteAttributeString("HomeRange", en.HomeRange.ToString()); xml.WriteEndElement(); eCount++;
                        }
                    }
                    xml.WriteEndElement();

                    xml.WriteStartElement("Populations");
                    foreach (var z in DungeonManager.ZoneList) if (z.ManualMaxPopulation >= 0) { xml.WriteStartElement("Pop"); xml.WriteAttributeString("RCode", ((int)z.RCode).ToString()); xml.WriteAttributeString("MaxPop", z.ManualMaxPopulation.ToString()); xml.WriteEndElement(); popCount++; }
                    xml.WriteEndElement(); xml.WriteEndElement(); xml.WriteEndDocument();
                }
            }

            if (mode == 0 || mode == 2)
            {
                string dir = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem"); if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string vPath = Path.Combine(dir, "NewVendor.xml");
                using (XmlWriter writer = XmlWriter.Create(vPath, new XmlWriterSettings { Indent = true, IndentChars = "\t" }))
                {
                    writer.WriteStartDocument(); writer.WriteStartElement("Vendors");
                    foreach (Item item in World.Items.Values)
                    {
                        if (item is VendorNode v)
                        {
                            writer.WriteStartElement("VendorNode"); writer.WriteAttributeString("Name", v.VendorName ?? "a vendor"); writer.WriteAttributeString("Map", v.Map?.Name ?? "Trammel");
                            writer.WriteAttributeString("X", v.X.ToString()); writer.WriteAttributeString("Y", v.Y.ToString()); writer.WriteAttributeString("Z", v.Z.ToString());
                            writer.WriteAttributeString("MaxCount", v.MaxCount.ToString()); writer.WriteAttributeString("Range", v.HomeRange.ToString()); 
                            int townID = v.TownID > 0 ? v.TownID : TownNumber.GetID(v.Location, v.Map); writer.WriteAttributeString("TownID", townID.ToString());
                            writer.WriteStartElement("Inventory");
                            if (townID > 0 && TownEconomyManager.Towns.TryGetValue(townID, out var town)) foreach (var entry in town.InventoryEntries) { writer.WriteStartElement("Item"); writer.WriteAttributeString("Type", entry.ItemType.Name); writer.WriteAttributeString("Amount", entry.InitialStock.ToString()); writer.WriteAttributeString("Price", entry.BasePrice.ToString()); writer.WriteEndElement(); }
                            writer.WriteEndElement(); writer.WriteEndElement(); vCount++;
                        }
                    }
                    writer.WriteEndElement(); writer.WriteEndDocument();
                }
            }
            from.SendMessage(68, $"[Export 완료] (D:{dCount} / Eco:{eCount} / V:{vCount} / Pop:{popCount})");
        }

        public static void DoImport(Mobile from, int mode)
        {
            if (mode == 0) CommandSystem.Handle(from, $"{CommandSystem.Prefix}BaseVendorWipe");
            else if (mode == 2)
            {
                string path = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "NewVendor.xml");
                if (File.Exists(path))
                {
                    foreach (var n in World.Items.Values.OfType<VendorNode>().Where(i => !i.Deleted).ToList()) n.Delete();
                    foreach (var v in World.Mobiles.Values.OfType<BaseVendor>().Where(m => !m.Deleted).ToList()) v.Delete();
                    foreach (var town in TownEconomyManager.Towns.Values) { town.VendorCount = 0; town.InventoryEntries.Clear(); town.Warehouse.Clear(); }

                    try
                    {
                        XmlDocument doc = new(); doc.Load(path);
                        XmlNodeList? nodes = doc.SelectNodes("//VendorNode") ?? doc.SelectNodes("//Vendor");
                        if (nodes != null)
                        {
                            foreach (XmlNode n in nodes)
                            {
                                Map map = Map.Parse(n.Attributes?["Map"]?.Value ?? "Trammel");
                                Point3D loc = new(int.Parse(n.Attributes?["X"]?.Value ?? "0"), int.Parse(n.Attributes?["Y"]?.Value ?? "0"), int.Parse(n.Attributes?["Z"]?.Value ?? "0"));
                                int townID = TownNumber.GetID(loc, map);

                                if (townID > 0)
                                {
                                    if (!TownEconomyManager.Towns.TryGetValue(townID, out var town)) { town = new TownEconomy(townID, 1000000) { Center = loc, Facet = map }; TownEconomyManager.Towns[townID] = town; }
                                    VendorNode vNode = new VendorNode { TownID = townID, VendorName = n.Attributes?["Name"]?.Value ?? "a vendor", MaxCount = int.Parse(n.Attributes?["MaxCount"]?.Value ?? "1"), HomeRange = int.Parse(n.Attributes?["Range"]?.Value ?? "5") };
                                    vNode.MoveToWorld(loc, map);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            from.SendMessage(68, $"[NewSpawn] Mode {mode} Processed.");
        }
        #endregion

        public static void DoResetDungeonNodes(Mobile from)
        {
            foreach (var item in World.Items.Values.OfType<DungeonNode>().ToList()) item.Delete();
            foreach (var z in DungeonManager.ZoneList) { z.ClearAllSpawns(); z.CacheNodes(); }
            from.SendMessage(33, $"[던전 리셋 완료]");
        }

        public static void DoResetEcoNodes(Mobile from)
        {
            foreach (var item in World.Items.Values.OfType<EcoNode>().ToList()) item.Delete();
            foreach (var z in EcosystemManager.ZoneList) { z.ClearAllSpawns(); z.CacheNodes(); }
            from.SendMessage(33, $"[생태계 리셋 완료]");
        }

        public static void DoResetVendorNodes(Mobile from) 
        { 
            foreach (var item in World.Items.Values.OfType<VendorNode>().ToList()) item.Delete(); 
            from.SendMessage(33, $"[상인 리셋 완료]"); 
        }

        public static void DoResetAll(Mobile from) { DoResetEcoNodes(from); }

        public static void DoReset(Mobile from)
        {
            Type xmlSpawnerType = ScriptCompiler.FindTypeByName("XmlSpawner");
            if (xmlSpawnerType == null) return;
            foreach (Item item in World.Items.Values.Where(i => i.GetType() == xmlSpawnerType).ToList()) item.Delete();
            from.SendMessage(68, $"구형 스포너 삭제 완료");
        }
    }

    #region [Gumps]
    public class NewSpawnGump : Gump
    {
        public NewSpawnGump() : base(100, 100)
        {
            AddPage(0); 
            AddBackground(0, 0, 560, 640, 9270); 
            AddAlphaRegion(10, 10, 540, 620);
            AddHtml(10, 15, 540, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>MASTER SPAWN MANAGER</BASEFONT></CENTER>", false, false);

            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            int y = 75; 
            for (int i = 0; i < maps.Length; i++)
            {
                AddImageTiled(20, y, 520, 38, 9354); 
                AddLabel(35, y + 9, 1152, maps[i].Name);
                
                AddButton(140, y + 7, 4005, 4007, (i * 10) + 1, GumpButtonType.Reply, 0); AddLabel(175, y + 9, 0x481, "DUNGEON");
                AddButton(260, y + 7, 4023, 4025, (i * 10) + 2, GumpButtonType.Reply, 0); AddLabel(295, y + 9, 0x481, "ECOLOGY");
                
                AddButton(380, y + 7, 4011, 4013, (i * 10) + 3, GumpButtonType.Reply, 0); AddLabel(415, y + 9, 68, "VENDOR RESPAWN");
                y += 42;
            }
            
            y += 5; AddImageTiled(20, y, 520, 38, 9354); AddButton(35, y + 7, 4011, 4013, 999, GumpButtonType.Reply, 0); AddLabel(75, y + 9, 0x35, "미매칭/에러 노드 리스트 (CHECK LIST)");
            
            y += 45; AddImageTiled(20, y, 520, 40, 9354); 
            AddButton(25, y + 8, 4005, 4007, 998, GumpButtonType.Reply, 0); AddLabel(60, y + 10, 0x42, "던전 모니터");
            AddButton(145, y + 8, 4023, 4025, 997, GumpButtonType.Reply, 0); AddLabel(180, y + 10, 0x42, "생태계 모니터");
            AddButton(285, y + 8, 4011, 4013, 996, GumpButtonType.Reply, 0); AddLabel(320, y + 10, 0x58, "자원/농사");
            
            AddButton(400, y + 8, 4020, 4022, 995, GumpButtonType.Reply, 0); AddLabel(435, y + 10, 53, "도시/경제 탭");
            
            y = 480; AddImageTiled(20, y, 520, 140, 9354);
            AddLabel(100, y + 35, 1152, "전체(ALL)"); AddLabel(250, y + 35, 0x481, "던전(DUNGEON)"); AddLabel(420, y + 35, 68, "벤더(VENDOR)");
            
            AddButton(35, y + 55, 4011, 4013, 810, GumpButtonType.Reply, 0); AddLabel(70, y + 57, 1152, "Export");
            AddButton(210, y + 55, 4011, 4013, 811, GumpButtonType.Reply, 0); AddLabel(245, y + 57, 1152, "Export");
            AddButton(380, y + 55, 4011, 4013, 812, GumpButtonType.Reply, 0); AddLabel(415, y + 57, 1152, "Export");
            
            AddButton(35, y + 80, 4005, 4007, 820, GumpButtonType.Reply, 0); AddLabel(70, y + 82, 0x42, "Import");
            AddButton(210, y + 80, 4005, 4007, 821, GumpButtonType.Reply, 0); AddLabel(245, y + 82, 0x42, "Import");
            AddButton(380, y + 80, 4005, 4007, 822, GumpButtonType.Reply, 0); AddLabel(415, y + 82, 0x42, "Import");
            
            AddButton(35, y + 105, 4020, 4022, 803, GumpButtonType.Reply, 0); AddLabel(70, y + 107, 0x21, "Eco Reset!");
            AddButton(210, y + 105, 4020, 4022, 831, GumpButtonType.Reply, 0); AddLabel(245, y + 107, 0x21, "Dungeon Reset");
            AddButton(380, y + 105, 4020, 4022, 832, GumpButtonType.Reply, 0); AddLabel(415, y + 107, 0x21, "Vendor Reset");
        }
        
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int btn = info.ButtonID; if (btn == 0) return;
            
            if (btn >= 810 && btn <= 812) { NewSpawnManager.DoExport(sender.Mobile, btn - 810); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn >= 820 && btn <= 822) { NewSpawnManager.DoImport(sender.Mobile, btn - 820); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn == 803) { NewSpawnManager.DoResetAll(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn == 831) { NewSpawnManager.DoResetDungeonNodes(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn == 832) { NewSpawnManager.DoResetVendorNodes(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }

            if (btn == 995) 
            { 
                GlobalEconomyMonitor.GenerateUserReport(); 
                sender.Mobile.SendGump(new EconomyAdminGump(sender.Mobile)); 
                return; 
            }

            if (btn == 999)
            {
                List<Item> checkList = new List<Item>();
                checkList.AddRange(World.Items.Values.OfType<DungeonNode>().Where(n => n.Map == sender.Mobile.Map && !NewSpawnManager.IsManaged(n.RCode)));
                checkList.AddRange(World.Items.Values.OfType<EcoNode>().Where(n => n.Map == sender.Mobile.Map && !NewSpawnManager.IsManaged(n.RCode)));
                sender.Mobile.SendGump(new NodeCheckGump(checkList, 0)); return;
            }
            
            if (btn == 998) { sender.Mobile.SendGump(new ZoneMonitorGump(0, 0)); return; } 
            if (btn == 997) { sender.Mobile.SendGump(new ZoneMonitorGump(1, 0)); return; } 
            if (btn == 996) { sender.Mobile.SendGump(new ZoneMonitorGump(2, 0)); return; } 
            
            if (btn / 10 < 6) 
            {
                Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
                Map targetMap = maps[btn / 10];
                
                if (btn % 10 == 3)
                {
                    int respawnCount = 0;
                    foreach (Item item in World.Items.Values)
                    {
                        if (item is VendorNode vNode && vNode.Map == targetMap && vNode.IsActive)
                        {
                            vNode.Respawn();
                            respawnCount++;
                        }
                    }
                    sender.Mobile.SendMessage(68, $"[{targetMap.Name}] {respawnCount}개의 벤더 노드 리스폰이 완료되었습니다.");
                }
                
                sender.Mobile.SendGump(new NewSpawnGump());
            }
        }
    }

    public class NodeCheckGump : Gump
    {
        private List<Item> m_List; private int m_Page;

        public NodeCheckGump(List<Item> list, int page) : base(500, 100)
        {
            m_List = list; m_Page = page;
            AddPage(0); AddBackground(0, 0, 550, 550, 9270);
            AddHtml(10, 15, 530, 20, $"<CENTER><BASEFONT COLOR='#FF5555'>미매칭 노드 리스트 (총 {list.Count}개)</BASEFONT></CENTER>", false, false);
            
            int start = page * 10; int end = Math.Min(start + 10, list.Count);
            
            for (int i = start; i < end; i++)
            {
                Item n = list[i]; string zoneName = "Unknown", typeName = "Node";
                if (n is DungeonNode dn) { zoneName = NewSpawnManager.GetDisplayName(dn.RCode); typeName = "DUNGEON"; }
                else if (n is EcoNode en) { zoneName = NewSpawnManager.GetDisplayName(en.RCode); typeName = "ECOSYSTEM"; }

                int y = 50 + ((i - start) * 45);
                AddImageTiled(15, y, 520, 40, 9354);
                AddHtml(20, y + 10, 430, 20, $"<BASEFONT COLOR='#FFFFFF'>[{typeName}] {zoneName}</BASEFONT>", false, false);
                AddButton(460, y + 8, 4005, 4007, i + 100, GumpButtonType.Reply, 0); AddLabel(495, y + 10, 1152, "GO");
            }
            if (page > 0) AddButton(20, 510, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddLabel(250, 510, 0xFFFFFF, $"{page + 1} / {Math.Max(1, (list.Count - 1) / 10 + 1)}");
            if (end < list.Count) AddButton(500, 510, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddButton(400, 15, 4014, 4016, 3, GumpButtonType.Reply, 0); 
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || info.ButtonID == 3) { sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (info.ButtonID == 1) sender.Mobile.SendGump(new NodeCheckGump(m_List, m_Page - 1));
            else if (info.ButtonID == 2) sender.Mobile.SendGump(new NodeCheckGump(m_List, m_Page + 1));
            else if (info.ButtonID >= 100 && info.ButtonID - 100 < m_List.Count) 
            {
                sender.Mobile.MoveToWorld(m_List[info.ButtonID - 100].Location, m_List[info.ButtonID - 100].Map);
                sender.Mobile.SendGump(new NodeCheckGump(m_List, m_Page));
            }
        }
    }

public class ZoneMonitorGump : Gump
    {
        private int m_Mode, m_SubMode, m_Page, m_MapFilter;

        public ZoneMonitorGump(int mode, int page) : this(mode, 0, page, 0) { } 

        public ZoneMonitorGump(int mode, int subMode, int page, int mapFilter) : base(30, 50)
        {
            m_Mode = mode; m_SubMode = subMode; m_Page = page; m_MapFilter = mapFilter;
            AddPage(0); AddBackground(0, 0, 950, 550, 9270); AddImageTiled(10, 10, 930, 530, 2624); AddAlphaRegion(10, 10, 930, 530);
            AddHtml(10, 15, 930, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>MASTER MONITOR</BASEFONT></CENTER>", false, false);
            
            AddImageTiled(20, 50, 910, 30, 9354);
            AddButton(30, 55, mode == 0 ? 4006 : 4005, 4007, 10, GumpButtonType.Reply, 0); AddLabel(65, 55, mode == 0 ? 68 : 1152, "던전 모니터링");
            AddButton(200, 55, mode == 1 ? 4006 : 4005, 4007, 11, GumpButtonType.Reply, 0); AddLabel(235, 55, mode == 1 ? 68 : 1152, "생태계 모니터링");
            AddButton(370, 55, mode == 2 ? 4006 : 4005, 4007, 13, GumpButtonType.Reply, 0); AddLabel(405, 55, mode == 2 ? 68 : 1152, "자원 생태계 모니터링");
            AddButton(820, 55, 4011, 4012, 12, GumpButtonType.Reply, 0); AddLabel(855, 55, 0xFFFFFF, "새로고침");

            int y = 85; AddImageTiled(20, y, 910, 30, 2624);
            string[] mapNames = { "전체", "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur" };
            Map[] mapRefs = { null, Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            Map currentFilterMap = mapRefs[m_MapFilter];

            for (int i = 0; i < mapNames.Length; i++) 
            { 
                AddButton(30 + (i * 90), y + 5, m_MapFilter == i ? 4006 : 4005, 4007, 70 + i, GumpButtonType.Reply, 0); 
                AddLabel(65 + (i * 90), y + 5, m_MapFilter == i ? 68 : 1152, mapNames[i]); 
            }
            if (m_MapFilter > 0 && currentFilterMap != null)
            {
                bool isActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(currentFilterMap, true);
                AddButton(690, y + 5, isActive ? 2361 : 2360, isActive ? 2361 : 2360, 800, GumpButtonType.Reply, 0); AddLabel(710, y + 3, isActive ? 68 : 33, isActive ? $"[{mapNames[m_MapFilter]}] 시스템 ON" : $"[{mapNames[m_MapFilter]}] 시스템 OFF");
            }
            y += 35;

            if (mode == 2)
            {
                AddImageTiled(20, y, 910, 25, 2624); string[] subNames = { "전체 자원", "광산", "벌목", "낚시", "농사" };
                for (int i = 0; i < subNames.Length; i++) { AddButton(30 + (i * 100), y + 2, m_SubMode == i ? 4006 : 4005, 4007, 50 + i, GumpButtonType.Reply, 0); AddLabel(65 + (i * 100), y + 2, m_SubMode == i ? 68 : 1152, subNames[i]); }
                y += 30; 
            }

            int start = m_Page * 10, totalListCount = 0;

            if (mode == 0) // Dungeon
            {
                AddHtml(25, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>컨트롤</BASEFONT>", false, false); AddHtml(200, y, 250, 20, "<BASEFONT COLOR='#FFFF00'>던전 구역명</BASEFONT>", false, false); AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>상태</BASEFONT>", false, false); AddHtml(560, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>난이도</BASEFONT>", false, false); AddHtml(710, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>인구 설정</BASEFONT>", false, false); y += 25;
                
                var list = DungeonManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID)).ToList();
                totalListCount = list.Count; int end = Math.Min(start + 10, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var z = list[i]; AddImageTiled(20, y - 2, 910, 24, 9354);
                    if (z.Nodes.Count > 0) { AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 1152, $"GO({z.Nodes.Count})"); if (z.Nodes.Count > 1) { AddButton(110, y + 2, 4017, 4018, 600 + (i - start), GumpButtonType.Reply, 0); AddLabel(145, y, 0x35, "정리"); } }
                    else { AddButton(25, y + 2, 4011, 4013, 400 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 33, "생성"); }

                    AddLabel(200, y, 0xFFFFFF, NewSpawnManager.GetDisplayName(z.RCode));
                    double heatPct = z.MaxDifficulty > 0 ? (double)z.CurrentDifficulty / z.MaxDifficulty : 0;
                    AddLabel(560, y, heatPct >= 0.8 ? 33 : (heatPct >= 0.4 ? 1258 : 1152), $"{z.CurrentDifficulty:N0} / {z.MaxDifficulty:N0} ({heatPct:P0})");
                    AddLabel(450, y, z.MaxPopulation == 0 ? 33 : (z.Phase == DungeonPhase.Active ? 68 : (z.Phase == DungeonPhase.BossSpawned ? 33 : 1359)), z.MaxPopulation == 0 ? "잠금됨" : (z.Phase == DungeonPhase.Active ? "사냥 중" : (z.Phase == DungeonPhase.BossSpawned ? "보스 등장!" : "휴식기")));
                    AddLabel(710, y, 0xFFFFFF, $"{z.GetTotalActiveCount()} /"); AddImageTiled(780, y - 1, 55, 22, 2624); AddAlphaRegion(780, y - 1, 55, 22); AddTextEntry(785, y, 45, 20, 53, i - start, z.ManualMaxPopulation >= 0 ? z.ManualMaxPopulation.ToString() : z.MaxPopulation.ToString()); AddButton(840, y + 2, 4023, 4025, 200 + (i - start), GumpButtonType.Reply, 0); AddLabel(875, y, 68, "SET");
                    y += 30;
                }
            }
            else if (mode == 1) // Ecology
            {
                AddHtml(25, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>컨트롤</BASEFONT>", false, false); AddHtml(200, y, 250, 20, "<BASEFONT COLOR='#FFFF00'>생태계 구역명</BASEFONT>", false, false); AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>상태</BASEFONT>", false, false); AddHtml(560, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>개체수</BASEFONT>", false, false); AddHtml(710, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>평균 활력</BASEFONT>", false, false); y += 25;
                
                var list = EcosystemManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID)).ToList();
                totalListCount = list.Count; int end = Math.Min(start + 10, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var z = list[i]; AddImageTiled(20, y - 2, 910, 24, 9354);
                    if (z.Nodes.Count > 0) { AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 1152, $"GO({z.Nodes.Count})"); if (z.Nodes.Count > 1) { AddButton(110, y + 2, 4017, 4018, 600 + (i - start), GumpButtonType.Reply, 0); AddLabel(145, y, 0x35, "정리"); } }
                    else { AddButton(25, y + 2, 4011, 4013, 400 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 33, "생성"); }

                    AddLabel(200, y, 0xFFFFFF, NewSpawnManager.GetDisplayName(z.RCode)); AddLabel(450, y, 68, "독립 통제"); AddLabel(560, y, 0xFFFFFF, $"{z.Nodes.Count} 노드"); AddLabel(710, y, 68, "자율 생태계");
                    y += 28; 
                }
            }
            else if (mode == 2) // Resource
            {
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false); AddHtml(70, y, 80, 20, "<BASEFONT COLOR='#FFFF00'>종류</BASEFONT>", false, false); AddHtml(160, y, 200, 20, "<BASEFONT COLOR='#FFFF00'>구역명</BASEFONT>", false, false); AddHtml(400, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>잔여량</BASEFONT>", false, false); AddHtml(560, y, 300, 20, "<BASEFONT COLOR='#FFFF00'>상태 및 재료</BASEFONT>", false, false); y += 25;
                var list = ResourceManager.PoolList.Where(p => currentFilterMap == null || (p.Facet != null && p.Facet.MapID == currentFilterMap.MapID)).ToList();
                if (m_SubMode > 0) list = list.Where(p => p.Type == (ResourceType)(m_SubMode == 1 ? 0 : m_SubMode == 2 ? 1 : m_SubMode == 3 ? 2 : 4)).ToList();
                list = list.Where(p => p.LocType != LocationType.Farm_Remote && !p.IsPrivate).ToList();
                totalListCount = list.Count; int end = Math.Min(start + 10, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var pool = list[i]; AddImageTiled(20, y - 2, 910, 24, 9354); AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
                    double percent = pool.MaxCapacity > 0 ? ((double)pool.CurrentCapacity / pool.MaxCapacity) * 100.0 : 0;
                    int color = percent < 50.0 ? 33 : (percent > 90.0 ? 68 : 0xFFFFFF);

                    AddLabel(70, y, color, pool.Type.ToString());
                    string pName = pool.RCode == RegionCode.None ? "해양 구역" : NewSpawnManager.GetDisplayName(pool.RCode);
                    AddLabel(160, y, color, pName.Length > 25 ? pName.Substring(0, 25) : pName);
                    AddLabel(400, y, color, string.Format("{0}/{1} ({2:F0}%)", pool.CurrentCapacity, pool.MaxCapacity, percent));
                    
                    TimeSpan cd = pool.DepletionCooldown - DateTime.Now;
                    
                    // 🌟 [해결] 시스템 OFF 상태면 UI에 가장 먼저 "시스템 정지 (OFF)" 출력
                    bool isMapActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(pool.Facet, true);
                    if (!isMapActive) 
                        AddHtml(560, y, 350, 20, "<BASEFONT COLOR='#777777'>시스템 정지 (OFF)</BASEFONT>", false, false);
                    else if (cd.TotalSeconds > 0) 
                        AddHtml(560, y, 350, 20, string.Format("<BASEFONT COLOR='#FF3333'>고갈 ({0:F1}분)</BASEFONT>", cd.TotalMinutes), false, false);
                    else 
                        AddHtml(560, y, 350, 20, "<BASEFONT COLOR='#42FF42'>정상 스폰 중</BASEFONT>", false, false);
                    
                    y += 30;
                }
            }

            if (m_Page > 0) AddButton(20, 500, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddLabel(460, 500, 0xFFFFFF, string.Format("{0} / {1}", m_Page + 1, Math.Max(1, (totalListCount - 1) / 10 + 1)));
            if ((start + 10) < totalListCount) AddButton(900, 500, 4005, 4007, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (info.ButtonID == 0 || info.ButtonID == 3) { from.SendGump(new NewSpawnGump()); return; }
            
            if (info.ButtonID >= 50 && info.ButtonID <= 54) { from.SendGump(new ZoneMonitorGump(m_Mode, info.ButtonID - 50, 0, m_MapFilter)); return; }
            if (info.ButtonID >= 70 && info.ButtonID <= 76) { from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, 0, info.ButtonID - 70)); return; }
            
            if (info.ButtonID == 12) 
            { 
                foreach (var dz in DungeonManager.ZoneList) dz.CacheNodes();
                EcosystemManager.RebuildZones();
                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page, m_MapFilter)); 
                return; 
            }

            if (info.ButtonID == 10) { from.SendGump(new ZoneMonitorGump(0, 0, 0, m_MapFilter)); return; }
            if (info.ButtonID == 11) { from.SendGump(new ZoneMonitorGump(1, 0, 0, m_MapFilter)); return; }
            if (info.ButtonID == 13) { from.SendGump(new ZoneMonitorGump(2, 0, 0, m_MapFilter)); return; }
            if (info.ButtonID == 1)  { from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page - 1, m_MapFilter)); return; }
            if (info.ButtonID == 2)  { from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page + 1, m_MapFilter)); return; }

            Map[] mapRefs = { null, Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            Map currentFilterMap = mapRefs[m_MapFilter];

            // 🌟 [핵심 패치] 대륙 시스템 ON/OFF 논리 반전 오류 수정 및 청소 로직 최적화
            if (info.ButtonID == 800 && m_MapFilter > 0 && currentFilterMap != null)
            {
                bool wasActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(currentFilterMap, true);
                bool nowActive = !wasActive; // 켜져있으면 끄고, 꺼져있으면 켭니다.
                NewSpawnManager.ActiveMaps[currentFilterMap] = nowActive;

                if (!nowActive) // 시스템을 껐을 때 (OFF)
                {
                    // 1. 던전 노드 파괴
                    var nodes = World.Items.Values.OfType<DungeonNode>().Where(n => n.Map == currentFilterMap).ToList();
                    foreach (var n in nodes) n.Delete();
                    int dCount = 0;
                    foreach (var dz in DungeonManager.ZoneList.Where(z => z.Facet == currentFilterMap)) { dCount += dz.GetTotalActiveCount(); dz.ClearAllSpawns(); dz.CacheNodes(); }

                    // 2. 생태계(EcoNode) 제거 및 청소 (🌟 숲 동물도 이쪽 소속이므로 한 번에 청소됨)
                    int eCount = 0;
                    var eNodes = World.Items.Values.OfType<EcoNode>().Where(n => n.Map == currentFilterMap).ToList();
                    foreach (var n in eNodes) { n.Delete(); eCount++; }
                    foreach (var ez in EcosystemManager.ZoneList.Where(z => z.Facet == currentFilterMap)) { ez.ClearAllSpawns(); }
                    EcosystemManager.RebuildZones();

                    // 3. 자원 부산물 몬스터(엘리멘탈/크라켄) 청소
                    int rCount = 0;
                    foreach (var pool in ResourceManager.PoolList.Where(p => p.Facet == currentFilterMap))
                    {
                        rCount += pool.ActiveMonsters.Count;
                        foreach (var m in pool.ActiveMonsters) m?.Delete();
                        pool.ActiveMonsters.Clear();
                    }

                    from.SendMessage(33, $"[{currentFilterMap.Name}] 리스폰 정지! 던전({nodes.Count}개)/생태계({eCount}개) 노드 삭제 및 몹 청소 완료.");
                }
                else // 시스템을 켰을 때 (ON)
                {
                    // 1. 파괴되었던 던전 노드 원상 복구
                    int dNodeCount = 0;
                    foreach (var dz in DungeonManager.ZoneList.Where(z => z.Facet == currentFilterMap))
                    {
                        if (dz.Nodes.Count == 0)
                        {
                            Point3D centerLoc = RegionSaver.GetRegionCenter(dz.RCode, dz.Facet);
                            if (centerLoc != Point3D.Zero)
                            {
                                DungeonNode newNode = new DungeonNode();
                                newNode.RCode = dz.RCode;
                                newNode.SpawnRange = 30;
                                newNode.HomeRange = 50;
                                newNode.Depth = DungeonDepth.Entrance;
                                newNode.MoveToWorld(centerLoc, dz.Facet);
                                dNodeCount++;
                            }
                        }
                    }
                    foreach (var dz in DungeonManager.ZoneList.Where(z => z.Facet == currentFilterMap)) dz.CacheNodes();

                    // 2. 생태계(EcoNode) 재생성
                    int eNodeCount = 0;
                    foreach (var kvp in EcoGridDatabase.Chunks.Where(c => c.Key.Facet == currentFilterMap))
                    {
                        bool exists = World.Items.Values.OfType<EcoNode>().Any(n => n.Map == currentFilterMap && n.RCode == kvp.Value.Code);
                        if (!exists)
                        {
                            EcoNode node = new EcoNode();
                            node.RCode = kvp.Value.Code; 
                            NewSpawnManager.ApplyDefaultSettings(node);

                            if (node.RCode == RegionCode.None)
                            {
                                int mapId = kvp.Key.Facet.MapID;
                                int cx = kvp.Value.CenterX / 128;
                                int cy = kvp.Value.CenterY / 128;
                                int pseudoCode = ((mapId + 1) * 1000000) + (cx * 1000) + cy;
                                node.RCode = (RegionCode)pseudoCode;
                            }

                            int z = kvp.Key.Facet.GetAverageZ(kvp.Value.CenterX, kvp.Value.CenterY);
                            node.MoveToWorld(new Point3D(kvp.Value.CenterX, kvp.Value.CenterY, z), kvp.Key.Facet);
                            eNodeCount++;
                        }
                    }
                    EcosystemManager.RebuildZones();

                    from.SendMessage(68, $"[{currentFilterMap.Name}] 시스템 재가동! 던전({dNodeCount}개)/생태계({eNodeCount}개) 노드 복구 완료.");
                }

                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page, m_MapFilter));
                return;
            }

            int listIndex = -1;
            if (info.ButtonID >= 200 && info.ButtonID < 300) listIndex = info.ButtonID - 200;
            else if (info.ButtonID >= 300 && info.ButtonID < 400) listIndex = info.ButtonID - 300;
            else if (info.ButtonID >= 400 && info.ButtonID < 500) listIndex = info.ButtonID - 400; 
            else if (info.ButtonID >= 600 && info.ButtonID < 700) listIndex = info.ButtonID - 600;

            if (listIndex >= 0)
            {
                int targetIndex = (m_Page * 10) + listIndex;

                if (m_Mode == 0) // Dungeon
                {
                    var list = DungeonManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID)).ToList();
                    if (targetIndex < list.Count)
                    {
                        var z = list[targetIndex];
                        if (info.ButtonID >= 200 && info.ButtonID < 300) // SET
                        {
                            TextRelay tr = info.GetTextEntry(listIndex);
                            if (tr != null)
                            {
                                try
                                {
                                    int newPop = int.Parse(tr.Text);
                                    z.SetPopulation(newPop);
                                    from.SendMessage(68, $"{NewSpawnManager.GetDisplayName(z.RCode)} 구역의 최대 인구가 {newPop}으로 설정되었습니다.");
                                }
                                catch { from.SendMessage(33, "잘못된 숫자 형식입니다."); }
                            }
                        }
                        else if (info.ButtonID >= 300 && info.ButtonID < 400 && z.Nodes.Count > 0) // GO
                        {
                            from.MoveToWorld(z.Nodes[0].Location, z.Nodes[0].Map);
                        }
                        else if (info.ButtonID >= 400 && info.ButtonID < 500 && z.Nodes.Count == 0) // 생성
                        {
                            DungeonNode newNode = new DungeonNode();
                            newNode.RCode = z.RCode;
                            newNode.SpawnRange = 30;
                            newNode.HomeRange = 50;
                            newNode.Depth = DungeonDepth.Entrance;
                            newNode.MoveToWorld(from.Location, from.Map);
                            
                            if (z.Facet == null) z.Facet = from.Map; 
                            z.CacheNodes(); 
                            
                            from.SendMessage(68, $"[던전 노드 생성] {NewSpawnManager.GetDisplayName(z.RCode)} 통제 노드가 플레이어 발밑에 설치되었습니다. 스폰을 개시합니다!");
                        }
                        else if (info.ButtonID >= 600 && info.ButtonID < 700 && z.Nodes.Count > 1) // 정리
                        {
                            for (int i = 1; i < z.Nodes.Count; i++) z.Nodes[i].Delete();
                            z.CacheNodes();
                            from.SendMessage(68, "중복 던전 노드가 정리되었습니다.");
                        }
                    }
                }
                else if (m_Mode == 1) // Ecology
                {
                    var list = EcosystemManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID)).ToList();
                    if (targetIndex < list.Count)
                    {
                        var z = list[targetIndex];
                        if (info.ButtonID >= 300 && info.ButtonID < 400 && z.Nodes.Count > 0) // GO
                        {
                            from.MoveToWorld(z.Nodes[0].Location, z.Nodes[0].Map);
                        }
                        else if (info.ButtonID >= 400 && info.ButtonID < 500 && z.Nodes.Count == 0) // 생성
                        {
                            EcoNode newNode = new EcoNode();
                            newNode.RCode = z.RCode;
                            newNode.SpawnRange = 30;
                            newNode.HomeRange = 50;
                            newNode.MoveToWorld(from.Location, from.Map);
                            
                            if (z.Facet == null) z.Facet = from.Map; 
                            z.CacheNodes();
                            
                            from.SendMessage(68, $"[생태계 노드 생성] {NewSpawnManager.GetDisplayName(z.RCode)} 생태 노드가 플레이어 발밑에 설치되었습니다.");
                        }
                        else if (info.ButtonID >= 600 && info.ButtonID < 700 && z.Nodes.Count > 1) // 정리
                        {
                            for (int i = 1; i < z.Nodes.Count; i++) z.Nodes[i].Delete();
                            z.CacheNodes();
                            from.SendMessage(68, "중복 생태계 노드가 정리되었습니다.");
                        }
                    }
                }
                else if (m_Mode == 2) // Resource
                {
                    var list = ResourceManager.PoolList.Where(p => currentFilterMap == null || (p.Facet != null && p.Facet.MapID == currentFilterMap.MapID)).ToList();
                    if (m_SubMode > 0) list = list.Where(p => p.Type == (ResourceType)(m_SubMode == 1 ? 0 : m_SubMode == 2 ? 1 : m_SubMode == 3 ? 2 : 4)).ToList();
                    list = list.Where(p => p.LocType != LocationType.Farm_Remote && !p.IsPrivate).ToList();
                    
                    if (targetIndex < list.Count)
                    {
                        var pool = list[targetIndex];
                        if (info.ButtonID >= 300 && info.ButtonID < 400) // GO
                        {
                            Point3D? loc = NewSpawnManager.FindLocationByRegionCode(pool.RCode, pool.Facet);
                            if (loc.HasValue) from.MoveToWorld(loc.Value, pool.Facet);
                            else from.SendMessage(33, "이동할 정확한 기준 좌표를 찾을 수 없습니다.");
                        }
                    }
                }
            }
            
            from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page, m_MapFilter));
        }
    }
	#endregion
}