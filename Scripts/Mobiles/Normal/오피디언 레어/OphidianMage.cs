using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ophidian corpse")]
    [TypeAlias("Server.Mobiles.OphidianShaman")]
    public class OphidianMage : BaseCreature
    {
        private static readonly string[] m_Names = new string[]
        {
            "an ophidian apprentice mage",
            "an ophidian shaman"
        };
        [Constructable]
        public OphidianMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = m_Names[Utility.Random(m_Names.Length)];
            this.Body = 85;
            this.BaseSoundID = 639;

			/* [Ophidian Mage - Normal - Fame 6,000 / Weight 1.20]
			   - 오피디언 중급 마법사 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 3 (기본 6 + 로브 보정 -3)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(100, 115); 
			this.SetHits(2300, 2500); 
			this.SetDex(20, 25);
			this.SetInt(20, 25);

			SetAttackSpeed(10.0);
			SetDamage(12, 20);
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80);

			// [Resistances] 최고 저항 75 이하 준수 및 명확한 약점(물리/냉기) 설정
			this.SetResistance(ResistanceType.Physical, 25, 35); // 전사가 붙으면 순삭 가능
			this.SetResistance(ResistanceType.Fire, 30, 40);      
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 60, 75);   // 독 저항 특화
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 90~100에 역산 보너스(3.6) 가산
			this.SetSkill(SkillName.Wrestling, 95.0, 105.0); 
			this.SetSkill(SkillName.Tactics, 95.0, 105.0);
			this.SetSkill(SkillName.Magery, 95.0, 105.0);
			this.SetSkill(SkillName.EvalInt, 95.0, 105.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 100.0);

			this.Tamable = false;
			this.VirtualArmor = 3;
			this.Fame = 6000;
			this.Karma = -6000;

            this.VirtualArmor = 30;

            this.PackReg(10);

			switch (Utility.Random(6))
            {
                case 0: PackItem(new PainSpikeScroll()); break;
			}

        }

        public OphidianMage(Serial serial)
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
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.LowScrolls);
            this.AddLoot(LootPack.MedScrolls);
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
