using System;
using Server;
using Server.Mobiles;
using Server.Commands.Generic;
using System.Collections.Generic;

namespace Server.Commands
{
	public class XmlSpawnerGoInfoCommand
	{
		public static void Initialize()
		{
	      	CommandSystem.Register( "XSG", AccessLevel.GameMaster, new CommandEventHandler( XmlSpawnerGoInfo_OnCommand ) );
		}

		[Usage( "XmlSpawnerGo Outdoorsnumber" )]
		[Description( "스포너 삭제 코드." )]
		public static void XmlSpawnerGoInfo_OnCommand( CommandEventArgs e )
		{
			//e.Mobile.SendMessage("스포너 삭제를 시작합니다!");
			string index = "Outdoors#";
			if( e.Arguments.Length == 0 )
				e.Mobile.SendMessage( String.Format("스포너 이름을 넣어야합니다.") ); // Thy current bank balance is ~1_AMOUNT~ platinum and ~2_AMOUNT~ gold.
			else
			{
				index += e.Arguments[0];
				bool count = false;

				foreach ( Item i in World.Items.Values )
				{
					if ( i is XmlSpawner && i.Name == index )
					{
						Map map = i.Map;
						Point3D loc = new Point3D( i.X, i.Y, i.Z);
						e.Mobile.MoveToWorld( loc, map );
						count = true;
						break;
					}
				}
				if( !count )
					e.Mobile.SendMessage("해당 이름의 스포너가 없습니다!");
			}
		}
	}
}