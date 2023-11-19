using System;
using Server.Items;

namespace Server.Mobiles
{
    [TypeAlias("Server.Mobiles.DreadSpiderWeak")]
    [CorpseName("a dread spider corpse")]
    public class DreadSpider : BaseCreature
    {
        [Constructable]
        public DreadSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a dread spider";
            Body = 11;
            BaseSoundID = 1170;

            SetStr(250, 275);
            SetDex(100, 150);
            SetInt(250, 300);

            SetHits(1600, 1750);
			SetMana(850, 1000);
			SetStam(50, 80);

            SetDamage(15, 25);
			SetAttackSpeed( 4.0 );

            SetDamageType(ResistanceType.Poison, 100);

            Fame = 9000;
            Karma = -9000;

            PackItem(new SpidersSilk(30));

			AcidTime = 4.0;
			AcidBreath = true;
			
			
            Tamable = true;
            ControlSlots = 19;
            MinTameSkill = 96.0;
            VirtualArmor = 15;
        }

        public DreadSpider(Serial serial)
            : base(serial)
        {
        }


        public override Poison PoisonImmune { get { return Poison.Greater; } }

		/*
        public void BeginAcidBreath()
        {
            Mobile m = Combatant as Mobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || !CanBeHarmful(m) || m.Poisoned )
                return;

			if( 0.4 >= Utility.RandomDouble() )
				m.ApplyPoison(m, Poison.Greater);
			else
				AOS.Damage( m, this, Utility.RandomMinMax( 40, 51 ), false, 0, 0, 0, 100, 0 );
			
            PlaySound(0x118);
            MovingEffect(m, 0x36D4, 1, 0, false, false, 0x3F, 0);

           // TimeSpan delay = TimeSpan.FromSeconds(GetDistanceToSqrt(m) / 5.0);
           // Timer.DelayCall<Mobile>(delay, new TimerStateCallback<Mobile>(EndAcidBreath), m);

            //m_NextAcidBreath = DateTime.Now + TimeSpan.FromSeconds(5);
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
            AddLoot(LootPack.FilthyRich);
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
