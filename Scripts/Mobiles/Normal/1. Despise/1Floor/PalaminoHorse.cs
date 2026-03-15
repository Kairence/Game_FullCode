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

            this.SetStr(1, 10);      // 최종 Str 547~556
			this.SetDex(35, 55);     
			this.SetInt(1, 10);      // 최종 Int 56~65

			this.SetHits(80, 180);   // 최종 Hits 1,300~1,400
			this.SetStam(35, 55);

			SetAttackSpeed(3.0);
			SetDamage(14, 22);

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Cold, 5, 10);

			this.SetSkill(SkillName.Wrestling, 18.5, 28.5);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 29.1;

			this.Fame = 600;
			this.Karma = 0;
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
