using System;

namespace Server.Items
{
    public class FemaleGargishStoneKilt : BaseArmor
    {
        [Constructable]
        public FemaleGargishStoneKilt()
            : this(0)
        {
        }

        [Constructable]
        public FemaleGargishStoneKilt(int hue)
            : base(0x287)
        {
            Weight = 10.0;
            Hue = hue;
        }

        public FemaleGargishStoneKilt(Serial serial)
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