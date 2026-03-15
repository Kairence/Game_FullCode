using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a mimic corpse")]
    public class Mimic : BaseCreature
    {
        [Constructable]
        public Mimic()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a mimic";
            this.Body = 729;

			/* [Mimic - Normal - Fame 15,000 / Weight 1.25]
			   - 작은 숲 던전 기습형 몬스터 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 20 (기본 15 + 상자 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(410, 435); 
			this.SetHits(9200, 9500); 
			this.SetDex(80, 90);
			this.SetInt(80, 90);

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); // 단단한 외피
			this.SetResistance(ResistanceType.Fire, 20, 30);     // ★ 명확한 약점 (불타는 상자)
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 60, 75); 
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 110~120에 역산 보너스(14) 가산
			this.SetSkill(SkillName.Wrestling, 124.0, 134.0); 
			this.SetSkill(SkillName.Tactics, 124.0, 134.0);
			this.SetSkill(SkillName.Anatomy, 124.0, 134.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 130.0); // 상자의 독니

			this.Tamable = false;
			this.VirtualArmor = 20;
			this.Fame = 15000;
			this.Karma = -15000;

            this.PackReg(20);
        }

        public Mimic(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich, 4);
            this.AddLoot(LootPack.MedScrolls);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.03)            
                c.DropItem(new LuckyCoin());            
        }

        public override int GetIdleSound()
        {
            return 1561;
        }

        public override int GetAngerSound()
        {
            return 1558;
        }

        public override int GetHurtSound()
        {
            return 1560;
        }

        public override int GetDeathSound()
        {
            return 1559;
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