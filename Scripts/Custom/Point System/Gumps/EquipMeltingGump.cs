using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Multis;
using Server.Accounting;
using System.Collections.Generic;
//using Server.Engines.CityLoyalty;

namespace Server.Gumps
{
    public class EquipMeltingGump : Gump
    {
		PlayerMobile m_pm;

        public EquipMeltingGump(PlayerMobile pm) : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 200, 90, 5054);
            AddImageTiled(10, 10, 180, 70, 2624);
			
			AddHtml(50, 20, 150, 20, CenterGray("아이템 해체"), false, false); // <CENTER>HOUSE
			AddButton(10, 20, 4005, 4007, 1, GumpButtonType.Reply, 0);

			AddHtml(50, 50, 150, 20, CenterGray("옵션 열기"), false, false); // <CENTER>HOUSE
			AddButton(10, 50, 4005, 4007, 2, GumpButtonType.Reply, 0);
			
			m_pm = pm;
			
			pm.CloseGump(typeof(EquipMeltingGump));
			Account acc = pm.Account as Account;
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			if( info.ButtonID == 1 )
			{
				Misc.Util.EquipPointReturn(m_pm);

			}
			else if( info.ButtonID == 2 )
			{
				m_pm.CloseGump(typeof(EquipMeltingGump));
				m_pm.SendGump(new EquipMeltingOptionGump(m_pm));
			}
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
    public class EquipMeltingOptionGump : Gump
    {
		PlayerMobile m_pm;

		string[] rank = {"일반", "희귀", "영웅", "서사", "전설", "신화", "모두" };
		string[] tier = {"0", "40", "80", "120", "160", "200", "240", "모두" };
		string[] named = {"제작", "몬스터", "모두" };
		string[] bag = {"장비 가방", "모두" };
		
        public EquipMeltingOptionGump(PlayerMobile pm) : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 820, 215, 5054);
            AddImageTiled(10, 10, 800, 195, 2624);
			
			AddHtml(0, 20, 800, 20, CenterGray("옵션 설정"), false, false); // <CENTER>HOUSE
			
			m_pm = pm;
			
			pm.CloseGump(typeof(EquipMeltingOptionGump));
			Account acc = pm.Account as Account;
		
			int y = 50;
			
			int step = 150;

			for( int i = 1; i < 6; ++i)
			{
				AddHtml(50 + step * (i -1), y, 50, 20, LeftGreen(rank[i]), false, false); // <DIV 
				AddHtml(150 + step * (i -1), y, 100, 20, LeftGreen(String.Format("{0:#,###}", acc.Point[860 + i] > 0 ? acc.Point[860 + i].ToString() : "0" ) + "점"), false, false);
			}
			
			int index = 1;
			y += 30;
			
			step = 100;
			for( int i = 0; i < rank.Length; ++i)
			{
				if( i != rank.Length - 1 )
				{
					if( m_pm.EquipMeltingOptionRank[i] )
						AddHtml(50 + i * step, y, 50, 20, LeftGreen(rank[i]), false, false); // <DIV 
					else
						AddHtml(50 + i * step, y, 50, 20, LeftRed(rank[i]), false, false); // <DIV 
				}
				else
					AddHtml(50 + i * step, y, 50, 20, LeftGreen(rank[i]), false, false); // <DIV 

				AddButton(10 + i * step, y, 4005, 4007, index, GumpButtonType.Reply, 0);
				index++;
			}
			
			y += 30;

			for( int i = 0; i < tier.Length; ++i)
			{
				if( i != tier.Length - 1 )
				{
					if( m_pm.EquipMeltingOptionTier[i] )
						AddHtml(50 + i * step, y, 50, 20, LeftGreen(tier[i]), false, false); // <DIV 
					else
						AddHtml(50 + i * step, y, 50, 20, LeftRed(tier[i]), false, false); // <DIV 
				}
				else
					AddHtml(50 + i * step, y, 50, 20, LeftGreen(tier[i]), false, false); // <DIV 

				AddButton(10 + i * step, y, 4005, 4007, index, GumpButtonType.Reply, 0);
				index++;
			}
			
