using System;

namespace Server.Mobiles
{
    [CorpseName("a red dragon corpse")]
    public class RidableDragon : BaseMount
    {
        [Constructable]
        public RidableDragon()
            : this("a red dragon")
        {
        }
 		
        [Constructable]
        public RidableDragon(string name)
            : base(name, 0x31A, 0x3EBD, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 59;//Utility.RandomList(12, 59);
            BaseSoundID = 362;

            SetStr(5796, 5825);
            SetDex(6086, 6105);
            SetInt(6436, 6475);

            SetHits(14780, 14950);
			SetStam( 10000, 12000 );
			SetMana( 10000, 11500 );

			SetAttackSpeed( 5.0 );

            SetDamage(240, 406);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 60, 70);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 25, 35);
            SetResistance(ResistanceType.Energy, 35, 45);

            SetSkill(SkillName.EvalInt, 200.1, 205.0);
            SetSkill(SkillName.Magery, 200.1, 205.0);
            SetSkill(SkillName.MagicResist, 199.1, 200.0);
            SetSkill(SkillName.Tactics, 197.6, 200.0);
            SetSkill(SkillName.Wrestling, 190.1, 192.5);

            Fame = 21000;
            Karma = -21000;

            VirtualArmor = 26;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 173.9;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public RidableDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return !Controlled;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return !Controlled;
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
                return 7;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return (Body == 12 ? ScaleType.Yellow : ScaleType.Red);
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
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
            AddLoot(LootPack.Gems, 8);
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
