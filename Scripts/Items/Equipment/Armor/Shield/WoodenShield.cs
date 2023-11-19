using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishWoodenShield))]
    public class WoodenShield : BaseShield
    {
        [Constructable]
        public WoodenShield()
            : base(0x1B7A)
        {
            this.Weight = 5.0;
        }

        public WoodenShield(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 20;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 25;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 10;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 30;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 17;
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