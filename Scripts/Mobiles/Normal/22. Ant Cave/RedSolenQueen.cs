using System;
using System.Collections;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a solen queen corpse")]
    public class RedSolenQueen : BaseCreature, IRedSolen
    {
        private bool m_BurstSac;
        private static bool m_Laid;

        [Constructable]
        public RedSolenQueen()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a red solen queen";
            this.Body = 783;
            this.BaseSoundID = 959;

			/* [Red Solen Queen - Fame 15,000 / General / Weight 1.30]
			   - 스킬 200 마스터 서버용 '상급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (15,000/1000) - 5 = 10 (공격 특화 경량화)
			   - 흑개미 여왕보다 높은 데미지와 화염 속성 폭발력
			   -------------------------------------------------- */

			// [Attributes] 명성 15,000 보너스 + 가중치 1.30 반영
			this.SetStr(450, 550); 
			this.SetHits(10000, 12500); 
			this.SetDex(90, 120);
			this.SetInt(90, 120);

			SetAttackSpeed(2.5);
			SetDamage(75, 105);

			// [Damage Types] 50% 물리 + 50% 화염 (붉은 여왕의 분노)
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			// [Resistances] 총합 약 245 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 50, 60);
			this.SetResistance(ResistanceType.Fire, 75);         // 화염 완전 내성 (Max 75)
			this.SetResistance(ResistanceType.Cold, 15, 25);     // 치명적 약점: 냉기
			this.SetResistance(ResistanceType.Poison, 45, 55);
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] ★ 스킬 200 서버 기준 - 상급자용 핵심 타겟 (재설계)
			// 유저 스킬 130 ~ 170 구간에서 도전하기 적절한 수치
			this.SetSkill(SkillName.Wrestling, 125.0, 140.0); 
			this.SetSkill(SkillName.Tactics, 125.0, 140.0);
			this.SetSkill(SkillName.Anatomy, 130.0, 145.0); // 공격 효율 극대화
			this.SetSkill(SkillName.MagicResist, 115.0, 130.0);

			// [Misc] 가상 방어력(Virtual Armor): (15,000/1000) - 5 = 10
			this.VirtualArmor = 10;

			this.Fame = 15000;
			this.Karma = -15000;

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus((Utility.RandomDouble() > 0.05) ? 5 : 25));

            if (Utility.RandomDouble() < 0.05)
                this.PackItem(new BallOfSummoning());
        }

        public RedSolenQueen(Serial serial)
            : base(serial)
        {
        }

        public bool BurstSac
        {
            get
            {
                return this.m_BurstSac;
            }
        }
        public override int GetAngerSound()
        {
            return 0x259;
        }

        public override int GetIdleSound()
        {
            return 0x259;
        }

        public override int GetAttackSound()
        {
            return 0x195;
        }

        public override int GetHurtSound()
        {
            return 0x250;
        }

        public override int GetDeathSound()
        {
            return 0x25B;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {

            if (attacker.Weapon is BaseRanged)

                BeginAcidBreath();

            else if (this.Map != null && attacker != this && m_Laid == false && 0.20 > Utility.RandomDouble()) //  if (m_Talked == false)
            {
                RSQEggSac sac = new RSQEggSac();

                sac.MoveToWorld(this.Location, this.Map);
                PlaySound(0x582);
                Say(1114445); // * * The solen queen summons her workers to her aid! * *
                m_Laid = true;
                EggSacTimer e = new EggSacTimer();
                e.Start();
            }

            base.OnGotMeleeAttack(attacker);
        }

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

            if (0.80 >= Utility.RandomDouble())
                BeginAcidBreath();
        }

        #region Acid Breath
        private DateTime m_NextAcidBreath;

        public void BeginAcidBreath()
        {
            PlayerMobile m = Combatant as PlayerMobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || m_NextAcidBreath > DateTime.Now || !CanBeHarmful(m))
                return;

            PlaySound(0x118);
            MovingEffect(m, 0x36D4, 1, 0, false, false, 0x3F, 0);

            TimeSpan delay = TimeSpan.FromSeconds(GetDistanceToSqrt(m) / 5.0);
            Timer.DelayCall<Mobile>(delay, new TimerStateCallback<Mobile>(EndAcidBreath), m);

            m_NextAcidBreath = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void EndAcidBreath(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive || !Alive)
                return;

            if (0.2 >= Utility.RandomDouble())
                m.ApplyPoison(this, Poison.Greater);

            AOS.Damage(m, Utility.RandomMinMax(100, 120), 0, 0, 0, 100, 0);
        }
        #endregion

        private class EggSacTimer : Timer
        {
            public EggSacTimer()
                : base(TimeSpan.FromSeconds(10))
            {
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                m_Laid = false;

            }
        }

        public override bool IsEnemy(Mobile m)
        {
            if (SolenHelper.CheckRedFriendship(m))
                return false;
            else
                return base.IsEnemy(m);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            SolenHelper.OnRedDamage(from);

            if (!willKill)
            {
                if (!this.BurstSac)
                {
                    if (this.Hits < 50)
                    {
                        this.PublicOverheadMessage(MessageType.Regular, 0x3B2, true, "* The solen's acid sac is burst open! *");
                        this.m_BurstSac = true;
                    }
                }
                else if (from != null && from != this && this.InRange(from, 1))
                {
                    this.SpillAcid(from, 1);
                }
            }

            base.OnDamage(amount, from, willKill);
        }

        public override bool OnBeforeDeath()
        {
            this.SpillAcid(4);

            return base.OnBeforeDeath();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
            writer.Write(this.m_BurstSac);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
			
            switch( version )
            {
                case 1:
                    {
                        this.m_BurstSac = reader.ReadBool();
                        break;
                    }
            }
        }
    }

    public class RSQEggSac : Item, ICarvable
    {
        private SpawnTimer m_Timer;

        public override string DefaultName
        {
            get { return "egg sac"; }
        }

        [Constructable]
        public RSQEggSac()
            : base(4316)
        {
            Movable = false;
            Hue = 350;

            m_Timer = new SpawnTimer(this);
            m_Timer.Start();
        }

        public bool Carve(Mobile from, Item item)
        {
            Effects.PlaySound(GetWorldLocation(), Map, 0x027);
            Effects.SendLocationEffect(GetWorldLocation(), Map, 0x3728, 10, 10, 0, 0);

            from.SendMessage("You destroy the egg sac.");
            Delete();
            m_Timer.Stop();

            return true;
        }

        public RSQEggSac(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Timer = new SpawnTimer(this);
            m_Timer.Start();
        }

        private class SpawnTimer : Timer
        {
            private Item m_Item;

            public SpawnTimer(Item item)
                : base(TimeSpan.FromSeconds(Utility.RandomMinMax(5, 10)))
            {
                Priority = TimerPriority.FiftyMS;

                m_Item = item;
            }

            protected override void OnTick()
            {
                if (m_Item.Deleted)
                    return;

                Mobile spawn;

                switch (Utility.Random(2))
                {
                    case 0:
                        spawn = new RedSolenWarrior();
                        spawn.MoveToWorld(m_Item.Location, m_Item.Map);
                        m_Item.Delete();
                        break;
                    case 1:
                        spawn = new RedSolenWorker();
                        spawn.MoveToWorld(m_Item.Location, m_Item.Map);
                        m_Item.Delete();
                        break;
                }
            }
        }
    }
}
