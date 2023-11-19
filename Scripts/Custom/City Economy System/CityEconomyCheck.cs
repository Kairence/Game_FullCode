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
	
	public abstract class CityEnonomyCheck : Item 
	{
		public string[] CityName = new string[] { "브리튼", "부케니어스 덴", "코브", "허트우드", "젤롬", "마진시아", "미녹", "문글로우", "누젤롬", "헤븐", "서펜트 홀드", "스카라 브레", "트린식", "베스퍼", "윈드", "유" };
		private static int[] m_Pop = new int[] { 1000, 200, 200, 300, 700, 600, 750, 700, 500, 500, 400, 650, 900, 900, 100, 600 };

		private double[] m_Gold = new double[16];
		[Constructable]
		public CityEnonomyCheck() : base( 0xED4 )
		{
		Movable = false;
		Hue = 0x56;
		Name = "도시 경제 시스템";
		}

		public CityEnonomyCheck( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{

					break;
				}
			}
		}
	}
}
