using System;

namespace Server.Mobiles
{
    [CorpseName("a mantra effervescence corpse")]
    public class MantraEffervescence : BaseCreature
    {
        [Constructable]
        public MantraEffervescence()
            : base(AIType.AI_Spellweaving, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a mantra effervescence";
            Body = 0x111;
            BaseSoundID = 0x56E;

			/* [Mantra Effervescence - Fame 11,000 / Normal / Weight 1.25]
			   - 빛의 프리즘 던전 고위 마법 정령
			   - 에페르베선스 컨셉: 높은 마나 재생, 에너지/냉기 특화, 물리/화염 취약
			   - 비정형 마력체: 테이밍 불가 (200 숙련도 고려), Karma 0
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.25) - 기본 보너스
			// Str: 보너스 약 1,100 -> 최종 Set 약 150-200
			this.SetStr(150, 200); 

			// Hits: 보너스 약 25,000 -> 최종 Set 약 5,500-6,500
			this.SetHits(5500, 6500); 

			this.SetDex(160, 200); 
			this.SetInt(500, 600); // ★ 거대한 마나 통과 지능

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 0);
			this.SetDamageType(ResistanceType.Energy, 100);

			// [Resistances] 마력 기포 컨셉 (에너지/냉기 특화, 물리/화염 약점)
			this.SetResistance(ResistanceType.Physical, 25, 35); // 기체에 가까워 물리 방어력 낮음
			this.SetResistance(ResistanceType.Fire, 20, 30);      // ★ 고열에 의해 기포가 증발함 (약점)
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 무기물 정령으로 독에 강함
			this.SetResistance(ResistanceType.Energy, 70, 75);  // ★ 마력 응집체 (에너지 흡수)

			// [Skills] 고위 마법 능력 극대화
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0); // 마법 저항력 매우 높음
			this.SetSkill(SkillName.Magery, 120.0, 135.0);      // 8서클 마법 자유자재 구사
			this.SetSkill(SkillName.EvalInt, 120.0, 135.0);
			this.SetSkill(SkillName.Meditation, 130.0, 150.0); // 압도적인 마나 회복

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 8;

			this.Fame = 11000;
			this.Karma = 0; // 중립적 마력 개체

            SetAreaEffect(AreaEffect.AuraOfEnergy);
        }

        public MantraEffervescence(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
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
