using System;

namespace Server.Items
{
    public class LongbowOfMight : Maul
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public LongbowOfMight()
        {
			//명중률 50%, 무기 피해 50%, 공속 30%, 체력 125, 기력 125
            Attributes.WeaponDamage = 5;
        }

        public LongbowOfMight(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073508;
            }
        }// longbow of might
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