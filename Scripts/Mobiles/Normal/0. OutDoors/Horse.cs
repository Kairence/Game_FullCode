using System;

namespace Server.Mobiles
{
    [CorpseName("a horse corpse")]
    [TypeAlias("Server.Mobiles.BrownHorse", "Server.Mobiles.DirtyHorse", "Server.Mobiles.GrayHorse", "Server.Mobiles.TanHorse")]
    public class Horse : BaseMount
    {
        private static readonly int[] m_IDs = new int[]
        {
            0xC8, 0x3E9F,
            0xE2, 0x3EA0,
            0xE4, 0x3EA1,
            0xCC, 0x3EA2
        };
        [Constructable]
        public Horse()
            : this("a horse")
        {
        }

        [Constructable]
        public Horse(string name)
            : base(name, 0xE2, 0x3EA0, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            int random = Utility.Random(4);

            Body = m_IDs[random * 2];
            ItemID = m_IDs[random * 2 + 1];
            BaseSoundID = 0xA8;

            this.SetStr(1, 10); 
			this.SetDex(100, 150);

			this.SetHits(28, 50); // 최종 Hits 800~822
			this.SetStam(93, 143); // 최종 Stam 150~200
			this.SetMana(0);

			SetAttackSpeed(3.0);
			SetDamage(3, 6); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 10, 15);

			this.Fame = 350;
			this.VirtualArmor = 1;
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 29.1;
        }

        public Horse(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 10;
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