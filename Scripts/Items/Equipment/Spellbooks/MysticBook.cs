using System;

namespace Server.Items
{
    public class MysticBook : Spellbook
    {
        [Constructable]
        public MysticBook()
            : this((ulong)0)
        {
        }

        [Constructable]
        public MysticBook(ulong content)
            : base(content, 0x2D9D)
        {
            //this.Layer = Layer.OneHanded;
			Weight = 3.0;
			Attributes.CastSpeed += 50;
			Attributes.CastRecovery += 50;
        }
        public override int LabelNumber
        {
            get
            {
                return 1039019;
            }
        }
        public MysticBook(Serial serial)
            : base(serial)
        {
        }
        public override int InitMinHits { get { return 31; } }
        public override int InitMaxHits { get { return 50; } }
		public override int AosIntelligenceReq
		{
			get
			{
				return 500;
			}
		}
        public override SpellbookType SpellbookType
        {
            get
            {
                return SpellbookType.Mystic;
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
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadEncodedInt();
        }
    }
}