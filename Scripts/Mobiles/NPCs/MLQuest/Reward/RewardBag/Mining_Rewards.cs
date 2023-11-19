using System;
using Reward = Server.Engines.Quests.BaseReward;

namespace Server.Items
{
    public class Mining1RewardBag : Bag
    {
        [Constructable]
        public Mining1RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			DropItem(new Gold(100));
						
			//DropItem(new Pickaxe());
        }

        public Mining1RewardBag(Serial serial)
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
    public class Mining2RewardBag : Bag
    {
        [Constructable]
        public Mining2RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new Gold(400));
        }

        public Mining2RewardBag(Serial serial)
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
    public class Mining3RewardBag : Bag
    {
        [Constructable]
        public Mining3RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new LeatherGlovesOfMining(1));
        }

        public Mining3RewardBag(Serial serial)
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
    public class Mining4RewardBag : Bag
    {
        [Constructable]
        public Mining4RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new BankCheck(8000));

        }

        public Mining4RewardBag(Serial serial)
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
    public class Mining5RewardBag : Bag
    {
        [Constructable]
        public Mining5RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new StuddedGlovesOfMining(3));

        }

        public Mining5RewardBag(Serial serial)
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
    public class Mining6RewardBag : Bag
    {
        [Constructable]
        public Mining6RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new BankCheck(50000));

        }

        public Mining6RewardBag(Serial serial)
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
    public class Mining7RewardBag : Bag
    {
        [Constructable]
        public Mining7RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new RingmailGlovesOfMining(5));

        }

        public Mining7RewardBag(Serial serial)
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
    public class Mining8RewardBag : Bag
    {
        [Constructable]
        public Mining8RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
			
			DropItem(new RingmailGlovesOfMining(5));

        }

        public Mining8RewardBag(Serial serial)
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
    public class Mining9RewardBag : Bag
    {
        public Mining9RewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
        }

        public Mining9RewardBag(Serial serial)
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