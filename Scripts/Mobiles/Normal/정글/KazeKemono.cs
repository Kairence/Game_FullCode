using System;
using System.Collections;

namespace Server.Mobiles
{
    [CorpseName("a kaze kemono corpse")]
    public class KazeKemono : BaseCreature
    {
        private static readonly Hashtable m_FlurryOfTwigsTable = new Hashtable();
        private static readonly Hashtable m_ChlorophylBlastTable = new Hashtable();

        [Constructable]
        public KazeKemono()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a kaze kemono";
            Body = 196;
            BaseSoundID = 655;

			/* [Kaze Kemono - Normal - Fame 16,000 / Weight 1.25]
			   - 정글 던전의 바람 정령 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 10 (명성/1000 보정 -6)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(450, 470); 
			this.SetHits(10000, 10400); 
			this.SetDex(220, 250); // 바람처럼 빠른 민첩성
			this.SetInt(100, 120);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Cold, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] 최고 저항 75 이하 준수 / 물리 약점 설정
			this.SetResistance(ResistanceType.Physical, 25, 35); // ★ 확실한 약점 (실체가 약함)
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 70, 75);  // 에너지 내성 특화

			// [Skills] 기본 110~120에 역산 보너스(15.3) 가산
			this.SetSkill(SkillName.Wrestling, 125.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 125.0, 135.0);
			this.SetSkill(SkillName.Magery, 115.0, 130.0);       // 바람의 마법
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 3; 
			this.MinTameSkill = 145.5; // 200 숙련도 시대의 중상급 펫
			this.VirtualArmor = 10;
			this.Fame = 16000;
			this.Karma = -16000;

            SetSpecialAbility(SpecialAbility.ConductiveBlast);
            SetSpecialAbility(SpecialAbility.FlurryForce);
        }

        public KazeKemono(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 3);
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
