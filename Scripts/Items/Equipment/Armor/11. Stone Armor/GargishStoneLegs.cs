using System;

namespace Server.Items
{
    [TypeAlias("Server.Items.MaleGargishStoneLegs")]
    public class GargishStoneLegs : BaseArmor
    {
        [Constructable]
        public GargishStoneLegs()
            : this(0)
        {
        }

        [Constructable]
        public GargishStoneLegs(int hue)
            : base(0x28A)
        {
            Weight = 15.0;
            Hue = hue;
        }

        public GargishStoneLegs(Serial serial)
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