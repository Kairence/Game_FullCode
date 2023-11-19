using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Commands.Generic;

namespace Server.Commands
{
	public class LoopInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "Loop", AccessLevel.Player, new CommandEventHandler( LoopInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "반복 명령어." )]
		public static void LoopInfo_OnCommand( CommandEventArgs e )
		{
			if( e.Mobile is PlayerMobile )
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if( pm.Loop )
				{
					pm.Loop = false;
					e.Mobile.SendMessage("반복 작업을 중단합니다");
				}
				else
				{
					pm.Loop = true;
					if( e.Arguments.Length == 0 )
					{
						e.Mobile.SendMessage("반복 작업을 시작합니다");
						pm.LoopCount = 50000;
					}
					else
					{
						string index = e.Arguments[0];
						int number;
						bool isNum = Int32.TryParse(index, out number );
						if( !isNum )
							e.Mobile.SendMessage("잘못된 명령어 입니다. [Loop 숫자 를 넣으세요.");
							
						if( number == 0 )
						{
							pm.LastTarget = null;
							e.Mobile.SendMessage("루프 타겟을 삭제합니다");
						}
						else if( number < 0 || number > 50000 )
							e.Mobile.SendMessage("1 ~ 50000 사이의 값을 넣어주세요.");
						else
						{
							pm.LoopCount = number;
							e.Mobile.SendMessage("반복 작업을 {0} 회 시작합니다", number);
						}
					}
				}
			}
		}	
	}
}