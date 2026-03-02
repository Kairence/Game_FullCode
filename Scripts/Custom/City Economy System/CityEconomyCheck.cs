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

	public abstract class CityEnonomyCheck : Item
	{
		public string[] CityName = new string[]
		{
			"�긮ư",
			"���ɴϾ ��",
			"�ں�",
			"��Ʈ���",
			"����",
			"�����þ�",
			"�̳�",
			"���۷ο�",
			"������",
			"���",
			"����Ʈ Ȧ��",
			"��ī�� �극",
			"Ʈ����",
			"������",
			"����",
			"��",
		};
		private static int[] m_Pop = new int[]
		{
			1000,
			200,
			200,
			300,
			700,
			600,
			750,
			700,
			500,
			500,
			400,
			650,
			900,
			900,
			100,
			600,
		};

		private double[] m_Gold = new double[16];

		[Constructable]
		public CityEnonomyCheck()
			: base(0xED4)
		{
			Movable = false;
			Hue = 0x56;
			Name = "���� ���� �ý���";
		}

		public CityEnonomyCheck(Serial serial)
			: base(serial) { }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
				{
					break;
				}
			}
		}
	}
}
