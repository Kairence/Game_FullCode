using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text; 
using Server;
using Server.Mobiles;
using Server.Accounting;

namespace AutoUserConnect
{
    public class SummonEntry
    {
        public Type CreatureType { get; set; }
        public double MinTame { get; set; }

        public SummonEntry(Type type, double minTame)
        {
            CreatureType = type;
            MinTame = minTame;
        }
    }

    public class AutoUserConnect : Timer
    {
        public static List<SummonEntry> SummonPool = new List<SummonEntry>();

        public AutoUserConnect()
            : base(TimeSpan.FromSeconds(2.0)) 
        {
            this.Priority = TimerPriority.OneSecond;
        }

        public static void Initialize()
        {
            new AutoUserConnect().Start();
        }

        protected override void OnTick()
        {
            int connectedCount = 0; // 성공 횟수 체크용

            try
            {
                foreach (Account a in Accounts.GetAccounts())
                {
                    if (a == null) continue;

                    int slots = a.CharacterSlotsBonus;
                    for (int j = 0; j < (slots + 1); ++j)
                    {
                        if (a[j] is PlayerMobile pm && !pm.IsStaff() && pm.NetState == null)
                        {
                            pm.MoveToWorld(pm.Location, pm.LogoutMap);
                            connectedCount++;
                        }
                    }
                }

                // 성공 로그: 접속한 유저가 있을 때만 출력 (매우 간결하게)
                if (connectedCount > 0)
                {
                    Console.WriteLine("AutoConnect: {0} players have entered the world.", connectedCount);
                }
            }
            catch (Exception ex)
            {
                // 에러 발생 시에만 출력
                Console.WriteLine("AutoConnect Error: {0}", ex.Message);
            }

            Stop(); 
        }
    }
}