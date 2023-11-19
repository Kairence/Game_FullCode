using System;
using Server;
using System.Collections;
using Server.Items; 
using Server.Mobiles;

namespace Server 
{ 
	public class CropHelper
	{
		public static bool FarmLocation( Mobile from, int number, Item item )
		{
			if ( number == 1 && from.Map == Map.Trammel && from.Location.X >= 2216 && from.Location.X <= 2231 && from.Location.Y >= 1144 && from.Location.Y <= 1159 && from.Location.Z == 0 )  //코브
				return true;
			else if ( number == 2 && from.Map == Map.Trammel && from.Location.X >= 4558 && from.Location.X <= 4565 && from.Location.Y >= 1471 && from.Location.Y <= 1479 && from.Location.Z == 0 )
			{
				if( item is CottonSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 목화만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 3 && from.Map == Map.Trammel && from.Location.X >= 4569 && from.Location.X <= 4577 && from.Location.Y >= 1471 && from.Location.Y <= 1479 && from.Location.Z == 0 )
			{
				if( item is CottonSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 목화만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 4 && from.Map == Map.Trammel && from.Location.X >= 4407 && from.Location.X <= 4416 && from.Location.Y >= 1442 && from.Location.Y <= 1452 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 5 && from.Map == Map.Trammel && from.Location.X >= 4624 && from.Location.Y >= 1292 && from.Location.X <= 4631 && from.Location.Y <= 1299 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 6 && from.Map == Map.Trammel && from.Location.X >= 4624 && from.Location.Y >= 1304 && from.Location.X <= 4631 && from.Location.Y <= 1311 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 7 && from.Map == Map.Trammel && from.Location.X >= 4636 && from.Location.Y >= 1292 && from.Location.X <= 4642 && from.Location.Y <= 1299 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 8 && from.Map == Map.Trammel && from.Location.X >= 4636 && from.Location.Y >= 1304 && from.Location.X <= 4642 && from.Location.Y <= 1311 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 9 && from.Map == Map.Trammel && from.Location.X >= 1176 && from.Location.Y >= 1672 && from.Location.X <= 1207 && from.Location.Y <= 1695 && from.Location.Z == 0 )
			{
				if( item is CabbageSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 양배추, 순무만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 10 && from.Map == Map.Trammel && from.Location.X >= 1208 && from.Location.Y >= 1712 && from.Location.X <= 1239 && from.Location.Y <= 1735 && from.Location.Z == 0 )
			{
				if( item is CabbageSeed || item is TurnipSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 당근, 양파만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 11 && from.Map == Map.Trammel && from.Location.X >= 1208 && from.Location.Y >= 1592 && from.Location.X <= 1239 && from.Location.Y <= 1615 && from.Location.Z == 0 )
			{
				if( item is CarrotSeed || item is OnionSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 당근, 순무만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 12 && from.Map == Map.Trammel && from.Location.X >= 1136 && from.Location.Y >= 1560 && from.Location.X <= 1167 && from.Location.Y <= 1591 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 13 && from.Map == Map.Trammel && from.Location.X >= 1104 && from.Location.Y >= 1608 && from.Location.X <= 1135 && from.Location.Y <= 1639 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 14 && from.Map == Map.Trammel && from.Location.X >= 1120 && from.Location.Y >= 1176 && from.Location.X <= 1151 && from.Location.Y <= 1807 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 15 && from.Map == Map.Trammel && from.Location.X >= 1184 && from.Location.Y >= 1808 && from.Location.X <= 1215 && from.Location.Y <= 1839 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 16 && from.Map == Map.Trammel && from.Location.X >= 1216 && from.Location.Y >= 1872 && from.Location.X <= 1247 && from.Location.Y <= 1903 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 17 && from.Map == Map.Trammel && from.Location.X >= 560 && from.Location.Y >= 1232 && from.Location.X <= 575 && from.Location.Y <= 1247 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 18 && from.Map == Map.Trammel && from.Location.X >= 368 && from.Location.Y >= 1176 && from.Location.X <= 382 && from.Location.Y <= 1207 && from.Location.Z == 0 )
			{
				if( item is WheatSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 밀만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 19 && from.Map == Map.Trammel && from.Location.X >= 796 && from.Location.Y >= 2152 && from.Location.X <= 831 && from.Location.Y <= 2175 && from.Location.Z == 0 )
					return true;
			else if ( number == 20 && from.Map == Map.Trammel && from.Location.X >= 816 && from.Location.Y >= 2251 && from.Location.X <= 831 && from.Location.Y <= 2288 && from.Location.Z == 0 )
			{
				if( item is CarrotSeed || item is OnionSeed || item is CabbageSeed || item is TurnipSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 당근, 양파, 양배추, 순무만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 21 && from.Map == Map.Trammel && from.Location.X >= 816 && from.Location.Y >= 2344 && from.Location.X <= 831 && from.Location.Y <= 2367 && from.Location.Z == 0 )
			{
				if( item is CottonSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 목화만 심을 수 있습니다."); 
					return false;
				}
			}
			else if ( number == 22 && from.Map == Map.Trammel && from.Location.X >= 835 && from.Location.Y >= 2344 && from.Location.X <= 850 && from.Location.Y <= 2360 && from.Location.Z == 0 )
			{
				if( item is PumpkinSeed )
					return true;
				else
				{
					from.SendMessage("여기에는 호박만 심을 수 있습니다."); 
					return false;
				}
			}
			else
			{
				from.SendMessage("대여받은 공간이 아닙니다."); 
				return false;
			}
		}
		public static ArrayList CheckCrop( Point3D pnt, Map map, int range )
		{
			ArrayList crops = new ArrayList();

			IPooledEnumerable eable = map.GetItemsInRange( pnt, range );
			foreach ( Item crop in eable ) 
			{ 
				if ( ( crop != null ) && ( crop is Seeding ) )
					crops.Add( (Seeding)crop ); 
			} 
			eable.Free();

			return crops;
		}
	}
}


