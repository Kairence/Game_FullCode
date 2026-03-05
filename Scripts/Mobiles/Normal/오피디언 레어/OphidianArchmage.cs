using System;

namespace Server.Mobiles
{
    [CorpseName("an ophidian corpse")]
    [TypeAlias("Server.Mobiles.OphidianJusticar", "Server.Mobiles.OphidianZealot")]
    public class OphidianArchmage : BaseCreature
    {
        private static readonly string[] m_Names = new string[]
        {
            "an ophidian justicar",
            "an ophidian zealot"
        };
        [Constructable]
        public OphidianArchmage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = m_Names[Utility.Random(m_Names.Length)];
            this.Body = 85;
            this.BaseSoundID = 639;

			/* [Ophidian Archmage - Normal - Fame 11,500 / Weight 1.25]
			   - 오피디언 최상위 마법사 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 9 (기본 11 + 로브 보정 -2)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(290, 310); 
			this.SetHits(6500, 6700); 
			this.SetDex(55, 65);
			this.SetInt(55, 65);

			// [Combat Options] 물리 50% / 마법 50% (에너지)
			this.SetDamage(35, 60);
			this.SetAttackSpeed(2.5);
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] 75% 캡 준수 및 약점(화염) 설정
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 30, 40);      // 사막의 태양에도 불구, 불엔 취약
			this.SetResistance(ResistanceType.Cold, 50, 65);    
			this.SetResistance(ResistanceType.Poison, 60, 75);   // 독 저항 특화
			this.SetResistance(ResistanceType.Energy, 50, 65);   

			// [Skills] 기본 110~120에 역산 보너스(9.9) 가산
			this.SetSkill(SkillName.Wrestling, 115.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 115.0, 125.0);
			this.SetSkill(SkillName.Magery, 120.0, 130.0);       // 강력한 대마법사
			this.SetSkill(SkillName.EvalInt, 120.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 115.0, 125.0);

			this.Tamable = false;
			this.VirtualArmor = 9;
			this.Fame = 11500;
			this.Karma = -11500;

            this.PackReg(5, 15);
            this.PackNecroReg(5, 15);
        }

        public OphidianArchmage(Serial serial)
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
		public override int TreasureMapLevel
        {
            get
            {
                return 2;
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
