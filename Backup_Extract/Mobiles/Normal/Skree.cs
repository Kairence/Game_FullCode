using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skree corpse")]
    public class Skree : BaseCreature
    {
        [Constructable]
        public Skree()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skree";
            Body = 733;

			/* [Skree - Fame 5,000 / General / Weight 1.19]
			   - 스킬 200 마스터 서버용 '중급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (5,000/1000) + 2 = 7 (질긴 비행 가죽)
			   - 케페치(40~55)와 솔렌 워리어(75~85) 사이의 징검다리
			   -------------------------------------------------- */

			// [Attributes] 명성 5,000 보너스 + 가중치 1.19 반영
			this.SetStr(70, 95); 
			this.SetHits(1600, 1850); 
			this.SetDex(12, 20);
			this.SetInt(12, 20);

			// [Combat Options]
			this.SetDamage(20, 35);
			this.SetAttackSpeed(2.0);

			// [Damage Types] 60% 물리 + 20% 에너지 + 20% 독
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Energy, 20);
			this.SetDamageType(ResistanceType.Poison, 20);

			// [Resistances] 총합 약 170 (중급 저항)
			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 20, 30);
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 40, 50); // 에너지 저항 특화

			// [Skills] ★ 스킬 200 서버 기준 - 중급자 수련의 핵심 (재설계)
			// 유저 스킬 70 ~ 90 구간 사냥에 최적화
			this.SetSkill(SkillName.Wrestling, 60.0, 75.0); 
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);
			this.SetSkill(SkillName.Anatomy, 55.0, 70.0);
			this.SetSkill(SkillName.MagicResist, 70.0, 85.0); // 비행 야수답게 마법 저항이 높음

			// [Misc] 가상 방어력(Virtual Armor): (5,000/1000) + 2 = 7
			this.VirtualArmor = 7;

			this.Fame = 5000;
			this.Karma = -5000;

		}

        public Skree(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get { return 3; }
        }

        public override MeatType MeatType
        {
            get { return MeatType.Bird; }
        }

        public override int Hides
        {
            get { return 5; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override int GetIdleSound()
        {
            return 1585;
        }

        public override int GetAngerSound()
        {
            return 1582;
        }

        public override int GetHurtSound()
        {
            return 1584;
        }

        public override int GetDeathSound()
        {
            return 1583;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}
