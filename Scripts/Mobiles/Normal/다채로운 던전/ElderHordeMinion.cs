using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an elder horde minion corpse")]
    public class ElderHordeMinion : BaseCreature
    {
        [Constructable]
        public ElderHordeMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an elder horde minion";
            this.Body = 796;
            this.BaseSoundID = 357;

			/* [Elder Horde Minion - Fame 7,000 / Diverse / Weight 1.23]
			   - 스킬 200 마스터 서버용 '상급 기술형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (7,000/1000) + 3 = 10
			   - 테이밍 불가능 (교활한 우두머리)
			   -------------------------------------------------- */

			// [Attributes] 명성 7,000 보너스 + 가중치 1.23 반영
			this.SetStr(130, 160); 
			this.SetHits(2800, 3500); 
			this.SetDex(25, 35);
			this.SetInt(25, 35);

			// [Combat Options] 노련한 급소 가격
			this.SetDamage(28, 45);
			this.SetAttackSpeed(2.0); // 빠른 속도 유지

			// [Damage Types] 80% 물리 + 20% 에너지 (번개 인챈트 무기 컨셉)
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 산전수전 겪은 저항력 (최대 저항 75% 캡 준수)
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 45, 55);

			// [Skills] 유저 스킬 100 ~ 130 구간 (GM 이상의 벽)
			this.SetSkill(SkillName.Wrestling, 95.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 95.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 130.0);    // 높은 치명타율
			this.SetSkill(SkillName.MagicResist, 90.0, 110.0);
			this.SetSkill(SkillName.Fencing, 100.0, 120.0);    // 날카로운 찌르기

			// [Taming] ★ 테이밍 불가능
			this.Tamable = false;

			// [Misc]
			this.VirtualArmor = 10;

			this.Fame = 7000;
			this.Karma = -7000;
        }

        public ElderHordeMinion(Serial serial)
            : base(serial)
        {
        }

        public override int GetIdleSound()
        {
            return 338;
        }

        public override int GetAngerSound()
        {
            return 338;
        }

        public override int GetDeathSound()
        {
            return 338;
        }

        public override int GetAttackSound()
        {
            return 406;
        }

        public override int GetHurtSound()
        {
            return 194;
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