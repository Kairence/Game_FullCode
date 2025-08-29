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

            this.SetStr(530, 950);
            this.SetDex(36, 55);
            this.SetInt(36, 60);

            this.SetHits(380, 510);
			this.SetStam(44, 67);
            this.SetMana(100);

			SetAttackSpeed( 4.5 );
			
            this.SetDamage(4, 10);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 20, 25);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Cold, 10, 15);


            this.Fame = 450;
            this.Karma = 0;

            this.VirtualArmor = 2;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = -18.9;
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