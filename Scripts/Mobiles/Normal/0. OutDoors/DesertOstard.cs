using System;

namespace Server.Mobiles
{
    [CorpseName("an ostard corpse")]
    public class DesertOstard : BaseMount
    {
        [Constructable]
        public DesertOstard()
            : this("a desert ostard")
        {
        }

        [Constructable]
        public DesertOstard(string name)
            : base(name, 0xD2, 0x3EA3, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0x270;

            this.SetStr(194, 270);
            this.SetDex(156, 175);
            this.SetInt(16, 20);

            this.SetHits(271, 288);
			this.SetStam(100, 120);
            this.SetMana(1, 5);

            this.SetDamage(1, 4);
			SetAttackSpeed(20.0);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.Fame = 450;
            this.Karma = 0;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 29.1;
        }

        public DesertOstard(Serial serial)
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