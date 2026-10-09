using System;
using Server.Items; // IronOre 참조를 위해 필요할 수 있습니다.
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
            m_From = from;
            m_Tool = tool;
            m_System = system;
            m_Definition = def;
            m_ToHarvest = toHarvest;
            m_Locked = locked;

            Type expectedType = typeof(IronOre); // 기본값 
            
            // C# 패턴 매칭 및 TryGetValue를 사용한 최적화
            if (system.PreRolledHarvest.TryGetValue(from, out var preRolled) && preRolled.Type != null)
            {
                expectedType = preRolled.Type;
            }

            // 불필요한 기존 난수 할당(Utility.RandomList) 제거 후 다이나믹 공식 적용
            m_Count = system.GetHarvestAttemptCount(from, tool, expectedType);
        }

        protected override void OnTick()
        {
            m_Index++;

            if (!m_System.OnHarvesting(m_From, m_Tool, m_Definition, m_ToHarvest, m_Locked, m_Index == m_Count))
                Stop();
        }
    }
}