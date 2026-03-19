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

        // ========================================================================
        // [신규] XML Export (내보내기)
        // ========================================================================
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

                // 1. 노드 정보 저장
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

                // 2. 수동 세팅된 인구 정보 저장
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

        // ========================================================================
        // [신규] XML Import (가져오기)
        // ========================================================================
        public static void DoImport(Mobile from)
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "NewRespawn.xml");
            if (!File.Exists(path))
            {
                from.SendMessage(33, "오류: Data/NewRespawn.xml 파일을 찾을 수 없습니다.");
                return;
            }

            // 기존 노드 완전 삭제
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

        // ========================================================================
        // [신규] Reset (기존 XmlSpawner 청소)
        // ========================================================================
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

        // --- 기타 유틸리티 및 명령어 로직 ---
        [Usage("fixallnodes")] public static void OnFixAllNodes(CommandEventArgs e) { /* 생략(Gump로 대체가능) */ }
        [Usage("wipeworldspawns")] public static void OnWipeWorldSpawns(CommandEventArgs e) { /* 생략 */ }
        [Usage("wipeoldspawns")] public static void OnWipeOldSpawns(CommandEventArgs e) { /* 생략 */ }
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

    // ========================================================================
    // 대시보드 UI (버튼 3개 추가)
    // ========================================================================
    public class NewSpawnGump : Gump
    {
        public NewSpawnGump() : base(100, 100)
        {
            AddPage(0); 
            // 배경 크기를 세이브/로드 버튼을 위해 조금 더 늘렸습니다.
            AddBackground(0, 0, 450, 680, 9270); 
            AddAlphaRegion(10, 10, 430, 660);
            AddHtml(10, 15, 430, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>SPAWN MANAGER</BASEFONT></CENTER>", false, false);
            
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            int y = 60;
            for (int i = 0; i < maps.Length; i++)
            {
                AddImageTiled(20, y, 410, 40, 9354); AddLabel(35, y + 10, 1152, maps[i].Name);
                AddButton(150, y + 8, 4005, 4007, (i * 10) + 1, GumpButtonType.Reply, 0); AddLabel(185, y + 10, 0xFFFFFF, "DUNGEON");
                AddButton(280, y + 8, 4023, 4025, (i * 10) + 2, GumpButtonType.Reply, 0); AddLabel(315, y + 10, 0xFFFFFF, "ECOLOGY");
                y += 50;
            }
            
            // 미매칭 에러노드 리스트
            AddImageTiled(20, y, 410, 40, 9354); 
            AddButton(35, y + 8, 4011, 4013, 999, GumpButtonType.Reply, 0); 
            AddLabel(75, y + 10, 0x35, "미매칭/에러 노드 리스트 (CHECK LIST)");
            
            // 상태 모니터창
            y += 50;
            AddImageTiled(20, y, 410, 40, 9354); 
            AddButton(35, y + 8, 4005, 4007, 998, GumpButtonType.Reply, 0); AddLabel(70, y + 10, 68, "던전 상태 모니터");
            AddButton(235, y + 8, 4023, 4025, 997, GumpButtonType.Reply, 0); AddLabel(270, y + 10, 68, "생태계 상태 모니터");

            // [신규] 시스템 이식 및 초기화 섹션
            y += 60;
            AddImageTiled(20, y, 410, 80, 9354);
            AddHtml(25, y + 5, 400, 20, "<CENTER><BASEFONT COLOR='#FFFF00'>--- 서버 간 데이터 이식 및 초기화 ---</BASEFONT></CENTER>", false, false);
            
            AddButton(35, y + 30, 4011, 4013, 801, GumpButtonType.Reply, 0); AddLabel(70, y + 32, 1152, "내보내기 (Export)");
            AddButton(180, y + 30, 4005, 4007, 802, GumpButtonType.Reply, 0); AddLabel(215, y + 32, 68, "가져오기 (Import)");
            AddButton(305, y + 30, 4020, 4022, 803, GumpButtonType.Reply, 0); AddLabel(340, y + 32, 33, "리셋 (Reset)");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;

            // [신규] 데이터 관리 버튼 응답
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

            int mapIdx = (info.ButtonID / 10); int mode = (info.ButtonID % 10) - 1;
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            if (mapIdx < maps.Length) NewSpawnManager.DoMigration(sender.Mobile, maps[mapIdx], mode);
        }
    }

    // ... NodeCheckGump 및 ZoneMonitorGump 로직은 변경 없이 이전과 동일하게 사용하시면 됩니다 ...
    // (여기서 생략했지만 실제 적용하실 땐 기존 코드 밑에 그대로 두시면 됩니다.)
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
        private int m_Page;

        public ZoneMonitorGump(int mode, int page) : base(30, 50)
        {
            m_Mode = mode;
            m_Page = page;
            
            AddPage(0);
            AddBackground(0, 0, 950, 500, 9270);
            AddImageTiled(10, 10, 930, 480, 2624);
            AddAlphaRegion(10, 10, 930, 480);
            
            AddHtml(10, 15, 930, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>RESPAWN SYSTEM MONITOR</BASEFONT></CENTER>", false, false);

            AddButton(20, 15, 4014, 4016, 3, GumpButtonType.Reply, 0);
            AddLabel(55, 15, 1152, "MAIN");

            AddImageTiled(20, 50, 910, 30, 9354);
            
            AddButton(30, 55, mode == 0 ? 4006 : 4005, 4007, 10, GumpButtonType.Reply, 0);
            AddLabel(65, 55, mode == 0 ? 68 : 1152, "던전 모니터링");

            AddButton(200, 55, mode == 1 ? 4006 : 4005, 4007, 11, GumpButtonType.Reply, 0);
            AddLabel(235, 55, mode == 1 ? 68 : 1152, "생태계 모니터링");

            AddButton(820, 55, 4011, 4012, 12, GumpButtonType.Reply, 0);
            AddLabel(855, 55, 0xFFFFFF, "새로고침");

            int y = 95;
            int start = page * 10;
            int end = start + 10;

            if (mode == 0) 
            {
                // [수정] 이동 버튼 공간(X=25)을 만들고, 구역명을 우측(X=60)으로 밀었습니다.
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false);
                AddHtml(60, y, 360, 20, "<BASEFONT COLOR='#FFFF00'>던전 구역명 (ZoneId)</BASEFONT>", false, false);
                AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>상태</BASEFONT>", false, false);
                AddHtml(560, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>난이도 (현재/최대)</BASEFONT>", false, false);
                AddHtml(710, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>개체수 조절 (현재/최대)</BASEFONT>", false, false);
                y += 25;

                var list = DungeonManager.Zones.Values.ToList();
                end = Math.Min(end, list.Count);

                for (int i = start; i < end; i++)
                {
                    var z = list[i];
                    AddImageTiled(20, y - 2, 910, 24, 9354);

                    // [신규] 노드가 하나라도 있으면 GO 버튼 생성, 없으면 빨간 X 표시
                    if (z.Nodes != null && z.Nodes.Count > 0)
                        AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
                    else
                        AddLabel(25, y, 33, "X"); // 33번 빨간색

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

                if (page > 0) AddButton(20, 450, 4014, 4016, 1, GumpButtonType.Reply, 0);
                AddLabel(460, 450, 0xFFFFFF, $"{page + 1} / {Math.Max(1, (list.Count - 1) / 10 + 1)}");
                if (end < list.Count) AddButton(900, 450, 4005, 4007, 2, GumpButtonType.Reply, 0);
            }
            else 
            {
                // [수정] 이동 버튼 공간 생성
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false);
                AddHtml(60, y, 360, 20, "<BASEFONT COLOR='#FFFF00'>생태계 구역명 (ZoneId)</BASEFONT>", false, false);
                AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>관리 종(류)</BASEFONT>", false, false);
                AddHtml(560, y, 120, 20, "<BASEFONT COLOR='#FFFF00'>개체 (현재/최대)</BASEFONT>", false, false);
                AddHtml(750, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>평균 활력</BASEFONT>", false, false);
                y += 25;

                var list = EcosystemManager.Zones.Values.ToList();
                end = Math.Min(end, list.Count);

                for (int i = start; i < end; i++)
                {
                    var z = list[i];
                    AddImageTiled(20, y - 2, 910, 24, 9354);

                    // [신규] GO 버튼 또는 X 표시
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

                if (page > 0) AddButton(20, 450, 4014, 4016, 1, GumpButtonType.Reply, 0);
                AddLabel(460, 450, 0xFFFFFF, $"{page + 1} / {Math.Max(1, (list.Count - 1) / 10 + 1)}");
                if (end < list.Count) AddButton(900, 450, 4005, 4007, 2, GumpButtonType.Reply, 0);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || info.ButtonID == 3) 
            {
                sender.Mobile.SendGump(new NewSpawnGump());
                return;
            }

            switch (info.ButtonID)
            {
                case 1: sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_Page - 1)); return;
                case 2: sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_Page + 1)); return;
                case 10: sender.Mobile.SendGump(new ZoneMonitorGump(0, 0)); return; 
                case 11: sender.Mobile.SendGump(new ZoneMonitorGump(1, 0)); return; 
                case 12: 
                    foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
                    foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();
                    sender.Mobile.SendMessage(68, "노드 데이터 캐시가 갱신되었습니다.");
                    sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_Page)); 
                    return; 
            }

            // 인구(Population) 수동 세팅 로직 (200 ~ 209)
            if (info.ButtonID >= 200 && info.ButtonID < 210)
            {
                int actualIndex = (m_Page * 10) + (info.ButtonID - 200);
                var list = DungeonManager.Zones.Values.ToList();
                if (m_Mode == 0 && actualIndex < list.Count)
                {
                    TextRelay relay = info.GetTextEntry(info.ButtonID - 200);
                    if (relay != null && int.TryParse(relay.Text, out int newPop))
                    {
                        if (newPop >= 0)
                        {
                            list[actualIndex].SetPopulation(newPop); list[actualIndex].CacheNodes();
                            if (newPop == 0) { list[actualIndex].ClearAllSpawns(); sender.Mobile.SendMessage(33, $"{list[actualIndex].ZoneId} 스폰이 잠금 처리되었습니다."); }
                            else sender.Mobile.SendMessage(68, $"{list[actualIndex].ZoneId} 몬스터 최대 숫자가 {newPop}마리로 영구 고정되었습니다.");
                        }
                        else if (newPop == -1)
                        {
                            list[actualIndex].SetPopulation(-1); list[actualIndex].CacheNodes();
                            sender.Mobile.SendMessage(1152, $"{list[actualIndex].ZoneId} 구역이 자동 면적 계산 모드로 복귀했습니다.");
                        }
                    }
                }
                sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_Page));
                return;
            }

            // [신규] GO(이동) 버튼 로직 (300 ~ 309)
            if (info.ButtonID >= 300 && info.ButtonID < 310)
            {
                int actualIndex = (m_Page * 10) + (info.ButtonID - 300);

                if (m_Mode == 0) // 던전
                {
                    var list = DungeonManager.Zones.Values.ToList();
                    if (actualIndex < list.Count)
                    {
                        var zone = list[actualIndex];
                        var targetNode = zone.Nodes.FirstOrDefault();
                        if (targetNode != null && zone.Facet != null)
                            sender.Mobile.MoveToWorld(targetNode.Location, zone.Facet);
                    }
                }
                else // 생태계
                {
                    var list = EcosystemManager.Zones.Values.ToList();
                    if (actualIndex < list.Count)
                    {
                        var zone = list[actualIndex];
                        var targetNode = zone.Nodes.FirstOrDefault();
                        if (targetNode != null && zone.Facet != null)
                            sender.Mobile.MoveToWorld(targetNode.Location, zone.Facet);
                    }
                }

                // 이동 후 상태창을 다시 띄워줌
                sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, m_Page));
            }
        }
    }
}