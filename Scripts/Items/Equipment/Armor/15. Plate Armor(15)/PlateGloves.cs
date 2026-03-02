using System;
using Server.Engines.Craft;

namespace Server.Items
{
	[Alterable(typeof(DefBlacksmithy), typeof(GargishPlateKilt))]
	[FlipableAttribute(0x1414, 0x1418)]
	public class PlateGloves : BaseArmor
	{
		[Constructable]
		public PlateGloves()
			: base(0x1414)
		{
			PrefixOption[50] = 17; //세트 옵션 번호
			PrefixOption[61] = 101; //기절 시간 감소 (임의 코드 101)
			SuffixOption[61] = 10000; //1.0초
			Weight = 20.0;
		}

		public PlateGloves(Serial serial)
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
			get { return 10; }
		}
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Plate; }
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
