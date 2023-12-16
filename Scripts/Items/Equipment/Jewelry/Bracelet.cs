using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public abstract class BaseBracelet : BaseJewel
    {
        public BaseBracelet(int itemID)
            : base(itemID, Layer.Bracelet)
        {

        }

        public BaseBracelet(Serial serial)
            : base(serial)
        {
        }
        public override int BaseGemTypeNumber
        {
            get
            {
                return 1044221;
            }
        }// star sapphire bracelet

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 1)
            {
                if (Weight == .1)
                {
                    Weight = -1;
                }
            }
        }
    }

    public class GoldBracelet : BaseBracelet
    {
        [Constructable]
        public GoldBracelet()
            : base(0x1086)
        {
            //Weight = 0.1;
			PrefixOption[61] = 7;
			SuffixOption[61] = 1000;
			PrefixOption[62] = 1;
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
        public override int LabelNumber { get { return 1138011; } }
        public GoldBracelet(Serial serial)
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

    public class SilverBracelet : BaseBracelet, IRepairable
    {
        public CraftSystem RepairSystem { get { return DefTinkering.CraftSystem; } }

        [Constructable]
        public SilverBracelet()
            : base(0x1F06)
        {
			PrefixOption[61] = 8;
			SuffixOption[61] = 1000;
			PrefixOption[62] = 2;
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
        public override int LabelNumber { get { return 1138012; } }
        public SilverBracelet(Serial serial)
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