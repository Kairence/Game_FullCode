using System;

namespace Server.Mobiles
{
    [CorpseName("a walrus corpse")]
    public class Walrus : BaseCreature
    {
        [Constructable]
        public Walrus()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a walrus";
            this.Body = 0xDD;
            this.BaseSoundID = 0xE0;

            this.SetStr(250, 630);
            this.SetDex(25, 30);
            this.SetInt(12, 18);

            SetHits(230, 280);
            SetStam(25, 30);
            SetMana(5, 10);
			
			SetAttackSpeed(10.0);

            this.SetDamage(4, 11);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 10, 15);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Poison, 5, 10);

            this.Fame = 300;
            this.Karma = 0;

            this.VirtualArmor = 1;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 49.1;

            if (Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public Walrus(Serial serial)
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
                return FoodType.Fish;
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