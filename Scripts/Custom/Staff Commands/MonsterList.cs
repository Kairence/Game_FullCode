using System;
using Server;
using Server.Commands;
using Server.Misc; // MonsterDropHandler�� Gump�� �ִ� ���ӽ����̽�

namespace Server.Commands
{
	public class MonsterDropHandlerCommand
	{
		public static void Initialize()
		{
			// [mlt ���ɾ� ���: ���Ӹ�����(GameMaster) ���� �ʿ�
			CommandSystem.Register("mlt", AccessLevel.GameMaster, new CommandEventHandler(MLT_OnCommand));
		}

		[Usage("mlt")]
		[Description("MonsterDropHandler�� ��ϵ� ��� ���̺� ���� ����� ������ Ȯ���մϴ�.")]
		private static void MLT_OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;

			if (from != null && !from.Deleted)
			{
				// ù ��° ������(0)���� ������ �����ϴ�.
				from.SendGump(new MonsterDropHandlerGump(from, 0));
				from.SendMessage(0x482, "���� ��� �ڵ鷯 ����Ʈ�� �ҷ��Խ��ϴ�.");
			}
		}
	}
}
