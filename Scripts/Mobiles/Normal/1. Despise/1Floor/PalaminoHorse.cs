using System;
using Server.Mobiles;

namespace Server.Mobiles
{
    [CorpseName("a horse corpse")]
    public class Palomino : BaseMount
    {
        [Constructable]
        public Palomino() : this("a horse")
        {
        }

        [Constructable]
        public Palomino(string name) : base(name, 1408, 0x3ECD, AIType.AI_Animal, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0xA8;

            SetStr(550, 980);
            SetDex(56, 75);
            SetInt(6, 10);

            SetHits(28, 45);
			SetStam(10, 15);
            SetMana(10, 15);
			SetAttackSpeed( 30.0 );

            SetDamage(3, 11);

            SetDamageType(ResistanceType.Physical, 100);

            Fame = 0;
            Karma = 1000;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 89.1;
        }

        public override int Meat { get { return 3; } }
        public override int Hides { get { return 10; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

        public Palomino(Serial serial) : base(serial)
        {
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
