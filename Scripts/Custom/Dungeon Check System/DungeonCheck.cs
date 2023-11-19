using System;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
	/*
		브리튼 : 1000
		부케니어스 덴 : 300
		코브 : 200
		허트우드 : 300
		젤롬 : 700
		마진시아 : 600
		미녹 : 750
		문글로우 : 700
		누젤롬 : 500
		헤븐 : 500
		서펜트 홀드 : 400
		스카라 브레 : 650
		트린식 : 900
		베스퍼 : 900
		윈드 : 100
		유 : 600
	*/
	
	public class DungeonCheck : Item 
	{
		//private string[] DungeonName = new string[] { "코베투스", "데스파이즈", "쉐임" };
		private int[] m_Death = new int[100];

		public int[] Death
		{
			get{ return m_Death;}
			set{ m_Death = value; InvalidateProperties();}
		}

		public override string DefaultName
		{
			get { return "던전 체크 시스템"; }
		}
		[Constructable]
		public DungeonCheck() : base( 0xED4 )
		{
			Movable = false;
			Hue = 1168;
			Name = "던전 체크 시스템";
		}
		public override void OnDoubleClick( Mobile from )
		{
			from.CloseGump( typeof( DungeonCheckGump ) );
			from.SendGump( new DungeonCheckGump( from, this ) );
		}

		public DungeonCheck( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			for (int i = 0; i < 100; i++)
			{
				writer.Write( (int) m_Death[i] );
			}
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
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
