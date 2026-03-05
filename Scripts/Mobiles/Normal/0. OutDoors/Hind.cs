using System;

namespace Server.Mobiles
{
    [CorpseName("a deer corpse")]
    public class Hind : BaseCreature
    {
        [Constructable]
        public Hind()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a hind";
            this.Body = 0xED;

            this.SetStr(1, 10); 
			this.SetDex(100, 150);

			this.SetHits(31, 50); // 최종 Hits 650~669
			this.SetStam(46, 66);
			this.SetMana(0);

			SetAttackSpeed(3.0);
			SetDamage(2, 4); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);

			this.Fame = 250;
			this.VirtualArmor = 1;
			this.Tamable = true;
			this.MinTameSkill = 15.1;

        }

        public Hind(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 5;
            }
        }
        public override int Hides
        {
            get
            {
                return 8;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override int GetAttackSound() 
        { 
            return 0x82; 
        }

        public override int GetHurtSound() 
        { 
            return 0x83; 
        }

        public override int GetDeathSound() 
        { 
            return 0x84; 
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