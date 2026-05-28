using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Misc
{
    // ==============================================================================
    // 🌟 [호환성 패치 완료] 궁극의 경제 키 (EconomyItemKey)
    // ==============================================================================
    public record struct EconomyItemKey(Type ItemType, CraftResource Resource = CraftResource.None, int SubID = 0, bool IsExceptional = false)
    {
        // 🌟 Gump(UI) 출력용 스마트 네이밍 로직
        public string Name 
        { 
            get 
            {
                if (ItemType == null) return "Unknown";

                // 1. 색자원 이름 (예: Valorite, ShadowIron 등)
                string resName = (Resource != CraftResource.None && Resource != CraftResource.Iron) ? $"{Resource} " : "";
                
                // 2. 기본 아이템 이름
                string baseName = ItemType.Name;

                // 3. 음료수 내용물 표기 (SubID)
                if (ItemType == typeof(BeverageBottle) || ItemType == typeof(Pitcher) || ItemType == typeof(Jug) || ItemType == typeof(GlassMug))
                {
                    baseName += $"({(BeverageType)SubID})";
                }

                // 4. 명품(Exceptional) 표기
                string excName = IsExceptional ? " [Exc]" : "";

                // 최종 조합 (예: "Valorite PlateChest [Exc]" 또는 "BeverageBottle(Ale)")
                return $"{resName}{baseName}{excName}";
            }
        }
        
        public static implicit operator EconomyItemKey(Type t) => new EconomyItemKey(t, CraftResource.None, 0, false);
        public static implicit operator Type(EconomyItemKey key) => key.ItemType; 
    }

    public class TownInventoryEntry
    {
        public EconomyItemKey ItemKey { get; set; }
        public Type ItemType => ItemKey.ItemType; // 🌟 구버전 호환용 (BaseVendor 에러 차단)
        
        public int InitialStock { get; set; }
        public int BasePrice { get; set; }
        public TownInventoryEntry(EconomyItemKey key, int stock, int price)
        {
            ItemKey = key; InitialStock = stock; BasePrice = price;
        }
    }

    public class WarehouseItem
    {
        public EconomyItemKey ItemKey { get; set; } 
        public Type ItemType => ItemKey.ItemType; // 🌟 구버전 호환용 (AdminGump 에러 차단)
        
        public int Stock { get; set; }
        public int BasePrice { get; set; }
        public int TargetStock { get; set; } 
        public int LastStock { get; set; } // 🌟 [추가] 변동 추이 계산을 위한 과거 재고량

        public WarehouseItem(EconomyItemKey key, int stock, int price)
        {
            ItemKey = key; Stock = stock; BasePrice = price; TargetStock = stock; LastStock = stock;
        }

        public WarehouseItem(EconomyItemKey key, int stock, int price, int targetStock)
        {
            ItemKey = key; Stock = stock; BasePrice = price; TargetStock = targetStock; LastStock = stock;
        }
    }

    public class TownEconomy
    {
        private List<BaseVendor> m_ActiveVendors = [];
        private int m_TownID;
        private long m_Wealth;

        public string[] TerritoryMap { get; set; } 
        public int Population { get; set; }
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int CurrentTilePrice
        {
            get
            {
                var grid = TownNumber.GetGridInfo(this.TownID);
                var info = TownNumber.GetInfo(this.TownID);

                if (grid.Total <= 0) return 50000;

                long basePrice = info.Grade switch
                {
                    "S" => 200000, "A" => 150000, "B" => 100000, "C" => 50000, _ => 100000
                };

                double density = (double)Population / grid.Total;
                double weight = 1.0 + (density * 2.0);

                return (int)(basePrice * weight);
            }
        }

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
                    0 => Map.Trammel, 1 => Map.Felucca, 2 => Map.Ilshenar, 3 => Map.Malas,
                    4 => Map.Tokuno, 5 => Map.TerMur, _ => Map.Trammel
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
		[CommandProperty(AccessLevel.GameMaster)] public int Security { get; set; } = 100;
        [CommandProperty(AccessLevel.GameMaster)] public int CrimeIndex { get; set; } = 0;
		

        public long TotalWealth => Wealth; 

        public List<VirtualCitizen> Citizens { get; set; } = [];
        public List<VirtualHouse> Houses { get; set; } = [];
        
        public Dictionary<EconomyItemKey, WarehouseItem> Warehouse { get; set; } = new();
        
        public List<TownInventoryEntry> InventoryEntries { get; set; } = [];
        public Dictionary<NpcJobClass, double> JobBirthWeights { get; set; } = [];

        public long Platinum { get => Wealth / 100_000_000L; set => Wealth = value * 100_000_000L; }
        public long ExtraGold => Wealth % 100_000_000L;
        public string TotalWealthString => $"{Platinum}P {ExtraGold:N0}g";
        
        [CommandProperty(AccessLevel.GameMaster)]
        public double MacroModifier => Math.Clamp((double)Wealth / Math.Max(1, BaseWealth) - 1.0, -0.5, 1.0);

        [CommandProperty(AccessLevel.GameMaster)]
        public double PriceMultiplier => 1.0 + MacroModifier;

        public double EventPriceModifier { get; set; } = 0.0;
        public double SecurityPriceModifier { get; set; } = 0.0;

        public int GetPrice(EconomyItemKey key, double externalMultiplier = 1.0)
        {
            if (!Warehouse.ContainsKey(key)) return 100;
            var item = Warehouse[key];

            double macroMod = Math.Clamp((double)Wealth / Math.Max(1, BaseWealth) - 1.0, -0.5, 1.0);
            double snapshot = Math.Max(1, item.TargetStock);
            double ratio = (double)item.Stock / snapshot;

            double microMod = Math.Clamp((1.0 - ratio) * 0.25, -0.25, 0.25);
            double finalFactor = 1.0 + macroMod + microMod + EventPriceModifier + SecurityPriceModifier;

            if (TownID >= 900 || TownIndex == "C") finalFactor *= 1.25; 

            return Math.Max(1, (int)(item.BasePrice * Math.Max(0.25, finalFactor) * externalMultiplier));
        }
        
        public long WarehouseValue => Warehouse.Values.Sum(i => (long)i.Stock * i.BasePrice);
        public long ActualTotalWealth => Wealth + WarehouseValue;

        public TownEconomy(int townID, long baseWealth)
        {
            TownID = townID; BaseWealth = baseWealth; Wealth = baseWealth;
            var grid = TownNumber.GetGridInfo(townID);
            TerritoryMap = new string[Math.Max(1, grid.Total)];
        }

        public void UpdateBaseWealth()
        {
            if (TownID >= 900 || TownIndex == "C")
            {
                this.BaseWealth = this.VendorCount * 15000L;
                return;
            }

            long totalStock = Warehouse.Values.Sum(i => (long)i.Stock);
            long areaValue = (long)this.TotalTiles * 500;
            long humanValue = (long)this.VendorCount * 25000;
            long stockValue = totalStock * 25;

            long baseSum = areaValue + humanValue + stockValue;
            this.BaseWealth = TownID > 0 ? baseSum * 2 : baseSum;
        }

        public void InitInitialStock(EconomyItemKey key, int amount, int basePrice)
        {
            if (Warehouse.ContainsKey(key)) return;
            Warehouse[key] = new WarehouseItem(key, amount, basePrice, amount);
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
            // 🌟 [수정] 버전 13 -> 14로 승급 (LastStock 저장용)
            writer.Write((int)14); 
            
            writer.Write(m_TownID);
            writer.Write(m_Wealth);
            writer.Write(BaseWealth);
            writer.Write(VendorCount);
            
            writer.Write(Warehouse.Count);
            foreach (var kvp in Warehouse)
            {
                writer.Write(kvp.Key.ItemType.FullName);
                writer.Write((int)kvp.Key.Resource);
                writer.Write(kvp.Key.SubID);
                writer.Write(kvp.Key.IsExceptional);
                
                writer.Write(kvp.Value.Stock);
                writer.Write(kvp.Value.BasePrice);
                writer.Write(kvp.Value.TargetStock);
                writer.Write(kvp.Value.LastStock); // 🌟 [추가] 추이 기록 저장
            }

            writer.Write(Citizens.Count);
            foreach (var citizen in Citizens) citizen.Serialize(writer);

            writer.Write(Houses.Count);
            foreach (var house in Houses)
            {
                writer.Write(house.HouseName);
                writer.Write(house.Prestige);
                writer.Write(house.TotalWealth);
                writer.Write((int)house.PrimaryRank);
                writer.Write(house.MultiID);

                writer.Write(house.CurrentFameScore);
                writer.Write(house.LastSocialEventTime);
                writer.Write(house.IsHostingEventTonight);
                writer.Write(house.EventFameBonus);

                // 🌟 [중요 수정] UnfulfilledNeeds 세이브 로직 갱신 (에러 원인 해결)
                writer.Write(house.UnfulfilledNeeds.Count);
                foreach (var need in house.UnfulfilledNeeds)
                {
                    writer.Write(need.Key.ItemType.FullName);
                    writer.Write((int)need.Key.Resource);
                    writer.Write(need.Key.SubID);
                    writer.Write(need.Key.IsExceptional);
                    writer.Write(need.Value);
                }

                writer.Write(house.OwnedTileIndices.Count);
                foreach (int tileIndex in house.OwnedTileIndices) writer.Write(tileIndex);

                writer.Write(house.HasGarden);
                writer.Write(house.HasWorkshop);
                writer.Write(house.HasBarracks);

                writer.Write(house.HousingAmbition);
                writer.Write(house.Grudges.Count);
                foreach (var kvp in house.Grudges) { writer.Write(kvp.Key); writer.Write(kvp.Value); }

                writer.Write(house.Families.Count);
                foreach (var family in house.Families)
                {
                    writer.Write(Citizens.IndexOf(family.Father));
                    writer.Write(Citizens.IndexOf(family.Mother));
                    writer.Write(family.SharedWealth);
                    writer.Write(family.Prestige);

                    writer.Write(family.Children.Count);
                    foreach (var child in family.Children) writer.Write(Citizens.IndexOf(child));
                }
            }

            writer.Write(TerritoryMap.Length);
            for (int i = 0; i < TerritoryMap.Length; i++) writer.Write(TerritoryMap[i] ?? ""); 
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
            m_TownID = reader.ReadInt();
            TownName = TownNumber.GetName(m_TownID);
            m_Wealth = reader.ReadLong();
            BaseWealth = reader.ReadLong();
            VendorCount = reader.ReadInt();

            var grid = TownNumber.GetGridInfo(m_TownID);
            if (TerritoryMap == null || TerritoryMap.Length != grid.Total)
                TerritoryMap = new string[Math.Max(1, grid.Total)];

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Type type = ScriptCompiler.FindTypeByFullName(reader.ReadString());
                CraftResource res = version >= 13 ? (CraftResource)reader.ReadInt() : CraftResource.None;
                int subID = version >= 13 ? reader.ReadInt() : 0;
                bool isExc = version >= 13 ? reader.ReadBool() : false;
                
                int stock = reader.ReadInt();
                int price = reader.ReadInt();
                int targetStock = (version >= 7) ? reader.ReadInt() : stock;
                int lastStock = (version >= 14) ? reader.ReadInt() : stock; // 🌟 [추가] 과거 재고량 로드

                if (type != null) 
                {
                    EconomyItemKey key = new EconomyItemKey(type, res, subID, isExc);
                    Warehouse[key] = new WarehouseItem(key, stock, price, targetStock) { LastStock = lastStock };
                }
            }

            if (version >= 6)
            {
                int citizenCount = reader.ReadInt();
                for (int i = 0; i < citizenCount; i++) Citizens.Add(new VirtualCitizen(reader)); 
            }

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
                    house.MultiID = reader.ReadInt();

                    if (version >= 12)
                    {
                        house.CurrentFameScore = reader.ReadInt();
                        house.LastSocialEventTime = reader.ReadDateTime();
                        house.IsHostingEventTonight = reader.ReadBool();
                        house.EventFameBonus = reader.ReadInt();

                        int needsCount = reader.ReadInt();
                        for (int k = 0; k < needsCount; k++)
                        {
                            Type t = ScriptCompiler.FindTypeByFullName(reader.ReadString());
                            // 🌟 [중요 수정] UnfulfilledNeeds 로드 로직 갱신
                            CraftResource res = version >= 13 ? (CraftResource)reader.ReadInt() : CraftResource.None;
                            int subID = version >= 13 ? reader.ReadInt() : 0;
                            bool isExc = version >= 13 ? reader.ReadBool() : false;
                            int amt = reader.ReadInt();
                            if (t != null) house.UnfulfilledNeeds[new EconomyItemKey(t, res, subID, isExc)] = amt;
                        }
                    }

                    if (version >= 9)
                    {
                        int ownedTileCount = reader.ReadInt();
                        for (int t = 0; t < ownedTileCount; t++)
                        {
                            int tileIdx = reader.ReadInt();
                            if (tileIdx >= 0 && tileIdx < TerritoryMap.Length) house.OwnedTileIndices.Add(tileIdx);
                        }
                    }

                    if (version >= 10)
                    {
                        house.HasGarden = reader.ReadBool();
                        house.HasWorkshop = reader.ReadBool();
                        house.HasBarracks = reader.ReadBool();
                    }

                    if (version >= 11)
                    {
                        house.HousingAmbition = reader.ReadInt();
                        int grudgeCount = reader.ReadInt();
                        for (int k = 0; k < grudgeCount; k++)
                        {
                            string rivalName = reader.ReadString();
                            int grudgeVal = reader.ReadInt();
                            house.Grudges[rivalName] = grudgeVal;
                        }
                    }

                    Houses.Add(house);

                    int familyCount = reader.ReadInt();
                    for (int j = 0; j < familyCount; j++)
                    {
                        int fatherIdx = reader.ReadInt();
                        int motherIdx = reader.ReadInt();
                        long famWealth = reader.ReadLong();
                        int famPrestige = reader.ReadInt();

                        VirtualCitizen father = (fatherIdx >= 0 && fatherIdx < Citizens.Count) ? Citizens[fatherIdx] : null;
                        VirtualCitizen mother = (motherIdx >= 0 && motherIdx < Citizens.Count) ? Citizens[motherIdx] : null;

                        FamilyUnit family = new FamilyUnit(father, mother) { SharedWealth = famWealth, Prestige = famPrestige };
                        house.Families.Add(family);

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
                                child.Family = family;
                                child.House = house;
                            }
                        }
                    }
                }
            }

            if (version >= 9)
            {
                int mapLength = reader.ReadInt();
                for (int i = 0; i < mapLength; i++)
                {
                    string ownerName = reader.ReadString();
                    if (i < TerritoryMap.Length) TerritoryMap[i] = string.IsNullOrEmpty(ownerName) ? null : ownerName;
                }
            }
        }
    }

    public static class TownEconomyManager
    {
        public static Dictionary<int, TownEconomy> Towns = [];
        private static bool m_IsLoaded = false;

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
            
            CommandSystem.Register("ResetVirtualCitizens", AccessLevel.Administrator, new CommandEventHandler(ResetVirtualCitizens_OnCommand));
        }

        [Usage("ResetVirtualCitizens")]
        [Description("기존의 가상 시민과 주택 데이터를 모두 삭제하고 새로운 기획(1/10 인구)에 맞춰 재배양합니다.")]
        private static void ResetVirtualCitizens_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("가상 시민 데이터를 초기화하고 재배양을 시작합니다. 콘솔을 확인하세요.");
            ResetAllVirtualCitizens();
            e.Mobile.SendMessage("가상 시민 재배양이 완료되었습니다.");
        }

        public static void ResetAllVirtualCitizens()
        {
            Console.WriteLine("[TownEconomy] 기존 가상 시민 데이터 초기화 중...");
            
            int deletedCount = 0;
            int deletedHouses = 0;

            foreach (var town in Towns.Values)
            {
                if (town.Citizens != null)
                {
                    deletedCount += town.Citizens.Count;
                    town.Citizens.Clear();
                }
                
                if (town.Houses != null)
                {
                    deletedHouses += town.Houses.Count;
                    town.Houses.Clear();
                }

                UpdateTownWealth(town);
            }

            Console.WriteLine($"[TownEconomy] {deletedCount}명의 시민과 {deletedHouses}채의 가옥 장부가 삭제되었습니다.");

            foreach (var town in Towns.Values)
            {
                int newCap = TownDemographics.CalculatePopulationCap(town);
                TownDemographics.InitializeTown(town, newCap);
            }
            
            Console.WriteLine($"[TownEconomy] 소수 정예(1/10) 맞춤형 인구 배양이 완료되었습니다.");
        }

        public static void UpdateTownWealth(TownEconomy town)
        {
            if (town == null) return;

            int vendorCount = town.VendorCount;
            if (vendorCount == 0) vendorCount = 10; 

            long humanValue = (long)(vendorCount * 250000 * 10.0);

            if (town.Wealth < humanValue)
            {
                town.Wealth = humanValue;
            }
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
                    Console.WriteLine($"[Economy] {Towns.Count}개 도시 로드 완료.");
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

            var cleanupList = Towns.Values.Where(t => 
            {
                int activeVendors = World.Mobiles.Values.OfType<BaseVendor>()
                    .Count(v => v is not Banker && TownNumber.GetID(v.Location, v.Map) == t.TownID);
                int inventoryCount = t.Warehouse.Count;
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