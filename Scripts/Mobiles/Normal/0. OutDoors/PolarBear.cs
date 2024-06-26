using System;

namespace Server.Mobiles
{
    [CorpseName("a polar bear corpse")]
    [TypeAlias("Server.Mobiles.Polarbear")]
    public class PolarBear : BaseCreature
    {
        [Constructable]
        public PolarBear()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a polar bear";
            this.Body = 213;
            this.BaseSoundID = 0xA3;

            this.SetStr(1116, 1410);
            this.SetDex(810, 2050);
            this.SetInt(260, 500);

            this.SetHits(7000, 8400);
            SetStam(1000, 2000);
            SetMana(10, 20);

			SetAttackSpeed(6.0);

            this.SetDamage(77, 120);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 35);
            this.SetResistance(ResistanceType.Cold, 60, 80);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 10, 15);

            this.Fame = 3500;
            this.Karma = 0;

            this.Tamable = true;
            this.ControlSlots = 2;
            this.MinTameSkill = 85.1;
        }

        public PolarBear(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 16;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish | FoodType.FruitsAndVegies | FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Bear;
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