using System;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a ratman's corpse")]
    public class Ratman : BaseCreature
    {
        [Constructable]
        public Ratman()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("ratman");
            this.Body = 42;
            this.BaseSoundID = 437;

			/* [Ratman - Normal - Fame 3,500 / Weight 1.15]
			   - 정글 던전의 하급 약탈자 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 4 (명성/1000 + 1 보정)
			   - 특이사항: 낮은 체력이지만 빠른 공속과 회피율 보유
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(40, 50); 
			this.SetHits(950, 1000); 
			this.SetDex(80, 100); // 랫맨 특유의 재빠른 움직임
			this.SetInt(50, 65);

			// [Combat Options] 물리 100% (조잡한 단검 연타)
			this.SetDamage(15, 25);
			this.SetAttackSpeed(1.6); // 소형 몬스터다운 매우 빠른 공격
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 25, 35); 
			this.SetResistance(ResistanceType.Fire, 30, 40);      
			this.SetResistance(ResistanceType.Cold, 30, 40);    
			this.SetResistance(ResistanceType.Poison, 55, 65);  // 불결한 환경 내성
			this.SetResistance(ResistanceType.Energy, 10, 20);  // ★ 확실한 약점 (전격에 마비됨)

			// [Skills] 기본 85~95에 역산 보너스(1.4) 가산
			// 최종 숙력도 약 90~100대의 하급 몬스터
			this.SetSkill(SkillName.Wrestling, 86.0, 96.0); 
			this.SetSkill(SkillName.Tactics, 86.0, 96.0);
			this.SetSkill(SkillName.Anatomy, 86.0, 96.0);
			this.SetSkill(SkillName.MagicResist, 70.0, 85.0);

			this.Tamable = false;
			this.VirtualArmor = 4;
			this.Fame = 3500;
			this.Karma = -3500;
        }

        public Ratman(Serial serial)
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
            this.AddLoot(LootPack.Meager);
            // TODO: weapon, misc
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
