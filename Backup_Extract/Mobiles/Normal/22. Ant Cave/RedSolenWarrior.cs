using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a solen warrior corpse")]
    public class RedSolenWarrior : BaseCreature, IRedSolen
    {
        private bool m_BurstSac;
        [Constructable]
        public RedSolenWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a red solen warrior";
            this.Body = 782;
            this.BaseSoundID = 959;

			/* [Red Solen Warrior - Fame 8,000 / General / Weight 1.22]
			   - 스킬 200 마스터 서버용 '중급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (8,000/1000) - 3 = 5 (경량 공격형 갑각)
			   - 흑개미 전사보다 높은 Str와 화염 속성 공격 가미
			   -------------------------------------------------- */

			// [Attributes] 명성 8,000 보너스 + 가중치 1.22 반영
			this.SetStr(150, 180); 
			this.SetHits(3500, 3800); 
			this.SetDex(30, 40);
			this.SetInt(30, 40);

			SetAttackSpeed(2.2);
			SetDamage(38, 55);

			// [Damage Types] 70% 물리 + 30% 화염 속성 (붉은 솔렌의 호전성)
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Fire, 30);

			// [Resistances] 총합 약 200 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, 60, 70);      // 화염 저항 우수
			this.SetResistance(ResistanceType.Cold, 15, 25);      // 냉기 약점 확실
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 25, 35);

			// [Skills] ★ 스킬 200 서버 기준 - 중반부 핵심 사냥용 (재설계)
			// 유저 스킬 80 ~ 100(그마) 구간 수련에 최적화
			this.SetSkill(SkillName.Wrestling, 75.0, 85.0); 
			this.SetSkill(SkillName.Tactics, 75.0, 85.0);
			this.SetSkill(SkillName.Anatomy, 80.0, 95.0); // 공격적 성향 반영
			this.SetSkill(SkillName.MagicResist, 60.0, 70.0);

			// [Misc] 가상 방어력(Virtual Armor): (8,000/1000) - 3 = 5
			this.VirtualArmor = 5;

			this.Fame = 8000;
			this.Karma = -8000;

            SolenHelper.PackPicnicBasket(this);
            this.PackItem(new ZoogiFungus((0.05 < Utility.RandomDouble()) ? 3 : 13));

            if (Utility.RandomDouble() < 0.05)
                this.PackItem(new BraceletOfBinding());
        }

        public RedSolenWarrior(Serial serial)
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
}
