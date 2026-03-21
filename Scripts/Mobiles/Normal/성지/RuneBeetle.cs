using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rune beetle corpse")]
    public class RuneBeetle : BaseCreature
    {
        private static readonly Hashtable m_Table = new Hashtable();
        [Constructable]
        public RuneBeetle()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a rune beetle";
            Body = 244;

			/* [Rune Beetle - Holy City Dungeon / Original Wiki & Keep Formula]
			   - 명성: 12,000 / 카르마: -12,000
			   - 슬롯: 3 (마법/중독 특화 펫)
			   - 가방 방어력: 17 (외골격 보정 +5)
			   -------------------------------------------------- */

			// [Attributes] 공식 가중치 1.25 적용
			this.SetStr(400, 500); 
			this.SetHits(8500, 10000); // 3슬롯 중 최상위권 체력
			this.SetDex(125, 175); 
			this.SetInt(350, 450);    // 위키 고증: 매우 높은 지능 (강력한 마법)

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] ★ 형님 지침 반영: 75%를 넘지 않는 상식적 저항
			this.SetResistance(ResistanceType.Physical, 45, 60); // 유저 대미지 절반 이상 박힘
			this.SetResistance(ResistanceType.Fire, 35, 50);     
			this.SetResistance(ResistanceType.Cold, 35, 50);    
			this.SetResistance(ResistanceType.Poison, 70, 75);  // ★ 독 저항은 컨셉상 높게 (75% 캡)
			this.SetResistance(ResistanceType.Energy, 60, 70);   // 에너지 저항 (75% 미만 유지)

			// [Skills] 강력한 마법과 독 기술
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Magery, 105.0, 120.0);    // 위키 고증: 상급 마법 사용
			this.SetSkill(SkillName.Poisoning, 110.0, 125.0); // 위키 고증: 치명적 중독

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 3; 
			this.MinTameSkill = 150.1; // 스킬 200 서버 기준 고숙련 테이머용
			this.VirtualArmor = 17;    // 공식: (12000/1000) + 5

			this.Fame = 12000;
			this.Karma = -12000;

            if (Utility.RandomDouble() < .25)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            PackBodyPartOrBones();

            Tamable = true;
            ControlSlots = 18;
            MinTameSkill = 93.9;

            SetSpecialAbility(SpecialAbility.RuneCorruption);
            SetWeaponAbility(WeaponAbility.BleedAttack);
        }

        public RuneBeetle(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }

        public override int GetAngerSound()
        {
            return 0x4E8;
        }

        public override int GetIdleSound()
        {
            return 0x4E7;
        }

        public override int GetAttackSound()
        {
            return 0x4E6;
        }

        public override int GetHurtSound()
        {
            return 0x4E9;
        }

        public override int GetDeathSound()
        {
            return 0x4E5;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.MedScrolls, 1);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)3);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version < 1)
            {
                for (int i = 0; i < Skills.Length; ++i)
                {
                    Skills[i].Cap = Math.Max(100.0, Skills[i].Cap * 0.9);

                    if (Skills[i].Base > Skills[i].Cap)
                    {
                        Skills[i].Base = Skills[i].Cap;
                    }
                }
            }

            if (version < 3)
            {
                if (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None)
                {
                    SetMagicalAbility(MagicalAbility.Poisoning);
                }

                if (version == 1)
                {
                    SetSpecialAbility(SpecialAbility.RuneCorruption);
                    SetWeaponAbility(WeaponAbility.BleedAttack);
                }
            }
        }
    }
}
