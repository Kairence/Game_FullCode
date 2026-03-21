using System;

namespace Server.Mobiles
{
    [CorpseName("a llama corpse")]
    public class RidableLlama : BaseMount
    {
        [Constructable]
        public RidableLlama()
            : this("a ridable llama")
        {
        }

        [Constructable]
        public RidableLlama(string name)
            : base(name, 0xDC, 0x3EA6, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0x3F3;

            this.SetStr(1, 10);      
			this.SetDex(35, 55);     
			this.SetInt(1, 10);      

			this.SetHits(80, 180);   // 최종 Hits 1,300~1,400
			this.SetStam(35, 55);

			SetAttackSpeed(3.0);
			SetDamage(3, 7); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);

			this.SetSkill(SkillName.Wrestling, 18.5, 28.5);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 29.1;

			this.Fame = 600;
			this.Karma = 0;
        }

        public RidableLlama(Serial serial)
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

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}