using System;

namespace Server.Items
{
    [FlipableAttribute(0x1452, 0x1457)]
    public class DaemonLegs : BaseArmor
    {
        [Constructable]
        public DaemonLegs()
            : base(0x1452)
        {
            this.Weight = 3.0;
            this.Hue = 0x648;
			PrefixOption[50] = 14;
			PrefixOption[61] = 44;
			SuffixOption[61] = 1000;
			PrefixOption[62] = 45;
			SuffixOption[62] = 1000;
        }

        public DaemonLegs(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 16;
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
                return 1041375;
            }
        }// daemon bone leggings
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
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