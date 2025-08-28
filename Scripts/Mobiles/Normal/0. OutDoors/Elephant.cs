using System;

namespace Server.Mobiles
{
    [CorpseName("a elephant corpse")]
    public class Elephant : BaseMount
    {
        [Constructable]
        public Elephant()
            : this("a elephant")
        {
        }

        [Constructable]
        public Elephant(string name)
            : base(name, 187, 0x3EBA, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0x3F3;

            SetStr(5580, 7300);
            SetDex(3600, 4500);
            SetInt(4600, 6000);

            SetHits(11000, 14000);
            SetStam(12000, 14500);
            SetMana(100, 500);

			SetAttackSpeed(10.0);

            SetDamage(555, 777);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 25);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 5, 10);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 5, 10);

            Fame = 20000;
            Karma = 20000;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 188.8;
        }

        public Elephant(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 12;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override bool OverrideBondingReqs()
        {
            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}