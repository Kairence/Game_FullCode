using System;

namespace Server.Mobiles
{
    [CorpseName("a white wolf corpse")]
    [TypeAlias("Server.Mobiles.Whitewolf")]
    public class WhiteWolf : BaseCreature
    {
        [Constructable]
        public WhiteWolf()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a white wolf";
            this.Body = Utility.RandomList(34, 37);
            this.BaseSoundID = 0xE5;

            SetStr(396, 520);
            SetDex(281, 305);
            SetInt(136, 160);

            SetHits(1236, 1260);
            SetStam(200, 300);
            SetMana(5, 10);
			SetAttackSpeed(2.0);

            SetDamage(22, 37);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 20, 25);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 5, 10);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 10, 15);

            Fame = 4000;
            Karma = -4000;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 83.1;
        }

        public WhiteWolf(Serial serial)
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
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Canine;
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