using System;

namespace Server.Mobiles
{
    [CorpseName("a bear corpse")]
    [TypeAlias("Server.Mobiles.Bear")]
    public class BlackBear : BaseCreature
    {
        [Constructable]
        public BlackBear()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a black bear";
            this.Body = 211;
            this.BaseSoundID = 0xA3;

            this.SetStr(24, 44);     
			this.SetDex(35, 55);     
			this.SetInt(5, 15);      

			this.SetHits(80, 180);   // 최종 Hits 1,300~1,400
			this.SetStam(35, 55);

			SetAttackSpeed(4.0);
			SetDamage(22, 30);

			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Cold, 10, 15);

			this.SetSkill(SkillName.Wrestling, 23.5, 33.5);
			this.SetSkill(SkillName.Tactics, 23.5, 33.5);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 35.1; // 초보 테이머용

			this.Fame = 600;
			this.Karma = 0;
        }

        public BlackBear(Serial serial)
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
                return FoodType.Fish | FoodType.Meat | FoodType.FruitsAndVegies;
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