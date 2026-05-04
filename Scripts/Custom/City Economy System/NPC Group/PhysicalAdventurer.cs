using System;
using System.Linq;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public class PhysicalAdventurer : BaseCreature
    {
        public VirtualAdventurer Brain { get; set; }

        // 🌟 무적 속성 원천 차단
        public override bool IsInvulnerable => false;
        public override bool CanBeDamaged() => true;

        public PhysicalAdventurer(VirtualAdventurer data) : base(DetermineAIType(data), FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Brain = data;

            string rawName = data.Name;
            if (rawName.Contains(" the ")) 
                rawName = rawName.Split(new string[] { " the " }, StringSplitOptions.None)[0];

            Name = rawName;
            Title = data.Party != null ? $"[{data.Party.Name}의 {data.JobClass}]" : $"[방랑하는 {data.JobClass}]";
            
            this.Female = data.IsFemale;
            this.Body = data.IsFemale ? 0x191 : 0x190;
            this.Hue = Utility.RandomSkinHue();

            Blessed = false;     
            CantWalk = false;   
            AccessLevel = AccessLevel.Player;
            
            Karma = data.Karma; 
            Fame = data.Fame;
            
            if (data.Party != null)
                Team = data.Party.TeamID;

            // ==========================================
            // ⚔️ [Combat 코드 이식 1] CreatureBalancer 우회 (스탯 직접 주입)
            // ==========================================
            this.RawStr = 100 + (data.Level * 2) + (data.Role == AdventurerRole.Tank || data.Role == AdventurerRole.MeleeDPS ? 50 : 0);
            this.RawDex = 100 + (data.Level * 2) + (data.Role == AdventurerRole.RangedDPS ? 50 : 0);
            this.RawInt = 100 + (data.Level * 2) + (data.Role == AdventurerRole.MagicDPS || data.Role == AdventurerRole.Healer ? 50 : 0);
            
            this.HitsMaxSeed = data.MaxHP;
            this.Hits = data.HP;
            this.ManaMaxSeed = 100 + (data.Level * 5);
            this.Mana = this.ManaMax;
            this.StamMaxSeed = 100 + (data.Level * 2);
            this.Stam = this.StamMax;

            SetDamage(10 + (data.EquipmentTier * 2), 20 + (data.EquipmentTier * 3));

            // ==========================================
            // ⚔️ [Combat 코드 이식 2] 스킬 주입 (공격 명중률 보장)
            // ==========================================
            double skillVal = (double)data.CombatSkill;
            SetSkill(SkillName.Swords, skillVal);
            SetSkill(SkillName.Tactics, skillVal);
            SetSkill(SkillName.Anatomy, skillVal);
            SetSkill(SkillName.MagicResist, skillVal);
            SetSkill(SkillName.Healing, skillVal);

            var aiType = DetermineAIType(data);
            if (aiType == AIType.AI_Mage || aiType == AIType.AI_Healer) 
                SetSkill(SkillName.Magery, skillVal);
            else if (aiType == AIType.AI_Archer) 
                SetSkill(SkillName.Archery, skillVal);

            // ==========================================
            // 장비, 탈것, 가방 동기화
            // ==========================================
            EquipVirtualItems(); 
            if (data.HasMount) EquipMount();

            // 🎒 [Combat 코드 이식 3] 가상 가방 -> 물리 가방으로 아이템 이동
            if (data.Backpack != null)
            {
                var items = data.Backpack.Items.ToList();
                foreach (var item in items) 
                { 
                    if (item != null && !item.Deleted) 
                        this.Backpack.DropItem(item); 
                }
            }
        }

        private static AIType DetermineAIType(VirtualAdventurer data)
        {
            return data.Role switch
            {
                AdventurerRole.MagicDPS => AIType.AI_Mage,
                AdventurerRole.Healer => AIType.AI_Healer,
                AdventurerRole.RangedDPS => AIType.AI_Archer,
                _ => AIType.AI_Melee
            };
        }

        private void EquipVirtualItems()
        {
            if (Brain == null || Brain.VirtualEquipments == null) return;

            foreach (var kvp in Brain.VirtualEquipments)
            {
                Type itemType = kvp.Value;
                try
                {
                    Item item = (Item)Activator.CreateInstance(itemType);
                    if (item != null)
                    {
                        item.LootType = LootType.Blessed; 
                        if (!this.EquipItem(item)) item.Delete();
                    }
                }
                catch { } 
            }
            
            this.HairItemID = 0x203B; 
            this.HairHue = Utility.RandomHairHue();
        }

        private void EquipMount()
        {
            Horse horse = new Horse();
            horse.Rider = this;
        }

        // ==========================================================
        // 🛡️ [Combat 코드 이식 4] 적군 판별 (플레이어 공격 방지)
        // ==========================================================
        public override bool IsEnemy(Mobile m)
        {
            if (m is PhysicalAdventurer other && other.Team == this.Team && this.Team != 0) return false;
            if (m is PlayerMobile) return false;
            return base.IsEnemy(m);
        }

        // ==========================================================
        // 🩸 사망 및 삭제 처리
        // ==========================================================
        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            if (Brain != null) Brain.Die(); 
        }

        public override void OnDelete()
        {
            if (Brain != null)
            {
                // 🩸 [Combat 코드 이식 5] 삭제될 때 깎인 체력을 가상 두뇌에 저장!
                Brain.HP = this.Hits;
                
                // 🎒 물리 가방에 남은 템들을 다시 가상 가방으로 회수
                if (this.Backpack != null && Brain.Backpack != null)
                {
                    foreach (var item in this.Backpack.Items.ToList()) 
                        Brain.Backpack.DropItem(item);
                }
                Brain.PhysicalObject = null;
            }
            base.OnDelete();
        }

        // ==========================================================
        // 🧠 파티 걷기, 겹침 방지 산개, 대기 애니메이션 로직
        // ==========================================================
        public override void OnThink()
        {
            base.OnThink();
            if (Brain == null) { this.Delete(); return; }

            if (Brain.Party != null)
            {
                if (Brain.Party.Members.Count > 0 && Brain.Party.Members[0] != this.Brain)
                {
                    var leaderVirtual = Brain.Party.Members[0];
                    if (leaderVirtual.PhysicalObject != null && !leaderVirtual.PhysicalObject.Deleted)
                    {
                        var leaderPhysical = leaderVirtual.PhysicalObject;
                        double dist = Utility.GetDistanceToSqrt(this.Location, leaderPhysical.Location);

                        if (dist > 15.0)
                        {
                            MoveToWorld(GetScatteredLocation(leaderPhysical.Location, 2), leaderPhysical.Map);
                        }
                        else if (dist > 2.0)
                        {
                            Direction d = this.GetDirectionTo(leaderPhysical.Location);
                            this.Move(d);
                        }
                    }
                }
                
                if (Brain.Party.State == AdventurerState.Resting)
                {
                    PerformIdleAnimation();
                }
            }
        }

        private void PerformIdleAnimation()
        {
            if (Utility.RandomDouble() > 0.05) return;

            if (Brain.Role == AdventurerRole.Support) 
            {
                this.Animate(34, 5, 1, true, false, 0); 
                this.PlaySound(Utility.RandomBool() ? 0x3A : 0x4C);
            }
            else if (Brain.Role == AdventurerRole.MagicDPS || Brain.Role == AdventurerRole.Healer)
                this.Animate(17, 5, 1, true, false, 0); 
            else
                this.Animate(5, 5, 1, true, false, 0); 
        }

        private Point3D GetScatteredLocation(Point3D baseLoc, int scatterRange)
        {
            if (this.Map == null || this.Map == Map.Internal) return baseLoc;

            for (int i = 0; i < 5; i++) 
            {
                int offsetX = Utility.RandomMinMax(-scatterRange, scatterRange);
                int offsetY = Utility.RandomMinMax(-scatterRange, scatterRange);
                if (offsetX == 0 && offsetY == 0) continue; 

                int x = baseLoc.X + offsetX;
                int y = baseLoc.Y + offsetY;
                int z = this.Map.GetAverageZ(x, y);

                return new Point3D(x, y, z);
            }
            return baseLoc; 
        }

        public PhysicalAdventurer(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) 
        { 
            base.Deserialize(reader); 
            reader.ReadInt(); 
            this.Blessed = false;
            this.AccessLevel = AccessLevel.Player;
        }
    }
}