using System;
using System.Collections.Generic;
using System.Linq;
using Server.Factions;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.Misc;

namespace Server.SkillHandlers
{
    public class DetectHidden
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.DetectHidden].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile src)
        {
            double skill = src.Skills[SkillName.DetectHidden].Value;

            src.SendLocalizedMessage(500819); // Where will you search?
            src.Target = new InternalTarget();

            if (skill >= 100.0)
                return TimeSpan.FromSeconds(10.0);

            return TimeSpan.FromSeconds(30.0);
        }

        // --- [핵심: 마법과 스킬이 공용으로 사용하는 탐지 메서드] ---
        public static bool OnDetect(Mobile src, Point3D p, int range)
        {
            bool foundAnyone = false;
            double srcSkill = src.Skills[SkillName.DetectHidden].Value;

            // 집 내부 판정 (범위 22타일 확장)
            BaseHouse house = BaseHouse.FindHouseAt(p, src.Map, 16);
            bool inHouse = house != null && house.IsFriend(src);

            if (inHouse) range = 22;

            if (range > 0)
            {
                // 1. 생명체 발견 로직
                IPooledEnumerable inRange = src.Map.GetMobilesInRange(p, range);
                foreach (Mobile trg in inRange)
                {
                    if (trg.Hidden && src != trg)
                    {
                        // 은신 성공 확률 주사위
                        double dice = SkillCheck.GetSuccessChance(srcSkill, trg.Skills[SkillName.Hiding].Value);

                        if (inHouse || dice > Utility.RandomDouble())
                        {
                            trg.RevealingAction();
                            trg.SendLocalizedMessage(500814); // You have been revealed!
                            foundAnyone = true;
                        }

                        // 스킬 숙련도 체크 (집 안이 아닐 때만)
                        if (!inHouse)
                        {
                            src.CheckSkill(SkillName.DetectHidden, trg.Skills[SkillName.Hiding].Value);
                        }
                    }
                }
                inRange.Free();

                // 2. 아이템(덫) 발견 로직
                IPooledEnumerable itemsInRange = src.Map.GetItemsInRange(p, range);
                foreach (Item item in itemsInRange)
                {
                    BaseTrap trap = item as BaseTrap;
                    if (trap != null && !item.Visible)
                    {
                        double dice = SkillCheck.GetSuccessChance(srcSkill, trap.Difficulty);

                        if (inHouse || dice > Utility.RandomDouble())
                        {
                            trap.OnRevealed(src);
                            foundAnyone = true;
                        }

                        if (!inHouse)
                            src.CheckSkill(SkillName.DetectHidden, trap.Difficulty);
                    }
                }
                itemsInRange.Free();
            }

            return foundAnyone;
        }

        public class InternalTarget : Target
        {
            public InternalTarget() : base(12, true, TargetFlags.None) { }

            protected override void OnTarget(Mobile src, object targ)
            {
                Point3D p;
                if (targ is Mobile) p = ((Mobile)targ).Location;
                else if (targ is Item) p = ((Item)targ).Location;
                else if (targ is IPoint3D) p = new Point3D((IPoint3D)targ);
                else p = src.Location;

                // 스킬 기반 범위 계산
                double srcSkill = src.Skills[SkillName.DetectHidden].Value;
                int range = (srcSkill >= 50.0) ? 3 : 2;

                // 공용 메서드 호출
                if (!OnDetect(src, p, range))
                {
                    src.SendLocalizedMessage(500817); // You can see nothing out of the ordinary.
                }
            }
        }

        // --- 패시브 탐지 및 기타 유틸리티 (기존 유지) ---
        public static void DoPassiveDetect(Mobile src)
        {
            if (src == null || src.Map == null || src.IsStaff()) return;

            double ss = src.Skills[SkillName.DetectHidden].Value;
            if (ss < 150.0) return;

            int range = (ss >= 200.0) ? 8 : 5;
            bool isLegendary = (ss >= 200.0);
            bool foundLimit = (ss < 200.0);

            IPooledEnumerable items = src.Map.GetItemsInRange(src.Location, range);
            foreach (Item item in items)
            {
                BaseTrap trap = item as BaseTrap;
                if (trap != null && !item.Visible)
                {
                    double passiveChance = SkillCheck.GetSuccessChance(ss, trap.Difficulty) * 0.2;
                    if (isLegendary || Utility.RandomDouble() < 0.20)
                    {
                        if (Utility.RandomDouble() < passiveChance)
                        {
                            trap.OnRevealed(src);
                            src.SendLocalizedMessage(1153493);
                            if (foundLimit) { items.Free(); return; }
                        }
                    }
                }
            }
            items.Free();

            IPooledEnumerable mobiles = src.Map.GetMobilesInRange(src.Location, range);
            foreach (Mobile m in mobiles)
            {
                if (m != src && m.Hidden && CanDetect(src, m))
                {
                    double passiveChance = SkillCheck.GetSuccessChance(ss, m.Skills[SkillName.Hiding].Value) * 0.2;
                    if (isLegendary || Utility.RandomDouble() < 0.20)
                    {
                        if (Utility.RandomDouble() < passiveChance)
                        {
                            m.RevealingAction();
                            m.SendLocalizedMessage(500814);
                            if (foundLimit) { mobiles.Free(); return; }
                        }
                    }
                }
            }
            mobiles.Free();
        }

        public static bool CanDetect(Mobile src, Mobile target)
        {
            if (src.Map == null || target.Map == null || !src.CanBeHarmful(target, false)) return false;
            if (target.Blessed || (target is BaseCreature && ((BaseCreature)target).IsInvulnerable)) return false;
            if (src.Aggressed.Any(x => x.Defender == target) || src.Aggressors.Any(x => x.Attacker == target)) return true;
            return src.Map.Rules == MapRules.FeluccaRules;
        }
    }
}
