using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a large horde minion corpse")]
    public class LargeHordeMinion : BaseCreature
    {
        [Constructable]
        public LargeHordeMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a large horde minion";
            this.Body = 776;
            this.BaseSoundID = 357;

			/* [Large Horde Minion - Fame 4,000 / Diverse / Weight 1.20]
			   - 스킬 200 마스터 서버용 '중급 맷집형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (4,000/1000) + 2 = 6
			   - 테이밍 불가능 (강화된 약탈자)
			   -------------------------------------------------- */

			// [Attributes] 명성 4,000 보너스 + 가중치 1.20 반영
			this.SetStr(60, 80); 
			this.SetHits(1300, 1600); 
			this.SetDex(10, 15);
			this.SetInt(10, 15);

			// [Combat Options] 묵직한 몽둥이질
			this.SetDamage(18, 32);
			this.SetAttackSpeed(2.2); // 일반 미니언보다 느리지만 강력함

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 강화된 무장 (최대 저항 75% 캡 준수)
			this.SetResistance(ResistanceType.Physical, 35, 45); 
			this.SetResistance(ResistanceType.Fire, 25, 35);      
			this.SetResistance(ResistanceType.Cold, 25, 35);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 35, 45);

			// [Skills] 유저 스킬 70 ~ 100 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 75.0, 95.0); 
			this.SetSkill(SkillName.Tactics, 75.0, 95.0);
			this.SetSkill(SkillName.MagicResist, 60.0, 80.0);
			this.SetSkill(SkillName.Macing, 80.0, 100.0); // 둔기 타격으로 스태미나 갉아먹기

			// [Taming] ★ 테이밍 불가능
			this.Tamable = false;

			// [Misc]
			this.VirtualArmor = 6;

			this.Fame = 4000;
			this.Karma = -4000;
        }

        public LargeHordeMinion(Serial serial)
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