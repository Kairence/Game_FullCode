using System;

namespace Server.Items
{
    public abstract class BaseNecklace : BaseJewel
    {
        public BaseNecklace(int itemID)
            : base(itemID, Layer.Neck)
        {

        }

        public BaseNecklace(Serial serial)
            : base(serial)
        {
        }

        public override int BaseGemTypeNumber
        {
            get
            {
                return 1044241;
            }
        }// star sapphire necklace
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

    public class Necklace : BaseNecklace
    {
        [Constructable]
        public Necklace()
            : base(0x1085)
        {
			Attributes.Brittle += 250;
            this.Weight = 0.1;
        }
        public override int InitMinHits
        {
            get
            {
                return 25;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 30;
            }
        }
		public override int AosIntelligenceReq
		{
			get
			{
				return 50;
			}
		}
        public Necklace(Serial serial)
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

    public class GoldNecklace : BaseNecklace
    {
        [Constructable]
        public GoldNecklace()
            : base(0x1088)
        {
			PrefixOption[61] = 42;
			SuffixOption[61] = 500;
			PrefixOption[62] = 44;
			SuffixOption[62] = 1000;
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
        public override int LabelNumber { get { return 1138015; } }
		
        public GoldNecklace(Serial serial)
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

    public class GoldBeadNecklace : BaseNecklace
    {
        [Constructable]
        public GoldBeadNecklace()
            : base(0x1089)
        {
            this.Weight = 0.1;
        }
		public override int AosIntelligenceReq
		{
			get
			{
				return 200;
			}
		}
        public GoldBeadNecklace(Serial serial)
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

    public class SilverNecklace : BaseNecklace
    {
        [Constructable]
        public SilverNecklace()
            : base(0x1F08)
        {
			PrefixOption[61] = 43;
			SuffixOption[61] = 500;
			PrefixOption[62] = 45;
			SuffixOption[62] = 1000;
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
        public override int LabelNumber { get { return 1138016; } }
        public SilverNecklace(Serial serial)
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

    public class SilverBeadNecklace : BaseNecklace
    {
        [Constructable]
        public SilverBeadNecklace()
            : base(0x1F05)
        {
            this.Weight = 0.1;
        }

        public SilverBeadNecklace(Serial serial)
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