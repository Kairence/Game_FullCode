using System;

namespace Server.Items
{
    public class MiniRefreshPotion : BaseRefreshPotion
    {
        [Constructable]
        public MiniRefreshPotion()
            : base(PotionEffect.RefreshMini)
        {
			Name = "최하급 기력 회복 물약";
        }

        public MiniRefreshPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 20;
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