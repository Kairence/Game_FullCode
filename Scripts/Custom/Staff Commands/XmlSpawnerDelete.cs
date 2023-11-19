using System;
using Server;
using Server.Mobiles;
using Server.Commands.Generic;
using System.Collections.Generic;

namespace Server.Commands
{
	public class XmlSpawnerDeleteInfoCommand
	{
		public static void Initialize()
		{
	      	CommandSystem.Register( "XSD", AccessLevel.GameMaster, new CommandEventHandler( XmlSpawnerDeleteInfo_OnCommand ) );
		}

		[Usage( "XmlSpawnerDelete" )]
		[Description( "스포너 삭제 코드." )]
		public static void XmlSpawnerDeleteInfo_OnCommand( CommandEventArgs e )
		{
			e.Mobile.SendMessage("스포너 삭제를 시작합니다!");
			int count = 0;
			var list = new List<Item>();
			foreach ( Item i in World.Items.Values )
			{
				if (( i is XmlSpawner || i is Spawner ) && i.Map != Map.Trammel )
				{
					count++;
					list.Add( i );
				}
			}
			for ( int i = 0; i < list.Count; ++i )
			{
				Item tar = (Item)list[i];
				tar.Delete();
			}
			e.Mobile.SendMessage("총 {0}개의 스포너를 삭제했습니다.", count);
		}
	}
}