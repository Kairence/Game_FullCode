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

			this.SetAttackSpeed(4.0);  // [조정] 3.5초 -> 4.0초.
									   // 멧돼지(4.0s)와 같은 리듬을 공유하여 '돼지류'의 공통 속도 부여.
									   // 개(3.5s)보다는 확실히 둔한 느낌을 줍니다.

			this.SetDamage(10, 15);    // [방어구 효능 반영] 개(10-14)와 비슷한 수준.

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