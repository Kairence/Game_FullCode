using System;
using System.Collections.Generic;
using Server.Engines.Quests;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
	public class GoldShield : OrderShield
	{
		[Constructable]
		public GoldShield()
		{
			Name = "a gold shield";
			Hue = 0x501;
		}

		public GoldShield(Serial serial)
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
