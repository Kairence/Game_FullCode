using System;

namespace Server.Mobiles
{
    [CorpseName("a grizzly bear corpse")]
    [TypeAlias("Server.Mobiles.Grizzlybear")]
    public class GrizzlyBear : BaseCreature
    {
        [Constructable]
        public GrizzlyBear()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a grizzly bear";
            this.Body = 212;
            this.BaseSoundID = 0xA3;

            this.SetStr(144, 194);   
			this.SetDex(56, 86);     
			this.SetInt(19, 39);     

			this.SetHits(1102, 1602); // 최종 Hits 5,000~5,500
			this.SetStam(56, 86);

			SetAttackSpeed(6.5);
			SetDamage(45, 65);

			this.SetResistance(ResistanceType.Physical, 20, 25);
			this.SetResistance(ResistanceType.Cold, 20, 25);

			this.SetSkill(SkillName.Wrestling, 64.6, 74.6);
			this.SetSkill(SkillName.Tactics, 64.6, 74.6);

			this.VirtualArmor = 5;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 59.1; // 중급 테이머의 주력 펫

			this.Fame = 2000;
			this.Karma = 0;
        }

        public GrizzlyBear(Serial serial)
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