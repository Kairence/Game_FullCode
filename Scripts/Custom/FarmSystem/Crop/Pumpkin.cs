using System; 

namespace Server.Items
{ 
    [FlipableAttribute(0xC6A, 0xC6B)]
    public class Pumpkin : Food
    {
        [Constructable]
        public Pumpkin()
            : this(1)
        {
        }

        [Constructable]
        public Pumpkin(int amount)
            : base(amount, 0xC6A)
        {
            this.Weight = 1.0;
            this.FillFactor = 8;
        }

        public Pumpkin(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1)
            {
                if (this.FillFactor == 4)
                    this.FillFactor = 8;

                if (this.Weight == 5.0)
                    this.Weight = 1.0;
            }
        }
    }
}