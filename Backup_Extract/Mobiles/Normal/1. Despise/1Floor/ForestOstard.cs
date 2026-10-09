using System;

namespace Server.Mobiles
{
    [CorpseName("an ostard corpse")]
    public class ForestOstard : BaseMount
    {
        [Constructable]
        public ForestOstard()
            : this("a forest ostard")
        {
        }

        [Constructable]
        public ForestOstard(string name)
            : base(name, 0xDB, 0x3EA5, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0x270;

            this.SetStr(1, 10);      // 최종 Str 578~587
			this.SetDex(83, 113);    // 최종 Dex ~400 (빠름)
			this.SetInt(1, 10);      

			this.SetHits(134, 234);  // 최종 Hits 2,000~2,100
			this.SetStam(83, 113);

			SetAttackSpeed(2.5);
			SetDamage(16, 24);

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 10, 15);
			this.SetResistance(ResistanceType.Fire, 10, 15);

			// 최종 Skill 35.0~45.0 (45.0 - 2.5 = 42.5)
			this.SetSkill(SkillName.Wrestling, 32.5, 42.5);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 29.1; // 길들이기 쉬운 탈것

			this.Fame = 1000;
			this.Karma = 0;
        }

        public ForestOstard(Serial serial)
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
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
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
