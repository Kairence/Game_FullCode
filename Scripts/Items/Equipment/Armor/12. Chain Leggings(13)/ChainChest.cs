using System;

namespace Server.Items
{
	[FlipableAttribute(0x13bf, 0x13c4)]
	public class ChainChest : BaseArmor
	{
		public override int AosStrReq
		{
			get { return 4000; }
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
			get { return 9; }
		}
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Chainmail; }
		}

		[Constructable]
		public ChainChest()
			: base(0x13BF)
		{
			PrefixOption[50] = 16; //세트 옵션 번호
			PrefixOption[61] = 120; //모든 피격 피해 감소%
			SuffixOption[61] = 50000; //5%
			PrefixOption[62] = 42; //물리 치명 확률%
			SuffixOption[62] = 50000; //5%
			PrefixOption[63] = 44; //물리 치명 피해%
			SuffixOption[63] = 50000; //5%

			Weight = 35.0;
		}

		public ChainChest(Serial serial)
			: base(serial) { }

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
