using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;

namespace Server.SkillHandlers
{
    public class Provocation
    {
        private static readonly Dictionary<Mobile, BaseInstrument> m_Instruments = new Dictionary<Mobile, BaseInstrument>();
        private static readonly Dictionary<Mobile, DateTime> m_ProvokedEntities = new Dictionary<Mobile, DateTime>();

        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Provocation].Callback = OnUse;
        }

        public static (int CritChance, int CritDamage) GetProvokeCritBonus(Mobile m)
        {
            if (m_ProvokedEntities.ContainsKey(m))
            {
                if (m_ProvokedEntities[m] > DateTime.Now)
                {
                    return (20, 50); 
                }
                else
                {
                    m_ProvokedEntities.Remove(m);
                }
            }
            return (0, 0);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            m.RevealingAction();

            BaseInstrument inst = null;

            if (m_Instruments.ContainsKey(m))
            {
                inst = m_Instruments[m];
                if (inst.Deleted || (!inst.IsChildOf(m.Backpack) && inst.Parent != m))
                {
                    inst = null;
                }
            }

            if (inst == null)
            {
                inst = BaseInstrument.GetInstrument(m);
            }

            if (inst != null)
            {
                OnPickedInstrument(m, inst);
            }
            else
            {
                m.SendMessage("어떤 악기를 연주하시겠습니까? (악기를 선택하여 등록하세요)");
                m.Target = new InternalInstrumentTarget();
            }

            return TimeSpan.FromSeconds(1.0);
        }

        private class InternalInstrumentTarget : Target
        {
            public InternalInstrumentTarget() : base(1, false, TargetFlags.None) { }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is BaseInstrument inst)
                {
                    if (!inst.IsChildOf(from.Backpack) && inst.Parent != from)
                    {
                        from.SendLocalizedMessage(1042001);
                    }
                    else
                    {
                        m_Instruments[from] = inst;
                        OnPickedInstrument(from, inst);
                    }
                }
                else
                {
                    from.SendMessage("악기가 아닙니다.");
                }
            }
        }

        public static void OnPickedInstrument(Mobile from, BaseInstrument instrument)
        {
            from.RevealingAction();
            from.SendLocalizedMessage(501587); 
            from.Target = new InternalFirstTarget(from, instrument);
        }

        public class InternalFirstTarget : Target
        {
            private readonly BaseInstrument m_Instrument;

            public InternalFirstTarget(Mobile from, BaseInstrument instrument)
                : base(BaseInstrument.GetBardRange(from, SkillName.Provocation), false, TargetFlags.None)
            {
                m_Instrument = instrument;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                from.RevealingAction();

                if (targeted is BaseCreature creature && from.CanBeHarmful(creature, true))
                {
                    if (!m_Instrument.IsChildOf(from.Backpack) && m_Instrument.Parent != from)
                    {
                        from.SendLocalizedMessage(1062488);
                    }
                    else if (from is PlayerMobile && creature.Controlled)
                    {
                        from.SendLocalizedMessage(501590); 
                    }
                    else
                    {
                        from.RevealingAction();
                        m_Instrument.PlayInstrumentWell(from);
                        from.SendLocalizedMessage(1008085); 
                        from.Target = new InternalSecondTarget(from, m_Instrument, creature);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(501589); 
                }
            }
        }

        public class InternalSecondTarget : Target
        {
            private readonly BaseCreature m_Creature;
            private readonly BaseInstrument m_Instrument;

            public InternalSecondTarget(Mobile from, BaseInstrument instrument, BaseCreature creature)
                : base(BaseInstrument.GetBardRange(from, SkillName.Provocation), false, TargetFlags.None)
            {
                m_Instrument = instrument;
                m_Creature = creature;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                from.RevealingAction();

                if (!m_Instrument.IsChildOf(from.Backpack) && m_Instrument.Parent != from)
                {
                    from.SendLocalizedMessage(1062488);
                    return;
                }

                if (targeted is BaseCreature targetCreature)
                {
                    if (m_Creature == targetCreature)
                    {
                        from.SendLocalizedMessage(501593); 
                        return;
                    }

                    if (m_Creature.Map != targetCreature.Map || !m_Creature.InRange(targetCreature, BaseInstrument.GetBardRange(from, SkillName.Provocation)))
                    {
                        from.SendLocalizedMessage(1049450); 
                        return;
                    }

                    from.Frozen = true;
                    Timer.DelayCall(TimeSpan.FromSeconds(10.0), () => 
                    {
                        if (from != null && !from.Deleted)
                            from.Frozen = false;
                    });

                    double musicSkill = from.Skills[SkillName.Musicianship].Value;
                    double provoSkill = from.Skills[SkillName.Provocation].Value;

                    double avgActualFame = (m_Creature.Fame + targetCreature.Fame) / 2.0;
                    double avgFameToUse = avgActualFame;

                    if (musicSkill >= 100.0)
                    {
                        avgFameToUse -= 10000.0;
                    }

                    double chance = 10.0 + ((musicSkill * 100.0) - avgFameToUse) / 100.0;

                    if (chance > 0)
                    {
                        int grade1 = CreatureBalancer.MonsterGrade(m_Creature.Grade);
                        int grade2 = CreatureBalancer.MonsterGrade(targetCreature.Grade);
                        chance /= Math.Max(grade1, grade2);

                        double expGain = avgActualFame / 10.0;

                        if (chance > Utility.RandomDouble() * 100.0)
                        {
                            from.SendLocalizedMessage(501602); 
                            m_Instrument.PlayInstrumentWell(from);

                            from.CheckSkill(SkillName.Provocation, expGain);
                            from.CheckSkill(SkillName.Musicianship, expGain);

                            ApplyProvocation(from, m_Creature, targetCreature, provoSkill);

                            if (provoSkill >= 150.0 && m_Creature.RawInt < 100)
                            {
                                if (Utility.RandomDouble() * 100.0 < chance * 0.25)
                                {
                                    ProcessSweepProvoke(from, m_Creature, provoSkill);
                                }
                            }
                        }
                        else
                        {
                            from.SendLocalizedMessage(501599); 
                            m_Instrument.PlayInstrumentBadly(from);

                            from.CheckSkill(SkillName.Provocation, expGain * 0.1);
                            from.CheckSkill(SkillName.Musicianship, expGain * 0.1);
                        }
                    }
                    else
                    {
                        from.SendMessage("상대의 명성이 너무 높아 연주가 통하지 않습니다.");
                        m_Instrument.PlayInstrumentBadly(from);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(501589); 
                }
            }

            private void ProcessSweepProvoke(Mobile bard, BaseCreature center, double skillVal)
            {
                foreach (Mobile m in center.GetMobilesInRange(5))
                {
                    if (m is BaseCreature third && m != center && m.Alive && bard.CanBeHarmful(third, false))
                    {
                        bard.SendMessage("연주에 휩쓸린 다른 개체가 전투에 난입합니다!");
                        ApplyProvocation(bard, center, third, skillVal);
                        break; 
                    }
                }
            }

            private void ApplyProvocation(Mobile bard, BaseCreature c1, BaseCreature c2, double skillVal)
            {
                c1.Provoke(bard, c2, true);
                c2.Provoke(bard, c1, true);

                if (skillVal >= 50.0)
                {
                    m_ProvokedEntities[c1] = DateTime.Now + TimeSpan.FromSeconds(60.0);
                    m_ProvokedEntities[c2] = DateTime.Now + TimeSpan.FromSeconds(60.0);
                }

                new ProvokeControlTimer(bard, c1, c2, skillVal).Start();
            }
        }

        private class ProvokeControlTimer : Timer
        {
            private readonly Mobile m_Bard;
            private readonly BaseCreature m_C1;
            private readonly BaseCreature m_C2;
            private readonly double m_SkillVal;

            public ProvokeControlTimer(Mobile bard, BaseCreature c1, BaseCreature c2, double skillVal) 
                : base(TimeSpan.FromSeconds(3.0), TimeSpan.FromSeconds(3.0))
            {
                m_Bard = bard;
                m_C1 = c1;
                m_C2 = c2;
                m_SkillVal = skillVal;
            }

            protected override void OnTick()
            {
                if (!m_C1.Alive && !m_C2.Alive)
                {
                    Stop();
                    return;
                }

                if (m_SkillVal >= 100.0)
                {
                    if (!m_C1.Alive && m_C2.Alive && m_C2.Combatant == null)
                    {
                        ChainProvoke(m_C2);
                        Stop();
                        return;
                    }
                    if (!m_C2.Alive && m_C1.Alive && m_C1.Combatant == null)
                    {
                        ChainProvoke(m_C1);
                        Stop();
                        return;
                    }
                }

                if (!m_C1.Alive || !m_C2.Alive)
                {
                    Stop();
                    return;
                }

                if (CheckBreakProvoke(m_C1) || CheckBreakProvoke(m_C2))
                {
                    m_C1.Combatant = null;
                    m_C2.Combatant = null;
                    m_C1.Warmode = false;
                    m_C2.Warmode = false;
                    
                    if (m_ProvokedEntities.ContainsKey(m_C1)) m_ProvokedEntities.Remove(m_C1);
                    if (m_ProvokedEntities.ContainsKey(m_C2)) m_ProvokedEntities.Remove(m_C2);

                    Stop();
                }
            }

            private void ChainProvoke(BaseCreature survivor)
            {
                foreach (Mobile m in survivor.GetMobilesInRange(5))
                {
                    if (m is BaseCreature newTarg && m != survivor && m.Alive && m_Bard.CanBeHarmful(m, false))
                    {
                        survivor.Provoke(m_Bard, newTarg, true);
                        newTarg.Provoke(m_Bard, survivor, true);
                        new ProvokeControlTimer(m_Bard, survivor, newTarg, m_SkillVal).Start();
                        break;
                    }
                }
            }

            private bool CheckBreakProvoke(BaseCreature c)
            {
                double breakChance = c.RawInt * 0.1; 
                
                int sameTypeCount = 0;
                foreach (Mobile m in c.GetMobilesInRange(5))
                {
                    if (m is BaseCreature && m != c && m.Alive && m.GetType() == c.GetType())
                    {
                        sameTypeCount++;
                    }
                }
                
                breakChance += sameTypeCount * 5.0;

                if (m_SkillVal >= 200.0)
                {
                    breakChance /= 2.0;
                }

                return Utility.RandomDouble() * 100.0 < breakChance;
            }
        }
    }
}