using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a wolf spider spider corpse")]
    public class WolfSpider : BaseCreature
    {
        [Constructable]
        public WolfSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a Wolf spider";
            Body = 736;
            Hue = 0;

            /* Wolf Spider - Fame 6,500 */
			this.SetStr(200, 250);  // 보너스(+1,085) 합산 시 약 1,300
			this.SetDex(120, 140);   
			this.SetInt(50, 70);     

			this.SetHits(300, 500);  // 보너스(+13,177) 합산 시 약 13,600
			this.SetStam(120, 140);

			this.SetAttackSpeed(2.2);   
			SetDamage(25, 35);

			this.SetSkill(SkillName.Wrestling, 115.1); // 보너스 +20.1 반영
			this.SetSkill(SkillName.Tactics, 115.1);
			this.SetSkill(SkillName.Anatomy, 100.0);

			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Cold, 20);

			// 저항 패널티: 물리 데미지가 1.5배로 박힘
			this.SetResistance(ResistanceType.Physical, -50, -40);
			this.SetResistance(ResistanceType.Energy, -40, -30);
			this.VirtualArmor = 5;

			this.Tamable = true;
			this.ControlSlots = 2;
			this.MinTameSkill = 95.1;

			this.Fame = 6500;
			this.Karma = -6500;
			
        }
		
        public WolfSpider(Serial serial)
            : base(serial)
        {
        }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Arachnid;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }

        public override void GenerateLoot()
        {
            PackItem(new SpidersSilk(8));
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Gems, 2);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            //if (!Controlled && Utility.RandomDouble() < 0.01)
            //    c.DropItem(new LuckyCoin());
        }
		
		/*
        public void BeginAcidBreath()
        {
            Mobile m = Combatant as Mobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || !CanBeHarmful(m) || m.Poisoned )
                return;

			AOS.Damage( m, this, Utility.RandomMinMax( 7, 8 ), false, 0, 0, 0, 100, 0 );

            PlaySound(0x118);
            MovingEffect(m, 0x36D4, 1, 0, false, false, 0x3F, 0);

           // TimeSpan delay = TimeSpan.FromSeconds(GetDistanceToSqrt(m) / 5.0);
           // Timer.DelayCall<Mobile>(delay, new TimerStateCallback<Mobile>(EndAcidBreath), m);

            m_NextAcidBreath = DateTime.Now + TimeSpan.FromSeconds(5);
        }
		
        public override void OnGotMeleeAttack(Mobile attacker)
        {
            BeginAcidBreath();
            base.OnGotMeleeAttack(attacker);
        }

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

            BeginAcidBreath();
        }
		*/
        public override int GetIdleSound()
        {
            return 1605;
        }

        public override int GetAngerSound()
        {
            return 1602;
        }

        public override int GetHurtSound()
        {
            return 1604;
        }

        public override int GetDeathSound()
        {
            return 1603;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
            {
                Hue = 0;
                Body = 736;
            }

            if (version == 1 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }
    }
}
