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
	public class UserAutoFoodInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "Food", AccessLevel.Player, new CommandEventHandler( UserAutoFoodInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "자동 음식 먹기." )]
		public static void UserAutoFoodInfo_OnCommand( CommandEventArgs e )
		{
			if( e.Mobile is PlayerMobile )
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if( pm.AutoFood == null || e.Arguments.Length == 0 )
					e.Mobile.Target = new InternalTarget();
				else if( pm.AutoFood != null )
				{
					string index = e.Arguments[0];
					int number;
					bool isNum = Int32.TryParse(index, out number );
					if( !isNum )
						e.Mobile.SendMessage("잘못된 명령어 입니다. [Food 숫자 를 넣으세요.");
						
					if( number == 0 )
					{
						pm.AutoFood = null;
						pm.FoodPercent = 250;
					}
					else if( number < 0 || number > 1000 ) 
						e.Mobile.SendMessage( String.Format("0 ~ 1000 사이의 값을 넣어야 합니다.") );
					else
					{
						pm.FoodPercent = number;
						e.Mobile.SendMessage( String.Format("배고픔이 {0}% 일 때 음식을 먹도록 설정합니다.", number * 0.1) );
					}
				}
			}

		}

		private class InternalTarget : Target
		{
			public InternalTarget() :  base ( 8, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Food )
				{
					Food food = targeted as Food;
					if( from is PlayerMobile )
					{
						PlayerMobile pm = from as PlayerMobile;
						pm.AutoFood = food;
					}
				}
			}			
		}
	}
}