using System;

namespace Server.Items
{
	[FlipableAttribute(0x1450, 0x1455)]
	public class BoneGloves : BaseArmor
	{
		public override int AosStrReq
		{
			get { return 450; }
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
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Bone; }
		}
		public override CraftResource DefaultResource
		{
			get { return CraftResource.RegularLeather; }
		}

		[Constructable]
		public BoneGloves()
			: base(0x1450)
		{
			Weight = 10.0;

			PrefixOption[50] = 7; //세트 옵션 번호
			PrefixOption[61] = 3; //운
			SuffixOption[61] = 1250000; //125
			PrefixOption[62] = 20; //기력 회복
			SuffixOption[62] = 40000; //4
			PrefixOption[63] = 21; //마나 회복
			SuffixOption[63] = 40000; //4
		}

		public BoneGloves(Serial serial)
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
