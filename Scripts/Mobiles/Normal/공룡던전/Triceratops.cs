using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a triceratops corpse")]
    public class Triceratops : BaseCreature
    {
        public override double HealChance { get { return .167; } }

        [Constructable]
        public Triceratops()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Triceratops";
            Body = 0x587;
            Female = true;

			/* [Triceratops - Fame 8,500 / Dinosaur / Weight 1.26]
			   - 스킬 200 마스터 서버용 '상급 하드 탱커' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (8,500/1000) + 6.5 = 15
			   - 테이밍 난이도: 95.0 ~ 105.0 (상급 테이머의 든든한 방패)
			   -------------------------------------------------- */

			// [Attributes] 명성 8,500 보너스 + 가중치 1.26 반영
			this.SetStr(180, 230); 
			this.SetHits(4200, 5200); 
			this.SetDex(35, 50);
			this.SetInt(35, 50);

			// [Combat Options] 세 개의 뿔을 이용한 돌진
			this.SetDamage(35, 55);
			this.SetAttackSpeed(2.8); // 묵직하고 느린 한 방

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 천연 장갑판 (물리 저항 75% 캡 근접)
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 35, 45);

			// [Skills] 유저 스킬 110 ~ 140 구간 최적화
			this.SetSkill(SkillName.Wrestling, 110.0, 130.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 110.0);
			this.SetSkill(SkillName.Parry, 80.0, 100.0); // 프릴을 이용한 방어 효과

			// [Taming] ★ 테이밍 가능 (음식 설정 제외)
			this.Tamable = true;
			this.ControlSlots = 3; // 압도적인 맷집으로 인해 3슬롯 점유
			this.MinTameSkill = 95.0; // 숙련된 테이머만이 다룰 수 있는 거수

			// [Misc]
			this.VirtualArmor = 15;

			this.Fame = 8500;
			this.Karma = -8500;

            SetMagicalAbility(MagicalAbility.Piercing);
        }

        public override int GetIdleSound() { return 0x673; }
        public override int GetAngerSound() { return 0x670; }
        public override int GetHurtSound() { return 0x672; }
        public override int GetDeathSound() { return 0x671; }

        public override double WeaponAbilityChance { get { return 0.5; } }
        
        public override int Hides { get { return 11; } }
        public override HideType HideType { get { return HideType.Regular; } }
        public override int Meat { get { return 3; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies; } }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool StatLossAfterTame { get { return true; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 1);
        }

        public Triceratops(Serial serial)
            : base(serial)
        {
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
