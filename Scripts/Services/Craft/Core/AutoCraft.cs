using System;
using Server;
using Server.Prompts;
using System.Collections.Generic;
using Server.Items;

namespace Server.Engines.Craft
{
    public class MakeNumberCraftPrompt : Prompt
    {
        private Mobile m_From;
        private CraftSystem m_CraftSystem;
        private CraftItem m_CraftItem;
        private ITool m_Tool;

        public MakeNumberCraftPrompt(Mobile from, CraftSystem system, CraftItem item, ITool tool)
        {
            m_From = from;
            m_CraftSystem = system;
            m_CraftItem = item;
            m_Tool = tool;
        }

        public override void OnCancel(Mobile from)
        {
            m_From.SendLocalizedMessage(501806); //Request cancelled.
            from.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null));
        }

        public override void OnResponse(Mobile from, string text)
        {
            int amount = Utility.ToInt32(text);

            if (amount < 1 || amount > 100)
            {
                from.SendLocalizedMessage(1112587); // Invalid Entry.
                ResendGump();
            }
            else
            {
                AutoCraftTimer.EndTimer(from);
                new AutoCraftTimer(m_From, m_CraftSystem, m_CraftItem, m_Tool, amount, TimeSpan.FromSeconds(m_CraftSystem.Delay * m_CraftSystem.MaxCraftEffect + 1.0), TimeSpan.FromSeconds(m_CraftSystem.Delay * m_CraftSystem.MaxCraftEffect + 1.0));

                CraftContext context = m_CraftSystem.GetContext(from);

                if (context != null)
                    context.MakeTotal = amount;
            }
        }

        public void ResendGump()
        {
            m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null));
        }
    }

    public class AutoCraftTimer : Timer
    {
        private static Dictionary<Mobile, AutoCraftTimer> m_AutoCraftTable = new Dictionary<Mobile, AutoCraftTimer>();
        public static Dictionary<Mobile, AutoCraftTimer> AutoCraftTable { get { return m_AutoCraftTable; } }

        private Mobile m_From;
        private CraftSystem m_CraftSystem;
        private CraftItem m_CraftItem;
        private ITool m_Tool;
        private int m_Amount;
        private int m_Attempts;
        private int m_Ticks;
        private Type m_TypeRes;

        public int Amount { get { return m_Amount; } }
        public int Attempts { get { return m_Attempts; } }

        public AutoCraftTimer(Mobile from, CraftSystem system, CraftItem item, ITool tool, int amount, TimeSpan delay, TimeSpan interval)
            : base(delay, interval)
        {
            m_From = from;
            m_CraftSystem = system;
            m_CraftItem = item;
            m_Tool = tool;
            m_Amount = amount;
            m_Ticks = 0;
            m_Attempts = 0;

            CraftContext context = m_CraftSystem.GetContext(m_From);

            if (context != null)
            {
                CraftSubResCol res = (m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);
                int resIndex = (m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                    m_TypeRes = res.GetAt(resIndex).ItemType;
            }

            m_AutoCraftTable[from] = this;

            this.Start();
        }

        public AutoCraftTimer(Mobile from, CraftSystem system, CraftItem item, ITool tool, int amount)
            : this(from, system, item, tool, amount, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3))
        {
        }

        protected override void OnTick()
        {
            m_Ticks++;

            if (m_From.NetState == null)
            {
                EndTimer(m_From);
                return;
            }

            CraftItem();

            if (m_Ticks >= m_Amount)
                EndTimer(m_From);
        }

        private void CraftItem()
        {
            if (m_From.HasGump(typeof(CraftGump)))
                m_From.CloseGump(typeof(CraftGump));

            if (m_From.HasGump(typeof(CraftGumpItem)))
                m_From.CloseGump(typeof(CraftGumpItem));

            m_Attempts++;

			ITool NextTool = null;
			
			if( m_Tool.UsesRemaining == 1 )
			{
				Container pack = m_From.Backpack;
				if( pack != null )
				{
					List<Item> tools = pack.FindItemsByType<Item>();
					for ( int i = tools.Count -1; i >= 0; i--)
					{
						Item toolitem = tools[i];
						if( toolitem is BaseTool )
						{
							BaseTool newTool = toolitem as BaseTool;
							if( m_Tool is BaseTool )
							{
								BaseTool oldTool = m_Tool as BaseTool; 
								if( newTool is IUsesRemaining )
								{
									if( newTool.ItemID == oldTool.ItemID && oldTool != newTool )
									{
										NextTool = newTool;
										break;
									}
								}
							}
						}
					}
				}				
			}
			
            if (m_CraftItem.TryCraft != null)
            {
                m_CraftItem.TryCraft(m_From, m_CraftItem, m_Tool);
            }
            else
            {
                m_CraftSystem.CreateItem(m_From, m_CraftItem.ItemType, m_TypeRes, m_Tool, m_CraftItem);
            }
			if( NextTool != null )
				m_Tool = NextTool;
        }

        public static void EndTimer(Mobile from)
        {
            if (m_AutoCraftTable.ContainsKey(from))
            {
                m_AutoCraftTable[from].Stop();
                m_AutoCraftTable.Remove(from);
            }
        }

        public static bool HasTimer(Mobile from)
        {
            return from != null && m_AutoCraftTable.ContainsKey(from);
        }
    }
}
