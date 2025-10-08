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
			PrefixOption[50] = 14;   //세트 옵션 번호
			PrefixOption[61] = 113;  //모든 저항력%
			SuffixOption[61] = 60000; //6%
			PrefixOption[62] = 59;   //곤충 피해 증가% 
			SuffixOption[62] = 250000; //25%
			PrefixOption[63] = 118;  //모든 속도%
			SuffixOption[63] = 100000; //10%
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