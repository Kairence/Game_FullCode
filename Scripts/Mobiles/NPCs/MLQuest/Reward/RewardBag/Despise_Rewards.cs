using System;
using Reward = Server.Engines.Quests.BaseReward;

namespace Server.Items
{
    public class Despise1RewardBag : Bag
    {
        public Despise1RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new Gold(200));
        }

        public Despise1RewardBag(Serial serial)
            : base(serial)
        {
        }

        public virtual int ItemAmount
        {
            get
            {
                return 0;
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