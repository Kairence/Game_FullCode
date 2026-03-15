using System;

namespace Server.Mobiles
{
    [CorpseName("an ophidian corpse")]
    public class OphidianMatriarch : BaseCreature
    {
        [Constructable]
        public OphidianMatriarch()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ophidian matriarch";
            this.Body = 87;
            this.BaseSoundID = 644;

			/* [Ophidian Matriarch - Normal - Fame 13,500 / Weight 1.25]
			   - 오피디언 고위 여군주 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 14 (기본 13 + 보정 1)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(355, 380); 
			this.SetHits(8000, 8300); 
			this.SetDex(70, 80);
			this.SetInt(70, 80);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Poison, 50);

			// [Resistances] 최고 저항 75 이하 준수 및 약점 설정
			this.SetResistance(ResistanceType.Physical, 50, 60); 
			this.SetResistance(ResistanceType.Fire, 30, 40);      // ★ 명확한 약점
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 70, 75);   // 여왕다운 극독 내성
			this.SetResistance(ResistanceType.Energy, 50, 60);   

			// [Skills] 기본 115~125에 역산 보너스(12.2) 가산
			this.SetSkill(SkillName.Wrestling, 125.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 125.0, 135.0);
			this.SetSkill(SkillName.Anatomy, 125.0, 135.0);
			this.SetSkill(SkillName.Magery, 120.0, 135.0);       // 상급 마법 구사
			this.SetSkill(SkillName.EvalInt, 120.0, 135.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);

			this.Tamable = false;
			this.VirtualArmor = 14;
			this.Fame = 13500;
			this.Karma = -13500;
        }

        public OphidianMatriarch(Serial serial)
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
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }

        public override TribeType Tribe { get { return TribeType.Ophidian; } }

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
