using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a beholder corpse")]
    public class Beholder : BaseCreature
    {
        [Constructable]
        public Beholder()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a beholder";
            this.Body = 53;
            this.BaseSoundID = 377;

			Boss = true;
				
            /* [Shame Boss - Beholder - Fame 28,000 / Weight 1.23]
			   - 컨셉: 고대 마법의 지배자
			   - VirtualArmor: (28,000/1000) + 0 = 28 (상한 30 준수)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 25,000 부근
			this.SetStr(20500, 21300); 

			// 최종 Hits 약 553,000 (민맥 편차 2,000 고정)
			this.SetHits(462800, 464800); 

			// 최종 Dex/Int 약 5,000 (지능형 마법사 특화)
			this.SetDex(4100, 4250);
			this.SetInt(4100, 4250);

			// 최종 Stam/Mana 약 5,200 (무한에 가까운 마력)
			this.SetStam(4300, 4500);
			this.SetMana(4300, 4500);

			// [Combat Options]
			this.SetDamage(60, 90);
			this.SetAttackSpeed(1.5);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 50, 60);
			this.SetResistance(ResistanceType.Fire, 55, 65);
			this.SetResistance(ResistanceType.Cold, 55, 65);
			this.SetResistance(ResistanceType.Poison, 70, 75);    // 독 면역에 가까운 저항
			this.SetResistance(ResistanceType.Energy, 70, 75);    // 마법 에너지 정점

			// [Skills] 최종 332.8 부근
			this.SetSkill(SkillName.Wrestling, 193.0, 203.0);
			this.SetSkill(SkillName.Magery, 200.0, 210.0);       // 신급 마법 숙련도
			this.SetSkill(SkillName.EvalInt, 200.0, 210.0);
			this.SetSkill(SkillName.Meditation, 200.0, 210.0);
			this.SetSkill(SkillName.MagicResist, 200.0, 210.0);

			// 가방 방어력: (28,000/1000) + 0 = 28
			this.VirtualArmor = 28;

			this.Fame = 28000;
			this.Karma = -28000;
        }

        public Beholder(Serial serial)
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
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
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