using System;
using Server.Gumps;

namespace Server.Mobiles
{
    public class LasherStatue : Item, ICreatureStatuette
    {
        public override int LabelNumber { get { return 1157214; } } // Lasher

        public Type CreatureType { get { return typeof(Lasher); } }

        [Constructable]
        public LasherStatue() 
            : base(0x9E35)
        {
            LootType = LootType.Blessed;
        }
        public LasherStatue(Serial serial)
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

    [CorpseName("a Lasher corpse")]
    public class Lasher : BaseMount
    {
        [Constructable]
        public Lasher()
            : this("Lasher")
        {
        }

        [Constructable]
        public Lasher(string name)
            : base(name, 1407, 16075, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0xA8;

			/* [Lasher - Normal - Fame 25,000 / Karma +25,000 / Weight 1.30]
			   - 정글 던전의 전설적 유니콘 / 무지개 꼬리의 영물
			   - Taming 200 시대를 반영한 2슬롯 종결급 탈것
			   - VirtualArmor: 30 (명성/1000 + 5 보정)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 2.2만 대)
			this.SetStr(1020, 1050); 
			this.SetHits(22700, 23000); 
			this.SetDex(200, 220); 
			this.SetInt(200, 220);

			// [Combat Options] 물리 20% / 에너지 80% (무지개빛 마력 타격)
			this.SetDamage(65, 95);
			this.SetAttackSpeed(1.7); // 유니콘 계열 특유의 극강 공속
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80);

			// [Resistances] 최고 저항 75 이하 엄격 준수 / 독 약점 유지
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 60, 70);      
			this.SetResistance(ResistanceType.Cold, 60, 70);    
			this.SetResistance(ResistanceType.Poison, 35, 45);   // ★ 확실한 약점
			this.SetResistance(ResistanceType.Energy, 75, 75);  // 마력의 화신 (Max 75)

			// [Skills] 기본 125~135에 역산 보너스(34.4) 가산
			this.SetSkill(SkillName.Wrestling, 159.0, 169.0); 
			this.SetSkill(SkillName.Tactics, 159.0, 169.0);
			this.SetSkill(SkillName.Anatomy, 159.0, 169.0);
			this.SetSkill(SkillName.Magery, 150.0, 165.0);       
			this.SetSkill(SkillName.EvalInt, 150.0, 165.0);
			this.SetSkill(SkillName.MagicResist, 150.0, 165.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // ★ 200 시대 최고의 2슬롯 가성비/성능 탈것
			this.MinTameSkill = 175.5; // 전설의 테이머만 가능한 극악의 난이도
			this.VirtualArmor = 30;
			this.Fame = 25000;
			this.Karma = 25000;
		}

        public Lasher(Serial serial)
            : base(serial)
        {
        }

        public override int Meat { get { return 3; } }
        public override int Hides { get { return 10; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

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
