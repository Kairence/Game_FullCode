using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;

namespace Server.SkillHandlers
{
    public class Discordance
    {
        private static readonly Dictionary<Mobile, DiscordanceInfo> m_Table = new Dictionary<Mobile, DiscordanceInfo>();
        private static readonly Dictionary<Mobile, BaseInstrument> m_Instruments = new Dictionary<Mobile, BaseInstrument>();

        public static bool UnderEffects(Mobile m) => m != null && m_Table.ContainsKey(m);

        public static void RemoveEffects(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
                DiscordanceInfo info = m_Table[m];
                info.ClearMods();
                if (info.m_Timer != null) info.m_Timer.Stop();
                m_Table.Remove(m);
            }
        }

        public static int GetSpeedPenalty(Mobile targ)
        {
            DiscordanceInfo info = m_Table.GetValueOrDefault(targ);
            if (info != null)
            {
                if (info.m_From != null && info.m_From.Skills[SkillName.Discordance].Value >= 50.0)
                {
                    return info.m_Stacks * 50000;
                }
            }
            return 0;
        }

        public static double GetRegenScalar(Mobile targ)
        {
            DiscordanceInfo info = m_Table.GetValueOrDefault(targ);
            if (info != null)
            {
                if (info.m_From != null && info.m_From.Skills[SkillName.Discordance].Value >= 100.0)
                {
                    return 0.5;
                }
            }
            return 1.0;
        }

        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Discordance].Callback = OnUse;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            m.RevealingAction();

            BaseInstrument inst = null;
            
            if (m_Instruments.ContainsKey(m))
            {
                inst = m_Instruments[m];
                if (inst.Deleted || (!inst.IsChildOf(m.Backpack) && inst.Parent != m))
                    inst = null;
            }

            if (inst == null)
                inst = BaseInstrument.GetInstrument(m);

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
            from.SendMessage("불협화음을 걸 대상을 지정하세요.");
            from.Target = new DiscordanceTarget(from, instrument);
        }

        public class DiscordanceTarget : Target
        {
            private readonly BaseInstrument m_Instrument;

            public DiscordanceTarget(Mobile from, BaseInstrument inst) : base(10, false, TargetFlags.Harmful)
            {
                m_Instrument = inst;
            }

            protected override void OnTarget(Mobile from, object target)
            {
                from.RevealingAction();

                if (!m_Instrument.IsChildOf(from.Backpack) && m_Instrument.Parent != from)
                {
                    from.SendLocalizedMessage(1062488);
                    return;
                }

                if (target is Mobile targ)
                {
                    if (targ == from || !from.CanBeHarmful(targ, false))
                    {
                        from.SendLocalizedMessage(1049535);
                        return;
                    }

                    from.Frozen = true;
                    Timer.DelayCall(TimeSpan.FromSeconds(10.0), () => 
                    {
                        if (from != null && !from.Deleted)
                            from.Frozen = false;
                    });

                    double music = from.Skills[SkillName.Musicianship].Value;
                    
                    double actualFame = targ.Fame;
                    double fameToUse = targ.Fame;

                    if (music >= 100.0)
                    {
                        fameToUse -= 10000.0;
                    }

                    double chance = 10.0 + ((music * 100.0) - fameToUse) / 100.0;

                    if (chance > 0)
                    {
                        if (targ is BaseCreature bc)
                        {
                            int grade = CreatureBalancer.MonsterGrade(bc.Grade);
                            chance /= grade;
                        }

                        double expGain = actualFame / 10.0;

                        if (chance > Utility.RandomDouble() * 100.0)
                        {
                            from.SendMessage("불협화음 연주에 성공했습니다!");
                            m_Instrument.PlayInstrumentWell(from);

                            from.CheckSkill(SkillName.Discordance, expGain);
                            from.CheckSkill(SkillName.Musicianship, expGain);

                            DiscordanceInfo info = m_Table.GetValueOrDefault(targ);
                            if (info != null)
                            {
                                info.AddStack(from);
                            }
                            else
                            {
                                info = new DiscordanceInfo(from, targ);
                                info.AddStack(from);
                                m_Table[targ] = info;
                            }
                        }
                        else
                        {
                            from.SendMessage("연주에 실패했습니다.");
                            m_Instrument.PlayInstrumentBadly(from);

                            from.CheckSkill(SkillName.Discordance, expGain * 0.1);
                            from.CheckSkill(SkillName.Musicianship, expGain * 0.1);
                        }
                    }
                    else
                    {
                        from.SendMessage("상대의 명성이 너무 높아 연주가 통하지 않습니다.");
                        m_Instrument.PlayInstrumentBadly(from);
                    }
                }
            }
        }

        private class DiscordanceInfo
        {
            public Mobile m_From;
            public Mobile m_Target;
            public int m_Stacks;
            public DateTime m_EndTime;
            public Timer m_Timer;
            
            private readonly List<ResistanceMod> m_ResistMods = new List<ResistanceMod>();
            private readonly List<StatMod> m_StatMods = new List<StatMod>();

            public DiscordanceInfo(Mobile from, Mobile target)
            {
                m_From = from;
                m_Target = target;
                m_Stacks = 0;
                m_EndTime = DateTime.Now;

                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), ProcessDiscordance);
            }

            public void AddStack(Mobile from)
            {
                m_From = from;
                double discord = from.Skills[SkillName.Discordance].Value;
                
                int maxStacks = discord >= 150.0 ? 6 : 4;
                int stacksToAdd = (m_Stacks == 0 && discord >= 150.0) ? 3 : 1;

                if (m_EndTime < DateTime.Now)
                    m_EndTime = DateTime.Now;

                m_EndTime += TimeSpan.FromSeconds(60.0 * stacksToAdd);

                double maxAllowedTime = maxStacks * 60.0;
                if ((m_EndTime - DateTime.Now).TotalSeconds > maxAllowedTime)
                {
                    m_EndTime = DateTime.Now + TimeSpan.FromSeconds(maxAllowedTime);
                }

                UpdateStacksAndMods();
            }

            private void ProcessDiscordance()
            {
                if (DateTime.Now >= m_EndTime || !m_Target.Alive || m_Target.Deleted)
                {
                    RemoveDiscord(this);
                    return;
                }

                int currentStacks = (int)Math.Ceiling((m_EndTime - DateTime.Now).TotalSeconds / 60.0);
                if (currentStacks != m_Stacks)
                {
                    m_Stacks = currentStacks;
                    UpdateMods();
                }
            }

            private void UpdateStacksAndMods()
            {
                m_Stacks = (int)Math.Ceiling((m_EndTime - DateTime.Now).TotalSeconds / 60.0);
                m_Target.FixedEffect(0x376A, 1, 32);
                UpdateMods();
            }

            public void ClearMods()
            {
                foreach (var mod in m_ResistMods)
                    m_Target.RemoveResistanceMod(mod);
                m_ResistMods.Clear();

                foreach (var mod in m_StatMods)
                    m_Target.RemoveStatMod(mod.Name);
                m_StatMods.Clear();
            }

            private void UpdateMods()
            {
                ClearMods();
                
                if (m_Stacks <= 0) return;

                int resistPenalty = m_Stacks * 5;
                m_ResistMods.Add(new ResistanceMod(ResistanceType.Physical, -resistPenalty));
                m_ResistMods.Add(new ResistanceMod(ResistanceType.Fire, -resistPenalty));
                m_ResistMods.Add(new ResistanceMod(ResistanceType.Cold, -resistPenalty));
                m_ResistMods.Add(new ResistanceMod(ResistanceType.Poison, -resistPenalty));
                m_ResistMods.Add(new ResistanceMod(ResistanceType.Energy, -resistPenalty));

                foreach (var mod in m_ResistMods)
                    m_Target.AddResistanceMod(mod);

                if (m_From.Skills[SkillName.Discordance].Value >= 200.0)
                {
                    double statPenalty = m_Stacks * 0.05;
                    m_StatMods.Add(new StatMod(StatType.Str, "DiscordanceStr", (int)(m_Target.RawStr * -statPenalty), TimeSpan.Zero));
                    m_StatMods.Add(new StatMod(StatType.Dex, "DiscordanceDex", (int)(m_Target.RawDex * -statPenalty), TimeSpan.Zero));
                    m_StatMods.Add(new StatMod(StatType.Int, "DiscordanceInt", (int)(m_Target.RawInt * -statPenalty), TimeSpan.Zero));

                    foreach (var mod in m_StatMods)
                        m_Target.AddStatMod(mod);
                }
            }

            public static void RemoveDiscord(DiscordanceInfo info)
            {
                if (info.m_Timer != null) info.m_Timer.Stop();
                info.ClearMods();
                m_Table.Remove(info.m_Target);
            }
        }
    }
}