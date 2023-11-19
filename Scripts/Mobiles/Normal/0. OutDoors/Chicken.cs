using System;

namespace Server.Mobiles
{
    [CorpseName("a chicken corpse")]
    public class Chicken : BaseCreature
    {
        [Constructable]
        public Chicken()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a chicken";
            Body = 0xD0;
            BaseSoundID = 0x6E;

            this.VirtualArmor = Utility.RandomMinMax(0, 1);

            this.SetStr(10, 15);
            this.SetDex(25, 35);
            this.SetInt(10, 15);

            SetHits(21, 30);
            SetStam(12, 15);
            SetMana(10, 11);
			
			SetAttackSpeed(15.0);

            SetDamage(1, 4);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 1, 5);

            Fame = 150;
            Karma = 0;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = -6.9;
        }

        public Chicken(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override int Feathers
        {
            get
            {
                return 25;
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