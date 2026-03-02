using System;
using System.Collections.Generic;
using Server;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;

//using Server.Engines.CityLoyalty;

namespace Server.Gumps
{
	public class CityPointGump : Gump
	{
		//private const int LabelColor = 0x7FFF;
		//private const int LabelColorDisabled = 0x4210;
		private readonly PlayerMobile m_pm;

		public CityPointGump(PlayerMobile pm)
			: base(50, 50)
		{
			AddHtml(50, 7, 345, 20, "���� �ý���", false, false); // City Loyalty

			m_pm = pm;

			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			//pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

			AddPage(0);

			Account acc = pm.Account as Account;

			AddBackground(0, 0, 280, 220, 5054);

			AddHtml(10, 10, 250, 20, "���� �ý���", false, false);

			AddButton(10, 190, 4017, 4019, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 190, 150, 20, 3000363, false, false); // Close

			AddPage(1);

			AddButton(10, 40, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtml(45, 40, 105, 20, "���� ���� ����Ʈ:", false, false);
			AddHtml(150, 40, 110, 16, String.Format("{0:#,###}", pm.GoldPoint[49]), false, false);

			AddButton(10, 60, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtml(45, 60, 105, 20, "���� ����Ʈ:", false, false);
			AddHtml(150, 60, 110, 16, String.Format("{0:#,###}", pm.SilverPoint[0]), false, false);

			AddButton(10, 80, 4005, 4007, 3, GumpButtonType.Reply, 0);
			AddHtml(45, 80, 105, 20, "���� ����Ʈ", false, false);
			AddHtml(150, 80, 110, 16, String.Format("{0:#,###}", acc.Point[0]), false, false);
			//AddHtml(150, 40, 110, 16, "), false, false);

			AddButton(10, 100, 4005, 4007, 4, GumpButtonType.Reply, 0);
			AddHtml(45, 100, 105, 20, "���� ����Ʈ:", false, false);
			AddHtml(150, 40, 110, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

			AddButton(10, 120, 4005, 4007, 5, GumpButtonType.Reply, 0);
			AddHtml(45, 120, 105, 20, "��ų ����Ʈ", false, false);
			//AddHtml(150, 40, 110, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

			AddHtml(10, 150, 150, 20, "����: ", false, false); // Fame: ~1_AMT~
			AddHtml(10, 170, 150, 20, "ī����: ", false, false); // Karma: ~1_AMT~}

			AddHtml(65, 150, 150, 20, pm.Fame.ToString(), false, false); // Fame: ~1_AMT~
			AddHtml(65, 170, 150, 20, pm.Karma.ToString(), false, false); // Fame: ~1_AMT~
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive())
				return;

			switch (info.ButtonID)
			{
				case 1:
				{
					m_pm.SendGump(new GoldPointGump(m_pm));
					break;
				}
				case 2:
				{
					m_pm.SendGump(new SilverPointGump(m_pm));
					break;
				}
				case 3:
				{
					m_pm.SendGump(new EquipPointGump(m_pm));
					break;
				}
				case 4:
				{
					m_pm.SendGump(new ArtifactPointGump(m_pm));
					break;
				}
				case 5:
				{
					m_pm.SendGump(new SkillPointGump(m_pm));
					break;
				}
			}
		}
	}

	#region ä��/����
	public class GoldPointGump : Gump
	{
		private string[] GrawName =
		{
			"ä�� ���� ����",
			"���� ä�� ����",
			"ä�� ���� ����",
			"ä�� ��ȯ ����",
			"���� ���� ����",
			"���� ���� ����",
			"���� ���� ����",
			"���� ���� ����",
		};

		//private int[] MaxLevel = { 40, 75, 25, 50, 50, 70, 100, 100 };
		//private int[] BuffOption = { 100, 300, 500, 1000, 2000, 5000, 100, 250, 500, 1000, 2000 };

		//private int[] SkillCount = { 3, 36, 19, 10, 38, 56};

		/*
		private int pointCal( int number, int level )
		{
			int point = 1000000000;
			if( number == 1 )
				point = ( level + 1 ) * 50;
			else if( number == 2 )
				point = ( level + 1 ) * 250;
			else if( number == 3 || number == 4 )
				point = ( level + 1 ) * ( level + 1 ) * 10000;
			else
				point = BuffOption[level];
			return point;
		}
		*/
		readonly int Max_Level = 100;

		private PlayerMobile m_pm;
		private int m_Harvestpoint = 0;
		private int m_Craftpoint = 0;

		public GoldPointGump(PlayerMobile pm)
			: base(50, 50)
		{
			/*
			49�� : ���� ���� ����ġ
			0�� : ä�� ����ġ
			1�� : ä�� ����Ʈ
			2�� : ä�� ���� ����
			3�� : ���� ä�� ����
			4�� : ä�� ���� ����
			5�� : ä�� ��ȯ ����
			10�� : ���� ����ġ
			11�� : ���� ����Ʈ
			12�� : ���� ���� ����
			13�� : ���� ���� ����
			14�� : ���� ���� ����
			15�� : ���� ���� ����
			*/
			pm.CloseGump(typeof(CityPointGump));
			pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;

			int HarvestQuest = 0;
			for (int i = 0; i < 9; i++)
			{
				if (pm.QuestCheck[i])
				{
					HarvestQuest += 5;
					if (i == 8)
						HarvestQuest += 5;
				}
			}

			m_Harvestpoint = Misc.Util.Level(pm.GoldPoint[0]) - pm.GoldPoint[1] + HarvestQuest;
			m_Craftpoint = Misc.Util.Level(pm.GoldPoint[10]) - pm.GoldPoint[11];
			//pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

			AddPage(0);

			AddBackground(0, 0, 425, 600, 5054);

			//AddImageTiled(10, 10, 500, 150, 2624);
			//AddAlphaRegion(10, 10, 500, 150);

			AddHtml(10, 10, 250, 20, "ä�� ����ġ", false, false); // <CENTER>HOUSE
			AddHtml(
				130,
				10,
				200,
				16,
				pm.GoldPoint[0] > 0 ? (String.Format("{0:#,###}", pm.GoldPoint[0])) : "0",
				false,
				false
			);

			AddHtml(10, 60, 250, 20, "���� ����Ʈ", false, false); // <CENTER>HOUSE
			AddHtml(
				130,
				60,
				200,
				16,
				m_Harvestpoint > 0 ? String.Format("{0:#,###}", m_Harvestpoint) : "0",
				false,
				false
			);

			AddHtml(10, 80, 100, 20, "���� ����", false, false); // <CENTER>HOUSE
			AddHtml(
				130,
				80,
				40,
				16,
				Misc.Util.Level(pm.GoldPoint[0]) >= Misc.Util.MaxLevel
					? Misc.Util.MaxLevel.ToString()
					: Misc.Util.Level(pm.GoldPoint[0]).ToString(),
				false,
				false
			);

			AddHtml(10, 110, 130, 20, "���� ����ġ", false, false); // <CENTER>HOUSE
			AddHtml(
				130,
				110,
				250,
				16,
				Misc.Util.Level(pm.GoldPoint[0]) >= Misc.Util.MaxLevel
					? "����"
					: String.Format("{0:#,###}", Misc.Util.NextLevel(pm.GoldPoint[0])),
				false,
				false
			);

			AddHtml(210, 10, 250, 20, "���� ����ġ", false, false); // <CENTER>HOUSE
			AddHtml(
				330,
				10,
				200,
				16,
				pm.GoldPoint[10] > 0 ? (String.Format("{0:#,###}", pm.GoldPoint[10])) : "0",
				false,
				false
			);

			AddHtml(210, 60, 250, 20, "���� ����Ʈ", false, false); // <CENTER>HOUSE
			AddHtml(330, 60, 200, 16, m_Craftpoint > 0 ? String.Format("{0:#,###}", m_Craftpoint) : "0", false, false);

			AddHtml(210, 80, 100, 20, "���� ����", false, false); // <CENTER>HOUSE
			AddHtml(
				330,
				80,
				40,
				16,
				Misc.Util.Level(pm.GoldPoint[10]) >= Misc.Util.MaxLevel
					? "�ִ� ����"
					: Misc.Util.Level(pm.GoldPoint[10]).ToString(),
				false,
				false
			);

			AddHtml(210, 110, 130, 20, "���� ����ġ", false, false); // <CENTER>HOUSE
			AddHtml(
				330,
				110,
				250,
				16,
				Misc.Util.Level(pm.GoldPoint[10]) >= Misc.Util.MaxLevel
					? "����"
					: String.Format("{0:#,###}", Misc.Util.NextLevel(pm.GoldPoint[10])),
				false,
				false
			);

			AddHtml(50, 140, 225, 20, "�ɼ�", false, false); // House Description
			AddHtml(260, 140, 225, 20, "�ɼ�", false, false); // House Description
			//AddHtml(275, 90, 225, 20, "�̸�(���� ����)", false, false); // House Description

			if (pm.DeathCheck == 0)
			{
				AddHtml(50, 40, 195, 20, "����Ʈ ����(1�� 1ȸ)", false, false);
				AddButton(10, 40, 4005, 4007, 100, GumpButtonType.Reply, 0);
				AddHtml(260, 40, 195, 20, "����Ʈ ����(1�� 1ȸ)", false, false);
				AddButton(210, 40, 4005, 4007, 200, GumpButtonType.Reply, 0);
			}

			int x = 50;
			int y = 160;
			int index = 2;
			for (int i = 0; i < GrawName.Length; i++)
			{
				if (i == 4)
				{
					x = 260;
					y = 160;
					index = 8;
				}
				int point = 0;
				//�̸�
				string allname = GrawName[i];

				allname += "(" + pm.GoldPoint[i + index].ToString() + ")";
				AddHtml(x, y, 250, 20, allname, false, false);
				if (i < 4)
				{
					if (m_Harvestpoint > 0 && pm.GoldPoint[i + index] < Max_Level)
						AddButton(x - 40, y, 4005, 4007, i + index, GumpButtonType.Reply, 0);
				}
				else
				{
					if (m_Craftpoint > 0 && pm.GoldPoint[i + index] < Max_Level)
						AddButton(x - 40, y, 4005, 4007, i + index, GumpButtonType.Reply, 0);
				}
				y += 20;
				//index++;
			}

			y += 20;

			string HarvestQuestList = "ä�� ����Ʈ �޼� ����";
			for (int i = 8; i < 0; --i)
			{
				if (pm.QuestCheck[i])
				{
					HarvestQuestList = "ä�� ����Ʈ " + (i + 1).ToString() + "�ܰ� �޼�";
					break;
				}
			}
			AddHtml(10, y, 250, 20, HarvestQuestList, false, false);

			y += 20;

			AddHtml(50, y, 250, 20, "ä�� ���� Ȯ�� ��ư", false, false);
			AddButton(10, y, 4005, 4007, 150, GumpButtonType.Reply, 0);

			AddHtml(260, y, 250, 20, "���� ���� Ȯ�� ��ư", false, false);
			AddButton(210, y, 4005, 4007, 250, GumpButtonType.Reply, 0);

			if (pm.GoldPoint[49] > 0)
			{
				y += 30;
				AddHtml(50, y, 205, 20, "���� ���� ����Ʈ:", false, false);
				AddHtml(215, y, 110, 16, String.Format("{0:#,###}", pm.GoldPoint[49]), false, false);

				y += 20;
				AddHtml(50, y, 300, 20, "���� ���� ����Ʈ�� ä�� ����Ʈ�� ��ȯ", false, false);
				AddButton(10, y, 4005, 4007, 300, GumpButtonType.Reply, 0);

				y += 20;
				AddHtml(50, y, 300, 20, "���� ���� ����Ʈ�� ���� ����Ʈ�� ��ȯ", false, false);
				AddButton(10, y, 4005, 4007, 400, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			if (info.ButtonID == 100)
			{
				m_pm.DeathCheck = 1;
				for (int i = 1; i <= 5; i++)
				{
					m_pm.GoldPoint[i] = 0;
				}
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else if (info.ButtonID == 150)
			{
				m_pm.CloseGump(typeof(HarvestGump));
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			else if (info.ButtonID == 200)
			{
				m_pm.DeathCheck = 1;
				for (int i = 11; i <= 15; i++)
				{
					m_pm.GoldPoint[i] = 0;
				}
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else if (info.ButtonID == 250)
			{
				m_pm.CloseGump(typeof(CraftingGump));
				m_pm.SendGump(new CraftingGump(m_pm));
			}
			else if (info.ButtonID == 300)
			{
				m_pm.SendMessage("1���� " + m_pm.GoldPoint[49].ToString() + "������ ���ڸ� ��������!");
				m_pm.BeginPrompt(
					(from, text) =>
					{
						int amount = Utility.ToInt32(text);
						if (amount <= m_pm.GoldPoint[49])
						{
							from.SendMessage("ä�� ����Ʈ�� " + amount.ToString() + "��ŭ ȹ���մϴ�!");
							m_pm.GoldPoint[0] += amount;
							m_pm.GoldPoint[49] -= amount;
						}
						else
							from.SendMessage("�߸��� ���ڳ� ���ڸ� �����̳׿�...");
					}
				);
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else if (info.ButtonID == 400)
			{
				m_pm.SendMessage("1���� " + m_pm.GoldPoint[49].ToString() + "������ ���ڸ� ��������!");
				m_pm.BeginPrompt(
					(from, text) =>
					{
						int amount = Utility.ToInt32(text);
						if (amount <= m_pm.GoldPoint[49])
						{
							from.SendMessage("���� ����Ʈ�� " + amount.ToString() + "��ŭ ȹ���մϴ�!");
							m_pm.GoldPoint[10] += amount;
							m_pm.GoldPoint[49] -= amount;
						}
						else
							from.SendMessage("�߸��� ���ڳ� ���ڸ� �����̳׿�...");
					}
				);
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else
			{
				int index = 1;
				if (info.ButtonID >= 12 && info.ButtonID <= 15)
					index = 11;
				m_pm.GoldPoint[info.ButtonID]++;
				m_pm.GoldPoint[index]++;
				m_pm.Delta(MobileDelta.Stat);
				m_pm.ProcessDelta();
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
		}

		private void SendGump()
		{
			m_pm.SendGump(new GoldPointGump(m_pm));
		}
	}
	#endregion

	#region ����
	public class SilverPointGump : Gump
	{
		readonly int Max_Level = 50;
		private PlayerMobile m_pm;

		//30������ ����
		//35������ ����Ʈ
		//70�� ��ü ���� ����Ʈ
		//71������ ���� ����Ʈ

		private string[] Name =
		{
			"���� ���� ����",
			"��׷� ����",
			"��׷� ����",
			"Ư���� Ȯ�� ����",
			"���� ������ ����",
			"���� ������ ����",
			"���� ������ ����",
			"��� ������ ����",
			"���� ������ ����",
			"ȭ�� ������ ����",
			"�ñ� ������ ����",
			"�� ������ ����",
			"������ ������ ����",
			"ġ���� ����",
			"ȸ���� ����",
			"���� ġ��Ÿ Ȯ�� ����",
			"���� ġ��Ÿ Ȯ�� ����",
			"���� ġ��Ÿ ������ ����",
			"���� ġ��Ÿ ������ ����",
			"ü�� ����",
			"��� ����",
			"���� ����",
			"ü�� ��� ����",
			"��� ��� ����",
			"���� ��� ����",
			"��� ���� ����",
			"�� ����",
			"��ø ����",
			"���� ����",
			"�� ����",
		};

		private string[] Point_Name = { "��� ����Ʈ", "���� ����Ʈ", "���� ����Ʈ", "���� ����Ʈ", "��ȭ ����Ʈ" };

		private string[] Basic_Dungeon_Name =
		{
			"�ں�����",
			"����������",
			"���",
			"����",
			"��ũ ����",
			"�� ����",
			"���̽� ����",
			"���̾� ����",
			"�����ν� ����",
			"����Ÿ�� ����",
		};

		private int[] Space = { 0, 2, 3, 5, 8, 12, 14, 16, 18, 21, 24 };

		private int m_point = 0;

		public SilverPointGump(PlayerMobile pm)
			: base(50, 50)
		{
			int DungeonQuest = 0;
			for (int i = 0; i < 30; i++)
			{
				if (pm.QuestCheck[10000 + i])
				{
					DungeonQuest += (i % 3) + 1;
				}
			}

			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;
			m_point = Misc.Util.Level(pm.SilverPoint[0]) - pm.SilverPoint[1] + DungeonQuest;
			pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));
			AddPage(0);

			AddBackground(0, 0, 1300, 880, 5054);

			AddHtml(10, 10, 250, 20, "���� ����Ʈ", false, false); // <CENTER>HOUSE
			AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", pm.SilverPoint[0]), false, false);

			AddBackground(0, 0, 1300, 880, 5054);

			AddHtml(10, 40, 250, 20, "���� ����Ʈ", false, false); // <CENTER>HOUSE
			AddHtml(130, 40, 200, 16, m_point > 0 ? String.Format("{0:#,###}", m_point) : "0", false, false);

			AddHtml(10, 60, 100, 20, "���� ����", false, false); // <CENTER>HOUSE
			AddHtml(
				120,
				60,
				40,
				16,
				Misc.Util.Level(pm.SilverPoint[0]) >= Misc.Util.MaxLevel
					? Misc.Util.MaxLevel.ToString()
					: Misc.Util.Level(pm.SilverPoint[0]).ToString(),
				false,
				false
			);

			AddHtml(10, 90, 130, 20, "���� ����ġ", false, false); // <CENTER>HOUSE
			AddHtml(
				130,
				90,
				250,
				16,
				Misc.Util.Level(pm.SilverPoint[0]) >= Misc.Util.MaxLevel
					? "����"
					: String.Format("{0:#,###}", Misc.Util.NextLevel(pm.SilverPoint[0])),
				false,
				false
			);

			//AddHtml(500, 10, 250, 20, "���� ��ų ����Ʈ", false, false); // <CENTER>HOUSE
			//AddHtml(380, 10, 200, 16, String.Format("{0:#,###}", m_point), false, false);


			AddHtml(50, 120, 225, 20, "�ɼ�", false, false); // House Description
			//AddHtml(275, 40, 225, 20, "�̸�(���� ����)", false, false); // House Description
			//AddHtml(275, 40, 75, 20, "����", false, false); // Storage
			//AddHtml(350, 40, 150, 20, "���", false, false); // Lockdowns
			if (pm.DeathCheck == 0)
			{
				AddHtml(655, 40, 195, 20, "����Ʈ ����(1�� 1ȸ)", false, false);
				AddButton(850, 40, 4005, 4007, 100, GumpButtonType.Reply, 0);
			}
			int y = 140;
			int size = 0;

			for (int i = 0; i < Name.Length; i++)
			{
				int point = 0;
				//�̸�
				string allname = Name[i];

				allname += "(" + pm.SilverPoint[i + 2].ToString() + ")";
				AddHtml(50 + size, y, 250, 20, allname, false, false);
				if (m_point > 0 && pm.SilverPoint[i + 2] < Max_Level)
					AddButton(10 + size, y, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				size += 250;
				for (int j = 0; j < Space.Length; j++)
				{
					if (i == Space[j]) //����
					{
						y += 20;
						size = 0;
					}
				}
			}
			//����Ʈ ����
			/*
			y += 30;
			AddHtml(50, y, 405, 20, "��� ����Ʈ", false, false); // House Description
			y += 20;
			AddHtml(50, y, 405, 20, "����Ʈ ��ȯ �� Ƽ�� �� 1, 2, 4, 7, 10, 15, 23�� ȹ��. ���� 1�ܰ� �� 5�� �߰� ����", false, false); // 			y += 20;
			y += 20;
			AddHtml(50, y, 405, 20, "������ ���� �� 1 + ���� ��� * ���� ��� �Ҹ�", false, false); // House Description
			y += 20;
			AddHtml(50, y, 405, 20, "������ ��ȯ�� ��� ���� ȭ��ǥ Ŭ��. ������ ���� ��ų�� ������ ��ȭ Ŭ��.", false, false); // House Description
			y += 20;
			size = 0;
			for ( int i = 0; i < Point_Name.Length; i++ )
			{
				string allname = Point_Name[i];
				Account acc = pm.Account as Account;
				allname += " : " + String.Format("{0:#,###}", acc.Point[861 + i] > 0 ? acc.Point[861 + i].ToString() : "0" ) + "��";
				AddHtml(50 + size, y, 250, 20, allname, false, false);
				AddButton(10 + size, y, 4005, 4007, i + 31, GumpButtonType.Reply, 0);
				size += 250;
			}
			*/
			//���� Ȯ��
			y += 30;
			size = 0;
			AddHtml(50, y, 405, 20, "���� Ȯ�� ��ư", false, false); // House Description
			AddButton(10 + size, y, 4005, 4007, 150, GumpButtonType.Reply, 0);

			//����Ʈ �Ϸ� Ȯ��
			y += 30;
			AddHtml(50, y, 405, 20, "����Ʈ �Ϸ� Ȯ��", false, false); // House Description

			y += 30;

			string BasicDungeonList = "";
			int index = 0;
			for (int i = 0; i < 10; ++i)
			{
				BasicDungeonList = " ����Ʈ �޼� ����";
				for (int j = 0; j < 3; ++j)
				{
					if (pm.QuestCheck[10000 + index * 3 + j])
						BasicDungeonList = " ����Ʈ " + (index + 1).ToString() + "�ܰ� �޼�";
				}
				if (i == 5)
				{
					index = 0;
					size = 0;
					y += 20;
				}
				AddHtml(50 + size, y, 250, 20, Basic_Dungeon_Name[i] + BasicDungeonList, false, false);

				index++;
				size += 250;
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			if (info.ButtonID == 100)
			{
				m_pm.DeathCheck = 1;
				int returnPoint = 0;
				m_pm.DeathCheck = 1;
				for (int i = 1; i < m_pm.SilverPoint.Length; i++)
				{
					m_pm.SilverPoint[i] = 0;
				}
				m_pm.SendGump(new SilverPointGump(m_pm));
				//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
			}
			else if (info.ButtonID == 150)
			{
				m_pm.CloseGump(typeof(MonsterFeatGump));
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			else if (info.ButtonID <= 30)
			{
				m_pm.SilverPoint[info.ButtonID + 1]++;
				m_pm.SilverPoint[1]++;
				m_pm.Delta(MobileDelta.Stat);
				m_pm.ComputeResistances();
				m_pm.ProcessDelta();
				m_pm.SendGump(new SilverPointGump(m_pm));
				//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
			}
			else
			{
				//m_pm.Target = new InternalTarget(info.ButtonID - 30);
				m_pm.SendGump(new SilverPointGump(m_pm));
			}
		}

		//��� ��ȯ Ŭ��
		/*
		private class InternalTarget : Target
		{
			int m_Rank = 0;
			public InternalTarget(int rank) :  base ( 8, false, TargetFlags.None )
			{
				m_Rank = rank;
			}
			protected override void OnTarget( Mobile from, object targeted )
			{
				if( m_Rank == 0 )
				{
				}
				else if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					if( targeted is Item )
					{
						Item item = targeted as Item;
						BaseHouse house = BaseHouse.FindHouseAt(from);
						if( item.RootParent == from || ( house != null && house.IsOwner(from)) )
							Misc.Util.EquipPointReturn(pm, item, m_Rank);
					}
				}
			}
		}
		*/
		private void SendGump()
		{
			m_pm.SendGump(new SilverPointGump(m_pm));
		}
	}
	#endregion

	#region ����
	public class EquipPointGump : Gump //1����Ʈ ��밪 171.6666666 ����
	{
		private PlayerMobile m_pm;

		private string[] NameA = { "�α��� ���ʽ�" }; //, "���� ����Ʈ (10 ����Ʈ)", "���� ����Ʈ (10 ����Ʈ)" };
		private string[] Name =
		{
			"���� ä��",
			"���� ä��",
			"���� ä��",
			"����ǰ ����",
			"1�� ����",
			"2�� ����",
			"3�� ����",
		};
		private string[] accProduct = { "�ɸ��� ����", "�� ����", "��ų ����ġ 1% ���� ����" };
		private string[] goldProduct =
		{
			"�Ｎ ���� ����(0.25% 100��, 1% 10��, 10% 1��)",
			"��÷ ���� ����",
			"�þ� 8�� 1000��Ʈ 6õ",
			"�þ� 8�� 10000��Ʈ 6��",
			"�� �ڷ���Ʈ 10��",
		};

		private int[] GivePoint = { 100, 900, 8500, 80000 };

		private int[] CharacterBuyPrice = { 10, 80, 500, 2500, 10000, 25000 };

		private int[] Price = { 15, 150, 17, 165, 1600, 10000 };
		private int[] goldPrice = { 1000, 1000, 6000, 60000, 100000 };

		public EquipPointGump(PlayerMobile pm)
			: base(50, 50)
		{
			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;
			//pm.CloseGump(typeof(SilverPointGump));
			pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

			Account acc = pm.Account as Account;

			AddPage(0);

			AddBackground(0, 0, 525, 590, 5054);

			//AddImageTiled(10, 10, 500, 150, 2624);
			//AddAlphaRegion(10, 10, 500, 150);

			AddHtml(10, 10, 250, 20, "���� ����Ʈ: ", false, false); // <CENTER>HOUSE
			AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", acc.Point[0]), false, false);

			AddHtml(50, 40, 225, 20, "���", false, false); // House Description
			AddHtml(275, 40, 75, 20, "����", false, false); // Storage
			AddHtml(350, 40, 150, 20, "ȹ��", false, false); // Lockdowns

			string donation = String.Format("{0:#,###}", acc.DonationPoint);

			if (acc.DonationPoint == 0)
				AddHtml(50, 90, 350, 20, "�̹� �� ��� ����", false, false); // House
			else
				AddHtml(50, 90, 3500, 20, "�̹� �� ��� ����Ʈ " + donation + "��", false, false); // House Description
			AddButton(10, 90, 4005, 4007, 100, GumpButtonType.Reply, 0);

			donation = "";

			DonationCheck check = null;
			if (Server.Event.dc == null)
			{
				foreach (Item item in World.Items.Values)
				{
					if (item is DonationCheck)
					{
						DonationCheck dc = item as DonationCheck;
						Server.Event.dc = dc;
						check = dc;
						break;
					}
				}
			}
			else
				check = Server.Event.dc;

			if (check != null)
			{
				for (int i = 0; i < 1000; i++)
				{
					if (check.DonationList[i] == acc.Username)
					{
						donation = (i + 1).ToString();
						break;
					}
				}
			}

			if (donation == "")
				AddHtml(250, 90, 150, 20, "���� ���� ����", false, false); // Storage
			else
				AddHtml(250, 90, 150, 20, "���� ���� " + donation + "��", false, false); // Storage

			AddHtml(50, 110, 500, 20, "1�� ��� ����ϱ� : ��� & ���� ����Ʈ 100�� ȹ��", false, false); // Lockdowns
			AddHtml(50, 130, 500, 20, "10�� ��� ����ϱ� : ��� & ���� ����Ʈ 900�� ȹ��", false, false); // Lockdowns
			AddHtml(50, 150, 500, 20, "100�� ��� ����ϱ� : ��� & ���� ����Ʈ 8500�� ȹ��", false, false); // Lockdowns
			AddHtml(50, 170, 500, 20, "1000�� ��� ����ϱ� : ��� & ���� ����Ʈ 80000�� ȹ��", false, false); // Lockdowns
			if (Banker.GetBalance(pm) >= 10000)
				AddButton(10, 110, 4005, 4007, 1, GumpButtonType.Reply, 0);
			if (Banker.GetBalance(pm) >= 100000)
				AddButton(10, 130, 4005, 4007, 2, GumpButtonType.Reply, 0);
			if (Banker.GetBalance(pm) >= 1000000)
				AddButton(10, 150, 4005, 4007, 3, GumpButtonType.Reply, 0);
			if (Banker.GetBalance(pm) >= 10000000)
				AddButton(10, 170, 4005, 4007, 4, GumpButtonType.Reply, 0);

			int y = 60;

			for (int i = 0; i < 1; i++)
			{
				//�̸�
				AddHtml(50, y + i * 20, 225, 20, NameA[i], false, false);
				switch (i)
				{
					case 0: //�α���
					{
						//�޼���
						AddHtml(275, y + i * 20, 75, 20, acc.LoginBonus.ToString() + "��", false, false);
						//���ʽ�
						if (acc.LoginBonus <= 1)
							AddHtml(
								350,
								y + i * 20,
								150,
								20,
								Misc.Util.Equip_Login[0].ToString() + " ����Ʈ",
								false,
								false
							);
						else
							AddHtml(
								350,
								y + i * 20,
								150,
								20,
								Misc.Util.Equip_Login[acc.LoginBonus - 1].ToString() + " ����Ʈ",
								false,
								false
							);
						break;
					}
				}
			}

			AddHtml(50, 200, 500, 20, "���� �ý��� Ȯ��", false, false); // Storage
			int x = 0;
			y = 0;
			for (int i = 0; i < Name.Length; i++)
			{
				if (i == 4)
				{
					x = 0;
					y += 20;
				}
				AddHtml(50 + x * 125, 220 + y, 200, 20, Name[i], false, false);
				AddButton(10 + x * 125, 220 + y, 4005, 4007, i + 5, GumpButtonType.Reply, 0);
				x++;
			}

			//���� ����Ʈ ���
			AddHtml(50, 270, 500, 20, "���� ����Ʈ ����", false, false); // Storage

			if (acc.CharacterSlotsBonus < 6)
			{
				AddHtml(
					50,
					290,
					500,
					20,
					accProduct[0] + "(" + CharacterBuyPrice[acc.CharacterSlotsBonus].ToString() + ")",
					false,
					false
				);
				if (CharacterBuyPrice[acc.CharacterSlotsBonus] <= acc.Point[0])
					AddButton(10, 290, 4005, 4007, 15, GumpButtonType.Reply, 0);
			}
			else
				AddHtml(50, 290, 500, 20, "��� �ɸ��� ����", false, false);
			int usePrice = (acc.HouseSlotsBonus + 1) * 2000;

			AddHtml(50, 310, 500, 20, accProduct[1] + "(" + usePrice.ToString() + ")", false, false);
			if (acc.HouseSlotsBonus < 5 + acc.CharacterSlotsBonus && usePrice <= acc.Point[0])
				AddButton(10, 310, 4005, 4007, 16, GumpButtonType.Reply, 0);

			usePrice = (acc.TeachingBonus + 1) * 500;

			AddHtml(50, 330, 500, 20, accProduct[2] + "(" + usePrice.ToString() + ")", false, false);
			if (usePrice <= acc.Point[0])
				AddButton(10, 330, 4005, 4007, 17, GumpButtonType.Reply, 0);

			for (int i = 0; i < goldProduct.Length; i++)
			{
				if (i == 1 && acc.Lotto != 0)
				{
					AddHtml(
						50,
						360 + i * 20,
						300,
						20,
						"����� ������ ��ȣ��" + acc.Lotto.ToString() + "�Դϴ�.",
						false,
						false
					); // Storage
				}
				else
				{
					AddHtml(50, 360 + i * 20, 500, 20, goldProduct[i], false, false); // Storage
					if (acc.Point[0] >= goldPrice[i])
						AddButton(10, 360 + i * 20, 4005, 4007, 18 + i, GumpButtonType.Reply, 0);
				}
			}
			LottoCheck lottocheck = null;
			if (Server.Event.lc == null)
			{
				foreach (Item item in World.Items.Values)
				{
					if (item is LottoCheck)
					{
						LottoCheck lc = item as LottoCheck;
						Server.Event.lc = lc;
						lottocheck = lc;
						break;
					}
				}
				Console.WriteLine("Donation Respawn success");
			}
			else
				lottocheck = Server.Event.lc;
			if (lottocheck != null)
			{
				if (lottocheck.LottoNumber == 0 || lottocheck.LottoNumber == null)
					AddHtml(350, 380, 200, 20, "��÷ �������Դϴ�", false, false); // Storage
				else
					AddHtml(350, 400, 200, 20, "��÷ ��ȣ : " + lottocheck.LottoNumber.ToString(), false, false); // Storage
			}
		}

		private void RandomCheck_Reward(PlayerMobile pm)
		{
			int Dice = Utility.RandomMinMax(1, 10000);
			int price = 0;
			if (Dice <= 25)
				price = 1000000; //��밪 1000��
			else if (Dice <= 125)
				price = 100000; //��밪 900��
			else if (Dice <= 1125)
				price = 10000; //��밪 400��

			if (price > 0)
			{
				Banker.Deposit(pm, price);

				if (price == 1000000)
					World.Broadcast(0x0, false, "{0}���� �Ｎ ���ǿ��� 100�� ��忡 ��÷�Ǽ̽��ϴ�!", m_pm.Name);
				else
					pm.SendMessage("�Ｎ ���ǿ��� {0}��带 ȹ���մϴ�!", price.ToString());
			}
			else
				pm.SendMessage("���� ���׿�...");
		}

		private void DonationSave(Account acc, int donation)
		{
			acc.DonationPoint += donation;
			acc.Point[0] += donation;

			if (acc.DonationPoint >= 100)
			{
				DonationCheck check = null;
				if (Server.Event.dc == null)
				{
					foreach (Item item in World.Items.Values)
					{
						if (item is DonationCheck)
						{
							DonationCheck dc = item as DonationCheck;
							Server.Event.dc = dc;
							check = dc;
							break;
						}
					}
				}
				else
					check = Server.Event.dc;

				if (check != null)
				{
					for (int i = 0; i < check.DonationList.Length; i++)
					{
						if (check.DonationList[i] == acc.Username)
							break;
						if (check.DonationList[i] == "" || check.DonationList[i] == null)
						{
							check.DonationList[i] = acc.Username;
							break;
						}
						if (check.DonationList[i] != "")
						{
							foreach (Account a in Accounts.GetAccounts())
							{
								if (check.DonationList[i] == a.Username)
								{
									if (a.DonationPoint < acc.DonationPoint)
									{
										check.DonationList[i] = acc.Username;
										for (int j = i; j < check.DonationList.Length - 1; j++)
										{
											if (check.DonationList[j] == "" || check.DonationList[i] == null)
												break;
											else
												check.DonationList[j + 1] = check.DonationList[j];
										}
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			//m_pm.EquipPoint[0] -= Price[info.ButtonID - 1];

			if (info.ButtonID >= 1 && info.ButtonID < 5)
			{
				Banker.Withdraw(m_pm, (int)Math.Pow(10, info.ButtonID + 3), true);
				Account acc = m_pm.Account as Account;
				DonationSave(acc, GivePoint[info.ButtonID - 1]);
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if (info.ButtonID >= 5 && info.ButtonID < 12)
			{
				//m_pm.CloseGump(typeof(SavingAccountGump));
				m_pm.SendGump(new SavingAccountGump(m_pm, info.ButtonID - 5));
			}
			else if (info.ButtonID == 15)
			{
				Account acc = m_pm.Account as Account;
				acc.Point[0] -= CharacterBuyPrice[acc.CharacterSlotsBonus];
				acc.CharacterSlotsBonus++;
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if (info.ButtonID == 16)
			{
				Account acc = m_pm.Account as Account;
				acc.Point[0] -= (acc.HouseSlotsBonus + 1) * 2000;
				acc.HouseSlotsBonus++;
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if (info.ButtonID == 17)
			{
				Account acc = m_pm.Account as Account;
				acc.Point[0] -= (acc.TeachingBonus + 1) * 500;
				acc.TeachingBonus++;
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if (info.ButtonID >= 18 && info.ButtonID <= 22)
			{
				Account acc = m_pm.Account as Account;
				if (info.ButtonID != 19)
					acc.Point[0] -= goldPrice[info.ButtonID - 18];
				//Banker.Withdraw(m_pm, goldPrice[info.ButtonID - 18], true);
				switch (info.ButtonID)
				{
					case 18:
					{
						RandomCheck_Reward(m_pm);
						break;
					}
					case 19:
					{
						if (acc.Lotto == 0)
						{
							m_pm.SendMessage("1���� 9999 ������ ���ڸ� ��������!");
							m_pm.BeginPrompt(
								(from, text) =>
								{
									int amount = Utility.ToInt32(text);
									if (amount >= 1 && amount <= 10000)
									{
										from.SendMessage(amount.ToString() + "���� �����ϼ̽��ϴ�. ����� ���ϴ�!");
										acc.Lotto = amount;
										Banker.Withdraw(m_pm, 10000, true);
									}
									else
										from.SendMessage("�߸��� ���ڳ� ���ڸ� �����̳׿�...");
								}
							);
						}
						break;
					}
					case 20:
					{
						BagOfReagents br = new BagOfReagents(1000);
						m_pm.AddToBackpack(br);
						m_pm.SendMessage("��� �þ��� 1000�� ȹ���մϴ�.");
						break;
					}
					case 21:
					{
						BagOfReagents br = new BagOfReagents(10000);
						m_pm.AddToBackpack(br);
						m_pm.SendMessage("��� �þ��� 10000�� ȹ���մϴ�.");
						break;
					}
					case 22:
					{
						HouseTeleporterTileBag br = new HouseTeleporterTileBag(false);
						m_pm.AddToBackpack(br);
						m_pm.SendMessage("�� �ڷ���Ʈ�� ȹ���մϴ�.");
						break;
					}
				}
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if (info.ButtonID == 100)
			{
				m_pm.CloseGump(typeof(DonationRankingGump));
				m_pm.SendGump(new DonationRankingGump(m_pm));
			}
		}

		private void SendGump()
		{
			m_pm.SendGump(new EquipPointGump(m_pm));
		}
	}
	#endregion

	#region ����
	public class ArtifactPointGump : Gump
	{
		//private const int LabelColor = 0x7FFF;
		//private const int LabelHue = 0x481;

		private PlayerMobile m_pm;

		private string[] Name =
		{
			"�� ����",
			"��ø�� ����",
			"���� ����",
			"��� ����",
			"ü�� ����",
			"��� ����",
			"���� ����",
			"���� ������ ����",
			"���� ������ ����",
			"���� ������ ����",
			"���� ������ ����",
			"������ ����",
		};

		public ArtifactPointGump(PlayerMobile pm)
			: base(50, 50)
		{
			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;
			//pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

			AddPage(0);

			AddBackground(0, 0, 500, 350, 5054);

			//11 : ���� ���� ȹ�� üũ

			//AddImageTiled(10, 10, 500, 150, 2624);
			//AddAlphaRegion(10, 10, 500, 150);

			AddHtml(10, 10, 250, 20, "���� ����Ʈ ���� ��", false, false); // <CENTER>HOUSE
			/*
			AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

			AddHtml(50, 40, 225, 20, "�̸�", false, false); // House Description
			AddHtml(275, 40, 75, 20, "����", false, false); // Storage
			AddHtml(350, 40, 150, 20, "���", false, false); // Lockdowns
			
			//AddImageTiled(10, 70, 500, 280, 2624);
			//AddAlphaRegion(10, 70, 500, 280);

		   // AddImageTiled(10, 970, 500, 20, 2624);
		   // AddAlphaRegion(10, 970, 500, 20);

			int y = 60;
			for ( int i = 0; i < 12; i++)
			{
				int mul = 1;
				if( i == 11 )
					mul = 10;
				if( ( pm.ArtifactPoint[i + 1] + 1 ) * ( pm.ArtifactPoint[i + 1] + 1 ) * mul <= pm.ArtifactPoint[0] && pm.ArtifactPoint[i + 1] < 100 )
					AddButton(10, y + i * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				
				//�̸�
				AddHtml( 50, y + i * 20, 225, 20, Name[i], false, false);

				//����
				AddHtml( 275, y + i * 20, 75, 20, pm.ArtifactPoint[i + 1].ToString(), false, false);

				//���
				if( pm.ArtifactPoint[ i + 1 ] < 100 )
					AddHtml( 350, y + i * 20, 150, 20, ( ( pm.ArtifactPoint[i + 1] + 1 ) * ( pm.ArtifactPoint[i + 1] + 1 ) * mul ).ToString(), false, false);
			}
			*/
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			int mul = 1;

			if (info.ButtonID == 12)
				mul = 10;
			m_pm.ArtifactPoint[0] -=
				(m_pm.ArtifactPoint[info.ButtonID] + 1) * (m_pm.ArtifactPoint[info.ButtonID] + 1) * mul;
			m_pm.ArtifactPoint[info.ButtonID]++;
			m_pm.Delta(MobileDelta.Stat);
			m_pm.ProcessDelta();
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);

			//m_pm.CloseGump(typeof(SilverPointGump));
			//m_pm.SendGump(new SilverPointGump(m_pm));
		}

		private void SendGump()
		{
			m_pm.SendGump(new ArtifactPointGump(m_pm));
		}
	}
	#endregion

	#region ��ų
	public class SkillPointGump : Gump
	{
		private PlayerMobile m_pm;

		private FirstSkillCheck fsc = null;
		private int price = 0;

		//private bool pagecheck = false;

		public SkillPointGump(PlayerMobile pm)
			: base(50, 50)
		{
			pm.CloseGump(typeof(CityPointGump));
			m_pm = pm;

			pm.CloseGump(typeof(SkillPointGump));

			foreach (Item item in World.Items.Values)
			{
				if (item is FirstSkillCheck)
				{
					fsc = item as FirstSkillCheck;
					break;
				}
			}
			AddPage(0);

			AddBackground(0, 0, 810, 650, 5054);

			//AddImageTiled(10, 10, 500, 150, 2624);
			//AddAlphaRegion(10, 10, 500, 150);

			AddHtml(10, 10, 250, 20, "��ų ����ġ", false, false); // <CENTER>HOUSE
			//AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

			AddHtml(300, 10, 75, 20, "��ų ����: ", false, false); // <CENTER>HOUSE
			AddHtml(380, 10, 100, 16, String.Format("{0:N1}", (double)pm.SkillsTotal * 0.1), false, false);

			AddHtml(50, 40, 100, 20, "�̸�", false, false); // House Description
			AddHtml(150, 40, 75, 20, "���� ����", false, false); // House Description
			AddHtml(225, 40, 75, 20, "���� ��ų", false, false); // House Description
			AddHtml(300, 40, 175, 20, "����", false, false); // Storage
			AddHtml(475, 40, 75, 20, "�� ��ų", false, false);
			AddHtml(550, 40, 100, 20, "��ų �ۼ�Ʈ", false, false); // Storage
			AddHtml(650, 40, 100, 20, "�ִ� ��ų", false, false); // Storage
			//AddHtml(750, 40, 50, 20, "����", false, false); // Storage

			//AddButton(100, 10, 4005, 4007, 100, GumpButtonType.Reply, 0);

			int y = 60;
			int page = pm.SkillGumpPage;
			int index = 0;
			//pm.skillpage = false;
			//AddButton( 130, 10, 4005, 4007, 100, GumpButtonType.Reply, 0);
			Account acc = pm.Account as Account;

			for (int i = 0 + page * 29; i < 29 + page * 29; i++)
			{
				if (page == 0)
				{
					AddButton(750, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				else
				{
					AddButton(750, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}
				//double AccountSkillSum = 0.0;
				double AccountSkillBest = 0.0;

				//���� ��ų üũ
				if (acc.Count > 1)
				{
					for (int j = 0; j < acc.Length; ++j)
					{
						Mobile check = acc[j];
						if (check != null && check != pm)
						{
							if (AccountSkillBest < check.Skills[i].Base)
								AccountSkillBest = check.Skills[i].Base;

							//AccountSkillSum += check.Skills[i].Base * 0.1;
						}
					}
				}

				//��ų ���� || �ε� ��ư
				if (
					(acc.Point[i + 801] == 0 && pm.Skills[i].Base != 0)
					|| (
						acc.Point[i + 801] != 0
						&& acc.Point[i + 801] > pm.Skills[i].Base
						&& pm.SkillsTotal + acc.Point[i + 801] <= 15000
					)
				)
					AddButton(10, y + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);

				//�̸�
				AddHtml(50, y + index * 20, 100, 20, fsc.SkillName[i], false, false);

				//���� ���� & ���� ��ų
				string save = "����";
				string skill = "����";
				if (acc.Point[i + 801] != 0)
				{
					save = "���� ��";
					skill = (acc.Point[i + 801] * 0.1).ToString();
				}
				AddHtml(150, y + index * 20, 75, 20, save, false, false);
				AddHtml(225, y + index * 20, 75, 20, skill, false, false);

				//����
				string skillnow = ((int)(pm.SkillList[i])).ToString();
				if (pm.SkillList[i] >= 1000)
					skillnow = string.Format("{0:#,###}", pm.SkillList[i]);
				skillnow += " / " + string.Format("{0:#,###}", Misc.Util.SkillExp_Calc(pm, i));
				AddHtml(300, y + index * 20, 175, 20, skillnow, false, false);

				//�� ��ų
				AddHtml(475, y + index * 20, 75, 20, pm.Skills[i].Base.ToString(), false, false);

				//��ų �ۼ�Ʈ
				AddHtml(550, y + index * 20, 100, 20, string.Format("{0:N2}%", SkillPercent(pm, i)), false, false);

				//���� �ְ� ��ų
				AddHtml(650, y + index * 20, 150, 20, AccountSkillBest.ToString(), false, false);

				index++;
			}
		}

		private double SkillPercent(PlayerMobile pm, int skill)
		{
			double skillcalc = pm.SkillList[skill] * 100 / Misc.Util.SkillExp_Calc(pm, skill);
			return skillcalc > 100 ? 100 : skillcalc;
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			if (info.ButtonID == 100) { }
			else if (info.ButtonID == 99)
			{
				m_pm.SkillGumpPage = 0;
				m_pm.SendGump(new SkillPointGump(m_pm));
			}
			else if (info.ButtonID == 98)
			{
				m_pm.SkillGumpPage = 1;
				m_pm.SendGump(new SkillPointGump(m_pm));
			}
			else
			{
				Account acc = m_pm.Account as Account;
				if (acc.Point[info.ButtonID + 800] == 0 && m_pm.Skills[info.ButtonID - 1].Base != 0)
				{
					acc.Point[info.ButtonID + 800] = (int)(m_pm.Skills[info.ButtonID - 1].Base * 10);
					m_pm.Skills[info.ButtonID - 1].Base = 0;
				}
				else if (
					acc.Point[info.ButtonID + 800] != 0
					&& acc.Point[info.ButtonID + 800] > m_pm.Skills[info.ButtonID - 1].Base
				)
				{
					m_pm.Skills[info.ButtonID - 1].Base = acc.Point[info.ButtonID + 800] * 0.1;
					acc.Point[info.ButtonID + 800] = 0;
				}
				m_pm.SendGump(new SkillPointGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
		}

		private void SendGump()
		{
			m_pm.SendGump(new SkillPointGump(m_pm));
		}
	}
	#endregion
	public class HarvestGump : Gump
	{
		PlayerMobile m_pm;
		int m_maxpage = 1;
		private string[] HarvestName =
		{
			"ö ä��",
			"���� ä��",
			"û�� ä��",
			"�� ä��",
			"�ư�����Ʈ ä��",
			"������Ʈ ä��",
			"��������Ʈ ä��",
			"�̽��� ä��",
			"�ɽõ�� ä��",
			"�� ä��",
			"�Ϲ� ���� ä��",
			"���� ���� ä��",
			"��Ǫ�� ���� ä��",
			"�ָ� ���� ä��",
			"���� ���� ä��",
			"�� ���� ä��",
			"���� ���� ä��",
			"ĥ�� ���� ä��",
			"���� ���� ä��",
			"",
			"�Ϲ� ���� ä��",
			"���� ���� ä��",
			"��ģ ���� ä��",
			"��ȭ ���� ä��",
			"���� ���� ä��",
			"�� ���� ä��",
			"�̴� ���� ä��",
			"���� ���� ä��",
			"��� ���� ä��",
			"",
			"�۾� ä��",
			"�轺 ä��",
			"���� ä��",
			"�ؾ� ä��",
			"�ޱ� ä��",
			"�뱸 ä��",
			"��� ä��",
			"û�� ä��",
			"��ġ ä��",
			"",
		};

		int[] AllCount = { 1, 5, 10, 20, 30, 50, 75, 100, 140, 200 };
		int[] HarvestStartNumber = { 2, 12, 22, 82 };

		public HarvestGump(PlayerMobile pm)
			: base(50, 50)
		{
			AddPage(0);
			AddBackground(0, 0, 620, 270, 5054);
			AddImageTiled(10, 10, 600, 250, 2624);

			AddHtml(0, 20, 620, 20, CenterGray("ä�� ä�� ��"), false, false); // <CENTER>HOUSE

			m_pm = pm;
			pm.CloseGump(typeof(HarvestGump));

			Account acc = pm.Account as Account;
			int page = pm.HarvestGumpPage;
			int index = 0;
			int maxlist = HarvestName.Length;
			int step = 10;
			m_maxpage = maxlist / step;
			int maxpage = Misc.Util.MaxpageCreate(maxlist, page, step);

			for (int i = page * step; i < maxpage + page * step; i++)
			{
				if (page < m_maxpage - 1)
				{
					AddButton(480, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				if (page > 0)
				{
					AddButton(450, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}

				AddHtml(50, 50 + index * 20, 200, 20, LeftGreen(HarvestName[i]), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				if (i == 9)
					AddHtml(
						250,
						50 + index * 20,
						200,
						20,
						LeftGreen("���� : " + acc.Point[90].ToString()),
						false,
						false
					); // <DIV
				else if (i == 19 || i == 29 || i == 39)
				{
					continue;
				} //AddHtml(250, 50 + index * 20, 200, 20, LeftGreen("���� : 0", false, false)); // <DIV
				else
					AddHtml(
						250,
						50 + index * 20,
						200,
						20,
						LeftGreen("���� : " + acc.Point[HarvestStartNumber[page] + index].ToString()),
						false,
						false
					); // <DIV ALIGN=LEFT>Name:</DIV>

				//10�� �� 400 11�� �� 600 13�� �� 1000, ���� 5������ 1000.
				int nextCount = 200;
				if (pm.HarvestPoint[i] < 10)
					nextCount = AllCount[pm.HarvestPoint[i]];
				else if (pm.HarvestPoint[i] < 310)
					nextCount = (pm.HarvestPoint[i] - 9) * 200;
				else if (pm.HarvestPoint[i] < 400)
					nextCount = (pm.HarvestPoint[i] - 250) * 1000;
				else if (pm.HarvestPoint[i] < 485)
					nextCount = (pm.HarvestPoint[i] - 483) * 10000;
				else if (pm.HarvestPoint[i] < 494)
					nextCount = (pm.HarvestPoint[i] - 492) * 1000000;
				else
					nextCount = (pm.HarvestPoint[i] + 1) * 10000000;

				AddHtml(450, 50 + index * 20, 200, 20, LeftGreen("���� : " + nextCount.ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				if (index == 9)
				{
					if (i == 9 && acc.Point[90] >= nextCount)
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
					else if (i == 19 && acc.Point[91] >= nextCount)
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
					else if (i == 29 && acc.Point[92] >= nextCount)
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
					else if (i == 39 && acc.Point[93] >= nextCount)
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				}
				else if (acc.Point[HarvestStartNumber[page] + index] >= nextCount)
					AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				index++;
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			if (info.ButtonID == 99)
			{
				if (m_pm.HarvestGumpPage > 0)
					m_pm.HarvestGumpPage--;
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			else if (info.ButtonID == 98)
			{
				if (m_pm.HarvestGumpPage < m_maxpage)
					m_pm.HarvestGumpPage++;
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			else
			{
				//
				m_pm.HarvestPoint[info.ButtonID - 1]++;
				Misc.Util.HarvestReward(m_pm, info.ButtonID - 1);
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
		}

		private void SendGump()
		{
			m_pm.SendGump(new HarvestGump(m_pm));
		}

		private string CenterGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
		}

		private string LeftGreen(string format)
		{
			return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
		}
	}

	#region Crafting
	public class CraftingGump : Gump
	{
		PlayerMobile m_pm;
		int m_maxpage = 1;
		private string[] CraftName =
		{
			"���� ����",
			"�������� ����",
			"Ȱ&���� ����",
			"��� ����",
			"���� ����",
			"�丮 ����",
			"��ũ�� ����",
			"��� ����",
			"���� ����",
			"�Ӻ��� ����",
		};

		int[] AllCount = { 1, 5, 10, 20, 30, 50, 75, 100, 140, 200 };

		public CraftingGump(PlayerMobile pm)
			: base(50, 50)
		{
			AddPage(0);
			AddBackground(0, 0, 620, 270, 5054);
			AddImageTiled(10, 10, 600, 250, 2624);

			AddHtml(0, 20, 620, 20, CenterGray("���� ���� ��"), false, false); // <CENTER>HOUSE

			m_pm = pm;

			pm.CloseGump(typeof(CraftingGump));
			Account acc = pm.Account as Account;
			int page = pm.CraftGumpPage;
			int index = 0;
			int maxlist = CraftName.Length;
			int step = 10;
			//m_maxpage = 1 + maxlist / step;
			int maxpage = Misc.Util.MaxpageCreate(maxlist, page, step);

			for (int i = 0; i < 10; i++)
			{
				/*
				if( page < m_maxpage -1 )
				{
					AddButton(480, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				if( page > 0 )
				{
					AddButton(450, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}
				*/
				AddHtml(50, 50 + i * 20, 200, 20, LeftGreen(CraftName[i]), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(250, 50 + i * 20, 200, 20, LeftGreen("���� : " + acc.Point[31 + i].ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				/*
				int nextCount = 200;
				if( pm.CraftPoint[i] < 10 )
					nextCount = AllCount[pm.CraftPoint[i]];
				else
					nextCount = ( pm.CraftPoint[i] - 9 ) * 200;
				*/

				int nextCount = 200;
				if (pm.CraftPoint[i] < 10)
					nextCount = AllCount[pm.CraftPoint[i]];
				else if (pm.CraftPoint[i] < 310)
					nextCount = (pm.CraftPoint[i] - 9) * 200;
				else if (pm.CraftPoint[i] < 400)
					nextCount = (pm.CraftPoint[i] - 250) * 1000;
				else if (pm.CraftPoint[i] < 485)
					nextCount = (pm.CraftPoint[i] - 483) * 10000;
				else if (pm.CraftPoint[i] < 494)
					nextCount = (pm.CraftPoint[i] - 492) * 1000000;
				else
					nextCount = (pm.CraftPoint[i] + 1) * 10000000;

				AddHtml(450, 50 + index * 20, 200, 20, LeftGreen("���� : " + nextCount.ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				if (acc.Point[31 + i] >= nextCount)
					AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				index++;
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			/*
			if( info.ButtonID == 99 )
			{
				if( m_pm.CraftGumpPage > 0 )
					m_pm.CraftGumpPage--;
			}
			else if( info.ButtonID == 98 )
			{
				if( m_pm.CraftGumpPage < m_maxpage )
					m_pm.CraftGumpPage++;
			}
			*/
			m_pm.CraftPoint[info.ButtonID - 1]++;
			Misc.Util.CraftReward(m_pm, info.ButtonID - 1);
			m_pm.SendGump(new CraftingGump(m_pm));
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
		}

		private void SendGump()
		{
			m_pm.SendGump(new CraftingGump(m_pm));
		}

		private string CenterGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
		}

		private string LeftGreen(string format)
		{
			return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
		}
	}

	#endregion

	public class MonsterFeatGump : Gump
	{
		private string CenterGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
		}

		private string RightGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=RIGHT>{0}</DIV>", format);
		}

		private string LeftGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=LEFT>{0}</DIV>", format);
		}

		private string LeftGreen(string format)
		{
			return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
		}

		int[] KillCount = { 1, 3, 5, 10, 15, 20, 30, 50, 75, 100 };

		PlayerMobile m_pm;
		int m_maxpage = 1;

		public MonsterFeatGump(PlayerMobile pm)
			: base(50, 50)
		{
			m_pm = pm;
			AddPage(0);
			AddBackground(0, 0, 620, 270, 5054);
			AddImageTiled(10, 10, 600, 250, 2624);

			AddHtml(0, 20, 620, 20, CenterGray("���� ��� ��"), false, false); // <CENTER>HOUSE

			pm.CloseGump(typeof(MonsterFeatGump));
			Account acc = pm.Account as Account;
			int page = pm.MonsterFeatGumpPage;
			int index = 0;
			int maxlist = Misc.Util.m_MonsterItemDrop.GetLength(0);
			m_maxpage = 1 + maxlist / 10;
			int step = 10;
			int maxpage = Misc.Util.MaxpageCreate(maxlist, page, step);

			for (int i = page * step; i < maxpage + page * step; i++)
			{
				if (page < m_maxpage - 1)
				{
					AddButton(480, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				if (page > 0)
				{
					AddButton(450, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}

				AddHtmlLocalized(50, 50 + index * 20, 200, 20, 1052085 + i, "", 0x00FA9A, false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(
					250,
					50 + index * 20,
					200,
					20,
					LeftGreen("���� : " + acc.Point[i + 201].ToString()),
					false,
					false
				); // <DIV ALIGN=LEFT>Name:</DIV>

				int nextCount = 100;
				if (pm.MonsterPoint[i] < 10)
					nextCount = KillCount[pm.MonsterPoint[i]];
				else if (pm.MonsterPoint[i] < 310)
					nextCount = (pm.MonsterPoint[i] - 10) * 100;
				else if (pm.MonsterPoint[i] < 400)
					nextCount = (pm.MonsterPoint[i] - 250) * 500;
				else if (pm.MonsterPoint[i] < 485)
					nextCount = (pm.MonsterPoint[i] - 483) * 5000;
				else if (pm.MonsterPoint[i] < 494)
					nextCount = (pm.MonsterPoint[i] - 492) * 500000;
				else
					nextCount = (pm.MonsterPoint[i] + 1) * 5000000;

				AddHtml(450, 50 + index * 20, 200, 20, LeftGreen("���� : " + nextCount.ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				if (acc.Point[i + 201] >= nextCount)
					AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);

				index++;
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_pm.CheckAlive() || info.ButtonID == 0)
				return;

			if (info.ButtonID == 99)
			{
				if (m_pm.MonsterFeatGumpPage > 0)
					m_pm.MonsterFeatGumpPage--;
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			else if (info.ButtonID == 98)
			{
				if (m_pm.MonsterFeatGumpPage < m_maxpage)
					m_pm.MonsterFeatGumpPage++;
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			else
			{
				m_pm.MonsterPoint[info.ButtonID - 1]++;
				Misc.Util.MonsterFeatReward(m_pm, info.ButtonID - 1);
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
		}

		private void SendGump()
		{
			m_pm.SendGump(new MonsterFeatGump(m_pm));
		}
	}

	public class SavingAccountGump : Gump
	{
		private string[,] Name =
		{
			{
				"���� ä��",
				"ö ä��",
				"���� ä��",
				"û�� ä��",
				"�� ä��",
				"�ư�����Ʈ ä��",
				"������Ʈ ä��",
				"��������Ʈ ä��",
				"�̽��� ä��",
				"�ɽõ�� ä��",
			},
			{
				"���� ä��",
				"�Ϲ� ���� ä��",
				"���� ���� ä��",
				"��Ǫ�� ���� ä��",
				"�ָ� ���� ä��",
				"���� ���� ä��",
				"�� ���� ä��",
				"���� ���� ä��",
				"ĥ�� ���� ä��",
				"���� ���� ä��",
			},
			{
				"���� ä��",
				"�Ϲ� ���� ä��",
				"���� ���� ä��",
				"��ģ ���� ä��",
				"��ȭ ���� ä��",
				"���� ���� ä��",
				"�� ���� ä��",
				"�̴� ���� ä��",
				"���� ���� ä��",
				"��� ���� ä��",
			},
			{
				"���� ����",
				"�������� ����",
				"Ȱ&���� ����",
				"��� ����",
				"���� ����",
				"�丮 ����",
				"��ũ�� ����",
				"��� ����",
				"���� ����",
				"�Ӻ��� ����",
			},
			{ "1,000", "2,000", "3,000", "4,000", "5,000", "6,000", "7,000", "8,000", "9,000", "10,000" },
			{ "11,000", "12,000", "13,000", "14,000", "15,000", "16,000", "17,000", "18,000", "19,000", "20,000" },
			{ "21,000", "22,000", "23,000", "24,000", "25,000", "26,000", "27,000", "28,000", "29,000", "30,000" },
		};

		public SavingAccountGump(PlayerMobile pm, int list)
			: base(50, 50)
		{
			AddPage(0);
			AddBackground(0, 0, 620, 270, 5054);
			AddImageTiled(10, 10, 600, 250, 2624);

			AddHtml(0, 20, 620, 20, CenterGray("���� �ý���"), false, false); // <CENTER>HOUSE
			pm.CloseGump(typeof(SavingAccountGump));

			Account acc = pm.Account as Account;

			for (int i = 0; i < Name.GetLength(1); i++)
			{
				AddHtml(20, 50 + i * 20, 200, 20, LeftGreen(Name[list, i]), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(
					220,
					50 + i * 20,
					200,
					20,
					LeftGreen("���� : " + acc.Point[list * 10 + i + 1].ToString()),
					false,
					false
				); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(
					420,
					50 + i * 20,
					200,
					20,
					LeftGreen("���� : " + Math.Pow(acc.Point[500 + list * 10 + i + 1] + 1, 2).ToString()),
					false,
					false
				); // <DIV ALIGN=LEFT>Name:</DIV>
			}
		}

		private string PointText(int ranking, string point)
		{
			if (point == "0")
				point = "����";
			else
				point += "��";

			if (ranking == 0)
				return String.Format("<basefont color=#FF0090><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if (ranking == 1)
				return String.Format("<basefont color=#FFB400><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if (ranking == 2)
				return String.Format("<basefont color=#B36BFF><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if (ranking == 3 || ranking == 4)
				return String.Format("<basefont color=#68D5ED><DIV ALIGN=CENTER>{0}</DIV>", point);
			else
				return String.Format("<basefont color=#00A000><DIV ALIGN=CENTER>{0}</DIV>", point);
		}

		private string CenterGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
		}

		private string RightGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=RIGHT>{0}</DIV>", format);
		}

		private string LeftGray(string format)
		{
			return String.Format("<basefont color=#A9A9A9><DIV ALIGN=LEFT>{0}</DIV>", format);
		}

		private string LeftGreen(string format)
		{
			return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
		}
	}
}
