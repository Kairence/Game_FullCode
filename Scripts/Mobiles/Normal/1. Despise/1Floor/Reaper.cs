using System;
using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
    [CorpseName("a reapers corpse")]
    public class Reaper : BaseCreature
    {
        [Constructable]
        public Reaper()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a reaper";
            this.Body = 47;
            this.BaseSoundID = 442;

			Boss = true;

            /* [Despise Level 1 Boss - Reaper - Fame 8,000 / Weight 1.21]
			   - 컨셉: 원거리 마법 포탑형 고대 나무
			   - VirtualArmor: (8,000/1000) + 5 = 13 (단단한 나무 껍질 보정 +5)
			   - 편차 수정: 보스급 안정화 룰 적용 (편차 1,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 5,500
			this.SetStr(4500, 4800); 

			// 최종 Hits 약 102,000 (1층 유저들에겐 통곡의 벽)
			this.SetHits(84500, 85500); 

			// 최종 Dex/Int 약 1,100 (지능적인 마법 구사)
			this.SetDex(900, 1000);
			this.SetInt(900, 1000);

			// 최종 Stam/Mana 약 960
			this.SetStam(780, 830);
			this.SetMana(780, 830);

			// 사용자님의 마법사 원칙(10s+)에 따라 공속을 12.0초로 대폭 하향했습니다.
			// 평타 데미지는 보스급 위엄을 위해 최소한의 수치(20-30)만 남겼습니다.
			SetAttackSpeed(12.0); 
			SetDamage(20, 30);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 60, 70); // 나무라 물리 방어 우수
			this.SetResistance(ResistanceType.Fire, 10, 20);      // 약점: 화염 (나무의 숙명)
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 65, 75);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 최종 61.2 부근
			this.SetSkill(SkillName.Wrestling, 34.0, 38.0);
			this.SetSkill(SkillName.Magery, 50.0, 60.0);         // 마법 특화 보정
			this.SetSkill(SkillName.EvalInt, 50.0, 60.0);
			this.SetSkill(SkillName.MagicResist, 50.0, 60.0);

			// 가방 방어력: (8,000/1000) + 5 = 13
			this.VirtualArmor = 13;

			this.Fame = 8000;
			this.Karma = -8000;
        }

        public Reaper(Serial serial)
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
                return 2;
            }
        }
        public override bool DisallowAllMoves
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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