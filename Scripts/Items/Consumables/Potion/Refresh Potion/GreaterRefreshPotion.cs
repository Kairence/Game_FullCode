using System;

namespace Server.Items
{
    public class GreaterRefreshPotion : BaseRefreshPotion
    {
        [Constructable]
        public GreaterRefreshPotion()
            : base(PotionEffect.RefreshGreater)
        {
			Name = "상급 기력 회복 물약";
        }

        public GreaterRefreshPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 320;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}