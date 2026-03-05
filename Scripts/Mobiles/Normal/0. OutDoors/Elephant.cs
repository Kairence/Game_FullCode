using System;

namespace Server.Mobiles
{
    [CorpseName("a elephant corpse")]
    public class Elephant : BaseMount
    {
        [Constructable]
        public Elephant()
            : this("a elephant")
        {
        }

        [Constructable]
        public Elephant(string name)
            : base(name, 187, 0x3EBA, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0x599;

			// [역산] 명성 20,000 보너스(Str+3000, Hits+55650, Stam+543, Skill+83.3) 반영
			// 최종 Str 4,200~4,500 목표 (서버 상위권 완력)
			this.SetStr(1200, 1500);
			this.SetDex(100, 150);  // 최종 Dex ~800 도달 (거대하지만 신성한 기운으로 빠름)
			this.SetInt(800, 1000); 

			// 최종 Hits 150,000~160,000 목표 (명성 3만급 보스 아래 최강의 맷집)
			this.SetHits(94350, 104350);
			this.SetStam(300, 400); // 최종 Stam ~900
			this.SetMana(1000, 1500);

			// [컨셉] 산이 무너지는 듯한 일격
			SetAttackSpeed(4.5);
			SetDamage(120, 180); // 평균 150.0 (전사 체력 4,000의 약 4%를 한 방에 삭감)

			// [역산] 최종 Skill 160.0~175.0 목표 (상한 200 대비 준마스터 등급)
			this.SetSkill(SkillName.Wrestling, 76.7, 91.7);
			this.SetSkill(SkillName.Tactics, 76.7, 91.7);
			this.SetSkill(SkillName.MagicResist, 96.7, 116.7); // 마법 저항에 매우 특화

			this.Fame = 20000;
			this.Karma = 20000; 
			this.VirtualArmor = 20; // 신성한 오라를 두른 외피

			SetDamageType(ResistanceType.Physical, 100);

			// 저항 설정 (50 이하 유지)
			this.SetResistance(ResistanceType.Physical, 48, 50);
			this.SetResistance(ResistanceType.Fire, 35, 45);
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 45, 50);
			this.SetResistance(ResistanceType.Energy, 48, 50);

			this.Tamable = true;
			this.ControlSlots = 5;
			// [난이도] 스킬 200 만점 서버 기준 최상위 테이밍 난이도
			this.MinTameSkill = 185.1;
		}
        public Elephant(Serial serial)
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