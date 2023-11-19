using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Commands.Generic;
using Server.Accounting;
using Server.Targeting;

namespace Server.Commands
{
	public class UserGoldInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "UG", AccessLevel.GameMaster, new CommandEventHandler( UserGoldInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "계정 금고 골드 확인." )]
		public static void UserGoldInfo_OnCommand( CommandEventArgs e )
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
				if ( targeted is PlayerMobile && AccountGold.Enabled && ((Mobile)targeted).Account != null )
				{
					from.SendMessage( String.Format("백금 {0:#,0}, 금화 \t{1:#,0}", ((Mobile)targeted).Account.TotalPlat, ((Mobile)targeted).Account.TotalGold) ); // Thy current bank balance is ~1_AMOUNT~ platinum and ~2_AMOUNT~ gold.
				}
			}			
		}
	}
	public class GoldInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "Gold", AccessLevel.Player, new CommandEventHandler( GoldInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "계정 금고 골드 확인." )]
		public static void GoldInfo_OnCommand( CommandEventArgs e )
		{
			if ( AccountGold.Enabled && e.Mobile.Account != null )
				e.Mobile.SendMessage( String.Format("백금 {0:#,0}, 금화 \t{1:#,0}", e.Mobile.Account.TotalPlat, e.Mobile.Account.TotalGold) ); // Thy current bank balance is ~1_AMOUNT~ platinum and ~2_AMOUNT~ gold.
		}	
	}
}