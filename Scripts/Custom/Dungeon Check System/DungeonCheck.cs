using System;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	/*
		�긮ư : 1000
		���ɴϾ �� : 300
		�ں� : 200
		��Ʈ��� : 300
		���� : 700
		�����þ� : 600
		�̳� : 750
		���۷ο� : 700
		������ : 500
		��� : 500
		����Ʈ Ȧ�� : 400
		��ī�� �극 : 650
		Ʈ���� : 900
		������ : 900
		���� : 100
		�� : 600
	*/

	public class DungeonCheck : Item
	{
		//private string[] DungeonName = new string[] { "�ں�����", "����������", "����" };
		private int[] m_Death = new int[100];

		public int[] Death
		{
			get { return m_Death; }
			set
			{
				m_Death = value;
				InvalidateProperties();
			}
		}

		public override string DefaultName
		{
			get { return "���� üũ �ý���"; }
		}

		[Constructable]
		public DungeonCheck()
			: base(0xED4)
		{
			Movable = false;
			Hue = 1168;
			Name = "���� üũ �ý���";
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.CloseGump(typeof(DungeonCheckGump));
			from.SendGump(new DungeonCheckGump(from, this));
		}

		public DungeonCheck(Serial serial)
			: base(serial) { }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			for (int i = 0; i < 100; i++)
			{
				writer.Write((int)m_Death[i]);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
				{
					for (int i = 0; i < 100; i++)
					{
						m_Death[i] = reader.ReadInt();
					}
					break;
				}
			}
		}
	}
}
