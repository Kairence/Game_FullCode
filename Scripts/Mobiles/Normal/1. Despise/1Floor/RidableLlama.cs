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

            this.SetStr(210, 490);
            this.SetDex(56, 75);
            this.SetInt(16, 30);

			SetAttackSpeed( 30.0 );
            this.SetHits(15, 27);
			SetStam(10, 15);
            this.SetMana(10, 15);

            this.SetDamage(3, 5);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 10, 15);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Cold, 5, 10);
            this.SetResistance(ResistanceType.Poison, 5, 10);
            this.SetResistance(ResistanceType.Energy, 5, 10);

            this.Fame = 0;
            this.Karma = 300;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 35.1;
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