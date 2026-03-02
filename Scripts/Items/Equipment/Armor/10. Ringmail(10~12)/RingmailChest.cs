using System;

namespace Server.Items
{
	[FlipableAttribute(0x13ec, 0x13ed)]
	public class RingmailChest : BaseArmor
	{
		[Constructable]
		public RingmailChest()
			: base(0x13EC)
		{
			PrefixOption[50] = 15; //세트 옵션 번호
			PrefixOption[61] = 7; //무기 피해%
			SuffixOption[61] = 100000; //10%
			PrefixOption[62] = 40; //공격 속도%
			SuffixOption[62] = 50000; //5%

			Weight = 30.0;
		}

		public RingmailChest(Serial serial)
			: base(serial) { }

		public override int AosStrReq
		{
			get { return 3500; }
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
			get { return 8; }
		}

		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Ringmail; }
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
