using System;

namespace Server.Mobiles
{
    [CorpseName("a clockwork scorpion corpse")]
    public class ClockworkScorpion : BaseCreature, IRepairableMobile
    {
        public Type RepairResource { get { return typeof(Server.Items.IronIngot); } }

        [Constructable]
        public ClockworkScorpion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.4, 0.8)
        {
            Name = "a clockwork scorpion";
            Body = 717;

            /* Clockwork Scorpion - Fame 1,000 / Weak Mechanical Insect */
			this.SetStr(100, 150);       // 힘
			this.SetDex(150, 250);       // 민첩
			this.SetInt(30, 50);         // 지능

			// [Hits] 최종 약 2,000 ~ 2,500 타겟 (보너스 약 1,750 제외)
			this.SetHits(250, 750); 
			this.SetStam(150, 250);      // 기력
			this.SetMana(30, 50);        // 마나

			SetAttackSpeed(2.5);
			SetDamage(14, 22);      // 데미지

			// [Damage Type] 속성 타입
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistance] 저항
			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Cold, 5, 15);
			this.SetResistance(ResistanceType.Poison, 100); // 기계라 독 면역
			this.SetResistance(ResistanceType.Energy, 5, 10);

			// [Skills] 스킬
			this.SetSkill(SkillName.Wrestling, 50.0, 60.0);
			this.SetSkill(SkillName.Tactics, 50.0, 60.0);

			this.VirtualArmor = 10;      // 가상 방어력
			this.Tamable = false;        // 테이밍 불가 (기계)

			this.Fame = 1000;            // 명성
			this.Karma = -1000;          // 카르마
        }

        public ClockworkScorpion(Serial serial)
            : base(serial)
        {
        }

        public override bool IsScaredOfScaryThings
        {
            get
            {
                return false;
            }
        }
        public override bool IsScaryToPets
        {
            get
            {
                return true;
            }
        }
        public override bool IsBondable
        {
            get
            {
                return false;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return !Controlled;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override bool DeleteOnRelease
        {
            get
            {
                return true;
            }
        }
        public override bool BardImmune
        {
            get
            {
                return !Core.AOS || Controlled;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager, 2);
        }

        public override int GetAngerSound()
        {
            return 541;
        }

        public override int GetIdleSound()
        {
            if (!Controlled)
                return 542;

            return base.GetIdleSound();
        }

        public override int GetDeathSound()
        {
            if (!Controlled)
                return 545;

            return base.GetDeathSound();
        }

        public override int GetAttackSound()
        {
            return 562;
        }

        public override int GetHurtSound()
        {
            if (Controlled)
                return 320;

            return base.GetHurtSound();
        }
  
        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            Mobile master = GetMaster();

            if (master != null && master.Player && master.Map == Map && master.InRange(Location, 20))
            {
                if (master.Mana >= amount)
                {
                    master.Mana -= amount;
                }
                else
                {
                    amount -= master.Mana;
                    master.Mana = 0;
                    master.Damage(amount);
                }
            }

            base.OnDamage(amount, from, willKill);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }
    }
}
