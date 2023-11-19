using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Guilds;
using Server.Network;
using System.Collections.Generic;
using System.Linq;
using Server.Accounting;

namespace Server.Gumps
{
    public class DonationRankingGump : Gump
    {
        public DonationRankingGump(PlayerMobile pm) : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 420, 320, 5054);
            AddImageTiled(10, 10, 400, 300, 2624);
			
			AddHtml(0, 20, 420, 20, CenterGray("기부 랭킹"), false, false); // <CENTER>HOUSE

            AddHtml(20, 60, 200, 20, CenterGray("순위"), false, false); // <DIV ALIGN=CENTER>#:</DIV>
            AddHtml(220, 60, 200, 20, CenterGray("기부 포인트"), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

			DonationCheck check = null;
			if( Server.Event.dc == null )
			{
				foreach ( Item item in World.Items.Values )
				{
					if ( item is DonationCheck )
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
			
			if( check != null )
			{
				try
				{
					for( int i = 0; i < 10; i++ )
					{
						AddHtml(20, 90 + i * 20, 200, 20, RankingText[i], false, false); // <DIV ALIGN=LEFT>Name:</DIV>
						
						
						if( check.DonationList[i] == "" || check.DonationList[i] == null )
							AddHtml(200, 90 + i * 20, 200, 20, String.Format("{0:#,###}", PointText( i, "0" ) ), false, false); // <DIV 
						else
						{
							foreach (Account a in Accounts.GetAccounts())
							{
								if( check.DonationList[i] == a.Username )
								{
									AddHtml(200, 90 + i * 20, 200, 20, String.Format("{0:#,###}", PointText( i, a.DonationPoint.ToString() ) ), false, false); // <DIV 
								}
							}
						}
					}
				}
				catch
				{
				}
			}
        }

		private string[] RankingText = 
		{
			"<basefont color=#FF0090><DIV ALIGN=CENTER>1등</DIV>",
			"<basefont color=#FFB400><DIV ALIGN=CENTER>2등</DIV>",
			"<basefont color=#B36BFF><DIV ALIGN=CENTER>3등</DIV>",
			"<basefont color=#68D5ED><DIV ALIGN=CENTER>4등</DIV>",
			"<basefont color=#68D5ED><DIV ALIGN=CENTER>5등</DIV>",
			"<basefont color=#00A000><DIV ALIGN=CENTER>6등</DIV>",
			"<basefont color=#00A000><DIV ALIGN=CENTER>7등</DIV>",
			"<basefont color=#00A000><DIV ALIGN=CENTER>8등</DIV>",
			"<basefont color=#00A000><DIV ALIGN=CENTER>9등</DIV>",
			"<basefont color=#00A000><DIV ALIGN=CENTER>10등</DIV>"
		};

		private string PointText( int ranking, string point ) 
		{
			if( point == "0" )
				point = "없음";
			
			else
				point += "점";
			
			if( ranking == 0 )
				return String.Format("<basefont color=#FF0090><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if( ranking == 1 )
				return String.Format("<basefont color=#FFB400><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if( ranking == 2 )
				return String.Format("<basefont color=#B36BFF><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if( ranking == 3 || ranking == 4 )
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

        private string RightGreen(string format)
        {
            return String.Format("<basefont color=#00FA9A><DIV ALIGN=RIGHT>{0}</DIV>", format);
        }
    }
}