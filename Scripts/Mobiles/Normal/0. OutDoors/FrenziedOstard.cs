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

            this.SetStr(94, 170);
            this.SetDex(96, 115);
            this.SetInt(6, 10);

            this.SetHits(71, 110);
            SetStam(20, 30);
            SetMana(10, 20);
			SetAttackSpeed(2.0);

            this.SetDamage(1, 3);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Fire, 10, 15);
            this.SetResistance(ResistanceType.Poison, 20, 25);
            this.SetResistance(ResistanceType.Energy, 20, 25);


            this.Fame = 1500;
            this.Karma = -1500;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 77.1;
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