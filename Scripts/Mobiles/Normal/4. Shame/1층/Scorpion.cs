using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a scorpion corpse")]
    public class Scorpion : BaseCreature
    {
        [Constructable]
        public Scorpion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a scorpion";
            Body = 48;
            BaseSoundID = 397;

            /* Scorpion - Fame 2,000 / Basic Insect */
			this.SetStr(150, 200);       
			this.SetDex(150, 200);       
			this.SetInt(50, 80);         

			// [Hits] 명성 보석(3,500) 포함 최종 4천 내외
			this.SetHits(400, 600); 
			this.SetStam(150, 200);      
			this.SetMana(50, 80);        

			this.SetAttackSpeed(2.8);    
			this.SetDamage(5, 10);       

			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			this.SetResistance(ResistanceType.Physical, 15, 20);
			this.SetResistance(ResistanceType.Fire, 5, 10);
			this.SetResistance(ResistanceType.Cold, 5, 10);
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 5, 10);

			this.SetSkill(SkillName.Wrestling, 50.0, 60.0);
			this.SetSkill(SkillName.Tactics, 50.0, 60.0);
			this.SetSkill(SkillName.Poisoning, 60.0, 80.0);

			this.VirtualArmor = 5;      
			this.Tamable = true;
			this.MinTameSkill = 45.0;    // 테이밍 요구치
			this.ControlSlots = 1;       // 추종자 수

			this.Fame = 2000;           
			this.Karma = -2000;

            //PackItem(new LesserPoisonPotion());
        }

        public Scorpion(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
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
                return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly);
            }
        }
		*/
		
        public void BeginAcidBreath()
        {
            Mobile m = Combatant as Mobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || !CanBeHarmful(m) || m.Poisoned )
                return;

			m.ApplyPoison(m, Poison.Lesser);
			
            PlaySound(0x118);
            MovingEffect(m, 0x36D4, 1, 0, false, false, 0x3F, 0);

           // TimeSpan delay = TimeSpan.FromSeconds(GetDistanceToSqrt(m) / 5.0);
           // Timer.DelayCall<Mobile>(delay, new TimerStateCallback<Mobile>(EndAcidBreath), m);

            //m_NextAcidBreath = DateTime.Now + TimeSpan.FromSeconds(5);
        }
		
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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