using System;

namespace Server.Items
{
    [FlipableAttribute(0x1451, 0x1456)]
    public class DaemonHelm : BaseArmor
    {
        [Constructable]
        public DaemonHelm()
            : base(0x1451)
        {
            this.Hue = 0x648;
            this.Weight = 8.0;
			PrefixOption[50] = 14;   //세트 옵션 번호
			PrefixOption[61] = 113;  //모든 저항력%
			SuffixOption[61] = 60000; //6%
			PrefixOption[62] = 56;   //영장류 피해 증가% 
			SuffixOption[62] = 250000; //25%
			PrefixOption[63] = 118;  //모든 속도%
			SuffixOption[63] = 100000; //10%

        }

        public DaemonHelm(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 1216; } }
        public override int AosDexReq { get { return 1216; } }
        public override int AosIntReq { get { return 1216; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 6;
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
                return 1041374;
            }
        }// daemon bone helmet
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
                this.Weight = 3.0;

            if (this.ArmorAttributes.SelfRepair == 0)
                this.ArmorAttributes.SelfRepair = 1;
        }
    }
}
