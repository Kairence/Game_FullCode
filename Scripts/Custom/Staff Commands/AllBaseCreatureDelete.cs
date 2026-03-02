using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Mobiles;

namespace Server.Commands
{
	public class AllBaseCreatureDeleteInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"ABCD",
				AccessLevel.GameMaster,
				new CommandEventHandler(AllBaseCreatureDeleteInfo_OnCommand)
			);
		}

		[Usage("AllBaseCreatureDelete")]
		[Description("��� ���� ���� �ڵ�.")]
		public static void AllBaseCreatureDeleteInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("���� ������ �����մϴ�!");
			int count = 0;
			var list = new List<Mobile>();
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m is BaseCreature)
				{
					BaseCreature bc = m as BaseCreature;
					//if( bc.AI != AIType.AI_Vendor || bc.ControlMaster == null )
					//{
					count++;
					list.Add(m);
					//}
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
