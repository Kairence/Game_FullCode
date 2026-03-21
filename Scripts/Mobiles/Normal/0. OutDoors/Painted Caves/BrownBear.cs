using System;

namespace Server.Mobiles
{
    [CorpseName("a bear corpse")]
    public class BrownBear : BaseCreature
    {
        [Constructable]
        public BrownBear()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a brown bear";
            this.Body = 167;
            this.BaseSoundID = 0xA3;

            this.SetStr(43, 93);     
			this.SetDex(35, 55);     
			this.SetInt(10, 20);     

			this.SetHits(134, 334);  // 최종 Hits 2,000~2,200
			this.SetStam(35, 55);

			SetAttackSpeed(4.8);
			SetDamage(28, 38);

			this.SetResistance(ResistanceType.Physical, 10, 15);
			this.SetResistance(ResistanceType.Cold, 15, 20);

			this.SetSkill(SkillName.Wrestling, 37.5, 47.5);
			this.SetSkill(SkillName.Tactics, 37.5, 47.5);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 47.1;

			this.Fame = 1000;
			this.Karma = 0;
        }

        public BrownBear(Serial serial)
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
                return 12;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish | FoodType.FruitsAndVegies | FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Bear;
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