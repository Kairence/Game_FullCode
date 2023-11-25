using System;

namespace Server.Mobiles
{
    [CorpseName("a cat corpse")]
    [TypeAlias("Server.Mobiles.Housecat")]
    public class Cat : BaseCreature
    {
        [Constructable]
        public Cat()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a cat";
            this.Body = 0xC9;
            this.Hue = Utility.RandomAnimalHue();
            this.BaseSoundID = 0x69;

            this.SetStr(10, 15);
            this.SetDex(125, 135);
            this.SetInt(10, 15);

            SetHits(250, 280);
            SetStam(140, 150);
            SetMana(10, 11);
			
			SetAttackSpeed(4.0);

            this.SetDamage(4, 8);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 5, 10);

            this.Fame = 600;
            this.Karma = 600;

            this.VirtualArmor = 0;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 9.9;
        }

        public Cat(Serial serial)
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
                return FoodType.Meat | FoodType.Fish;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Feline;
            }
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