using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public class PhysicalAdventurer : BaseCreature
    {
        private VirtualAdventurer m_Source;
        public VirtualAdventurer Source => m_Source;

        public PhysicalAdventurer(VirtualAdventurer source) 
            : base(GetAIType(source), FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            m_Source = source;

            // 1. 기본 외형 및 타이틀
            this.Name = source.Name;
            this.Female = source.IsFemale;
            this.Body = source.IsFemale ? 0x191 : 0x190;
            this.Hue = source.IsFemale ? 0x83E1 : 0x83EA;
            this.Title = $"[{source.Role}]";

            // 🌟 [CreatureBalancer 방식 적용] Read-only 에러 해결
            // Mobile 클래스의 Raw 스탯과 BaseCreature의 Seed를 직접 수정합니다.
            this.RawStr = 100 + (source.Level * 2);
            this.RawDex = 100 + (source.Level * 2);
            this.RawInt = 100 + (source.Level * 2);
            
            this.HitsMaxSeed = source.MaxHP;
            this.Hits = source.HP;
            this.ManaMaxSeed = 100 + (source.Level * 5);
            this.Mana = this.ManaMax;
            this.StamMaxSeed = 100 + (source.Level * 2);
            this.Stam = this.StamMax;

            // 2. 스킬 주입
            double skillVal = (double)source.CombatSkill;
            SetSkill(SkillName.Swords, skillVal);
            SetSkill(SkillName.Tactics, skillVal);
            SetSkill(SkillName.Anatomy, skillVal);
            SetSkill(SkillName.MagicResist, skillVal);
            
            // 🌟 [수정] VirtualAdventurer 내부에 AiType 프로퍼티가 있다고 가정하거나
            // 직접 GetAiTypeByJob 로직을 사용합니다.
            if (GetAiTypeByJob(source.JobClass) == AdventurerAiType.Mage) 
                SetSkill(SkillName.Magery, skillVal);
            else if (GetAiTypeByJob(source.JobClass) == AdventurerAiType.Archer) 
                SetSkill(SkillName.Archery, skillVal);

            // 3. 팀 및 전투 설정
            if (source.Party != null) 
                this.Team = source.Party.TeamID;

            this.FightMode = FightMode.Closest;

            // 4. 장비 및 인벤토리
            EquipVirtualItems();

            if (source.Backpack != null)
            {
                var items = source.Backpack.Items.ToList();
                foreach (var item in items) 
                { 
                    if (item != null && !item.Deleted) 
                        this.Backpack.DropItem(item); 
                }
            }
        }

        // 내부 AI 판정용 로직 (VirtualAdventurer 참조 에러 방지용 직접 구현)
        private static AdventurerAiType GetAiTypeByJob(NpcJobClass job) => job switch {
            NpcJobClass.Knight or NpcJobClass.Paladin => AdventurerAiType.Paladin,
            NpcJobClass.Wizard or NpcJobClass.Necromancer => AdventurerAiType.Mage,
            NpcJobClass.Archer_Expert or NpcJobClass.Crossbowman => AdventurerAiType.Archer,
            _ => AdventurerAiType.Melee
        };

        private static AIType GetAIType(VirtualAdventurer v) => GetAiTypeByJob(v.JobClass) switch
        {
            AdventurerAiType.Mage or AdventurerAiType.Necro => AIType.AI_Mage,
            AdventurerAiType.Archer => AIType.AI_Archer,
            _ => AIType.AI_Melee
        };

        private void EquipVirtualItems()
        {
            if (m_Source == null) return;
            foreach (var kvp in m_Source.VirtualEquipments)
            {
                try
                {
                    Item item = (Item)Activator.CreateInstance(kvp.Value);
                    if (item != null) { if (!this.EquipItem(item)) item.Delete(); }
                }
                catch { }
            }
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m is PhysicalAdventurer other && other.Team == this.Team && this.Team != 0) return false;
            if (m is PlayerMobile) return false;
            return base.IsEnemy(m);
        }

        public override void OnDelete()
        {
            if (m_Source != null)
            {
                m_Source.HP = this.Hits;
                if (this.Backpack != null)
                {
                    foreach (var item in this.Backpack.Items.ToList()) m_Source.Backpack.DropItem(item);
                }
                m_Source.PhysicalObject = null;
            }
            base.OnDelete();
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            if (m_Source != null) m_Source.Die();
        }

        public PhysicalAdventurer(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}