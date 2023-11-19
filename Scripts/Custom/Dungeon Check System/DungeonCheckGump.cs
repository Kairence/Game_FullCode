using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
//using Server.Engines.CityLoyalty;

namespace Server.Gumps
{
    public class DungeonCheckGump : Gump
    {
		private string[] Name = { "코베투스", "데스파이즈", "디싯", "쉐임", "오크 던전", "롱", "다채로운 동굴" };
		private DungeonCheck m_dungeon;
		private Mobile m_from;
        public DungeonCheckGump(Mobile from, DungeonCheck dungeon) : base(50, 50)
        {
			from.CloseGump(typeof(DungeonCheckGump));

            AddBackground(0, 0, 500, 480, 5054);
			
			m_dungeon = dungeon;
			m_from = from;

            AddHtml(10, 10, 250, 20, "던전 시스템", false, false); // <CENTER>HOUSE 			
			//AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", dungeon.Death[0]), false, false);

            AddHtml(50, 40, 225, 20, "이름", false, false); // House Description
            AddHtml(275, 40, 75, 20, "보상", false, false); // Storage
            //AddHtml(350, 40, 150, 20, "사망", false, false); // Lockdowns			
			int y = 60;
			for ( int i = 0; i < Name.Length; i++ )
			{
				//이름
				AddHtml( 50, y + i * 20, 225, 20, Name[i], false, false);
				//보상
				AddHtml(350, y + i * 20, 200, 16, String.Format("{0:#,###}", dungeon.Death[i]), false, false);
				//버튼
				AddButton( 10, y + i * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				//사망
				//AddHtml(350, y + i * 20, 200, 16, String.Format("{0:#,###}", dungeon.Death[i + 1]), false, false);
			}
			if( from.Str >= 100 )
				AddButton( 10, 260, 20992, 20992, 7, GumpButtonType.Reply, 0); 
			else
				AddImage(10, 260, 20998);
			//AddButton( 10, 260, 20998, 20998, 7, GumpButtonType.Reply, 0); 
				
			AddButton( 60, 260, 20993, 20993, 8, GumpButtonType.Reply, 0); 
			AddButton( 118, 260, 20994, 20994, 9, GumpButtonType.Reply, 0); 
			AddButton( 172, 260, 20995, 20995, 10, GumpButtonType.Reply, 0); 
			AddButton( 226, 260, 20996, 20996, 11, GumpButtonType.Reply, 0); 
			AddButton( 280, 260, 20997, 20997, 12, GumpButtonType.Reply, 0); 
		}

        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_from.CheckAlive() || info.ButtonID == 0)
                return;

			if( info.ButtonID >= 7 )
				return;
			
			m_from.SendMessage("던전 점수를 적으시오");
			m_from.BeginPrompt(
			(from, text ) =>
			{
				int amount = Utility.ToInt32(text);
				if( amount >= 0 )
				{
					m_dungeon.Death[ info.ButtonID - 1 ] = amount;
				}
			});
        }		
	}
}
