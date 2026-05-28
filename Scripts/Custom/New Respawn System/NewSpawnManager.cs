using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // 🌟 트라멜을 제외한 모든 대륙 기본 잠금(false) 처리
            { Map.Felucca, false }, { Map.Trammel, true }, { Map.Ilshenar, false },
            { Map.Malas, false }, { Map.Tokuno, false }, { Map.TerMur, false }
        };

        public static Dictionary<string, int> TeleportIndex = new Dictionary<string, int>();

        private static string MapSavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "ActiveMaps.bin");

        public static bool IsMapActive(Map map) 
        { 
            if (map == null || map == Map.Internal) return false; 
            return ActiveMaps.TryGetValue(map, out bool isActive) && isActive; 
        }

        public static void Configure()
        {
            EventSink.WorldSave += OnSaveActiveMaps;
            EventSink.WorldLoad += OnLoadActiveMaps;

            CommandSystem.Register("ns", AccessLevel.Administrator, new CommandEventHandler(OnNewSpawn));
            CommandSystem.Register("zonemonitor", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("zm", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("wipeworldspawns", AccessLevel.Administrator, new CommandEventHandler(OnWipeWorldSpawns));
            CommandSystem.Register("wipewildcrops", AccessLevel.Administrator, new CommandEventHandler(OnWipeWildCrops));
            
            Timer.DelayCall(TimeSpan.FromSeconds(5.0), () =>
            {
                Console.WriteLine("Ecosystem & Dungeon Monitor Data Rebuilding...");
                EcosystemManager.RebuildZones(); 
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
                writer.Close(); 
            }
        }

        private static void OnLoadActiveMaps()
        {
            if (!File.Exists(MapSavePath)) return;

            try
            {
                using (FileStream bin = new FileStream(MapSavePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (bin.Length == 0) return;

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
                Console.WriteLine($"[NewSpawnManager] ActiveMaps.bin 로드 실패: {ex.Message}. 기본 설정을 유지합니다.");
            }
        }

        [Usage("WipeOrphans")]
        [Description("노드가 삭제되어 길을 잃은 고아 몬스터들을 일괄 정리합니다.")]
        public static void OnWipeOrphans(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            int count = 0;
            var toDelete = new System.Collections.Generic.List<Server.Mobiles.BaseCreature>();

            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m is Server.Mobiles.BaseCreature bc)
                {
                    if (bc.Controlled || bc.Summoned || bc.IsInvulnerable || bc.FightMode == FightMode.None) continue;
                    if (bc is Server.Mobiles.BaseVendor || bc is Server.Mobiles.BaseGuard || bc is Server.Mobiles.BaseHealer) continue;

                    bool isOrphan = false;

                    if (bc.Spawner == null) 
                        isOrphan = true;
                    else if (bc.Spawner is Item spawnerItem && spawnerItem.Deleted) 
                        isOrphan = true;
                    else if (bc.Spawner.GetType().Name.Contains("XmlSpawner")) 
                        isOrphan = true;

                    if (isOrphan) toDelete.Add(bc);
                }
            }

            foreach (var mob in toDelete) { mob.Delete(); count++; }
            from.SendMessage(68, $"[청소 완료] 고아 몬스터 {count}마리가 맵에서 삭제되었습니다.");
        }

        // ========================================================================
        // 유틸리티 함수들
        // ========================================================================
        public static string GetDisplayName(RegionCode code)
        {
            int val = (int)code;
            if (val == 0) return "Unknown Area";
            
            if (val >= 1000000)
            {
                int mapId = (val / 1000000) - 1;
                string mapName = mapId switch { 0 => "Felucca", 1 => "Trammel", 2 => "Ilshenar", 3 => "Malas", 4 => "Tokuno", 5 => "TerMur", _ => "Unknown" };
                int rem = val % 1000000;
                return $"[{mapName}] 야생 필드 ({rem / 1000 * 128}, {rem % 1000 * 128})"; 
            }
            
            if ((val % 100000) >= 99000)
            {
                int mapId = (val / 100000) - 1;
                string mapName = mapId switch { 0 => "Felucca", 1 => "Trammel", 2 => "Ilshenar", 3 => "Malas", 4 => "Tokuno", 5 => "TerMur", _ => "Unknown" };
                return $"[{mapName}] 미개척 야생 ({(EcoAreaType)((val % 100) / 10)}/{(EcoClimateType)(val % 10)})";
            }

            string full = code.ToString(); 
            string[] parts = full.Split('_');
            if (parts.Length >= 3)
            {
                string result = string.Join(" ", parts.Skip(2)).Replace("Level", "Level ");
                return $"[{parts[0]}] {result}";
            }
            return full.Replace("_", " ");
        }

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

        [Usage("wipeworldspawns")] public static void OnWipeWorldSpawns(CommandEventArgs e) => DoReset(e.Mobile);
        [Usage("ns")] public static void OnNewSpawn(CommandEventArgs e) => e.Mobile.SendGump(new NewSpawnGump());
        [Usage("zonemonitor")] public static void OnMonitor(CommandEventArgs e) => e.Mobile.SendGump(new ZoneMonitorGump(0, 0));

        public static void DoResetDungeonNodes(Mobile from)
        {
            int nodeCount = 0;
            // 🌟 폐기된 DungeonNode 아이템들을 맵에서 물리적으로 완전히 뽑아냅니다.
            foreach (var item in World.Items.Values.OfType<DungeonNode>().ToList()) 
            { 
                item.Delete(); 
                nodeCount++; 
            }
            foreach (var z in DungeonManager.ZoneList) { z.ClearAllSpawns(); }
            from.SendMessage(33, $"[던전 정리 완료] 폐기된 노드 {nodeCount}개 및 소환 몹 청소 완료.");
        }

        public static void DoResetEcoNodes(Mobile from)
        {
            int nodeCount = 0;
            foreach (var item in World.Items.Values.OfType<EcoNode>().ToList()) { item.Delete(); nodeCount++; }
            foreach (var z in EcosystemManager.ZoneList) { z.ClearAllSpawns(); z.CacheNodes(); }
            from.SendMessage(33, $"[생태계 리셋 완료] 생태 노드 {nodeCount}개 삭제 완료.");
        }

        public static void DoResetVendorNodes(Mobile from) 
        { 
            int nodeCount = 0;
            foreach (var item in World.Items.Values.OfType<VendorNode>().ToList()) { item.Delete(); nodeCount++; } 
            from.SendMessage(33, $"[상인 리셋 완료] 상인 노드 {nodeCount}개 삭제 완료."); 
        }

        public static void DoReset(Mobile from)
        {
            int xmlCount = 0;
            Type xmlSpawnerType = ScriptCompiler.FindTypeByName("XmlSpawner");
            if (xmlSpawnerType != null)
            {
                foreach (Item item in World.Items.Values.Where(i => i.GetType() == xmlSpawnerType).ToList()) 
                {
                    item.Delete();
                    xmlCount++;
                }
            }

            int dunNodeCount = 0;
            // 🌟 wipeworldspawns 명령어 입력 시, 맵에 남은 모든 던전 노드도 함께 삭제시킵니다.
            foreach (var item in World.Items.Values.OfType<DungeonNode>().ToList()) 
            { 
                item.Delete(); 
                dunNodeCount++; 
            }

            from.SendMessage(68, $"[월드 청소 완료] 구형 스포너 {xmlCount}개, 폐기된 던전 노드 {dunNodeCount}개 삭제됨.");
        }
		// ========================================================================
        // 🌟 외부 시스템(Tanning, Mining, RegionDataExtractor 등) 연동용 복구 함수
        // ========================================================================
        public static string GetGoGumpZoneName(Point3D loc, Map map)
        {
            RegionCode code = RegionSaver.GetRegionCode(map, loc.X, loc.Y, loc.Z);
            return code == RegionCode.None ? "Unknown" : code.ToString();
        }

        public static Point3D? FindLocationByRegionCode(RegionCode code, Map map)
        {
            if (code == RegionCode.None) return null;
            
            // 던전 구역일 경우 (AreaBounds의 중앙 좌표를 가짜 노드 위치로 반환)
            if (DungeonManager.Zones.TryGetValue(code, out var dz) && dz.AreaBounds.Count > 0)
            {
                Rectangle2D rect = dz.AreaBounds[0];
                int z = map.GetAverageZ(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                return new Point3D(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, z);
            }
            
            // 생태계 구역일 경우
            if (EcosystemManager.Zones.TryGetValue(code, out var ez) && ez.Nodes.Count > 0)
                return ez.Nodes[0].Location;
                
            return null;
        }

        public static bool IsManaged(RegionCode code) 
        { 
            return code != RegionCode.None && (DungeonManager.Zones.ContainsKey(code) || EcosystemManager.Zones.ContainsKey(code)); 
        }
    }

    #region [Gumps]
    public class NewSpawnGump : Gump
    {
        public NewSpawnGump() : base(100, 100)
        {
            AddPage(0); 
            // 🌟 배경을 어두운 톤(9200)으로 교체
            AddBackground(0, 0, 400, 350, 9200); 
            AddAlphaRegion(10, 10, 380, 330);
            AddHtml(10, 15, 380, 25, "<CENTER><BASEFONT COLOR=#FFCC00 SIZE=6>SYSTEM MONITORING</BASEFONT></CENTER>", false, false);

            int y = 70;
            
            AddImageTiled(40, y, 320, 50, 9354);
            AddButton(50, y + 10, 4005, 4007, 1, GumpButtonType.Reply, 0); 
            AddHtml(90, y + 15, 200, 20, "<BASEFONT COLOR=#FFFFFF SIZE=5>던전 모니터</BASEFONT>", false, false);
            
            y += 60;
            AddImageTiled(40, y, 320, 50, 9354);
            AddButton(50, y + 10, 4023, 4025, 2, GumpButtonType.Reply, 0); 
            AddHtml(90, y + 15, 200, 20, "<BASEFONT COLOR=#FFFFFF SIZE=5>생태계 모니터</BASEFONT>", false, false);

            y += 60;
            AddImageTiled(40, y, 320, 50, 9354);
            AddButton(50, y + 10, 4011, 4013, 3, GumpButtonType.Reply, 0); 
            AddHtml(90, y + 15, 200, 20, "<BASEFONT COLOR=#FFFFFF SIZE=5>자원/농사 관리</BASEFONT>", false, false);

            y += 60;
            AddImageTiled(40, y, 320, 50, 9354);
            AddButton(50, y + 10, 4020, 4022, 4, GumpButtonType.Reply, 0); 
            AddHtml(90, y + 15, 200, 20, "<BASEFONT COLOR=#FFFFFF SIZE=5>도시/경제 관리</BASEFONT>", false, false);
        }
        
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            // 🌟 새 창을 열기 전 현재 창을 확실히 닫음
            from.CloseGump(typeof(NewSpawnGump));

            if (info.ButtonID == 1) from.SendGump(new ZoneMonitorGump(0, 0));
            else if (info.ButtonID == 2) from.SendGump(new ZoneMonitorGump(1, 0));
            else if (info.ButtonID == 3) from.SendGump(new ZoneMonitorGump(2, 0));
            else if (info.ButtonID == 4) 
            { 
                GlobalEconomyMonitor.GenerateUserReport(); 
                from.SendGump(new EconomyAdminGump(from)); 
            }
        }
    }

    // ========================================================================
    // 🌍 ZoneMonitorGump: 마스터 모니터 (던전 그룹핑 UI 이름 표기 버그 수정)
    // ========================================================================
    public class ZoneMonitorGump : Gump
    {
        private int m_Mode, m_SubMode, m_Page, m_MapFilter;

        public ZoneMonitorGump(int mode, int page) : this(mode, 0, page, 0) { } 

        public ZoneMonitorGump(int mode, int subMode, int page, int mapFilter) : base(30, 50)
        {
            m_Mode = mode; m_SubMode = subMode; m_Page = page; m_MapFilter = mapFilter;
            AddPage(0); 
            AddBackground(0, 0, 950, 550, 9200); 
            AddAlphaRegion(10, 10, 930, 530);
            AddHtml(10, 15, 930, 25, "<CENTER><BASEFONT COLOR=#FFCC00 SIZE=6>MASTER MONITOR</BASEFONT></CENTER>", false, false);
            
            AddImageTiled(20, 50, 910, 30, 9354);
            AddButton(30, 55, mode == 0 ? 4006 : 4005, 4007, 10, GumpButtonType.Reply, 0); AddLabel(65, 55, mode == 0 ? 68 : 0x481, "던전 모니터링");
            AddButton(200, 55, mode == 1 ? 4006 : 4005, 4007, 11, GumpButtonType.Reply, 0); AddLabel(235, 55, mode == 1 ? 68 : 0x481, "생태계 모니터링");
            AddButton(370, 55, mode == 2 ? 4006 : 4005, 4007, 13, GumpButtonType.Reply, 0); AddLabel(405, 55, mode == 2 ? 68 : 0x481, "자원 생태계 모니터링");
            
            AddButton(730, 55, 4014, 4016, 999, GumpButtonType.Reply, 0); AddLabel(765, 55, 0xFFFFFF, "메인메뉴");
            AddButton(840, 55, 4011, 4012, 12, GumpButtonType.Reply, 0); AddLabel(875, 55, 0xFFFFFF, "새로고침");

            int y = 85; AddImageTiled(20, y, 910, 30, 2624);
            string[] mapNames = { "전체", "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur" };
            Map[] mapRefs = { null, Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            Map currentFilterMap = mapRefs[m_MapFilter];

            for (int i = 0; i < mapNames.Length; i++) 
            { 
                AddButton(30 + (i * 90), y + 5, m_MapFilter == i ? 4006 : 4005, 4007, 70 + i, GumpButtonType.Reply, 0); 
                AddLabel(65 + (i * 90), y + 5, m_MapFilter == i ? 68 : 0x481, mapNames[i]); 
            }

            if (m_MapFilter > 0 && currentFilterMap != null)
            {
                bool isActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(currentFilterMap, true);
                AddButton(690, y + 5, isActive ? 2361 : 2360, isActive ? 2361 : 2360, 800, GumpButtonType.Reply, 0); 
                AddLabel(710, y + 3, isActive ? 68 : 33, isActive ? $"[{mapNames[m_MapFilter]}] 대륙 스폰 ON" : $"[{mapNames[m_MapFilter]}] 대륙 스폰 OFF");
            }
            else
            {
                AddLabel(690, y + 5, 0x35, "◀ 대륙별 스폰 스위치는 대륙 선택시 표시됨");
            }
            y += 35;

            if (mode == 2)
            {
                AddImageTiled(20, y, 910, 25, 2624); string[] subNames = { "전체 자원", "광산", "벌목", "낚시", "농사" };
                for (int i = 0; i < subNames.Length; i++) { AddButton(30 + (i * 100), y + 2, m_SubMode == i ? 4006 : 4005, 4007, 50 + i, GumpButtonType.Reply, 0); AddLabel(65 + (i * 100), y + 2, m_SubMode == i ? 68 : 0x481, subNames[i]); }
                y += 30; 
            }

            int start = m_Page * 10, totalListCount = 0;

            // =========================================================
            // 던전 모드 (트리 구조 및 신규 구역 생성 기능)
            // =========================================================
            if (mode == 0) 
            {
                AddHtml(25, y, 80, 20, "<BASEFONT COLOR='#FFFF00'>이동(GO)</BASEFONT>", false, false); 
                AddHtml(120, y, 200, 20, "<BASEFONT COLOR='#FFFF00'>던전 구역명 (그룹/세부)</BASEFONT>", false, false); 
                AddHtml(350, y, 120, 20, "<BASEFONT COLOR='#FFFF00'>진행 상태</BASEFONT>", false, false); 
                AddHtml(480, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>스폰/최대</BASEFONT>", false, false); 
                AddHtml(600, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>열기 (Heat)</BASEFONT>", false, false); 
                AddHtml(820, y, 80, 20, "<BASEFONT COLOR='#FFFF00'>설정</BASEFONT>", false, false); 

                AddButton(865, y, 4011, 4013, 888, GumpButtonType.Reply, 0); 
                AddLabel(900, y, 0x42, "신규");
                y += 25;
                
                // 🌟 화면 표시용 이름으로 미리 그룹핑하여 정렬되도록 수정
                var list = DungeonManager.ZoneList
                    .Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID))
                    .OrderBy(z => (string.IsNullOrEmpty(z.GroupName) || z.GroupName == "Uncategorized" || z.GroupName == "기본 그룹") ? "미분류 던전" : z.GroupName)
                    .ThenBy(z => (string.IsNullOrEmpty(z.SubZoneName) || z.SubZoneName == "Main" || z.SubZoneName == "메인 구역") ? NewSpawnManager.GetDisplayName(z.RCode) : z.SubZoneName)
                    .ToList();

                totalListCount = list.Count; 
                int end = Math.Min(start + 10, totalListCount);
                string lastGroup = null;

                for (int i = start; i < end; i++)
                {
                    var z = list[i]; 
                    
                    // 🌟 1. 그룹명 필터링 (기본값이면 '미분류 던전'으로 표시)
                    string currentGroup = (string.IsNullOrEmpty(z.GroupName) || z.GroupName == "Uncategorized" || z.GroupName == "기본 그룹") ? "미분류 던전" : z.GroupName;

                    if (currentGroup != lastGroup)
                    {
                        AddImageTiled(20, y, 910, 20, 2624);
                        AddLabel(30, y, 68, $"[그룹명: {currentGroup}]");
                        y += 20;
                        lastGroup = currentGroup;
                    }

                    AddImageTiled(20, y - 2, 910, 24, 9354);
                    bool isMapActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(z.Facet, true);

                    if (z.AreaBounds.Count > 0) { AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 0x481, "GO"); }
                    
                    // 🌟 2. 구역명 필터링 (기본값이면 원래의 지역 코드 이름 표시)
                    string displayName = (string.IsNullOrEmpty(z.SubZoneName) || z.SubZoneName == "Main" || z.SubZoneName == "메인 구역") ? NewSpawnManager.GetDisplayName(z.RCode) : z.SubZoneName;
                    AddLabel(120, y, 0xFFFFFF, " └ " + displayName);
                    
                    if (!isMapActive || !z.IsActive) 
                        AddLabel(350, y, 33, "비활성화 (OFF)");
                    else
                        AddLabel(350, y, z.Phase == DungeonPhase.Active ? 68 : (z.Phase == DungeonPhase.BossSpawned ? 33 : 1359), z.Phase == DungeonPhase.Active ? "사냥 중" : (z.Phase == DungeonPhase.BossSpawned ? "보스 등장!" : "휴식기"));
                    
                    AddLabel(480, y, 0xFFFFFF, $"{z.GetTotalActiveCount()} / {z.MaxPopulation}");

                    double heatPct = z.TargetHeat > 0 ? (double)z.CurrentHeat / z.TargetHeat : 0;
                    AddLabel(600, y, heatPct >= 0.8 ? 33 : (heatPct >= 0.4 ? 1258 : 1152), $"{z.CurrentHeat:N0} / {z.TargetHeat:N0} ({heatPct:P0})");
                    
                    AddButton(820, y + 2, 4023, 4025, 200 + (i - start), GumpButtonType.Reply, 0); AddLabel(855, y, 68, "SET");
                    y += 26; 
                }
            }
            // =========================================================
            // 생태계 모드 
            // =========================================================
            else if (mode == 1) 
            {
                AddHtml(25, y, 80, 20, "<BASEFONT COLOR='#FFFF00'>이동(GO)</BASEFONT>", false, false); 
                AddHtml(120, y, 200, 20, "<BASEFONT COLOR='#FFFF00'>생태계 구역명</BASEFONT>", false, false); 
                AddHtml(350, y, 120, 20, "<BASEFONT COLOR='#FFFF00'>시스템 상태</BASEFONT>", false, false); 
                AddHtml(500, y, 120, 20, "<BASEFONT COLOR='#FFFF00'>노드 개수</BASEFONT>", false, false); 
                AddHtml(680, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>관리 옵션</BASEFONT>", false, false); y += 25;
                
                var list = EcosystemManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID)).ToList();
                totalListCount = list.Count; int end = Math.Min(start + 10, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var z = list[i]; AddImageTiled(20, y - 2, 910, 24, 9354);
                    bool isMapActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(z.Facet, true);

                    if (z.Nodes.Count > 0) { AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 0x481, "GO"); }
                    else { AddButton(25, y + 2, 4011, 4013, 400 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 33, "생성"); }

                    AddLabel(120, y, 0xFFFFFF, NewSpawnManager.GetDisplayName(z.RCode)); 
                    
                    if (!isMapActive) AddLabel(350, y, 33, "대륙 스폰 OFF");
                    else AddLabel(350, y, 68, "자율 생존 ON"); 
                        
                    AddLabel(500, y, 0xFFFFFF, $"{z.Nodes.Count} 노드"); 
                    if (z.Nodes.Count > 1)
                    {
                        AddButton(680, y + 2, 4017, 4018, 600 + (i - start), GumpButtonType.Reply, 0); AddLabel(715, y, 0x35, "중복 정리");
                    }
                    y += 28; 
                }
            }
            // =========================================================
            // 자원 생태계 모드 
            // =========================================================
            else if (mode == 2) 
            {
                AddHtml(25, y, 80, 20, "<BASEFONT COLOR=#FFFF00>이동(GO)</BASEFONT>", false, false); 
                AddHtml(120, y, 100, 20, "<BASEFONT COLOR=#FFFF00>자원 종류</BASEFONT>", false, false); 
                AddHtml(250, y, 200, 20, "<BASEFONT COLOR=#FFFF00>구역명</BASEFONT>", false, false); 
                AddHtml(480, y, 150, 20, "<BASEFONT COLOR=#FFFF00>잔여량</BASEFONT>", false, false); 
                AddHtml(650, y, 200, 20, "<BASEFONT COLOR=#FFFF00>상태 및 재료</BASEFONT>", false, false); y += 25;
                
                var rawList = ResourceManager.PoolList.Where(p => currentFilterMap == null || (p.Facet != null && p.Facet.MapID == currentFilterMap.MapID)).ToList();
                if (m_SubMode > 0) rawList = rawList.Where(p => p.Type == (ResourceType)(m_SubMode == 1 ? 0 : m_SubMode == 2 ? 1 : m_SubMode == 3 ? 2 : 4)).ToList();
                rawList = rawList.Where(p => p.LocType != LocationType.Farm_Remote && !p.IsPrivate).ToList();

                var tempGrouped = rawList.Where(p => p.RCode != RegionCode.None).GroupBy(p => new { p.Facet, p.RCode, p.Type }).ToList();
                var finalGrouped = new List<object>();

                foreach (var g in tempGrouped)
                {
                    var first = g.First();
                    if (first.Type == ResourceType.Farming) 
                    {
                        int rCount = World.Items.Values.OfType<EcoNode>().Count(n => n.Map == first.Facet && n.RCode == first.RCode && string.Equals(n.Name, "FarmNode", StringComparison.OrdinalIgnoreCase));
                        if (rCount == 0) continue; 
                    }
                    finalGrouped.Add(g);
                }

                var wild = rawList.Where(p => p.RCode == RegionCode.None).Select(p => (object)p).ToList();
                var targetList = finalGrouped.Concat(wild).ToList();
                
                totalListCount = targetList.Count; 
                int end = Math.Min(start + 10, totalListCount);

                for (int i = start; i < end; i++)
                {
                    object entry = targetList[i]; AddImageTiled(20, y - 2, 910, 24, 9354); AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0); AddLabel(55, y, 0x481, "GO");
                    
                    if (entry is IGrouping<object, ResourcePool> group)
                    {
                        var pool = group.First();
                        double percent = pool.MaxCapacity > 0 ? ((double)group.Sum(p => p.CurrentCapacity) / group.Sum(p => p.MaxCapacity)) * 100.0 : 0;
                        int color = percent < 50.0 ? 33 : (percent > 90.0 ? 68 : 0xFFFFFF);

                        AddLabel(120, y, color, pool.Type.ToString());
                        string pName = pool.RCode == RegionCode.None ? "해양 구역" : NewSpawnManager.GetDisplayName(pool.RCode);
                        AddLabel(250, y, color, pName.Length > 25 ? pName.Substring(0, 25) : pName);
                        AddLabel(480, y, color, string.Format("{0}/{1} ({2:F0}%)", group.Sum(p => p.CurrentCapacity), group.Sum(p => p.MaxCapacity), percent));
                        
                        TimeSpan cd = pool.DepletionCooldown - DateTime.Now;
                        bool isMapActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(pool.Facet, true);
                        if (!isMapActive) AddHtml(650, y, 350, 20, "<BASEFONT COLOR=#777777>대륙 정지 (OFF)</BASEFONT>", false, false);
                        else if (cd.TotalSeconds > 0) AddHtml(650, y, 350, 20, string.Format("<BASEFONT COLOR=#FF3333>고갈 ({0:F1}분)</BASEFONT>", cd.TotalMinutes), false, false);
                        else AddHtml(650, y, 350, 20, "<BASEFONT COLOR=#42FF42>정상 스폰 중</BASEFONT>", false, false);
                    }
                    else if (entry is ResourcePool pool)
                    {
                        double percent = pool.MaxCapacity > 0 ? ((double)pool.CurrentCapacity / pool.MaxCapacity) * 100.0 : 0;
                        int color = percent < 50.0 ? 33 : (percent > 90.0 ? 68 : 0xFFFFFF);

                        AddLabel(120, y, color, pool.Type.ToString());
                        string pName = pool.Type == ResourceType.Fishing ? string.Format("Ocean {0}", wild.Where(x => ((ResourcePool)x).Type == ResourceType.Fishing).ToList().IndexOf(pool) + 1) : string.Format("Wild {0}", pool.Type);
                        AddLabel(250, y, color, pName.Length > 25 ? pName.Substring(0, 25) : pName);
                        AddLabel(480, y, color, string.Format("{0}/{1} ({2:F0}%)", pool.CurrentCapacity, pool.MaxCapacity, percent));
                        
                        TimeSpan cd = pool.DepletionCooldown - DateTime.Now;
                        bool isMapActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(pool.Facet, true);
                        if (!isMapActive) AddHtml(650, y, 350, 20, "<BASEFONT COLOR=#777777>대륙 정지 (OFF)</BASEFONT>", false, false);
                        else if (cd.TotalSeconds > 0) AddHtml(650, y, 350, 20, string.Format("<BASEFONT COLOR=#FF3333>고갈 ({0:F1}분)</BASEFONT>", cd.TotalMinutes), false, false);
                        else AddHtml(650, y, 350, 20, "<BASEFONT COLOR=#42FF42>정상 스폰 중</BASEFONT>", false, false);
                    }
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
            from.CloseGump(typeof(ZoneMonitorGump));
            
            if (info.ButtonID == 0) return;
            if (info.ButtonID == 999) { from.SendGump(new NewSpawnGump()); return; }
            
            // 신규 던전 구역 생성
            if (info.ButtonID == 888)
            {
                Map targetMap = m_MapFilter > 0 ? (new Map[] { null, Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur })[m_MapFilter] : Map.Trammel;

                int newCode = 900000;
                while (DungeonManager.Zones.ContainsKey((RegionCode)newCode)) newCode++;

                DungeonZone newZone = new DungeonZone((RegionCode)newCode, targetMap, 100000, null, TimeSpan.FromMinutes(60));
                newZone.GroupName = ""; // 🌟 새 구역 생성시 빈 문자열 세팅
                newZone.SubZoneName = "";
                newZone.IsActive = false; 

                DungeonManager.RegisterZone(newZone);
                DungeonManager.FreezeData();

                from.SendMessage(68, "새로운 던전 구역이 생성되었습니다. 시작점과 끝점, 이름을 설정해 주세요.");
                from.SendGump(new DungeonSettingGump(newZone, m_Mode, m_MapFilter, m_Page));
                return;
            }

            if (info.ButtonID >= 50 && info.ButtonID <= 54) { from.SendGump(new ZoneMonitorGump(m_Mode, info.ButtonID - 50, 0, m_MapFilter)); return; }
            if (info.ButtonID >= 70 && info.ButtonID <= 76) { from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, 0, info.ButtonID - 70)); return; }
            if (info.ButtonID == 12) { EcosystemManager.RebuildZones(); from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page, m_MapFilter)); return; }
            if (info.ButtonID == 10) { from.SendGump(new ZoneMonitorGump(0, 0, 0, m_MapFilter)); return; }
            if (info.ButtonID == 11) { from.SendGump(new ZoneMonitorGump(1, 0, 0, m_MapFilter)); return; }
            if (info.ButtonID == 13) { from.SendGump(new ZoneMonitorGump(2, 0, 0, m_MapFilter)); return; }
            if (info.ButtonID == 1)  { from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page - 1, m_MapFilter)); return; }
            if (info.ButtonID == 2)  { from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page + 1, m_MapFilter)); return; }

            Map[] mapRefs = { null, Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            Map currentFilterMap = mapRefs[m_MapFilter];

            if (info.ButtonID == 800 && m_MapFilter > 0 && currentFilterMap != null)
            {
                bool wasActive = NewSpawnManager.ActiveMaps.GetValueOrDefault(currentFilterMap, true);
                bool nowActive = !wasActive; 
                NewSpawnManager.ActiveMaps[currentFilterMap] = nowActive;

                if (!nowActive) 
                { 
                    int dCount = DungeonManager.ClearMapSpawns(currentFilterMap);
                    
                    int eCount = 0;
                    var eNodes = World.Items.Values.OfType<EcoNode>().Where(n => n.Map == currentFilterMap).ToList();
                    foreach (var n in eNodes) { n.Delete(); eCount++; }
                    EcosystemManager.ClearMapSpawns(currentFilterMap);
                    EcosystemManager.RebuildZones();

                    int rCount = 0;
                    foreach (var pool in ResourceManager.PoolList.Where(p => p.Facet == currentFilterMap))
                    {
                        rCount += pool.ActiveMonsters.Count;
                        foreach (var m in pool.ActiveMonsters) m?.Delete();
                        pool.ActiveMonsters.Clear();
                    }
                    from.SendMessage(33, $"[{currentFilterMap.Name}] 스폰 정지! 몹 청소 완료."); 
                }
                else 
                { 
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
                                node.RCode = (RegionCode)((mapId + 1) * 1000000 + (cx * 1000) + cy);
                            }

                            int z = kvp.Key.Facet.GetAverageZ(kvp.Value.CenterX, kvp.Value.CenterY);
                            node.MoveToWorld(new Point3D(kvp.Value.CenterX, kvp.Value.CenterY, z), kvp.Key.Facet);
                            eNodeCount++;
                        }
                    }
                    EcosystemManager.RebuildZones();
                    from.SendMessage(68, $"[{currentFilterMap.Name}] 시스템 재가동! 생태계({eNodeCount}개) 복구 완료."); 
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
                    // 🌟 인덱스 선택 시 정렬 로직이 일치하도록 수정
                    var list = DungeonManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID))
                               .OrderBy(z => (string.IsNullOrEmpty(z.GroupName) || z.GroupName == "Uncategorized" || z.GroupName == "기본 그룹") ? "미분류 던전" : z.GroupName)
                               .ThenBy(z => (string.IsNullOrEmpty(z.SubZoneName) || z.SubZoneName == "Main" || z.SubZoneName == "메인 구역") ? NewSpawnManager.GetDisplayName(z.RCode) : z.SubZoneName).ToList();
                    
                    if (targetIndex < list.Count)
                    {
                        var z = list[targetIndex];
                        if (info.ButtonID >= 200 && info.ButtonID < 300)
                        {
                            from.SendGump(new DungeonSettingGump(z, m_Mode, m_MapFilter, m_Page));
                            return; 
                        }
                        else if (info.ButtonID >= 300 && info.ButtonID < 400)
                        {
                            z.GoToNextNode(from);
                        }
                    }
                }
                else if (m_Mode == 1) // Ecology
                {
                    var list = EcosystemManager.ZoneList.Where(z => currentFilterMap == null || (z.Facet != null && z.Facet.MapID == currentFilterMap.MapID)).ToList();
                    if (targetIndex < list.Count)
                    {
                        var z = list[targetIndex];
                        if (info.ButtonID >= 300 && info.ButtonID < 400 && z.Nodes.Count > 0)
                        {
                            string key = string.Format("Eco_{0}_{1}", z.Facet.MapID, (int)z.RCode);
                            int nextIdx = 0;
                            if (NewSpawnManager.TeleportIndex.ContainsKey(key)) nextIdx = NewSpawnManager.TeleportIndex[key];
                            
                            nextIdx %= z.Nodes.Count;
                            from.MoveToWorld(z.Nodes[nextIdx].Location, z.Nodes[nextIdx].Map);
                            NewSpawnManager.TeleportIndex[key] = nextIdx + 1;
                            
                            from.SendMessage(68, string.Format("[생태계] {0} 구역의 {1}/{2}번 노드로 이동했습니다.", NewSpawnManager.GetDisplayName(z.RCode), nextIdx + 1, z.Nodes.Count));
                        }
                        else if (info.ButtonID >= 400 && info.ButtonID < 500 && z.Nodes.Count == 0)
                        {
                            EcoNode newNode = new EcoNode();
                            newNode.RCode = z.RCode;
                            newNode.SpawnRange = 30; newNode.HomeRange = 50;
                            newNode.MoveToWorld(from.Location, from.Map);
                            
                            if (z.Facet == null) z.Facet = from.Map; 
                            z.CacheNodes();
                            from.SendMessage(68, $"[생태계 노드 생성] {NewSpawnManager.GetDisplayName(z.RCode)} 생태 노드가 플레이어 발밑에 설치되었습니다.");
                        }
                        else if (info.ButtonID >= 600 && info.ButtonID < 700 && z.Nodes.Count > 1)
                        {
                            for (int i = 1; i < z.Nodes.Count; i++) z.Nodes[i].Delete();
                            z.CacheNodes();
                            from.SendMessage(68, "중복 생태계 노드가 정리되었습니다.");
                        }
                    }
                }
                else if (m_Mode == 2) // Resource
                {
                    var rawList = ResourceManager.PoolList.Where(p => currentFilterMap == null || (p.Facet != null && p.Facet.MapID == currentFilterMap.MapID)).ToList();
                    if (m_SubMode > 0) rawList = rawList.Where(p => p.Type == (ResourceType)(m_SubMode == 1 ? 0 : m_SubMode == 2 ? 1 : m_SubMode == 3 ? 2 : 4)).ToList();
                    rawList = rawList.Where(p => p.LocType != LocationType.Farm_Remote && !p.IsPrivate).ToList();
                    
                    var tempGrouped = rawList.Where(p => p.RCode != RegionCode.None).GroupBy(p => new { p.Facet, p.RCode, p.Type }).ToList();
                    var finalGrouped = new List<object>();

                    foreach (var g in tempGrouped)
                    {
                        var first = g.First();
                        if (first.Type == ResourceType.Farming)
                        {
                            int rCount = World.Items.Values.OfType<EcoNode>().Count(n => n.Map == first.Facet && n.RCode == first.RCode && string.Equals(n.Name, "FarmNode", StringComparison.OrdinalIgnoreCase));
                            if (rCount == 0) continue; 
                        }
                        finalGrouped.Add(g);
                    }

                    var wild = rawList.Where(p => p.RCode == RegionCode.None).Select(p => (object)p).ToList();
                    var targetList = finalGrouped.Concat(wild).ToList();

                    if (targetIndex < targetList.Count)
                    {
                        if (info.ButtonID >= 300 && info.ButtonID < 400)
                        {
                            object target = targetList[targetIndex];

                            if (target is IGrouping<object, ResourcePool> group)
                            {
                                var first = group.First();
                                string key = string.Format("{0}_{1}_{2}", first.Facet.MapID, (int)first.RCode, first.Type);
                                int nextIdx = 0;
                                if (NewSpawnManager.TeleportIndex.ContainsKey(key)) nextIdx = NewSpawnManager.TeleportIndex[key];

                                if (first.Type == ResourceType.Farming)
                                {
                                    var nodes = World.Items.Values.OfType<EcoNode>().Where(n => n.Map == first.Facet && n.RCode == first.RCode && string.Equals(n.Name, "FarmNode", StringComparison.OrdinalIgnoreCase)).OrderBy(n => n.X).ThenBy(n => n.Y).ToList();
                                    if (nodes.Count > 0)
                                    {
                                        nextIdx %= nodes.Count;
                                        from.MoveToWorld(nodes[nextIdx].Location, first.Facet);
                                        NewSpawnManager.TeleportIndex[key] = nextIdx + 1;
                                    }
                                }
                                else
                                {
                                    var pools = group.ToList();
                                    if (pools.Count > 0)
                                    {
                                        nextIdx %= pools.Count;
                                        ResourcePool targetPool = pools[nextIdx];
                                        int z = first.Facet.GetAverageZ(targetPool.CenterX, targetPool.CenterY);
                                        from.MoveToWorld(new Point3D(targetPool.CenterX, targetPool.CenterY, z), first.Facet);
                                        NewSpawnManager.TeleportIndex[key] = nextIdx + 1;
                                    }
                                }
                            }
                            else if (target is ResourcePool p)
                            {
                                int z = p.Facet.GetAverageZ(p.CenterX, p.CenterY);
                                from.MoveToWorld(new Point3D(p.CenterX, p.CenterY, z), p.Facet);
                            }
                        }
                    }
                }
            }
            from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page, m_MapFilter));
        }
    }

    // ========================================================================
    // ⚔️ DungeonSettingGump: 상세 설정 Gump (도시 치안 가중치 설정 페이지 추가)
    // ========================================================================
    public class DungeonSettingGump : Gump
    {
        private DungeonZone m_Zone;
        private int m_RetMode, m_RetFilter, m_RetPage;
        private int m_PageTab; // 0: 기본설정, 1: 도시치안설정

        public DungeonSettingGump(DungeonZone zone, int mode, int filter, int page) : this(zone, mode, filter, page, 0) { }

        public DungeonSettingGump(DungeonZone zone, int mode, int filter, int page, int pageTab) : base(50, 50)
        {
            m_Zone = zone; m_RetMode = mode; m_RetFilter = filter; m_RetPage = page; m_PageTab = pageTab;

            if (m_Zone.AreaBounds.Count == 0)
            {
                Rectangle2D defaultRect = RegionSaver.GetRegionBounds(m_Zone.RCode, m_Zone.Facet);
                if (defaultRect.Width > 0 && defaultRect.Height > 0) m_Zone.AreaBounds.Add(defaultRect);
            }

            AddPage(0);
            AddBackground(0, 0, 600, 550, 9200); 
            AddAlphaRegion(10, 10, 580, 530);
            
            string titleName = NewSpawnManager.GetDisplayName(m_Zone.RCode);
            AddHtml(10, 15, 580, 25, $"<CENTER><BASEFONT COLOR=#FFCC00 SIZE=5>[{titleName}] 상세 설정</BASEFONT></CENTER>", false, false);

            // 🌟 상단 탭 데코레이션
            AddImageTiled(20, 45, 560, 25, 2624);
            AddButton(30, 47, m_PageTab == 0 ? 4006 : 4005, 4007, 901, GumpButtonType.Reply, 0);
            AddLabel(65, 47, m_PageTab == 0 ? 68 : 1152, "기본 및 생태계 설정");

            AddButton(250, 47, m_PageTab == 1 ? 4006 : 4005, 4007, 902, GumpButtonType.Reply, 0);
            AddLabel(285, 47, m_PageTab == 1 ? 68 : 1152, "도시별 치안 영향도 설정");

            if (m_PageTab == 0)
            {
                RenderDefaultTab();
            }
            else
            {
                RenderSecurityTab();
            }
        }

        // --- 1페이지: 기본 설정 뷰 ---
        private void RenderDefaultTab()
        {
            int y = 75;
            // 대분류 / 소분류 편집
            AddImageTiled(20, y, 560, 45, 9354);
            AddLabel(30, y + 10, 1152, "대분류(그룹명)"); AddImageTiled(130, y + 8, 140, 20, 2624); 
            string displayGroup = (m_Zone.GroupName == "Uncategorized" || m_Zone.GroupName == "기본 그룹") ? "" : m_Zone.GroupName;
            AddTextEntry(135, y + 8, 130, 20, 1152, 12, displayGroup);

            AddLabel(290, y + 10, 1152, "소분류(구역명)"); AddImageTiled(390, y + 8, 140, 20, 2624); 
            string displaySub = (m_Zone.SubZoneName == "Main" || m_Zone.SubZoneName == "메인 구역") ? "" : m_Zone.SubZoneName;
            AddTextEntry(395, y + 8, 130, 20, 1152, 13, displaySub);
            y += 50;

            // 구역 설정
            AddImageTiled(20, y, 560, 45, 9354);
            AddLabel(30, y + 10, 0x481, "물리적 구역:"); 
            int startX = 0, startY = 0, endX = 0, endY = 0;
            if (m_Zone.AreaBounds.Count > 0)
            {
                startX = m_Zone.AreaBounds[0].X; startY = m_Zone.AreaBounds[0].Y;
                endX = startX + m_Zone.AreaBounds[0].Width; endY = startY + m_Zone.AreaBounds[0].Height;
            }
            AddLabel(100, y + 10, 1152, "시작 X"); AddImageTiled(140, y + 8, 35, 20, 2624); AddTextEntry(145, y + 8, 30, 20, 1152, 0, startX.ToString());
            AddLabel(180, y + 10, 1152, "Y"); AddImageTiled(195, y + 8, 35, 20, 2624); AddTextEntry(200, y + 8, 30, 20, 1152, 1, startY.ToString());
            AddLabel(240, y + 10, 1152, "끝 X"); AddImageTiled(270, y + 8, 35, 20, 2624); AddTextEntry(275, y + 8, 30, 20, 1152, 2, endX.ToString());
            AddLabel(310, y + 10, 1152, "Y"); AddImageTiled(325, y + 8, 35, 20, 2624); AddTextEntry(330, y + 8, 30, 20, 1152, 3, endY.ToString());
            AddButton(375, y + 10, 4011, 4013, 10, GumpButtonType.Reply, 0); AddLabel(405, y + 10, 0x42, "시작점 셋");
            AddButton(475, y + 10, 4011, 4013, 11, GumpButtonType.Reply, 0); AddLabel(505, y + 10, 0x42, "끝점 셋");
            y += 50;

            // 시스템 ON/OFF
            AddImageTiled(20, y, 560, 40, 9354);
            bool isActive = m_Zone.IsActive;
            AddButton(30, y + 8, isActive ? 2361 : 2360, isActive ? 2361 : 2360, 20, GumpButtonType.Reply, 0);
            AddLabel(55, y + 10, isActive ? 68 : 33, isActive ? "시스템 가동 중 (ONLINE)" : "시스템 정지됨 (OFFLINE)");
            AddButton(415, y + 10, 4017, 4019, 30, GumpButtonType.Reply, 0); AddLabel(450, y + 10, 33, "소환 몹 즉시 삭제");
            y += 45;

            // 스폰 생태계
            AddImageTiled(20, y, 560, 75, 9354);
            AddHtml(20, y + 5, 560, 20, "<CENTER><BASEFONT COLOR=#42FF42>스폰(Spawn) 생태계 설정</BASEFONT></CENTER>", false, false);
            AddLabel(30, y + 30, 1152, "최대 인구"); AddImageTiled(100, y + 28, 45, 20, 2624); 
            string popStr = m_Zone.ManualMaxPopulation == -1 ? "-1" : m_Zone.ManualMaxPopulation.ToString();
            AddTextEntry(105, y + 28, 40, 20, 1152, 4, popStr); AddLabel(150, y + 30, 0x481, "(-1: 자동 계산)");
            AddLabel(280, y + 30, 1152, "보충률(%)"); AddImageTiled(350, y + 28, 35, 20, 2624); AddTextEntry(355, y + 28, 30, 20, 1152, 5, (m_Zone.ReplenishRate * 100).ToString("0"));
            AddLabel(405, y + 30, 1152, "냉각 가중치"); AddImageTiled(485, y + 28, 45, 20, 2624); AddTextEntry(490, y + 28, 40, 20, 1152, 6, m_Zone.HeatDecayWeight.ToString());
            y += 80;

            // 열기 및 보스
            AddImageTiled(20, y, 560, 100, 9354);
            AddHtml(20, y + 5, 560, 20, "<CENTER><BASEFONT COLOR=#FF5555>열기(Heat) 및 보스(Boss) 설정</BASEFONT></CENTER>", false, false);
            AddLabel(30, y + 30, 1152, "목표 열기"); AddImageTiled(110, y + 28, 80, 20, 2624); AddTextEntry(115, y + 28, 70, 20, 1152, 7, m_Zone.TargetHeat.ToString());
            AddLabel(210, y + 30, 1152, "휴식(분)"); AddImageTiled(280, y + 28, 45, 20, 2624); AddTextEntry(285, y + 28, 40, 20, 1152, 8, m_Zone.RestDuration.TotalMinutes.ToString());
            AddLabel(30, y + 60, 1152, "보스 클래스"); AddImageTiled(110, y + 58, 230, 20, 2624); AddTextEntry(115, y + 58, 220, 20, 1152, 9, m_Zone.BossType != null ? m_Zone.BossType.Name : "");
            AddLabel(350, y + 60, 0x481, "(미입력 시 보스 없음)");
            y += 105;

            // 유물 옵션
            AddImageTiled(20, y, 560, 45, 9354);
            bool rActive = m_Zone.EnableRareDrops;
            AddButton(30, y + 10, rActive ? 2361 : 2360, rActive ? 2361 : 2360, 40, GumpButtonType.Reply, 0); AddLabel(55, y + 10, 1152, "유물 드랍");
            AddLabel(130, y + 10, 1152, "열기(%)"); AddImageTiled(180, y + 8, 40, 20, 2624); AddTextEntry(185, y + 8, 35, 20, 1152, 10, m_Zone.RareDropHeatThreshold.ToString());
            AddLabel(230, y + 10, 1152, "확률"); AddImageTiled(270, y + 8, 40, 20, 2624); AddTextEntry(275, y + 8, 35, 20, 1152, 11, m_Zone.RareDropChance.ToString());
            bool isSteal = m_Zone.IsStealable;
            AddButton(330, y + 10, isSteal ? 2361 : 2360, isSteal ? 2361 : 2360, 50, GumpButtonType.Reply, 0); AddLabel(355, y + 10, 1152, "Stealable 처리");

            // 하단 컨트롤
            AddButton(100, 500, 4023, 4025, 1, GumpButtonType.Reply, 0); AddLabel(135, 500, 68, "설정 저장 (SAVE)");
            AddButton(400, 500, 4014, 4016, 100, GumpButtonType.Reply, 0); AddLabel(435, 500, 0x481, "뒤로가기");
        }

        // --- 🌟 2페이지: 도시 치안 연결 설정 뷰 ---
        private void RenderSecurityTab()
        {
            AddHtml(20, 75, 560, 40, "<BASEFONT COLOR=#55CCFF SIZE=4>본 던전의 열기(Heat)가 상승할 때 치안 패널티를 부여할 인접 도시들을 지정합니다. 여러 대도시를 중복 선택하여 연결할 수 있습니다.</BASEFONT>", false, false);

            // 고정 대도시 목록 (TownNumber.cs 데이터와 100% 일치)
            string[] cities = new string[] { "Britain", "Minoc", "Moonglow", "Trinsic", "Vesper", "Luna", "Zento", "Royal City", "Buccaneer's Den", "Jhelom", "Magincia", "Nujel'm", "Haven", "Serpent's Hold", "Skara Brae", "Wind", "Yew", "Delucia", "Papua", "Cove" };

            int startY = 130;
            // 2열 종대로 정렬하여 20개 도시를 깔끔하게 배치
            for (int i = 0; i < cities.Length; i++)
            {
                int col = i % 2;
                int row = i / 2;

                int x = 40 + (col * 280);
                int currentY = startY + (row * 32);

                // 배경 슬롯 설정
                AddImageTiled(x, currentY, 260, 28, 9354);

                // 현재 던전 장부에 해당 도시 영향도가 저장되어 있는지 확인
                double currentImpact = 0.0;
                if (m_Zone.CitySecurityImpact.TryGetValue(cities[i], out double val))
                {
                    currentImpact = val;
                }

                // 체크여부 (가중치가 0보다 크면 활성화 상태로 간주)
                bool isConnected = currentImpact > 0.0;
                AddImage(x + 10, currentY + 6, isConnected ? 211 : 210);

                // 도시 이름 레이블 표시
                AddLabel(x + 35, currentY + 4, isConnected ? 68 : 1152, cities[i]);

                // 가중치 입력 칸 (TextEntry ID는 50번부터 순차 부여)
                AddLabel(x + 160, currentY + 4, 0x481, "영향도:");
                AddImageTiled(x + 210, currentY + 4, 35, 18, 2624);
                
                // 가중치를 시각적으로 알기 쉽게 % 단위 정수형태로 변환하여 출력 (예: 0.20 -> 20)
                int displayPct = (int)(currentImpact * 100);
                AddTextEntry(x + 213, currentY + 4, 30, 18, 1152, 50 + i, displayPct.ToString());
                AddLabel(x + 246, currentY + 4, 0x481, "%");
            }

            // 하단 조작계
            AddButton(100, 500, 4023, 4025, 2, GumpButtonType.Reply, 0); AddLabel(135, 500, 68, "치안 설정 저장 (SAVE)");
            AddButton(400, 500, 4014, 4016, 100, GumpButtonType.Reply, 0); AddLabel(435, 500, 0x481, "뒤로가기");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            from.CloseGump(typeof(DungeonSettingGump));

            if (info.ButtonID == 100 || info.ButtonID == 0)
            {
                from.SendGump(new ZoneMonitorGump(m_RetMode, 0, m_RetPage, m_RetFilter));
                return;
            }

            // 🌟 상단 탭 전환 처리
            if (info.ButtonID == 901) { from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 0)); return; }
            if (info.ButtonID == 902) { from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 1)); return; }

            // 기본 버튼 핸들러 (1페이지 활성화 상태일 때만 개별 동작 작동)
            if (m_PageTab == 0)
            {
                if (info.ButtonID == 20) { m_Zone.IsActive = !m_Zone.IsActive; from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 0)); return; }
                if (info.ButtonID == 30) { m_Zone.ClearAllSpawns(); from.SendMessage(68, "소환 몹 삭제 완료."); from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 0)); return; }
                if (info.ButtonID == 40) { m_Zone.EnableRareDrops = !m_Zone.EnableRareDrops; from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 0)); return; }
                if (info.ButtonID == 50) { m_Zone.IsStealable = !m_Zone.IsStealable; from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 0)); return; }
                
                if (info.ButtonID == 10 || info.ButtonID == 11) 
                { 
                    int cx1 = 0, cy1 = 0, cx2 = 0, cy2 = 0;
                    int.TryParse(info.GetTextEntry(0)?.Text, out cx1);
                    int.TryParse(info.GetTextEntry(1)?.Text, out cy1);
                    int.TryParse(info.GetTextEntry(2)?.Text, out cx2);
                    int.TryParse(info.GetTextEntry(3)?.Text, out cy2);

                    if (info.ButtonID == 10) { cx1 = from.X; cy1 = from.Y; from.SendMessage(68, "시작점이 임시 기록되었습니다."); }
                    else { cx2 = from.X; cy2 = from.Y; from.SendMessage(68, "끝점이 임시 기록되었습니다."); }

                    m_Zone.AreaBounds.Clear();
                    int nx = Math.Min(cx1, cx2); int ny = Math.Min(cy1, cy2);
                    int nw = Math.Abs(cx1 - cx2); int nh = Math.Abs(cy1 - cy2);
                    if (nw > 0 && nh > 0) m_Zone.AreaBounds.Add(new Rectangle2D(nx, ny, nw, nh));

                    from.SendGump(new DungeonSettingGump(m_Zone, m_RetMode, m_RetFilter, m_RetPage, 0)); 
                    return; 
                }

                if (info.ButtonID == 1) // 1페이지 저장
                {
                    try
                    {
                        m_Zone.GroupName = info.GetTextEntry(12)?.Text?.Trim() ?? "";
                        m_Zone.SubZoneName = info.GetTextEntry(13)?.Text?.Trim() ?? "";

                        int x1 = int.Parse(info.GetTextEntry(0)?.Text ?? "0");
                        int y1 = int.Parse(info.GetTextEntry(1)?.Text ?? "0");
                        int x2 = int.Parse(info.GetTextEntry(2)?.Text ?? "0");
                        int y2 = int.Parse(info.GetTextEntry(3)?.Text ?? "0");
                        m_Zone.AreaBounds.Clear();
                        int trueX = Math.Min(x1, x2); int trueY = Math.Min(y1, y2);
                        int w = Math.Abs(x1 - x2); int h = Math.Abs(y1 - y2);
                        if (w > 0 && h > 0) m_Zone.AreaBounds.Add(new Rectangle2D(trueX, trueY, w, h));

                        int popInput = int.Parse(info.GetTextEntry(4)?.Text ?? "-1");
                        if (popInput == -1) { m_Zone.ManualMaxPopulation = -1; m_Zone.CalculateDynamicPopulation(); }
                        else { m_Zone.SetPopulation(popInput); }

                        m_Zone.ReplenishRate = Math.Max(0.01, Math.Min(1.0, double.Parse(info.GetTextEntry(5)?.Text ?? "40") / 100.0));
                        m_Zone.HeatDecayWeight = int.Parse(info.GetTextEntry(6)?.Text ?? "5");
                        m_Zone.TargetHeat = int.Parse(info.GetTextEntry(7)?.Text ?? "100000");
                        m_Zone.RestDuration = TimeSpan.FromMinutes(double.Parse(info.GetTextEntry(8)?.Text ?? "360"));
                        
                        string bName = info.GetTextEntry(9)?.Text?.Trim() ?? "";
                        if (string.IsNullOrEmpty(bName)) m_Zone.BossType = null;
                        else { Type t = ScriptCompiler.FindTypeByName(bName); if (t != null && t.IsSubclassOf(typeof(BaseCreature))) m_Zone.BossType = t; }

                        m_Zone.RareDropHeatThreshold = int.Parse(info.GetTextEntry(10)?.Text ?? "80");
                        m_Zone.RareDropChance = double.Parse(info.GetTextEntry(11)?.Text ?? "0.05");

                        from.SendMessage(68, "던전 설정 기본 장부가 성공적으로 보존되었습니다.");
                    }
                    catch { from.SendMessage(33, "입력값이 올바르지 않습니다."); }
                    from.SendGump(new ZoneMonitorGump(m_RetMode, 0, m_RetPage, m_RetFilter));
                }
            }
            // 🌟 2페이지 전용 치안 가중치 대량 저장 핸들러
            else if (m_PageTab == 1 && info.ButtonID == 2)
            {
                try
                {
                    string[] cities = new string[] { "Britain", "Minoc", "Moonglow", "Trinsic", "Vesper", "Luna", "Zento", "Royal City", "Buccaneer's Den", "Jhelom", "Magincia", "Nujel'm", "Haven", "Serpent's Hold", "Skara Brae", "Wind", "Yew", "Delucia", "Papua", "Cove" };

                    m_Zone.CitySecurityImpact.Clear(); // 새로운 설정으로 완전 갱신

                    for (int i = 0; i < cities.Length; i++)
                    {
                        TextRelay entry = info.GetTextEntry(50 + i);
                        if (entry != null && int.TryParse(entry.Text, out int pctVal) && pctVal > 0)
                        {
                            // % 수치를 소수점 원시 데이터 가중치로 역산하여 장부에 저장 (예: 25 -> 0.25)
                            double finalImpact = Math.Clamp(pctVal / 100.0, 0.01, 1.0);
                            m_Zone.CitySecurityImpact[cities[i]] = finalImpact;
                        }
                    }

                    from.SendMessage(68, "이 던전과 대도시 간의 치안 패널티 연결 장부가 성공적으로 업데이트되었습니다.");
                }
                catch { from.SendMessage(33, "치안 설정 처리 중 예상치 못한 오류가 발생했습니다."); }
                
                from.SendGump(new ZoneMonitorGump(m_RetMode, 0, m_RetPage, m_RetFilter));
            }
        }
    }
	#endregion
}