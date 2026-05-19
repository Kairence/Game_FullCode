using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Quests;
using Server.Network;
using Server.Engines.Craft;

namespace Server.Misc
{
    public enum JobCategory 
    { 
        Menial = 100, Gathering = 101,     
        Crafting = 200,                    
        Delivery = 300,                    
        EcoHunting = 500,                  
        DungeonHunting = 600,              
        BlackMarket = 1100                 
    }

    public enum JobTier { Beginner = 0, Intermediate = 1, Advanced = 2, Special = 3 }
    public enum JobOrigin { TownPublic, CitizenPrivate }

    public class TownJobRequest
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public JobOrigin Origin { get; set; }
        public string TownName { get; set; }
        public string TargetHouseName { get; set; } 
        public JobCategory Category { get; set; }
        public JobTier Tier { get; set; }
        public string Title { get; set; }
        public Type TargetType { get; set; }
        public string RegionName { get; set; }

        public CraftResource RequiredResource { get; set; } = CraftResource.None;
        public bool RequireExceptional { get; set; }
        
        public int TotalRequired { get; set; }
        public int AmountPerPlayer { get; set; }
        public int CurrentParticipants { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public int RewardGold { get; set; }
        public int RewardFame { get; set; }
        public Type BonusRewardType { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.Now;
        public bool IsAIAssigned { get; set; } = false;
        public VirtualHouse IssuerHouse { get; set; }

        public bool IsFullyBooked => CurrentParticipants >= (AmountPerPlayer > 0 ? Math.Max(1, TotalRequired / AmountPerPlayer) : 1);

        public TownJobRequest() { }
        
        public TownJobRequest(GenericReader r)
        {
            int v = r.ReadInt();
            ID = Guid.Parse(r.ReadString());
            Origin = (JobOrigin)r.ReadInt();
            TownName = r.ReadString();
            Category = (JobCategory)r.ReadInt();
            Tier = (JobTier)r.ReadInt();
            Title = r.ReadString();
            string t = r.ReadString(); if (!string.IsNullOrEmpty(t)) TargetType = ScriptCompiler.FindTypeByFullName(t);
            RegionName = r.ReadString();
            RequiredResource = (CraftResource)r.ReadInt();
            RequireExceptional = r.ReadBool();
            TotalRequired = r.ReadInt();
            AmountPerPlayer = r.ReadInt();
            CurrentParticipants = r.ReadInt();
            TimeLimit = r.ReadTimeSpan();
            RewardGold = r.ReadInt();
            RewardFame = r.ReadInt();
            string b = r.ReadString(); if (!string.IsNullOrEmpty(b)) BonusRewardType = ScriptCompiler.FindTypeByFullName(b);
            CreationTime = r.ReadDateTime();
            IsAIAssigned = r.ReadBool();
            
            if (v >= 1) TargetHouseName = r.ReadString(); 
        }

        public void Serialize(GenericWriter w)
        {
            w.Write(1); 
            w.Write(ID.ToString()); w.Write((int)Origin); w.Write(TownName); w.Write((int)Category); w.Write((int)Tier);
            w.Write(Title); w.Write(TargetType?.FullName ?? ""); w.Write(RegionName ?? "");
            w.Write((int)RequiredResource); w.Write(RequireExceptional); w.Write(TotalRequired); w.Write(AmountPerPlayer);
            w.Write(CurrentParticipants); w.Write(TimeLimit); w.Write(RewardGold); w.Write(RewardFame);
            w.Write(BonusRewardType?.FullName ?? ""); w.Write(CreationTime); w.Write(IsAIAssigned);
            
            w.Write(TargetHouseName ?? "");
        }
    }

    public class PartTimeJob
    {
        public Guid RequestID { get; set; }
        public JobOrigin Origin { get; set; }
        public string TownName { get; set; }
        public string TargetHouseName { get; set; } 
        public JobCategory Category { get; set; }
        public string Title { get; set; }
        public Type TargetType { get; set; }
        public string RegionName { get; set; } 
        public CraftResource RequiredResource { get; set; }
        public int RequiredAmount { get; set; }
        public int CurrentAmount { get; set; }
        public bool RequireExceptional { get; set; }
        public DateTime ExpireTime { get; set; }
        public int RewardGold { get; set; }
        public int RewardFame { get; set; }
        public Type BonusRewardType { get; set; }
        public bool IsCompleted => CurrentAmount >= RequiredAmount;
        public bool IsExpired => DateTime.Now > ExpireTime;

        public PartTimeJob(TownJobRequest req)
        {
            RequestID = req.ID; Origin = req.Origin; TownName = req.TownName; TargetHouseName = req.TargetHouseName;
            Category = req.Category; Title = req.Title; TargetType = req.TargetType; RegionName = req.RegionName; 
            RequiredResource = req.RequiredResource; RequiredAmount = req.AmountPerPlayer; 
            RequireExceptional = req.RequireExceptional; ExpireTime = DateTime.Now.Add(req.TimeLimit); 
            RewardGold = req.RewardGold; RewardFame = req.RewardFame; BonusRewardType = req.BonusRewardType;
        }

        public PartTimeJob(GenericReader r)
        {
            int v = r.ReadInt();
            RequestID = Guid.Parse(r.ReadString()); Origin = (JobOrigin)r.ReadInt();
            TownName = r.ReadString(); Category = (JobCategory)r.ReadInt(); Title = r.ReadString();
            string t = r.ReadString(); if (!string.IsNullOrEmpty(t)) TargetType = ScriptCompiler.FindTypeByFullName(t);
            RequiredResource = (CraftResource)r.ReadInt(); RequiredAmount = r.ReadInt(); 
            CurrentAmount = r.ReadInt(); RequireExceptional = r.ReadBool(); 
            ExpireTime = r.ReadDateTime(); RewardGold = r.ReadInt(); RewardFame = r.ReadInt();
            string b = r.ReadString(); if (!string.IsNullOrEmpty(b)) BonusRewardType = ScriptCompiler.FindTypeByFullName(b);
            if (v >= 2) RegionName = r.ReadString();
            if (v >= 3) TargetHouseName = r.ReadString(); 
        }

        public void Serialize(GenericWriter w)
        {
            w.Write(3); 
            w.Write(RequestID.ToString()); w.Write((int)Origin); w.Write(TownName); w.Write((int)Category); 
            w.Write(Title); w.Write(TargetType?.FullName ?? ""); w.Write((int)RequiredResource);
            w.Write(RequiredAmount); w.Write(CurrentAmount); w.Write(RequireExceptional); 
            w.Write(ExpireTime); w.Write(RewardGold); w.Write(RewardFame); w.Write(BonusRewardType?.FullName ?? "");
            w.Write(RegionName ?? "");
            
            w.Write(TargetHouseName ?? "");
        }
    }

    public class PartTimeAccountProfile
    {
        public string AccountName { get; set; }
        public int AvailableCharges { get; set; }
        public int TotalCompleted { get; set; }
        public PartTimeJob CurrentJob { get; set; }

        public JobTier CurrentTier 
        {
            get 
            {
                if (TotalCompleted >= 200) return JobTier.Special;
                if (TotalCompleted >= 100) return JobTier.Advanced;
                if (TotalCompleted >= 30) return JobTier.Intermediate;
                return JobTier.Beginner;
            }
        }

        public PartTimeAccountProfile(string name) { AccountName = name; AvailableCharges = 2; }
        public PartTimeAccountProfile(GenericReader r)
        {
            int v = r.ReadInt(); AccountName = r.ReadString(); AvailableCharges = r.ReadInt();
            TotalCompleted = r.ReadInt(); if (r.ReadBool()) CurrentJob = new PartTimeJob(r);
        }
        public void Serialize(GenericWriter w)
        {
            w.Write(0); w.Write(AccountName); w.Write(AvailableCharges); w.Write(TotalCompleted);
            w.Write(CurrentJob != null); if (CurrentJob != null) CurrentJob.Serialize(w);
        }
    }

    public static class PartTimeManager
    {
        public static Dictionary<string, PartTimeAccountProfile> Profiles = new();
        public static List<TownJobRequest> ActiveRequests = new();
        
        public static Dictionary<JobTier, List<Type>> CachedSmithItems = new();
        public static Dictionary<JobTier, List<Type>> CachedTailorItems = new();
        public static Dictionary<JobTier, List<Type>> CachedFletcherItems = new();
        public static Dictionary<JobTier, List<Type>> CachedCarpentryItems = new();
        public static Dictionary<JobTier, List<Type>> CachedAlchemyItems = new();
        public static Dictionary<JobTier, List<Type>> CachedTinkerItems = new();
        public static Dictionary<JobTier, List<Type>> CachedInscriptionItems = new();
        public static Dictionary<JobTier, List<Type>> CachedCookingItems = new();
        public static Dictionary<JobTier, List<Type>> CachedImbuingItems = new(); 

        private static DateTime m_LastDailyReset;
        private static DateTime m_LastRefresh;
        private static readonly Random m_Random = new();

        public static void Configure() { EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; }
        
        public static void Initialize() 
        { 
            Timer.DelayCall(TimeSpan.Zero, CacheCraftingRecipes);
            Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), CheckSystem); 
        }

