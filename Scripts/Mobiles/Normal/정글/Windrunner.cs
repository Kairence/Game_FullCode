using System;
using Server.Gumps;

namespace Server.Mobiles
{
    public class WindrunnerStatue : Item, ICreatureStatuette
    {
        public override int LabelNumber { get { return 1124685; } } // Windrunner

        public Type CreatureType { get { return typeof(Windrunner); } }

        [Constructable]
        public WindrunnerStatue() 
            : base(0x9ED5)
        {
            LootType = LootType.Blessed;
        }
        public WindrunnerStatue(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
                from.SendGump(new ConfirmMountStatuetteGump(this));
            else
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
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

    [CorpseName("a Windrunner corpse")]
    public class Windrunner : BaseMount
    {
        [Constructable]
        public Windrunner()
            : this("Windrunner")
        {
        }

        [Constructable]
        public Windrunner(string name)
            : base(name, 1410, 16076, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0xA8;

			/* [Windrunner - Normal - Fame 19,000 / Weight 1.25]
			   - 정글 던전의 상급 조류 괴수 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 15 (명성/1000 보정 -4)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 1.3만 대)
			this.SetStr(570, 595); 
			this.SetHits(12800, 13100); 
			this.SetDex(240, 260); // 바람처럼 빠른 기동성과 회피
			this.SetInt(100, 120);

			// [Combat Options] 물리 20% / 냉기 40% / 에너지 40% (폭풍의 날갯짓)
			this.SetDamage(45, 75);
			this.SetAttackSpeed(1.5); // 극강의 공격 속도 (폭풍 연타)
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 20, 30);      // ★ 확실한 약점 (잘 타는 깃털)
			this.SetResistance(ResistanceType.Cold, 70, 75);    
			this.SetResistance(ResistanceType.Poison, 50, 65); 
			this.SetResistance(ResistanceType.Energy, 70, 75);  // 번개와 바람의 내성 (Max 75)

			// [Skills] 기본 115~125에 역산 보너스(19.4) 가산
			this.SetSkill(SkillName.Wrestling, 134.0, 144.0); 
			this.SetSkill(SkillName.Tactics, 134.0, 144.0);
			this.SetSkill(SkillName.Anatomy, 134.0, 144.0);
			this.SetSkill(SkillName.MagicResist, 125.0, 140.0);
			this.SetSkill(SkillName.Magery, 110.0, 125.0);       // 기후 변화 마법

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // 숙련도 시대 최고의 공속형 2슬롯 펫
			this.MinTameSkill = 156.2; // 전설급 테이머의 난이도
			this.VirtualArmor = 15;
			this.Fame = 19000;
			this.Karma = -19000;		
        }

        public Windrunner(Serial serial)
            : base(serial)
        {
        }

        public override int Meat { get { return 3; } }
        public override int Hides { get { return 10; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

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
