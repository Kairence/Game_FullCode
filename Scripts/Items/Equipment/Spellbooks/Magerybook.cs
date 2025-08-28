using System;

namespace Server.Items
{
    public class Magerybook : Spellbook
    {
        [Constructable]
        public Magerybook()
            : this((ulong)0)
        {
        }

        [Constructable]
        public Magerybook(ulong content)
            : base(0, 0x225A)
        {
            this.Layer = (Core.ML ? Layer.OneHanded : Layer.Invalid);
			Weight = 3.0;

			PrefixOption[61] = 41;
			SuffixOption[61] = 1000000;
			PrefixOption[62] = 7;
			SuffixOption[62] = -750000;

			//Attributes.CastSpeed += 50;
			//Attributes.CastRecovery += 50;
		}
        public override int LabelNumber
        {
            get
            {
                return 1039016;
            }
        }
        public override SpellbookType SpellbookType
        {
            get
            {
                return SpellbookType.Magery;
            }
        }
        public Magerybook(Serial serial)
            : base(serial)
        {
        }
        public override int InitMinHits { get { return 31; } }
        public override int InitMaxHits { get { return 50; } }
		public override int AosIntelligenceReq
		{
			get
			{
				return 1000;
			}
		}
        public override int BookOffset
        {
            get
            {
                return 0;
            }
        }
        public override int BookCount
        {
            get
            {
                return 0;
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

            if (version == 0 && Core.ML)
                this.Layer = Layer.OneHanded;
        }
    }
}