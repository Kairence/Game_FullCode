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
	public class UserStatInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "US", AccessLevel.GameMaster, new CommandEventHandler( UserStatInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "계정 금고 골드 확인." )]
		public static void UserStatInfo_OnCommand( CommandEventArgs e )
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
				if ( targeted is PlayerMobile )
				{
					PlayerMobile pm = targeted as PlayerMobile;
					from.CloseGump(typeof(CityPointGump));
					from.SendGump(new CityPointGump(pm));
				}
			}			
		}
	}
	public class StatInfoCommand
	{
		public static void Initialize()
		{
	      		CommandSystem.Register( "Stat", AccessLevel.Player, new CommandEventHandler( StatInfo_OnCommand ) );
		}

		[Usage( "Stat [second order]" )]
		[Description( "능력치 스텟창 열기." )]
		public static void StatInfo_OnCommand( CommandEventArgs e )
		{
			var sub = "";
			if (e.Length > 0 )
				sub = e.GetString(0);
			if( sub != "" )
				sub = sub.ToLower();
			if( e.Mobile is PlayerMobile )
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if( sub == "" )
				{
					e.Mobile.CloseGump(typeof(CityPointGump));
					e.Mobile.SendGump(new CityPointGump(pm));
				}
				else if( sub == "peace" )
				{
					e.Mobile.CloseGump(typeof(GoldPointGump));
					e.Mobile.SendGump(new GoldPointGump(pm));
				}
				else if( sub == "war")
				{
					e.Mobile.CloseGump(typeof(SilverPointGump));
					e.Mobile.SendGump(new SilverPointGump(pm));
				}
				else if( sub == "action")
				{
					e.Mobile.CloseGump(typeof(EquipPointGump));
					e.Mobile.SendGump(new EquipPointGump(pm));
				}
				else if( sub == "artifact")
				{
					e.Mobile.CloseGump(typeof(ArtifactPointGump));
					e.Mobile.SendGump(new ArtifactPointGump(pm));
				}
				else if( sub == "skill")
				{
					e.Mobile.CloseGump(typeof(SkillPointGump));
					e.Mobile.SendGump(new SkillPointGump(pm));
				}
			}
		}	
	}
}