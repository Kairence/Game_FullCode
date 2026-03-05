using System;

namespace Server.Mobiles
{
    [CorpseName("a pig corpse")]
    public class Pig : BaseCreature
    {
        [Constructable]
        public Pig()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a pig";
            this.Body = 0xCB;
            this.BaseSoundID = 0xC4;

            this.SetStr(5, 15);
			this.SetDex(17, 47); // 최종 Dex ~100

			this.SetHits(66, 116); // 최종 Hits 600~650
			this.SetStam(47, 97); 
			this.SetMana(0);

			SetAttackSpeed(3.5);
			SetDamage(2, 4); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);

			this.Fame = 200;
			this.VirtualArmor = 0;
			this.Tamable = true;
			this.MinTameSkill = -18.9;

            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public Pig(Serial serial)
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
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
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