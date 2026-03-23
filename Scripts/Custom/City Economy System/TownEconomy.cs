using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Server;
using Server.Mobiles;
using System.Linq;

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

        public WarehouseItem(Type type, int stock, int price)
        {
            ItemType = type; Stock = stock; BasePrice = price;
        }
    }

	public partial class TownEconomy
	{
		// [동적 계산 엔진] 고정 데이터 없이 실시간 지표로 BaseWealth 산출
		public void UpdateBaseWealth()
		{
			// 1. 전체 재고 수량(Stock) 합산
			long totalStock = Warehouse.Values.Sum(i => (long)i.Stock);
			
			// 2. 기초 가중치 연산 (5억 타겟 베이스)
			long areaValue = (long)this.TotalTiles * 500;          // 타일당 1,000g
			long humanValue = (long)this.VendorCount * 25000;      // 상인당 62.5만g
			long stockValue = totalStock * 25;                      // 재고당 25

			long baseSum = areaValue + humanValue + stockValue;

			// 3. 통합 배율 적용 (도시 2배 / 야생 1배)
			// 브리튼 적용 시: 약 5.08억 * 2 = 10.16억 (10P) 확정
			this.BaseWealth = TownID > 0 ? baseSum * 2 : baseSum;

		}
		// [중요] 상인이 보급품을 가져오는 로직 (최초 1회 제한)
		public void InitInitialStock(Type itemType, int amount, int basePrice)
		{
			// 이미 해당 아이템이 창고에 있다면(유저 거래 중인 데이터 포함) 보급을 건너뜁니다.
			if (Warehouse.ContainsKey(itemType)) return;

			Warehouse[itemType] = new WarehouseItem(itemType, amount, basePrice);
		}
		
		// [참조용] 실제 총 자산 (현금 + 재고 가치)
		public long WarehouseValue => Warehouse.Values.Sum(i => (long)i.Stock * i.BasePrice);
		public long ActualTotalWealth => Wealth + WarehouseValue;


	}

    // =========================================================================
    // 2. TownEconomy: 개별 도시의 자산 및 공용 창고 (하이브리드 완전판)
    // =========================================================================
    public partial class TownEconomy
    {
        private List<BaseVendor> m_ActiveVendors = [];
        private int m_TownID;
        private long m_Wealth;
		public string Name => TownNumber.GetName(this.TownID);
        // --- 핵심 식별 데이터 ---
        [CommandProperty(AccessLevel.GameMaster)]
		public int TownID
		{
			get => m_TownID;
			set
			{
				m_TownID = value;
				int logicID = value / 100;

				// 우리만의 논리 번호 체계 적용
				Facet = logicID switch {
					0 => Map.Trammel,  // ID 1~99 -> Trammel
					1 => Map.Felucca,  // ID 101~199 -> Felucca
					2 => Map.Ilshenar,
					3 => Map.Malas,
					4 => Map.Tokuno,
					5 => Map.TerMur,
					_ => Map.Trammel
				};
				TownName = TownNumber.GetName(value);
			}
		}
		// [추가] 상인 관리 리스트를 강제로 비우는 메서드
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

        // --- 경제 엔진 데이터 ---
        [CommandProperty(AccessLevel.GameMaster)] public long Wealth { get => m_Wealth; set => m_Wealth = value; }
        [CommandProperty(AccessLevel.GameMaster)] public long BaseWealth { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public long TaxFund { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public int VendorCount { get; set; }
        [CommandProperty(AccessLevel.GameMaster)] public int MaxInventoryCapacity { get; set; } = 2000;

        public long TotalWealth => Wealth; // 구버전 참조용 (CS1061 해결)

        // --- 리스트 및 딕셔너리 (CS1061 해결) ---
        public List<VirtualCitizen> Citizens { get; set; } = [];
        public List<VirtualHouse> Houses { get; set; } = [];
        public Dictionary<Type, WarehouseItem> Warehouse { get; set; } = [];
        public List<TownInventoryEntry> InventoryEntries { get; set; } = [];
        public Dictionary<NpcJobClass, double> JobBirthWeights { get; set; } = [];

        // --- 재화 표시 및 물가 로직 ---
        public long Platinum { get => Wealth / 100000000; set => Wealth = value * 100000000; }
        public long ExtraGold => Wealth % 100000000;
        public string TotalWealthString => $"{Platinum}P {ExtraGold:N0}g";
        		// [물가 배율] 기준 자산(BaseWealth) 대비 현재 총 자산(ActualTotalWealth) 비율
		public double PriceMultiplier => BaseWealth > 0 
			? Math.Clamp((double)BaseWealth / Math.Max(1, ActualTotalWealth), 0.5, 1.5) 
			: 1.0;

        // --- 메서드 ---
        public void RegisterVendor(BaseVendor v)
        {
            if (v != null && !m_ActiveVendors.Contains(v))
            {
                m_ActiveVendors.Add(v);
                VendorCount = m_ActiveVendors.Count;
            }
        }

        public void SupplyItem(params object[] args) { /* 구버전 호환용 공백 메서드 */ }

        public TownEconomy(int townID, long baseWealth)
        {
            TownID = townID;
            BaseWealth = baseWealth;
            Wealth = baseWealth;
        }

        public int GetPrice(Type type, double multiplier = 1.0)
        {
            if (Warehouse.TryGetValue(type, out var item))
                return (int)(item.BasePrice * multiplier * PriceMultiplier);
            return 100;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)5); // 버전 5: int ID 체계로 완전 개편
            writer.Write(TownID);
            writer.Write(Wealth);
            writer.Write(BaseWealth);
            writer.Write(VendorCount);
            
            writer.Write(Warehouse.Count);
            foreach (var kvp in Warehouse)
            {
                writer.Write(kvp.Key.FullName);
                writer.Write(kvp.Value.Stock);
                writer.Write(kvp.Value.BasePrice);
            }
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
            TownID = reader.ReadInt();
            TownName = TownNumber.GetName(TownID); // 로딩 시 이름 자동 복구
            Wealth = reader.ReadLong();
            BaseWealth = reader.ReadLong();
            VendorCount = reader.ReadInt();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
			{
				Type type = ScriptCompiler.FindTypeByFullName(reader.ReadString());
				int stock = reader.ReadInt();
				int price = reader.ReadInt();
				if (type != null) Warehouse[type] = new WarehouseItem(type, stock, price);
			}
        }
    }

