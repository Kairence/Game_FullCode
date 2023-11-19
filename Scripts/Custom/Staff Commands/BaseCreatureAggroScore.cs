using System;
using Server;
using Server.Mobiles;
using Server.Commands.Generic;
using System.Collections.Generic;
using Server.Targeting;

namespace Server.Commands
{
	public class BaseCreatureAggroScoreInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "AggroScore", AccessLevel.GameMaster, new CommandEventHandler( BaseCreatureAggroScoreInfo_OnCommand ) );
		}

		[Usage( "BaseCreatureDelete" )]
		[Description( "몬스터 삭제 코드." )]
		public static void BaseCreatureAggroScoreInfo_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new InternalTarget();
		}
		private class InternalTarget : Target
		{
			public InternalTarget() :  base ( 8, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if( targeted is BaseCreature )
				{
					BaseCreature bc = targeted as BaseCreature;

					for( int i = 0; i < 10000; i++)
					{
						if( bc.AggroMobile[i] == null )
							break;
						from.SendMessage("어그로 {0}, 점수 {1}", bc.AggroMobile[i].Name, bc.AggroScore[i] );
					}
				}
			}
		}
	}
}