using System;

namespace Server.Items
{
    // Based off a WoodenKiteShield
    [FlipableAttribute(0x4205, 0x420B)]
    public class LargeStoneShield : BaseShield
    {
        [Constructable]
        public LargeStoneShield()
            : base(0x4205)
        {
            //Weight = 5.0;
        }

        public LargeStoneShield(Serial serial)
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
                return 70;
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