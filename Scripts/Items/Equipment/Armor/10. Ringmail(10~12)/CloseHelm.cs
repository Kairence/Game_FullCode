using System;

namespace Server.Items
{
	public class CloseHelm : BaseArmor
	{
		[Constructable]
		public CloseHelm()
			: base(0x1408)
		{
			PrefixOption[50] = 15; //세트 옵션 번호
			PrefixOption[61] = 42; //물리 치명 확률%
			SuffixOption[61] = 50000; //5%
			PrefixOption[62] = 44; //물리 치명 피해%
			SuffixOption[62] = 50000; //5%
			Weight = 25.0;
		}

		public CloseHelm(Serial serial)
			: base(serial) { }

		public override int AosStrReq
		{
			get { return 2500; }
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
