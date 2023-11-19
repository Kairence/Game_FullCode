using System;

namespace Server.Items
{
    public class BarbedLongbow : DoubleBladedStaff
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public BarbedLongbow()
        {
			//포이즈닝 +5, 독피해 40, 독저 50%
            Attributes.ReflectPhysical = 12;
        }

        public BarbedLongbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073505;
            }
        }// barbed longbow
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}