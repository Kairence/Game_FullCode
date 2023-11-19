using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Commands.Generic;
using Server.Accounting;
using Server.Targeting;
using Server.Items;

namespace Server.Commands
{
	public class EquipPointInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "RankPoint", AccessLevel.Player, new CommandEventHandler( EquipPointInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "아이템 갈갈." )]
		public static void EquipPointInfo_OnCommand( CommandEventArgs e )
		{
			if( e.Mobile is PlayerMobile )
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				pm.CloseGump(typeof(EquipMeltingGump));
				pm.SendGump(new EquipMeltingGump(pm));

			}
		}
	}
}