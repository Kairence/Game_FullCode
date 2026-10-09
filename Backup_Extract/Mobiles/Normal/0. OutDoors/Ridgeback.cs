using System;

namespace Server.Mobiles
{
    [CorpseName("a ridgeback corpse")]
    public class Ridgeback : BaseMount
    {
        [Constructable]
        public Ridgeback()
            : this("a ridgeback")
        {
        }

        [Constructable]
        public Ridgeback(string name)
            : base(name, 187, 0x3EBA, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0x3F3;

            // [역산] 명성 600 보너스 반영
			this.SetStr(54, 104);  // 최종 Str 600~650
			this.SetDex(139, 189); // 최종 Dex ~350 (돌진력)

			this.SetHits(780, 1280); // 최종 Hits 2,000~2,500
			this.SetStam(39, 89);   
			this.SetMana(0);

			this.SetAttackSpeed(3.0);
			this.SetDamage(18, 26); // 늑대(16-24)보다 묵직하게 들이받음

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항 설정 (갑각)
			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Energy, 20, 30);

			this.Fame = 600;
			this.VirtualArmor = 8;
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 83.1;
        }

        public Ridgeback(Serial serial)
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
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override bool OverrideBondingReqs()
        {
            return true;
        }

        public override double GetControlChance(Mobile m, bool useBaseSkill)
        {
            if (PetTrainingHelper.Enabled)
            {
                var profile = PetTrainingHelper.GetAbilityProfile(this);

                if (profile != null && profile.HasCustomized())
                {
                    return base.GetControlChance(m, useBaseSkill);
                }
            }

            return 1.0;
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
