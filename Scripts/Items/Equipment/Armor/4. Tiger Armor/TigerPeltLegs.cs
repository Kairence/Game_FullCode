using System;
using Server.Items;

namespace Server.Items
{
    public class TigerPeltLegs : BaseArmor
	{
        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }

        public override int ArmorBase { get { return 4; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override bool AllowMaleWearer { get { return true; } }

        public override int LabelNumber { get { return 1109628; } } // Tiger Pelt Leggins

        [Constructable]
        public TigerPeltLegs()
            : base(0x7824)
        {
            Weight = 2.0;
			PrefixOption[50] = 2;
			PrefixOption[61] = 12;
			SuffixOption[61] = 300;
			PrefixOption[62] = 40;
			SuffixOption[62] = 250;
       }

        public TigerPeltLegs(Serial serial)
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