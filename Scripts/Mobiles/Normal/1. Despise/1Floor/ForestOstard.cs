using System;

namespace Server.Mobiles
{
    [CorpseName("an ostard corpse")]
    public class ForestOstard : BaseMount
    {
        [Constructable]
        public ForestOstard()
            : this("a forest ostard")
        {
        }

        [Constructable]
        public ForestOstard(string name)
            : base(name, 0xDB, 0x3EA5, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0x270;

            this.SetStr(294, 370);
            this.SetDex(56, 75);
            this.SetInt(6, 10);

            this.SetHits(71, 88);
			SetAttackSpeed( 30.0 );
			SetStam(10, 15);
            this.SetMana(10, 15);

            this.SetDamage(8, 14);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 15, 20);

            this.Fame = 0;
            this.Karma = 450;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 29.1;
        }

        public ForestOstard(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Ostard;
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