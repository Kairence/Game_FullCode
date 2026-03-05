using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ophidian corpse")]
    [TypeAlias("Server.Mobiles.OphidianAvenger")]
    public class OphidianKnight : BaseCreature
    {
        private static readonly string[] m_Names = new string[]
        {
            "an ophidian knight-errant",
            "an ophidian avenger"
        };
        [Constructable]
        public OphidianKnight()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = m_Names[Utility.Random(m_Names.Length)];
            this.Body = 86;
            this.BaseSoundID = 634;

			/* [Ophidian Knight - Normal - Fame 9,000 / Weight 1.28]
			   - 오피디언 정예 창병 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 14 (기본 9 + 판금 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (중갑 전사 컨셉)
			this.SetStr(240, 255); 
			this.SetHits(5400, 5600); 
			this.SetDex(45, 55);
			this.SetInt(45, 55);

			// [Combat Options] 100% 물리 대미지 (강력한 찌르기)
			this.SetDamage(35, 55);
			this.SetAttackSpeed(2.3);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 및 명확한 약점(냉기) 설정
			this.SetResistance(ResistanceType.Physical, 50, 60); 
			this.SetResistance(ResistanceType.Fire, 35, 45);      
			this.SetResistance(ResistanceType.Cold, 15, 25);     // ★ 확실한 약점 (냉혈 동물)
			this.SetResistance(ResistanceType.Poison, 65, 75);   // 독 저항 특화
			this.SetResistance(ResistanceType.Energy, 35, 45);   

			// [Skills] 기본 95~110에 역산 보너스(8.19) 가산
			this.SetSkill(SkillName.Wrestling, 105.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 105.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 105.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 105.0);

			this.Tamable = false;
			this.VirtualArmor = 14;
			this.Fame = 9000;
			this.Karma = -9000;
            this.PackItem(new LesserPoisonPotion());
        }

        public OphidianKnight(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
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
            this.AddLoot(LootPack.Rich, 2);
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
