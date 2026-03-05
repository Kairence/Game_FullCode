using System;
using Server.Gumps;

namespace Server.Mobiles
{
    public class EowmuStatue : Item, ICreatureStatuette
    {
        public override int LabelNumber { get { return 1158082; } } // Eowmu

        public Type CreatureType { get { return typeof(Eowmu); } }

        [Constructable]
        public EowmuStatue() 
            : base(0xA0C0)
        {
            LootType = LootType.Blessed;
        }
        public EowmuStatue(Serial serial)
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

    [CorpseName("an eowmu corpse")]
    public class Eowmu : BaseMount
    {
        [Constructable]
        public Eowmu()
            : this("Eowmu")
        {
        }

        [Constructable]
        public Eowmu(string name)
            : base(name, 1440, 16079, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0xA8;

			/* [Eowmu - Normal - Fame 24,000 / Karma +24,000 / Weight 1.30]
			   - 정글 던전의 전설적 거대 공작 / 최상급 탈것
			   - 배수: 1x (Normal)
			   - VirtualArmor: 29 (기본 24 + 보정 5)
			   - 테이밍 가능: 5슬롯 (종결급 전투 탈것)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(950, 980); 
			this.SetHits(21400, 21700); 
			this.SetDex(190, 200);
			this.SetInt(190, 200);

			// [Combat Options] 물리 20% / 냉기 40% / 에너지 40% (신비로운 깃털 타격)
			this.SetDamage(60, 95);
			this.SetAttackSpeed(2.0); // 공작다운 우아하고 빠른 연타
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 35, 45);      // ★ 확실한 약점 (깃털 생물)
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 55, 65); 
			this.SetResistance(ResistanceType.Energy, 60, 70);   

			// [Skills] 기본 120~130에 역산 보너스(32.4) 가산
			this.SetSkill(SkillName.Wrestling, 152.0, 162.0); 
			this.SetSkill(SkillName.Tactics, 152.0, 162.0);
			this.SetSkill(SkillName.Anatomy, 152.0, 162.0);
			this.SetSkill(SkillName.MagicResist, 140.0, 155.0);
			this.SetSkill(SkillName.Magery, 130.0, 145.0);       // 신비로운 마법 구사

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 5; // 200 숙련도 시대의 최강 5슬롯 탈것
			this.MinTameSkill = 165.5; 
			this.VirtualArmor = 29;
			this.Fame = 24000;
			this.Karma = 24000; // 영물 (선 성향)
        }

        public Eowmu(Serial serial)
            : base(serial)
        {
        }

        public override int Meat { get { return 3; } }
        public override int Hides { get { return 10; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies; } }

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
