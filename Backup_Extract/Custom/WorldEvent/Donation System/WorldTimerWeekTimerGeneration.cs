using System;
using Server;
using Server.Items;
using Server.Accounting;

namespace Server.Misc.WorldTimer
{
    public static class WorldTimerWeekTimerGeneration
    {
        public static void Generate()
        {
            DonationCheck dc = Server.Event.dc;
            if (dc == null || dc.RespawnTime > DateTime.Now) 
            {
                return;
            }

            try
            {
                for (int cat = 0; cat < 4; cat++)
                {
                    // VP(누적 승점) 기반으로 주간 최종 순위 산출 후 보상
                    foreach (var entry in Server.Event.WeeklyVP[cat])
                    {
                        Account a = Accounts.GetAccount(entry.Key) as Account;
                        if (a != null)
                        {
                            a.Point[0] += (entry.Value / 10);
                        }
                    }

                    // 100위까지의 랭킹 보드 초기화
                    for (int i = 0; i < 100; i++)
                    {
                        dc.RankingNames[cat][i] = "";
                        dc.RankingScores[cat][i] = 0;
                        dc.IsNpc[cat][i] = false;
                    }
                    Server.Event.WeeklyVP[cat].Clear();
                }
            }
            catch 
            { 
            }

            dc.RespawnTime = DateTime.Now.Date + TimeSpan.FromDays(Misc.Util.WeekCal());
            Console.WriteLine("[Family System] Weekly Reset Complete.");
        }
    }
}