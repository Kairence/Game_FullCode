using System;

namespace Server.Mobiles
{
    [CorpseName("a dog corpse")]
    public class Dog : BaseCreature
    {
        [Constructable]
        public Dog()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a dog";
            Body = 0xD9;
            Hue = Utility.RandomAnimalHue();
            BaseSoundID = 0x85;

            this.SetStr(11, 16);
            this.SetDex(35, 45);
            this.SetInt(12, 20);

            SetHits(32, 40);
            SetStam(12, 15);
            SetMana(10, 11);
			
			SetAttackSpeed(2.0);

            this.SetDamage(2,5);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 10, 15);

            Fame = 600;
            Karma = 600;

            VirtualArmor = 12;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 9.9;
        }

        public Dog(Serial serial)
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
                return PackInstinct.Canine;
            }
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
        }
    }
}