        private static void CacheCraftingRecipes()
        {
            LoadCraftSystem(DefBlacksmithy.CraftSystem, CachedSmithItems);
            LoadCraftSystem(DefTailoring.CraftSystem, CachedTailorItems);
            LoadCraftSystem(DefBowFletching.CraftSystem, CachedFletcherItems); 
            LoadCraftSystem(DefCarpentry.CraftSystem, CachedCarpentryItems);
            LoadCraftSystem(DefAlchemy.CraftSystem, CachedAlchemyItems);
            LoadCraftSystem(DefTinkering.CraftSystem, CachedTinkerItems);
            LoadCraftSystem(DefInscription.CraftSystem, CachedInscriptionItems);
            LoadCraftSystem(DefCooking.CraftSystem, CachedCookingItems);
            LoadCraftSystem(DefImbuing.CraftSystem, CachedImbuingItems); 
        }

        private static void LoadCraftSystem(CraftSystem system, Dictionary<JobTier, List<Type>> cache)
        {
            cache[JobTier.Beginner] = new List<Type>();
            cache[JobTier.Intermediate] = new List<Type>();
            cache[JobTier.Advanced] = new List<Type>();
            cache[JobTier.Special] = new List<Type>();

            if (system == null) return;

            foreach (CraftItem item in system.CraftItems)
            {
                if (item.ItemType == null) continue;
                
                double minSkill = 0;
                if (item.Skills.Count > 0) minSkill = item.Skills.GetAt(0).MinSkill;

                if (minSkill < 50.0) cache[JobTier.Beginner].Add(item.ItemType);
                else if (minSkill < 100.0) cache[JobTier.Intermediate].Add(item.ItemType);
                else if (minSkill < 150.0) cache[JobTier.Advanced].Add(item.ItemType);
                else cache[JobTier.Special].Add(item.ItemType);
            }
            
            if(cache[JobTier.Beginner].Count == 0) cache[JobTier.Beginner].Add(typeof(Dagger)); 
            if(cache[JobTier.Intermediate].Count == 0) cache[JobTier.Intermediate].AddRange(cache[JobTier.Beginner]);
            if(cache[JobTier.Advanced].Count == 0) cache[JobTier.Advanced].AddRange(cache[JobTier.Intermediate]);
            if(cache[JobTier.Special].Count == 0) cache[JobTier.Special].AddRange(cache[JobTier.Advanced]);
        }

