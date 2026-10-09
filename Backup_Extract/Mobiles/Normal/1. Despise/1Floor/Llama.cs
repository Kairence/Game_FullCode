using System;

namespace Server.Mobiles
{
    [CorpseName("a llama corpse")]
    public class Llama : BaseCreature
    {
        [Constructable]
        public Llama()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a llama";
            this.Body = 0xDC;
            this.BaseSoundID = 0x3F3;

            this.SetStr(1, 5);       // 최종 Str 530~534
			this.SetDex(35, 55);     
			this.SetInt(1, 5);       

			this.SetHits(50, 100);   // 최종 Hits 860~910
			this.SetStam(35, 55);

			SetAttackSpeed(3.0);
			SetDamage(14, 22);

			this.SetResistance(ResistanceType.Physical, 5, 10);

			this.SetSkill(SkillName.Wrestling, 9.0, 19.0);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 11.1;

			this.Fame = 400;
			this.Karma = 0;
		}
		
        public Llama(Serial serial)
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
