using System.Collections.Generic;
using Server.Mobiles;
using System;
using Server;
using Server.Accounting;
using Server.Items;

namespace AutoUserConnect
{
	public class WorldTimerWeekTime : Timer
	{
        public WorldTimerWeekTime()
            : base(TimeSpan.FromMinutes(0.0), TimeSpan.FromMinutes(30.0))
        {
            this.Priority = TimerPriority.OneMinute;
        }
        public static void Initialize()
        {
            new WorldTimerWeekTime().Start();
		}
        protected override void OnTick()
        {
			Server.Misc.WorldTimer.WorldTimerWeekTimerGeneration.Generate();
		}
	}

	public class AutoBossDungeon : Timer
	{
        public AutoBossDungeon()
            : base(TimeSpan.FromMinutes(0.0), TimeSpan.FromSeconds(5.0))
        {
            this.Priority = TimerPriority.OneSecond;
        }
        public static void Initialize()
        {
            new AutoBossDungeon().Start();
		}
        protected override void OnTick()
        {
			Server.Misc.WorldTimer.WorldTimerGeneration.Generate();
		}
	}

	public class AutoReward : Timer
	{
        public AutoReward()
            : base(TimeSpan.FromMinutes(0.0), TimeSpan.FromMinutes(30.0))
        {
            this.Priority = TimerPriority.OneMinute;
        }
        public static void Initialize()
        {
            new AutoReward().Start();
		}
        protected override void OnTick()
        {
			Server.Engines.LeaderReward.LeaderRewardGeneration.Generate();
		}
	}
	public class AutoUserConnect : Timer
	{
		private bool check = false;
        public AutoUserConnect()
            : base(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(2.0))
        {
            this.Priority = TimerPriority.OneSecond;
        }
		
        public static void Initialize()
        {
            new AutoUserConnect().Start();
		}
        protected override void OnTick()
        {
			if( !check )
			{
				var list = new List<Mobile>();
				var acct = new List<Account>();
				//var DungeonItem = new List<Item>();
				foreach (Account a in Accounts.GetAccounts())
				{
				  acct.Add(a);
				}
				for ( int i = 0; i < acct.Count; i++ )
				{
					Account a = acct[i] as Account;
					int character = a.CharacterSlotsBonus;
					for( int j = 0; j < character + 1; ++j )
					{
						Mobile m = a[j] as Mobile;
						if ( m is PlayerMobile )
						{
							PlayerMobile player = m as PlayerMobile;
							if( !player.IsStaff() )
								list.Add( m );
						}
					}
				}
				for ( int i = 0; i < list.Count; ++i )
				{
					Mobile target = (Mobile)list[i];
					target.MoveToWorld(target.Location, target.LogoutMap);
					Console.WriteLine("{0}", target.Name);
				}
				Console.WriteLine("User All Connect!");
				
				check = true;
			}
			else
				new AutoUserConnect().Stop();
		}
	}
}
