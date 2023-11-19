using System;

namespace Server.Mobiles
{
    [CorpseName("a walrus corpse")]
    public class Walrus : BaseCreature
    {
        [Constructable]
        public Walrus()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a walrus";
            this.Body = 0xDD;
            this.BaseSoundID = 0xE0;

            this.SetStr(21, 29);
            this.SetDex(46, 55);
            this.SetInt(16, 20);

            this.SetHits(34, 57);
            SetStam(100, 150);
            SetMana(10, 15);

			SetAttackSpeed(5.0);

            this.SetDamage(4, 10);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 20, 25);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Cold, 20, 25);
            this.SetResistance(ResistanceType.Poison, 5, 10);
            this.SetResistance(ResistanceType.Energy, 5, 10);

            this.Fame = 150;
            this.Karma = 0;

            this.VirtualArmor = 1;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 35.1;
        }

        public Walrus(Serial serial)
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
                return FoodType.Fish;
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