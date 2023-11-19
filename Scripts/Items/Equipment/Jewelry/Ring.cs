using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public abstract class BaseRing : BaseJewel
    {
        public BaseRing(int itemID)
            : base(itemID, Layer.Ring)
        {
			BaseJewelRating = 1;
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
			Attributes.WeaponSpeed += 250;
            //Weight = 0.1;
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

        public GoldRing(Serial serial)
            : base(serial)
        {
        }
		public override int AosIntelligenceReq
		{
			get
			{
				return 40;
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

    public class SilverRing : BaseRing, IRepairable
    {
        public CraftSystem RepairSystem { get { return DefTinkering.CraftSystem; } }

        [Constructable]
        public SilverRing()
            : base(0x1F09)
        {
			Attributes.CastSpeed += 250;
            //Weight = 0.1;
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
				return 80;
			}
		}
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