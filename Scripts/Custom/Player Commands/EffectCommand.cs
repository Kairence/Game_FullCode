using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Commands
{
	public class EffectCheckCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "EffectCheck", AccessLevel.GameMaster, new CommandEventHandler( EffectCheckInfo_OnCommand ) );
		}

		[Usage( "Effect string" )]
		[Description( "����Ʈ üũ" )]
		public static void EffectCheckInfo_OnCommand( CommandEventArgs e )
		{
			string index = "";
			if( e.Arguments.Length == 0 )
				e.Mobile.SendMessage( String.Format("����Ʈ ��ȣ�� �־���մϴ�.") ); // Thy current bank balance is ~1_AMOUNT~ platinum and ~2_AMOUNT~ gold.
			else
			{
				index = e.Arguments[0];
				int number = Convert.ToInt32( index );
				e.Mobile.FixedParticles( 0, 10, 5, number, EffectLayer.LeftHand );
				e.Mobile.FixedParticles( 0, 10, 5, number, EffectLayer.RightHand );
			}
		}	
	}
}