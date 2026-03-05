using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a troglodyte corpse")]
    public class Troglodyte : BaseCreature
    {
        public override double HealChance { get { return 1.0; } }

        [Constructable]
        public Troglodyte()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)// NEED TO CHECK
        {
            Name = "a troglodyte";
            Body = 267;
            BaseSoundID = 0x59F; 

			/* [Troglodyte - Fame 4,800 / Diverse / Weight 1.22]
			   - 스킬 200 마스터 서버용 '중상급 딜탱형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (4,800/1000) + 2.2 = 7
			   - 테이밍 불가능 (야만적인 지성체)
			   -------------------------------------------------- */

			// [Attributes] 명성 4,800 보너스 + 가중치 1.22 반영
			this.SetStr(80, 110); 
			this.SetHits(1800, 2200); 
			this.SetDex(15, 20);
			this.SetInt(15, 20);

			// [Combat Options] 무식한 돌도끼 타격
			this.SetDamage(20, 35);
			this.SetAttackSpeed(2.4);

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 거친 피부 (최대 저항 75% 캡 준수)
			this.SetResistance(ResistanceType.Physical, 40, 50); 
			this.SetResistance(ResistanceType.Fire, 20, 30);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 25, 35);

			// [Skills] 유저 스킬 80 ~ 110 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 85.0, 100.0); 
			this.SetSkill(SkillName.Tactics, 85.0, 100.0);
			this.SetSkill(SkillName.MagicResist, 65.0, 85.0);
			this.SetSkill(SkillName.Anatomy, 90.0, 110.0);    // 단순하지만 치명적인 공격

			// [Taming] ★ 테이밍 불가능
			this.Tamable = false;

			// [Misc]
			this.VirtualArmor = 7;

			this.Fame = 4800;
			this.Karma = -4800;
        }

        public Troglodyte(Serial serial)
            : base(serial)
        {
        }
		
		public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);  // Need to verify
        }
		
		public override void OnDeath( Container c )
        {
			base.OnDeath( c );
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