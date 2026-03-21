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

			this.SetAttackSpeed(3.0);  // [조정] 기존 3.0초 유지. 
									   // 유저의 무기 공속(2.5~3.0s)과 가장 잘 맞는 정직한 리듬입니다.

			this.SetDamage(14, 22);    // [방어구 효능 반영]

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