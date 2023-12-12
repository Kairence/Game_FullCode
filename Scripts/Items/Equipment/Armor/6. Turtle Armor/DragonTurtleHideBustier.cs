using System;
using Server.Items;

namespace Server.Items
{
    public class DragonTurtleHideBustier : BaseArmor
    {
        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 700; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }

        public override int ArmorBase { get { return 6; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        // We like to cross dress here!
        public override bool AllowMaleWearer { get { return true; } }

        public override int LabelNumber { get { return 1109635; } } // Dragon Turtle Hide Bustier

        [Constructable]
        public DragonTurtleHideBustier()
            : base(0x782B)
        {
            Weight = 6.0;
			PrefixOption[50] = 3;
			PrefixOption[61] = 12;
			SuffixOption[61] = 700;
			PrefixOption[62] = 18;
			SuffixOption[62] = 500;
        }

        public DragonTurtleHideBustier(Serial serial)
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