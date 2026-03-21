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
        public static void Initialize()
        {
            CommandSystem.Register("newspawn", AccessLevel.Administrator, new CommandEventHandler(OnNewSpawn));
            CommandSystem.Register("zonemonitor", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("zm", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("fixallnodes", AccessLevel.Administrator, new CommandEventHandler(OnFixAllNodes));
            CommandSystem.Register("wipeworldspawns", AccessLevel.Administrator, new CommandEventHandler(OnWipeWorldSpawns));
            CommandSystem.Register("wipeoldspawns", AccessLevel.Administrator, new CommandEventHandler(OnWipeOldSpawns));
			CommandSystem.Register("wipewildcrops", AccessLevel.Administrator, new CommandEventHandler(OnWipeWildCrops));
        }

		[Usage("wipewildcrops")]
		[Description("현재 맵의 모든 야생 작물을 삭제하고 자원 카운트를 0으로 초기화합니다.")]
		public static void OnWipeWildCrops(CommandEventArgs e)
		{
			Mobile from = e.Mobile;
			Map targetMap = from.Map;

			if (targetMap == null || targetMap == Map.Internal)
				return;

			int itemDeleted = 0;
			int poolReset = 0;

			// 1. 맵상의 모든 BaseFarmItem 삭제 (개인 농장은 제외하고 싶다면 조건 추가 가능)
			List<Item> toDelete = new List<Item>();
			foreach (Item item in World.Items.Values)
			{
				if (item.Map == targetMap && item is BaseFarmItem)
				{
					// 유저가 심은 개인 농장 작물은 살려두고 싶다면 아래 주석을 해제하세요.
					// if (((BaseFarmItem)item).Owner != null) continue; 
					
					toDelete.Add(item);
				}
			}

			itemDeleted = toDelete.Count;
			foreach (Item i in toDelete) i.Delete();

			// 2. ResourceManager의 모든 Farming Pool 카운트를 0으로 초기화
			foreach (var kvp in ResourceManager.Pools)
			{
				// 현재 맵에 속한 농사(Farming) 풀만 타겟팅
				if (kvp.Key.MapName == targetMap.Name && kvp.Key.Type == ResourceType.Farming)
				{
					kvp.Value.CurrentCapacity = 0;
					poolReset++;
				}
			}

			from.SendMessage(68, $"{targetMap.Name}: 작물 {itemDeleted}개 삭제 및 {poolReset}개 구역 카운트 초기화 완료.");
		}
        [Usage("fixallnodes")]
        [Description("모든 노드의 구역 정보를 현재 위치 기반으로 재설정합니다.")]
        public static void OnFixAllNodes(CommandEventArgs e)
        {
            int count = 0;
            foreach (Item item in World.Items.Values)
            {
                if (item is DungeonNode node)
                {
                    string newZone = GetGoGumpZoneName(node.Location, node.Map);
                    node.ZoneId = newZone;
                    count++;
                }
            }
            e.Mobile.SendMessage(68, $"총 {count}개의 노드 정보를 갱신했습니다.");
        }

        [Usage("wipeworldspawns")]
        [Description("전 세계의 모든 XmlSpawner를 삭제합니다. (매우 위험)")]
        public static void OnWipeWorldSpawns(CommandEventArgs e)
        {
            // 이 기능은 DoReset과 유사하지만 맵 제한 없이 전체를 타겟팅할 때 사용합니다.
            DoReset(e.Mobile);
        }

        // ★ [복구 완료] wipeoldspawns: 현재 맵의 모든 구형 스포너 청소
        [Usage("wipeoldspawns")]
        [Description("현재 맵에 존재하는 모든 XmlSpawner와 기본 Spawner를 삭제합니다.")]
        public static void OnWipeOldSpawns(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            Map targetMap = from.Map;

            if (targetMap == null || targetMap == Map.Internal)
                return;

            int count = 0;
            List<Item> toDelete = new List<Item>();
            Type xmlSpawnerType = ScriptCompiler.FindTypeByName("XmlSpawner");

            // [수정] Map.Items.Values 대신 World.Items.Values를 순회하며 맵 필터링
            foreach (Item item in World.Items.Values)
            {
                if (item.Map != targetMap)
                    continue;

                // 1. XmlSpawner 체크
                if (xmlSpawnerType != null && item.GetType() == xmlSpawnerType)
                {
                    if (!IsSafeCheck(item)) // 안전 검사 (상인/뱅커 등 제외)
                        toDelete.Add(item);
                }
                // 2. 기본 Spawner 체크
                else if (item is Spawner)
                {
                    toDelete.Add(item);
                }
            }

            count = toDelete.Count;
            foreach (Item i in toDelete) 
                i.Delete();

            from.SendMessage(68, $"{targetMap.Name} 맵에서 총 {count}개의 구형 스포너를 삭제했습니다.");
        }

        public static bool IsManaged(string nodeZoneId)
        {
            if (string.IsNullOrEmpty(nodeZoneId)) return false;
            string cleanNode = DungeonManager.CleanString(nodeZoneId);
            
            var allKeys = DungeonManager.Zones.Keys.Concat(EcosystemManager.Zones.Keys);
            foreach (string key in allKeys)
            {
                string cleanKey = DungeonManager.CleanString(key);
                if (cleanNode.Contains(cleanKey) || cleanKey.Contains(cleanNode)) return true;
            }
            return false;
        }

        public static string FindBestLogicKey(string rawPath)
        {
            if (string.IsNullOrEmpty(rawPath)) return null;
            string cleanRaw = DungeonManager.CleanString(rawPath);
            var allKeys = DungeonManager.Zones.Keys.Concat(EcosystemManager.Zones.Keys);
            foreach (string key in allKeys)
            {
                string ck = DungeonManager.CleanString(key);
                if (cleanRaw == ck || cleanRaw.Contains(ck) || ck.Contains(cleanRaw)) return key;
            }
            return null;
        }

        public static void DoExport(Mobile from)
        {
            string dir = Path.Combine(Core.BaseDirectory, "Data");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "NewRespawn.xml");

            int nodeCount = 0, popCount = 0;

            using (XmlTextWriter xml = new XmlTextWriter(path, System.Text.Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement("NewRespawn");

                xml.WriteStartElement("Nodes");
                foreach (Item item in World.Items.Values)
                {
                    if (item is DungeonNode node)
                    {
                        xml.WriteStartElement("Node");
                        xml.WriteAttributeString("Map", node.Map.Name);
                        xml.WriteAttributeString("X", node.X.ToString());
                        xml.WriteAttributeString("Y", node.Y.ToString());
                        xml.WriteAttributeString("Z", node.Z.ToString());
                        xml.WriteAttributeString("ZoneId", node.ZoneId);
                        xml.WriteAttributeString("Depth", ((int)node.Depth).ToString());
                        xml.WriteAttributeString("SpawnRange", node.SpawnRange.ToString());
                        xml.WriteAttributeString("HomeRange", node.HomeRange.ToString());
                        xml.WriteEndElement();
                        nodeCount++;
                    }
                }
                xml.WriteEndElement();

                xml.WriteStartElement("Populations");
                foreach (var z in DungeonManager.Zones.Values)
                {
                    if (z.ManualMaxPopulation >= 0)
                    {
                        xml.WriteStartElement("Pop");
                        xml.WriteAttributeString("ZoneId", z.ZoneId);
                        xml.WriteAttributeString("MaxPop", z.ManualMaxPopulation.ToString());
                        xml.WriteEndElement();
                        popCount++;
                    }
                }
                xml.WriteEndElement();

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
            from.SendMessage(68, $"[내보내기 완료] 노드 {nodeCount}개 / 설정 {popCount}개가 Data/NewRespawn.xml에 저장되었습니다.");
        }

        public static void DoImport(Mobile from)
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "NewRespawn.xml");
            if (!File.Exists(path))
            {
                from.SendMessage(33, "오류: Data/NewRespawn.xml 파일을 찾을 수 없습니다.");
                return;
            }

            List<DungeonNode> existingNodes = World.Items.Values.OfType<DungeonNode>().ToList();
            foreach (var n in existingNodes) n.Delete();

            int nodeCount = 0, popCount = 0;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);

                XmlNodeList nodes = doc.SelectNodes("/NewRespawn/Nodes/Node");
                if (nodes != null)
                {
                    foreach (XmlNode n in nodes)
                    {
                        try
                        {
                            Map map = Map.Parse(n.Attributes["Map"].Value);
                            int x = int.Parse(n.Attributes["X"].Value);
                            int y = int.Parse(n.Attributes["Y"].Value);
                            int z = int.Parse(n.Attributes["Z"].Value);
                            if (map != null && map != Map.Internal)
                            {
                                DungeonNode newNode = new DungeonNode
                                {
                                    ZoneId = n.Attributes["ZoneId"].Value,
                                    Depth = (DungeonDepth)int.Parse(n.Attributes["Depth"].Value),
                                    SpawnRange = int.Parse(n.Attributes["SpawnRange"].Value),
                                    HomeRange = int.Parse(n.Attributes["HomeRange"].Value)
                                };
                                newNode.MoveToWorld(new Point3D(x, y, z), map);
                                nodeCount++;
                            }
                        }
                        catch { }
                    }
                }

                XmlNodeList pops = doc.SelectNodes("/NewRespawn/Populations/Pop");
                if (pops != null)
                {
                    foreach (XmlNode p in pops)
                    {
                        string zid = p.Attributes["ZoneId"].Value;
                        if (int.TryParse(p.Attributes["MaxPop"].Value, out int pop) && DungeonManager.Zones.TryGetValue(zid, out var zone))
                        {
                            zone.SetPopulation(pop);
                            popCount++;
                        }
                    }
                }

                foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
                foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();

                from.SendMessage(68, $"[가져오기 완료] 노드 {nodeCount}개와 설정 {popCount}개를 성공적으로 이식했습니다.");
            }
            catch (Exception ex)
            {
                from.SendMessage(33, $"XML 읽기 오류 발생: {ex.Message}");
            }
        }

        public static void DoReset(Mobile from)
        {
            int deletedCount = 0, protectedCount = 0;
            Type xmlSpawnerType = ScriptCompiler.FindTypeByName("XmlSpawner");
            if (xmlSpawnerType == null)
            {
                from.SendMessage(33, "서버에서 XmlSpawner 시스템을 찾을 수 없습니다.");
                return;
            }

            List<Item> spawnersToDelete = new List<Item>();
            foreach (Item item in World.Items.Values)
            {
                if (item.GetType() == xmlSpawnerType)
                {
                    if (IsSafeCheck(item)) { protectedCount++; continue; }
                    string rawPath = GetGoGumpZoneName(item.Location, item.Map);
                    if (FindBestLogicKey(rawPath) != null) spawnersToDelete.Add(item);
                }
            }

            foreach (Item spawner in spawnersToDelete) { spawner.Delete(); deletedCount++; }
            from.SendMessage(68, $"[리셋 완료] 사냥터 XmlSpawner {deletedCount}개 삭제됨 (보호된 NPC 스포너: {protectedCount}개)");
        }

        private static bool IsSafeCheck(Item spawner)
        {
            try
            {
                var spawnObjectsInfo = spawner.GetType().GetProperty("m_SpawnObjects") ?? spawner.GetType().GetProperty("SpawnObjects");
                if (spawnObjectsInfo != null)
                {
                    var spawnObjects = spawnObjectsInfo.GetValue(spawner, null) as System.Collections.IList;
                    if (spawnObjects == null || spawnObjects.Count == 0) return false;

                    foreach (var so in spawnObjects)
                    {
                        string typeName = (string)so.GetType().GetProperty("TypeName")?.GetValue(so, null);
                        if (string.IsNullOrEmpty(typeName)) continue;
                        string name = typeName.ToLower();
                        if (name.Contains("healer")) return false; 
                        if (name.Contains("vendor") || name.Contains("banker") || name.Contains("guard") || name.Contains("guildmaster") || name.Contains("stablemaster") || name.Contains("innkeeper"))
                        {
                            Type tCheck = ScriptCompiler.FindTypeByName(typeName);
                            if (tCheck != null && tCheck.IsSubclassOf(typeof(BaseCreature)) && !tCheck.IsSubclassOf(typeof(BaseHealer))) continue;
                            return true; 
                        }
                        Type t = ScriptCompiler.FindTypeByName(typeName);
                        if (t != null && t.IsSubclassOf(typeof(BaseVendor))) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        [Usage("newspawn")] public static void OnNewSpawn(CommandEventArgs e) => e.Mobile.SendGump(new NewSpawnGump());
        [Usage("zonemonitor")] public static void OnMonitor(CommandEventArgs e) => e.Mobile.SendGump(new ZoneMonitorGump(0, 0));

        public static void DoMigration(Mobile from, Map map, int mode)
        {
            int newlyAddedCount = 0;
            foreach (Region r in Region.Regions)
            {
                if (r.Map != map || string.IsNullOrEmpty(r.Name)) continue;
                bool isDungeonReg = (r is DungeonRegion || r.Name.ToLower().Contains("dungeon"));
                if ((mode == 0 && !isDungeonReg) || (mode == 1 && isDungeonReg)) continue;
                if (!World.Items.Values.OfType<DungeonNode>().Any(n => n.Map == map && Region.Find(n.Location, n.Map) == r))
                {
                    Point3D spawnLoc = GetRegionCenter(r);
                    if (spawnLoc != Point3D.Zero)
                    {
                        string rPath = GetGoGumpZoneName(spawnLoc, map);
                        string logicKey = FindBestLogicKey(rPath);
                        if (logicKey != null)
                        {
                            DungeonNode emptyNode = new DungeonNode { ZoneId = logicKey, SpawnRange = 15 };
                            emptyNode.MoveToWorld(spawnLoc, map); newlyAddedCount++;
                        }
                    }
                }
            }
            foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
            foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();
            from.SendMessage(68, $"오토 마이그레이션 완료: {newlyAddedCount}개 자동 생성됨.");
        }

        public static string GetGoGumpZoneName(Point3D loc, Map map)
        {
            LocationTree tree = (map == Map.Felucca) ? GoGump.Felucca : (map == Map.Trammel ? GoGump.Trammel : (map == Map.Ilshenar ? GoGump.Ilshenar : (map == Map.Malas ? GoGump.Malas : (map == Map.Tokuno ? GoGump.Tokuno : GoGump.TerMur))));
            if (tree == null || tree.Root == null) return "Unknown";
            int bestDist = int.MaxValue; string bestPath = "Unknown";
            FindClosestGoGumpNode(tree.Root, loc, "", ref bestDist, ref bestPath);
            return bestPath.Replace("Locations ", "").Trim();
        }

        private static void FindClosestGoGumpNode(ParentNode node, Point3D target, string currentPath, ref int bestDist, ref string bestPath)
        {
            string path = string.IsNullOrEmpty(currentPath) ? node.Name : currentPath + " " + node.Name;
            if (node.Children == null) return;
            foreach (object child in node.Children)
            {
                if (child is ParentNode pNode) FindClosestGoGumpNode(pNode, target, path, ref bestDist, ref bestPath);
                else if (child is ChildNode cNode)
                {
                    int dist = (int)Math.Sqrt(Math.Pow(cNode.Location.X - target.X, 2) + Math.Pow(cNode.Location.Y - target.Y, 2));
                    if (dist < bestDist) { bestDist = dist; bestPath = path + " " + cNode.Name; }
                }
            }
        }

        private static Point3D GetRegionCenter(Region r) { if (r.Area != null && r.Area.Length > 0) { var a = r.Area[0]; return new Point3D(a.Start.X + ((a.End.X - a.Start.X) / 2), a.Start.Y + ((a.End.Y - a.Start.Y) / 2), r.Map.GetAverageZ(a.Start.X, a.Start.Y)); } return Point3D.Zero; }
    }

    public class NewSpawnGump : Gump
    {
        public NewSpawnGump() : base(100, 100)
        {
            int totalPools = ResourceManager.Pools.Count;
            int miningCount = ResourceManager.Pools.Values.Count(p => p.Type == ResourceType.Mining);
            int lumberCount = ResourceManager.Pools.Values.Count(p => p.Type == ResourceType.Lumberjacking);
            int fishingCount = ResourceManager.Pools.Values.Count(p => p.Type == ResourceType.Fishing);
            int farmingCount = ResourceManager.Pools.Values.Count(p => p.Type == ResourceType.Farming);

            AddPage(0); 
            AddBackground(0, 0, 480, 640, 9270); 
            AddAlphaRegion(10, 10, 460, 620);

            AddHtml(10, 15, 460, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>MASTER SPAWN MANAGER</BASEFONT></CENTER>", false, false);
            AddHtml(10, 45, 460, 20, $"<CENTER><BASEFONT COLOR='#88FFFF'>Resources: M:{miningCount} / L:{lumberCount} / F:{fishingCount} / A:{farmingCount}</BASEFONT></CENTER>", false, false);

            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            int y = 75; 

            for (int i = 0; i < maps.Length; i++)
            {
                AddImageTiled(20, y, 440, 38, 9354); 
                AddLabel(35, y + 9, 1152, maps[i].Name);
                
                AddButton(150, y + 7, 4005, 4007, (i * 10) + 1, GumpButtonType.Reply, 0); 
                AddLabel(185, y + 9, 0x481, "DUNGEON");

                AddButton(280, y + 7, 4023, 4025, (i * 10) + 2, GumpButtonType.Reply, 0); 
                AddLabel(315, y + 9, 0x481, "ECOLOGY");
                y += 42;
            }
            
            y += 5;
            AddImageTiled(20, y, 440, 38, 9354); 
            AddButton(35, y + 7, 4011, 4013, 999, GumpButtonType.Reply, 0); 
            AddLabel(75, y + 9, 0x35, "미매칭/에러 노드 리스트 (CHECK LIST)");
            
            y += 45;
            AddImageTiled(20, y, 440, 40, 9354); 
            AddButton(30, y + 8, 4005, 4007, 998, GumpButtonType.Reply, 0); 
            AddLabel(65, y + 10, 0x42, "던전 모니터");

            AddButton(165, y + 8, 4023, 4025, 997, GumpButtonType.Reply, 0); 
            AddLabel(200, y + 10, 0x42, "생태계 모니터");

            AddButton(305, y + 8, 4011, 4013, 996, GumpButtonType.Reply, 0); 
            AddLabel(340, y + 10, 0x58, $"자원/농사 ({totalPools})");
            
            y += 55;
            AddImageTiled(20, y, 440, 85, 9354);
            AddHtml(25, y + 8, 430, 20, "<CENTER><BASEFONT COLOR='#FFFF00'>--- 서버 간 데이터 이식 및 초기화 ---</BASEFONT></CENTER>", false, false);
            
            AddButton(35, y + 35, 4011, 4013, 801, GumpButtonType.Reply, 0); 
            AddLabel(70, y + 37, 1152, "내보내기 (Export)");

            AddButton(185, y + 35, 4005, 4007, 802, GumpButtonType.Reply, 0); 
            AddLabel(220, y + 37, 0x42, "가져오기 (Import)");

            AddButton(335, y + 35, 4020, 4022, 803, GumpButtonType.Reply, 0); 
            AddLabel(370, y + 37, 0x21, "리셋 (Reset)");
        }
        
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;

            if (info.ButtonID == 801) { NewSpawnManager.DoExport(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (info.ButtonID == 802) { NewSpawnManager.DoImport(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (info.ButtonID == 803) { NewSpawnManager.DoReset(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }

            if (info.ButtonID == 999)
            {
                List<DungeonNode> checkList = World.Items.Values.OfType<DungeonNode>()
                    .Where(n => n.Map == sender.Mobile.Map && !NewSpawnManager.IsManaged(n.ZoneId)).ToList();
                sender.Mobile.SendGump(new NodeCheckGump(checkList, 0));
                return;
            }
            
            if (info.ButtonID == 998) { sender.Mobile.SendGump(new ZoneMonitorGump(0, 0)); return; } 
            if (info.ButtonID == 997) { sender.Mobile.SendGump(new ZoneMonitorGump(1, 0)); return; } 
            if (info.ButtonID == 996) { sender.Mobile.SendGump(new ZoneMonitorGump(2, 0)); return; } 

            int mapIdx = (info.ButtonID / 10); int mode = (info.ButtonID % 10) - 1;
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            if (mapIdx < maps.Length) NewSpawnManager.DoMigration(sender.Mobile, maps[mapIdx], mode);
        }
    }

    public class NodeCheckGump : Gump
    {
        private List<DungeonNode> m_List;
        private int m_Page;
        public NodeCheckGump(List<DungeonNode> list, int page) : base(500, 100)
        {
            m_List = list; m_Page = page;
            AddPage(0);
            AddBackground(0, 0, 550, 550, 9270);
            AddHtml(10, 15, 530, 20, $"<CENTER><BASEFONT COLOR='#FF5555'>로직에 등록되지 않은 노드 (총 {list.Count}개)</BASEFONT></CENTER>", false, false);
            int start = page * 10;
            int end = Math.Min(start + 10, list.Count);
            for (int i = start; i < end; i++)
            {
                DungeonNode n = list[i];
                int y = 50 + ((i - start) * 45);
                AddImageTiled(15, y, 520, 40, 9354);
                string desc = $"<BASEFONT COLOR='#FFFFFF'>{n.ZoneId}</BASEFONT>";
                AddHtml(20, y + 10, 430, 20, desc, false, false);
                AddButton(460, y + 8, 4005, 4007, i + 100, GumpButtonType.Reply, 0);
                AddLabel(495, y + 10, 1152, "GO");
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
            else if (info.ButtonID >= 100) 
            {
                int idx = info.ButtonID - 100;
                if (idx < m_List.Count) sender.Mobile.MoveToWorld(m_List[idx].Location, m_List[idx].Map);
                sender.Mobile.SendGump(new NodeCheckGump(m_List, m_Page));
            }
        }
    }

    public class ZoneMonitorGump : Gump
    {
        private int m_Mode; 
        private int m_SubMode;
        private int m_Page;

        public ZoneMonitorGump(int mode, int page) : this(mode, 0, page) { }

        public ZoneMonitorGump(int mode, int subMode, int page) : base(30, 50)
        {
            m_Mode = mode;
            m_SubMode = subMode;
            m_Page = page;
            
            AddPage(0);
            AddBackground(0, 0, 950, 500, 9270);
            AddImageTiled(10, 10, 930, 480, 2624);
            AddAlphaRegion(10, 10, 930, 480);
            
            AddHtml(10, 15, 930, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>MASTER MONITOR</BASEFONT></CENTER>", false, false);

            AddButton(20, 15, 4014, 4016, 3, GumpButtonType.Reply, 0);
            AddLabel(55, 15, 1152, "MAIN");

            AddImageTiled(20, 50, 910, 30, 9354);
            
            AddButton(30, 55, mode == 0 ? 4006 : 4005, 4007, 10, GumpButtonType.Reply, 0);
            AddLabel(65, 55, mode == 0 ? 68 : 1152, "던전 모니터링");

            AddButton(200, 55, mode == 1 ? 4006 : 4005, 4007, 11, GumpButtonType.Reply, 0);
            AddLabel(235, 55, mode == 1 ? 68 : 1152, "생태계 모니터링");

            AddButton(370, 55, mode == 2 ? 4006 : 4005, 4007, 13, GumpButtonType.Reply, 0);
            AddLabel(405, 55, mode == 2 ? 68 : 1152, "자원 생태계 모니터링");

            AddButton(820, 55, 4011, 4012, 12, GumpButtonType.Reply, 0);
            AddLabel(855, 55, 0xFFFFFF, "새로고침");

            int y = 95;

            if (mode == 2)
            {
                AddImageTiled(20, y, 910, 25, 2624);
                string[] subNames = { "전체", "광산", "벌목", "낚시", "농사" };
                for (int i = 0; i < subNames.Length; i++)
                {
                    int btnX = 30 + (i * 100);
                    AddButton(btnX, y + 2, m_SubMode == i ? 4006 : 4005, 4007, 50 + i, GumpButtonType.Reply, 0);
                    AddLabel(btnX + 35, y + 2, m_SubMode == i ? 68 : 1152, subNames[i]);
                }
                y += 35; 
            }

            int start = m_Page * 10;
            int end = start + 10;
            int totalListCount = 0;

            if (mode == 0) 
            {
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false);
                AddHtml(60, y, 360, 20, "<BASEFONT COLOR='#FFFF00'>던전 구역명 (ZoneId)</BASEFONT>", false, false);
                AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>상태</BASEFONT>", false, false);
                AddHtml(560, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>난이도 (현재/최대)</BASEFONT>", false, false);
                AddHtml(710, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>개체수 조절 (현재/최대)</BASEFONT>", false, false);
                y += 25;

                var list = DungeonManager.Zones.Values.ToList();
                totalListCount = list.Count;
                end = Math.Min(end, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var z = list[i];
                    AddImageTiled(20, y - 2, 910, 24, 9354);
                    if (z.Nodes != null && z.Nodes.Count > 0)
                        AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
                    else
                        AddLabel(25, y, 33, "X"); 

                    AddLabel(60, y, 0xFFFFFF, z.ZoneId.Length > 55 ? z.ZoneId.Substring(0, 55) + "..." : z.ZoneId);
                    
                    int phaseColor = 0xFFFFFF;
                    string phaseText = "";
                    if (z.MaxPopulation == 0) { phaseColor = 33; phaseText = "잠금됨 (Locked)"; }
                    else if (z.Phase == DungeonPhase.Active) { phaseColor = 68; phaseText = "사냥 중"; }
                    else if (z.Phase == DungeonPhase.BossSpawned) { phaseColor = 33; phaseText = "보스 등장!"; }
                    else if (z.Phase == DungeonPhase.Cooldown) { phaseColor = 1359; phaseText = "휴식기"; }
                    
                    AddLabel(450, y, phaseColor, phaseText);
                    
                    double diffPercent = z.MaxDifficulty > 0 ? (double)z.CurrentDifficulty / z.MaxDifficulty : 0;
                    int diffColor = diffPercent > 0.5 ? 68 : (diffPercent > 0.2 ? 53 : 33);
                    AddLabel(560, y, diffColor, $"{z.CurrentDifficulty:N0} / {z.MaxDifficulty:N0}");
                    
                    int activeCount = z.GetTotalActiveCount();
                    AddLabel(710, y, 0xFFFFFF, $"{activeCount} /");
                    
                    AddImageTiled(750, y - 1, 55, 22, 2624); 
                    AddAlphaRegion(750, y - 1, 55, 22); 
                    
                    string popText = z.ManualMaxPopulation >= 0 ? z.ManualMaxPopulation.ToString() : z.MaxPopulation.ToString();
                    AddTextEntry(755, y, 45, 20, 53, i - start, popText); 
                    
                    AddButton(810, y + 2, 4023, 4025, 200 + (i - start), GumpButtonType.Reply, 0);
                    AddLabel(845, y, 68, "SET");

                    y += 30;
                }
            }
            else if (mode == 1) 
            {
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false);
                AddHtml(60, y, 360, 20, "<BASEFONT COLOR='#FFFF00'>생태계 구역명 (ZoneId)</BASEFONT>", false, false);
                AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>관리 종(류)</BASEFONT>", false, false);
                AddHtml(560, y, 120, 20, "<BASEFONT COLOR='#FFFF00'>개체 (현재/최대)</BASEFONT>", false, false);
                AddHtml(750, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>평균 활력</BASEFONT>", false, false);
                y += 25;

                var list = EcosystemManager.Zones.Values.ToList();
                totalListCount = list.Count;
                end = Math.Min(end, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var z = list[i];
                    AddImageTiled(20, y - 2, 910, 24, 9354);
                    if (z.Nodes != null && z.Nodes.Count > 0)
                        AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
                    else
                        AddLabel(25, y, 33, "X");

                    AddLabel(60, y, 0xFFFFFF, z.ZoneId.Length > 55 ? z.ZoneId.Substring(0, 55) + "..." : z.ZoneId);
                    
                    int totalSpecies = z.SpeciesInfo.Count;
                    int totalActive = z.SpeciesInfo.Values.Sum(s => s.ActiveAnimals.Count);
                    int totalMax = z.SpeciesInfo.Values.Sum(s => s.MaxPopulation);
                    int avgVitality = totalSpecies > 0 ? z.SpeciesInfo.Values.Sum(s => s.Vitality) / totalSpecies : 0;

                    AddLabel(450, y, 0xFFFFFF, $"{totalSpecies} 가지 종");
                    
                    double popPercent = totalMax > 0 ? (double)totalActive / totalMax : 0;
                    int popColor = popPercent >= 1.0 ? 33 : 0xFFFFFF; 
                    AddLabel(560, y, popColor, $"{totalActive:N0} / {totalMax:N0} 마리");
                    
                    int vitColor = avgVitality > 8000 ? 68 : (avgVitality > 3000 ? 53 : 33);
                    AddLabel(750, y, vitColor, $"{avgVitality / 100.0:F1}%");
                    
                    y += 30;
                }
            }
            else if (mode == 2)
            {
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false);
                AddHtml(70, y, 80, 20, "<BASEFONT COLOR='#FFFF00'>종류 (Type)</BASEFONT>", false, false);
                AddHtml(160, y, 200, 20, "<BASEFONT COLOR='#FFFF00'>대륙 및 구역 (Region)</BASEFONT>", false, false);
                AddHtml(370, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>잔여량 (%)</BASEFONT>", false, false);
                AddHtml(480, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>상태 (쿨타임)</BASEFONT>", false, false);
                y += 25;

                var list = ResourceManager.Pools.Values.ToList();
                if (m_SubMode > 0)
                {
                    ResourceType targetType = ResourceType.Mining;
                    switch (m_SubMode)
                    {
                        case 1: targetType = ResourceType.Mining; break;
                        case 2: targetType = ResourceType.Lumberjacking; break;
                        case 3: targetType = ResourceType.Fishing; break;
                        case 4: targetType = ResourceType.Farming; break;
                    }
                    list = list.Where(p => p.Type == targetType).ToList();
                }

                totalListCount = list.Count;
                end = Math.Min(end, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var pool = list[i];
                    AddImageTiled(20, y - 2, 910, 24, 9354);
                    AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);

                    double percent = pool.MaxCapacity > 0 ? ((double)pool.CurrentCapacity / pool.MaxCapacity) * 100.0 : 0;
                    int color = percent < 50.0 ? 33 : percent > 90.0 ? 68 : 0xFFFFFF;

                    AddLabel(70, y, color, pool.Type.ToString());
                    AddLabel(160, y, color, $"{pool.MapName} - {pool.RegionName}");
                    AddLabel(370, y, color, $"{pool.CurrentCapacity}/{pool.MaxCapacity} ({percent:F0}%)");

                    // ★ [핵심 복구] 자원별 표기 복구 완료
                    if (pool.Type == ResourceType.Farming)
                    {
                        int pending = FarmingSystem.GetPendingCount(pool.RegionName); 
                        
                        // [수정] 뻔뻔한 "양배추" 하드코딩 삭제, 상황에 맞는 텍스트 출력
                        string cropInfo = "다양한 작물";
                        
                        if (pool.RegionName.StartsWith("PrivateFarm")) 
                        {
                            cropInfo = "유저 개인 작물"; // 개인 농장일 경우
                        }
                        else
                        {
                            string n = pool.RegionName.ToLower();
                            if (n.Contains("wheat")) cropInfo = "밀";
                            else if (n.Contains("carrot")) cropInfo = "당근";
                            else if (n.Contains("corn")) cropInfo = "옥수수";
                            else if (n.Contains("cotton")) cropInfo = "목화";
                            else cropInfo = "야생 작물"; // 특정 작물밭이 아닌 일반 밭일 경우
                        }
                        
                        string resStatus = $"자라는 중 [{cropInfo}]";
                        if (pending > 0) resStatus += $" <BASEFONT COLOR='#FF8888'>+ 새끼({pending})</BASEFONT>"; 
                        
                        AddHtml(480, y, 440, 20, $"<BASEFONT COLOR='#42FF42'>{resStatus}</BASEFONT>", false, false); 
                    }
                    else
                    {
                        // [완벽 복구] 단순 '광물' 표기가 아닌, 실제 풀에 들어있는 타입 리스트를 추출
                        string materialName = "알 수 없음";
                        if (pool.AvailableResources != null && pool.AvailableResources.Count > 0)
                        {
                            // 잔여량이 0보다 큰 실제 재료들의 이름(Type.Name)만 쉼표로 연결
                            var activeRes = pool.AvailableResources.Where(k => k.Value > 0).Select(k => k.Key.Name).ToList();
                            
                            if (activeRes.Count > 0)
                            {
                                materialName = string.Join(", ", activeRes);
                                // 이름이 너무 길어 UI를 뚫고 나가는 것 방지
                                if (materialName.Length > 28) materialName = materialName.Substring(0, 25) + "...";
                            }
                            else
                            {
                                materialName = "전체 고갈";
                            }
                        }

                        TimeSpan cooldownLeft = pool.DepletionCooldown - DateTime.Now;
                        if (cooldownLeft.TotalSeconds > 0)
                        {
                            AddHtml(480, y, 440, 20, $"<BASEFONT COLOR='#FF3333'>고갈됨 ({cooldownLeft.TotalMinutes:F1}분 후)</BASEFONT> <BASEFONT COLOR='#AAAAAA'>[{materialName}]</BASEFONT>", false, false);
                        }
                        else
                        {
                            AddHtml(480, y, 440, 20, $"<BASEFONT COLOR='#42FF42'>정상 스폰 중</BASEFONT> <BASEFONT COLOR='#AAAAAA'>[{materialName}]</BASEFONT>", false, false);
                        }
                    }

                    y += 30;
                }
            }

            if (m_Page > 0) AddButton(20, 450, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddLabel(460, 450, 0xFFFFFF, $"{m_Page + 1} / {Math.Max(1, (totalListCount - 1) / 10 + 1)}");
            if (end < totalListCount) AddButton(900, 450, 4005, 4007, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || info.ButtonID == 3) 
            {
                sender.Mobile.SendGump(new NewSpawnGump());
                return;
            }

            if (info.ButtonID >= 50 && info.ButtonID <= 54)
            {
                sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, info.ButtonID - 50, 0));
                return;
            }

            switch (info.ButtonID)
            {
                case 1: sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page - 1)); return;
                case 2: sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page + 1)); return;
                case 10: sender.Mobile.SendGump(new ZoneMonitorGump(0, 0, 0)); return; 
                case 11: sender.Mobile.SendGump(new ZoneMonitorGump(1, 0, 0)); return; 
                case 13: sender.Mobile.SendGump(new ZoneMonitorGump(2, 0, 0)); return; 
                case 12: 
                    foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
                    foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();
                    sender.Mobile.SendMessage(68, "데이터가 갱신되었습니다.");
                    sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page)); 
                    return; 
            }

            int actualIndex = (m_Page * 10) + (info.ButtonID % 100);

            if (info.ButtonID >= 300 && info.ButtonID < 310) 
            {
                if (m_Mode == 0)
                {
                    var list = DungeonManager.Zones.Values.ToList();
                    if (actualIndex < list.Count && list[actualIndex].Nodes.Count > 0)
                        sender.Mobile.MoveToWorld(list[actualIndex].Nodes[0].Location, list[actualIndex].Nodes[0].Map);
                }
                else if (m_Mode == 1) 
                {
                    var list = EcosystemManager.Zones.Values.ToList();
                    if (actualIndex < list.Count && list[actualIndex].Nodes.Count > 0)
                        sender.Mobile.MoveToWorld(list[actualIndex].Nodes[0].Location, list[actualIndex].Nodes[0].Map);
                }
                else if (m_Mode == 2) 
                {
                    var list = ResourceManager.Pools.Values.ToList();
                    if (m_SubMode > 0)
                    {
                        ResourceType targetType = ResourceType.Mining;
                        switch (m_SubMode)
                        {
                            case 1: targetType = ResourceType.Mining; break;
                            case 2: targetType = ResourceType.Lumberjacking; break;
                            case 3: targetType = ResourceType.Fishing; break;
                            case 4: targetType = ResourceType.Farming; break;
                        }
                        list = list.Where(p => p.Type == targetType).ToList();
                    }

                    if (actualIndex < list.Count)
                    {
                        var pool = list[actualIndex];
                        Map map = Map.Parse(pool.MapName);

                        if (pool.Type == ResourceType.Fishing)
                        {
                            string[] parts = pool.RegionName.Split('_');
                            if (parts.Length >= 3)
                            {
                                int size = parts[0] == "Ocean" ? 256 : parts[0] == "Coastal" ? 192 : 128;
                                int x = int.Parse(parts[1]) * size + (size / 2);
                                int y = int.Parse(parts[2]) * size + (size / 2);
                                sender.Mobile.MoveToWorld(new Point3D(x, y, map.GetAverageZ(x, y)), map);
                            }
                        }
                        else
                        {
                            Region r = ResourceManager.GetRegionByName(pool.RegionName, map);
                            if (r != null && r.Area.Length > 0)
                            {
                                var a = r.Area[0];
                                Point3D center = new Point3D(a.Start.X + (a.End.X - a.Start.X) / 2, a.Start.Y + (a.End.Y - a.Start.Y) / 2, map.GetAverageZ(a.Start.X, a.Start.Y));
                                sender.Mobile.MoveToWorld(center, map);
                            }
                        }
                    }
                }
                sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
            else if (info.ButtonID >= 200 && info.ButtonID < 210 && m_Mode == 0) 
            {
                var list = DungeonManager.Zones.Values.ToList();
                if (actualIndex < list.Count)
                {
                    TextRelay relay = info.GetTextEntry(info.ButtonID - 200);
                    if (relay != null && int.TryParse(relay.Text, out int newPop))
                    {
                        list[actualIndex].SetPopulation(newPop);
                        sender.Mobile.SendMessage(68, $"{list[actualIndex].ZoneId} 인구가 {newPop}으로 설정되었습니다.");
                    }
                }
                sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
        }
    }
}