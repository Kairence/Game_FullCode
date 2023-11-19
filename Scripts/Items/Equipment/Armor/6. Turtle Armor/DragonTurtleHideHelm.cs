using System;
using Server;

namespace Server.Items
{
    public class DragonTurtleHideHelm : BaseArmor
    {
        public override int InitMinHits { get { return 20; } }
        public override int InitMaxHits { get { return 35; } }

        public override int AosStrReq { get { return 40; } }
        public override int OldStrReq { get { return 10; } }

        public override int ArmorBase { get { return 6; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int LabelNumber { get { return 1109637; } } // Dragon Turtle Hide Helm

        [Constructable]
        public DragonTurtleHideHelm()
            : base(0x782D)
        {
            Weight = 2.0;
   			AbsorptionAttributes.ResonanceKinetic += 110;
        }

        public DragonTurtleHideHelm(Serial serial)
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