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

            // [역산] 명성 150 보너스(Str+511, Hits+450, Stam+52, Skill+0.3) 반영
			this.SetStr(4, 9);
			this.SetDex(10, 20);
			this.SetInt(10, 20);

			this.SetHits(50, 70); // 최종 Hits 500~520
			this.SetStam(8, 18);  // 최종 Stam 60~70
			this.SetMana(0);

			this.SetAttackSpeed(3.5);  // [조정] 2.5초 -> 3.5초. 
									   // 고양이(2.5s)보다는 느리게 설정하여 덩치와 성격에 맞게 조정했습니다.
			this.SetDamage(10, 14);    // [방어구 효능 반영]

			this.SetSkill(SkillName.Wrestling, 0.7, 1.7);

			this.Fame = 150;
			this.VirtualArmor = 0;
			this.Tamable = true;
			this.MinTameSkill = -18.9;
			this.SetDamageType(ResistanceType.Physical, 100);
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