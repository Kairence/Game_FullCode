using System;

namespace Server.Items
{
    // Based off a HeaterShield
    [FlipableAttribute(0x4204, 0x4208)]
    public class LargePlateShield : BaseShield
    {
        [Constructable]
        public LargePlateShield()
            : base(0x4204)
        {

        }

        public LargePlateShield(Serial serial)
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
                return 200;
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