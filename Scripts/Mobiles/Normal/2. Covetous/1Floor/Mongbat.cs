using System;

namespace Server.Mobiles
{
    [CorpseName("a mongbat corpse")]
    public class Mongbat : BaseCreature
    {
        [Constructable]
        public Mongbat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a mongbat";
            Body = 39;
            BaseSoundID = 422;

            SetStr(86, 130);
            SetDex(46, 98);
            SetInt(36, 54);

			SetAttackSpeed( 5.0 );

            SetHits(200, 245);
			SetStam(50, 100);
            SetMana(10, 30);

            SetDamage(10, 15);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 5, 10);

            Fame = 1750;
            Karma = -1750;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = -18.9;
            VirtualArmor = 7;
        }

        public Mongbat(Serial serial)
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
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }

        public override bool CanFly
        {
            get
            {
                return true;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Poor);
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