using System;

namespace Server.Mobiles
{
    [CorpseName("a terathan warrior corpse")]
    public class TerathanWarrior : BaseCreature
    {
        [Constructable]
        public TerathanWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a terathan warrior";
            this.Body = 70;
            this.BaseSoundID = 589;

			/* [Terathan Warrior - Normal - Fame 8,500 / Weight 1.20]
			   - 테라탄 던전의 주력 전투병 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 9 (명성/1000 + 1 보정)
			   - 특이사항: 중급 수준의 체력과 강력한 독 공격
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(160, 170); 
			this.SetHits(3600, 3700); 
			this.SetDex(30, 40); 
			this.SetInt(30, 40);

			// [Combat Options] 물리 80% / 독 20% (독이 발린 창)
			this.SetDamage(25, 45);
			this.SetAttackSpeed(2.3); 
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Poison, 20);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 40, 55); 
			this.SetResistance(ResistanceType.Fire, 10, 25);     // ★ 확실한 약점 (불에 취약)
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 독성 저항 Max 근접
			this.SetResistance(ResistanceType.Energy, 30, 40);   

			// [Skills] 기본 100~110에 역산 보너스(5.5) 가산
			// 최종 숙련도 약 105~115대의 중급 전사
			this.SetSkill(SkillName.Wrestling, 105.5, 115.5); 
			this.SetSkill(SkillName.Tactics, 105.5, 115.5);
			this.SetSkill(SkillName.Anatomy, 105.5, 115.5);
			this.SetSkill(SkillName.MagicResist, 85.0, 100.0);
			this.SetSkill(SkillName.Poisoning, 90.0, 110.0); // 상대를 중독시키는 능력

			this.Tamable = false;
			this.VirtualArmor = 9;
			this.Fame = 8500;
			this.Karma = -8500;

            if (Core.ML && Utility.RandomDouble() < .33)
                this.PackItem(Engines.Plants.Seed.RandomPeculiarSeed(4));
        }

        public TerathanWarrior(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
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
            this.AddLoot(LootPack.Average);
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
