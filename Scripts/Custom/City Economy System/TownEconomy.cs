using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Mobiles;
using Server.Commands;

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

        // [신규] 영토 관리를 위한 1차원 배열 (타일 소유주 식별)
        // 값: 소유한 가문의 이름 (null이거나 빈 문자열이면 빈 땅)
        public string[] TerritoryMap { get; set; } 
        public int Population { get; set; }
        
        // [신규] 마을의 기본 타일 시세 (매일/매번 변동 가능)
        [CommandProperty(AccessLevel.GameMaster)]
        public int CurrentTilePrice
        {
            get
            {
                var grid = TownNumber.GetGridInfo(this.TownID);
                var info = TownNumber.GetInfo(this.TownID);

                if (grid.Total <= 0) return 50000;

                // [수정] 영토 타일당 가격 100배 상향
                long basePrice = info.Grade switch
                {
                    "S" => 200000,
                    "A" => 150000,
                    "B" => 100000,
                    "C" => 50000,
                    _ => 100000
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

        public long Platinum { get => Wealth / 100_000_000L; set => Wealth = value * 100_000_000L; }
        public long ExtraGold => Wealth % 100_000_000L;
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
            if (!Warehouse.ContainsKey(type)) return 100;
            var item = Warehouse[type];

            double macroMod = Math.Clamp((double)Wealth / Math.Max(1, BaseWealth) - 1.0, -0.5, 1.0);
            double snapshot = Math.Max(1, item.TargetStock);
            double ratio = (double)item.Stock / snapshot;

            double microMod = Math.Clamp((1.0 - ratio) * 0.25, -0.25, 0.25);
            double finalFactor = 1.0 + macroMod + microMod + EventPriceModifier + SecurityPriceModifier;

            if (TownID >= 900 || TownIndex == "C")
            {
                finalFactor *= 1.25; 
            }

            return Math.Max(1, (int)(item.BasePrice * Math.Max(0.25, finalFactor) * externalMultiplier));
        }
        public long WarehouseValue => Warehouse.Values.Sum(i => (long)i.Stock * i.BasePrice);
        public long ActualTotalWealth => Wealth + WarehouseValue;

        public TownEconomy(int townID, long baseWealth)
        {
            TownID = townID;
            BaseWealth = baseWealth;
            Wealth = baseWealth;
            
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

        public void InitInitialStock(Type itemType, int amount, int basePrice)
        {
            if (Warehouse.ContainsKey(itemType)) return;
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

        // =========================================================================
        // 💾 데이터 저장 (Serialize) - 가문의 모든 신규 변수 포함
        // =========================================================================
        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)12); // 🌟 [버전 12] 명예 점수, 파티 기록, 미수급 장부 추가
            
            writer.Write(m_TownID);
            writer.Write(m_Wealth);
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

            // 3. 가문(VirtualHouse) 저장
            writer.Write(Houses.Count);
            foreach (var house in Houses)
            {
                writer.Write(house.HouseName);
                writer.Write(house.Prestige);
                writer.Write(house.TotalWealth);
                writer.Write((int)house.PrimaryRank);
                writer.Write(house.MultiID);

                // --- 🌟 [버전 12 추가 데이터] ---
                writer.Write(house.CurrentFameScore);
                writer.Write(house.LastSocialEventTime);
                writer.Write(house.IsHostingEventTonight);
                writer.Write(house.EventFameBonus);

                // 미수급 장부(UnfulfilledNeeds) 저장
                writer.Write(house.UnfulfilledNeeds.Count);
                foreach (var need in house.UnfulfilledNeeds)
                {
                    writer.Write(need.Key.FullName);
                    writer.Write(need.Value);
                }
                // ------------------------------

                writer.Write(house.OwnedTileIndices.Count);
                foreach (int tileIndex in house.OwnedTileIndices)
                {
                    writer.Write(tileIndex);
                }

                writer.Write(house.HasGarden);
                writer.Write(house.HasWorkshop);
                writer.Write(house.HasBarracks);

                writer.Write(house.HousingAmbition);
                writer.Write(house.Grudges.Count);
                foreach (var kvp in house.Grudges)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }

                // 4. 가족(FamilyUnit) 및 관계도 저장
                writer.Write(house.Families.Count);
                foreach (var family in house.Families)
                {
                    writer.Write(Citizens.IndexOf(family.Father));
                    writer.Write(Citizens.IndexOf(family.Mother));
                    writer.Write(family.SharedWealth);
                    writer.Write(family.Prestige);

                    writer.Write(family.Children.Count);
                    foreach (var child in family.Children)
                    {
                        writer.Write(Citizens.IndexOf(child));
                    }
                }
            }

            // 5. 마을 영토 소유권 저장
            writer.Write(TerritoryMap.Length);
            for (int i = 0; i < TerritoryMap.Length; i++)
            {
                writer.Write(TerritoryMap[i] ?? ""); 
            }
        }

        // =========================================================================
        // 📂 데이터 로드 (Deserialize) - 바늘과 실처럼 순서가 완벽하게 일치해야 함
        // =========================================================================
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

            // 1. 창고 복구
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Type type = ScriptCompiler.FindTypeByFullName(reader.ReadString());
                int stock = reader.ReadInt();
                int price = reader.ReadInt();
                int targetStock = (version >= 7) ? reader.ReadInt() : stock;

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

            // 3. 가문 복구 
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

                    // --- 🌟 [버전 12 데이터 로드] ---
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
                            int amt = reader.ReadInt();
                            if (t != null) house.UnfulfilledNeeds[t] = amt;
                        }
                    }
                    // ------------------------------

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

            // 5. 마을 전체 영토 소유 맵 복구
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
            
            // 🌟 [명령어 등록] 게임 내에서 [ResetVirtualCitizens 입력 시 리셋 발동
            CommandSystem.Register("ResetVirtualCitizens", AccessLevel.Administrator, new CommandEventHandler(ResetVirtualCitizens_OnCommand));
        }

        // ====================================================================
        // 🌟 [추가] 가상 사회 초기화 및 인구 재배양 (GM 전용 명령어)
        // ====================================================================
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
                    // 장부상에서 집을 모두 삭제
                    deletedHouses += town.Houses.Count;
                    town.Houses.Clear();
                }

                // 🌟 마을 인구수에 기반한 자본금(Wealth) 재산정 호출
                UpdateTownWealth(town);
            }

            Console.WriteLine($"[TownEconomy] {deletedCount}명의 시민과 {deletedHouses}채의 가옥 장부가 삭제되었습니다.");

            // ====================================================================
            // 🌟 [수정 완료] 옛날 엔진 대신, 새롭게 통합된 인구통계학(Demographics) 엔진 호출!
            // ====================================================================
            foreach (var town in Towns.Values)
            {
                // 마을별 1/10 상한선 계산 후, 맞춤형 초기화 진행
                int newCap = TownDemographics.CalculatePopulationCap(town);
                TownDemographics.InitializeTown(town, newCap);
            }
            
            Console.WriteLine($"[TownEconomy] 소수 정예(1/10) 맞춤형 인구 배양이 완료되었습니다.");
        }

        // ====================================================================
        // 🌟 [추가] 마을의 기초 자본금(Wealth) 산정 (10배 경제 배율 적용)
        // ====================================================================
        public static void UpdateTownWealth(TownEconomy town)
        {
            if (town == null) return;

            int vendorCount = town.VendorCount;
            if (vendorCount == 0) vendorCount = 10; // 최소 보정

            // [기획 반영] 인구는 1/10로 줄었지만, 경제 규모는 유지하기 위해 10배 배율(10.0) 적용
            long humanValue = (long)(vendorCount * 250000 * 10.0);

            // 마을의 초기 자본금 세팅 (돈이 너무 없을 때만 보충)
            if (town.Wealth < humanValue)
            {
                town.Wealth = humanValue;
            }
        }

        // ====================================================================
        // 기존 Save / Load 및 유령 구역 정리 로직
        // ====================================================================
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