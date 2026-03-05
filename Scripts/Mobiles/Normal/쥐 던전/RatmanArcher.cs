using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a ratman archer corpse")]
    public class RatmanArcher : BaseCreature
    {
        [Constructable]
        public RatmanArcher()
            : base(AIType.AI_Archer, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("ratman");
            this.Body = 0x8E;
            this.BaseSoundID = 437;

			/* [Ratman Archer - Normal - Fame 4,000 / Weight 1.18]
			   - 정글 던전의 원거리 저격수 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 4 (명성/1000 공식 준수)
			   - 특이사항: 높은 Archery 스킬과 독 화살 공격
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(60, 65); 
			this.SetHits(1350, 1400); 
			this.SetDex(140, 160); // 궁수다운 빠른 사격 속도
			this.SetInt(60, 75);

			// [Combat Options] 물리 100% (조잡한 화살 사격)
			this.SetDamage(20, 35);
			this.SetAttackSpeed(2.8); // 활의 기본 속도 반영
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 35, 45); 
			this.SetResistance(ResistanceType.Fire, 30, 45);      
			this.SetResistance(ResistanceType.Cold, 30, 45);    
			this.SetResistance(ResistanceType.Poison, 60, 70); 
			this.SetResistance(ResistanceType.Energy, 10, 20);  // ★ 확실한 약점 (전격에 취약)

			// [Skills] 기본 95~105에 역산 보너스(2.0) 가산
			// 최종 숙련도 약 100~110대의 상급 정찰병
			this.SetSkill(SkillName.Wrestling, 97.0, 107.0); 
			this.SetSkill(SkillName.Tactics, 97.0, 107.0);
			this.SetSkill(SkillName.Anatomy, 97.0, 107.0);
			this.SetSkill(SkillName.Archery, 105.0, 115.0); // 핵심 공격 스킬
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);

			this.Tamable = false;
			this.VirtualArmor = 4;
			this.Fame = 4000;
			this.Karma = -4000;

            this.AddItem(new Bow());
            //this.PackItem(new Arrow(Utility.RandomMinMax(50, 70)));
        }

        public RatmanArcher(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Ratman;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int Hides
        {
            get
            {
                return 8;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
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