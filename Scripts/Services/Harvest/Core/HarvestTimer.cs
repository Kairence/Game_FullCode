using System;
using Server.Mobiles;

namespace Server.Engines.Harvest
{
    public class HarvestTimer : Timer
    {
        private readonly Mobile m_From;
        private readonly Item m_Tool;
        private readonly HarvestSystem m_System;
        private readonly HarvestDefinition m_Definition;
        private readonly object m_ToHarvest;
        private readonly object m_Locked;
        private readonly int m_Count;
        private int m_Index;
        public HarvestTimer(Mobile from, Item tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked)
            : base(TimeSpan.Zero, def.EffectDelay)
        {
            this.m_From = from;
            this.m_Tool = tool;
            this.m_System = system;
            this.m_Definition = def;
            this.m_ToHarvest = toHarvest;
            this.m_Locked = locked;
            this.m_Count = Utility.RandomList(def.EffectCounts);
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				int nofishing = 1;
				if( def.Skill is SkillName.Fishing )
					nofishing = 0;
				pm.TimerList[71] = (int)( ( m_Count + nofishing ) * def.EffectDelay.TotalSeconds * 10 );
			}
        }

        protected override void OnTick()
        {
			m_Index++;
			/*
			if( m_From is PlayerMobile )
			{
				PlayerMobile pm = m_From as PlayerMobile;
				if ( pm.TimerList[71] > 0 )
				{
					pm.TimerList[71] = m_TimeCheck;
					pm.SendMessage("움직이는 바람에 자원 채취에 실패했습니다.");
					this.Stop();
				}
			}
			*/
            if (!this.m_System.OnHarvesting(this.m_From, this.m_Tool, this.m_Definition, this.m_ToHarvest, this.m_Locked, this.m_Index == this.m_Count))
                this.Stop();
			
			//m_System.OnHarvestStarted(this.m_From, this.m_Tool, this.m_Definition, this.m_ToHarvest);    this.Stop();
			
        }
    }
}