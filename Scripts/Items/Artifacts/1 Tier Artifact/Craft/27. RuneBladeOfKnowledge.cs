using System;

namespace Server.Items
{
    public class RuneBladeOfKnowledge : HeavyCrossbow
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public RuneBladeOfKnowledge()
        {
			//운 1, 명중률 40%
            Attributes.SpellDamage = 5;
        }

        public RuneBladeOfKnowledge(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073539;
            }
        }// rune blade of knowledge
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