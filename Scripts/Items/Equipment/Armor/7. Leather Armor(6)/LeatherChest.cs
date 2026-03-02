using System;
using Server.Engines.Craft;

namespace Server.Items
{
	[Alterable(typeof(DefTailoring), typeof(GargishLeatherChest))]
	[FlipableAttribute(0x13cc, 0x13d3)]
	public class LeatherChest : BaseArmor
	{
		[Constructable]
		public LeatherChest()
			: base(0x13CC)
		{
			Weight = 17.0;
			PrefixOption[50] = 4; //세트 옵션 번호
			PrefixOption[61] = 19; //체력 회복
			SuffixOption[61] = 30000; //3
			PrefixOption[62] = 20; //기력 회복
			SuffixOption[62] = 30000; //3
			PrefixOption[63] = 21; //마나 회복
			SuffixOption[63] = 30000; //3
		}

		public LeatherChest(Serial serial)
			: base(serial) { }

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
			get { return 2100; }
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
			get { return 3; }
		}
		public override ArmorMaterialType MaterialType
		{
			get { return ArmorMaterialType.Leather; }
		}
		public override CraftResource DefaultResource
		{
			get { return CraftResource.RegularLeather; }
		}
		public override ArmorMeditationAllowance DefMedAllowance
		{
			get { return ArmorMeditationAllowance.All; }
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
