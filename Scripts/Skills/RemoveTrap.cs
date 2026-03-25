using System;
using Server.Items;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class RemoveTrap
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.RemoveTrap].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            double skill = m.Skills[SkillName.RemoveTrap].Value;
            m.Target = new InternalTarget();
            m.SendLocalizedMessage(502368); // Which trap will you attempt to disarm?

            if (skill >= 200.0)
                return TimeSpan.FromSeconds(10.0);

            return TimeSpan.FromSeconds(30.0);
        }

        // --- [설계 변경: 공용 함정 해제 메서드] ---
        public static void OnRemove(Mobile from, object targeted, bool isMagic, double magicPower)
        {
            double srcSkill = from.Skills[SkillName.RemoveTrap].Value;

            if (targeted is TrapableContainer)
            {
                TrapableContainer targ = (TrapableContainer)targeted;
                if (targ.TrapType == TrapType.None)
                {
                    from.SendLocalizedMessage(502373); // That doesn't appear to be trapped
                    return;
                }

                // 마법 시전 시 물리 함정(Dart, Poison, Explosion)은 해제 불가
                if (isMagic && targ.TrapType != TrapType.MagicTrap)
                {
                    from.SendMessage("마법으로는 물리적인 함정을 해제할 수 없습니다.");
                    return;
                }

                if (isMagic)
                {
                    // 마법 해제 로직
                    if (magicPower >= targ.TrapPower)
                    {
                        targ.TrapPower = 0;
                        targ.TrapLevel = 0;
                        targ.TrapType = TrapType.None;
                        from.SendLocalizedMessage(502377);
                    }
                    else
                    {
                        targ.TrapPower -= (int)magicPower;
                        from.SendMessage("함정의 위력이 약화되었습니다.");
                    }
                }
                else
                {
                    // 스킬 해제 로직 (기존 기획 반영)
                    double dice = Misc.SkillCheck.GetSuccessChance(srcSkill, (double)targ.TrapPower);
                    if (srcSkill >= 50.0) dice += 0.10;
                    if (srcSkill >= 150.0 && dice < 1.0) dice = 1.0 - ((1.0 - dice) * 0.5);

                    if (Utility.RandomDouble() < dice)
                    {
                        targ.TrapType = TrapType.None;
                        from.SendLocalizedMessage(502377);
                    }
                    else
                    {
                        if (Utility.RandomDouble() < 0.20) targ.OnSnoop(from);
                        else from.SendLocalizedMessage(502372);
                    }
                    from.CheckSkill(SkillName.RemoveTrap, (double)targ.TrapPower);
                }
            }
            else if (targeted is BaseTrap || targeted is TrapTrigger)
            {
                BaseTrap trap = (targeted is TrapTrigger) ? ((TrapTrigger)targeted).ParentTrap : (BaseTrap)targeted;

                if (trap == null || trap.Deleted || !trap.Detected)
                {
                    from.SendLocalizedMessage(502373);
                    return;
                }

                if (!isMagic && srcSkill < 100.0)
                {
                    from.SendLocalizedMessage(503429); // 실력이 부족하여 던전 함정은 손댈 수 없습니다.
                    return;
                }

                double difficulty = trap.Difficulty;

                if (isMagic)
                {
                    if (magicPower >= difficulty) { from.SendLocalizedMessage(502377); trap.Delete(); }
                }
                else
                {
                    double dice = Misc.SkillCheck.GetSuccessChance(srcSkill, difficulty);
                    if (srcSkill >= 50.0) dice += 0.10;
                    if (srcSkill >= 150.0 && dice < 1.0) dice = 1.0 - ((1.0 - dice) * 0.5);

                    if (Utility.RandomDouble() < dice) { from.SendLocalizedMessage(502377); trap.Delete(); }
                    else
                    {
                        if (Utility.RandomDouble() < 0.20) { from.SendLocalizedMessage(502375); trap.CheckAndTrigger(from); }
                        else from.SendLocalizedMessage(502372);
                    }
                    from.CheckSkill(SkillName.RemoveTrap, difficulty);
                }
            }
        }

        private class InternalTarget : Target
        {
            public InternalTarget() : base(2, false, TargetFlags.None) { }
            protected override void OnTarget(Mobile from, object targeted)
            {
                OnRemove(from, targeted, false, 0);
            }
        }
    }
}
