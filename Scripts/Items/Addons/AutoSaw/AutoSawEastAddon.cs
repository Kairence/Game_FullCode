using System;
using Server.Mobiles;
namespace Server.Items
{
    public class AutoSawEastAddon : BaseAddon
    {
        [Constructable]
        public AutoSawEastAddon()
        {
            this.AddComponent(new AddonComponent(0xB8C), 0, 0, 0);
            this.AddComponent(new AddonComponent(0xB8B), 1, 0, 0);
            this.AddComponent(new AddonComponent(0xB8E), 0, -1, 0);
            this.AddComponent(new AddonComponent(0xB8E), 1, -1, 0);
            this.AddComponent(new AddonComponent(0xB8D), 0, -2, 0);
            this.AddComponent(new AddonComponent(0xB8A), 1, -2, 0);
            this.AddComponent(new AddonComponent(0x11B5), 1, 0, 6);
 			Server.Multis.BaseHouse house = Server.Multis.BaseHouse.FindHouseAt(this);
			if ( house != null )
			{
				MaxHits = Utility.RandomMinMax(30, 50);//Utility.RandomMinMax(InitMinHits, InitMaxHits);
				Hits = MaxHits;
			}
        }
        public override int LabelNumber
        {
            get
            {
                return 1046485;
            }
        }		
		/*
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
					else if( pm.Tired < 50 )
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
		*/
        public AutoSawEastAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                return new ElvenBedEastDeed();
            }
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

    public class AutoSawEastAddonDeed : BaseAddonDeed
    {
        [Constructable]
        public AutoSawEastAddonDeed()
        {
        }

        public AutoSawEastAddonDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                return new AutoSawEastAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1046483;
            }
        }// elven bed (east)
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