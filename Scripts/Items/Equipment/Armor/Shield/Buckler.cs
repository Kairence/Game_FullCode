using System;
using Server.Engines.Craft;

namespace Server.Items
{
	[Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
	public class Buckler : BaseShield
	{
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
			get { return 2000; }
		}
		public override int AosDexReq
		{
			get { return 1000; }
		}
		public override int AosIntReq
		{
			get { return 1000; }
		}
		public override int ArmorBase
		{
			get { return 7; }
		}

		[Constructable]
		public Buckler()
			: base(0x1B73)
		{
			this.Weight = 25.0;
			ShieldMinDamage = 5;
			ShieldMaxDamage = 10;
		}

		public Buckler(Serial serial)
			: base(serial) { }

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
