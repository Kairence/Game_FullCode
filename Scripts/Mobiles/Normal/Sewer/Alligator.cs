using System;

namespace Server.Mobiles
{
    [CorpseName("an alligator corpse")]
    public class Alligator : BaseCreature
    {
        [Constructable]
        public Alligator()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an alligator";
            Body = 0xCA;
            BaseSoundID = 660;

			/* [Alligator - Fame 2,200 / Sewer / Weight 1.19]
			   - 스킬 200 마스터 서버용 '중급 공격형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (2,200/1000) + 2.8 = 5
			   - 테이밍 난이도: 45.0 ~ 55.0 (불프로그 다음 단계)
			   -------------------------------------------------- */

			// [Attributes] 명성 2,200 보너스 + 가중치 1.19 반영
			this.SetStr(30, 40); 
			this.SetHits(650, 850); 
			this.SetDex(6, 8);
			this.SetInt(6, 8);

			// [Combat Options] 날카로운 이빨의 대미지
			this.SetDamage(12, 22);
			this.SetAttackSpeed(2.2); // 두꺼비보다 약간 더 빠름

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 파충류의 질긴 가죽
			this.SetResistance(ResistanceType.Physical, 25, 35); 
			this.SetResistance(ResistanceType.Fire, 5, 15);
			this.SetResistance(ResistanceType.Cold, 25, 35);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 10, 20);

			// [Skills] 유저 스킬 50 ~ 70 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 55.0, 70.0); 
			this.SetSkill(SkillName.Tactics, 55.0, 70.0);
			this.SetSkill(SkillName.MagicResist, 35.0, 50.0);

			// [Taming & Food] ★ 가상 방어구 상단 배치
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 45.0; // 불프로그(35.0)를 졸업한 테이머의 표적

			// [Misc]
			this.VirtualArmor = 5;

			this.Fame = 2200;
			this.Karma = -2200;
        }

        public Alligator(Serial serial)
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
                return FoodType.Meat | FoodType.Fish;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}