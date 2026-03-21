using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network; // 패킷 전송을 위해 추가

namespace Server.Misc
{
    public delegate Int32 RegenBonusHandler(Mobile from);

    public class RegenRates
    {
        // [중요] 주변 유저에게 상태를 동기화하는 범용 메서드

		public static void Configure()
        {
            Mobile.DefaultHitsRate = 0;
            Mobile.DefaultStamRate = 0;
            Mobile.DefaultManaRate = 0;

            Mobile.ManaRegenRateHandler = new RegenRateHandler(Mobile_ManaRegenRate);

            if (Core.AOS)
            {
                Mobile.StamRegenRateHandler = new RegenRateHandler(Mobile_StamRegenRate);
                Mobile.HitsRegenRateHandler = new RegenRateHandler(Mobile_HitsRegenRate);
            }
        }

        public static int Mobile_HitsRegenRate(Mobile from)
        {
            int rate = 1; // 기본값

            if (from is PlayerMobile)
            {
                rate = 0; 
            }
            else if (from is BaseCreature)
            {
                from.HitsRegenBonus += from.Str / 200;
                BaseCreature bc = from as BaseCreature;
                if (bc is Troll || bc is FrostTroll)
                    from.HitsRegenBonus *= 3;
                
                rate = 1 + from.HitsRegenBonus;
            }
            
            return rate;
        }

        public static int Mobile_StamRegenRate(Mobile from)
        {
            // 기존 로직 유지...
            int rate = 1 + from.StamRegenBonus;
            return rate;
        }

        public static int Mobile_ManaRegenRate(Mobile from)
        {
            // 기존 로직 유지...
            int rate = 1 + from.ManaRegenBonus;
            return rate;
        }
    }
}