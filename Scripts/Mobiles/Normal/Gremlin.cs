using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gremlin corpse")]
    public class Gremlin : BaseCreature
    {
        [Constructable]
        public Gremlin()
            : base(AIType.AI_Archer, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a gremlin";
            Body = 724; 

			/* [Gremlin - Fame 1,200 / General / Weight 1.13]
			   - 스킬 200 마스터 서버용 '초급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (1,200/1000) - 2.2 = -1 (최종 0)
			   - 불개미보다 약간 높고 일개미(Worker)보다 낮은 징검다리 단계
			   -------------------------------------------------- */

			// [Attributes] 명성 1,200 보너스 + 가중치 1.13 반영
			this.SetStr(10, 15); 
			this.SetHits(250, 300); 
			this.SetDex(2, 5);
			this.SetInt(2, 5);

			// [Combat Options]
			this.SetDamage(5, 12);
			this.SetAttackSpeed(1.8); // 몸집이 작아 공격 속도는 다소 빠름

			// [Damage Types] 100% 물리 공격 (할퀴기/물어뜯기)
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 총합 약 100 (초보자용 저항)
			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Cold, 10, 20);
			this.SetResistance(ResistanceType.Poison, 20, 30);      // 오물 속에 살아 독 저항은 다소 보유
			this.SetResistance(ResistanceType.Energy, 10, 20);

			// [Skills] ★ 스킬 200 서버 기준 - 갓 졸업한 유저용 수련 단계 (재설계)
			// 유저 스킬 30 ~ 50 구간에서 성장하기 최적화된 수치
			this.SetSkill(SkillName.Wrestling, 25.0, 35.0); 
			this.SetSkill(SkillName.Tactics, 25.0, 35.0);
			this.SetSkill(SkillName.Anatomy, 20.0, 30.0);
			this.SetSkill(SkillName.MagicResist, 20.0, 30.0);
			this.SetSkill(SkillName.Snooping, 50.0, 70.0); // 컨셉용 스킬

			// [Misc] 가상 방어력(Virtual Armor): (1,200/1000) - 2.2 = 0
			this.VirtualArmor = 0;

			this.Fame = 1200;
			this.Karma = -1200;

            AddItem(new Bow());
            PackItem(new Arrow(Utility.RandomMinMax(60, 80)));
            PackItem(new Apple(5));
        }

        public Gremlin(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.01)
                c.DropItem(new LuckyCoin());
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