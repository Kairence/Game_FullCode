using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Points;
using Server.Accounting;

namespace Server.Misc.WorldTimer
{
    public static class WorldTimerWeekTimerGeneration
    {
        public static void Generate()
        {
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
				Console.WriteLine("Donation Respawn success");
			}
			else
				check = Server.Event.dc;

			LottoCheck lottocheck = null;
			if( Server.Event.lc == null )
			{
				foreach ( Item item in World.Items.Values )
				{
					if ( item is LottoCheck )
					{
						LottoCheck lc = item as LottoCheck;
						Server.Event.lc = lc;
						lottocheck = lc;
						break;
					}
				}				
				Console.WriteLine("lottocheck Respawn success");
			}
			else
				lottocheck = Server.Event.lc;

			int weekcheck = Misc.Util.WeekCal();

			
			if( check != null )
			{
				if( check.RespawnTime <= DateTime.Now )
				{
					try
					{
						foreach (Account a in Accounts.GetAccounts())
						{
							for( int i = 0; i < 1000; i++ )
							{
								if( check.DonationList[i] == "" || check.DonationList[i] == null )
									break;
								else
								{
									if( check.DonationList[i] == a.Username )
									{
										if( i == 0 )
											a.Point[0] += 100;
										else if( i == 1 )
											a.Point[0] += 70;
										else if( i == 2 )
											a.Point[0] += 50;
										else if( i >= 4 && i <= 5 )
											a.Point[0] += 30;
										else if( i >= 6 && i <= 10 )
											a.Point[0] += 20;
										else
											a.Point[0] += 10;
									}
									check.DonationList[i] = "";
								}
								a.DonationPoint = 0;
							}
						}
					}
					catch
					{
					}
					check.RespawnTime = DateTime.Now.Date + TimeSpan.FromDays( weekcheck );
					Console.WriteLine("Reset Donation System.");
				}
			}
			if( lottocheck != null )
			{
				if( lottocheck.RespawnTime <= DateTime.Now )
				{
					lottocheck.LottoNumber = Utility.RandomMinMax( 1, 10000 );
					try
					{
						foreach (Account a in Accounts.GetAccounts())
						{
							for( int i = 0; i < 1000; i++ )
							{
								if( a.Lotto == 0 || a.Lotto == null )
									break;
								else
								{
									int two = lottocheck.LottoNumber;
									two /= 10;
									two *= 10;
									
									int three = two;
									three /= 100;
									three *= 100;
									
									int four = three;
									four /= 1000;
									four *= 1000;
									
									if( lottocheck.LottoNumber == a.Lotto )
										a.DepositGold( 10000000 );
									else if( two >= a.Lotto && two + 9 <= a.Lotto )
										a.DepositGold( 1000000 );
									else if( three >= a.Lotto && three + 99 <= a.Lotto )
										a.DepositGold( 100000 );
									else if( four >= a.Lotto && four + 999 <= a.Lotto )
										a.DepositGold( 10000 );
									
									a.Lotto = 0;
								}
							}
						}
					}
					catch
					{
					}
					lottocheck.RespawnTime = DateTime.Now.Date + TimeSpan.FromDays( weekcheck );
					Console.WriteLine("Reset Lotto System.");
				}
			}			
        }
    }
}
