using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Mobiles;

namespace Server.Commands
{
	public class BaseCreatureDeleteInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"BCD",
				AccessLevel.GameMaster,
				new CommandEventHandler(BaseCreatureDeleteInfo_OnCommand)
			);
		}

		[Usage("BaseCreatureDelete")]
		[Description("���� ���� �ڵ�.")]
		public static void BaseCreatureDeleteInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("���� ������ �����մϴ�!");
			int count = 0;
			var list = new List<Mobile>();
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m is BaseCreature)
				{
					BaseCreature bc = m as BaseCreature;
					if (bc.ControlMaster != null || bc.AI == AIType.AI_Vendor)
						continue;
					else
					{
						count++;
						list.Add(m);
					}
				}
			}
			for (int i = 0; i < list.Count; ++i)
			{
				Mobile tar = (Mobile)list[i];
				tar.Delete();
			}
			e.Mobile.SendMessage("�� {0}������ ���͸� �����߽��ϴ�.", count);
		}
	}
}
