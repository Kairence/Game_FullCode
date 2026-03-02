using System;

namespace Server.Items
{
	[FlipableAttribute(0x2B73, 0x316A)]
	public class WingedHelm : BaseArmor
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
			get { return 800; }
		}
		public override int AosDexReq
		{
			get { return 100; }
		}
		public override int AosIntReq
		{
			get { return 100; }
		}
		public override int OldStrReq
		{
			get { return 15; }
		}
		public override int ArmorBase
		{
			get { return 2; }
		}

		[Constructable]
		public WingedHelm()
			: base(0x2B73)
		{
			Weight = 13.0;
			PrefixOption[50] = 4; //세트 옵션 번호
			PrefixOption[61] = 21; //마나 회복
			SuffixOption[61] = 50000; //5
			PrefixOption[62] = 6; //마나
			SuffixOption[62] = 2500000; //250
		}

		public WingedHelm(Serial serial)
			: base(serial) { }

		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Leather; }
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
