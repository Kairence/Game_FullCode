using System;

namespace Server.Items
{
    public class MacheteOfDefense : VikingSword
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public MacheteOfDefense()
        {
			//기력 1000, 공속 10%, 무피 10%
            Attributes.DefendChance = 5;
        }

        public MacheteOfDefense(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073535;
            }
        }// machete of defense
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