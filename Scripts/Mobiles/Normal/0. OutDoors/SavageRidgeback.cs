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

            SetStr(58, 100);
            SetDex(56, 75);
            SetInt(16, 30);

            SetHits(41, 54);
            SetStam(10, 15);
            SetMana(10, 15);

			SetAttackSpeed(30.0);

            SetDamage(3, 5);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Fire, 10, 15);
            SetResistance(ResistanceType.Cold, 15, 20);
            SetResistance(ResistanceType.Poison, 10, 15);
            SetResistance(ResistanceType.Energy, 10, 15);

            Fame = 300;
            Karma = 0;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 63.1;
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