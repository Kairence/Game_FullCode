using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Commands.Generic;
using Server.Accounting;
using Server.Targeting;

namespace Server.Commands
{
	public class YoungInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "Young", AccessLevel.Owner, new CommandEventHandler( YoungInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "보호 상태 설정" )]
		public static void YoungInfo_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;
			if( DateTime.Now > pm.YoungTime )
			{
				pm.YoungTime = DateTime.Now + TimeSpan.FromDays(1);
				if( pm.Young )
					pm.Young = false;
				else
					pm.Young = true;
			}
			else
			{
				TimeSpan time = pm.YoungTime - DateTime.Now;
				int Hour = time.Hours;
				int Minute = time.Minutes;
				int Second = time.Seconds;
				e.Mobile.SendMessage( Hour.ToString() + "시 " + Minute.ToString() + "분 " + Second.ToString() + "초 이후에 변경 가능합니다." );
			}
		}
	}
}