using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(FemaleGargishPlateChest))]
    [FlipableAttribute(0x1c04, 0x1c05)]
    public class FemalePlateChest : BaseArmor
    {
        [Constructable]
        public FemalePlateChest()
            : base(0x1C04)
        {
            Weight = 20.0;
        }

        public FemalePlateChest(Serial serial)
            : base(serial)
        {
        }
        public override int BasePhysicalResistance
        {
            get
            {
                return 11;
            }
        }

        public override int InitMinHits
        {
            get
            {
                return 50;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 65;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 115;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 45;
            }
        }
        public override int OldDexBonus
        {
            get
            {
                return -5;
            }
        }
        public override bool AllowMaleWearer
        {
            get
            {
                return false;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 15;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Plate;
            }
        }
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