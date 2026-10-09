using System;
using Server.Gumps;

namespace Server.Mobiles
{
    public class SkeletalCatStatue : Item, ICreatureStatuette
    {
        public override int LabelNumber { get { return 1158462; } } // Skeletal Cat

        public Type CreatureType { get { return typeof(SkeletalCat); } }

        [Constructable]
        public SkeletalCatStatue() 
            : base(0xA138)
        {
            LootType = LootType.Blessed;
        }
        public SkeletalCatStatue(Serial serial)
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

    [CorpseName("a Skeletal Cat corpse")]
    public class SkeletalCat : BaseMount
    {
        [Constructable]
        public SkeletalCat()
            : this("Skeletal Cat")
        {
        }

        [Constructable]
        public SkeletalCat(string name)
            : base(name, 1441, 16080, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 229;

            /* Skeletal Cat - Fame 5,000 */
			this.Fame = 5000;
			this.Karma = -5000;

			this.SetDex(300, 400);    // 매우 빠른 움직임
			this.SetHits(500, 800);   // 최종 Hits 약 15,000
			this.SetStam(500, 800);

			SetAttackSpeed(1.5);
			SetDamage(15, 25);

			this.SetSkill(SkillName.Wrestling, 140.0, 155.0); // 작아서 맞추기 힘듦 설정
			this.SetSkill(SkillName.Tactics, 100.0, 110.0);

			this.SetDamageType(ResistanceType.Physical, 100);
			this.SetResistance(ResistanceType.Physical, 20, 30);
			this.VirtualArmor = 5;
			
			// 테이밍 관련 설정
			this.Tamable = true; 
			this.ControlSlots = 1;      // 1슬롯으로 효율 극대화 (최대 5마리 운용 가능)
			this.MinTameSkill = 95.1;			
			
        }

        public SkeletalCat(Serial serial)
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