        private static void CheckSystem()
        {
            if (m_LastDailyReset.Date != DateTime.Now.Date) PerformDailyReset();

            DateTime now = DateTime.Now;
            bool needsRefresh = (now - m_LastRefresh).TotalMinutes >= 30.0;

            for (int i = ActiveRequests.Count - 1; i >= 0; i--)
            {
                var req = ActiveRequests[i];
                if (now >= req.CreationTime + req.TimeLimit)
                {
                    if (req.IsAIAssigned) ProcessAIResult(req); 
                    ActiveRequests.RemoveAt(i);
                }
            }

            foreach (var state in NetState.Instances)
            {
                Mobile m = state.Mobile;
                if (m == null) continue;

                if( m is PlayerMobile pm)
                {
                    var quest = QuestHelper.GetQuest(pm, typeof(PartTimeQuest)) as PartTimeQuest;
                    if (quest != null && quest.JobData != null && quest.JobData.IsExpired)
                    {
                        m.SendMessage(33, "업무 기한(2시간)이 초과되어 파트타임 의뢰가 자동으로 파기되었습니다.");
                        quest.OnResign(false); 
                    }
                }
            }

            if (needsRefresh) PerformRefresh();
        }

        public static double GetMarginRate(JobTier tier) => tier switch
        {
            JobTier.Beginner => 0.80,
            JobTier.Intermediate => 0.75,
            JobTier.Advanced => 0.70,
            JobTier.Special => 0.65,
            _ => 0.80
        };

        private static void ProcessAIResult(TownJobRequest req)
        {
            if (!TownEconomyManager.Towns.TryGetValue(TownNumber.GetID(new Point3D(0,0,0), Map.Felucca), out var dummy)) 
                return;
            
            var town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == req.TownName);
            if (town == null) return;

            if (!string.IsNullOrEmpty(req.TargetHouseName))
            {
                var house = town.Houses.FirstOrDefault(h => h.HouseName == req.TargetHouseName);
                if (house != null)
                {
                    if (house.UnfulfilledNeeds.ContainsKey(req.TargetType))
                    {
                        house.UnfulfilledNeeds[req.TargetType] = Math.Max(0, house.UnfulfilledNeeds[req.TargetType] - req.TotalRequired);
                    }
                    // 변경점: AI가 대신 처리했을 경우, 유저 납품 정보가 없으므로 가장 기초 자원(CraftResource.None)으로 입고
                    house.AlterWarehouseItem(req.TargetType, CraftResource.None, false, req.TotalRequired, -1);
                }
                return; 
            }

