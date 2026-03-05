using System;

namespace Server.Mobiles
{
    [CorpseName("a savage ridgeback corpse")]
    public class SavageRidgeback : BaseMount
    {
        [Constructable]
        public SavageRidgeback()
            : this("a savage ridgeback")
        {
        }

        [Constructable]
        public SavageRidgeback(string name)
            : base(name, 188, 0x3EB8, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0x3F3;

            this.SetStr(89, 139); // 최종 Str 650~700
			this.SetDex(137, 187); // 최종 Dex ~350

			this.SetHits(934, 1434); // 최종 Hits 2,500~3,000
			this.SetStam(87, 137); 
			this.SetMana(0);

			SetAttackSpeed(2.5);
			SetDamage(12, 20); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 40, 50); // 장갑 강화
			this.SetResistance(ResistanceType.Fire, 15, 25);

			this.Fame = 800;
			this.VirtualArmor = 10; // 전용 방어구 착용 컨셉
        }

        public SavageRidgeback(Serial serial)
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