using System;

namespace Server.Items
{
	[FlipableAttribute(0x2641, 0x2642)]
	public class GreenDragonChest : BaseArmor
	{
		[Constructable]
		public GreenDragonChest()
			: base(0x2641)
		{
			PrefixOption[50] = 10; //세트 옵션 번호
			PrefixOption[61] = 15; //독 저항
			SuffixOption[61] = 250000; //25%
			PrefixOption[62] = 25; //독 피해 증가
			SuffixOption[62] = 200000; //20
			Weight = 25.0;
		}

		public GreenDragonChest(Serial serial)
			: base(serial) { }

		public override int LabelNumber
		{
			get { return 1029793; }
		}
		public override int AosStrReq
		{
			get { return 4000; }
		}
		public override int AosDexReq
		{
			get { return 4000; }
		}
		public override int AosIntReq
		{
			get { return 4000; }
		}
		public override int OldStrReq
		{
			get { return 15; }
		}
		public override int ArmorBase
		{
			get { return 3; }
		}
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Dragon; }
		}
		public override CraftResource DefaultResource
		{
			get { return CraftResource.GreenScales; }
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
