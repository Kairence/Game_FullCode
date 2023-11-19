namespace Server.Items
{
    class ClosedBarrel : TrapableContainer
    { 
        [Constructable]
        public ClosedBarrel()
            : base(0x0FAE)
        {
			Weight = 300;
        }

        public ClosedBarrel(Serial serial)
            : base(serial)
        {
        }
       public override int DefaultMaxItems
        {
            get
            {
				int items = 1000;
				if( Quality == ItemQuality.Exceptional )
				{
					items *= 125;
					items /= 100;
				}
                return items;
            }
        }
        public override int DefaultMaxWeight
        {
            get
            {
				int weight = 15000;
				if( IsSecure )
					weight *= 10;
				if( Quality == ItemQuality.Exceptional )
				{
					weight *= 120;
					weight /= 100;
				}
				return weight;
            }
        }

        public override int DefaultGumpID
        {
            get
            {
                return 0x3e;
            }
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