using System;

namespace Server.Items
{
    [FlipableAttribute(0x1451, 0x1456)]
    public class DaemonHelm : BaseArmor
    {
        public override bool IsArtifact { get { return true; } }
        [Constructable]
        public DaemonHelm()
            : base(0x1451)
        {
            this.Hue = 0x648;
            this.Weight = 3.0;

        }

        public DaemonHelm(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 66;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 66;
            }
        }

        public override int AosStrReq
        {
            get
            {
                return 20;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 40;
            }
        }
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
