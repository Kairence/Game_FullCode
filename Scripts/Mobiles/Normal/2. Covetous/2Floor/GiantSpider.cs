using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a giant spider corpse")]
    public class GiantSpider : BaseCreature
    {
        [Constructable]
        public GiantSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a giant spider";
            Body = 28;
            BaseSoundID = 0x388;

            SetStr(126, 130);
            SetDex(176, 195);
            SetInt(116, 120);

            SetHits(450, 500);
			SetStam(50, 100);
            SetMana(20, 30);

			SetAttackSpeed( 10.0 );

            SetDamage(24, 50);

            SetDamageType(ResistanceType.Poison, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Poison, 25, 35);

            SetSkill(SkillName.Poisoning, 10.1, 12.0);

            Fame = 4000;
            Karma = -4000;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 59.1;

			SetAttackSpeed( 20.0 );
			AcidTime = 20.0;
			AcidBreath = true;

            PackItem(new SpidersSilk(8));
            VirtualArmor = 10;
			
       }

        public GiantSpider(Serial serial)
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
		/*
        public override Poison HitPoison
        {
            get
            {
                return Poison.Regular;
            }
        }

        public void BeginAcidBreath()
        {
            Mobile m = Combatant as Mobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || !CanBeHarmful(m) || m.Poisoned )
                return;

			if( 0.1 >= Utility.RandomDouble() )
				m.ApplyPoison(m, Poison.Lesser);
			else
				AOS.Damage( m, this, Utility.RandomMinMax( 11, 12 ), false, 0, 0, 0, 100, 0 );
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
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Poor);
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