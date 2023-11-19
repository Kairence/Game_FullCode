using System;
using Server.Items;

namespace Server.Items
{
    public class DragonTurtleHideChest : BaseArmor
    {

        public override int InitMinHits { get { return 35; } }
        public override int InitMaxHits { get { return 45; } }

        public override int AosStrReq { get { return 70; } }
        public override int OldStrReq { get { return 35; } }

        public override int ArmorBase { get { return 6; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int LabelNumber { get { return 1109634; } } // Dragon Turtle Hide Chest

        [Constructable]
        public DragonTurtleHideChest()
            : base(0x782A)
        {
            Weight = 8.0;
   			AbsorptionAttributes.ResonanceKinetic += 140;
        }

        public DragonTurtleHideChest(Serial serial)
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