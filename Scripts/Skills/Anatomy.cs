using System;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System.Collections.Generic;
using Server.Items;
using Server.Targets;

namespace Server.SkillHandlers
{
    public class Anatomy
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Anatomy].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				
				if( pm.Fury > 50 + (int)( pm.Skills.Anatomy.Value / 5 ) )
					m.SendMessage("이미 충분히 분노하고 있습니다." );
				else if( pm.Stam >= 30 )
				{
					pm.Fury = 50 + (int)( pm.Skills.Anatomy.Value / 5 );
					m.SendMessage("당신은 {0} 만큼의 분노를 축적합니다.", pm.Fury );
					pm.Stam -= 30;
				}
				else
					m.SendMessage("당신은 기력이 없습니다." );
			}
            return TimeSpan.FromSeconds(1.0);
        }

        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(8, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
				/*
				//아나토미 가죽 채취
				if( from.Backpack != null && SkinningKnife.IsChildOf( from.Backpack) )
				{
					from.SendMessage("시체를 더블클릭 하세요");
					Item item = targeted as Item;
					from.Target = new BladedItemTarget( SkinningKnife );
				}


                if (from == targeted)
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500324); // You know yourself quite well enough already.
                }
                else if (targeted is TownCrier)
                {
                    ((TownCrier)targeted).PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500322, from.NetState); // This person looks fine to me, though he may have some news...
                }
                else if (targeted is BaseVendor && ((BaseVendor)targeted).IsInvulnerable)
                {
                    ((BaseVendor)targeted).PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500326, from.NetState); // That can not be inspected.
                }
                else if (targeted is Mobile)
                {
                    Mobile targ = (Mobile)targeted;

                    int marginOfError = Math.Max(0, 25 - (int)(from.Skills[SkillName.Anatomy].Value / 4));

                    int str = targ.Str + Utility.RandomMinMax(-marginOfError, +marginOfError);
                    int dex = targ.Dex + Utility.RandomMinMax(-marginOfError, +marginOfError);
                    int stm = ((targ.Stam * 100) / Math.Max(targ.StamMax, 1)) + Utility.RandomMinMax(-marginOfError, +marginOfError);

                    int strMod = str / 10;
                    int dexMod = dex / 10;
                    int stmMod = stm / 10;

                    if (strMod < 0)
                        strMod = 0;
                    else if (strMod > 10)
                        strMod = 10;

                    if (dexMod < 0)
                        dexMod = 0;
                    else if (dexMod > 10)
                        dexMod = 10;

                    if (stmMod > 10)
                        stmMod = 10;
                    else if (stmMod < 0)
                        stmMod = 0;

                }
                else if (targeted is Item)
                {
                    ((Item)targeted).SendLocalizedMessageTo(from, 500323, ""); // Only living things have anatomies!
                }
				*/
            }
        }
    }
}