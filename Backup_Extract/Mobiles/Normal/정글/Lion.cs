using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a lion corpse")]
    public class Lion : BaseCreature
    {
        public override double HealChance { get { return .167; } }

        [Constructable]
        public Lion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Lion";
            Body = 0x592;
            Female = true;
            BaseSoundID = 0x3EF;

			/* [Lion - Normal - Fame 11,000 / Weight 1.25]
			   - 정글 던전의 상위 포식자 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 11 (명성/1000 공식 준수)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(275, 290); 
			this.SetHits(6200, 6400); 
			this.SetDex(80, 100);
			this.SetInt(80, 100);

			// [Combat Options] 물리 100% (강력한 앞발과 물어뜯기)
			this.SetDamage(35, 50);
			this.SetAttackSpeed(2.4); 
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 50, 65); 
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 25, 35);   // ★ 확실한 약점

			// [Skills] 기본 110~120에 역산 보너스(9.4) 가산
			this.SetSkill(SkillName.Wrestling, 119.0, 129.0); 
			this.SetSkill(SkillName.Tactics, 119.0, 129.0);
			this.SetSkill(SkillName.Anatomy, 119.0, 129.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 105.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 1; // 200 숙련도 시대의 기초적인 1슬롯 펫
			this.MinTameSkill = 105.0; 
			this.VirtualArmor = 11;
			this.Fame = 11000;
			this.Karma = -11000;

            SetMagicalAbility(MagicalAbility.Piercing);
        }

        public override int GetIdleSound() { return 0x673; }
        public override int GetAngerSound() { return 0x670; }
        public override int GetHurtSound() { return 0x672; }
        public override int GetDeathSound() { return 0x671; }

        public override double WeaponAbilityChance { get { return 0.5; } }
        
        public override int Hides { get { return 11; } }
        public override HideType HideType { get { return HideType.Regular; } }
        public override int Meat { get { return 5; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 1);
        }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool StatLossAfterTame { get { return true; } }

        public Lion(Serial serial)
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
                SetWeaponAbility(WeaponAbility.ArmorIgnore);
                SetWeaponAbility(WeaponAbility.BleedAttack);
                SetWeaponAbility(WeaponAbility.ParalyzingBlow);
            }
        }
    }
}
