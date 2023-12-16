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
			PrefixOption[61] = 5;
			SuffixOption[61] = 25000;
			PrefixOption[62] = 20;
			SuffixOption[62] = 50;
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
		public override int AosDexterityReq { get { return 1; } }
		public override int AosIntelligenceReq { get { return 0; } }
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
			PrefixOption[61] = 6;
			SuffixOption[61] = 25000;
			PrefixOption[62] = 21;
			SuffixOption[62] = 50;
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
		public override int AosDexterityReq { get { return 0; } }
		public override int AosIntelligenceReq { get { return 1; } }
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