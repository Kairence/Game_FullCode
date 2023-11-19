using System;

namespace Server.Items
{
    public class RangersShortbow : Axe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public RangersShortbow()
        {
			//곤충슬 200%, 무피 60%, 공속 20%
            Attributes.WeaponSpeed = 5;
        }

        public RangersShortbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073509;
            }
        }// ranger's shortbow
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