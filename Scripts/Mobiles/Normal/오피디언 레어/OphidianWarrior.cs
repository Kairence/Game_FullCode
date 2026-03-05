using System;

namespace Server.Mobiles
{
    [CorpseName("an ophidian corpse")]
    public class OphidianWarrior : BaseCreature
    {
        private static readonly string[] m_Names = new string[]
        {
            "an ophidian warrior",
            "an ophidian enforcer"
        };
        [Constructable]
        public OphidianWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = m_Names[Utility.Random(m_Names.Length)];
            this.Body = 86;
            this.BaseSoundID = 634;

			/* [Ophidian Warrior - Normal - Fame 4,500 / Weight 1.15]
			   - 오피디언 일반 보병 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 5 (기본 4 + 보정 1)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(55, 65); 
			this.SetHits(1200, 1350); 
			this.SetDex(10, 15);
			this.SetInt(10, 15);

			// [Combat Options] 100% 물리 대미지
			this.SetDamage(15, 30);
			this.SetAttackSpeed(2.2);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 초중급 사냥터에 맞게 쾌적한 저항 설정
			this.SetResistance(ResistanceType.Physical, 35, 45); 
			this.SetResistance(ResistanceType.Fire, 20, 30);      // ★ 사막 생물 약점
			this.SetResistance(ResistanceType.Cold, 25, 35);    
			this.SetResistance(ResistanceType.Poison, 40, 50);   // 종족 기본 내성
			this.SetResistance(ResistanceType.Energy, 20, 30);   

			// [Skills] 기본 80~90에 역산 보너스(1.9) 가산
			this.SetSkill(SkillName.Wrestling, 82.0, 92.0); 
			this.SetSkill(SkillName.Tactics, 82.0, 92.0);
			this.SetSkill(SkillName.Anatomy, 82.0, 92.0);
			this.SetSkill(SkillName.MagicResist, 75.0, 85.0);

			this.Tamable = false;
			this.VirtualArmor = 5;
			this.Fame = 4500;
			this.Karma = -4500;
        }

        public OphidianWarrior(Serial serial)
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
                return 1;
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
            this.AddLoot(LootPack.Meager);
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Gems);
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
