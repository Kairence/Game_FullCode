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

	public class RespawnCheck : Item
	{
		private DateTime m_RespawnTime = DateTime.Now;

		public DateTime RespawnTime
		{
			get { return m_RespawnTime; }
			set
			{
				m_RespawnTime = value;
				InvalidateProperties();
			}
		}

		public override string DefaultName
		{
			get { return "���� ���� üũ �ý���"; }
		}

		[Constructable]
		public RespawnCheck()
			: base(0xED4)
		{
			Movable = false;
			Hue = 1168;
			Name = "���� ���� üũ �ý���";
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendMessage(m_RespawnTime.ToString());
		}

		public RespawnCheck(Serial serial)
			: base(serial) { }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			writer.Write((DateTime)m_RespawnTime);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			m_RespawnTime = reader.ReadDateTime();
		}
	}
}
