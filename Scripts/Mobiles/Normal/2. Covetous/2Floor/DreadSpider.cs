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

            /* Dread Spider - Fame 8,500 */
			this.SetStr(250, 300);  // 보너스(+1,298) 합산 시 약 1,600
			this.SetDex(140, 160);   
			this.SetInt(234, 250);   

			this.SetHits(400, 600);  // 보너스(+17,232) 합산 시 약 17,800
			this.SetStam(140, 160);
			this.SetMana(1000, 1500);

			SetAttackSpeed(2.0);
			SetDamage(40, 55);

			this.SetSkill(SkillName.Wrestling, 132.0); // 보너스 +27.0 반영
			this.SetSkill(SkillName.Tactics, 132.0);
			this.SetSkill(SkillName.Magery, 110.0, 120.0);
			this.SetSkill(SkillName.Poisoning, 100.0, 115.0);

			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Cold, 30);
			this.SetDamageType(ResistanceType.Poison, 30);

			// 저항 패널티: 극단적인 저항 하향으로 사냥 효율 증대
			this.SetResistance(ResistanceType.Physical, -70, -60);
			this.SetResistance(ResistanceType.Fire, -70, -60);
			this.SetResistance(ResistanceType.Cold, 20, 30); 
			this.VirtualArmor = 5;

			this.Tamable = true;
			this.ControlSlots = 3;
			this.MinTameSkill = 105.1;

			this.Fame = 8500;
			this.Karma = -8500;
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
