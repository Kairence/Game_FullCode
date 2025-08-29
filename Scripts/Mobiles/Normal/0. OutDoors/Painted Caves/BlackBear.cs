using System;

namespace Server.Mobiles
{
    [CorpseName("a bear corpse")]
    [TypeAlias("Server.Mobiles.Bear")]
    public class BlackBear : BaseCreature
    {
        [Constructable]
        public BlackBear()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a black bear";
            this.Body = 211;
            this.BaseSoundID = 0xA3;

            this.SetStr(560, 900);
            this.SetDex(1056, 1075);
            this.SetInt(11, 14);

            this.SetHits(1160, 1300);
            this.SetStam(46, 60);
            this.SetMana(0);

            this.SetDamage(60, 80);

			SetAttackSpeed( 5.0 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 20, 25);
            this.SetResistance(ResistanceType.Cold, 10, 15);
            this.SetResistance(ResistanceType.Poison, 5, 10);

            this.Fame = 4500;
            this.Karma = 0;

            this.VirtualArmor = 2;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 35.1;
        }

        public BlackBear(Serial serial)
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
                return 12;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish | FoodType.Meat | FoodType.FruitsAndVegies;
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