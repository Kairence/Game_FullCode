using System;
using System.Collections.Generic;
using Server.Engines.Quests;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
	public class SpecialPrintingOfVirtue : Item
	{
		public override int LabelNumber
		{
			get { return 1075793; }
		} // Special Printing of 'Virtue' by Lord British

		[Constructable]
		public SpecialPrintingOfVirtue()
			: base(4082) { }

		public SpecialPrintingOfVirtue(Serial serial)
			: base(serial) { }

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
