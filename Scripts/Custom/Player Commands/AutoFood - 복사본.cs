using System;
using Server;
using Server.Accounting;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Commands
{
	public class AllMoveInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("AMI", AccessLevel.Player, new CommandEventHandler(AllMoveInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("������ �̵���Ű��")]
		public static void AllMoveInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("��, �ڽ��� ����, ���࿡�� �ű� �������� Ŭ���ϼ���");
			e.Mobile.Target = new InternalTarget();
		}

		//List<Item> select_item = null;
		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item)
				{
					Item target_item = targeted as Item;
					BankBox box = from.FindBankNoCreate();

					BaseHouse house = null;
					Container housecontainer = null;
					if (target_item.Parent is Container)
						housecontainer = target_item.Parent as Container;

					if (housecontainer != null)
						house = BaseHouse.FindHouseAt(housecontainer);
					Container pack = from.Backpack;
					if (box != null && target_item.IsChildOf(box))
					{
						//box.FindItemsByType<target_item>();
						from.SendMessage("�� �����۵��� ���� �ű�ðڽ��ϱ�?");
					}
					else if (pack != null)
					{
						//pack.FindItemsByType<target_item>();
						from.SendMessage("�� �����۵��� ���� �ű�ðڽ��ϱ�?");
					}
					else if (house != null && house.IsOwner(from)) { }
					else
					{
						from.SendMessage("��, �ڽ��� ����, ����ȿ� �ִ� �����۸� ������ �� �ֽ��ϴ�.");
					}
				}
				else
				{
					from.SendMessage("�����۸� �����մϴ�!");
				}
			}
		}
	}
}
