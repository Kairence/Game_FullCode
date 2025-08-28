using System;

namespace Server.Mobiles
{
    [CorpseName("a white wyrm corpse")]
    public class WhiteWyrm : BaseCreature, IAuraCreature
    {
        public override double AverageThreshold { get { return 0.25; } }

        [Constructable]
        public WhiteWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = Utility.RandomBool() ? 180 : 49;
			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;			
            Name = "a white wyrm";
            BaseSoundID = 362;

            SetStr(7721, 9760);
            SetDex(6101, 9130);
            SetInt(8860, 9250);

            SetHits(34300, 38600);
			SetStam(30000, 35000);
			SetMana(34000, 38000);

            SetDamage(570, 1650);
			SetAttackSpeed( 10.0 );

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Cold, 50);

            SetResistance(ResistanceType.Physical, 55, 70);
            SetResistance(ResistanceType.Fire, 15, 25);
            SetResistance(ResistanceType.Cold, 80, 90);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.EvalInt, 199.1, 250.0);
            SetSkill(SkillName.Magery, 199.1, 250.0);
            SetSkill(SkillName.MagicResist, 199.1, 250.0);
            SetSkill(SkillName.Tactics, 197.6, 250.0);
            SetSkill(SkillName.Wrestling, 190.1, 250.0);

            Fame = 29000;
            Karma = -29000;

            VirtualArmor = 44;

            Tamable = true;
            ControlSlots = 4;
            MinTameSkill = 176.3;
            SetAreaEffect(AreaEffect.AuraDamage);
		}

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }


        public WhiteWyrm(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int DragonBlood
        {
            get
            {
                return 8;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override int Scales
        {
            get
            {
                return 9;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.White;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Gold;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Gems, Utility.Random(1, 5));
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