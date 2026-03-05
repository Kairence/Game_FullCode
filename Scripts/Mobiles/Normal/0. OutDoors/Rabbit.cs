using System;

namespace Server.Mobiles
{
    [CorpseName("a hare corpse")]
    public class Rabbit : BaseCreature
    {
        [Constructable]
        public Rabbit()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a rabbit";
            Body = 205;

            if (0.5 >= Utility.RandomDouble())
                Hue = Utility.RandomAnimalHue();

            this.SetStr(1, 5);
			this.SetDex(38, 58); // 최종 Dex ~150 (도망)

			this.SetHits(50, 80); // 최종 Hits 500~530
			this.SetStam(48, 68);
			this.SetMana(0);

			SetAttackSpeed(2.0);
			SetDamage(1, 1);

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 0, 5);

			this.Fame = 150;
			this.Tamable = true;
			this.MinTameSkill = -18.9;
        }

        public Rabbit(Serial serial)
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
                return 1;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies;
            }
        }
        public override int GetAttackSound() 
        { 
            return 0xC9; 
        }

        public override int GetHurtSound() 
        { 
            return 0xCA; 
        }

        public override int GetDeathSound() 
        { 
            return 0xCB; 
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