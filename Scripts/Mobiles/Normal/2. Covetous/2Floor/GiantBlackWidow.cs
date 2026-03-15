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

            /* Giant Black Widow - Fame 3,500 */
			this.SetStr(60, 80);    // 보너스(+804) 합산 시 약 880
			this.SetDex(96, 115);    
			this.SetInt(36, 60);     

			this.SetHits(100, 200);  // 보너스(+7,095) 합산 시 약 7,200
			this.SetStam(96, 115);

			SetAttackSpeed(1.8);
			SetDamage(25, 38);

			this.SetSkill(SkillName.Wrestling, 90.4); // 보너스 +10.4 반영
			this.SetSkill(SkillName.Tactics, 90.4);
			this.SetSkill(SkillName.Poisoning, 95.0, 105.0);

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Poison, 50);

			// 저항 패널티: 물리 타격에 약함
			this.SetResistance(ResistanceType.Physical, -20, -10);
			this.SetResistance(ResistanceType.Fire, -40, -30);
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.VirtualArmor = 0;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 75.1;

			this.Fame = 3500;
			this.Karma = -3500;

            this.PackItem(new SpidersSilk(15));
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