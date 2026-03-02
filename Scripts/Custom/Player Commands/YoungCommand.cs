using System;
using Server;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Commands
{
	public class YoungInfoCommand
	{
		public static void Initialize()
		{
			// ���ɾ ������ ����� �� �ֵ��� Player �������� ����ϵ�, ���ο��� ������ �и��մϴ�.
			CommandSystem.Register("Young", AccessLevel.Player, new CommandEventHandler(YoungInfo_OnCommand));
		}

		[Usage("Young")]
		[Description("���� ĳ���� ���¸� �����ϰų� �����մϴ�.")]
		public static void YoungInfo_OnCommand(CommandEventArgs e)
		{
			PlayerMobile from = e.Mobile as PlayerMobile;
			if (from == null)
				return;

			// 1. ������(GameMaster �̻�)�� ����� ��� -> Ÿ���� ���
			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				from.SendMessage("����(Young) ���¸� ������ ĳ���͸� �����ϼ���.");
				from.Target = new YoungTarget();
			}
			// 2. �Ϲ� �÷��̾ ����� ��� -> ���� ���� ���� ���
			else
			{
				if (!from.Young)
				{
					from.SendMessage(0x22, "����� ���� ĳ���� ���°� �ƴմϴ�.");
					return;
				}

				from.Young = false;
				from.SendMessage(0x481, "���� ĳ���� ���¸� �����ϼ̽��ϴ�.");

				if (SeasonController.IsSeasonActive())
				{
					// ����� ��ǥ ��� pm.PlayerMove�� ȣ���Ͽ�
					// ������ ������ SaveTown ��ġ�� Ʈ��� ������ �̵���ŵ�ϴ�.
					from.PlayerMove(false);
					from.SendMessage(0x22, "���� ����� ���� ������ ���� ����(Ʈ���)�� �̼۵Ǿ����ϴ�.");
				}
			}
		}

		// ������ ���� Ÿ�� Ŭ����
		private class YoungTarget : Target
		{
			public YoungTarget()
				: base(12, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is PlayerMobile pm)
				{
					pm.Young = !pm.Young; // ���� ���� (ON/OFF)

					from.SendMessage(
						0x481,
						"{0} ĳ������ ����(Young) ���¸� {1}�� �����߽��ϴ�.",
						pm.Name,
						pm.Young ? "ON" : "OFF"
					);

					pm.SendMessage(0x481, "�����ڿ� ���� ���� ĳ���� ���°� {0} �Ǿ����ϴ�.", pm.Young ? "Ȱ��ȭ" : "����");
				}
				else
				{
					from.SendMessage("�÷��̾� ĳ���͸� ���� �����մϴ�.");
				}
			}
		}
	}
}
