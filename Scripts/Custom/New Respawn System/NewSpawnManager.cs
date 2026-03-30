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
using System.Xml.Linq;

namespace Server.Misc
{
    public class NewSpawnManager
    {
        public static void Initialize()
        {
            CommandSystem.Register("ns", AccessLevel.Administrator, new CommandEventHandler(OnNewSpawn));
            CommandSystem.Register("zonemonitor", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("zm", AccessLevel.Administrator, new CommandEventHandler(OnMonitor));
            CommandSystem.Register("fixallnodes", AccessLevel.Administrator, new CommandEventHandler(OnFixAllNodes));
            CommandSystem.Register("wipeworldspawns", AccessLevel.Administrator, new CommandEventHandler(OnWipeWorldSpawns));
            CommandSystem.Register("WipeOldVendorSpawners", AccessLevel.Administrator, new CommandEventHandler(OnWipeOldVendors));
            CommandSystem.Register("wipewildcrops", AccessLevel.Administrator, new CommandEventHandler(OnWipeWildCrops));
			CommandSystem.Register("wipezonespawns", AccessLevel.Administrator, new CommandEventHandler(OnWipeZoneSpawns));
			CommandSystem.Register("EcoWipe", AccessLevel.Administrator, new CommandEventHandler(OnEcoWipe));
			CommandSystem.Register("UpdateRanges", AccessLevel.Administrator, new CommandEventHandler(OnUpdate));
			CommandSystem.Register("CleanOcean", AccessLevel.Administrator, new CommandEventHandler(OnClean));
        }

		[Usage("CleanOcean")]
        public static void OnClean(CommandEventArgs e)
        {
            // 이름에 Ocean, Sea가 들어가거나 관리하는 종이 아예 없는(0종) 껍데기 구역들을 찾아냅니다.
            var keysToRemove = EcosystemManager.Zones.Keys
                .Where(k => k.ToLower().Contains("ocean") || 
                            k.ToLower().Contains("sea") || 
                            EcosystemManager.Zones[k].SpeciesInfo.Count == 0)
                .ToList();

            int count = 0;
            foreach (var k in keysToRemove)
            {
                // 매니저에서 완전히 도려냅니다. (이후 월드 세이브 시 영구 삭제됨)
                EcosystemManager.Zones.Remove(k);
                count++;
            }

            e.Mobile.SendMessage(68, $"[정리 완료] {count}개의 해양 및 빈 껍데기 생태계 구역을 매니저에서 삭제했습니다.");
        }
		[Usage("UpdateRanges")]
        public static void OnUpdate(CommandEventArgs e)
        {
			int count = 0;
            foreach (var item in World.Items.Values.OfType<EcoNode>())
            {
                if (EcosystemManager.Zones.TryGetValue(item.ZoneId, out var zone))
                {
                    // 🌟 수정: 모든 동물 종의 MaxPopulation 합산
                    int totalPop = zone.SpeciesInfo.Values.Sum(s => s.MaxPopulation);
                    
                    int range = Math.Max(30, (int)Math.Sqrt(totalPop * 100));
                    item.SpawnRange = range;
                    item.HomeRange = range + 20;
                    count++;
                }
            }
            e.Mobile.SendMessage(68, $"{count}개의 에코 노드 범위가 인구수에 비례하여 갱신되었습니다.");
        }
		[Usage("EcoWipe")]
        [Description("트라멜의 모든 야생 몬스터를 강제로 삭제합니다. (펫, 상인, 가드는 보호됨)")]
        public static void OnEcoWipe(CommandEventArgs e)
        {
            int count = 0;
            List<Mobile> toDelete = [];

            // 월드의 모든 몹을 뒤집니다.
            foreach (Mobile m in World.Mobiles.Values)
            {
                // 트라멜에 있는 몬스터(BaseCreature)만 타겟
                if (m is BaseCreature bc && bc.Map == Map.Trammel)
                {
                    // [절대 보호 구역] 유저의 펫, 마구간에 있는 펫, 상인, 가드는 건드리지 않음
                    if (bc.Controlled || bc.IsStabled || bc is BaseVendor || bc is BaseGuard)
                        continue;

                    // 야생에 돌아다니는 녀석들은 모조리 청소 명단에 추가
                    toDelete.Add(bc);
                }
            }

            // 일괄 삭제 (서버 렉 방지를 위해 리스트로 모아서 한 번에 지웁니다)
            foreach (Mobile m in toDelete)
            {
                m.Delete();
                count++;
            }

            e.Mobile.SendMessage(66, $"[강제 청소 완료] 트라멜의 야생 몬스터 {count}마리가 소멸되었습니다.");
            e.Mobile.SendMessage(66, $"이제 새로운 광역 생태계(40~60 범위) 설정에 맞춰 몬스터들이 리스폰됩니다.");
        }


		[Usage("wipezonespawns")]
        [Description("던전 및 생태계 매니저에 의해 스폰된 모든 몬스터와 동물을 일괄 삭제합니다.")]
        public static void OnWipeZoneSpawns(CommandEventArgs e)
        {
            int dCount = 0, eCount = 0;
            foreach (var z in DungeonManager.Zones.Values) 
            {
                dCount += z.GetTotalActiveCount();
                z.ClearAllSpawns();
            }
            foreach (var z in EcosystemManager.Zones.Values) 
            {
                eCount += z.SpeciesInfo.Values.Sum(s => s.ActiveAnimals.Count);
                z.ClearAllSpawns(); // 방금 추가한 생태계 청소 로직 호출
            }
            e.Mobile.SendMessage(68, $"던전 몬스터 {dCount}마리, 생태계 동물 {eCount}마리를 일괄 삭제했습니다!");
        }

		public static void DoImport(Mobile from, int mode)
		{
			if (mode == 0) // 삭제
			{
				CommandSystem.Handle(from, $"{CommandSystem.Prefix}BaseVendorWipe");
			}
			else if (mode == 2) // 로드 (벤더 XML 동기화)
			{
				var checkRespawn = CheckRespawnXml();
				
				if (checkRespawn.isValid && checkRespawn.doc != null)
				{
					from.SendMessage(68, "2번 시나리오: NewRespawn.xml 데이터로 VendorNode를 구성합니다.");
					LoadFromRespawnXml(checkRespawn.doc);
				}
				else
				{
					from.SendMessage(68, "1번 시나리오: NewRespawn.xml이 없어 NewVendor.xml 기준으로 구성합니다.");
					LoadFromXml();
				}
			}
			
			from.SendMessage(68, $"[NewSpawn] Mode {mode} Processed.");
		}

		private static (bool isValid, XmlDocument? doc) CheckRespawnXml()
		{
			string path = Path.Combine(Core.BaseDirectory, "Data", "NewRespawn.xml");
			if (!File.Exists(path)) return (false, null);

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				bool isValid = doc.SelectNodes("//VendorNodes/Vendor")?.Count > 0;
				return (isValid, doc);
			}
			catch
			{
				return (false, null);
			}
		}

		private static void LoadFromRespawnXml(XmlDocument doc)
		{
			// 1. 기존 스포너(VendorNode) 및 떠도는 BaseVendor 청소
			var oldNodes = World.Items.Values.OfType<VendorNode>().Where(i => !i.Deleted).ToList();
			foreach (var n in oldNodes) n.Delete();

			var oldVendors = World.Mobiles.Values.OfType<BaseVendor>().Where(m => !m.Deleted).ToList();
			foreach (var v in oldVendors) v.Delete();

			// 2. NewRespawn.xml 데이터를 읽어 VendorNode 설치
			XmlNodeList? vendors = doc.SelectNodes("//VendorNodes/Vendor");
			if (vendors == null) return;

			int nodeCount = 0;
			foreach (XmlNode v in vendors)
			{
				string mapName = v.Attributes?["Map"]?.Value ?? "Trammel";
				string zoneId = v.Attributes?["ZoneId"]?.Value ?? "Unknown";
				string spawnList = v.Attributes?["List"]?.Value ?? "";
				
				Map map = Map.Parse(mapName);
				int x = int.Parse(v.Attributes?["X"]?.Value ?? "0");
				int y = int.Parse(v.Attributes?["Y"]?.Value ?? "0");
				int z = int.Parse(v.Attributes?["Z"]?.Value ?? "0");
				Point3D loc = new Point3D(x, y, z);

				// 해당 좌표(loc)에 존재하는 구형 XmlSpawner를 찾아 모두 삭제
				var oldSpawners = map.GetItemsInRange(loc, 0)
					.Where(i => i.GetType().Name.Contains("XmlSpawner", StringComparison.OrdinalIgnoreCase))
					.ToList();
				foreach (var spawner in oldSpawners) spawner.Delete();

				// ZoneId(문자열)를 TownID(정수)로 변환
				int townID = TownNumber.GetID(loc, map);

				VendorNode node = new VendorNode
				{
					TownID = townID,
					MaxCount = int.Parse(v.Attributes?["MaxCount"]?.Value ?? "1"),
					HomeRange = int.Parse(v.Attributes?["Range"]?.Value ?? "5"),
					SpawnList = spawnList
				};
				
				node.MoveToWorld(loc, map);
				nodeCount++;
			}
			
			Console.WriteLine($"[Economy] {nodeCount}개의 VendorNode가 로드되었으며 구형 스포너가 삭제되었습니다.");
		}

