using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;

namespace Server.Spells.Chivalry
{
    public class RemoveCurseSpell : PaladinSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo("Remove Curse", "Extermo Vomica", -1, 9002);

        public RemoveCurseSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);
        public override double RequiredSkill => 200.0;

        // [기획] 마나 500 소모
        public override int RequiredMana => 500;

        // RequiredTithing 오버라이드를 삭제하여 십일조 비용을 지불하지 않게 함

        public override int MantraNumber => 1060726;
        public override int RequiredTithing => 0;

        public override void OnCast()
        {
            if (CheckSequence())
            {
                // [기획] 10타일 이내의 아군 및 자신 검색
                List<Mobile> targets = new List<Mobile>();
                IPooledEnumerable eable = Caster.GetMobilesInRange(10);

                foreach (Mobile m in eable)
                {
                    // 적대적 생물이나 범죄자/살인마 제외
                    if (m is BaseCreature || (m.Player && (m.Criminal || m.Murderer)))
                        continue;

                    if (m.InLOS(Caster) && Caster.CanBeBeneficial(m, false, true))
                        targets.Add(m);
                }
                eable.Free();

                // 시전자 효과음
                Caster.PlaySound(0xF6);
                Caster.PlaySound(0x1F7);

                foreach (Mobile m in targets)
                {
                    // 1. 모든 저주 및 상태이상 100% 해제
                    m.Paralyzed = false;
                    EvilOmenSpell.TryEndEffect(m);
                    StrangleSpell.RemoveCurse(m);
                    CorpseSkinSpell.RemoveCurse(m);
                    CurseSpell.RemoveEffect(m);
                    WeakenSpell.RemoveEffects(m);
                    FeeblemindSpell.RemoveEffects(m);
                    ClumsySpell.RemoveEffects(m);
                    BloodOathSpell.RemoveCurse(m);
                    MindRotSpell.ClearMindRotScalar(m);
                    BuffInfo.RemoveBuff(m, BuffIcon.MassCurse);

                    // 2. [기획] 기력 회복: 100 * 카르마 효율
                    // 카르마 15000 기준: 100 * 2.5 = 250 기력 회복
                    int stamGain = (int)GetKarmaScaler(100.0, true);
                    m.Stam += stamGain;

                    // 대상 이펙트
                    m.FixedParticles(0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head);
                }
            }

            FinishSequence();
        }
    }
}