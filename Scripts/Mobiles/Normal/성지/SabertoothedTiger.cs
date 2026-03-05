using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a saber-toothed tiger corpse")]
    public class SabertoothedTiger : BaseCreature
    {
        public override double HealChance { get { return .167; } }

        [Constructable]
        public SabertoothedTiger()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "saber-toothed tiger";
            Body = 0x588;
            Female = true;

			/* [Sabertoothed Tiger - Holy City Dungeon / Original Wiki & Keep Formula]
			   - 명성: 6,000 / 카르마: -6,000
			   - 슬롯: 2 (민첩형 딜러 펫)
			   - 가방 방어력: 5 (날렵한 가죽 보정 -1)
			   -------------------------------------------------- */

			// [Attributes] 공식 가중치 1.15 적용
			this.SetStr(300, 400); 
			this.SetHits(2500, 3500); // 저항이 낮은 대신 준수한 체력
			this.SetDex(150, 200);    // 위키 고증: 매우 빠른 민첩성
			this.SetInt(80, 120);

			// [Combat Options] 100% 물리 대미지 (출혈 공격 특화)
			this.SetDamage(30, 50); 
			this.SetAttackSpeed(2.0); // 빠른 연타 속도
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] ★ 형님 지침 반영: 75%를 넘지 않는 상식적 저항
			this.SetResistance(ResistanceType.Physical, 40, 55); // 유저 대미지 50% 이상 박힘
			this.SetResistance(ResistanceType.Fire, 15, 25);      // ★ 확실한 약점 (불에 약한 털)
			this.SetResistance(ResistanceType.Cold, 50, 65);     // 고산지대 적응 컨셉
			this.SetResistance(ResistanceType.Poison, 30, 40); 
			this.SetResistance(ResistanceType.Energy, 30, 40);   

			// [Skills] 포식자의 전투 기술
			this.SetSkill(SkillName.Wrestling, 105.0, 120.0); 
			this.SetSkill(SkillName.Tactics, 105.0, 120.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);   // 위키 고증: 높은 해부학 (출혈 대미지)
			this.SetSkill(SkillName.MagicResist, 85.0, 100.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; 
			this.MinTameSkill = 115.1; // 스킬 200 서버 기준 중급 펫
			this.VirtualArmor = 5;    // 공식: (6000/1000) - 1

			this.Fame = 6000;
			this.Karma = -6000;

            SetMagicalAbility(MagicalAbility.Slashing);
        }

        public override int GetIdleSound() { return 0x673; }
        public override int GetAngerSound() { return 0x670; }
        public override int GetHurtSound() { return 0x672; }
        public override int GetDeathSound() { return 0x671; }

        public override double WeaponAbilityChance { get { return 0.5; } }
        
        public override int Hides { get { return 11; } }
        public override HideType HideType { get { return HideType.Regular; } }
        public override int Meat { get { return 3; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }
        public override bool StatLossAfterTame { get { return true; } }
        public override bool CanAngerOnTame { get { return true; } }

        public override void OnAfterTame(Mobile tamer)
        {
            if (Owners.Count == 0 && PetTrainingHelper.Enabled)
            {
                RawStr = (int)Math.Max(1, RawStr * 0.5);
                RawDex = (int)Math.Max(1, RawDex * 0.5);

                HitsMaxSeed = RawStr;
                Hits = RawStr;

                StamMaxSeed = RawDex;
                Stam = RawDex;
            }
            else
            {
                base.OnAfterTame(tamer);
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 1);
        }

        public SabertoothedTiger(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
            {
                SetMagicalAbility(MagicalAbility.Slashing);
            }
        }
    }
}
