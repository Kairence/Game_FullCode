using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(MediumPlateShield))]
    public class MetalShield : BaseShield
    {
        [Constructable]
        public MetalShield()
            : base(0x1B7B)
        {
            this.Weight = 6.0;
        }

        public MetalShield(Serial serial)
            : base(serial)
        {
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
                return 45;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 25;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 20;
            }
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);//version
        }
    }
}