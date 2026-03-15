using System;

namespace Server.Mobiles
{
    [CorpseName("a gorilla corpse")]
    public class Gorilla : BaseCreature
    {
        [Constructable]
        public Gorilla()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a gorilla";
            this.Body = 0x1D;
            this.BaseSoundID = 0x9E;

            this.SetStr(57, 107);    
			this.SetDex(77, 127);    
			this.SetInt(37, 67);     

			this.SetHits(172, 372);  // 최종 Hits 2,500~2,700
			this.SetStam(77, 127);

			SetAttackSpeed(3.0);
			SetDamage(22, 32);

			this.SetResistance(ResistanceType.Physical, 10, 15);
			this.SetResistance(ResistanceType.Energy, 15, 20);

			this.SetSkill(SkillName.Wrestling, 46.9, 56.9);
			this.SetSkill(SkillName.Tactics, 46.9, 56.9);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 53.1;

			this.Fame = 1200;
			this.Karma = 0;
        }

        public Gorilla(Serial serial)
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
                return 6;
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