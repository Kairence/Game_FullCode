using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
	public class HitLocationManager
	{
		/// <summary>
		/// �÷��̾� �ǰ� �� �� ������ Ȯ���� ���� �ǰ� ������ �����մϴ�.
		/// (��: 10%, �Ӹ�: 10%, ��: 5%, ���: 15%, ����: 25%, ����: 35%)
		/// </summary>
		public static int GetRandomLocation()
		{
			double roll = Utility.RandomDouble();

			if (roll < 0.10)
				return 1; // �� (10%)
			else if (roll < 0.20)
				return 2; // �Ӹ� (10%)
			else if (roll < 0.25)
				return 3; // �� (5%)
			else if (roll < 0.40)
				return 4; // ��� (15%)
			else if (roll < 0.65)
				return 5; // ���� (25%)
			else
				return 6; // ���� (35%)
		}

		/// <summary>
		/// ������ ġ��Ÿ Ȯ�� ���ʽ��� ��ȯ�մϴ�.
		/// </summary>
		public static double GetCritChanceBonus(int location)
		{
			switch (location)
			{
				case 1:
					return 0.10; // �� 10%
				case 2:
					return 0.25; // �Ӹ� 25%
				case 3:
					return 0.50; // �� 50%
				case 4:
					return 0.15; // ��� 15%
				case 5:
					return 0.20; // ���� 20%
				case 6:
					return 0.20; // ���� 20%
				default:
					return 0.0;
			}
		}

		/// <summary>
		/// ������ ġ��Ÿ �߰� ������ ���ʽ��� ��ȯ�մϴ�.
		/// </summary>
		public static double GetCritDamageBonus(int location)
		{
			switch (location)
			{
				case 2:
					return 0.50; // �Ӹ� +50%
				case 3:
					return 1.50; // �� +150%
				default:
					return 0.0; // ������ 0%
			}
		}

		/// <summary>
		/// ���� ��ȣ�� ���ڿ��� ��ȯ�մϴ�. (�����/�޽�����)
		/// </summary>
		public static string GetLocationName(int location)
		{
			switch (location)
			{
				case 0:
					return "����";
				case 1:
					return "��";
				case 2:
					return "�Ӹ�";
				case 3:
					return "��";
				case 4:
					return "���";
				case 5:
					return "����";
				case 6:
					return "����";
				default:
					return "�� �� ����";
			}
		}
	}
}
