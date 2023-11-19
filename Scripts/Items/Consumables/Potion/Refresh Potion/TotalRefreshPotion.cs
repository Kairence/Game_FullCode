using System;

namespace Server.Items
{
    public class TotalRefreshPotion : BaseRefreshPotion
    {
        [Constructable]
        public TotalRefreshPotion()
            : base(PotionEffect.RefreshTotal)
        {
			Name = "최상급 기력 회복 물약";
        }

        public TotalRefreshPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 500;
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