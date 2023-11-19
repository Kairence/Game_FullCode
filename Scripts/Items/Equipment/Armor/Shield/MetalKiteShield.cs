using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishKiteShield))]
    public class MetalKiteShield : BaseShield, IDyable
    {
        [Constructable]
        public MetalKiteShield()
            : base(0x1B74)
        {
            Weight = 7.0;
        }

        public MetalKiteShield(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 45;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 60;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 50;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 40;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 21;
            }
        }
        public bool Dye(Mobile from, DyeTub sender)
        {
            if (this.Deleted)
                return false;

            this.Hue = sender.DyedHue;

            return true;
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