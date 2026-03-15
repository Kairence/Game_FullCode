using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a solen warrior corpse")]
    public class BlackSolenWarrior : BaseCreature, IBlackSolen
    {
        private bool m_BurstSac;
        [Constructable]
        public BlackSolenWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a black solen warrior";
            this.Body = 806;
            this.BaseSoundID = 959;
            this.Hue = 0x453;

			/* [Black Solen Warrior - Fame 8,000 / General / Weight 1.16]
			   - 스킬 200 마스터 서버용 '중급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (8,000/1000) + 2 = 10
			   - 저항 밸런스: 최대 75 상한 엄격 준수
			   -------------------------------------------------- */

			// [Attributes] 명성 8,000 보너스 + 가중치 1.16 반영
			this.SetStr(110, 135); 
			this.SetHits(2500, 2800); 
			this.SetDex(20, 30);
			this.SetInt(20, 30);

			SetAttackSpeed(2.0);
			SetDamage(12, 18);

			// [Damage Types] 80% 물리 + 20% 독 속성
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Poison, 20);

			// [Resistances] 총합 약 205 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 50, 60);
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 25, 35);
			this.SetResistance(ResistanceType.Poison, 60, 70);
			this.SetResistance(ResistanceType.Energy, 20, 30);

			// [Skills] ★ 스킬 200 서버 기준 - 중반부 핵심 사냥용 (재설계)
			// 유저 스킬 80 ~ 100(그랜드 마스터) 구간 수련에 최적화
			this.SetSkill(SkillName.Wrestling, 75.0, 85.0); 
			this.SetSkill(SkillName.Tactics, 75.0, 85.0);
			this.SetSkill(SkillName.Anatomy, 70.0, 80.0);
			this.SetSkill(SkillName.MagicResist, 65.0, 75.0);

			// [Misc] 가상 방어력(Virtual Armor): (8,000/1000) + 2 = 10
			this.VirtualArmor = 10;

			this.Fame = 8000;
			this.Karma = -8000;

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus((0.05 > Utility.RandomDouble()) ? 13 : 3));

            if (Utility.RandomDouble() < 0.05)
                this.PackItem(new BraceletOfBinding());
        }

        public BlackSolenWarrior(Serial serial)
            : base(serial)
        {
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            if (attacker.Weapon is BaseRanged)
                BeginAcidBreath();

            base.OnGotMeleeAttack(attacker);
        }

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

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

        public bool BurstSac
        {
            get
            {
                return this.m_BurstSac;
            }
        }
        public override int GetAngerSound()
        {
            return 0xB5;
        }

        public override int GetIdleSound()
        {
            return 0xB5;
        }

        public override int GetAttackSound()
        {
            return 0x289;
        }

        public override int GetHurtSound()
        {
            return 0xBC;
        }

        public override int GetDeathSound()
        {
            return 0xE4;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 4));
        }

        public override bool IsEnemy(Mobile m)
        {
            if (SolenHelper.CheckBlackFriendship(m))
                return false;
            else
                return base.IsEnemy(m);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            SolenHelper.OnBlackDamage(from);

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
}
