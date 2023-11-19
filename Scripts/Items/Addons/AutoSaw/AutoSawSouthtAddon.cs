using System;
using Server.Mobiles;
namespace Server.Items
{
    public class AutoSawSouthtAddon : BaseAddon
    {
        [Constructable]
        public AutoSawSouthtAddon()
        {
            this.AddComponent(new AddonComponent(0xB8C), 0, 0, 0);
            this.AddComponent(new AddonComponent(0xB8D), 1, 0, 0);
            this.AddComponent(new AddonComponent(0xB8B), 2, 0, 0);
            this.AddComponent(new AddonComponent(0xB8D), 0, -1, 0);
            this.AddComponent(new AddonComponent(0xB8D), 1, -1, 0);
            this.AddComponent(new AddonComponent(0xB8A), 2, -1, 0);
            this.AddComponent(new AddonComponent(0x11B5), 1, 0, 3);
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
        public AutoSawSouthtAddon(Serial serial)
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

    public class AutoSawSouthtAddonDeed : BaseAddonDeed
    {
        [Constructable]
        public AutoSawSouthtAddonDeed()
        {
        }

        public AutoSawSouthtAddonDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                return new AutoSawSouthtAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1046484;
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