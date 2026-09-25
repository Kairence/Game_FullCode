using System;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public class RegisterSeedTarget : Target
    {
        private readonly PrivateFarmAddon m_Addon;

        public RegisterSeedTarget(PrivateFarmAddon addon) : base(10, false, TargetFlags.None)
        {
            m_Addon = addon;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (targeted is BaseSeed seed)
            {
                if (!seed.IsChildOf(from.Backpack))
                {
                    from.SendMessage(33, "가방에 있는 씨앗만 등록할 수 있습니다.");
                    return;
                }

                string seedName = seed.GetType().FullName;
                int amount = seed.Amount;

                if (!m_Addon.RegisteredSeeds.ContainsKey(seedName))
                    m_Addon.RegisteredSeeds[seedName] = 0;

                m_Addon.RegisteredSeeds[seedName] += amount;
                seed.Delete();

                from.SendMessage(68, $"{amount}개의 씨앗을 농장에 등록했습니다.");
            }
            else
            {
                from.SendMessage(33, "올바른 씨앗이 아닙니다.");
            }
            from.SendGump(new FarmBuilderGump(m_Addon, 0));
        }
    }
}
