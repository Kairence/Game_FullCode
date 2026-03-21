using System;

namespace Server.Mobiles
{
    [CorpseName("a polar bear corpse")]
    [TypeAlias("Server.Mobiles.Polarbear")]
    public class PolarBear : BaseCreature
    {
        [Constructable]
        public PolarBear()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a polar bear";
            this.Body = 213;
            this.BaseSoundID = 0xA3;

            this.SetStr(282, 332); // 최종 Str 900~950
			this.SetDex(77, 127);  // 최종 Dex ~350

			this.SetHits(3134, 4134); // 최종 Hits 6,000~7,000
			this.SetStam(77, 127);
			this.SetMana(0);

			SetAttackSpeed(5.5);
			SetDamage(35, 50);

			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Cold, 20);

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Cold, 45, 50);
			this.SetResistance(ResistanceType.Fire, 5, 10);

			this.SetSkill(SkillName.Wrestling, 116.1, 126.1); // 최종 120.0~

			this.Fame = 1500;
			this.VirtualArmor = 7;
			this.Tamable = true;
			this.MinTameSkill = 95.1;
        }

        public PolarBear(Serial serial)
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
                return 16;
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