using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a pixie corpse")]
    public class Pixie : BaseCreature
    {
        [Constructable]
        public Pixie()
            : base(AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("pixie");
            Body = 128;
            BaseSoundID = 0x467;

			/* [Pixie - Normal - Fame 7,000 / Karma +7,000 / Weight 1.15]
			   - 작은 숲 던전 상급 정령 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 0 (가방 방어력 없음, 명성/1000 - 7 보정)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(90, 105); 
			this.SetHits(2100, 2250); 
			this.SetDex(15, 25);
			this.SetInt(15, 25);

			// [Combat Options] 100% 에너지 대미지 (순수 마력 공격)
			this.SetDamage(15, 30);
			this.SetAttackSpeed(1.8); // 상급 정령다운 매우 빠른 기동성
			this.SetDamageType(ResistanceType.Energy, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 15, 25); 
			this.SetResistance(ResistanceType.Fire, 10, 20);      // ★ 명확한 약점
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 65, 75);   // 요정의 정화 결계
			this.SetResistance(ResistanceType.Energy, 60, 70);   

			// [Skills] 기본 95~105에 역산 보너스(3.2) 가산
			this.SetSkill(SkillName.Wrestling, 98.0, 108.0); 
			this.SetSkill(SkillName.Tactics, 98.0, 108.0);
			this.SetSkill(SkillName.Magery, 105.0, 120.0);       // 상급 요정의 강력한 마법
			this.SetSkill(SkillName.EvalInt, 105.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);

			this.Tamable = false;
			this.VirtualArmor = 0;
			this.Fame = 7000;
			this.Karma = 7000; // 선 성향 수치 상향

            VirtualArmor = 100;
            if (0.02 > Utility.RandomDouble())
                PackStatue();
        }

        public Pixie(Serial serial)
            : base(serial)
        {
        }

        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override int Hides
        {
            get
            {
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Gems, 2);
        }

		public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.3)
                c.DropItem(new PixieLeg());
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
