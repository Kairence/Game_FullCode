using System;

namespace Server.Mobiles
{
    [CorpseName("an ostard corpse")]
    public class FrenziedOstard : BaseMount
    {
        [Constructable]
        public FrenziedOstard()
            : this("a frenzied ostard")
        {
        }

        [Constructable]
        public FrenziedOstard(string name)
            : base(name, 0xDA, 0x3EA4, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Hue = Utility.RandomHairHue() | 0x8000;

            this.BaseSoundID = 0x275;

			// [역산] 명성 1200 보너스(Str+592, Hits+2298, Skill+3.1) 반영
			this.SetStr(108, 158); // 최종 Str 700~750
			this.SetDex(182, 232); // 최종 Dex ~450 (폭주하는 속도)

			this.SetHits(702, 902); // 최종 Hits 3,000~3,200
			this.SetStam(132, 182); // 최종 Stam 200~250
			this.SetMana(0);

			SetAttackSpeed(2.0); // 타조 중 가장 빠름
			SetDamage(15, 25); // 평균 20.0

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 10, 20);

			this.Fame = 1200;
			this.Karma = -1200;
			this.VirtualArmor = 4;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 91.1; // 전투용 펫으로 인기 높음
        }

        public FrenziedOstard(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish | FoodType.Eggs | FoodType.FruitsAndVegies;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Ostard;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}