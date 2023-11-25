using System;

namespace Server.Mobiles
{
    [CorpseName("a pig corpse")]
    public class Boar : BaseCreature
    {
        [Constructable]
        public Boar()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a boar";
            this.Body = 0x122;
            this.BaseSoundID = 0xC4;

            this.SetStr(25, 30);
            this.SetDex(25, 30);
            this.SetInt(12, 18);

            SetHits(230, 280);
            SetStam(25, 30);
            SetMana(5, 10);
			
			SetAttackSpeed(10.0);

            this.SetDamage(7, 11);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 10, 15);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Poison, 5, 10);

            this.Fame = 300;
            this.Karma = 0;

            this.VirtualArmor = 1;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 29.1;
			
            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;			
        }

        public Boar(Serial serial)
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

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}