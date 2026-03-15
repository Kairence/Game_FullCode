using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a terathan matriarch corpse")]
    public class TerathanMatriarch : BaseCreature
    {
        [Constructable]
        public TerathanMatriarch()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a terathan matriarch";
            this.Body = 72;
            this.BaseSoundID = 599;

			/* [Terathan Matriarch - Normal - Fame 14,500 / Weight 1.24]
			   - 테라탄 던전의 고위 술사 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 12 (명성/1000 - 2 보정)
			   - 특이사항: 강력한 마법 및 커즈(Curse) 위주의 디버프
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 약 8,500대)
			this.SetStr(380, 395); 
			this.SetHits(8550, 8700); 
			this.SetDex(75, 85); 
			this.SetInt(390, 410); // 매우 높은 지능으로 인한 마법 위력 극대화

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Poison, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			// [Resistances] 최고 저항 75 이하 준수 / 물리 및 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 30, 40); // ★ 약점 (근접전에 취약)
			this.SetResistance(ResistanceType.Fire, 15, 25);     // ★ 확실한 약점 (불에 취약)
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 75, 75);  // 독성 저항 Max (면역)
			this.SetResistance(ResistanceType.Energy, 65, 75);  // 높은 마법 내성

			// [Skills] 기본 115~125에 역산 보너스(12.9) 가산
			// 최종 숙련도 약 130~140대의 고위 술사
			this.SetSkill(SkillName.Wrestling, 127.9, 137.9); 
			this.SetSkill(SkillName.Tactics, 127.9, 137.9);
			this.SetSkill(SkillName.Magery, 130.0, 145.0);      // 상급 마법 및 소환
			this.SetSkill(SkillName.EvalInt, 130.0, 145.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 135.0);

			this.Tamable = false;
			this.VirtualArmor = 12;
			this.Fame = 14500;
			this.Karma = -14500;

            this.PackItem(new SpidersSilk(5));
            this.PackNecroReg(Utility.RandomMinMax(4, 10));
        }

        public TerathanMatriarch(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }

        public override TribeType Tribe { get { return TribeType.Terathan; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.TerathansAndOphidians;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Average, 2);
            this.AddLoot(LootPack.MedScrolls, 2);
            this.AddLoot(LootPack.Potions);
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
