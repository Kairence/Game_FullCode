using System;
using Server.Engines.Craft;

namespace Server.Items
{
	[Alterable(typeof(DefBlacksmithy), typeof(LargePlateShield))]
	public class HeaterShield : BaseShield
	{
		[Constructable]
		public HeaterShield()
			: base(0x1B76)
		{
			this.Weight = 50.0;
			ShieldMinDamage = 5;
			ShieldMaxDamage = 8;
		}

		public HeaterShield(Serial serial)
			: base(serial) { }

		public override int InitMinHits
		{
			get { return 100; }
		}
		public override int InitMaxHits
		{
			get { return 100; }
		}
		public override int AosStrReq
		{
			get { return 3500; }
		}
		public override int AosDexReq
		{
			get { return 1000; }
		}
		public override int AosIntReq
		{
			get { return 100; }
		}
		public override int ArmorBase
		{
			get { return 12; }
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); //version
		}
	}
}
