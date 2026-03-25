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

            /* Giant Spider - Fame 2,500 */
			/* Giant Spider - Fame 2,500 */
			this.SetStr(50, 60);    // 시스템 보너스(+711) 합산 시 약 770
			this.SetDex(76, 95);     
			this.SetInt(36, 60);     

			this.SetHits(100, 200);  // 보너스(+5,068) 합산 시 약 5,200
			this.SetStam(76, 95);

			SetAttackSpeed(2.0);
			SetDamage(22, 32);

			this.SetSkill(SkillName.Wrestling, 82.2); // 보너스 +7.2 반영
			this.SetSkill(SkillName.Tactics, 82.2);
			this.SetSkill(SkillName.Poisoning, 75.0, 85.0);

			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Poison, 20);

			// 저항 패널티: 물리와 불에 매우 취약 (데미지 약 1.3배 증폭)
			this.SetResistance(ResistanceType.Physical, -30, -20);
			this.SetResistance(ResistanceType.Fire, -50, -40); 
			this.VirtualArmor = 0;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 65.1;

			this.Fame = 2500;
			this.Karma = -2500;
			AcidBreath = true;

            PackItem(new SpidersSilk(8));
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
