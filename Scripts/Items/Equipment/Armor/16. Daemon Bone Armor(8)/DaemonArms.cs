using System;

namespace Server.Items
{
    [FlipableAttribute(0x144e, 0x1453)]
    public class DaemonArms : BaseArmor
    {
        [Constructable]
        public DaemonArms()
            : base(0x144E)
        {
            this.Weight = 10.0;
            this.Hue = 0x648;
			PrefixOption[50] = 14;
			PrefixOption[61] = 114;
			SuffixOption[61] = 60000;
			PrefixOption[62] = 62;
			SuffixOption[62] = 250000;
			PrefixOption[63] = 118;
			SuffixOption[63] = 100000;

        }

        public DaemonArms(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 1416; } }
        public override int AosDexReq { get { return 1416; } }
        public override int AosIntReq { get { return 1416; } }
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
                return 1041371;
            }
        }// daemon bone arms
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            if (this.Weight == 1.0)
                this.Weight = 2.0;
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (this.ArmorAttributes.SelfRepair == 0)
                this.ArmorAttributes.SelfRepair = 1;
        }
    }
}
