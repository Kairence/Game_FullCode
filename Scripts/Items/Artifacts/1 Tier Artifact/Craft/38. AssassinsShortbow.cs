using System;

namespace Server.Items
{
    public class AssassinsShortbow : ShortSpear
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public AssassinsShortbow()
        {
			//물치확률 5%, 무피 50%, 명중 4%
            Attributes.AttackChance = 3;
            Attributes.WeaponDamage = 4;
        }

        public AssassinsShortbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073512;
            }
        }// assassin's shortbow
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