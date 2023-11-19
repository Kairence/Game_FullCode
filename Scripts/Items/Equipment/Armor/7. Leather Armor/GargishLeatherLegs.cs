using Server.Engines.Craft;
using System;

namespace Server.Items
{
    [TypeAlias("Server.Items.MaleGargishLeatherLegs")]
    public class GargishLeatherLegs : BaseArmor
    {
        [Constructable]
        public GargishLeatherLegs()
            : this(0)
        {
        }

        [Constructable]
        public GargishLeatherLegs(int hue)
            : base(0x305)
        {
            Weight = 5.0;
            Hue = hue;
        }

        public GargishLeatherLegs(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 30; } }
        public override int InitMaxHits { get { return 50; } }
        public override int ArmorBase { get { return 7; } }

        public override int AosStrReq { get { return 20; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override Race RequiredRace { get { return Race.Gargoyle; } }
        public override bool CanBeWornByGargoyles { get { return true; } }

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