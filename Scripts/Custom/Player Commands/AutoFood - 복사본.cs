using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Commands.Generic;
using Server.Accounting;
using Server.Targeting;
using Server.Items;
using Server.Multis;

namespace Server.Commands
{
	public class AllMoveInfoCommand
	{
		public static void Initialize()
		{
      		CommandSystem.Register( "AMI", AccessLevel.Player, new CommandEventHandler( AllMoveInfo_OnCommand ) );
		}

		[Usage( "Status" )]
		[Description( "아이템 이동시키기" )]
		public static void AllMoveInfo_OnCommand( CommandEventArgs e )
		{
			e.Mobile.SendMessage("집, 자신의 가방, 은행에서 옮길 아이템을 클릭하세요");
			e.Mobile.Target = new InternalTarget();
		}

		//List<Item> select_item = null;
		private class InternalTarget : Target
		{
			public InternalTarget() :  base ( 8, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if( targeted is Item )
				{
					Item target_item = targeted as Item;
					BankBox box = from.FindBankNoCreate();
					
					BaseHouse house = null;
					Container housecontainer = null;
					if( target_item.Parent is Container )
						housecontainer = target_item.Parent as Container;

					if( housecontainer != null )
						house = BaseHouse.FindHouseAt(housecontainer);
					Container pack = from.Backpack;
					if( box != null && target_item.IsChildOf(box) ) 
					{
						//box.FindItemsByType<target_item>();
						from.SendMessage("이 아이템들을 어디로 옮기시겠습니까?");
						
					}
					else if( pack != null )
					{
						//pack.FindItemsByType<target_item>();
						from.SendMessage("이 아이템들을 어디로 옮기시겠습니까?");
						
					}
					else if( house != null && house.IsOwner(from)  )
					{}
					else
					{
						from.SendMessage("집, 자신의 가방, 은행안에 있는 아이템만 선택할 수 있습니다.");
					}
				}
				else
				{
					from.SendMessage("아이템만 가능합니다!");
				}
			}			
		}
	}
}