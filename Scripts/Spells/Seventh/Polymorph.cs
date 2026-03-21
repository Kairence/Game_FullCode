using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Spells; // SummonPoolManager를 참조하기 위해 추가

namespace Server.Spells.Seventh
{
    public class PolymorphSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Polymorph", "Vas Ylem Rel",
            221, 9002,
            Reagent.Bloodmoss, Reagent.SpidersSilk, Reagent.MandrakeRoot);

        private static readonly Hashtable m_Timers = new Hashtable();

        public PolymorphSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Seventh;

        public override bool CheckCast()
        {
            if (Caster.Flying || Factions.Sigil.ExistsOn(Caster) || TransformationSpellHelper.UnderTransformation(Caster))
                return false;

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                if (Caster.BeginAction(typeof(PolymorphSpell)))
                {
                    // 1. 소환수 강제 해제 (기존 로직 유지)
                    if (Caster.Followers > 0)
                    {
                        List<Mobile> toDelete = new List<Mobile>();
                        foreach (Mobile m in Caster.GetMobilesInRange(20))
                        {
                            if (m is BaseCreature bc && bc.ControlMaster == Caster && bc.Summoned)
                                toDelete.Add(bc);
                        }

                        for (int i = 0; i < toDelete.Count; ++i)
                            toDelete[i].Delete();
                    }

                    // 2. [근본적 해결] 매니저를 통해 변신할 동물 타입 결정
                    // 이제 OnCast 내부에서 Activator.CreateInstance를 사용하지 않습니다.
                    Type chosenType = SummonPoolManager.GetEligibleAnimal(Caster);
                    
                    int newBody = 0xD9; // 기본값: Dog

                    // 3. 변신할 BodyID 추출
                    // 리스트가 확정되었으므로 굳이 매번 생성할 필요 없이 
                    // 엔진 내부의 기본 Body 값을 참조하거나, 안전하게 1회성 생성을 유지합니다.
                    BaseCreature temp = Activator.CreateInstance(chosenType) as BaseCreature;
                    if (temp != null)
                    {
                        newBody = temp.Body;
                        temp.Delete(); // 로그 최소화를 위해 즉시 삭제
                    }

                    // 4. 변신 적용 (탈것 해제 및 효과)
                    IMount mt = Caster.Mount;
                    if (mt != null) mt.Rider = null;

                    Caster.BodyMod = newBody;
                    Caster.HueMod = 0;

                    // 시각/청각 효과 추가 (변신 느낌 극대화)
                    Caster.FixedParticles(0x3728, 1, 13, 9918, 92, 3, EffectLayer.Head);
                    Caster.PlaySound(0x221);

                    // 5. 지속 시간 (5분 + 보너스 * 0.06)
                    double bonusDuration = SpellHelper.GetMagicValue(Caster, 0.06);
                    TimeSpan duration = TimeSpan.FromMinutes(5.0) + TimeSpan.FromSeconds(bonusDuration);

                    StopTimer(Caster);
                    Timer t = new InternalTimer(Caster, duration);
                    m_Timers[Caster] = t;
                    t.Start();

                    BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.Polymorph, 1075824, duration, Caster));
                }
            }
            FinishSequence();
        }

        public static void EndPolymorph(Mobile m)
        {
            if (!m.CanBeginAction(typeof(PolymorphSpell)))
            {
                m.BodyMod = 0;
                m.HueMod = -1;
                m.EndAction(typeof(PolymorphSpell));
                StopTimer(m);
                BuffInfo.RemoveBuff(m, BuffIcon.Polymorph);
            }
        }

        public static bool StopTimer(Mobile m)
        {
            Timer t = (Timer)m_Timers[m];
            if (t != null) { t.Stop(); m_Timers.Remove(m); }
            return (t != null);
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Owner;
            public InternalTimer(Mobile owner, TimeSpan duration) : base(duration)
            {
                m_Owner = owner;
                Priority = TimerPriority.OneSecond;
            }
            protected override void OnTick() { EndPolymorph(m_Owner); }
        }
    }
}