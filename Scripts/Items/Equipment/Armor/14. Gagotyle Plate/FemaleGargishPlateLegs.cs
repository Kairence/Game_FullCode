using System;

namespace Server.Items
{
    public class FemaleGargishPlateLegs : BaseArmor
    {
        [Constructable]
        public FemaleGargishPlateLegs()
            : this(0)
        {
        }

        [Constructable]
        public FemaleGargishPlateLegs(int hue)
            : base(0x30D)
        {
            Weight = 7.0;
            Hue = hue;
        }

        public FemaleGargishPlateLegs(Serial serial)
            : base(serial)
        {
        }

        public override int ArmorBase
        {
            get
            {
                return 14;
            }
        }
        public override int InitMinHits { get { return 50; } }
        public override int InitMaxHits { get { return 65; } }

        public override int AosStrReq { get { return 90; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Plate; } }

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