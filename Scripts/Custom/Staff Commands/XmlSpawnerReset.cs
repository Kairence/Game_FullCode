using System;
using Server;
using Server.Mobiles;
using Server.Commands.Generic;
using System.Collections.Generic;
using Server.Regions;

namespace Server.Commands
{
	public class XmlSpawnerResetInfoCommand
	{
		public static void Initialize()
		{
	      	CommandSystem.Register( "XSS", AccessLevel.GameMaster, new CommandEventHandler( XmlSpawnerResetInfo_OnCommand ) );
		}

		[Usage( "XmlSpawnerReset" )]
		[Description( "스포너 리셋." )]
		public static void XmlSpawnerResetInfo_OnCommand( CommandEventArgs e )
		{
			var list = new List<Item>();
			foreach ( Item i in World.Items.Values )
			{
				if ( i is XmlSpawner && i.Map == Map.Ilshenar )
				{
					XmlSpawner xs = i as XmlSpawner;
					
					if( xs.X >= 1752 && xs.Y >= 952 && xs.X <= 1864 && xs.Y <= 1000 ) //스파이더 던전 1층
						list.Add( i );
					if( xs.X >= 1480 && xs.Y >= 864 && xs.X <= 1528 && xs.Y <= 896 ) //스파이더 던전 2층
						list.Add( i );
				}
			}
			if( list.Count > 0 )
			{
				for ( int i = 0; i < list.Count; ++i )
				{
					XmlSpawner targeted = (XmlSpawner)list[i];
					targeted.HomeRange = 20;
					targeted.MaxDelay = targeted.MinDelay = TimeSpan.FromDays(1);
					targeted.Reset();
					targeted.Respawn();
				}
				e.Mobile.SendMessage("총 {0}개의 스포너를 수정했습니다.", list.Count);
			}
			else
				e.Mobile.SendMessage("수정할 내역이 없습니다.");
		}
	}
}