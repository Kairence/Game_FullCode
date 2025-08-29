using System;
using Server.Items;

namespace Server.Items
{
    public class DragonTurtleHideLegs : BaseArmor
    {
        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 600; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }

        public override int ArmorBase { get { return 12; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int LabelNumber { get { return 1109636; } } // Dragon Turtle Hide Leggings

        [Constructable]
        public DragonTurtleHideLegs()
            : base(0x782C)
        {
            Weight = 20.0;
			PrefixOption[50] = 3;
			PrefixOption[61] = 12;
			SuffixOption[61] = 70000;
			PrefixOption[62] = 18;
			SuffixOption[62] = 50000;
			PrefixOption[63] = 100;
			SuffixOption[63] = 250000;

        }

        public DragonTurtleHideLegs(Serial serial)
            : base(serial)
        {
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
}