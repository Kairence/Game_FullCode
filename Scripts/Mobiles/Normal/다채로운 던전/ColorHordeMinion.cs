using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a color horde minion corpse")]
    public class ColorHordeMinion : BaseCreature
    {
        [Constructable]
        public ColorHordeMinion()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
           this.Name = "a color horde minion";
            this.Body = 999;
            this.BaseSoundID = 357;
			
			Boss = true;

			/* [Color Horde Minion - World Boss - Fame 20,000 / Weight 1.30]
			   - 컨셉: 모든 원소의 지배자 (무지개빛 파괴신)
			   - VirtualArmor: (20,000 / 1000) + 5 = 25 (마법적 외피 보정)
			   - 체력 5만 이상 룰 적용: 민맥 편차 2,000 이내 고정
			   -------------------------------------------------- */

			// 최종 Str 약 16,000 (공식 보너스 2,500 포함)
			this.SetStr(13500, 13800); 

			// 최종 Hits 약 360,000 (민맥 편차 2,000 고정)
			this.SetHits(304000, 306000); 

			// 최종 Dex/Int 약 3,200 (마법 위력 극대화)
			this.SetDex(2700, 2800);
			this.SetInt(2700, 2800);

			SetAttackSpeed(12.0);
			SetDamage(25, 40);

			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Fire, 20);
			this.SetDamageType(ResistanceType.Cold, 20);
			this.SetDamageType(ResistanceType.Poison, 20);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 65, 75);      
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 65, 75); 
			this.SetResistance(ResistanceType.Energy, 65, 75);

			// [Skills] 최종 약 216.5 (공식 보너스 83.3 포함)
			this.SetSkill(SkillName.Wrestling, 130.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 130.0, 135.0);
			this.SetSkill(SkillName.MagicResist, 150.0, 160.0);
			this.SetSkill(SkillName.Magery, 130.0, 135.0);
			this.SetSkill(SkillName.EvalInt, 130.0, 135.0);

			// [Taming] ★ 월드 보스 테이밍 불가
			this.Tamable = false;

			// 가상 방어력: (20,000/1000) + 5 = 25
			this.VirtualArmor = 25;

			this.Fame = 20000;
			this.Karma = -20000; 
            // TODO: Body parts
        }

        public ColorHordeMinion(Serial serial)
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