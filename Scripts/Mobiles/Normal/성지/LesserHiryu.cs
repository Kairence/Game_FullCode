using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a hiryu corpse")]
    public class LesserHiryu : BaseMount
    {
        [Constructable]
        public LesserHiryu()
            : base("a lesser hiryu", 243, 0x3E94, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Hue = GetHue();

			/* [Lesser Hiryu - Google Keep Formula: Mid-Tier Balance]
			   - 명성: 6,000 / 카르마: -6,000
			   - 슬롯: 2 (조합형 주력 펫)
			   - 저항: 75% 같은 사태 방지, 30-40%대의 쾌적한 타격감 확보
			   -------------------------------------------------- */

			// [Attributes] 공식 가중치 1.15 적용
			this.SetStr(300, 400); 
			this.SetHits(2500, 3500); // 저항 대신 체력으로 중급 맷집 보정
			this.SetDex(110, 160); 
			this.SetInt(100, 150);

			// [Combat Options] 
			this.SetDamage(25, 45); 
			this.SetAttackSpeed(2.2);
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] ★ 형님 지침 반영: 유저 대미지가 시원하게 박히는 저항
			this.SetResistance(ResistanceType.Physical, 30, 45); // 뎀감 40% 내외
			this.SetResistance(ResistanceType.Fire, 10, 20);      // 화염 약점 확실히 노출
			this.SetResistance(ResistanceType.Cold, 25, 35);    
			this.SetResistance(ResistanceType.Poison, 25, 35); 
			this.SetResistance(ResistanceType.Energy, 30, 45);   

			// [Skills]
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 90.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 85.0, 105.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; 
			this.MinTameSkill = 110.1; // 스킬 200 서버 기준 입문/중급 펫
			this.VirtualArmor = 8;

			this.Fame = 6000;
			this.Karma = -6000;

            if (Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            SetWeaponAbility(WeaponAbility.Dismount);
            SetSpecialAbility(SpecialAbility.GraspingClaw);
        }

        public LesserHiryu(Serial serial)
            : base(serial)
        {
        }

        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
            }
        }
        public override int Meat
        {
            get
            {
                return 16;
            }
        }
        public override int Hides
        {
            get
            {
                return 60;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }

        public override bool OverrideBondingReqs()
        {
            if (ControlMaster.Skills[SkillName.Bushido].Base >= 90.0)
                return true;
            return false;
        }

        public override int GetAngerSound()
        {
            return 0x4FE;
        }

        public override int GetIdleSound()
        {
            return 0x4FD;
        }

        public override int GetAttackSound()
        {
            return 0x4FC;
        }

        public override int GetHurtSound()
        {
            return 0x4FF;
        }

        public override int GetDeathSound()
        {
            return 0x4FB;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.Gems, 4);
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

            double tamingChance = base.GetControlChance(m, useBaseSkill);

            if (tamingChance >= 0.95)
            {
                return tamingChance;
            }

            double skill = (useBaseSkill ? m.Skills.Bushido.Base : m.Skills.Bushido.Value);

            if (skill < 90.0)
            {
                return tamingChance;
            }

            double bushidoChance = (skill - 30.0) / 100;

            if (m.Skills.Bushido.Base >= 120)
                bushidoChance += 0.05;

            return bushidoChance > tamingChance ? bushidoChance : tamingChance;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)4);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
                Timer.DelayCall(TimeSpan.Zero, delegate { Hue = GetHue(); });

            if (version <= 1)
                Timer.DelayCall(TimeSpan.Zero, delegate
                {
                    if (InternalItem != null)
                    {
                        InternalItem.Hue = Hue;
                    }
                });

            if (version < 2)
            {
				/*
                for (int i = 0; i < Skills.Length; ++i)
                {
                    Skills[i].Cap = Math.Max(100.0, Skills[i].Cap * 0.9);

                    if (Skills[i].Base > Skills[i].Cap)
                    {
                        Skills[i].Base = Skills[i].Cap;
                    }
                }
				*/
            }

            if (version < 3)
            {
                SetWeaponAbility(WeaponAbility.Dismount);
            }

            if (version < 3 && Controlled && RawStr >= 301 && ControlSlots == ControlSlotsMin)
            {
                //Server.SkillHandlers.AnimalTaming.ScaleStats(this, 0.5);
            }

            if (version < 4 && PetTrainingHelper.Enabled && ControlSlots <= 3)
            {
                var profile = PetTrainingHelper.GetAbilityProfile(this);

                if (profile == null || !profile.HasCustomized())
                {
                    MinTameSkill = 98.7;
                    ControlSlotsMin = 1;
                    ControlSlots = 1;
                }
            }
        }

        private static int GetHue()
        {
            int rand = Utility.Random(527);

            /*

            500	527	No Hue Color	94.88%	0
            10	527	Green			1.90%	0x8295
            10	527	Green			1.90%	0x8163	(Very Close to Above Green)	//this one is an approximation
            5	527	Dark Green		0.95%	0x87D4
            1	527	Valorite		0.19%	0x88AB
            1	527	Midnight Blue	0.19%	0x8258

            * */

            if (rand <= 0)
                return 0x8258;
            else if (rand <= 1)
                return 0x88AB;
            else if (rand <= 6)
                return 0x87D4;
            else if (rand <= 16)
                return 0x8163;
            else if (rand <= 26)
                return 0x8295;

            return 0;
        }
    }
}
