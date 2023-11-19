using System;

namespace Server.Items
{
    public class WoodenKiteShield : BaseShield
    {
        [Constructable]
        public WoodenKiteShield()
            : base(0x1B78)
        {
            Weight = 5.0;
        }

        public WoodenKiteShield(Serial serial)
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
                return 30;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 50;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 18;
            }
        }
		
		public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);//version
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }       
    }
}