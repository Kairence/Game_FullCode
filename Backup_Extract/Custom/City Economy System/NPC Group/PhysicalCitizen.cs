using System;
using System.Linq;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    public class PhysicalCitizen : BaseCreature
    {
        public VirtualCitizen Brain { get; set; }
        private int m_CurrentGameHour = -1;

        // 🌟 [무적 해제 패치 1] 엔진 차원의 무적 속성을 원천 차단합니다.
        public override bool IsInvulnerable => false;
        public override bool CanBeDamaged() => true;

        public PhysicalCitizen(VirtualCitizen data) : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Brain = data;

            string rawName = data.Name;
            if (rawName.Contains(" the ")) 
                rawName = rawName.Split(new string[] { " the " }, StringSplitOptions.None)[0];

            Name = rawName;
            Title = $"[{data.TargetRegionName}의 {data.JobClass}]"; 
            
            // 🌟 [무적 해제 패치 2] 초기 생성 시 무적 상태를 확실히 끕니다.
            Blessed = false;     
            CantWalk = false;   
            AccessLevel = AccessLevel.Player;
            
            // 이름 색 결정을 위한 성향치 설정
            Karma = data.Karma; 
            Fame = data.Fame;
            
            int baseStat = 50 + (int)(data.Potential * 10);
            SetStr(baseStat + (data.RankLevel >= NobilityRank.Knight ? 50 : 0));
            SetDex(baseStat);
            SetInt(baseStat);
            SetHits(100 + (int)(data.Potential * 50)); 
            SetDamage(5, 15); 

            Body = (data.Gender == Gender.Male) ? 0x190 : 0x191;
            Hue = Utility.RandomSkinHue();
            
            EquipByJob(); 
        }

        // ==========================================================
        // 🩸 1. 전투 중 살해당했을 때
        // ==========================================================
        public override void OnKilledBy(Mobile killer)
        {
            base.OnKilledBy(killer);
            if (Brain == null || Brain.IsExpired) return;

            if (killer is PlayerMobile pm)
            {
                TownSocialRegistry.ProcessMurder(Brain, pm); 
            }
            Brain.IsKilled = true; 
            
            CheckAndDemolishEmptyHouse(); 
        }

        // ==========================================================
        // 🩸 2. 강제 삭제 시 세대 교체 및 빈집 확인
        // ==========================================================
        public override void OnDelete()
        {
            if (Brain != null && !Brain.IsExpired)
            {
                Brain.IsKilled = true;
                TownEconomy town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == Brain.TargetRegionName);
                if (town != null && town.Citizens.Contains(Brain))
                {
                    TownSocietyEngine.PerformInheritance(Brain, town);
                }
                CheckAndDemolishEmptyHouse(); 
            }
            base.OnDelete();
        }

        private void CheckAndDemolishEmptyHouse()
        {
            if (Brain == null || Brain.House == null) return;
            TownEconomy town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == Brain.TargetRegionName);
            if (town == null || !town.Houses.Contains(Brain.House)) return;

            bool isHouseEmpty = true;
            foreach (var fam in Brain.House.Families)
            {
                if (fam.Father != null && !fam.Father.IsExpired) isHouseEmpty = false;
                if (fam.Mother != null && !fam.Mother.IsExpired) isHouseEmpty = false;
                foreach (var child in fam.Children) { if (child != null && !child.IsExpired) isHouseEmpty = false; }
            }

            if (isHouseEmpty)
            {
                Console.WriteLine($"[멸문] {Brain.House.HouseName} 가문의 집이 철거됩니다.");
                TownSocietyEngine.DemolishEstateArea(Brain.House, town);
                town.Houses.Remove(Brain.House);
            }
        }

        public override void OnThink()
        {
            base.OnThink();
            if (Brain == null || Brain.IsExpired) { this.Delete(); return; }

            int gameHour = (int)((DateTime.Now.TimeOfDay.TotalMinutes / 5.0) % 24);
            if (gameHour != m_CurrentGameHour)
            {
                m_CurrentGameHour = gameHour;
                ExecuteDailySchedule(gameHour);
            }
            PerformIdleAnimation(); 
        }

        private void ExecuteDailySchedule(int hour)
        {
            if (hour == 8) DoSmartMoveTo(GetWorkplaceLocation());
            else if (hour == 18) DoSmartMoveTo(GetTavernLocation());
            else if (hour == 22) DoSmartMoveTo(GetHomeLocation());
        }

        private void DoSmartMoveTo(Point3D dest)
        {
            if (dest == Point3D.Zero || this.Location == dest) return;

            string currentTown = GetCurrentTownName(this.Location, this.Map);
            string destTown = Brain.TargetRegionName ?? "";

            if (currentTown.Equals(destTown, StringComparison.OrdinalIgnoreCase) || IsLocalBoundTown(destTown))
            {
                MoveToWorld(dest, this.Map);
                return;
            }

            if (Brain.Gold >= 50)
            {
                Brain.Gold -= 50; 
                this.Animate(16, 5, 1, true, false, 0);
                this.PlaySound(0x243); 

                Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
                {
                    if (this.Deleted) return;
                    FixedParticles(0x3728, 10, 10, 2023, EffectLayer.Waist);
                    PlaySound(0x1FC);
                    MoveToWorld(dest, this.Map);
                    FixedParticles(0x3728, 10, 10, 2023, EffectLayer.Waist);
                    PlaySound(0x1FC);
                });
            }
            else { Brain.Stress += 15; }
        }

        private void PerformIdleAnimation()
        {
            if (Utility.RandomDouble() > 0.1) return;
            if (m_CurrentGameHour >= 8 && m_CurrentGameHour < 18)
            {
                switch (Brain.JobClass)
                {
                    case NpcJobClass.SurfaceMiner:
                    case NpcJobClass.StoneQuarryman: this.Animate(11, 5, 1, true, false, 0); this.PlaySound(Utility.RandomList(0x125, 0x126)); break;
                    case NpcJobClass.Woodcutter: this.Animate(13, 5, 1, true, false, 0); this.PlaySound(0x13E); break;
                    case NpcJobClass.DeepSeaFisher:
                    case NpcJobClass.CoastalFisher: this.Animate(12, 5, 1, true, false, 0); this.PlaySound(0x240); break;
                    case NpcJobClass.Smelter:
                    case NpcJobClass.PigIronWorker: this.Animate(9, 5, 1, true, false, 0); this.PlaySound(0x2A); break;
                    default: this.Animate(Utility.RandomBool() ? 5 : 6, 5, 1, true, false, 0); break;
                }
            }
            else if (m_CurrentGameHour >= 18 && m_CurrentGameHour < 22)
            {
                int roll = Utility.Random(100);
                if (roll < 30) { this.Animate(34, 5, 1, true, false, 0); this.PlaySound(Utility.RandomBool() ? 0x3A : 0x30); }
                else if (roll < 50) { this.Animate(33, 5, 1, true, false, 0); }
                else { this.Animate(5, 5, 1, true, false, 0); }
            }
            else { if (Utility.RandomDouble() < 0.2) this.Animate(32, 5, 1, true, false, 0); }
        }

        private Point3D GetScatteredLocation(Point3D baseLoc, int scatterRange)
        {
            if (this.Map == null || this.Map == Map.Internal) return baseLoc;
            int offsetX = Utility.RandomMinMax(-scatterRange, scatterRange);
            int offsetY = Utility.RandomMinMax(-scatterRange, scatterRange);
            return new Point3D(baseLoc.X + offsetX, baseLoc.Y + offsetY, this.Map.GetAverageZ(baseLoc.X + offsetX, baseLoc.Y + offsetY));
        }

        // ==========================================================
        // 🏠 [안방 리스폰] 바닥 타일 추적 및 벽 충돌 방지 로직
        // ==========================================================
        private Point3D GetHomeLocation()
        {
            if (Brain.House != null && Brain.House.EstateSign is VirtualEstateSign sign)
            {
                var floors = new List<Static>();
                var walls = new HashSet<Point2D>();

                foreach (var t in sign.AttachedTiles)
                {
                    if (t == null || t.Deleted) continue;
                    ItemData id = TileData.ItemTable[t.ItemID & TileData.MaxItemValue];

                    if ((id.Flags & TileFlag.Surface) != 0) floors.Add(t);
                    if ((id.Flags & TileFlag.Wall) != 0 || (id.Flags & TileFlag.Impassable) != 0)
                        walls.Add(new Point2D(t.X, t.Y));
                }

                var safeFloors = floors.Where(f => !walls.Contains(new Point2D(f.X, f.Y))).ToList();

                if (safeFloors.Count > 0)
                {
                    Static target = safeFloors[Utility.Random(safeFloors.Count)];
                    return new Point3D(target.X, target.Y, target.Z + 1);
                }
                
                return new Point3D(sign.X, sign.Y - 2, sign.Z + 7);
            }

            string tName = Brain.TargetRegionName?.ToLower() ?? "";
            if (tName.Contains("papua")) return GetScatteredLocation(new Point3D(5750, 3130, 0), 3);
            if (tName.Contains("delucia")) return GetScatteredLocation(new Point3D(5270, 4000, 0), 3);
            if (tName.Contains("magincia")) return GetScatteredLocation(new Point3D(3720, 2220, 20), 3);
            if (tName.Contains("sea market")) return GetScatteredLocation(new Point3D(62, 1941, 0), 2);

            return this.Location; 
        }

        private Point3D GetWorkplaceLocation()
        {
            Point3D home = GetHomeLocation();
            return GetScatteredLocation(new Point3D(home.X + Utility.RandomMinMax(10, 20), home.Y + Utility.RandomMinMax(10, 20), home.Z), 5);
        }

        private Point3D GetTavernLocation()
        {
            Point3D home = GetHomeLocation();
            return GetScatteredLocation(new Point3D(home.X + Utility.RandomMinMax(-10, 10), home.Y + Utility.RandomMinMax(-10, 10), home.Z), 3);
        }

        private string GetCurrentTownName(Point3D loc, Map map)
        {
            Region r = Region.Find(loc, map);
            return r != null ? r.Name : "Wilderness";
        }

        private bool IsLocalBoundTown(string tName)
        {
            tName = tName.ToLower();
            return tName.Contains("papua") || tName.Contains("delucia") || tName.Contains("magincia") || tName.Contains("sea market");
        }

        private void EquipByJob()
        {
            if (Brain.JobClass == NpcJobClass.SurfaceMiner) AddItem(new Pickaxe());
            else if (Brain.JobClass == NpcJobClass.Woodcutter) AddItem(new Hatchet());
            else if (Brain.JobClass == NpcJobClass.DeepSeaFisher) AddItem(new FishingPole());
        }

        // ==========================================================
        // 💾 직렬화 및 무적 해제 고정 로직
        // ==========================================================
        public PhysicalCitizen(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) 
        { 
            base.Deserialize(reader); 
            int version = reader.ReadInt(); 

            // 🌟 [무적 해제 패치 3] 서버 재시작 후 로드될 때 무적 속성을 다시 강제로 해제합니다.
            this.Blessed = false;
            this.AccessLevel = AccessLevel.Player;
        }
    }
}