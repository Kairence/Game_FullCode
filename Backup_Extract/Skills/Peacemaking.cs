#region References
using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;
#endregion

namespace Server.SkillHandlers
{
    public class Peacemaking
    {
        private static readonly Dictionary<Mobile, BaseInstrument> m_Instruments = new Dictionary<Mobile, BaseInstrument>();
        private static readonly Dictionary<Mobile, DateTime> m_TamingBonus = new Dictionary<Mobile, DateTime>();

        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Peacemaking].Callback = OnUse;
        }

        public static bool UnderEffects(Mobile m)
        {
            return m is BaseCreature && ((BaseCreature)m).BardPacified;
        }

        public static bool CheckAndConsumeTamingBonus(Mobile targ)
        {
            if (m_TamingBonus.ContainsKey(targ))
            {
                DateTime expire = m_TamingBonus[targ];
                if (expire > DateTime.Now)
                {
                    m_TamingBonus.Remove(targ);
                    
                    if (targ is BaseCreature bc)
                    {
                        bc.BardEndTime = DateTime.Now; 
                    }
                    return true;
                }
                m_TamingBonus.Remove(targ);
            }
            return false;
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
            from.SendLocalizedMessage(1049525);
            from.Target = new InternalTarget(from, instrument);
        }

        public class InternalTarget : Target
        {
            private readonly BaseInstrument m_Instrument;

            public InternalTarget(Mobile from, BaseInstrument instrument)
                : base(BaseInstrument.GetBardRange(from, SkillName.Peacemaking), false, TargetFlags.None)
            {
                m_Instrument = instrument;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                from.RevealingAction();

                if (!m_Instrument.IsChildOf(from.Backpack) && m_Instrument.Parent != from)
                {
                    from.SendLocalizedMessage(1062488);
                    return;
                }

                if (targeted is Mobile targ)
                {
                    from.Frozen = true;
                    Timer.DelayCall(TimeSpan.FromSeconds(10.0), () => 
                    {
                        if (from != null && !from.Deleted)
                            from.Frozen = false;
                    });

                    double peaceSkill = from.Skills[SkillName.Peacemaking].Value;
                    double musicSkill = from.Skills[SkillName.Musicianship].Value;

                    bool isAlly = !from.CanBeHarmful(targ, false);
                    bool isDungeon = targ.Region.IsPartOf(typeof(Server.Regions.DungeonRegion));
                    
                    double fameToUse = targ.Fame;
                    double actualFame = targ.Fame;

                    if (isAlly)
                    {
                        if (peaceSkill >= 100.0)
                        {
                            if (isDungeon)
                            {
                                from.SendMessage("던전에서는 아군에게 평화 유지를 사용할 수 없습니다.");
                                m_Instrument.PlayInstrumentBadly(from);
                                return;
                            }
                            fameToUse = 100.0;
                        }
                        else
                        {
                            from.SendMessage("아군에게 평화 유지를 사용하려면 100 이상의 숙련도가 필요합니다.");
                            m_Instrument.PlayInstrumentBadly(from);
                            return;
                        }
                        actualFame = 1000.0;
                    }
                    else
                    {
                        if (musicSkill >= 100.0)
                        {
                            fameToUse -= 10000.0;
                        }
                    }

                    double chance = 10.0 + ((musicSkill * 100.0) - fameToUse) / 100.0;

                    if (chance > 0)
                    {
                        if (targ is BaseCreature bc)
                        {
                            int grade = CreatureBalancer.MonsterGrade(bc.Grade);
                            chance /= grade;

                            if (peaceSkill >= 150.0 && !bc.Controlled && (bc.Tamable || bc.AI == AIType.AI_Animal))
                            {
                                chance *= 1.5; 
                            }
                        }

                        double expGain = actualFame / 10.0;

                        if (chance > Utility.RandomDouble() * 100.0)
                        {
                            from.SendMessage("평화 유지 연주에 성공했습니다!");
                            m_Instrument.PlayInstrumentWell(from);

                            from.CheckSkill(SkillName.Peacemaking, expGain);
                            from.CheckSkill(SkillName.Musicianship, expGain);

                            if (isAlly)
                            {
                                targ.Combatant = null;
                                targ.Warmode = false;
                                
                                // [100 보너스] 플레이어 전투 타이머(5분 대기 상태) 즉시 해제
                                if (targ is PlayerMobile pmTarget)
                                {
                                    pmTarget.TimerList[64] = 0; // PvM
                                    pmTarget.TimerList[65] = 0; // PvP
                                }

                                targ.SendMessage("평화로운 선율에 전투 의지를 내려놓습니다.");
                            }
                            else if (targ is BaseCreature bcEnemy)
                            {
                                // [50 보너스] 대상의 어그로 테이블에서 시전자를 완전 삭제
                                if (peaceSkill >= 50.0)
                                {
                                    bcEnemy.Aggro.Table.Remove(from);
                                    if (bcEnemy.Combatant == from)
                                    {
                                        bcEnemy.Combatant = null;
                                    }
                                }

                                bcEnemy.Pacify(from, DateTime.Now + TimeSpan.FromSeconds(30.0));
                                bcEnemy.Combatant = null;
                                bcEnemy.Warmode = false;

                                if (peaceSkill >= 200.0)
                                {
                                    m_TamingBonus[bcEnemy] = DateTime.Now + TimeSpan.FromSeconds(30.0);
                                }
                            }
                        }
                        else
                        {
                            from.SendMessage("연주에 실패했습니다.");
                            m_Instrument.PlayInstrumentBadly(from);

                            from.CheckSkill(SkillName.Peacemaking, expGain * 0.1);
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
                    from.SendLocalizedMessage(1049528);
                }
            }
        }
    }
}