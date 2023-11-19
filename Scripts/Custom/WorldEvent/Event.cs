using Server.Items;
using Server.Mobiles;
using System;

namespace Server
{
	public class Event
	{
		public int ServerEvent = 0;
		public bool TGEvent = false;
		public bool VacanceEvent = false;
		public bool ChristmasEvent = false;
		public bool NewyearEvent = false;
		public bool StartEvent = true;
		public static DungeonCheck dungeoncheck = null;
		public static FirstSkillCheck fsc = null;
		public static RespawnCheck rc = null;
		public static DonationCheck dc = null;
		public static LottoCheck lc = null;
		public bool PaintedCaves = false;

		public DateTime PaintedCavesStart = DateTime.Now;
		public int PaintedCavesRound = 1;
		public void PaintedCavesEvent(int Stage = 0)
		{
			if( Stage == 1 )
			{
				Static stagewall1 = new Static(2272);
				stagewall1.MoveToWorld( new Point3D( 6267, 879, 1 ));
				Static stagewall2 = new Static(2272);
				stagewall2.MoveToWorld( new Point3D(  6267, 880, 1 ));
				Static stagewall3 = new Static(2272);
				stagewall3.MoveToWorld( new Point3D(  6267, 878, -1 ));
				Static stagewall4 = new Static(2272);
				stagewall4.MoveToWorld( new Point3D(  6267, 879, -2 ));
				Static stagewall5 = new Static(2272);
				stagewall5.MoveToWorld( new Point3D(  6267, 880, -1 ));
			}
			
			//if( 
			//dungeoncheck = DateTime.Now + TimeSpan.FromMinutes( 
			
		}
	}
}
