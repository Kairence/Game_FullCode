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

namespace Server.Misc
{
    // [수정] 기획하신 AI 역할군에 맞춰 100~1100번대로 카테고리 명확화
    public enum JobCategory 
    { 
        Menial = 100, Gathering = 101,     // 노동자, 농부, 어부 (기초 자원)
        Crafting = 200,                    // 장인, 제작공 (물품 납품, 인프라)
        Delivery = 300,                    // 상단, 셰르파 (무역, 호위)
        EcoHunting = 500,                  // 사냥꾼 (필드 생태계 정화, 해양 몬스터)
        DungeonHunting = 600,              // 모험가, 기사 (던전 열기도 억제)
        BlackMarket = 1100                 // 도적, 암살자 (장물, 희귀품)
    }

    public enum JobTier { Beginner = 0, Intermediate = 1, Advanced = 2, Special = 3 }
    public enum JobOrigin { TownPublic, CitizenPrivate }

    public class TownJobRequest
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public JobOrigin Origin { get; set; }
        public string TownName { get; set; }
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

        // [신규] 퀘스트 생성 시간 및 AI 수락 여부 플래그
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public bool IsAIAssigned { get; set; } = false;

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
        }

        public void Serialize(GenericWriter w)
        {
            w.Write(0); // version
            w.Write(ID.ToString()); w.Write((int)Origin); w.Write(TownName); w.Write((int)Category); w.Write((int)Tier);
            w.Write(Title); w.Write(TargetType?.FullName ?? ""); w.Write(RegionName ?? "");
            w.Write((int)RequiredResource); w.Write(RequireExceptional); w.Write(TotalRequired); w.Write(AmountPerPlayer);
            w.Write(CurrentParticipants); w.Write(TimeLimit); w.Write(RewardGold); w.Write(RewardFame);
            w.Write(BonusRewardType?.FullName ?? ""); w.Write(CreationTime); w.Write(IsAIAssigned);
        }
    }

    public class PartTimeJob
    {
        public Guid RequestID { get; set; }
        public JobOrigin Origin { get; set; }
        public string TownName { get; set; }
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
            RequestID = req.ID; Origin = req.Origin; TownName = req.TownName; Category = req.Category; 
            Title = req.Title; TargetType = req.TargetType; RegionName = req.RegionName; 
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
        }

        public void Serialize(GenericWriter w)
        {
            w.Write(2); // version
            w.Write(RequestID.ToString()); w.Write((int)Origin); w.Write(TownName); w.Write((int)Category); 
            w.Write(Title); w.Write(TargetType?.FullName ?? ""); w.Write((int)RequiredResource);
            w.Write(RequiredAmount); w.Write(CurrentAmount); w.Write(RequireExceptional); 
            w.Write(ExpireTime); w.Write(RewardGold); w.Write(RewardFame); w.Write(BonusRewardType?.FullName ?? "");
            w.Write(RegionName ?? "");
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
        public static Dictionary<string, PartTimeAccountProfile> Profiles = [];
        public static List<TownJobRequest> ActiveRequests = [];
        private static DateTime m_LastDailyReset;
        private static DateTime m_LastRefresh;
        private static readonly Random m_Random = new();

        public static void Configure() { EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; }
        public static void Initialize() { Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), CheckSystem); }

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

        private static void ProcessAIResult(TownJobRequest req)
        {
            if (!TownEconomyManager.Towns.TryGetValue(TownNumber.GetID(new Point3D(0,0,0), Map.Felucca), out var dummy)) 
                return;
            
            var town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == req.TownName);
            if (town == null) return;

            switch (req.Category)
            {
                case JobCategory.Menial:
                case JobCategory.Gathering: 
                    if (!town.Warehouse.ContainsKey(req.TargetType)) town.Warehouse[req.TargetType] = new WarehouseItem(req.TargetType, 0, 10, 100);
                    town.Warehouse[req.TargetType].Stock += req.TotalRequired;
                    break;
                    
                case JobCategory.Crafting: 
                    if (req.TargetType == typeof(WoodenBox)) 
                        town.MaxInventoryCapacity += 50; 
                    else
                    {
                        if (!town.Warehouse.ContainsKey(req.TargetType)) town.Warehouse[req.TargetType] = new WarehouseItem(req.TargetType, 0, req.RewardGold / 2, 10);
                        town.Warehouse[req.TargetType].Stock += req.AmountPerPlayer;
                    }
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
                    ActiveRequests.Add(CreateFallbackRequest(town, cat, info.Grade));
                }
            }
        }

        // 🌟 [개선] ClilocData를 이용한 자동 한글화 (노가다 switch문 제거!)
        public static string GetKoreanName(Type type)
        {
            if (type == null) return "알 수 없음";

            // 1. 아이템인 경우: 임시로 생성해서 LabelNumber(Cliloc ID)를 추출 후 한글 사전에서 검색
            if (type.IsSubclassOf(typeof(Item)))
            {
                try
                {
                    Item tempItem = (Item)Activator.CreateInstance(type);
                    int cliloc = tempItem.LabelNumber;
                    tempItem.Delete(); // 메모리 낭비 방지를 위해 즉시 삭제

                    if (cliloc > 0)
                    {
                        string korName = ClilocData.GetString(cliloc);
                        if (korName != "Unknown") return korName;
                    }
                }
                catch { }
            }
            
            // 2. 몬스터(Mobile)인 경우: LabelNumber가 명확하지 않으므로 최소한의 이름만 수동 매핑
            return type.Name switch
            {
                "SeaSerpent" => "바다뱀", "DeepSeaSerpent" => "심해 바다뱀", "Kraken" => "크라켄",
                "Orc" => "오크", "Troll" => "트롤", "Ogre" => "오우거", "Gargoyle" => "가고일", "Lich" => "리치", 
                "Daemon" => "데몬", "Dragon" => "드래곤", "Drake" => "드레이크",
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
                    DeficitRatio = (double)(kvp.Value.TargetStock - kvp.Value.Stock) / Math.Max(1, kvp.Value.TargetStock),
                    BasePrice = kvp.Value.BasePrice
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
                if (requestAmount < 5) continue; 

                int finalReward = (int)(item.BasePrice * requestAmount * 1.5 * town.PriceMultiplier); 

                ActiveRequests.Add(new TownJobRequest
                {
                    ID = Guid.NewGuid(),
                    Origin = origin,
                    TownName = town.TownName,
                    Category = cat,
                    Tier = tier,
                    Title = $"긴급 조달: {GetKoreanName(item.ItemType)} {requestAmount}개",
                    TargetType = item.ItemType,
                    TotalRequired = requestAmount * m_Random.Next(2, 5),
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

        private static int GenerateCitizenDrivenJobs(TownEconomy town, JobCategory cat, int amountToGenerate, string townGrade)
        {
            if (amountToGenerate <= 0 || town.Houses.Count == 0) return 0;

            Dictionary<Type, int> totalCitizenNeeds = new Dictionary<Type, int>();
            foreach (var house in town.Houses)
            {
                foreach (var need in house.UnfulfilledNeeds)
                {
                    if (GetCategoryForType(need.Key) == cat)
                    {
                        if (!totalCitizenNeeds.ContainsKey(need.Key)) totalCitizenNeeds[need.Key] = 0;
                        totalCitizenNeeds[need.Key] += need.Value;
                    }
                }
            }

            var sortedNeeds = totalCitizenNeeds.OrderByDescending(kvp => kvp.Value).ToList();
            
            int created = 0;
            JobTier tier = GetRandomTier(townGrade);
            int tMultiplier = (int)tier + 1;

            foreach (var need in sortedNeeds)
            {
                if (created >= amountToGenerate) break;

                int requestAmount = Math.Min(need.Value, 10 * tMultiplier);
                if (requestAmount < 2) continue; 

                int basePrice = town.Warehouse.ContainsKey(need.Key) ? town.Warehouse[need.Key].BasePrice : 50;
                int finalReward = (int)(basePrice * requestAmount * 2.0 * town.PriceMultiplier); 

                ActiveRequests.Add(new TownJobRequest
                {
                    ID = Guid.NewGuid(),
                    Origin = JobOrigin.CitizenPrivate,
                    TownName = town.TownName,
                    Category = cat,
                    Tier = tier,
                    Title = $"가문 생필품 구함: {GetKoreanName(need.Key)} {requestAmount}개",
                    TargetType = need.Key,
                    TotalRequired = requestAmount * m_Random.Next(1, 3),
                    AmountPerPlayer = requestAmount,
                    TimeLimit = TimeSpan.FromHours(2),
                    CreationTime = DateTime.Now,
                    RewardGold = finalReward,
                    RewardFame = tMultiplier * 150 
                });

                created++;
            }

            return created;
        }

        private static JobCategory GetCategoryForType(Type type)
        {
            Type[] gatheringTypes = [typeof(IronOre), typeof(Log), typeof(WheatSheaf), typeof(RawFishSteak), typeof(Hides), typeof(Fish), typeof(Wool)];
            if (gatheringTypes.Contains(type)) return JobCategory.Gathering;
            
            if (type == typeof(GoldRing) || type == typeof(DragonBlood)) return JobCategory.BlackMarket;
            
            return JobCategory.Crafting; 
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
                    Type[] gathers = [typeof(IronOre), typeof(Log), typeof(WheatSheaf)];
                    targetType = gathers[m_Random.Next(gathers.Length)]; 
                    amount = 20 * tMultiplier; 
                    title = $"{GetKoreanName(targetType)} 조달 {amount}개"; 
                    baseReward = 500 * tMultiplier; 
                    break;
                case JobCategory.Crafting:
                    targetType = typeof(Broadsword); 
                    amount = 5 * tMultiplier; 
                    title = $"마을 수비대 무기 납품 {amount}개"; 
                    baseReward = 2000 * tMultiplier; 
                    break;
                case JobCategory.Delivery:
                    targetType = typeof(CommodityDeed); 
                    amount = 1; 
                    title = "인근 마을 무역 호위 1회"; 
                    baseReward = 2500 * tMultiplier; 
                    break;
                case JobCategory.EcoHunting:
                    targetType = typeof(SeaSerpent); 
                    amount = 5 * tMultiplier; 
                    title = $"해안가 바다뱀 토벌 {amount}마리"; 
                    baseReward = 3000 * tMultiplier; 
                    region = "Ocean";
                    break;
                case JobCategory.DungeonHunting:
                    var dData = GetDungeonHuntingData(town, tier);
                    targetType = dData.TargetType; 
                    title = dData.Title;
                    baseReward = dData.BaseReward; 
                    amount = dData.Amount; 
                    region = dData.RegionName;
                    break;
                case JobCategory.BlackMarket:
                    targetType = typeof(DragonBlood); 
                    amount = 5 * tMultiplier; 
                    title = $"희귀한 용의 피 밀수 {amount}개"; 
                    baseReward = 5000 * tMultiplier; 
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
                RewardGold = (int)(baseReward * town.PriceMultiplier),
                RewardFame = tMultiplier * 100
            };
        }

        private static JobTier GetRandomTier(string grade)
        {
            int maxTier = grade switch { "S" => 4, "A" => 3, "B" => 2, "C" => 1, _ => 1 };
            return (JobTier)m_Random.Next(maxTier);
        }

        private static (Type TargetType, string Title, int BaseReward, int Amount, string RegionName) GetDungeonHuntingData(TownEconomy town, JobTier tier)
        {
            int t = (int)tier + 1;
            Point3D townLoc = TownNumber.GetCenter(town.TownID);

            DungeonZone targetDungeon = null;
            double minDist = double.MaxValue;

            foreach (var zone in DungeonManager.Zones.Values)
            {
                Point3D? entrance = NewSpawnManager.FindLocationByRegionCode(zone.RCode, zone.Facet);
                if (entrance.HasValue)
                {
                    double dist = Utility.GetDistanceToSqrt(townLoc, entrance.Value);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        targetDungeon = zone;
                    }
                }
            }

            if (targetDungeon != null)
            {
                int spawnedCount = 0;
                List<Mobile> aliveMobs = new List<Mobile>();

                if (targetDungeon.ActiveMonsters != null)
                {
                    foreach (var list in targetDungeon.ActiveMonsters.Values)
                    {
                        if (list != null)
                        {
                            var alive = list.Where(m => m != null && m.Alive).ToList();
                            spawnedCount += alive.Count;
                            aliveMobs.AddRange(alive);
                        }
                    }
                }

                if (spawnedCount > 0 && aliveMobs.Count > 0)
                {
                    Type selectedMob = aliveMobs[m_Random.Next(aliveMobs.Count)].GetType();
                    string dungeonName = NewSpawnManager.GetDisplayName(targetDungeon.RCode);

                    double heatPercent = targetDungeon.TargetHeat > 0 
                        ? Math.Clamp((double)targetDungeon.CurrentHeat / targetDungeon.TargetHeat, 0.0, 1.0) 
                        : 0.0;

                    int baseReward = 2000 * t; 
                    int reward = (int)(baseReward * (1.0 + heatPercent));
                    int requiredAmount = Math.Max(5, Math.Min(30 * t, spawnedCount / 2));
                    
                    string title = $"[{dungeonName}] 위협적인 {GetKoreanName(selectedMob)} 토벌 {requiredAmount}마리";

                    return (selectedMob, title, reward, requiredAmount, targetDungeon.RCode.ToString());
                }
            }

            return (typeof(Orc), "부근 오크 무리 소탕 10마리", 2000 * t, 10 * t, null);
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

        public static void CreateAIRequest(string townName, string title, JobCategory cat, Type targetType, int amount, int totalReward)
        {
            if (string.IsNullOrEmpty(townName) || targetType == null) return;

            TownJobRequest req = new TownJobRequest
            {
                ID = Guid.NewGuid(),
                Origin = JobOrigin.CitizenPrivate, 
                TownName = townName,
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
            };

            ActiveRequests.Add(req);
        }
    }
    public class PartTimeQuest : BaseQuest
    {
        public PartTimeJob JobData { get; set; }
        public override object Title => JobData?.Title ?? "파트타임 업무";
        public override object Description => JobData == null ? 
            "마을 공공 근로 업무입니다." : 
            $"마을 공공 근로 업무입니다.\n\n" +
            $"<basefont color=#FF4500>※ 업무 기한: {JobData.ExpireTime:HH:mm} 까지 (2시간 이내)</basefont>\n" +
            $"기한 내에 완수하지 못하면 업무는 자동으로 파기됩니다.";
        public override TimeSpan RestartDelay => TimeSpan.FromMinutes(30.0);
        public PartTimeQuest() : base() { }
        
        public PartTimeQuest(PartTimeJob job) : base()
        {
            JobData = job;

            if (JobData != null)
            {
                // 🌟 한글 아이템/몬스터 이름이 퀘스트 우측 목표창에도 완벽하게 출력되도록 Mapper 함수 적용
                if (JobData.Category == JobCategory.EcoHunting || JobData.Category == JobCategory.DungeonHunting)
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
            }
        }

        public override void OnResign(bool resignChain)
        {
            PartTimeAccountProfile profile = PartTimeManager.GetProfile(Owner);
            if (profile != null && profile.CurrentJob != null)
            {
                var req = PartTimeManager.ActiveRequests.FirstOrDefault(r => r.ID == profile.CurrentJob.RequestID);
                if (req != null) req.CurrentParticipants = Math.Max(0, req.CurrentParticipants - 1);
                
                profile.CurrentJob = null;
                Owner.SendMessage(33, "업무를 포기했습니다.");
            }
            base.OnResign(resignChain);
        }

        public override void GiveRewards()
        {
            if (Owner == null || JobData == null) return;

            QuestHelper.DeleteItems(this);
            QuestHelper.Delay(Owner, typeof(PartTimeQuest), this.RestartDelay);

            Banker.Deposit(Owner, JobData.RewardGold);
            Owner.SendMessage(63, "업무 완수! 보상금 {0}gp가 입금되었습니다.", JobData.RewardGold.ToString());

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