        #region [Commands & Utilities]
        [Usage("wipewildcrops")]
        [Description("현재 맵의 모든 야생 작물을 삭제하고 자원 카운트를 0으로 초기화합니다.")]
        public static void OnWipeWildCrops(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            Map targetMap = from.Map;

            if (targetMap == null || targetMap == Map.Internal) return;

            int itemDeleted = 0;
            int poolReset = 0;
            List<Item> toDelete = new List<Item>();

            foreach (Item item in World.Items.Values)
            {
                if (item.Map == targetMap && item is BaseFarmItem)
                    toDelete.Add(item);
            }

            itemDeleted = toDelete.Count;
            foreach (Item i in toDelete) i.Delete();

            foreach (var kvp in ResourceManager.Pools)
            {
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
                if (item is DungeonNode dNode && (dNode.ZoneId == "Unknown" || string.IsNullOrEmpty(dNode.ZoneId)))
                {
                    dNode.ZoneId = GetGoGumpZoneName(dNode.Location, dNode.Map);
                    count++;
                }
                else if (item is EcoNode eNode && (eNode.ZoneId == "Unknown" || string.IsNullOrEmpty(eNode.ZoneId)))
                {
                    eNode.ZoneId = GetGoGumpZoneName(eNode.Location, eNode.Map);
                    count++;
                }
            }
            e.Mobile.SendMessage(68, $"총 {count}개의 누락된 노드 정보를 갱신했습니다.");
        }

        [Usage("wipeworldspawns")]
        public static void OnWipeWorldSpawns(CommandEventArgs e) => DoReset(e.Mobile);

        [Usage("WipeOldVendorSpawners")]
		[Description("구형 XmlSpawner 중 일반 상인(BaseVendor)을 소환하는 스포너를 찾아 삭제합니다. (뱅커, 힐러 등 특수 NPC는 보호)")]
		public static void OnWipeOldVendors(CommandEventArgs e)
		{
			Type? xmlType = ScriptCompiler.FindTypeByName("XmlSpawner");
			if (xmlType == null) 
			{ 
				e.Mobile.SendMessage(33, "서버에서 XmlSpawner 시스템을 찾을 수 없습니다."); 
				return; 
			}

			int removedEntries = 0;
			List<Item> emptySpawners = [];
			List<Mobile> mobsToDelete = [];

			// 1. 스포너 내부 항목 정밀 타격 (스폰 리스트에서 상인만 삭제)
			foreach (Item item in World.Items.Values)
			{
				if (item.GetType() == xmlType)
				{
					try
					{
						var prop = item.GetType().GetProperty("m_SpawnObjects") ?? item.GetType().GetProperty("SpawnObjects");
						if (prop?.GetValue(item, null) is System.Collections.IList list)
						{
							bool modified = false;

							// 인덱스가 꼬이지 않도록 뒤에서부터 순회
							for (int i = list.Count - 1; i >= 0; i--)
							{
								var so = list[i];
								string typeName = so?.GetType().GetProperty("TypeName")?.GetValue(so, null) as string ?? "";
								if (string.IsNullOrEmpty(typeName)) continue;

								Type? t = ScriptCompiler.FindTypeByName(typeName);

								// 타입이 BaseVendor 상속자라면 검사 진행
								if (t != null && t.IsSubclassOf(typeof(BaseVendor)))
								{
									string nameLower = typeName.ToLower();

									// [보호] 뱅커, 힐러, 가드 등은 마을 기능 NPC이므로 무조건 살려둠
									if (nameLower.Contains("banker") || nameLower.Contains("healer") || 
										nameLower.Contains("guildmaster") || nameLower.Contains("animaltrainer") || 
										nameLower.Contains("stablemaster") || nameLower.Contains("guard"))
									{
										continue;
									}

									// 일반 상인 항목만 리스트에서 제거
									list.RemoveAt(i);
									removedEntries++;
									modified = true;
								}
							}

							// 항목이 지워졌다면 스포너 상태 갱신
							if (modified)
							{
								if (list.Count == 0) 
								{
									// 상인만 들어있어서 속이 텅 빈 스포너라면 삭제 리스트에 추가
									emptySpawners.Add(item);
								}
								else 
								{
									// 다른 NPC가 남아있다면 스포너를 갱신(Respawn)하여 유지
									item.GetType().GetMethod("Respawn")?.Invoke(item, null);
								}
							}
						}
					}
					catch { }
				}
			}

			// 텅 빈 껍데기 스포너들만 일괄 삭제
			foreach (Item spawner in emptySpawners) spawner.Delete();

			// 2. 이미 월드에 소환되어 돌아다니는 구형 상인들(BaseVendor) 직접 강제 퇴근
			foreach (Mobile m in World.Mobiles.Values)
			{
				// BaseVendor(우리가 만든 새 시스템)는 건드리지 않고, 옛날 BaseVendor만 타격
				if (m is BaseVendor && m is not BaseVendor)
				{
					string nameLower = m.GetType().Name.ToLower();
					if (nameLower.Contains("banker") || nameLower.Contains("healer") || 
						nameLower.Contains("guildmaster") || nameLower.Contains("animaltrainer") || 
						nameLower.Contains("stablemaster") || nameLower.Contains("guard"))
					{
						continue;
					}
					mobsToDelete.Add(m);
				}
			}
			
			int deletedMobiles = mobsToDelete.Count;
			foreach (Mobile m in mobsToDelete) m.Delete();

			e.Mobile.SendMessage(68, $"[청소 완료] XmlSpawner에서 상인 스폰 항목 {removedEntries}개 제거 (빈 껍데기 스포너 {emptySpawners.Count}개 삭제).");
			e.Mobile.SendMessage(68, $"월드에 남아있던 구형 상인 {deletedMobiles}명 강제 퇴근 완료.");
		}
		// 🌟 [추가] ZoneId 텍스트를 역추적해서 예상 좌표를 찾아내는 함수
        public static Point3D? FindLocationByZoneId(string zoneId, Map map)
        {
            // 1. [go 메뉴(LocationTree) 데이터에서 검색
            LocationTree tree = (map == Map.Felucca) ? GoGump.Felucca : (map == Map.Trammel ? GoGump.Trammel : (map == Map.Ilshenar ? GoGump.Ilshenar : (map == Map.Malas ? GoGump.Malas : (map == Map.Tokuno ? GoGump.Tokuno : GoGump.TerMur))));
            if (tree != null && tree.Root != null)
            {
                Point3D loc = Point3D.Zero;
                if (SearchGoGumpTree(tree.Root, zoneId, ref loc)) return loc;
            }

            // 2. 서버 Region 데이터에서 검색
            string cleanTarget = DungeonManager.CleanString(zoneId);
            foreach (Region r in Region.Regions)
            {
                if (r.Map == map && !string.IsNullOrEmpty(r.Name))
                {
                    string cleanReg = DungeonManager.CleanString(r.Name);
                    if (cleanReg.Contains(cleanTarget) || cleanTarget.Contains(cleanReg))
                    {
                        Point3D p = GetRegionCenter(r);
                        if (p != Point3D.Zero) return p;
                    }
                }
            }
            return null;
        }

