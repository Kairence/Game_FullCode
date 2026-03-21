using System;

namespace Server.Mobiles
{
    [CorpseName("a terathan avenger corpse")]
    public class TerathanAvenger : BaseCreature
    {
        [Constructable]
        public TerathanAvenger()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a terathan avenger";
            this.Body = 152;
            this.BaseSoundID = 0x24D;

			/* [Terathan Avenger - Normal - Fame 16,000 / Weight 1.28]
			   - 테라탄 던전의 정예 복수자 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 20 (명성/1000 + 4 보정)
			   - 특이사항: 높은 공격력과 체력, 치명적인 독 공격
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 약 1.1만 대)
			this.SetStr(510, 520); 
			this.SetHits(11400, 11500); 
			this.SetDex(100, 110); 
			this.SetInt(100, 110);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); 
			this.SetResistance(ResistanceType.Fire, 20, 35);     // ★ 확실한 약점 (불에 취약)
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 75, 75);  // 독성 저항 Max (완전 면역 수준)
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 115~125에 역산 보너스(17.2) 가산
			// 최종 숙련도 약 130~140대의 최정예 전사
			this.SetSkill(SkillName.Wrestling, 132.2, 142.2); 
			this.SetSkill(SkillName.Tactics, 132.2, 142.2);
			this.SetSkill(SkillName.Anatomy, 132.2, 142.2);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 140.0); // 치명적인 레벨의 독 주입

			this.Tamable = false;
			this.VirtualArmor = 20;
			this.Fame = 16000;
			this.Karma = -16000;
        }

        public TerathanAvenger(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
            }
        }
        public override int Meat
        {
            get
            {
                return 2;
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
            this.AddLoot(LootPack.Rich, 2);
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
