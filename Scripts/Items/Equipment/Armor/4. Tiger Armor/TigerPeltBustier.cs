using System;
using Server.Items;

namespace Server.Items
{
    public class TigerPeltBustier : BaseArmor
    {
        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 40; } }

        public override int AosStrReq { get { return 60; } }
        public override int OldStrReq { get { return 15; } }

        public override int ArmorBase { get { return 4; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }
        public override bool AllowMaleWearer { get { return true; } }

        public override int LabelNumber { get { return 1109627; } } // Tiger Pelt Bustier

        [Constructable]
        public TigerPeltBustier()
            : base(0x7823)
        {
            Weight = 6.0;
			AbsorptionAttributes.ResonancePierce += 140;
        }

        public TigerPeltBustier(Serial serial)
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