        private static bool SearchGoGumpTree(ParentNode node, string targetZone, ref Point3D loc)
        {
            if (node.Children == null) return false;
            foreach (object child in node.Children)
            {
                if (child is ParentNode pNode)
                {
                    if (SearchGoGumpTree(pNode, targetZone, ref loc)) return true;
                }
                else if (child is ChildNode cNode)
                {
                    // 구역 이름표의 끝부분(예: "Level 2")과 일치하면 해당 좌표를 반환
                    if (targetZone.EndsWith(cNode.Name) || DungeonManager.CleanString(targetZone).Contains(DungeonManager.CleanString(cNode.Name)))
                    {
                        loc = cNode.Location;
                        return true;
                    }
                }
            }
            return false;
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

        public static string FindBestLogicKey(string regionName)
        {
            if (string.IsNullOrEmpty(regionName)) return null;

            string cleanReg = regionName.ToLower();
            var allKeys = DungeonManager.Zones.Keys.Concat(EcosystemManager.Zones.Keys).ToList();

            foreach (string key in allKeys)
            {
                string cleanKey = key.ToLower();

                // 🌟 리전 이름이 유저님이 정한 ZoneId에 포함되어 있는지 확인
                // 예: "Jhelom Cemetery"가 "Trammel Towns Jhelom Cemetery" 안에 있는가?
                if (cleanKey.Contains(cleanReg))
                {
                    return key;
                }
            }
            return null;
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

        private static Point3D GetRegionCenter(Region r) 
        { 
            if (r.Area != null && r.Area.Length > 0) 
            { 
                var a = r.Area[0]; 
                return new Point3D(a.Start.X + ((a.End.X - a.Start.X) / 2), a.Start.Y + ((a.End.Y - a.Start.Y) / 2), r.Map.GetAverageZ(a.Start.X, a.Start.Y)); 
            } 
            return Point3D.Zero; 
        }

        [Usage("ns")] public static void OnNewSpawn(CommandEventArgs e) => e.Mobile.SendGump(new NewSpawnGump());
        [Usage("zonemonitor")] public static void OnMonitor(CommandEventArgs e) => e.Mobile.SendGump(new ZoneMonitorGump(0, 0));
        #endregion

        #region [Export & Import System]
        public static void DoExport(Mobile from, int mode) // 0:모두, 1:던전, 2:벤더
        {
            int dCount = 0, eCount = 0, vCount = 0, popCount = 0;

            // [1] 던전/생태계 노드 추출 (NewRespawn.xml)
            if (mode == 0 || mode == 1)
            {
                string dPath = Path.Combine(Core.BaseDirectory, "Data", "NewRespawn.xml");
                using (XmlTextWriter xml = new XmlTextWriter(dPath, System.Text.Encoding.UTF8))
                {
                    xml.Formatting = Formatting.Indented;
                    xml.WriteStartDocument();
                    xml.WriteStartElement("NewRespawn");

                    xml.WriteStartElement("DungeonNodes");
                    foreach (Item item in World.Items.Values)
                    {
                        if (item is DungeonNode n)
                        {
                            xml.WriteStartElement("Node");
                            xml.WriteAttributeString("Map", n.Map.Name);
                            xml.WriteAttributeString("X", n.X.ToString());
                            xml.WriteAttributeString("Y", n.Y.ToString());
                            xml.WriteAttributeString("Z", n.Z.ToString());
                            xml.WriteAttributeString("ZoneId", n.ZoneId);
                            xml.WriteAttributeString("Depth", ((int)n.Depth).ToString());
                            xml.WriteAttributeString("SpawnRange", n.SpawnRange.ToString());
                            xml.WriteAttributeString("HomeRange", n.HomeRange.ToString());
                            xml.WriteEndElement();
                            dCount++;
                        }
                    }
                    xml.WriteEndElement();

                    // 🌟 생태계(EcoNode) 백업 추가
                    xml.WriteStartElement("EcoNodes");
                    foreach (Item item in World.Items.Values)
                    {
                        if (item is EcoNode en)
                        {
                            xml.WriteStartElement("Node");
                            xml.WriteAttributeString("Map", en.Map.Name);
                            xml.WriteAttributeString("X", en.X.ToString());
                            xml.WriteAttributeString("Y", en.Y.ToString());
                            xml.WriteAttributeString("Z", en.Z.ToString());
                            xml.WriteAttributeString("ZoneId", en.ZoneId);
                            xml.WriteAttributeString("AreaType", ((int)en.AreaType).ToString());
                            xml.WriteAttributeString("ClimateType", ((int)en.ClimateType).ToString());
                            xml.WriteAttributeString("SpawnRange", en.SpawnRange.ToString());
                            xml.WriteAttributeString("HomeRange", en.HomeRange.ToString());
                            xml.WriteEndElement();
                            eCount++;
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
            }

            // [2] 벤더 노드 추출 (NewVendor.xml)
            if (mode == 0 || mode == 2)
            {
                string dir = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string vPath = Path.Combine(dir, "NewVendor.xml");

                XmlWriterSettings settings = new() { Indent = true, IndentChars = "\t" };
                using (XmlWriter writer = XmlWriter.Create(vPath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Vendors");

                    foreach (Item item in World.Items.Values)
                    {
                        if (item is VendorNode v)
                        {
                            writer.WriteStartElement("VendorNode");
                            writer.WriteAttributeString("Name", v.VendorName ?? "a vendor");
                            writer.WriteAttributeString("Map", v.Map?.Name ?? "Trammel");
                            writer.WriteAttributeString("X", v.X.ToString());
                            writer.WriteAttributeString("Y", v.Y.ToString());
                            writer.WriteAttributeString("Z", v.Z.ToString());
                            writer.WriteAttributeString("MaxCount", v.MaxCount.ToString());
                            writer.WriteAttributeString("Range", v.HomeRange.ToString()); 

                            int townID = v.TownID > 0 ? v.TownID : TownNumber.GetID(v.Location, v.Map);
                            string zoneName = townID > 0 ? TownNumber.GetName(townID) : "Unknown";
                            writer.WriteAttributeString("ZoneId", zoneName);

                            writer.WriteStartElement("Inventory");
                            if (townID > 0 && TownEconomyManager.Towns.TryGetValue(townID, out var town))
                            {
                                foreach (var entry in town.InventoryEntries)
                                {
                                    writer.WriteStartElement("Item");
                                    writer.WriteAttributeString("Type", entry.ItemType.Name);
                                    writer.WriteAttributeString("Amount", entry.InitialStock.ToString());
                                    writer.WriteAttributeString("Price", entry.BasePrice.ToString());
                                    writer.WriteEndElement();
                                }
                            }
                            writer.WriteEndElement(); // </Inventory>
                            
                            writer.WriteEndElement(); // </VendorNode>
                            vCount++;
                        }
                    }

                    writer.WriteEndElement(); // </Vendors>
                    writer.WriteEndDocument();
                }
            }

            from.SendMessage(68, $"[Export 완료] 대상: {(mode == 0 ? "전체" : mode == 1 ? "던전/생태계" : "벤더")} (D:{dCount} / Eco:{eCount} / V:{vCount} / Pop:{popCount})");
        }

        public static void LoadFromXml()
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "NewVendor.xml");
            if (!File.Exists(path)) return;

            // 1. 기존 스포너(VendorNode) 삭제
            var oldNodes = World.Items.Values.OfType<VendorNode>().Where(i => !i.Deleted).ToList();
            foreach (var n in oldNodes) n.Delete();

            // 만약을 대비해 떠돌고 있는 BaseVendor 잔여물 강제 청소
            var oldVendors = World.Mobiles.Values.OfType<BaseVendor>().Where(m => !m.Deleted).ToList();
            foreach (var v in oldVendors) v.Delete();

            // 2. 마을 창고 및 설계도 초기화
            foreach (var town in TownEconomyManager.Towns.Values)
            {
                town.VendorCount = 0;
                town.InventoryEntries.Clear();
                town.Warehouse.Clear();
            }

            try
            {
                XmlDocument doc = new();
                doc.Load(path);
                
                // 구버전 <Vendor> 호환 및 신버전 <VendorNode> 동시 지원
                XmlNodeList? nodes = doc.SelectNodes("//VendorNode") ?? doc.SelectNodes("//Vendor");
                if (nodes == null) return;

                int nodeCount = 0;
                foreach (XmlNode n in nodes)
                {
                    string mapName = n.Attributes?["Map"]?.Value ?? "Trammel";
                    string zoneId = n.Attributes?["ZoneId"]?.Value ?? "Unknown";
                    string vName = n.Attributes?["Name"]?.Value ?? "a vendor";
                    Map map = Map.Parse(mapName);

                    int x = 0, y = 0, z = 0;
                    XmlNode? posNode = n.SelectSingleNode("Position");
                    if (posNode != null)
                    {
                        x = int.Parse(posNode.SelectSingleNode("X")?.InnerText ?? "0");
                        y = int.Parse(posNode.SelectSingleNode("Y")?.InnerText ?? "0");
                        z = int.Parse(posNode.SelectSingleNode("Z")?.InnerText ?? "0");
                    }
                    else
                    {
                        x = int.Parse(n.Attributes?["X"]?.Value ?? "0");
                        y = int.Parse(n.Attributes?["Y"]?.Value ?? "0");
                        z = int.Parse(n.Attributes?["Z"]?.Value ?? "0");
                    }
                    Point3D loc = new(x, y, z);

                    int townID = TownNumber.GetID(loc, map);

                    if (townID > 0)
                    {
                        if (!TownEconomyManager.Towns.TryGetValue(townID, out var town))
                        {
                            town = new TownEconomy(townID, 1000000) { Center = loc, Facet = map };
                            TownEconomyManager.Towns[townID] = town;
                        }

                        // [핵심] 상인을 생성하지 않고 스포너(VendorNode)만 설치
                        VendorNode vNode = new VendorNode
                        {
                            TownID = townID,
                            VendorName = vName,
                            MaxCount = int.Parse(n.Attributes?["MaxCount"]?.Value ?? "1"),
                            HomeRange = int.Parse(n.Attributes?["Range"]?.Value ?? "5")
                        };
                        vNode.MoveToWorld(loc, map);
                        nodeCount++;

                        ParseInventoryToTown(n.SelectSingleNode("Inventory"), townID);
                    }
                }
                Console.WriteLine($"[Economy] {nodeCount}개의 VendorNode 로드 및 창고 데이터 세팅 완료.");
            }
            catch (Exception ex) { Console.WriteLine($"[Economy] XML 로드 중 오류 발생: {ex.Message}"); }
        }

        private static void ParseInventoryToTown(XmlNode? inventoryNode, int townID)
        {
            if (inventoryNode == null || !TownEconomyManager.Towns.TryGetValue(townID, out var town)) 
                return;

            var itemNodes = inventoryNode.SelectNodes("Item");
            if (itemNodes == null) return;

            foreach (XmlNode iNode in itemNodes)
            {
                string typeName = iNode.Attributes?["Type"]?.Value?.Trim() ?? "";
                string pStr = iNode.Attributes?["Price"]?.Value ?? "10";
                string aStr = iNode.Attributes?["Amount"]?.Value ?? "100";

                int price = int.Parse(string.IsNullOrEmpty(pStr) ? "10" : pStr);
                int amount = int.Parse(string.IsNullOrEmpty(aStr) ? "100" : aStr);

                Type? itemType = ScriptCompiler.FindTypeByName(typeName, true) 
                              ?? ScriptCompiler.FindTypeByFullName($"Server.Items.{typeName}", true);

                if (itemType != null)
                {
                    // 설계도 중복 누적
                    var existingEntry = town.InventoryEntries.FirstOrDefault(e => e.ItemType == itemType);
                    if (existingEntry != null) existingEntry.InitialStock += amount;
                    else town.InventoryEntries.Add(new TownInventoryEntry(itemType, amount, price));

                    // 실시간 창고 적재
                    if (town.Warehouse.TryGetValue(itemType, out var wItem))
                    {
                        wItem.Stock += amount;
                        wItem.BasePrice = price; 
                    }
                    else town.Warehouse[itemType] = new WarehouseItem(itemType, amount, price);
                }
            }
        }
        #endregion

        public static void DoResetDungeonNodes(Mobile from)
        {
            int count = 0;
            // DungeonNode만 리스트업 (EcoNode는 제외)
            var toDelete = World.Items.Values.OfType<DungeonNode>().ToList(); 
            foreach (var item in toDelete) { item.Delete(); count++; }
            
            foreach (var z in DungeonManager.Zones.Values) { z.ClearAllSpawns(); z.CacheNodes(); }
            
            from.SendMessage(33, $"[던전 리셋 완료] 총 {count}개의 던전 노드 및 관련 몹 청소 완료. (생태계/상인은 유지)");
        }

        // 🌟 [추가] 생태계 노드만 따로 지우는 기능 (필요 시 호출)
        public static void DoResetEcoNodes(Mobile from)
        {
            int count = 0;
            var toDelete = World.Items.Values.OfType<EcoNode>().ToList();
            foreach (var item in toDelete) { item.Delete(); count++; }
            foreach (var z in EcosystemManager.Zones.Values) { z.ClearAllSpawns(); z.CacheNodes(); }
            from.SendMessage(33, $"[생태계 리셋 완료] 총 {count}개의 생태계 노드 삭제 완료.");
        }

        public static void DoResetVendorNodes(Mobile from)
        {
            int count = 0;
            var toDelete = World.Items.Values.OfType<VendorNode>().ToList();
            foreach (var item in toDelete) { item.Delete(); count++; }
            
            from.SendMessage(33, $"[리셋 완료] 총 {count}개의 상인(Vendor) 노드를 삭제했습니다.");
        }

        public static void DoResetAll(Mobile from)
        {
			DoResetEcoNodes(from);
        }

        public static void DoReset(Mobile from)
        {
            int deletedCount = 0, protectedCount = 0;
            Type xmlSpawnerType = ScriptCompiler.FindTypeByName("XmlSpawner");
            if (xmlSpawnerType == null) { from.SendMessage(33, "서버에서 XmlSpawner 시스템을 찾을 수 없습니다."); return; }

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
            from.SendMessage(68, $"[리셋 완료] 사냥터 XmlSpawner {deletedCount}개 삭제됨 (보호된 스포너: {protectedCount}개)");
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

        private static Point3D GetSmartRegionLocation(Region r, Map map, List<Point3D> existingLocs, int minDistance)
        {
            if (r.Area != null && r.Area.Length > 0)
            {
                for (int i = 0; i < 200; i++) // 15타일 간격을 찾기 위해 탐색 횟수 증가
                {
                    var rect = r.Area[Utility.Random(r.Area.Length)];
                    int x = Utility.RandomMinMax(rect.Start.X, rect.End.X);
                    int y = Utility.RandomMinMax(rect.Start.Y, rect.End.Y);
                    int z = map.GetAverageZ(x, y);

                    Point3D testLoc = new Point3D(x, y, z);

                    if (map.CanSpawnMobile(x, y, z) && Region.Find(testLoc, map) == r)
                    {
                        bool tooClose = false;
                        foreach (var loc in existingLocs)
                        {
                            if (Utility.InRange(testLoc, loc, minDistance))
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (!tooClose)
                        {
                            return testLoc; // 조건 만족 시 좌표 반환
                        }
                    }
                }

                // 던전 방이 너무 좁아서 15타일 거리가 안 나오면 빈 공간 아무 곳이나 반환 (안전장치)
                for (int i = 0; i < 50; i++)
                {
                    var rect = r.Area[Utility.Random(r.Area.Length)];
                    int x = Utility.RandomMinMax(rect.Start.X, rect.End.X);
                    int y = Utility.RandomMinMax(rect.Start.Y, rect.End.Y);
                    int z = map.GetAverageZ(x, y);
                    if (map.CanSpawnMobile(x, y, z)) return new Point3D(x, y, z);
                }
                
                var a = r.Area[0];
                return new Point3D(a.Start.X + ((a.End.X - a.Start.X) / 2), a.Start.Y + ((a.End.Y - a.Start.Y) / 2), map.GetAverageZ(a.Start.X, a.Start.Y));
            }
            return Point3D.Zero;
        }

		#region [Reset & Migration System]

		public static void DoMigration(Mobile from, Map map, int mode)
		{
			if (mode != 1 || map == null || map == Map.Internal) return;
			if (EcosystemManager.Zones == null || DungeonManager.Zones == null) return;

			int ecoCount = 0;
			int fixCount = 0;
			int fillCount = 0;

			// [제외 필터]
			string[] finalExcludes = { "Ocean", "Lost Lands", "Hopper's Bog", "Desert of Compassion" };

			bool IsExcluded(string name)
			{
				if (string.IsNullOrEmpty(name)) return false;
				string lower = name.ToLower();
				return finalExcludes.Any(ex => lower.Contains(ex.ToLower()));
			}

			// 1. [기존 노드 전수조사] 치환 및 이름 세탁
			var allNodes = World.Items.Values.Where(i => (i is DungeonNode || i is EcoNode) && i.Map == map && !i.Deleted).ToList();
			var ecoKeys = EcosystemManager.Zones.Keys.ToList();

			foreach (Item item in allNodes)
			{
				Region r = Region.Find(item.Location, map);
				if (r == null || r is DungeonRegion || (r.Name != null && r.Name.Contains("Dungeon"))) continue;

				string rName = r.Name ?? "";
				string zid = (item is DungeonNode dn) ? (dn.ZoneId ?? "") : ((item as EcoNode)?.ZoneId ?? "");
				
				if (IsExcluded(rName) || IsExcluded(zid)) continue;

				bool isCombat = rName.Contains("Fort") || rName.Contains("Orc") || rName.Contains("Camp");

				if (item is DungeonNode oldNode && !isCombat) 
				{
					string matchKey = ecoKeys.FirstOrDefault(k => k != null && (k == zid || k.Contains(rName) || rName.Contains(k))) ?? zid;
					if (IsExcluded(matchKey)) continue;

					EcoNode newNode = new() { ZoneId = matchKey };
					ApplyDefaultSettings(newNode, (matchKey ?? "").ToLower()); // 범위 자동 설정
					newNode.MoveToWorld(oldNode.Location, map);
					
					oldNode.Delete();
					ecoCount++;
				}
				else if (string.IsNullOrEmpty(zid) || zid == "Unknown")
				{
					string correctName = GetGoGumpZoneName(item.Location, map) ?? "Unknown";
					if (IsExcluded(correctName)) continue;

					if (item is DungeonNode d) d.ZoneId = correctName;
					else if (item is EcoNode e) 
					{ 
						e.ZoneId = correctName; 
						ApplyDefaultSettings(e, correctName.ToLower());
					}
					fixCount++;
				}
				// 🌟 [추가된 부분] 이름이 이미 정상적으로 있는 기존 에코 노드의 범위 업데이트
				else if (item is EcoNode existingEco)
				{
					ApplyDefaultSettings(existingEco, (existingEco.ZoneId ?? "").ToLower());
				}
			}

			// 2. [벌목지 공백 채우기] ResourceManager와 연동하여 숲 한가운데에 노드 배치
			if (ResourceManager.Pools != null)
			{
				var lumberPools = ResourceManager.Pools.Values
					.Where(p => p.Type == ResourceType.Lumberjacking && p.MapName == map.Name)
					.ToList();

				foreach (var pool in lumberPools)
				{
					string rName = pool.RegionName ?? "";
					if (IsExcluded(rName)) continue;

					// 해당 벌목 리전 찾기
					Region actualRegion = Region.Regions.FirstOrDefault(reg => reg.Name == rName && reg.Map == map);
					if (actualRegion == null || actualRegion.Area.Length == 0) continue;

					// 기존에 구현되어 있던 헬퍼 함수 활용하여 중심점 계산
					Point3D center = GetRegionCenter(actualRegion);
					if (center == Point3D.Zero) continue;

					// 반경 15타일 이내에 이미 노드가 있는지 체크
					bool alreadyExists = false;
					IPooledEnumerable eable = map.GetItemsInRange(center, 15);
					foreach (Item it in eable) { if (it is EcoNode || it is DungeonNode) { alreadyExists = true; break; } }
					eable.Free();

					if (!alreadyExists)
					{
						string targetZoneId = ecoKeys.FirstOrDefault(k => k != null && (k == rName || k.Contains(rName) || rName.Contains(k))) ?? rName;
						if (IsExcluded(targetZoneId)) continue;

						EcoNode en = new() { ZoneId = targetZoneId };
						ApplyDefaultSettings(en, (targetZoneId ?? "").ToLower()); // 와일드 범위 적용
						en.MoveToWorld(center, map);
						fillCount++;
					}
				}
			}

			// 3. [GoGump 주요 거점 공백 채우기] (기존 로직 유지)
			LocationTree tree = (map == Map.Felucca) ? GoGump.Felucca : (map == Map.Trammel ? GoGump.Trammel : null);
			if (tree?.Root != null)
			{
				Stack<ParentNode> stack = new();
				stack.Push(tree.Root);
				while (stack.Count > 0)
				{
					ParentNode p = stack.Pop();
					if (p?.Children == null) continue;
					foreach (object child in p.Children)
					{
						if (child is ParentNode cp) stack.Push(cp);
						else if (child is ChildNode cNode)
						{
							if (cNode == null) continue;
							string pathName = GetGoGumpZoneName(cNode.Location, map) ?? "Unknown";

							if (IsExcluded(pathName)) continue;

							bool alreadyExists = false;
							IPooledEnumerable eable = map.GetItemsInRange(cNode.Location, 5);
							foreach (Item it in eable) { if (it is EcoNode || it is DungeonNode) { alreadyExists = true; break; } }
							eable.Free();

							if (!alreadyExists)
							{
								EcoNode en = new() { ZoneId = pathName };
								ApplyDefaultSettings(en, pathName.ToLower());
								en.MoveToWorld(cNode.Location, map);
								fillCount++;
							}
						}
					}
				}
			}

			// 4. [최종 동기화] ZM 즉시 반영
			EcosystemManager.Zones.Values.ToList().ForEach(z => z?.CacheNodes());
			DungeonManager.Zones.Values.ToList().ForEach(z => z?.CacheNodes());

			from.SendMessage(66, $"[완료] 치환:{ecoCount} / 교정:{fixCount} / 야생 및 거점 신규:{fillCount}");
		}

		private static void ApplyDefaultSettings(EcoNode node, string name)
        {
            int totalPop = 10;

            if (EcosystemManager.Zones.TryGetValue(name, out var zone))
            {
                // 🌟 수정: EcoZone 내부의 모든 동물 종(Species)의 MaxPopulation을 합산합니다.
                totalPop = zone.SpeciesInfo.Values.Sum(s => s.MaxPopulation);
            }

            // 인구수에 비례하여 스폰 반경 계산 (면적 비례)
            int calculatedRange = Math.Max(30, (int)Math.Sqrt(totalPop * 100));

            node.SpawnRange = calculatedRange;
            node.HomeRange = calculatedRange + 20; 

            // 범위 크기에 따라 구역 성격 자동 지정
            if (calculatedRange >= 80) node.AreaType = EcoAreaType.Forest;
            else if (calculatedRange >= 50) node.AreaType = EcoAreaType.Hunting;
            else node.AreaType = EcoAreaType.Town;

            if (name.Contains("desert")) node.ClimateType = EcoClimateType.Desert;
            else if (name.Contains("snow") || name.Contains("arctic") || name.Contains("ice")) node.ClimateType = EcoClimateType.Arctic;
            else if (name.Contains("swamp") || name.Contains("bog")) node.ClimateType = EcoClimateType.Swamp;
            else node.ClimateType = EcoClimateType.Temperate;
        }

		// 🌟 [추가] GoGump 정답지를 순회하며 노드가 없는 곳에 신규 설치
		public static void FillGapsFromGoGump(Mobile from, Map map)
		{
			// 1. 해당 맵의 GoGump(LocationTree) 가져오기
			LocationTree tree = (map == Map.Felucca) ? GoGump.Felucca : (map == Map.Trammel ? GoGump.Trammel : null);
			if (tree == null || tree.Root == null) return;

			int addedCount = 0;
			List<ChildNode> allLocations = [];
			
			// 2. 트리의 모든 자식 노드(좌표)를 리스트로 추출
			ExtractChildNodes(tree.Root, allLocations);

			foreach (ChildNode cNode in allLocations)
			{
				Point3D loc = cNode.Location;

				// 3. [중복 체크] 해당 좌표 반경 5타일 내에 이미 노드가 있는지 확인
				bool alreadyExists = false;
				IPooledEnumerable eable = map.GetItemsInRange(loc, 5);
				foreach (Item item in eable)
				{
					if (item is EcoNode || item is DungeonNode) { alreadyExists = true; break; }
				}
				eable.Free();

				if (alreadyExists) continue;

				// 4. [분류] 이름(cNode.Name)을 보고 에코로 할지 던전으로 할지 결정
				string pathName = GetGoGumpZoneName(loc, map);
				string lowerName = pathName.ToLower();

				bool isCombat = lowerName.Contains("cemetery") || lowerName.Contains("fort") || 
								lowerName.Contains("orc") || lowerName.Contains("camp") || 
								lowerName.Contains("dungeon");

				if (isCombat)
				{
					// 전투 구역이면 던전 노드 생성
					DungeonNode dNode = new() { ZoneId = pathName };
					dNode.MoveToWorld(loc, map);
				}
				else
				{
					// 일반 필드/마을이면 에코 노드 생성
					EcoNode eNode = new() { ZoneId = pathName };
					eNode.MoveToWorld(loc, map);
					ApplyDefaultSettings(eNode, eNode.ZoneId.ToLower());
				}
				addedCount++;
			}

			// 매니저 동기화
			EcosystemManager.Zones?.Values.ToList().ForEach(z => z?.CacheNodes());
			DungeonManager.Zones?.Values.ToList().ForEach(z => z?.CacheNodes());

			from.SendMessage(66, $"공백 채우기 완료: {addedCount}개의 새로운 노드가 GoGump 좌표에 배치되었습니다.");
		}

		// 트리 구조에서 ChildNode(좌표점)만 싹 긁어오는 헬퍼
		private static void ExtractChildNodes(ParentNode parent, List<ChildNode> list)
		{
			if (parent.Children == null) return;
			foreach (object obj in parent.Children)
			{
				if (obj is ChildNode child) list.Add(child);
				else if (obj is ParentNode p) ExtractChildNodes(p, list);
			}
		}

		// 기존 DoMigration의 마지막 줄에 FillGapsFromGoGump(from, map); 를 추가해서 연동하세요.
        #endregion
    }

    #region [Gumps]
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
            AddBackground(0, 0, 560, 640, 9270);
            AddAlphaRegion(10, 10, 540, 620);

            AddHtml(10, 15, 540, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>MASTER SPAWN MANAGER</BASEFONT></CENTER>", false, false);
            AddHtml(10, 45, 540, 20, $"<CENTER><BASEFONT COLOR='#88FFFF'>Resources: M:{miningCount} / L:{lumberCount} / F:{fishingCount} / A:{farmingCount}</BASEFONT></CENTER>", false, false);

            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            int y = 75; 

            for (int i = 0; i < maps.Length; i++)
            {
                AddImageTiled(20, y, 520, 38, 9354); 
                AddLabel(35, y + 9, 1152, maps[i].Name);
                
                AddButton(150, y + 7, 4005, 4007, (i * 10) + 1, GumpButtonType.Reply, 0); 
                AddLabel(185, y + 9, 0x481, "DUNGEON");

                AddButton(290, y + 7, 4023, 4025, (i * 10) + 2, GumpButtonType.Reply, 0); 
                AddLabel(325, y + 9, 0x481, "ECOLOGY");
                
                y += 42;
            }
            
            y += 5;
            AddImageTiled(20, y, 520, 38, 9354); 
            AddButton(35, y + 7, 4011, 4013, 999, GumpButtonType.Reply, 0); 
            AddLabel(75, y + 9, 0x35, "미매칭/에러 노드 리스트 (CHECK LIST - 던전 & 상인 & 생태계)");
            
            y += 45;
            AddImageTiled(20, y, 520, 40, 9354); 
            AddButton(25, y + 8, 4005, 4007, 998, GumpButtonType.Reply, 0); 
            AddLabel(60, y + 10, 0x42, "던전 모니터");

            AddButton(145, y + 8, 4023, 4025, 997, GumpButtonType.Reply, 0); 
            AddLabel(180, y + 10, 0x42, "생태계 모니터");

            AddButton(285, y + 8, 4011, 4013, 996, GumpButtonType.Reply, 0); 
            AddLabel(320, y + 10, 0x58, $"자원/농사");

            AddButton(410, y + 8, 4005, 4007, 995, GumpButtonType.Reply, 0); 
            AddLabel(445, y + 10, 68, "경제/상인"); 
            
            y = 480;
            AddImageTiled(20, y, 520, 140, 9354);
            AddHtml(25, y + 8, 510, 20, "<CENTER><BASEFONT COLOR='#FFFF00'>--- 서버 데이터 선택적 이식 및 초기화 ---</BASEFONT></CENTER>", false, false);
            
            AddLabel(100, y + 35, 1152, "전체(ALL)");
            AddLabel(250, y + 35, 0x481, "던전(DUNGEON)");
            AddLabel(420, y + 35, 68, "벤더(VENDOR)");

            AddButton(35, y + 55, 4011, 4013, 810, GumpButtonType.Reply, 0); AddLabel(70, y + 57, 1152, "Export");
            AddButton(210, y + 55, 4011, 4013, 811, GumpButtonType.Reply, 0); AddLabel(245, y + 57, 1152, "Export");
            AddButton(385, y + 55, 4011, 4013, 812, GumpButtonType.Reply, 0); AddLabel(420, y + 57, 1152, "Export");

            AddButton(35, y + 80, 4005, 4007, 820, GumpButtonType.Reply, 0); AddLabel(70, y + 82, 0x42, "Import");
            AddButton(210, y + 80, 4005, 4007, 821, GumpButtonType.Reply, 0); AddLabel(245, y + 82, 0x42, "Import");
            AddButton(385, y + 80, 4005, 4007, 822, GumpButtonType.Reply, 0); AddLabel(420, y + 82, 0x42, "Import");

            AddButton(35, y + 105, 4020, 4022, 803, GumpButtonType.Reply, 0); AddLabel(70, y + 107, 0x21, "Eco Reset!");
            AddButton(210, y + 105, 4020, 4022, 831, GumpButtonType.Reply, 0); AddLabel(245, y + 107, 0x21, "Dungeon Reset");
            AddButton(385, y + 105, 4020, 4022, 832, GumpButtonType.Reply, 0); AddLabel(420, y + 107, 0x21, "Vendor Reset");
        }
        
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int btn = info.ButtonID;
            if (btn == 0) return;

            if (btn >= 810 && btn <= 812) { NewSpawnManager.DoExport(sender.Mobile, btn - 810); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn >= 820 && btn <= 822) { NewSpawnManager.DoImport(sender.Mobile, btn - 820); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn == 803) { NewSpawnManager.DoResetAll(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn == 831) { NewSpawnManager.DoResetDungeonNodes(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (btn == 832) { NewSpawnManager.DoResetVendorNodes(sender.Mobile); sender.Mobile.SendGump(new NewSpawnGump()); return; }

            if (btn == 999)
            {
                List<Item> checkList = new List<Item>();
                checkList.AddRange(World.Items.Values.OfType<DungeonNode>().Where(n => n.Map == sender.Mobile.Map && !NewSpawnManager.IsManaged(n.ZoneId)));
                // 🌟 EcoNode 미매칭 검사 추가
                checkList.AddRange(World.Items.Values.OfType<EcoNode>().Where(n => n.Map == sender.Mobile.Map && !NewSpawnManager.IsManaged(n.ZoneId)));
                checkList.AddRange(World.Items.Values.OfType<VendorNode>().Where(v => v.Map == sender.Mobile.Map && v.TownID == 0));
                sender.Mobile.SendGump(new NodeCheckGump(checkList, 0));
                return;
            }
            
            if (btn == 998) { sender.Mobile.SendGump(new ZoneMonitorGump(0, 0)); return; } 
            if (btn == 997) { sender.Mobile.SendGump(new ZoneMonitorGump(1, 0)); return; } 
            if (btn == 996) { sender.Mobile.SendGump(new ZoneMonitorGump(2, 0)); return; } 

            if (btn == 995) 
            { 
                sender.Mobile.SendGump(new EconomyAdminGump(sender.Mobile, 0, 0, 0, 0)); 
                return; 
            } 

            int mapIdx = (btn / 10);
            int mode = (btn % 10); 
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            
            if (mapIdx < maps.Length)
            {
                NewSpawnManager.DoMigration(sender.Mobile, maps[mapIdx], mode - 1);
                sender.Mobile.SendGump(new NewSpawnGump());
            }
        }
    }

    public class NodeCheckGump : Gump
    {
        private List<Item> m_List;
        private int m_Page;

        public NodeCheckGump(List<Item> list, int page) : base(500, 100)
        {
            m_List = list; m_Page = page;
            AddPage(0);
            AddBackground(0, 0, 550, 550, 9270);
            AddHtml(10, 15, 530, 20, $"<CENTER><BASEFONT COLOR='#FF5555'>미매칭 노드 리스트 (총 {list.Count}개)</BASEFONT></CENTER>", false, false);
            
            int start = page * 10;
            int end = Math.Min(start + 10, list.Count);
            
            for (int i = start; i < end; i++)
            {
                Item n = list[i];
                string zoneId = "Unknown";
                string typeName = "Node";

                if (n is DungeonNode dn) { zoneId = dn.ZoneId; typeName = "DUNGEON"; }
                // 🌟 EcoNode 리스트 출력 지원
                else if (n is EcoNode en) { zoneId = en.ZoneId; typeName = "ECOSYSTEM"; }
                else if (n is VendorNode vn) { zoneId = TownNumber.GetName(vn.TownID); typeName = "VENDOR"; }

                int y = 50 + ((i - start) * 45);
                AddImageTiled(15, y, 520, 40, 9354);
                
                string desc = $"<BASEFONT COLOR='#FFFFFF'>[{typeName}] {zoneId}</BASEFONT>";
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
        private int m_Mode, m_SubMode, m_Page;

        public ZoneMonitorGump(int mode, int page) : this(mode, 0, page) { }

        public ZoneMonitorGump(int mode, int subMode, int page) : base(30, 50)
        {
            m_Mode = mode; m_SubMode = subMode; m_Page = page;
            
            AddPage(0);
            AddBackground(0, 0, 950, 500, 9270);
            AddImageTiled(10, 10, 930, 480, 2624);
            AddAlphaRegion(10, 10, 930, 480);
            
            AddHtml(10, 15, 930, 25, "<CENTER><BASEFONT COLOR='#FFFFFF' SIZE='6'>MASTER MONITOR</BASEFONT></CENTER>", false, false);
            AddButton(20, 15, 4014, 4016, 3, GumpButtonType.Reply, 0); AddLabel(55, 15, 1152, "MAIN");

            // 상단 탭 메뉴
            AddImageTiled(20, 50, 910, 30, 9354);
            AddButton(30, 55, mode == 0 ? 4006 : 4005, 4007, 10, GumpButtonType.Reply, 0); AddLabel(65, 55, mode == 0 ? 68 : 1152, "던전 모니터링");
            AddButton(200, 55, mode == 1 ? 4006 : 4005, 4007, 11, GumpButtonType.Reply, 0); AddLabel(235, 55, mode == 1 ? 68 : 1152, "생태계 모니터링");
            AddButton(370, 55, mode == 2 ? 4006 : 4005, 4007, 13, GumpButtonType.Reply, 0); AddLabel(405, 55, mode == 2 ? 68 : 1152, "자원 생태계 모니터링");
            AddButton(820, 55, 4011, 4012, 12, GumpButtonType.Reply, 0); AddLabel(855, 55, 0xFFFFFF, "새로고침");

            int y = 95;

            // 자원 모드 서부 필터
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
            int totalListCount = 0;

			if (mode == 0) // 던전 모니터링
			{
				// 헤더 출력 (기존과 동일)
				AddHtml(25, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>컨트롤</BASEFONT>", false, false);
				AddHtml(200, y, 250, 20, "<BASEFONT COLOR='#FFFF00'>던전 구역명</BASEFONT>", false, false);
				AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>상태</BASEFONT>", false, false);
				AddHtml(560, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>난이도</BASEFONT>", false, false);
				AddHtml(710, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>인구 설정</BASEFONT>", false, false);
				y += 25;

				var list = DungeonManager.Zones.Values.ToList(); // List<DungeonZone> 명시적 할당
				totalListCount = list.Count;
				int end = Math.Min(start + 10, totalListCount);

				for (int i = start; i < end; i++)
				{
					var z = list[i]; // DungeonZone 형식으로 직접 접근
					AddImageTiled(20, y - 2, 910, 24, 9354);

					if (z.Nodes != null && z.Nodes.Count > 0)
					{
						AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
						AddLabel(55, y, 1152, $"GO({z.Nodes.Count})");

						if (z.Nodes.Count > 1)
						{
							AddButton(110, y + 2, 4017, 4018, 600 + (i - start), GumpButtonType.Reply, 0);
							AddLabel(145, y, 0x35, "정리");
						}
					}
					else
					{
						AddButton(25, y + 2, 4011, 4013, 400 + (i - start), GumpButtonType.Reply, 0);
						AddLabel(55, y, 33, "생성");
						AddButton(110, y + 2, 4005, 4007, 500 + (i - start), GumpButtonType.Reply, 0);
						AddLabel(145, y, 1152, "찾기");
					}

					AddLabel(200, y, 0xFFFFFF, z.ZoneId.Length > 30 ? z.ZoneId.Substring(0, 30) + "..." : z.ZoneId);

					int phaseColor = 0xFFFFFF; string phaseText = "";
					if (z.MaxPopulation == 0) { phaseColor = 33; phaseText = "잠금됨"; }
					else if (z.Phase == DungeonPhase.Active) { phaseColor = 68; phaseText = "사냥 중"; }
					else if (z.Phase == DungeonPhase.BossSpawned) { phaseColor = 33; phaseText = "보스 등장!"; }
					else if (z.Phase == DungeonPhase.Cooldown) { phaseColor = 1359; phaseText = "휴식기"; }
					AddLabel(450, y, phaseColor, phaseText);

					double diffPercent = z.MaxDifficulty > 0 ? (double)z.CurrentDifficulty / z.MaxDifficulty : 0;
					AddLabel(560, y, diffPercent > 0.5 ? 68 : 33, $"{z.CurrentDifficulty:N0} / {z.MaxDifficulty:N0}");
					AddLabel(710, y, 0xFFFFFF, $"{z.GetTotalActiveCount()} /");

					AddImageTiled(780, y - 1, 55, 22, 2624); AddAlphaRegion(780, y - 1, 55, 22); 
					AddTextEntry(785, y, 45, 20, 53, i - start, z.ManualMaxPopulation >= 0 ? z.ManualMaxPopulation.ToString() : z.MaxPopulation.ToString()); 
					AddButton(840, y + 2, 4023, 4025, 200 + (i - start), GumpButtonType.Reply, 0); 
					AddLabel(875, y, 68, "SET");

					y += 30;
				}
			}
			else if (mode == 1) // 생태계 모니터링
			{
				// 헤더 출력 (기존과 동일)
				AddHtml(25, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>컨트롤</BASEFONT>", false, false);
				AddHtml(200, y, 250, 20, "<BASEFONT COLOR='#FFFF00'>생태계 구역명</BASEFONT>", false, false);
				AddHtml(450, y, 100, 20, "<BASEFONT COLOR='#FFFF00'>상태</BASEFONT>", false, false);
				AddHtml(560, y, 140, 20, "<BASEFONT COLOR='#FFFF00'>개체수</BASEFONT>", false, false);
				AddHtml(710, y, 180, 20, "<BASEFONT COLOR='#FFFF00'>평균 활력</BASEFONT>", false, false);
				y += 25;

				var list = EcosystemManager.Zones.Values.ToList(); // List<EcoZone> 명시적 할당
				totalListCount = list.Count;
				int end = Math.Min(start + 10, totalListCount);

				for (int i = start; i < end; i++)
				{
					var z = list[i]; // EcoZone 형식으로 직접 접근
					AddImageTiled(20, y - 2, 910, 24, 9354);

					if (z.Nodes != null && z.Nodes.Count > 0)
					{
						AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
						AddLabel(55, y, 1152, $"GO({z.Nodes.Count})");

						if (z.Nodes.Count > 1)
						{
							AddButton(110, y + 2, 4017, 4018, 600 + (i - start), GumpButtonType.Reply, 0);
							AddLabel(145, y, 0x35, "정리");
						}
					}
					else
					{
						AddButton(25, y + 2, 4011, 4013, 400 + (i - start), GumpButtonType.Reply, 0);
						AddLabel(55, y, 33, "생성");
						AddButton(110, y + 2, 4005, 4007, 500 + (i - start), GumpButtonType.Reply, 0);
						AddLabel(145, y, 1152, "찾기");
					}

					AddLabel(200, y, 0xFFFFFF, z.ZoneId.Length > 30 ? z.ZoneId.Substring(0, 30) + "..." : z.ZoneId);

					int totalActive = 0, totalMax = 0, avgVitality = 0;
					foreach (var s in z.SpeciesInfo.Values) { totalActive += s.ActiveAnimals.Count; totalMax += s.MaxPopulation; avgVitality += s.Vitality; }
					if (z.SpeciesInfo.Count > 0) avgVitality /= z.SpeciesInfo.Count;

					AddLabel(450, y, 0xFFFFFF, $"{z.SpeciesInfo.Count} 종 관리");
					AddLabel(560, y, totalActive >= totalMax ? 33 : 0xFFFFFF, $"{totalActive} / {totalMax}");
					AddLabel(710, y, avgVitality > 8000 ? 68 : 33, $"활력 {avgVitality / 100.0:F1}%");

					y += 30;
				}
			}
            else if (mode == 2) // 자원 생태계 모드
            {
                AddHtml(25, y, 40, 20, "<BASEFONT COLOR='#FFFF00'>이동</BASEFONT>", false, false);
                AddHtml(70, y, 80, 20, "<BASEFONT COLOR='#FFFF00'>종류</BASEFONT>", false, false);
                AddHtml(160, y, 200, 20, "<BASEFONT COLOR='#FFFF00'>구역명</BASEFONT>", false, false);
                AddHtml(400, y, 150, 20, "<BASEFONT COLOR='#FFFF00'>잔여량</BASEFONT>", false, false);
                AddHtml(560, y, 300, 20, "<BASEFONT COLOR='#FFFF00'>상태 및 재료</BASEFONT>", false, false);
                y += 25;

                var list = ResourceManager.Pools.Values.ToList();
                if (m_SubMode > 0)
                {
                    ResourceType targetType = m_SubMode == 1 ? ResourceType.Mining : m_SubMode == 2 ? ResourceType.Lumberjacking : m_SubMode == 3 ? ResourceType.Fishing : ResourceType.Farming;
                    list = list.Where(p => p.Type == targetType).ToList();
                }

                totalListCount = list.Count;
                int end = Math.Min(start + 10, totalListCount);

                for (int i = start; i < end; i++)
                {
                    var pool = list[i];
                    AddImageTiled(20, y - 2, 910, 24, 9354);
                    AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);

                    double percent = pool.MaxCapacity > 0 ? ((double)pool.CurrentCapacity / pool.MaxCapacity) * 100.0 : 0;
                    int color = percent < 50.0 ? 33 : (percent > 90.0 ? 68 : 0xFFFFFF);

                    AddLabel(70, y, color, pool.Type.ToString());
                    AddLabel(160, y, color, pool.RegionName.Length > 25 ? pool.RegionName.Substring(0, 25) : pool.RegionName);
                    AddLabel(400, y, color, string.Format("{0}/{1} ({2:F0}%)", pool.CurrentCapacity, pool.MaxCapacity, percent));

                    TimeSpan cd = pool.DepletionCooldown - DateTime.Now;
                    if (cd.TotalSeconds > 0) AddHtml(560, y, 350, 20, string.Format("<BASEFONT COLOR='#FF3333'>고갈 ({0:F1}분)</BASEFONT>", cd.TotalMinutes), false, false);
                    else AddHtml(560, y, 350, 20, "<BASEFONT COLOR='#42FF42'>정상 스폰 중</BASEFONT>", false, false);

                    y += 30;
                }
            }

            // 하단 페이지네이션
            if (m_Page > 0) AddButton(20, 450, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddLabel(460, 450, 0xFFFFFF, string.Format("{0} / {1}", m_Page + 1, Math.Max(1, (totalListCount - 1) / 10 + 1)));
            if ((start + 10) < totalListCount) AddButton(900, 450, 4005, 4007, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (info.ButtonID == 0 || info.ButtonID == 3) { from.SendGump(new NewSpawnGump()); return; }
            if (info.ButtonID >= 50 && info.ButtonID <= 54) { from.SendGump(new ZoneMonitorGump(m_Mode, info.ButtonID - 50, 0)); return; }

            switch (info.ButtonID)
            {
                case 1: from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page - 1)); return;
                case 2: from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page + 1)); return;
                case 10: from.SendGump(new ZoneMonitorGump(0, 0, 0)); return; 
                case 11: from.SendGump(new ZoneMonitorGump(1, 0, 0)); return; 
                case 13: from.SendGump(new ZoneMonitorGump(2, 0, 0)); return; 
                case 12: 
                    foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
                    foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();
                    from.SendMessage(68, "전체 노드 데이터가 동기화되었습니다.");
                    from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page)); 
                    return; 
            }

            int actualIndex = (m_Page * 10) + (info.ButtonID % 100);

            // 300번대: 순회 이동 (GoToNextNode)
            if (info.ButtonID >= 300 && info.ButtonID < 310) 
            {
                if (m_Mode == 0) { var list = DungeonManager.Zones.Values.ToList(); if (actualIndex < list.Count) list[actualIndex].GoToNextNode(from); }
                else if (m_Mode == 1) { var list = EcosystemManager.Zones.Values.ToList(); if (actualIndex < list.Count) list[actualIndex].GoToNextNode(from); }
                else if (m_Mode == 2) { /* 자원 구역 이동 로직 기존과 동일 */ }
                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
            // 400번대: 노드 가방 생성
            else if (info.ButtonID >= 400 && info.ButtonID < 410)
            {
                string zid = "";
                if (m_Mode == 0) { var list = DungeonManager.Zones.Values.ToList(); if (actualIndex < list.Count) { zid = list[actualIndex].ZoneId; from.AddToBackpack(new DungeonNode { ZoneId = zid }); } }
                else if (m_Mode == 1) { var list = EcosystemManager.Zones.Values.ToList(); if (actualIndex < list.Count) { zid = list[actualIndex].ZoneId; from.AddToBackpack(new EcoNode { ZoneId = zid }); } }
                if (!string.IsNullOrEmpty(zid)) from.SendMessage(68, string.Format("{0} 노드를 가방에 생성했습니다.", zid));
                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
            // 500번대: 역추적 텔레포트 (노드 실종 시)
            else if (info.ButtonID >= 500 && info.ButtonID < 510)
            {
                if (m_Mode == 0 || m_Mode == 1)
                {
                    string zid = ""; Map facet = null;
                    if (m_Mode == 0) { var list = DungeonManager.Zones.Values.ToList(); if (actualIndex < list.Count) { zid = list[actualIndex].ZoneId; facet = list[actualIndex].Facet; } }
                    else { var list = EcosystemManager.Zones.Values.ToList(); if (actualIndex < list.Count) { zid = list[actualIndex].ZoneId; facet = list[actualIndex].Facet; } }
                    Point3D? loc = NewSpawnManager.FindLocationByZoneId(zid, facet);
                    if (loc.HasValue) from.MoveToWorld(loc.Value, facet);
                }
                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
            // 600번대: 현재 노드 제외 중복 삭제 (정리)
            else if (info.ButtonID >= 600 && info.ButtonID < 610)
            {
                if (m_Mode == 0) { var list = DungeonManager.Zones.Values.ToList(); if (actualIndex < list.Count) list[actualIndex].KeepCurrentNodeOnly(from); }
                else if (m_Mode == 1) { var list = EcosystemManager.Zones.Values.ToList(); if (actualIndex < list.Count) list[actualIndex].KeepCurrentNodeOnly(from); }
                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
            // 200번대: 인구 설정
            else if (info.ButtonID >= 200 && info.ButtonID < 210 && m_Mode == 0) 
            {
                var list = DungeonManager.Zones.Values.ToList();
                if (actualIndex < list.Count)
                {
                    TextRelay relay = info.GetTextEntry(info.ButtonID - 200);
                    if (relay != null && int.TryParse(relay.Text, out int newPop)) list[actualIndex].SetPopulation(newPop);
                }
                from.SendGump(new ZoneMonitorGump(m_Mode, m_SubMode, m_Page));
            }
        }
    }
    #endregion
}