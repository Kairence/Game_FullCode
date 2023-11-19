using System;
using Server.Network;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    public class FoodDecayTimer : Timer
    {
        public FoodDecayTimer()
            : base(TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1))
        {
            this.Priority = TimerPriority.OneMinute;
        }

        public static void Initialize()
        {
            new FoodDecayTimer().Start();
        }

        public static void FoodDecay()
        {
            foreach (NetState state in NetState.Instances)
            {
                HungerDecay(state.Mobile);
                ThirstDecay(state.Mobile);
            }
        }

        public static void HungerDecay(Mobile m)
        {
            if (m != null && m.Hunger >= 1 )
			{
				int hungry = 10 + m.TotalWeight / 5;
				DungeonRegion dungeon = (DungeonRegion)m.Region.GetRegion(typeof(DungeonRegion));
				if( dungeon != null || m.Warmode )
					hungry *= 5;

				m.Hunger -= hungry;
				if( m.Hunger < 0 )
					m.Hunger = 0;
				
				if( m is PlayerMobile && m.Hunger <= 2000 )
					m.SendMessage("당신은 배가 매우 고파 보입니다.");
			}
        }

        public static void ThirstDecay(Mobile m)
        {
            if (m != null && m.Thirst >= 1)
                m.Thirst -= 1;
        }

        protected override void OnTick()
        {
            FoodDecay();			
        }
    }
}