using System;

namespace Server.Mobiles
{
    [CorpseName("an ostard corpse")]
    public class DesertOstard : BaseMount
    {
        [Constructable]
        public DesertOstard()
            : this("a desert ostard")
        {
        }

        [Constructable]
        public DesertOstard(string name)
            : base(name, 0xD2, 0x3EA3, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0x270;

			// [역산] 명성 450 보너스(Str+533, Hits+878, Stam+58, Skill+1.1) 반영
			this.SetStr(17, 27);
			this.SetDex(92, 112); // 최종 Dex ~260 (매우 빠름)
			this.SetInt(10, 20);

			this.SetHits(322, 350); // 최종 Hits 1,200~1,228
			this.SetStam(92, 112);  // 최종 Stam 150~170
			this.SetMana(0);

			SetAttackSpeed(2.5);
			SetDamage(5, 12); // 평균 8.5

			this.SetSkill(SkillName.Wrestling, 3.9, 5.9); // 최종 5.0~7.0

			this.Fame = 450;
			this.VirtualArmor = 1;
			this.Tamable = true;
			this.MinTameSkill = 29.1;
			
			SetDamageType(ResistanceType.Physical, 100);
        }

        public DesertOstard(Serial serial)
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
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
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