using System;

namespace Server.Mobiles
{
    [CorpseName("a grey wolf corpse")]
    [TypeAlias("Server.Mobiles.Greywolf")]
    public class GreyWolf : BaseCreature
    {
        [Constructable]
        public GreyWolf()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a grey wolf";
            this.Body = Utility.RandomList(25, 27);
            this.BaseSoundID = 0xE5;

            // [역산] 명성 600 보너스 반영
			this.SetStr(54, 74); // 최종 Str 600~620
			this.SetDex(100, 150); // 최종 Dex ~350

			this.SetHits(780, 880); // 최종 Hits 2,000~2,100
			this.SetStam(39, 89);  // 최종 Stam 100~150
			this.SetMana(0);

			SetAttackSpeed(2.2);
			SetDamage(8, 14); // 평균 11.0

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Cold, 20, 30);

			this.Fame = 600;
			this.VirtualArmor = 3;
			this.Tamable = true;
			this.MinTameSkill = 71.1;
        }

        public GreyWolf(Serial serial)
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