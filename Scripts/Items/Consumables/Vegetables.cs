using System;

namespace Server.Items
{
    [FlipableAttribute(0xC6A, 0xC6B)]
    public class SmallPumpkin : Food
    {
        [Constructable]
        public SmallPumpkin()
            : this(1)
        {
        }

        [Constructable]
        public SmallPumpkin(int amount)
            : base(amount, 0xC6C)
        {
            this.Weight = 2.4;
            this.FillFactor = 8;
        }

        public SmallPumpkin(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}