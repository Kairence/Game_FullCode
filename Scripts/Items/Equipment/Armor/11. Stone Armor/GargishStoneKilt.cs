using System;

namespace Server.Items
{
    [TypeAlias("Server.Items.MaleGargishStoneKilt")]
    public class GargishStoneKilt : BaseArmor
    {
        [Constructable]
        public GargishStoneKilt()
            : this(0)
        {
        }

        [Constructable]
        public GargishStoneKilt(int hue)
            : base(0x288)
        {
            Weight = 10.0;
            Hue = hue;
        }

        public GargishStoneKilt(Serial serial)
            : base(serial)
        {
        }

        public override int ArmorBase
        {
            get
            {
                return 11;
            }
        }


        public override int InitMinHits { get { return 40; } }
        public override int InitMaxHits { get { return 50; } }

        public override int AosStrReq { get { return 40; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Stone; } }

        public override Race RequiredRace { get { return Race.Gargoyle; } }
        public override bool CanBeWornByGargoyles { get { return true; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}