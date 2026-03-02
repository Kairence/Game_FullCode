using System;
using Server.Mobiles;

namespace Server
{
	public class QuestStringList
	{
		public string TownPoint(PlayerMobile pm)
		{
			string town = "";
			//�긮ư :
			if (pm.Map == Map.Trammel && pm.Location.X == 1479 && pm.Location.Y >= 1611 && pm.Location.Y <= 1612)
				town = "�긮ư ġ���";
			else if (pm.Map == Map.Trammel && pm.Location.X == 1419 && pm.Location.Y >= 1596 && pm.Location.Y <= 1597)
				town = "�긮ư ���� ������";
			else if (pm.Map == Map.Trammel && pm.Location.X == 1419 && pm.Location.Y >= 1596 && pm.Location.Y <= 1597)
				town = "��ġ�� ���";
			else if (pm.Map == Map.Trammel && pm.Location.X >= 1455 && pm.Location.X <= 1456 && pm.Location.Y == 1560)
				town = "�긮ư �긮Ƽ�� ���� ���� ����";

			return town;
		}

		public bool MoveCheck(PlayerMobile pm, int list)
		{
			bool success = false;
			//switch(list)
			//{
			//	if( pm.Map == Map.Trammel && pm.Location.X == 1479
			//
			//
			//}
			return success;
		}

		public string MoveString(int list)
		{
			string talk = "";
			switch (list)
			{
				case 1:
					talk =
						"�ڳ� Ȥ�� '�긮ư ġ���'�� ����\n �� �ش븦 �ǳ��� �� �ְڳ�? ������ �����ϸ� ��� ������ ���� �� �����ž�";
					break;

				case 2:
					talk =
						"���ѵ�... ��¾��. �ű� �ڳ� �ð��� �ֳ�? '�긮ư ���� ������' ������ �����ϸ� ��� ������ ���� �� �����ž�";
					break;
			}
			return talk;
		}
	}
}
