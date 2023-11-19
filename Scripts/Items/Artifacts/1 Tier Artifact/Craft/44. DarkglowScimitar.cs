using System;

namespace Server.Items
{
    public class DarkglowScimitar : RadiantScimitar
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public DarkglowScimitar()
        {
			//기력 증가 1000, 기력 회복 0.4
            WeaponAttributes.HitDispel = 10;
        }

        public DarkglowScimitar(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073542;
            }
        }// darkglow scimitar
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