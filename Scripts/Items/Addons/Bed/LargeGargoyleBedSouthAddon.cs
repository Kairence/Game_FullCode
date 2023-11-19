using System;
using Server.Mobiles;
namespace Server.Items
{
    public class LargeGargoyleBedSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed
        {
            get
            {
                return new LargeGargoyleBedSouthDeed();
            }
        }
       #region Mondain's Legacy
        public override bool RetainDeedHue
        {
            get
            {
                return true;
            }
        }
        #endregion

        [Constructable]
        public LargeGargoyleBedSouthAddon()
        { 
            //Left Side
            this.AddComponent(new AddonComponent(0x4010), 0, 0, 0);
            this.AddComponent(new AddonComponent(0x4013), 0, 1, 0);
            this.AddComponent(new AddonComponent(0x4016), 0, 2, 0);
            //Middle
            this.AddComponent(new AddonComponent(0x4011), 1, 0, 0);
            this.AddComponent(new AddonComponent(0x4014), 1, 1, 0);
            this.AddComponent(new AddonComponent(0x4017), 1, 2, 0);
            //Right Side
            this.AddComponent(new AddonComponent(0x4012), 2, 0, 0);
            this.AddComponent(new AddonComponent(0x4015), 2, 1, 0);
            this.AddComponent(new AddonComponent(0x4018), 2, 2, 0);
			Server.Multis.BaseHouse house = Server.Multis.BaseHouse.FindHouseAt(this);
			if ( house != null )
			{
				MaxHits = Utility.RandomMinMax(75, 100);//Utility.RandomMinMax(InitMinHits, InitMaxHits);
				Hits = MaxHits;
			}

        }
		public override void OnComponentUsed( AddonComponent c, Mobile from )
		{
			bool useOk = false;
			int housecheck = 0;
			if( from.Region is Server.Regions.TownRegion )
			{
				housecheck = 1;
				useOk = true;
			}
 			Server.Multis.BaseHouse house = Server.Multis.BaseHouse.FindHouseAt(from);

			if( house != null )
			{
				housecheck = 2;
				useOk = true;
			}
			if ( useOk && from.InRange( c.Location, 2 ) )
			{
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					if( !pm.IsStaff() && pm.TimerList[72] > 0 )
					{
						from.SendMessage("당신은 {0}동안 사용할 수 없습니다.", Server.Misc.Util.NowTime(pm.TimerList[72]) );
						return;
					}
					else if( pm.Tired < 0 )
					{
						from.SendMessage("지금은 휴식하지 않아도 괜찮습니다." );
						return;
					}
					else
					{
						switch(housecheck)
						{
							case 0:
							{
								from.SendMessage("당신은 이 침대를 사용할 수 없습니다.");
								break;
							}
							
							case 1:
							{
								BedUse(pm, false);
								break;
							}
							case 2:
							{
								if( MaxHits == 0 && Hits == 0 )
									MaxHits = Hits = Utility.RandomMinMax( 30, 50 );
								BedUse(pm, true);
								break;
							}
						}
					}
				}
			}
		}

        public LargeGargoyleBedSouthAddon(Serial serial)
            : base(serial)
        {
        }

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

    public class LargeGargoyleBedSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon
        {
            get
            {
                return new LargeGargoyleBedSouthAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1111761;
            }
        }// large gargish bed (south)

        [Constructable]
        public LargeGargoyleBedSouthDeed()
        {
        }

        public LargeGargoyleBedSouthDeed(Serial serial)
            : base(serial)
        {
        }

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