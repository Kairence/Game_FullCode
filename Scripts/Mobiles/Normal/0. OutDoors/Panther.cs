using System;

namespace Server.Mobiles
{
    [CorpseName("a panther corpse")]
    public class Panther : BaseCreature
    {
        [Constructable]
        public Panther()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a panther";
            Body = 0xD6;
            Hue = 0x901;
            BaseSoundID = 0x462;

            this.SetStr(89, 139); // 최종 Str 650~700
			this.SetDex(137, 187); // 최종 Dex ~350

			this.SetHits(434, 634); // 최종 Hits 2,000~2,200
			this.SetStam(87, 137); 
			this.SetMana(0);

			SetAttackSpeed(2.0); // 매우 빠름
			SetDamage(12, 18); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 20, 30);
			this.SetResistance(ResistanceType.Cold, 20, 30);

			this.Fame = 800;
			this.VirtualArmor = 4;
			this.Tamable = true;
			this.MinTameSkill = 85.1;
        }

        public Panther(Serial serial)
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
                return 10;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Feline;
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