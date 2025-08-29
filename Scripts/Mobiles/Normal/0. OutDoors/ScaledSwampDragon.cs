using System;

namespace Server.Mobiles
{
    [CorpseName("a swamp dragon corpse")]
    public class ScaledSwampDragon : BaseMount
    {
        [Constructable]
        public ScaledSwampDragon()
            : this("a swamp dragon")
        {
        }

        [Constructable]
        public ScaledSwampDragon(string name)
            : base(name, 0x31F, 0x3EBE, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.SetStr(200, 270);
            this.SetDex(156, 175);
            this.SetInt(16, 20);

            this.SetHits(271, 288);
			this.SetStam(100, 120);
            this.SetMana(1, 5);

            this.SetDamage(1, 4);
			SetAttackSpeed(20.0);

            this.SetDamageType(ResistanceType.Physical, 100);

            Fame = 2000;
            Karma = -2000;

            Tamable = true;
            ControlSlots = 5;
            MinTameSkill = 113.9;
        }

        public ScaledSwampDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool AutoDispel
        {
            get
            {
                return !Controlled;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
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