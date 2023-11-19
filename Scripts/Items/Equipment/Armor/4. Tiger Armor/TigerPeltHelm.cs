using System;
using Server;

namespace Server.Items
{
    public class TigerPeltHelm : BaseArmor
    {
        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 40; } }

        public override int AosStrReq { get { return 30; } }
        public override int OldStrReq { get { return 15; } }

        public override int ArmorBase { get { return 4; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int LabelNumber { get { return 1109632; } } // Tiger Pelt Helm

        [Constructable]
        public TigerPeltHelm()
            : base(0x7828)
        {
            Weight = 2.0;
   			AbsorptionAttributes.ResonancePierce += 120;
      }

        public TigerPeltHelm(Serial serial)
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