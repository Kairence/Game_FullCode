using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Mobiles;

namespace Server.Commands
{
	public class XmlSpawnerDeleteInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"XSD",
				AccessLevel.GameMaster,
				new CommandEventHandler(XmlSpawnerDeleteInfo_OnCommand)
			);
		}

		[Usage("XmlSpawnerDelete")]
		[Description("������ ���� �ڵ�.")]
		public static void XmlSpawnerDeleteInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("������ ������ �����մϴ�!");
			int count = 0;
			var list = new List<Item>();
			foreach (Item i in World.Items.Values)
			{
				if ((i is XmlSpawner || i is Spawner) && i.Map != Map.Trammel)
				{
					count++;
					list.Add(i);
				}
			}
			for (int i = 0; i < list.Count; ++i)
			{
				Item tar = (Item)list[i];
				tar.Delete();
			}
			e.Mobile.SendMessage("�� {0}���� �����ʸ� �����߽��ϴ�.", count);
		}
	}
}
