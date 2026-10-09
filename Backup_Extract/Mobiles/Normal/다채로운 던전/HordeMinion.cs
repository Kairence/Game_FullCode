using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a horde minion corpse")]
    public class HordeMinion : BaseCreature
    {
        [Constructable]
        public HordeMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a horde minion";
            this.Body = 776;
            this.BaseSoundID = 357;

			/* [Horde Minion - Fame 2,500 / Diverse / Weight 1.17]
			   - 스킬 200 마스터 서버용 '저급 무리형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (2,500/1000) + 1.5 = 4
			   - 테이밍 불가능 (탐욕스러운 인형 컨셉)
			   -------------------------------------------------- */

			// [Attributes] 명성 2,500 보너스 + 가중치 1.17 반영
			this.SetStr(30, 45); 
			this.SetHits(700, 900); 
			this.SetDex(6, 9);
			this.SetInt(6, 9);

			// [Combat Options] 조잡한 단검 연타
			this.SetDamage(10, 18);
			this.SetAttackSpeed(1.8); // 덩치에 걸맞게 매우 빠름

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 조잡한 무장 (최대 저항 75% 캡 준수)
			this.SetResistance(ResistanceType.Physical, 25, 35); 
			this.SetResistance(ResistanceType.Fire, 20, 30);      
			this.SetResistance(ResistanceType.Cold, 20, 30);    
			this.SetResistance(ResistanceType.Poison, 30, 40); 
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 유저 스킬 60 ~ 80 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 60.0, 75.0); 
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);
			this.SetSkill(SkillName.MagicResist, 50.0, 65.0);
			this.SetSkill(SkillName.Stealing, 80.0, 100.0); // 유저의 물건을 훔칠 수 있는 컨셉

			// [Taming] ★ 테이밍 불가능
			this.Tamable = false;

			// [Misc]
			this.VirtualArmor = 4;

			this.Fame = 2500;
			this.Karma = -2500;

            // TODO: Body parts
        }

        public HordeMinion(Serial serial)
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
