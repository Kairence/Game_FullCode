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
            // [역산] 최종 Str 1000 / Hits 6000~7000 목표
			this.SetStr(244, 344); 
			this.SetDex(66, 116);  // 최종 Dex ~350

			this.SetHits(2102, 3102); 
			this.SetStam(66, 116);
			this.SetMana(0);

			SetAttackSpeed(3.5);
			SetDamage(24, 36); // 리지백보다 상위 단계의 묵직한 공격

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항 설정 (비늘 갑옷)
			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Poison, 30, 40);

			// 최종 스킬 110.0~120.0 목표
			this.SetSkill(SkillName.Wrestling, 104.6, 114.6);

			this.Fame = 2000;
			this.VirtualArmor = 12;
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 93.1;
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