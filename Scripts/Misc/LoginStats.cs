using System;
using Server.Network;
using Server.Engines.Quests;
using Server.Mobiles;
using System.Collections.Generic;
using System.Linq;

namespace Server.Misc
{
    public class LoginStats
    {
        public static void Initialize()
        {
            // Register our event handler
            EventSink.Login += new LoginEventHandler(EventSink_Login);
        }

        private static void EventSink_Login(LoginEventArgs args)
        {
            int userCount = NetState.Instances.Count;
            int itemCount = World.Items.Count;
            int mobileCount = World.Mobiles.Count;

            Mobile m = args.Mobile;

            m.SendMessage("카이렌스 서버에 오신 것을 환영합니다!");
			
			/*
                args.Mobile.Name,
                userCount == 1 ? "is" : "are",
                userCount, userCount == 1 ? "" : "s",
                itemCount, itemCount == 1 ? "" : "s",
                mobileCount, mobileCount == 1 ? "" : "s");
			*/

            if (m.IsStaff())
            {
                Server.Engines.Help.PageQueue.Pages_OnCalled(m);
            }
        }
    }
}