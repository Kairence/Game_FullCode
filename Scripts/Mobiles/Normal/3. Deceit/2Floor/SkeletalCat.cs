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

            SetStr(400, 450);
            SetDex(200, 250);
            SetInt(150, 200);

            SetHits(2700, 2900);
			SetStam(250, 350);
            SetMana(100, 150);

			SetAttackSpeed( 2.5 );

            SetDamage(7, 23);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 10, 20);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 40, 50);
            SetResistance(ResistanceType.Poison, 90, 110);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.MagicResist, 85.0, 90.0);
            SetSkill(SkillName.Tactics, 90.0, 100.0);
            SetSkill(SkillName.Wrestling, 90.0, 95.0);

            Fame = 9000;
            Karma = -9000;

            VirtualArmor = 10;
            this.Tamable = true;
            this.ControlSlots = 3;
            this.MinTameSkill = 235.1;
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
