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
            int dCount = 0, vCount = 0, popCount = 0;

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

            from.SendMessage(68, $"[Export 완료] 대상: {(mode == 0 ? "전체" : mode == 1 ? "던전" : "벤더")} (D:{dCount} / V:{vCount} / Pop:{popCount})");
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

        #region [Reset & Migration System]
        public static void DoResetDungeonNodes(Mobile from)
        {
            int count = 0;
            var toDelete = World.Items.Values.OfType<DungeonNode>().ToList();
            foreach (var item in toDelete) { item.Delete(); count++; }
            
            foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
            from.SendMessage(33, $"[리셋 완료] 총 {count}개의 던전/생태계 노드를 삭제했습니다.");
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
            DoResetDungeonNodes(from);
            DoResetVendorNodes(from);
            from.SendMessage(33, "전 세계의 모든 스폰 노드 및 관련 데이터가 초기화되었습니다.");
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
            AddLabel(75, y + 9, 0x35, "미매칭/에러 노드 리스트 (CHECK LIST - 던전 & 상인)");
            
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

            AddButton(35, y + 105, 4020, 4022, 803, GumpButtonType.Reply, 0); AddLabel(70, y + 107, 0x21, "ALL Reset!");
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

            AddImageTiled(20, 50, 910, 30, 9354);
            AddButton(30, 55, mode == 0 ? 4006 : 4005, 4007, 10, GumpButtonType.Reply, 0); AddLabel(65, 55, mode == 0 ? 68 : 1152, "던전 모니터링");
            AddButton(200, 55, mode == 1 ? 4006 : 4005, 4007, 11, GumpButtonType.Reply, 0); AddLabel(235, 55, mode == 1 ? 68 : 1152, "생태계 모니터링");
            AddButton(370, 55, mode == 2 ? 4006 : 4005, 4007, 13, GumpButtonType.Reply, 0); AddLabel(405, 55, mode == 2 ? 68 : 1152, "자원 생태계 모니터링");
            AddButton(820, 55, 4011, 4012, 12, GumpButtonType.Reply, 0); AddLabel(855, 55, 0xFFFFFF, "새로고침");

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
                    if (z.Nodes != null && z.Nodes.Count > 0) AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
                    else AddLabel(25, y, 33, "X"); 

                    AddLabel(60, y, 0xFFFFFF, z.ZoneId.Length > 55 ? z.ZoneId.Substring(0, 55) + "..." : z.ZoneId);
                    
                    int phaseColor = 0xFFFFFF; string phaseText = "";
                    if (z.MaxPopulation == 0) { phaseColor = 33; phaseText = "잠금됨 (Locked)"; }
                    else if (z.Phase == DungeonPhase.Active) { phaseColor = 68; phaseText = "사냥 중"; }
                    else if (z.Phase == DungeonPhase.BossSpawned) { phaseColor = 33; phaseText = "보스 등장!"; }
                    else if (z.Phase == DungeonPhase.Cooldown) { phaseColor = 1359; phaseText = "휴식기"; }
                    AddLabel(450, y, phaseColor, phaseText);
                    
                    double diffPercent = z.MaxDifficulty > 0 ? (double)z.CurrentDifficulty / z.MaxDifficulty : 0;
                    int diffColor = diffPercent > 0.5 ? 68 : (diffPercent > 0.2 ? 53 : 33);
                    AddLabel(560, y, diffColor, $"{z.CurrentDifficulty:N0} / {z.MaxDifficulty:N0}");
                    AddLabel(710, y, 0xFFFFFF, $"{z.GetTotalActiveCount()} /");
                    
                    AddImageTiled(750, y - 1, 55, 22, 2624); AddAlphaRegion(750, y - 1, 55, 22); 
                    AddTextEntry(755, y, 45, 20, 53, i - start, z.ManualMaxPopulation >= 0 ? z.ManualMaxPopulation.ToString() : z.MaxPopulation.ToString()); 
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
                    if (z.Nodes != null && z.Nodes.Count > 0) AddButton(25, y + 2, 4005, 4007, 300 + (i - start), GumpButtonType.Reply, 0);
                    else AddLabel(25, y, 33, "X");

                    AddLabel(60, y, 0xFFFFFF, z.ZoneId.Length > 55 ? z.ZoneId.Substring(0, 55) + "..." : z.ZoneId);
                    
                    int totalSpecies = z.SpeciesInfo.Count;
                    int totalActive = z.SpeciesInfo.Values.Sum(s => s.ActiveAnimals.Count);
                    int totalMax = z.SpeciesInfo.Values.Sum(s => s.MaxPopulation);
                    int avgVitality = totalSpecies > 0 ? z.SpeciesInfo.Values.Sum(s => s.Vitality) / totalSpecies : 0;

                    AddLabel(450, y, 0xFFFFFF, $"{totalSpecies} 가지 종");
                    int popColor = (totalMax > 0 && ((double)totalActive / totalMax) >= 1.0) ? 33 : 0xFFFFFF; 
                    AddLabel(560, y, popColor, $"{totalActive:N0} / {totalMax:N0} 마리");
                    AddLabel(750, y, avgVitality > 8000 ? 68 : (avgVitality > 3000 ? 53 : 33), $"{avgVitality / 100.0:F1}%");
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
                    ResourceType targetType = m_SubMode == 1 ? ResourceType.Mining : m_SubMode == 2 ? ResourceType.Lumberjacking : m_SubMode == 3 ? ResourceType.Fishing : ResourceType.Farming;
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

                    if (pool.Type == ResourceType.Farming)
                    {
                        int pending = FarmingSystem.GetPendingCount(pool.RegionName); 
                        string cropInfo = pool.RegionName.StartsWith("PrivateFarm") ? "유저 개인 작물" : "야생 작물";
                        string n = pool.RegionName.ToLower();
                        if (n.Contains("wheat")) cropInfo = "밀"; else if (n.Contains("carrot")) cropInfo = "당근"; else if (n.Contains("corn")) cropInfo = "옥수수"; else if (n.Contains("cotton")) cropInfo = "목화";
                        
                        string resStatus = $"자라는 중 [{cropInfo}]";
                        if (pending > 0) resStatus += $" <BASEFONT COLOR='#FF8888'>+ 새끼({pending})</BASEFONT>"; 
                        AddHtml(480, y, 440, 20, $"<BASEFONT COLOR='#42FF42'>{resStatus}</BASEFONT>", false, false); 
                    }
                    else
                    {
                        string materialName = "전체 고갈";
                        if (pool.AvailableResources != null && pool.AvailableResources.Count > 0)
                        {
                            var activeRes = pool.AvailableResources.Where(k => k.Value > 0).Select(k => k.Key.Name).ToList();
                            if (activeRes.Count > 0) materialName = string.Join(", ", activeRes);
                            if (materialName.Length > 28) materialName = materialName.Substring(0, 25) + "...";
                        }

                        TimeSpan cd = pool.DepletionCooldown - DateTime.Now;
                        if (cd.TotalSeconds > 0) AddHtml(480, y, 440, 20, $"<BASEFONT COLOR='#FF3333'>고갈됨 ({cd.TotalMinutes:F1}분 후)</BASEFONT> <BASEFONT COLOR='#AAAAAA'>[{materialName}]</BASEFONT>", false, false);
                        else AddHtml(480, y, 440, 20, $"<BASEFONT COLOR='#42FF42'>정상 스폰 중</BASEFONT> <BASEFONT COLOR='#AAAAAA'>[{materialName}]</BASEFONT>", false, false);
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
            if (info.ButtonID == 0 || info.ButtonID == 3) { sender.Mobile.SendGump(new NewSpawnGump()); return; }
            if (info.ButtonID >= 50 && info.ButtonID <= 54) { sender.Mobile.SendGump(new ZoneMonitorGump(m_Mode, info.ButtonID - 50, 0)); return; }

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
                    if (actualIndex < list.Count && list[actualIndex].Nodes.Count > 0) sender.Mobile.MoveToWorld(list[actualIndex].Nodes[0].Location, list[actualIndex].Nodes[0].Map);
                }
                else if (m_Mode == 1) 
                {
                    var list = EcosystemManager.Zones.Values.ToList();
                    if (actualIndex < list.Count && list[actualIndex].Nodes.Count > 0) sender.Mobile.MoveToWorld(list[actualIndex].Nodes[0].Location, list[actualIndex].Nodes[0].Map);
                }
                else if (m_Mode == 2) 
                {
                    var list = ResourceManager.Pools.Values.ToList();
                    if (m_SubMode > 0)
                    {
                        ResourceType tType = m_SubMode == 1 ? ResourceType.Mining : m_SubMode == 2 ? ResourceType.Lumberjacking : m_SubMode == 3 ? ResourceType.Fishing : ResourceType.Farming;
                        list = list.Where(p => p.Type == tType).ToList();
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
                                int x = int.Parse(parts[1]) * size + (size / 2); int y = int.Parse(parts[2]) * size + (size / 2);
                                sender.Mobile.MoveToWorld(new Point3D(x, y, map.GetAverageZ(x, y)), map);
                            }
                        }
                        else
                        {
                            Region r = ResourceManager.GetRegionByName(pool.RegionName, map);
                            if (r != null && r.Area.Length > 0)
                            {
                                var a = r.Area[0];
                                sender.Mobile.MoveToWorld(new Point3D(a.Start.X + (a.End.X - a.Start.X) / 2, a.Start.Y + (a.End.Y - a.Start.Y) / 2, map.GetAverageZ(a.Start.X, a.Start.Y)), map);
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
    #endregion
}
