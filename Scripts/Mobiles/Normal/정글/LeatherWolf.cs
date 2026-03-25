using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a wolf corpse")]
    public class LeatherWolf : BaseCreature, IRepairableMobile
    {
        public Type RepairResource { get { return typeof(IronIngot); } }

        private const int MaxFellows = 3;

        private List<Mobile> m_Fellows = new List<Mobile>();
        private Timer m_FellowsTimer;

        [Constructable]
        public LeatherWolf()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a leather wolf";
            Body = 739;
            BaseSoundID = 0xE5;
			
			/* [Leather Wolf - Normal - Fame 13,000 / Weight 1.25]
			   - 정글 던전의 가죽 외골격 늑대 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 13 (명성/1000 공식 준수)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(340, 360); 
			this.SetHits(7600, 7850); 
			this.SetDex(110, 130);
			this.SetInt(110, 130);

			// [Combat Options] 물리 100% (날카로운 발톱과 이빨)
			this.SetDamage(30, 50);
			this.SetAttackSpeed(2.2); // 늑대 특유의 빠른 공속
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 15, 25);      // ★ 확실한 약점 (잘 타는 가죽)
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 110~120에 역산 보너스(11.6) 가산
			this.SetSkill(SkillName.Wrestling, 121.0, 131.0); 
			this.SetSkill(SkillName.Tactics, 121.0, 131.0);
			this.SetSkill(SkillName.Anatomy, 121.0, 131.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 1; // 200 숙련도 시대의 초반 주력 1슬롯 펫
			this.MinTameSkill = 110.5; 
			this.VirtualArmor = 13;
			this.Fame = 13000;
			this.Karma = -13000;
            SetWeaponAbility(WeaponAbility.BleedAttack);
        }

        public LeatherWolf(Serial serial)
            : base(serial)
        {
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (!Controlled && 0.2 > Utility.RandomDouble())
                c.DropItem(new LeatherWolfSkin());         
        }

        public override void OnCombatantChange()
        {
            if (Combatant != null && m_FellowsTimer == null)
            {
                m_FellowsTimer = new InternalTimer(this);
                m_FellowsTimer.Start();
            }
        }

        public void CheckFellows()
        {
            if (!Alive || Combatant == null || Map == null || Map == Map.Internal)
            {
                m_Fellows.ForEach(f => f.Delete());
                m_Fellows.Clear();

                m_FellowsTimer.Stop();
                m_FellowsTimer = null;
            }
            else
            {
                for (int i = 0; i < m_Fellows.Count; i++)
                {
                    Mobile friend = m_Fellows[i];

                    if (friend.Deleted)
                        m_Fellows.Remove(friend);
                }

                bool spawned = false;

                for (int i = m_Fellows.Count; i < MaxFellows; i++)
                {
                    BaseCreature friend = new LeatherWolfFellow();

                    friend.MoveToWorld(Map.GetSpawnPosition(Location, 6), Map);
                    friend.Combatant = Combatant;

                    if (friend.AIObject != null)
                        friend.AIObject.Action = ActionType.Combat;

                    m_Fellows.Add(friend);

                    spawned = true;
                }

                if (spawned)
                {
                    Say(1113132); // The leather wolf howls for help
                    PlaySound(0xE6);
                }
            }
        }

        private class InternalTimer : Timer
        {
            private LeatherWolf m_Owner;

            public InternalTimer(LeatherWolf owner)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(30.0))
            {
                m_Owner = owner;
            }

            protected override void OnTick()
            {
                m_Owner.CheckFellows();
            }
        }

        public override bool AlwaysMurderer { get { return true; } }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Canine;
            }
        }
        public override int Hides
        {
            get
            {
                return 7;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager, 2);
        }

        public override int GetIdleSound()
        {
            return 1545;
        }

        public override int GetAngerSound()
        {
            return 1542;
        }

        public override int GetHurtSound()
        {
            return 1544;
        }

        public override int GetDeathSound()
        {
            return 1543;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class LeatherWolfFellow : BaseCreature
    {
        [Constructable]
        public LeatherWolfFellow()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a leather wolf";
            Body = 739;
            BaseSoundID = 0xE5;

            SetStr(105, 115);
            SetDex(101, 114);
            SetInt(23, 34);

            SetHits(81, 110);

            SetDamage(9, 20);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 36, 50);
            SetResistance(ResistanceType.Fire, 10, 18);
            SetResistance(ResistanceType.Cold, 23, 29);
            SetResistance(ResistanceType.Poison, 10, 17);
            SetResistance(ResistanceType.Energy, 10, 15);

            SetSkill(SkillName.MagicResist, 59.2, 75);
            SetSkill(SkillName.Tactics, 53.3, 64.8);
            SetSkill(SkillName.Wrestling, 64, 79);

            Fame = 2500;
            Karma = -2500;
        }

        public override PackInstinct PackInstinct { get { return PackInstinct.Canine; } }

        public LeatherWolfFellow(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            /*int version = */
            reader.ReadInt();
        }
    }
}
