using System;

namespace Server.Items
{
	[Flipable(0x2645, 0x2646)]
	public class RedDragonHelm : BaseArmor
	{
		[Constructable]
		public RedDragonHelm()
			: base(0x2645)
		{
			Weight = 12.0;
			PrefixOption[50] = 8; //세트 옵션 번호
			PrefixOption[61] = 13; //화염 저항
			SuffixOption[61] = 250000; //25%
			PrefixOption[62] = 23; //불 피해 증가
			SuffixOption[62] = 200000; //20
		}

		public RedDragonHelm(Serial serial)
			: base(serial) { }

		public override int LabelNumber
		{
			get { return 1029797; }
		}
		public override int AosStrReq
		{
			get { return 2500; }
		}
		public override int AosDexReq
		{
			get { return 2500; }
		}
		public override int AosIntReq
		{
			get { return 2500; }
		}
		public override int OldStrReq
		{
			get { return 6; }
		}
		public override int ArmorBase
		{
			get { return 9; }
		}
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Dragon; }
		}
		public override CraftResource DefaultResource
		{
			get { return CraftResource.RedScales; }
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
