using System;

namespace Server.Mobiles
{
    [CorpseName("a silver steed corpse")]
    public class SilverSteed : BaseMount
    {
        [Constructable]
        public SilverSteed()
            : this("a silver steed")
        {
        }

        [Constructable]
        public SilverSteed(string name)
            : base(name, 0x75, 0x3EA8, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
			/* [Silver Steed - Normal - Fame 23,000 / Karma +23,000 / Weight 1.25]
			   - 정글 던전의 전설적 은빛 준마 / 최상급 탈것
			   - 배수: 1x (Normal)
			   - VirtualArmor: 25 (명성/1000 + 2 보정)
			   - 테이밍 가능: 2슬롯 (범용성 높은 종결급 펫)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 1.6만 대)
			this.SetStr(750, 775); 
			this.SetHits(16700, 17000); 
			this.SetDex(150, 165); // 뛰어난 기동성과 회피
			this.SetInt(150, 165);

			// [Combat Options] 물리 50% / 냉기 50% (은빛 냉기 타격)
			this.SetDamage(55, 85);
			this.SetAttackSpeed(1.9); // 상급 영물다운 빠른 타격
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			// [Resistances] 최고 저항 75 이하 준수 / 독 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); 
			this.SetResistance(ResistanceType.Fire, 60, 70);      
			this.SetResistance(ResistanceType.Cold, 70, 75);    // 냉기 내성 특화
			this.SetResistance(ResistanceType.Poison, 35, 45);   // ★ 확실한 약점 (오염에 취약)
			this.SetResistance(ResistanceType.Energy, 60, 70);   

			// [Skills] 기본 120~130에 역산 보너스(25.4) 가산
			this.SetSkill(SkillName.Wrestling, 145.0, 155.0); 
			this.SetSkill(SkillName.Tactics, 145.0, 155.0);
			this.SetSkill(SkillName.Anatomy, 145.0, 155.0);
			this.SetSkill(SkillName.MagicResist, 135.0, 150.0);
			this.SetSkill(SkillName.Magery, 130.0, 145.0);       // 은빛 마력의 보호

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // 숙련도 시대 최고의 범용 2슬롯 탈것
			this.MinTameSkill = 162.8; // 200 시대의 상징적 난이도 반영
			this.VirtualArmor = 25;
			this.Fame = 23000;
			this.Karma = 23000; // 영물 (선 성향)
        }

        public SilverSteed(Serial serial)
            : base(serial)
        {
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