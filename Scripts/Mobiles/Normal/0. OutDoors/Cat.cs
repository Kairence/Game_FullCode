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
            this.BaseSoundID = 0x69;

			// [역산] 보너스(Str+519, Hits+619, Stam+54, Skill+0.6) 제외 설정
			this.SetStr(6, 16); 
			this.SetDex(66, 76); // 최종 Dex ~180
			this.SetInt(10, 20);

			this.SetHits(31, 50); // 최종 Hits 650~669
			this.SetStam(66, 76);
			this.SetMana(0);

			SetAttackSpeed(2.0); // 매우 빠름
			SetDamage(1, 3); // 평균 2.0

			this.SetSkill(SkillName.Wrestling, 1.4, 2.4); 

			this.Fame = 250;
			this.VirtualArmor = 0;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = -18.9;
			this.SetDamageType(ResistanceType.Physical, 100);
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