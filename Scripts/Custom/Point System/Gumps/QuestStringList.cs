using System;
using Server.Mobiles;
namespace Server
{
	public class QuestStringList
	{
		public string TownPoint( PlayerMobile pm )
		{
			string town = "";
			//브리튼 : 
			if( pm.Map == Map.Trammel && pm.Location.X == 1479 && pm.Location.Y >= 1611 && pm.Location.Y <= 1612 )
				town = "브리튼 치료소";
			else if( pm.Map == Map.Trammel && pm.Location.X == 1419 && pm.Location.Y >= 1596 && pm.Location.Y <= 1597 )
				town = "브리튼 공공 도서관";
			else if( pm.Map == Map.Trammel && pm.Location.X == 1419 && pm.Location.Y >= 1596 && pm.Location.Y <= 1597 )
				town = "망치와 모루";
			else if( pm.Map == Map.Trammel && pm.Location.X >= 1455 && pm.Location.X <= 1456 && pm.Location.Y == 1560 )
				town = "브리튼 브리티시 왕의 음악 대학";
			
			
			return town;
			
		}
		
		public bool MoveCheck( PlayerMobile pm, int list )
		{
			bool success = false;
			//switch(list)
			//{
			//	if( pm.Map == Map.Trammel && pm.Location.X == 1479
			//	
			//	
			//}
			return success;
			
		}
		public string MoveString( int list )
		{
			string talk = "";
			switch(list)
			{
				case 1: talk = "자네 혹시 '브리튼 치료소'로 가서\n 이 붕대를 건내줄 수 있겠나? 무사히 도착하면 즉시 보상을 받을 수 있을거야";
								break;
				
				case 2: talk = "급한데... 어쩐다. 거기 자네 시간이 있나? '브리튼 공공 도서관' 무사히 도착하면 즉시 보상을 받을 수 있을거야";
								break;
				
				
				
				
				
			}
			return talk;
			
		}
		
	}
}
