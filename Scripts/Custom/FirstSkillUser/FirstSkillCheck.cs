using System;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	/*
		���� 100 ����
	*/

	public class FirstSkillCheck : Item
	{
		private bool[] m_Skill = new bool[100];
		private string[] m_User = new string[100];
		private int[] m_SkillStack = new int[100];
		private Mobile[] m_Reader = new Mobile[100];
		private DateTime m_LeaderRewardTime = DateTime.Now;

		public DateTime LeaderRewardTime
		{
			get { return m_LeaderRewardTime; }
			set
			{
				m_LeaderRewardTime = value;
				InvalidateProperties();
			}
		}

		public bool[] Skill
		{
			get { return m_Skill; }
			set
			{
				m_Skill = value;
				InvalidateProperties();
			}
		}
		public int[] SkillStack
		{
			get { return m_SkillStack; }
			set
			{
				m_SkillStack = value;
				InvalidateProperties();
			}
		}

		public string[] User
		{
			get { return m_User; }
			set
			{
				m_User = value;
				InvalidateProperties();
			}
		}

		public Mobile[] Reader
		{
			get { return m_Reader; }
			set
			{
				m_Reader = value;
				InvalidateProperties();
			}
		}

		public override string DefaultName
		{
			get { return "���� ��ų üũ �ý���"; }
		}

		[Constructable]
		public FirstSkillCheck()
			: base(0xED4)
		{
			Movable = false;
			Hue = 1168;
			Name = "���� ��ų üũ �ý���";
		}

		public string[] SkillName =
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
			"���",
			"�������ۼ�",
			"�丮",
			"���Ű���",
			"����ȭ��",
			"������",
			"ġ���",
			"����",
			"�ž�",
			"�����",
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
			"������",
			"���� ���",
			"�ݻ�Ű�",
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
			"���Ž�",
			"�ϼ�",
			"���Ҽ�",
			"�ź��",
			"�Ӻ���",
			"������",
		};

		public override void OnDoubleClick(Mobile from)
		{
			from.CloseGump(typeof(FirstSkillCheckGump));
			from.SendGump(new FirstSkillCheckGump(from, this));
		}

		public FirstSkillCheck(Serial serial)
			: base(serial) { }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)3); // version

			writer.Write((DateTime)m_LeaderRewardTime);

			for (int i = 0; i < 100; i++)
			{
				writer.Write((Mobile)m_Reader[i]);
			}
			for (int i = 0; i < 100; i++)
			{
				if (m_Skill[i] && m_SkillStack[i] == 0)
					m_SkillStack[i] = 1;
				writer.Write((int)m_SkillStack[i]);
			}
			for (int i = 0; i < 100; i++)
			{
				writer.Write((bool)m_Skill[i]);
				writer.Write((string)m_User[i]);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 3:
				{
					m_LeaderRewardTime = reader.ReadDateTime();
					goto case 2;
				}
				case 2:
				{
					for (int i = 0; i < 100; i++)
					{
						m_Reader[i] = reader.ReadMobile();
					}

					goto case 1;
				}
				case 1:
				{
					for (int i = 0; i < 100; i++)
					{
						m_SkillStack[i] = reader.ReadInt();
					}

					goto case 0;
				}
				case 0:
				{
					for (int i = 0; i < 100; i++)
					{
						m_Skill[i] = reader.ReadBool();
						m_User[i] = reader.ReadString();
					}
					break;
				}
			}
		}
	}
}