            switch (req.Category)
            {
                case JobCategory.Menial:
                case JobCategory.Gathering: 
                case JobCategory.Crafting: 
                    if (!town.Warehouse.ContainsKey(req.TargetType)) 
                    {
                        int basePrice = req.RewardGold > 0 ? req.RewardGold / Math.Max(1, req.TotalRequired) : 50;
                        town.Warehouse[req.TargetType] = new WarehouseItem(req.TargetType, 0, basePrice, 10);
                    }
                    town.Warehouse[req.TargetType].Stock += req.TotalRequired;
                    break;
                    
                case JobCategory.Delivery: 
                    town.Wealth += (int)(req.RewardGold * 1.5); 
                    break;
                    
                case JobCategory.BlackMarket: 
                    town.Wealth += req.RewardGold * 3; 
                    town.SecurityPriceModifier += 0.05; 
                    break;
            }
        }
		public static void PerformDailyReset()
        {
            m_LastDailyReset = DateTime.Now;
            foreach (PartTimeAccountProfile p in Profiles.Values)
            {
                IAccount acc = Accounts.GetAccount(p.AccountName);
                int charCount = acc != null ? acc.Length : 1;
                if (charCount == 0) charCount = 1;
                p.AvailableCharges = Math.Min(charCount * 14, p.AvailableCharges + (charCount * 2));
            }
        }

        public static void PerformRefresh()
        {
            m_LastRefresh = DateTime.Now;

            foreach (TownEconomy town in TownEconomyManager.Towns.Values) 
            {
                if (string.IsNullOrEmpty(town.TownName))
                    town.TownName = TownNumber.GetName(town.TownID);

                ForceGenerateForTown(town);
            }
        }

        public static void AddOrUpdateJobRequest(TownJobRequest newReq)
        {
            var existing = ActiveRequests.FirstOrDefault(r => r.TownName == newReq.TownName && r.Title == newReq.Title && r.Category == newReq.Category);
            
            if (existing != null)
            {
                existing.TotalRequired += newReq.AmountPerPlayer;
                existing.CreationTime = DateTime.Now; 
            }
            else
            {
                ActiveRequests.Add(newReq);
            }
        }

        public static void ForceGenerateForTown(TownEconomy town)
        {
            if (string.IsNullOrEmpty(town.TownName)) return;

            var info = TownNumber.GetInfo(town.TownID);
            int limitPerTab = info.Grade switch { "S" => 20, "A" => 16, "B" => 12, "C" => 10, _ => 10 };
            
            JobCategory[] categories = Enum.GetValues<JobCategory>();

            foreach (JobCategory cat in categories)
            {
                int currentJobs = ActiveRequests.Count(r => r.TownName == town.TownName && r.Category == cat && !r.IsAIAssigned);
                if (currentJobs >= limitPerTab) continue;

                int toGenerate = limitPerTab - currentJobs;

                int targetTownJobs = (int)(toGenerate * 0.6);
                int targetCitizenJobs = toGenerate - targetTownJobs;

                int townJobsCreated = GenerateDemandDrivenJobs(town, cat, targetTownJobs, JobOrigin.TownPublic, info.Grade);
                int citizenJobsCreated = GenerateCitizenDrivenJobs(town, cat, targetCitizenJobs, info.Grade);

                int remaining = toGenerate - (townJobsCreated + citizenJobsCreated);
                for (int i = 0; i < remaining; i++)
                {
                    AddOrUpdateJobRequest(CreateFallbackRequest(town, cat, info.Grade));
                }
            }
        }

        public static string GetKoreanName(Type type)
        {
            if (type == null) return "알 수 없음";

            if (type.IsSubclassOf(typeof(Item)))
            {
                try
                {
                    Item tempItem = (Item)Activator.CreateInstance(type);
                    int cliloc = tempItem.LabelNumber;
                    tempItem.Delete(); 

                    if (cliloc > 0)
                    {
                        string korName = ClilocData.GetString(cliloc);
                        if (korName != "Unknown") return korName;
                    }
                }
                catch { }
            }
            
            return type.Name switch
            {
                "SeaSerpent" => "바다뱀", "DeepSeaSerpent" => "심해 바다뱀", "Kraken" => "크라켄",
                "Orc" => "오크", "Troll" => "트롤", "Ogre" => "오우거", "Gargoyle" => "가고일", "Lich" => "리치", 
                "Daemon" => "데몬", "Dragon" => "드래곤", "Drake" => "드레이크", "Slime" => "슬라임",
                "Lizardman" => "리자드맨", "Skeleton" => "스켈레톤", "Zombie" => "좀비", "Wraith" => "레이스",
                "GiantSpider" => "거대 거미", "Harpy" => "하피", "DireWolf" => "다이어 울프", "OgreLord" => "오우거 군주",
                "BloodElemental" => "피의 정령", "Balron" => "발론",
                "EarthElemental" => "대지의 정령", "WaterElemental" => "물의 정령", 
                "FireElemental" => "불의 정령", "AirElemental" => "바람의 정령",
                _ => type.Name
            };
        }

        private static int GenerateDemandDrivenJobs(TownEconomy town, JobCategory cat, int amountToGenerate, JobOrigin origin, string townGrade)
        {
            if (amountToGenerate <= 0 || town.Warehouse.Count == 0) return 0;

            var deficientItems = town.Warehouse
                .Where(kvp => kvp.Value.TargetStock > kvp.Value.Stock && GetCategoryForType(kvp.Key) == cat)
                .Select(kvp => new 
                { 
                    ItemType = kvp.Key, 
                    Deficit = kvp.Value.TargetStock - kvp.Value.Stock,
                    DeficitRatio = (double)(kvp.Value.TargetStock - kvp.Value.Stock) / Math.Max(1, kvp.Value.TargetStock)
                })
                .OrderByDescending(x => x.DeficitRatio)
                .ToList();

            int created = 0;
            JobTier tier = GetRandomTier(townGrade);
            int tMultiplier = (int)tier + 1;

            foreach (var item in deficientItems)
            {
                if (created >= amountToGenerate) break;

                int requestAmount = Math.Min(item.Deficit, 20 * tMultiplier); 
                if (requestAmount < 2) continue; 

                int currentUnitPrice = town.GetPrice(item.ItemType);
                int finalReward = (int)(currentUnitPrice * requestAmount * GetMarginRate(tier)); 
                
                if (finalReward < 10) finalReward = 10 * requestAmount;

                AddOrUpdateJobRequest(new TownJobRequest
                {
                    ID = Guid.NewGuid(),
                    Origin = origin,
                    TownName = town.TownName,
                    TargetHouseName = null, 
                    Category = cat,
                    Tier = tier,
                    Title = $"[공용 물자] {GetKoreanName(item.ItemType)} 납품 {requestAmount}개",
                    TargetType = item.ItemType,
                    TotalRequired = requestAmount * m_Random.Next(2, 5), 
                    AmountPerPlayer = requestAmount,
                    TimeLimit = TimeSpan.FromHours(2),
                    CreationTime = DateTime.Now,
                    RewardGold = finalReward,
                    RewardFame = tMultiplier * 50
                });

                created++;
            }
            return created;
        }

        private static int GenerateCitizenDrivenJobs(TownEconomy town, JobCategory cat, int amountToGenerate, string townGrade)
        {
            if (amountToGenerate <= 0 || town.Houses.Count == 0) return 0;

            int created = 0;
            JobTier tier = GetRandomTier(townGrade);
            int tMultiplier = (int)tier + 1;

            var activeHouses = town.Houses.Where(h => h.IsActive && h.UnfulfilledNeeds.Count > 0).OrderBy(x => Guid.NewGuid()).ToList();

            foreach (var house in activeHouses)
            {
                if (created >= amountToGenerate) break;

                var validNeeds = house.UnfulfilledNeeds.Where(kvp => kvp.Value > 0 && GetCategoryForType(kvp.Key) == cat).ToList();
                if (validNeeds.Count == 0) continue;

                var need = validNeeds[m_Random.Next(validNeeds.Count)];

                int requestAmount = Math.Min(need.Value, 10 * tMultiplier);
                if (requestAmount < 1) continue; 

                int currentUnitPrice = town.GetPrice(need.Key);
                double tipRate = 1.0 + Math.Min(0.1, house.Prestige / 10000.0);
                int finalReward = (int)(currentUnitPrice * requestAmount * GetMarginRate(tier) * tipRate);

                AddOrUpdateJobRequest(new TownJobRequest
                {
                    ID = Guid.NewGuid(),
                    Origin = JobOrigin.CitizenPrivate,
                    TownName = town.TownName,
                    TargetHouseName = house.HouseName, 
                    Category = cat,
                    Tier = tier,
                    Title = $"[{house.HouseName} 의뢰] {GetKoreanName(need.Key)} {requestAmount}개",
                    TargetType = need.Key,
                    TotalRequired = requestAmount, 
                    AmountPerPlayer = requestAmount,
                    TimeLimit = TimeSpan.FromHours(2),
                    CreationTime = DateTime.Now,
                    RewardGold = finalReward,
                    RewardFame = tMultiplier * 100 
                });

                created++;
            }
            return created;
        }

        public static JobCategory GetCategoryForType(Type type)
        {
            if (type == null) return JobCategory.Menial;

            if (type == typeof(IronOre) || type.IsSubclassOf(typeof(BaseOre)) ||
                type == typeof(Log) || type == typeof(Board) ||
                type == typeof(Hides) || type == typeof(Leather) ||
                type == typeof(RawFishSteak) || type == typeof(Fish))
            {
                return JobCategory.Gathering;
            }

            if (type.IsSubclassOf(typeof(BaseWeapon)) || 
                type.IsSubclassOf(typeof(BaseArmor)) ||
                type.IsSubclassOf(typeof(BasePotion)) ||
                type.IsSubclassOf(typeof(Food)) ||
                type == typeof(Bandage) || type == typeof(Candle) ||
                type.IsSubclassOf(typeof(BaseClothing)))
            {
                return JobCategory.Crafting;
            }

            return JobCategory.Menial;
        }

        private static TownJobRequest CreateFallbackRequest(TownEconomy town, JobCategory cat, string townGrade)
        {
            JobTier tier = GetRandomTier(townGrade);
            int tMultiplier = (int)tier + 1;

            Type targetType = typeof(Gold);
            string title = "기타 업무 1건";
            int baseReward = 100;
            int amount = 1;
            string region = null;

            switch (cat)
            {
                case JobCategory.Menial:
                case JobCategory.Gathering:
                case JobCategory.Crafting:
                    var availableKeys = town.Warehouse.Keys.ToList();
                    if (availableKeys.Count > 0)
                    {
                        var categoryKeys = availableKeys.Where(k => GetCategoryForType(k) == cat).ToList();
                        targetType = categoryKeys.Count > 0 ? categoryKeys[m_Random.Next(categoryKeys.Count)] : availableKeys[m_Random.Next(availableKeys.Count)];
                    }
                    else
                    {
                        targetType = cat == JobCategory.Gathering ? typeof(IronOre) : typeof(Dagger);
                    }

                    bool isConsumable = targetType.Name.Contains("Potion") || targetType.Name.Contains("Scroll") || targetType.Name.Contains("Food") || targetType.Name.Contains("Bottle");
                    amount = (isConsumable ? 15 : 5) * tMultiplier; 
                    
                    title = $"[공용 비축] {GetKoreanName(targetType)} 추가 조달 {amount}개"; 

                    int itemValue = town.GetPrice(targetType);
                    baseReward = (int)(itemValue * amount * GetMarginRate(tier));
                    if (baseReward < 50) baseReward = 50 * amount; 
                    break;

                case JobCategory.Delivery:
                    targetType = typeof(TownDeliveryLetter);
                    amount = 1;

                    var otherTowns = TownEconomyManager.Towns.Values
                        .Where(t => t.TownName != town.TownName && t.Facet == town.Facet)
                        .OrderBy(x => Guid.NewGuid()).ToList(); 

                    TownEconomy destTown = otherTowns.FirstOrDefault() ?? town;
                    
                    int distance = (int)Utility.GetDistanceToSqrt(town.Center, destTown.Center);
                    baseReward = 5000 + (distance * 10); 

                    string dName = destTown.TownName.ToLower();
                    if (dName.Contains("sea market") || dName.Contains("magincia") || dName.Contains("ocllo") || dName.Contains("nujel") || dName.Contains("moonglow") || dName.Contains("skara") || dName.Contains("jhelom"))
                    {
                        baseReward *= 3; 
                    }

                    baseReward = (int)(baseReward * GetMarginRate(tier));

                    title = $"[특급 배달] {destTown.TownName}의 역장에게 중요 서신 배송"; 
                    region = destTown.TownName; 
                    break;

                case JobCategory.EcoHunting:
                    Type ecoTarget = typeof(DireWolf); 
                    string regionNameForEco = null;

                    var facetZones = EcosystemManager.ZoneList.Where(z => z.Facet == town.Facet && z.Nodes.Count > 0).ToList();
                    
                    if (facetZones.Count > 0)
                    {
                        var nearestZone = facetZones.OrderBy(z => Utility.GetDistanceToSqrt(town.Center, z.Nodes[0].Location)).First();
                        var targetNode = nearestZone.Nodes[m_Random.Next(nearestZone.Nodes.Count)];

                        var spawnPool = EcoSpawnDatabase.GetPoolFor(targetNode);
                        ecoTarget = EcoSpawnDatabase.RollFromPool(spawnPool);
                        
                        regionNameForEco = nearestZone.ZoneId;
                    }

                    targetType = ecoTarget; 
                    amount = 5 * tMultiplier; 

                    if (!string.IsNullOrEmpty(regionNameForEco))
                        title = $"[{regionNameForEco} 정화] {GetKoreanName(targetType)} 개체수 조절 {amount}마리"; 
                    else
                        title = $"[생태 정화] {GetKoreanName(targetType)} 개체수 조절 {amount}마리"; 

                    baseReward = (int)(2500 * tMultiplier * GetMarginRate(tier)); 
                    break;

                case JobCategory.DungeonHunting:
                    var dData = GetDungeonHuntingData(town, tier);
                    targetType = dData.TargetType; 
                    title = dData.Title;
                    baseReward = (int)(dData.BaseReward * GetMarginRate(tier)); 
                    amount = dData.Amount; 
                    region = dData.RegionName;
                    tier = dData.FinalTier; 
                    break;

                case JobCategory.BlackMarket:
                    targetType = typeof(DragonBlood); 
                    amount = 5 * tMultiplier; 
                    title = $"[암시장] 희귀한 용의 피 은밀한 조달 {amount}개"; 
                    baseReward = (int)(5000 * tMultiplier * GetMarginRate(tier)); 
                    region = "Destard";
                    break;
            }

            return new TownJobRequest
            {
                Origin = JobOrigin.TownPublic,
                TownName = town.TownName,
                Category = cat,
                Tier = tier,
                Title = title,
                TargetType = targetType,
                RegionName = region, 
                TotalRequired = amount * m_Random.Next(2, 6),
                AmountPerPlayer = amount,
                TimeLimit = TimeSpan.FromHours(2),
                CreationTime = DateTime.Now,
                RewardGold = baseReward,
                RewardFame = tMultiplier * 100
            };
        }

        private static JobTier GetRandomTier(string grade)
        {
            int maxTier = grade switch { "S" => 4, "A" => 3, "B" => 2, "C" => 1, _ => 1 };
            return (JobTier)m_Random.Next(maxTier);
        }

        private static (Type TargetType, string Title, int BaseReward, int Amount, string RegionName, JobTier FinalTier) GetDungeonHuntingData(TownEconomy town, JobTier requestedTier)
        {
            Point3D townLoc = town.Center;
            var validZones = new List<(DungeonZone Zone, double Distance)>();

            foreach (var kvp in DungeonManager.Zones)
            {
                DungeonZone zone = kvp.Value;
                if (zone.Facet != town.Facet) continue; 

                // 🌟 수정 1: 물리 노드 스캔 대신, 새로운 구역(AreaBounds)의 중앙 좌표를 직접 호출
                Point3D entrance = zone.GetCenterLocation();
                if (entrance != Point3D.Zero)
                {
                    double dist = Utility.GetDistanceToSqrt(townLoc, entrance);
                    validZones.Add((zone, dist));
                }
            }

            if (validZones.Count == 0) return GetDynamicFallbackData(requestedTier);

            var candidates = validZones.Where(z => z.Distance < 2500).ToList();
            if (candidates.Count == 0) candidates = validZones; 
            
            candidates = candidates.OrderBy(x => Guid.NewGuid()).ToList(); 
            DungeonZone targetZone = candidates[0].Zone;

            double heatRatio = targetZone.TargetHeat > 0 ? (double)targetZone.CurrentHeat / targetZone.TargetHeat : 0.0;
            int maxTierByHeat = targetZone.TargetHeat >= 1500000 ? 3 : (targetZone.TargetHeat >= 500000 ? 2 : 1);
            JobTier finalTier = (JobTier)Math.Min((int)requestedTier, maxTierByHeat);

            Type[] targetProfile = null;
            
            // 🌟 수정 2: SpawnProfiles의 Key가 int(Tier)로 바뀌었으므로 Enum 대신 숫자 1, 2, 3으로 명확하게 조회하여 에러 소거
            switch (finalTier)
            {
                case JobTier.Beginner: 
                    if (targetZone.SpawnProfiles.ContainsKey(1)) targetProfile = targetZone.SpawnProfiles[1]; 
                    break;
                case JobTier.Intermediate: 
                    if (targetZone.SpawnProfiles.ContainsKey(2)) targetProfile = targetZone.SpawnProfiles[2]; 
                    break;
                case JobTier.Advanced: 
                    if (targetZone.SpawnProfiles.ContainsKey(3)) targetProfile = targetZone.SpawnProfiles[3]; 
                    break;
                case JobTier.Special: 
                    targetProfile = targetZone.BossType != null ? new Type[] { targetZone.BossType } : (targetZone.SpawnProfiles.ContainsKey(3) ? targetZone.SpawnProfiles[3] : null); 
                    break;
            }

            if (targetProfile == null || targetProfile.Length == 0) return GetDynamicFallbackData(finalTier);
            
            Type selectedMob = targetProfile[m_Random.Next(targetProfile.Length)];

            double rewardRatio = 0.5 + (heatRatio * 0.5); 
            int requiredAmount = finalTier switch { JobTier.Beginner => 15, JobTier.Intermediate => 30, JobTier.Advanced => 45, JobTier.Special => 1, _ => 15 };
            int baseReward = finalTier switch { JobTier.Beginner => 1500, JobTier.Intermediate => 3000, JobTier.Advanced => 4500, JobTier.Special => 5000, _ => 1500 };

            int finalReward = (int)(baseReward * rewardRatio); 

            string dungeonName = NewSpawnManager.GetDisplayName(targetZone.RCode);
            if (string.IsNullOrEmpty(dungeonName)) dungeonName = targetZone.RCode.ToString().Replace("_", " ");

            string title = $"[{dungeonName}] 위협적인 {GetKoreanName(selectedMob)} 토벌 {requiredAmount}마리";

            return (selectedMob, title, finalReward, requiredAmount, targetZone.RCode.ToString(), finalTier);
        }

        private static (Type TargetType, string Title, int BaseReward, int Amount, string RegionName, JobTier FinalTier) GetDynamicFallbackData(JobTier tier)
        {
            int t = (int)tier + 1;
            Type[] mobs = tier switch {
                JobTier.Beginner => new Type[] { typeof(Orc), typeof(Lizardman), typeof(Skeleton), typeof(Zombie) },
                JobTier.Intermediate => new Type[] { typeof(Troll), typeof(Ogre), typeof(Gargoyle), typeof(Wraith) },
                JobTier.Advanced => new Type[] { typeof(Lich), typeof(Daemon), typeof(Drake), typeof(OgreLord) },
                JobTier.Special => new Type[] { typeof(Dragon), typeof(BloodElemental), typeof(Balron) },
                _ => new Type[] { typeof(Orc) }
            };

            Type selectedMob = mobs[m_Random.Next(mobs.Length)];
            string mobName = GetKoreanName(selectedMob);
            int amount = tier == JobTier.Special ? 1 : 10 * t;
            int reward = (tier == JobTier.Special ? 5000 : 1500 * t) + m_Random.Next(100, 500);

            string[] regions = { "부근 숲", "근교 동굴", "인근 폐허", "외곽 지대" };
            string region = regions[m_Random.Next(regions.Length)];

            return (selectedMob, $"[{region}] {mobName} 무리 소탕 {amount}마리", reward, amount, null, tier);
        }

        public static bool CanAcceptJob(Mobile m, TownJobRequest req)
        {
            PartTimeAccountProfile p = GetProfile(m);
            if (p == null || req.Tier > p.CurrentTier || p.AvailableCharges <= 0) return false;
            return true;
        }

        public static PartTimeAccountProfile GetProfile(Mobile m)
        {
            if (m?.Account == null) return null;
            if (!Profiles.TryGetValue(m.Account.Username, out PartTimeAccountProfile p)) 
                Profiles[m.Account.Username] = p = new PartTimeAccountProfile(m.Account.Username);
            return p;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "PartTimeSystem", "Profiles.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true);
                writer.Write(2); 
                writer.Write(m_LastDailyReset); writer.Write(m_LastRefresh);
                
                writer.Write(Profiles.Count); 
                foreach (PartTimeAccountProfile p in Profiles.Values) p.Serialize(writer);
                
                writer.Write(ActiveRequests.Count);
                foreach(var req in ActiveRequests) req.Serialize(writer);
                
                writer.Close();
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "PartTimeSystem", "Profiles.bin");
            if (!File.Exists(path)) return;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));
                int v = reader.ReadInt(); 
                m_LastDailyReset = reader.ReadDateTime(); m_LastRefresh = reader.ReadDateTime();
                
                int pCount = reader.ReadInt(); 
                for (int i = 0; i < pCount; i++) { PartTimeAccountProfile p = new PartTimeAccountProfile(reader); Profiles[p.AccountName] = p; }
                
                if (v >= 2) 
                {
                    int rCount = reader.ReadInt();
                    for (int i = 0; i < rCount; i++) ActiveRequests.Add(new TownJobRequest(reader));
                }
                reader.Close();
            }
        }

        // 🌟 수정: AI 발주 시 가문 정보를 받아 IssuerHouse와 TargetHouseName을 정상 매핑하도록 보강
        public static void CreateAIRequest(string townName, string title, JobCategory cat, Type targetType, int amount, int totalReward, VirtualHouse house)
        {
            if (string.IsNullOrEmpty(townName) || targetType == null || house == null) return;

            AddOrUpdateJobRequest(new TownJobRequest
            {
                ID = Guid.NewGuid(),
                Origin = JobOrigin.CitizenPrivate, 
                TownName = townName,
                TargetHouseName = house.HouseName, // 가문명 기입
                IssuerHouse = house, // 오브젝트 직접 연결
                Category = cat,
                Tier = JobTier.Beginner, 
                Title = title,
                TargetType = targetType,
                TotalRequired = amount,
                AmountPerPlayer = amount, 
                TimeLimit = TimeSpan.FromHours(2), 
                RewardGold = totalReward,
                RewardFame = 50, 
                CreationTime = DateTime.Now,
                IsAIAssigned = false
            });
        }
    }
    
    public class PartTimeQuest : BaseQuest
    {
        public PartTimeJob JobData { get; set; }
        public override object Title => JobData?.Title ?? "파트타임 업무";
        public override object Description => JobData == null ? 
            "마을 공공 근로 업무입니다." : 
            $"업무 기한: {JobData.ExpireTime:HH:mm} 까지 (2시간 이내)\n" +
            $"기한 내에 완수하지 못하면 업무는 자동으로 파기됩니다.";
        public override TimeSpan RestartDelay => TimeSpan.FromMinutes(30.0);
        public PartTimeQuest() : base() { }
        
        public PartTimeQuest(PartTimeJob job) : base()
        {
            JobData = job;

            if (JobData != null)
            {
                if (JobData.Category == JobCategory.Delivery)
                {
                    AddObjective(new ObtainObjective(typeof(TownDeliveryLetter), "중요 배달 서신", 1));
                }
                else if (JobData.Category == JobCategory.EcoHunting || JobData.Category == JobCategory.DungeonHunting)
                {
                    int timeLimitSeconds = (int)(JobData.ExpireTime - DateTime.Now).TotalSeconds;
                    if (!string.IsNullOrEmpty(JobData.RegionName))
                        AddObjective(new SlayObjective(JobData.TargetType, PartTimeManager.GetKoreanName(JobData.TargetType), JobData.RequiredAmount, timeLimitSeconds, JobData.RegionName));
                    else
                        AddObjective(new SlayObjective(JobData.TargetType, PartTimeManager.GetKoreanName(JobData.TargetType), JobData.RequiredAmount, timeLimitSeconds));
                }
                else
                {
                    AddObjective(new ObtainObjective(JobData.TargetType, PartTimeManager.GetKoreanName(JobData.TargetType), JobData.RequiredAmount));
                }
            }
        }

        public override void OnAccept()
        {
            base.OnAccept();
            PartTimeAccountProfile profile = PartTimeManager.GetProfile(Owner);
            if (profile != null && JobData != null)
            {
                profile.CurrentJob = JobData;
                profile.AvailableCharges--;
                var req = PartTimeManager.ActiveRequests.FirstOrDefault(r => r.ID == JobData.RequestID);
                if (req != null) req.CurrentParticipants++; 

                if (JobData.Category == JobCategory.Delivery)
                {
                    string destTown = string.IsNullOrEmpty(JobData.RegionName) ? "인근 마을" : JobData.RegionName;
                    
                    TownDeliveryLetter letter = new TownDeliveryLetter(destTown, "시장/역장", JobData.ExpireTime);
                    Owner.AddToBackpack(letter);
                    Owner.SendMessage(38, "가방에 중요한 배달 서신이 들어왔습니다. 마법적인 공간 이동이 차단됩니다!");
                }
            }
        }

        public override void OnResign(bool resignChain)
        {
            PartTimeAccountProfile profile = PartTimeManager.GetProfile(Owner);
            if (profile != null && profile.CurrentJob != null)
            {
                var req = PartTimeManager.ActiveRequests.FirstOrDefault(r => r.ID == profile.CurrentJob.RequestID);
                if (req != null) req.CurrentParticipants = Math.Max(0, req.CurrentParticipants - 1);
                
                if (profile.CurrentJob.Category == JobCategory.Delivery)
                {
                    Item letter = Owner.Backpack?.FindItemByType(typeof(TownDeliveryLetter));
                    if (letter != null) letter.Delete();
                }

                profile.CurrentJob = null;
                Owner.SendMessage(33, "업무를 포기했습니다.");
            }
            base.OnResign(resignChain);
        }

        // 🌟 수정: 아이템의 9단계 재질 및 품질을 스캔하여 귀족 가문 창고에 정확하게 넘겨주는 핵심 로직 (Iron Exploit 방어)
        public override void GiveRewards()
        {
            if (Owner == null || JobData == null) return;

            CraftResource turnInRes = CraftResource.None;
            bool isExc = false;

            // 1. 가방 내 납품 대상 아이템 스캔 (아이템이 삭제되기 전 미리 재질/품질 획득)
            if (JobData.Category == JobCategory.Crafting || JobData.Category == JobCategory.Gathering)
            {
                Item foundItem = Owner.Backpack?.FindItemByType(JobData.TargetType);
                if (foundItem != null)
                {
                    var prop = foundItem.GetType().GetProperty("Resource");
                    if (prop != null)
                    {
                        var resVal = prop.GetValue(foundItem);
                        if (resVal is CraftResource cr) turnInRes = cr; // 미스릴, 옵시디언 등 9단계 자원 동적 추출 완료
                    }

                    if (foundItem is IQuality q) isExc = (q.Quality == ItemQuality.Exceptional);
                    else
                    {
                        var qProp = foundItem.GetType().GetProperty("Quality");
                        if (qProp != null)
                        {
                            object val = qProp.GetValue(foundItem);
                            isExc = (val is int i && i == 2) || val?.ToString() == "Exceptional";
                        }
                    }
                }
            }

            // 2. 울티마 온라인 기본 퀘스트 아이템 삭제 처리 실행
            QuestHelper.DeleteItems(this);
            
            Banker.Deposit(Owner, JobData.RewardGold);
            Owner.SendMessage(63, "업무 완수! 보상금 {0}gp가 입금되었습니다.", JobData.RewardGold.ToString());

            if (Owner.Account is Server.Accounting.Account acc)
            {
                Server.Misc.FamilySystem.Contribute(acc.Username, 10, Server.Items.FamilyCompType.Economy, false);
            }

            if (TownEconomyManager.Towns.TryGetValue(TownNumber.GetID(new Point3D(0,0,0), Map.Felucca), out var dummy))
            {
                var town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == JobData.TownName);
                if (town != null)
                {
                    if (!string.IsNullOrEmpty(JobData.TargetHouseName))
                    {
                        var house = town.Houses.FirstOrDefault(h => h.HouseName == JobData.TargetHouseName);
                        if (house != null)
                        {
                            if (house.UnfulfilledNeeds.ContainsKey(JobData.TargetType))
                            {
                                house.UnfulfilledNeeds[JobData.TargetType] = Math.Max(0, house.UnfulfilledNeeds[JobData.TargetType] - JobData.RequiredAmount);
                            }
                            
                            // 🌟 변경점: 추출한 정확한 재질(turnInRes)과 품질(isExc)을 가문 창고로 직배송
                            house.AlterWarehouseItem(JobData.TargetType, turnInRes, isExc, JobData.RequiredAmount, -1);
                        }
                    }
                    else
                    {
                        if (JobData.Category == JobCategory.Gathering || JobData.Category == JobCategory.Crafting || JobData.Category == JobCategory.Menial)
                        {
                            if (!town.Warehouse.ContainsKey(JobData.TargetType)) 
                            {
                                int basePrice = JobData.RewardGold > 0 ? JobData.RewardGold / Math.Max(1, JobData.RequiredAmount) : 50;
                                town.Warehouse[JobData.TargetType] = new WarehouseItem(JobData.TargetType, 0, basePrice, 10);
                            }
                            town.Warehouse[JobData.TargetType].Stock += JobData.RequiredAmount;
                        }
                    }
                }
            }

            PartTimeAccountProfile p = PartTimeManager.GetProfile(Owner);
            if (p != null)
            {
                p.TotalCompleted++;
                p.CurrentJob = null;
            }

            AllRemoveQuest();
        }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); writer.Write(JobData != null); if (JobData != null) JobData.Serialize(writer); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); if (reader.ReadBool()) JobData = new PartTimeJob(reader); }
    }
}