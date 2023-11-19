using System;

namespace Server.Items
{
    // Based off a MetalKiteShield
    [FlipableAttribute(0x4201, 0x4206)]
    public class GargishKiteShield : BaseShield, IDyable
    {
        [Constructable]
        public GargishKiteShield()
            : base(0x4201)
        {
            //Weight = 7.0;
        }

        public GargishKiteShield(Serial serial)
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
                return 150;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 0;
            }
        }
        public override bool CanBeWornByGargoyles
        {
            get
            {
                return true;
            }
        }
        public override Race RequiredRace
        {
            get
            {
                return Race.Gargoyle;
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