// =========================================================================
    // 3. TownEconomyManager: 전체 경제 통제 및 세이브/로드 엔진
    // =========================================================================
	public static class TownEconomyManager
    {
        public static Dictionary<int, TownEconomy> Towns = new Dictionary<int, TownEconomy>();
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
                    
                    // 저장 전 데이터 확인 로그
                    Console.WriteLine($"[Economy Save] {Towns.Count}개의 도시 데이터를 저장합니다.");

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
						
						Towns.Clear();
						for (int i = 0; i < count; i++) {
							int townID = reader.ReadInt();
							
							// 1. 그릇(객체) 생성
							TownEconomy town = new TownEconomy(townID, 0); 
							
							// 2. 파일에 저장된 값들을 'Wealth', 'BaseWealth', 'Warehouse'에 정직하게 복사
							town.Deserialize(reader);
							
							Towns[townID] = town;
						}
					}
					Console.WriteLine($"[Economy] {Towns.Count}개 도시 로드 완료.");
				} catch { Console.WriteLine("[Economy] Binary 로딩 에러"); }
			}
			m_IsLoaded = true;
		}

        // Gump나 다른 곳에서 호출할 때 안전하게 로드 보장
        public static List<TownInventoryEntry> GetSetupData(int townID)
        {
            // ... 기존 리스트 반환 로직 ...
            if (Towns.TryGetValue(townID, out var town)) {
                List<TownInventoryEntry> list = new List<TownInventoryEntry>();
                foreach (var kvp in town.Warehouse) list.Add(new TownInventoryEntry(kvp.Key, kvp.Value.Stock, kvp.Value.BasePrice));
                return list;
            }
            return new List<TownInventoryEntry>();
        }
    }
}