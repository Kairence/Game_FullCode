using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a terathan drone corpse")]
    public class TerathanDrone : BaseCreature
    {
        [Constructable]
        public TerathanDrone()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a terathan drone";
            this.Body = 71;
            this.BaseSoundID = 594;

			/* [Terathan Drone - Normal - Fame 2,000 / Weight 1.15]
			   - 테라탄 던전의 하급 일꾼 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 1 (명성/1000 - 1 보정)
			   - 특이사항: 낮은 체력이지만 무리 지어 등장
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(20, 28); 
			this.SetHits(520, 540); 
			this.SetDex(35, 45); 
			this.SetInt(35, 45);

			// [Combat Options] 물리 100% (날카로운 다리 찌르기)
			this.SetDamage(10, 18);
			this.SetAttackSpeed(2.0); 
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 20, 30); 
			this.SetResistance(ResistanceType.Fire, 5, 15);      // ★ 확실한 약점 (불에 취약)
			this.SetResistance(ResistanceType.Cold, 20, 30);    
			this.SetResistance(ResistanceType.Poison, 60, 75);  // 독성 저항 특화
			this.SetResistance(ResistanceType.Energy, 20, 30);   

			// [Skills] 기본 80~90에 역산 보너스(0.8) 가산
			// 최종 숙련도 약 85~95대의 하급 몬스터
			this.SetSkill(SkillName.Wrestling, 81.0, 91.0); 
			this.SetSkill(SkillName.Tactics, 81.0, 91.0);
			this.SetSkill(SkillName.Anatomy, 81.0, 91.0);
			this.SetSkill(SkillName.MagicResist, 60.0, 75.0);

			this.Tamable = false;
			this.VirtualArmor = 1;
			this.Fame = 2000;
			this.Karma = -2000;
			
            this.PackItem(new SpidersSilk(2));
        }

        public TerathanDrone(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 4;
            }
        }

        public override TribeType Tribe { get { return TribeType.Terathan; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.TerathansAndOphidians;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
            // TODO: weapon?
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
