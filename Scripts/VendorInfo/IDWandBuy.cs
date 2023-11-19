using System;
using Server.Items;

namespace Server.Mobiles
{
    public class IDWandBuyInfo : GenericBuyInfo
    {
        private readonly int m_Content;
        public IDWandBuyInfo(Type type, int content, int price, int amount, int itemID, int hue)
            : this(null, type, content, price, amount, itemID, hue)
        {
        }

        public IDWandBuyInfo(string name, Type type, int content, int price, int amount, int itemID, int hue)
            : base(name, type, price, amount, itemID, hue)
        {
            m_Content = content;

			/*
			if( type == typeof(IDWand))
			{
				Item item;
				switch(m_Content)
				{
					case 1: Name = "하급 마법 물품 완드";
					break;
					case 2: Name = "중급 마법 물품 완드";
					break;
					case 3: Name = "상급 마법 물품 완드";
					break;
					case 4: Name = "최상급 마법 물품 완드";
					break;
					case 5: Name = "중급 유물 완드";
					break;
					case 6: Name = "상급 유물 완드";
					break;
					case 7: Name = "최상급 유물 완드";
					break;
					case 8: Name = "전설 유물 완드";
					break;
				}				
			}
			*/
        }

        public override bool CanCacheDisplay
        {
            get
            {
                return false;
            }
        }
        public override IEntity GetEntity()
        {
            return (IEntity)Activator.CreateInstance(Type, new object[] { m_Content });
        }
    }
}