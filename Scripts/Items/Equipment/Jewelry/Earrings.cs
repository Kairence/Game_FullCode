using System;

namespace Server.Items
{
    public abstract class BaseEarrings : BaseJewel
    {
		public override int BaseGemTypeNumber { get { return 1044203; } }// star sapphire earrings
		
        public BaseEarrings(int itemID)
            : base(itemID, Layer.Earrings)
        {

        }

        public BaseEarrings(Serial serial)
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

    public class GoldEarrings : BaseEarrings
    {
        [Constructable]
        public GoldEarrings()
            : base(0x1087)
        {
            Weight = 0.5;
			PrefixOption[50] = 19;
			PrefixOption[61] = 42;
			SuffixOption[61] = 75000;
			PrefixOption[62] = 44;
			SuffixOption[62] = 150000;
        }
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
		public override int AosDexterityReq { get { return 100; } }
		public override int AosIntelligenceReq { get { return 2500; } }
        public override int LabelNumber { get { return 1138017; } }
        public GoldEarrings(Serial serial)
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

    public class SilverEarrings : BaseEarrings
    {
        [Constructable]
        public SilverEarrings()
            : base(0x1F07)
        {
            Weight = 0.5;
			PrefixOption[50] = 20;
			PrefixOption[61] = 43;
			SuffixOption[61] = 75000;
			PrefixOption[62] = 45;
			SuffixOption[62] = 150000;
        }
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
		public override int AosDexterityReq { get { return 100; } }
		public override int AosIntelligenceReq { get { return 5000; } }
        public override int LabelNumber { get { return 1138018; } }
        public SilverEarrings(Serial serial)
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