using System;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Points;

namespace Server.Engines.LeaderReward
{
    public static class LeaderRewardGeneration
    {
        public static void Initialize()
        {
            EventSink.WorldSave += OnWorldSave;
        }

        private static void OnWorldSave(WorldSaveEventArgs e)
        {
            CheckEnabled(true);
        }

        public static void CheckEnabled(bool timed = false)
        {
        }

        public static void Generate()
        {
            FirstSkillCheck Leader = null;
			if( Server.Event.fsc == null )
			{
				foreach ( Item item in World.Items.Values )
				{
					if ( item is FirstSkillCheck )
					{
						FirstSkillCheck fs = item as FirstSkillCheck;
						Server.Event.fsc = fs;
						Leader = fs;
						break;
					}
				}				
				Console.WriteLine("Success FirstSkilLCheck");
			}
			else
				Leader = Server.Event.fsc;

			if( Leader != null )
			{
				if( Leader.LeaderRewardTime <= DateTime.Now )
				{
					for( int i = 0; i < Leader.Reader.Length; i++ )
					{
						if( Leader.Reader[i] == null )
							continue;
						else if( Leader.Reader[i] is PlayerMobile )
						{
							Banker.Deposit( Leader.Reader[i], 50000 );
							/*
							PlayerMobile pm = Leader.Reader[i] as PlayerMobile;
							pm.EquipPoint[0] += 500;
							*/
						}
					}
					int weekcheck = 0;
					switch ( DateTime.Now.DayOfWeek )
					{
						case DayOfWeek.Monday: //월
						weekcheck = 5;
						break;

						case DayOfWeek.Tuesday: //화
						weekcheck = 4;
						break;

						case DayOfWeek.Wednesday: //수
						weekcheck = 3;
						break;

						case DayOfWeek.Thursday: //목
						weekcheck = 2;
						break;

						case DayOfWeek.Friday: //금
						weekcheck = 1;
						break;

						case DayOfWeek.Saturday: //토
						weekcheck = 7;
						break;

						case DayOfWeek.Sunday: //일
						weekcheck = 6;
						break;
					}					
					Leader.LeaderRewardTime = DateTime.Now.Date + TimeSpan.FromDays( weekcheck );
					Console.WriteLine("Reset Leader Reward");
				}
				else
					Console.WriteLine("Leader Reward not yet");
					
			}
        }

        public static void Remove()
        {
        }
    }
}
