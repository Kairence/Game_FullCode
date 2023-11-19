using System;

namespace Server.Items
{
    public class CorruptedRuneBlade : BaseWand
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public CorruptedRuneBlade()
        {
			//시전 100%, 공속 -20%
            WeaponAttributes.ResistPhysicalBonus = -5;
            WeaponAttributes.ResistPoisonBonus = 12;
        }

        public CorruptedRuneBlade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073540;
            }
        }// Corrupted Rune Blade
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