using System;

namespace Server.Mobiles
{
    [CorpseName("a goat corpse")]
    public class Goat : BaseCreature
    {
        [Constructable]
        public Goat()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a goat";
            this.Body = 0xD1;
            this.BaseSoundID = 0x99;

			// [역산] 명성 150 보너스 반영
			this.SetStr(4, 9); // 최종 Str 515~520
			this.SetDex(10, 20); // 최종 Dex ~170

			this.SetHits(50, 70); // 최종 Hits 500~520
			this.SetStam(8, 18);
			this.SetMana(0);

			this.SetAttackSpeed(4.0);  // [조정] 3.5초 -> 4.0초. 
									   // 개(3.5s)보다 느리게 설정하여 초식 동물의 여유를 표현합니다.
			this.SetDamage(12, 16);    // [방어구 효능 반영]

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);

			this.Fame = 150;
			this.VirtualArmor = 0;
			this.Tamable = true;
			this.MinTameSkill = -18.9;
        }

        public Goat(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override int Hides
        {
            get
            {
                return 8;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay | FoodType.FruitsAndVegies;
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