using System;
using Server;
using Server.Commands.Generic;
using Server.Mobiles;
using Server.Network;

namespace Server.Commands
{
	public class LoopInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("Loop", AccessLevel.Player, new CommandEventHandler(LoopInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("�ݺ� ���ɾ�.")]
		public static void LoopInfo_OnCommand(CommandEventArgs e)
		{
			if (e.Mobile is PlayerMobile)
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if (pm.Loop)
				{
					pm.Loop = false;
					e.Mobile.SendMessage("�ݺ� �۾��� �ߴ��մϴ�");
				}
				else
				{
					pm.Loop = true;
					if (e.Arguments.Length == 0)
					{
						e.Mobile.SendMessage("�ݺ� �۾��� �����մϴ�");
						pm.LoopCount = 50000;
					}
					else
					{
						string index = e.Arguments[0];
						int number;
						bool isNum = Int32.TryParse(index, out number);
						if (!isNum)
							e.Mobile.SendMessage("�߸��� ���ɾ� �Դϴ�. [Loop ���� �� ��������.");

						if (number == 0)
						{
							pm.LastTarget = null;
							e.Mobile.SendMessage("���� Ÿ���� �����մϴ�");
						}
						else if (number < 0 || number > 50000)
							e.Mobile.SendMessage("1 ~ 50000 ������ ���� �־��ּ���.");
						else
						{
							pm.LoopCount = number;
							e.Mobile.SendMessage("�ݺ� �۾��� {0} ȸ �����մϴ�", number);
						}
					}
				}
			}
		}
	}
}
