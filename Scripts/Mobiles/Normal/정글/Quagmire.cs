using System;

namespace Server.Mobiles
{
    [CorpseName("a quagmire corpse")]
    public class Quagmire : BaseCreature
    {
        [Constructable]
        public Quagmire()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.4, 0.8)
        {
            this.Name = "a quagmire";
            this.Body = 789;
            this.BaseSoundID = 352;

			/* [Quagmire - Normal - Fame 15,000 / Weight 1.25]
			   - 정글 던전의 독성 진흙 정령 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 15 (명성/1000 공식 준수)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 9,000대)
			this.SetStr(415, 430); 
			this.SetHits(9200, 9500); 
			this.SetDex(80, 90);
			this.SetInt(80, 90);

			// [Combat Options] 물리 50% / 독 50% (오염된 진흙 타격)
			this.SetDamage(35, 60);
			this.SetAttackSpeed(2.8); // 끈적이는 몸체로 인한 다소 느린 공격
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Poison, 50);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); // 타격이 흡수되는 몸체
			this.SetResistance(ResistanceType.Fire, 15, 25);      // ★ 확실한 약점 (말라붙음)
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 독성 그 자체 (Max 75)
			this.SetResistance(ResistanceType.Energy, 35, 45);   

			// [Skills] 기본 110~120에 역산 보너스(14) 가산
			this.SetSkill(SkillName.Wrestling, 124.0, 134.0); 
			this.SetSkill(SkillName.Tactics, 124.0, 134.0);
			this.SetSkill(SkillName.Anatomy, 124.0, 134.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Poisoning, 125.0, 140.0); // 치명적인 정글의 역병 독

			this.Tamable = false;
			this.VirtualArmor = 15;
			this.Fame = 15000;
			this.Karma = -15000;
        }

        public Quagmire(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override double HitPoisonChance
        {
            get
            {
                return 0.1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
        }

        public override int GetAngerSound()
        {
            return 353;
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