			y += 30;
			for( int i = 0; i < named.Length; ++i)
			{
				if( i != named.Length - 1 )
				{
					if( m_pm.EquipMeltingOptionNamed[i] )
						AddHtml(50 + i * step, y, 50, 20, LeftGreen(named[i]), false, false); // <DIV 
					else
						AddHtml(50 + i * step, y, 50, 20, LeftRed(named[i]), false, false); // <DIV 
				}
				else
					AddHtml(50 + i * step, y, 50, 20, LeftGreen(named[i]), false, false); // <DIV 

				AddButton(10 + i * step, y, 4005, 4007, index, GumpButtonType.Reply, 0);
				index++;
			}

			y += 30;
			if( m_pm.EquipMeltingOptionBag )
			{
				AddHtml(50, y, 50, 20, LeftGreen(bag[0]), false, false); // <DIV 
				AddHtml(150, y, 50, 20, LeftRed(bag[1]), false, false); // <DIV 
			}
			else
			{
				AddHtml(150, y, 50, 20, LeftGreen(bag[1]), false, false); // <DIV 
				AddHtml(50, y, 50, 20, LeftRed(bag[0]), false, false); // <DIV 
			}
			AddButton(10, y, 4005, 4007, index, GumpButtonType.Reply, 0);
			AddButton(110, y, 4005, 4007, index + 1, GumpButtonType.Reply, 0);
			index += 1;
		}
		
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			if( info.ButtonID <= 6 )
			{
				m_pm.EquipMeltingOptionRank[info.ButtonID - 1] = !m_pm.EquipMeltingOptionRank[info.ButtonID - 1];
				SendGump();
			}
			else if( info.ButtonID == 7 )
			{
				for( int i = 0; i < m_pm.EquipMeltingOptionRank.Length; ++i )
				{
					m_pm.EquipMeltingOptionRank[i] = true;
				}
				SendGump();
			}
			else if( info.ButtonID <= 14 )
			{
				m_pm.EquipMeltingOptionTier[info.ButtonID - 8] = !m_pm.EquipMeltingOptionTier[info.ButtonID - 8];
				SendGump();
			}
			else if( info.ButtonID == 15 )
			{
				for( int i = 0; i < m_pm.EquipMeltingOptionTier.Length; ++i )
				{
					m_pm.EquipMeltingOptionTier[i] = true;
				}
				SendGump();
			}
			else if( info.ButtonID <= 17 )
			{
				m_pm.EquipMeltingOptionNamed[info.ButtonID - 16] = !m_pm.EquipMeltingOptionNamed[info.ButtonID - 16];
				SendGump();
			}
			else if( info.ButtonID == 18 )
			{
				for( int i = 0; i < m_pm.EquipMeltingOptionNamed.Length; ++i )
				{
					m_pm.EquipMeltingOptionNamed[i] = true;
				}
				SendGump();
			}
			else if( info.ButtonID == 19 )
			{
				m_pm.EquipMeltingOptionBag = true;
				SendGump();
			}
			else if( info.ButtonID == 20 )
			{
				m_pm.EquipMeltingOptionBag = false;
				SendGump();
			}
			else
			{
				m_pm.CloseGump(typeof(EquipMeltingOptionGump));
				m_pm.SendGump(new EquipMeltingGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
        }
        private void SendGump()
        {
			m_pm.CloseGump(typeof(EquipMeltingOptionGump));
            m_pm.SendGump(new EquipMeltingOptionGump(m_pm));
        }
		
        private string CenterGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
        }
        private string LeftGreen(string format)
        {
            return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
        }
        private string LeftRed(string format)
        {
            return String.Format("<basefont color=#FF0000><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
        }
    }		
}