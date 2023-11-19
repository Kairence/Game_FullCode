using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Points;

namespace Server.Misc.WorldTimer
{
    public static class WorldTimerGeneration
    {
		/*
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
		*/
        public static void Generate()
        {
			RespawnCheck check = null;
			if( Server.Event.rc == null )
			{
				foreach ( Item item in World.Items.Values )
				{
					if ( item is RespawnCheck )
					{
						RespawnCheck rc = item as RespawnCheck;
						Server.Event.rc = rc;
						check = rc;
						break;
					}
				}				
				Console.WriteLine("Spacial Dungeon Respawn success");
			}
			else
				check = Server.Event.rc;
			
			if( check != null )
			{
				if( check.RespawnTime <= DateTime.Now )
				{
					var list = new List<Item>();
					var baricate = new List<Item>();
					foreach ( Item i in World.Items.Values ) //던전 리셋
					{
						if ( i is XmlSpawner && i.Map == Map.Ilshenar )
						{
							XmlSpawner xs = i as XmlSpawner;
							
							if( xs.X >= 1752 && xs.Y >= 952 && xs.X <= 1864 && xs.Y <= 1000 ) //스파이더 던전 1층
								list.Add( i );
							if( xs.X >= 1480 && xs.Y >= 864 && xs.X <= 1528 && xs.Y <= 896 ) //스파이더 던전 2층
								list.Add( i );
						}
					}
					if( list.Count > 0 )
					{
						for ( int i = 0; i < list.Count; ++i )
						{
							XmlSpawner targeted = (XmlSpawner)list[i];
							targeted.HomeRange = 20;
							targeted.MaxDelay = targeted.MinDelay = TimeSpan.FromDays(1);
							targeted.Reset();
							targeted.Respawn();
						}
					}

					foreach ( Item i in World.Items.Values ) //바리케이트
					{
						if ( i is XmlSpawner && i.Map == Map.Ilshenar )
						{
							XmlSpawner xs = i as XmlSpawner;
								
							if( xs.Y == 990 && xs.X >= 1783 && xs.X <= 1788 ) //스파이더 던전 2층
								baricate.Add( i );
						}
					}
					if( baricate.Count > 0 )
					{
						for ( int i = 0; i < baricate.Count; ++i )
						{
							XmlSpawner targeted = (XmlSpawner)baricate[i];
							targeted.Reset();
							targeted.MaxDelay = targeted.MinDelay = TimeSpan.FromMinutes(10);
							targeted.Start();
						}
					}
					
					check.RespawnTime = DateTime.Now.Date + TimeSpan.FromHours( DateTime.Now.Hour + 1 );
					Console.WriteLine("Reset Special Dungeon.");
				}
					
			}
        }
    }
}
