using System;

namespace Server.Mobiles
{
    [CorpseName("an ostard corpse")]
    public class FrenziedOstard : BaseMount
    {
        [Constructable]
        public FrenziedOstard()
            : this("a frenzied ostard")
        {
        }

        [Constructable]
        public FrenziedOstard(string name)
            : base(name, 0xDA, 0x3EA4, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Hue = Utility.RandomHairHue() | 0x8000;

            this.BaseSoundID = 0x275;

            SetStr(596, 1020);
            SetDex(281, 305);
            SetInt(136, 160);

            SetHits(1236, 1260);
            SetStam(200, 300);
            SetMana(5, 10);
			SetAttackSpeed(2.0);

            SetDamage(22, 37);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 20, 25);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 5, 10);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 10, 15);

            Fame = 3500;
            Karma = -3500;

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 83.1;
        }

        public FrenziedOstard(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish | FoodType.Eggs | FoodType.FruitsAndVegies;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Ostard;
            }
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