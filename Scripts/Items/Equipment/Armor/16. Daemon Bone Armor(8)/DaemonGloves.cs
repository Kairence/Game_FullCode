using System;

namespace Server.Items
{
    [FlipableAttribute(0x1450, 0x1455)]
    public class DaemonGloves : BaseArmor
    {
        [Constructable]
        public DaemonGloves()
            : base(0x1450)
        {
            this.Weight = 5.0;
            this.Hue = 0x648;
			PrefixOption[50] = 14;
			PrefixOption[61] = 114;
			SuffixOption[61] = 60000;
			PrefixOption[62] = 59;
			SuffixOption[62] = 250000;
			PrefixOption[63] = 118;
			SuffixOption[63] = 100000;
        }

        public DaemonGloves(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 1116; } }
        public override int AosDexReq { get { return 1116; } }
        public override int AosIntReq { get { return 1116; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 8;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Bone;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1041373;
            }
        }// daemon bone gloves
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (this.Weight == 1.0)
                this.Weight = 2.0;

            if (this.ArmorAttributes.SelfRepair == 0)
                this.ArmorAttributes.SelfRepair = 1;
        }
    }
}