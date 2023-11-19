using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Commands;
using Server.Factions;
namespace Server.Gumps
{
	public class ShameMoongateGump : Gump
	{
		private PlayerMobile m_From;
		private ShameMoongate m_SM;
		public ShameMoongateGump(Mobile from, ShameMoongate ss) : base(0, 0)
		{
			m_From = (PlayerMobile)from;
			m_SM = (ShameMoongate)ss;
			Closable = true;

			Dragable = true;

			Resizable = false;

			AddBackground( 100, 95, 200, 180, 9200 );
			AddLabel(120, 102, 0, "���� ���� ����Ʈ");

			AddButton( 120, 152, 2117, 2118, 0x1, GumpButtonType.Reply, 0 ); // Okay
			AddLabel(140, 152, 0, "���� �κ� - ����");
			AddButton( 120, 182, 2117, 2118, 0x2, GumpButtonType.Reply, 0 ); // Okay
			AddLabel(140, 182, 0, "���� 3�� - 2000GP");
			AddButton( 120, 212, 2117, 2118, 0x3, GumpButtonType.Reply, 0 ); // Okay
			AddLabel(140, 212, 0, "���� 4�� - 5000GP");
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1)
			{
				if( m_From.Location != new Point3D( 5507, 162, 5 ) )
				{
					if( m_From.Location == m_SM.Location )
					{
						m_From.Account.WithdrawGold( 1000 );
						BaseCreature.TeleportPets( m_From, new Point3D( 5507, 162, 5 ), Map.Trammel );
						m_From.MoveToWorld( new Point3D( 5507, 162, 5 ), Map.Trammel );
						Effects.PlaySound( new Point3D( 5507, 162, 5 ), Map.Trammel, 0x1FE );
					}
					else
						m_From.SendMessage(1161, "����Ʈ���� �ʹ� ������ �ֽ��ϴ�!");
				}
				else
					m_From.SendMessage(1161, "���� ��ġ�Դϴ�!");
			}
			if ( info.ButtonID == 2)
			{
				if( m_From.Account.TotalGold >= 2000 )
				{
					if( m_From.Location != new Point3D( 5514, 147, 25 ) )
					{
						if( m_From.Location == m_SM.Location )
						{
							m_From.Account.WithdrawGold( 3000 );
							BaseCreature.TeleportPets( m_From, new Point3D( 5514, 147, 25 ), Map.Trammel );
							m_From.MoveToWorld( new Point3D( 5514, 147, 25 ), Map.Trammel );
							Effects.PlaySound( new Point3D( 5514, 147, 25 ), Map.Trammel, 0x1FE );
						}
						else
							m_From.SendMessage(1161, "����Ʈ���� �ʹ� ������ �ֽ��ϴ�!");
					}
					else
						m_From.SendMessage(1161, "���� ��ġ�Դϴ�!");
				}					
				else
					m_From.SendMessage(1161, "3���� ���� ���ؼ��� 2000��� �̻� �ʿ��մϴ�!");
			}
			if ( info.ButtonID == 3)
			{
				if( m_From.Account.TotalGold >= 5000 )
				{
					if( m_From.Location != new Point3D( 5875, 19, -5 ) )
					{
						if( m_From.Location == m_SM.Location )
						{
							m_From.Account.WithdrawGold( 5000 );
							BaseCreature.TeleportPets( m_From, new Point3D( 5875, 19, -5 ), Map.Trammel );
							m_From.MoveToWorld( new Point3D( 5875, 19, -5 ), Map.Trammel );
							Effects.PlaySound( new Point3D( 5875, 19, -5 ), Map.Trammel, 0x1FE );
						}
						else
							m_From.SendMessage(1161, "����Ʈ���� �ʹ� ������ �ֽ��ϴ�!");
					}
					else
						m_From.SendMessage(1161, "���� ��ġ�Դϴ�!");
				}
				else
					m_From.SendMessage(1161, "4���� ���� ���ؼ��� 5000��� �̻� �ʿ��մϴ�!");
			}
		}
	}
}