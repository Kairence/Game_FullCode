using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a giant black widow spider corpse")] // stupid corpse name
    public class GiantBlackWidow : BaseCreature
    {
        [Constructable]
        public GiantBlackWidow()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a giant black widow";
            this.Body = 0x9D;
            this.BaseSoundID = 0x388; // TODO: validate

            this.SetStr(1220, 1580);
            this.SetDex(1250, 1350);
            this.SetInt(100, 500);

            this.SetHits(1425, 1650);
			SetStam(1250, 1300);
			SetMana(100, 500);

            this.SetDamage(18, 20);
			SetAttackSpeed( 1.0 );

            this.SetDamageType(ResistanceType.Poison, 100);
            this.SetResistance(ResistanceType.Physical, 20, 30);
            this.SetResistance(ResistanceType.Fire, 10, 20);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 50, 60);
            this.SetResistance(ResistanceType.Energy, 10, 20);
			
            this.Fame = 6000;
            this.Karma = -6000;

            this.VirtualArmor = 0;

            this.PackItem(new SpidersSilk(15));
            VirtualArmor = 10;
        }

        public GiantBlackWidow(Serial serial)
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
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
		/*
        public void BeginAcidBreath()
        {
            Mobile m = Combatant as Mobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || !CanBeHarmful(m) || m.Poisoned )
                return;

			if( 0.4 >= Utility.RandomDouble() )
				m.ApplyPoison(m, Poison.Regular);
			else
				AOS.Damage( m, this, Utility.RandomMinMax( 20, 22 ), false, 0, 0, 0, 100, 0 );
			
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
            this.AddLoot(LootPack.Average);
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
}