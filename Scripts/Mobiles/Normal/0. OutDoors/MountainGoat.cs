using System;

namespace Server.Mobiles
{
    [CorpseName("a mountain goat corpse")]
    public class MountainGoat : BaseCreature
    {
        [Constructable]
        public MountainGoat()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a mountain goat";
            this.Body = 88;
            this.BaseSoundID = 0x99;

            // [역산] 명성 400 보너스 반영
			this.SetStr(20, 40); 
			this.SetDex(100, 150); // 최종 Dex ~350

			this.SetHits(176, 250); // 최종 Hits 1,000~1,074
			this.SetStam(44, 94);   // 최종 Stam 100~150
			this.SetMana(0);

			this.SetAttackSpeed(3.5);  // [조정] 3.0초 -> 3.5초.
									   // 큰뿔사슴(3.5s)과 동급의 공속을 부여하여 
									   // 초급 유저가 대응하기 편한 리듬을 유지합니다.

			this.SetDamage(16, 24);    // [방어구 가치 존중]

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Cold, 20, 30);

			this.Fame = 400;
			this.VirtualArmor = 3;
			this.Tamable = true;
			this.MinTameSkill = 35.1;

            
            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public MountainGoat(Serial serial)
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
                return 12;
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