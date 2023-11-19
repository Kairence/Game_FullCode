using System;
using Server;
using Server.Mobiles;
using Server.Commands.Generic;
using System.Collections.Generic;

namespace Server.Commands
{
	public class BaseCreatureDeleteInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "BCD", AccessLevel.GameMaster, new CommandEventHandler( BaseCreatureDeleteInfo_OnCommand ) );
		}

		[Usage( "BaseCreatureDelete" )]
		[Description( "몬스터 삭제 코드." )]
		public static void BaseCreatureDeleteInfo_OnCommand( CommandEventArgs e )
		{
			e.Mobile.SendMessage("몬스터 삭제를 시작합니다!");
			int count = 0;
			var list = new List<Mobile>();
			foreach ( Mobile m in World.Mobiles.Values )
			{
				if ( m is BaseCreature )
				{
					BaseCreature bc = m as BaseCreature;
					if( bc.ControlMaster != null || bc.AI == AIType.AI_Vendor )
						continue;
					else
					{
						count++;
						list.Add( m );
					}
				}
			}
			for ( int i = 0; i < list.Count; ++i )
			{
				Mobile tar = (Mobile)list[i];
				tar.Delete();
			}
			e.Mobile.SendMessage("총 {0}마리의 몬스터를 삭제했습니다.", count);
		}
	}
}