using System;

namespace Server.Items
{
    public class LesserRefreshPotion : BaseRefreshPotion
    {
        [Constructable]
        public LesserRefreshPotion()
            : base(PotionEffect.RefreshLesser)
        {
			Name = "하급 기력 회복 물약";
        }

        public LesserRefreshPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 80;
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