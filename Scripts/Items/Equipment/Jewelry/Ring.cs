using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public abstract class BaseRing : BaseJewel
    {
        public BaseRing(int itemID)
            : base(itemID, Layer.Ring)
        {

		}

        public BaseRing(Serial serial)
            : base(serial)
        {
        }

        public override int BaseGemTypeNumber
        {
            get
            {
                return 1044176;
            }
        }// star sapphire ring
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

    public class GoldRing : BaseRing
    {
        [Constructable]
        public GoldRing()
            : base(0x108a)
        {
            Weight = 0.5;
			PrefixOption[50] = 19;
			PrefixOption[61] = 115;
			SuffixOption[61] = 50000;
			PrefixOption[62] = 32;
			SuffixOption[62] = 250000;
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
		public override int AosIntelligenceReq { get { return 2000; } }
        public override int LabelNumber { get { return 1138013; } }
        public GoldRing(Serial serial)
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

    public class SilverRing : BaseRing, IRepairable
    {
        public CraftSystem RepairSystem { get { return DefTinkering.CraftSystem; } }

        [Constructable]
        public SilverRing()
            : base(0x1F09)
        {
            Weight = 0.5;
			PrefixOption[50] = 20;
			PrefixOption[61] = 119;
			SuffixOption[61] = 50000;
			PrefixOption[62] = 47;
			SuffixOption[62] = 600000;
			PrefixOption[63] = 33;
			SuffixOption[63] = 100000;
			PrefixOption[64] = 34;
			SuffixOption[64] = 100000;
			PrefixOption[65] = 35;
			SuffixOption[65] = 100000;
			PrefixOption[66] = 36;
			SuffixOption[66] = 100000;
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
		public override int AosIntelligenceReq { get { return 4000; } }
        public override int LabelNumber { get { return 1138014; } }
        public SilverRing(Serial serial)
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