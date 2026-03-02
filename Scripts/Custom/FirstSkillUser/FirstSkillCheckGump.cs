using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

//using Server.Engines.CityLoyalty;

namespace Server.Gumps
{
	public class FirstSkillCheckGump : Gump
	{
		//private const int LabelColor = 0x7FFF;
		//private const int LabelHue = 0x481;

		private string[] Name =
		{
			"���ݼ�",
			"�غ���",
			"��������",
			"������ ����",
			"�����",
			"���м�",
			"����",
			"��������",
			"Ȱ���ۼ�",
			"��ȭ����",
			"�߿���",
			"�����",
			"�������ۼ�",
			"�丮",
			"���Ű���",
			"����ȭ��",
			"������",
			"ġ���",
			"����",
			"������",
			"���",
			"�����ϱ�",
			"���߿���",
			"��ϼ�",
			"�ڹ��� ����",
			"������",
			"���� ����",
			"����",
			"���ĺ���",
			"���ǿ���",
			"�ߵ���",
			"�ü�",
			"��ȥ��ȭ",
			"��ġ��",
			"�����",
			"����̱�",
			"�� ����",
			"���� ���",
			"������",
			"������",
			"�˼�",
			"�б��",
			"���",
			"������",
			"�����",
			"ä��",
			"����",
			"�����̵�",
			"��������",
			"���ɼ�",
			"����",
			"��絵",
			"���絵",
			"���ڼ�",
			"�ֹ�����",
			"�ź��",
			"�Ӻ���",
			"������",
		};

		private FirstSkillCheck m_firstskill = null;
		private Mobile m_From = null;

		public FirstSkillCheckGump(Mobile from, FirstSkillCheck firstskill)
			: base(50, 50)
		{
			m_From = from;
			m_firstskill = firstskill;
			from.CloseGump(typeof(FirstSkillCheckGump));

			AddBackground(0, 0, 500, 480, 5054);

			AddHtml(10, 10, 250, 20, "��ų üũ �ý���", false, false); // <CENTER>HOUSE
			//AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", dungeon.Death[0]), false, false);

			AddHtml(50, 40, 225, 20, "��ų", false, false); // House Description
			AddHtml(275, 40, 75, 20, "����", false, false); // Storage
			//AddHtml(350, 40, 150, 20, "���", false, false); // Lockdowns
			int y = 60;
			for (int i = 0; i < Name.Length; i++)
			{
				//�̸�
				AddHtml(50, y + i * 20, 225, 20, Name[i], false, false);
				//����
				string name = "����";
				if (firstskill.Skill[i])
				{
					name = firstskill.User[i];
					AddButton(10, y + i * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				}
				AddHtml(350, y + i * 20, 200, 16, name, false, false);
				//���
				//AddHtml(350, y + i * 20, 200, 16, String.Format("{0:#,###}", dungeon.Death[i + 1]), false, false);
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_From.CheckAlive() || info.ButtonID == 0)
				return;

			m_firstskill.Skill[info.ButtonID - 1] = false;
		}
	}
}
