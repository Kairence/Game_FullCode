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

            SetStr(258, 300);
            SetDex(56, 75);
            SetInt(46, 60);

            SetHits(410, 540);
            SetStam(120, 145);
            SetMana(10, 50);

			SetAttackSpeed(10.0);

            SetDamage(7, 19);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 25);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 5, 10);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 5, 10);

            Fame = 1500;
            Karma = 0;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 88.8;
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