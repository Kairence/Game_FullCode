using System;
using Reward = Server.Engines.Quests.BaseReward;

namespace Server.Items
{
    public class TombRewardBag : Bag
    {
        public TombRewardBag()
            : base()
        {
            Hue = Reward.RewardBagHue();
        }

        public TombRewardBag(Serial serial)
            : base(serial)
        {
        }

        public virtual int Min
        {
            get
            {
                return 0;
            }
        }
        public virtual int Max
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
    public class CoveTombBag : TombRewardBag
    {
        [Constructable]
        public CoveTombBag()
            : base()
        { 
			int dice = Utility.RandomMinMax( Min, Max );
			DropItem(new Gold( dice * 10) );
			
			int tier = dice * 4;
			tier /= 100;
			
			Item item = null;
			if( 0.9 > Utility.RandomDouble() )
			{
				switch ( Utility.Random(5) )
				{
					case 0:
						item = new BoneArms();
						break;	
					case 1:
						item = new BoneChest();
						break;
					case 2:
						item = new BoneGloves();
						break;
					case 3:
						item = new BoneHelm();
						break;
					case 4:
						item = new BoneLegs();
						break;
				}
			}
			else 
			{
				item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();
			}
			if (item != null)
			{
				Misc.Util.NewItemCreate(item, 1);
				DropItem( item );
			}		
        }

        public CoveTombBag(Serial serial)
            : base(serial)
        {
        }

        public override int Min
        {
            get
            {
                return 10;
            }
        }
        public override int Max
        {
            get
            {
                return 25;
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