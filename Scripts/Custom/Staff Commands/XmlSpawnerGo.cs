using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Mobiles;

namespace Server.Commands
{
	public class XmlSpawnerGoInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("XSG", AccessLevel.GameMaster, new CommandEventHandler(XmlSpawnerGoInfo_OnCommand));
		}

		[Usage("XmlSpawnerGo Outdoorsnumber")]
		[Description("������ ���� �ڵ�.")]
		public static void XmlSpawnerGoInfo_OnCommand(CommandEventArgs e)
		{
			//e.Mobile.SendMessage("������ ������ �����մϴ�!");
			string index = "Outdoors#";
			if (e.Arguments.Length == 0)
				e.Mobile.SendMessage(String.Format("������ �̸��� �־���մϴ�.")); // Thy current bank balance is ~1_AMOUNT~ platinum and ~2_AMOUNT~ gold.
			else
			{
				index += e.Arguments[0];
				bool count = false;

				foreach (Item i in World.Items.Values)
				{
					if (i is XmlSpawner && i.Name == index)
					{
						Map map = i.Map;
						Point3D loc = new Point3D(i.X, i.Y, i.Z);
						e.Mobile.MoveToWorld(loc, map);
						count = true;
						break;
					}
				}
				if (!count)
					e.Mobile.SendMessage("�ش� �̸��� �����ʰ� �����ϴ�!");
			}
		}
	}
}
