using System;
using Server.Items;

namespace Server.ContextMenus
{
    public class AllEatEntry : ContextMenuEntry
    {
        private readonly Mobile m_From;
        private readonly Food m_Food;

        public AllEatEntry(Mobile from, Food food)
            : base(6312, 1)
        {
            m_From = from;
            m_Food = food;
        }

        public override void OnClick()
        {
			bool notmagicalfood = true;
			if( m_Food is BaseMagicalFood || !m_Food.Stackable || m_Food.Amount <= 1 )
				notmagicalfood = false;
            m_Food.TryEat(m_From, notmagicalfood);
        }
    }
}