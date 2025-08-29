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

            this.SetStr(210, 490);
            this.SetDex(36, 55);
            this.SetInt(16, 30);

			SetAttackSpeed( 3.0 );

			SetStam(10, 15);
            this.SetHits(15, 27);
            this.SetMana(10, 15);

            this.SetDamage(3, 5);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 15, 20);

            this.Fame = 0;
            this.Karma = 300;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 35.1;
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