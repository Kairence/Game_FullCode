using System;

namespace Server.Items
{
    public class RefreshPotion : BaseRefreshPotion
    {
        [Constructable]
        public RefreshPotion()
            : base(PotionEffect.Refresh)
        {
			Name = "기력 회복 물약";
        }

        public RefreshPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 180;
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