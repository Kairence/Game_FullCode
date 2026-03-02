using System;

namespace Server.Items
{
	[Flipable(0x2645, 0x2646)]
	public class BlackDragonHelm : BaseArmor
	{
		[Constructable]
		public BlackDragonHelm()
			: base(0x2645)
		{
			PrefixOption[50] = 13; //세트 옵션 번호
			PrefixOption[61] = 27; //혼돈 피해 증가
			SuffixOption[61] = 100000; //10
			PrefixOption[62] = 120; //모든 피격 피해 감소%
			SuffixOption[62] = 50000; //5%
			Weight = 15.0;
		}

		public BlackDragonHelm(Serial serial)
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
			get { return 15; }
		}
		public override int ArmorBase
		{
			get { return 7; }
		}
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Dragon; }
		}
		public override CraftResource DefaultResource
		{
			get { return CraftResource.BlackScales; }
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
