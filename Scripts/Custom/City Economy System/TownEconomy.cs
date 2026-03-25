using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    // =========================================================================
    // 1. Data Transfer Objects (DTO) & Enums
    // =========================================================================
    public class TownInventoryEntry
    {
        public Type ItemType { get; set; }
        public int InitialStock { get; set; }
        public int BasePrice { get; set; }
        public TownInventoryEntry(Type type, int stock, int price)
        {
            ItemType = type; InitialStock = stock; BasePrice = price;
        }
    }

    public class WarehouseItem
    {
        public Type ItemType { get; set; }
        public int Stock { get; set; }
        public int BasePrice { get; set; }
        public int TargetStock { get; set; } // 가격 변동의 기준이 되는 최초 스냅샷 수량

        // [추가] 1. 외부 스크립트 호환용 (3개만 보낼 때)
        // 외부에서 3개만 보낼 경우, 현재 들어오는 수량(stock)을 최초 스냅샷(TargetStock)으로 자동 세팅합니다.
        public WarehouseItem(Type type, int stock, int price)
        {
            ItemType = type; 
            Stock = stock; 
            BasePrice = price; 
            TargetStock = stock; // 3개만 들어오면 현재 수량을 4번째 기준으로 삼음
        }

        // 2. 세이브/로드용 원본 생성자 (4개 다 보낼 때)
        public WarehouseItem(Type type, int stock, int price, int targetStock)
        {
            ItemType = type; 
            Stock = stock; 
            BasePrice = price; 
            TargetStock = targetStock;
        }
    }

    // =========================================================================
    // 2. TownEconomy: 개별 도시의 자산 및 공용 창고
    // =========================================================================
    public class TownEconomy
    {
        private List<BaseVendor> m_ActiveVendors = [];
        private int m_TownID;
        private long m_Wealth;

        public string Name => TownNumber.GetName(this.TownID);

        [CommandProperty(AccessLevel.GameMaster)]
        public int TownID
        {
            get => m_TownID;
            set
            {
                m_TownID = value;
                int logicID = value / 100;

                Facet = logicID switch {
                    0 => Map.Trammel,  
                    1 => Map.Felucca,  
                    2 => Map.Ilshenar,
                    3 => Map.Malas,
                    4 => Map.Tokuno,
                    5 => Map.TerMur,
                    _ => Map.Trammel
                };
                TownName = TownNumber.GetName(value);
            }
        }

        public void ClearVendors()
        {
            m_ActiveVendors.Clear();
            this.VendorCount = 0;
        }

        [CommandProperty(AccessLevel.GameMaster)] public string TownName { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public Map Facet { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public Point3D Center { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public string TownIndex { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public TownType Type { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public bool IsOfficialTown { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public int TotalTiles => TownNumber.GetTotalTiles(this.TownID);

        [CommandProperty(AccessLevel.GameMaster)] public long Wealth { get => m_Wealth; set => m_Wealth = value; }
        [CommandProperty(AccessLevel.GameMaster)] public long BaseWealth { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public long TaxFund { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public int VendorCount { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public int MaxInventoryCapacity { get; set; } = 2000;

        public long TotalWealth => Wealth; 

        public List<VirtualCitizen> Citizens { get; set; } = [];
        public List<VirtualHouse> Houses { get; set; } = [];
        public Dictionary<Type, WarehouseItem> Warehouse { get; set; } = [];
        public List<TownInventoryEntry> InventoryEntries { get; set; } = [];
        public Dictionary<NpcJobClass, double> JobBirthWeights { get; set; } = [];

        public long Platinum { get => Wealth / 100000000; set => Wealth = value * 100000000; }
        public long ExtraGold => Wealth % 100000000;
        public string TotalWealthString => $"{Platinum}P {ExtraGold:N0}g";
        
		// 1. [Macro] 마을 전체 경제 변동치 (-0.5 ~ +1.0)
        // 자본이 반토막(-50%)나면 -0.5, 2배(+100%)면 +1.0
        [CommandProperty(AccessLevel.GameMaster)]
        public double MacroModifier => Math.Clamp((double)Wealth / Math.Max(1, BaseWealth) - 1.0, -0.5, 1.0);

        // 2. [복구] 외부 스크립트 참조용 전체 물가 배율 (1.0 + 변동치)
        // 자본 상태에 따라 0.5x ~ 2.0x를 반환합니다. (Gump 및 리포트 연동)
        [CommandProperty(AccessLevel.GameMaster)]
        public double PriceMultiplier => 1.0 + MacroModifier;

        // 특수 이벤트 및 치안 변동치
        public double EventPriceModifier { get; set; } = 0.0;
        public double SecurityPriceModifier { get; set; } = 0.0;

        // =========================================================================
        // --- 최종 가격 산출 (1.0 베이스 합산 방식) ---
        // =========================================================================
        public int GetPrice(Type type, double externalMultiplier = 1.0)
        {
            // [코딩 규칙] out 키워드 금지 적용
            if (!Warehouse.ContainsKey(type)) return 100;
            var item = Warehouse[type];

            // 1. [Macro] 마을 자본금 변동 (-0.5 ~ +1.0)
            double macroMod = Math.Clamp((double)Wealth / Math.Max(1, BaseWealth) - 1.0, -0.5, 1.0);

            // 2. [Micro] 재고 변동 (사용자님 기획: 2배수 시 -25% 직결)
            double snapshot = Math.Max(1, item.TargetStock);
            double ratio = (double)item.Stock / snapshot;

            // 공식: (1.0 - ratio) * 0.25
            // - 재고 1배(정상): (1-1) * 0.25 = 0%
            // - 재고 2배(과잉): (1-2) * 0.25 = -25%
            // - 재고 0배(품절): (1-0) * 0.25 = +25%
            double microMod = Math.Clamp((1.0 - ratio) * 0.25, -0.25, 0.25);

            // 3. [최종 합산] 1.0 + 거시 + 미시
            double finalFactor = 1.0 + macroMod + microMod + EventPriceModifier + SecurityPriceModifier;

            // [기획 3] C등급(전초기지) 물가 25% 할증 프리미엄 적용
            if (TownID >= 900 || TownIndex == "C")
            {
                finalFactor *= 1.25; 
            }

            // 최소 25% 하한선 보장
            return (int)(item.BasePrice * Math.Max(0.25, finalFactor) * externalMultiplier);
        }
        public long WarehouseValue => Warehouse.Values.Sum(i => (long)i.Stock * i.BasePrice);
        public long ActualTotalWealth => Wealth + WarehouseValue;

        public TownEconomy(int townID, long baseWealth)
        {
            TownID = townID;
            BaseWealth = baseWealth;
            Wealth = baseWealth;
        }

        public void UpdateBaseWealth()
        {
            // [기획 2] C등급(전초기지) 자본금 처리: 영토 면적(TotalTiles) 배제
            // TownID가 900번대 이상이거나 TownIndex가 "C"인 경우로 식별합니다.
            if (TownID >= 900 || TownIndex == "C")
            {
                // 상인 수당 15,000 gp의 영세한 기본 자본금만 책정 (아이템/면적 거품 제거)
                this.BaseWealth = this.VendorCount * 15000;
                return;
            }

            // --- 기존 대도시 로직 ---
            long totalStock = Warehouse.Values.Sum(i => (long)i.Stock);
            long areaValue = (long)this.TotalTiles * 500;
            long humanValue = (long)this.VendorCount * 25000;
            long stockValue = totalStock * 25;

            long baseSum = areaValue + humanValue + stockValue;
            this.BaseWealth = TownID > 0 ? baseSum * 2 : baseSum;
        }

        public void InitInitialStock(Type itemType, int amount, int basePrice)
        {
            if (Warehouse.ContainsKey(itemType)) return;
            // [핵심] 최초 등록 시 수량(amount)을 TargetStock으로 영구 기록합니다.
            Warehouse[itemType] = new WarehouseItem(itemType, amount, basePrice, amount);
        }

        public void RegisterVendor(BaseVendor v)
        {
            if (v != null && !m_ActiveVendors.Contains(v))
            {
                m_ActiveVendors.Add(v);
                VendorCount = m_ActiveVendors.Count;
            }
        }

        public void SupplyItem(params object[] args) { }

        public void Serialize(GenericWriter writer)
		{
			writer.Write((int)8); // 버전 8: 가문(House) 및 가족(Family) 데이터 직렬화 추가
			writer.Write(TownID);
			writer.Write(Wealth);
			writer.Write(BaseWealth);
			writer.Write(VendorCount);
			
			// 1. 창고 저장
			writer.Write(Warehouse.Count);
			foreach (var kvp in Warehouse)
			{
				writer.Write(kvp.Key.FullName);
				writer.Write(kvp.Value.Stock);
				writer.Write(kvp.Value.BasePrice);
				writer.Write(kvp.Value.TargetStock);
			}

			// 2. 시민 목록 저장
			writer.Write(Citizens.Count);
			foreach (var citizen in Citizens)
			{
				citizen.Serialize(writer);
			}

			// 3. [신규] 가문(VirtualHouse) 저장
			writer.Write(Houses.Count);
			foreach (var house in Houses)
			{
				writer.Write(house.HouseName);
				writer.Write(house.Prestige);
				writer.Write(house.TotalWealth);
				writer.Write((int)house.PrimaryRank);

				// 4. 가족(FamilyUnit) 및 관계도(Index) 저장
				writer.Write(house.Families.Count);
				foreach (var family in house.Families)
				{
					// 부모의 Index 저장 (리스트에 없으면 -1)
					writer.Write(Citizens.IndexOf(family.Father));
					writer.Write(Citizens.IndexOf(family.Mother));
					writer.Write(family.SharedWealth);
					writer.Write(family.Prestige);

					// 자식들의 Index 저장
					writer.Write(family.Children.Count);
					foreach (var child in family.Children)
					{
						writer.Write(Citizens.IndexOf(child));
					}
				}
			}
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();
			TownID = reader.ReadInt();
			TownName = TownNumber.GetName(TownID);
			Wealth = reader.ReadLong();
			BaseWealth = reader.ReadLong();
			VendorCount = reader.ReadInt();

			// 1. 창고 복구
			int count = reader.ReadInt();
			for (int i = 0; i < count; i++)
			{
				Type type = ScriptCompiler.FindTypeByFullName(reader.ReadString());
				int stock = reader.ReadInt();
				int price = reader.ReadInt();
				int targetStock = version >= 7 ? reader.ReadInt() : stock;

				if (type != null) Warehouse[type] = new WarehouseItem(type, stock, price, targetStock);
			}

			// 2. 시민 복구
			if (version >= 6)
			{
				int citizenCount = reader.ReadInt();
				for (int i = 0; i < citizenCount; i++)
				{
					Citizens.Add(new VirtualCitizen(reader)); 
				}
			}

			// 3. [신규] 가문 및 가족 관계도 복원 (버전 8 이상)
			if (version >= 8)
			{
				int houseCount = reader.ReadInt();
				for (int i = 0; i < houseCount; i++)
				{
					string hName = reader.ReadString();
					int hPrestige = reader.ReadInt();
					long hWealth = reader.ReadLong();
					NobilityRank hRank = (NobilityRank)reader.ReadInt();

					VirtualHouse house = new VirtualHouse(hName, hRank) { Prestige = hPrestige, TotalWealth = hWealth };
					Houses.Add(house);

					int familyCount = reader.ReadInt();
					for (int j = 0; j < familyCount; j++)
					{
						int fatherIdx = reader.ReadInt();
						int motherIdx = reader.ReadInt();
						long fWealth = reader.ReadLong();
						int fPrestige = reader.ReadInt();

						// Index를 역추적하여 실제 시민 객체와 매핑
						VirtualCitizen father = (fatherIdx >= 0 && fatherIdx < Citizens.Count) ? Citizens[fatherIdx] : null;
						VirtualCitizen mother = (motherIdx >= 0 && motherIdx < Citizens.Count) ? Citizens[motherIdx] : null;

						FamilyUnit family = new FamilyUnit(father, mother) { SharedWealth = fWealth, Prestige = fPrestige };
						house.Families.Add(family);

						// [중요] 시민 객체의 소속(House, Family) 양방향 연결 복구
						if (father != null) { father.Family = family; father.House = house; }
						if (mother != null) { mother.Family = family; mother.House = house; }

						int childCount = reader.ReadInt();
						for (int k = 0; k < childCount; k++)
						{
							int childIdx = reader.ReadInt();
							if (childIdx >= 0 && childIdx < Citizens.Count)
							{
								VirtualCitizen child = Citizens[childIdx];
								family.Children.Add(child);
								
								// 자식 객체 양방향 연결
								child.Family = family;
								child.House = house;
							}
						}
					}
				}
			}
		}
    }

    // =========================================================================
    // 3. TownEconomyManager: 전체 경제 통제 및 세이브/로드 엔진
    // =========================================================================
    public static class TownEconomyManager
    {
        public static Dictionary<int, TownEconomy> Towns = [];
        private static bool m_IsLoaded = false;

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string dir = Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "TownEconomyData.bin");

            try {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    BinaryFileWriter writer = new BinaryFileWriter(fs, true);
                    
                    Console.WriteLine($"[Economy Save] {Towns.Count}개의 도시 데이터를 저장합니다.");
					Console.WriteLine($"[Economy Save] {Towns.Count}\uAC1C \uB9C8\uC744 \uB370\uC774\uD130 \uC800\uC7A5 \uC644\uB8CC.");
                    writer.Write((int)1); 
                    writer.Write(Towns.Count);
                    foreach (var kvp in Towns) {
                        writer.Write(kvp.Key);
                        kvp.Value.Serialize(writer);
                    }
                    writer.Flush();
                    writer.Close();
                }
            } catch (Exception ex) { Console.WriteLine($"[Economy Save Error] {ex.Message}"); }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "TownEconomyData.bin");
            
            if (File.Exists(path)) {
                try {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        GenericReader reader = new BinaryFileReader(new BinaryReader(fs));
                        int version = reader.ReadInt();
                        int count = reader.ReadInt();
                        
                        for (int i = 0; i < count; i++) {
                            int townID = reader.ReadInt();
                            
                            TownEconomy town;
                            if (Towns.ContainsKey(townID))
                                town = Towns[townID];
                            else
                            {
                                town = new TownEconomy(townID, 0); 
                                Towns[townID] = town;
                            }
                            
                            town.Deserialize(reader);
                        }
                    }
                    Console.WriteLine($"[Economy] {Towns.Count}개 도시 	로드 완료.");
                } catch { Console.WriteLine("[Economy] Binary 로딩 에러"); }
            }

			if (Towns.Count == 0)
            {
                foreach (var m in World.Mobiles.Values)
                {
                    if (m is BaseVendor v && v is not Banker)
                    {
                        int tID = TownNumber.GetID(v.Location, v.Map);
                        if (tID > 0)
                        {
                            if (!Towns.ContainsKey(tID)) Towns[tID] = new TownEconomy(tID, 0);
                            Towns[tID].RegisterVendor(v);
                        }
                    }
                }
                Console.WriteLine($"[Economy] 파일 없음: 월드 스캔으로 {Towns.Count}개 도시를 자동 복구했습니다.");
            }

            // ====================================================================
			// [최종 정화] 상인이 없거나 취급 물품이 없는 유령 구역 일괄 삭제
			// ====================================================================
			var cleanupList = Towns.Values.Where(t => 
			{
				// 실시간 상인 수 확인
				int activeVendors = World.Mobiles.Values.OfType<BaseVendor>()
					.Count(v => v is not Banker && TownNumber.GetID(v.Location, v.Map) == t.TownID);
				
				// 창고 물품 가짓수 확인
				int inventoryCount = t.Warehouse.Count;

				// 상인이 0명이거나, 물품 종류가 0개면 삭제 대상으로 판정
				return activeVendors == 0 || inventoryCount == 0;
			})
			.Select(t => t.TownID)
			.ToList();

			foreach (int targetID in cleanupList)
			{
				Towns.Remove(targetID);
			}

			if (cleanupList.Count > 0)
			{
				Console.WriteLine($"[Economy] 유령 구역(상인 0명 또는 물품 없음) {cleanupList.Count}개를 정리했습니다.");
			}

            m_IsLoaded = true;
        }

        public static List<TownInventoryEntry> GetSetupData(int townID)
        {
            if (Towns.ContainsKey(townID)) {
                var town = Towns[townID];
                List<TownInventoryEntry> list = [];
                foreach (var kvp in town.Warehouse) list.Add(new TownInventoryEntry(kvp.Key, kvp.Value.Stock, kvp.Value.BasePrice));
                return list;
            }
            return [];
        }
    }
}
