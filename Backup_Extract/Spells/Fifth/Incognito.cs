using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class IncognitoSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Incognito", "Kal In Ex",
            206,
            9002,
            Reagent.Bloodmoss,
            Reagent.Garlic,
            Reagent.Nightshade);

        public IncognitoSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Mobile caster = Caster;

                // 1. 어그로 감소 확률/비율 계산 (20% + 보너스 * 0.008%)
                // 예: 보너스 2500일 때 20% + 20% = 40% 감소
                double bonus = SpellHelper.GetMagicValue(caster, 0.004);
                double reducePercent = 0.20 + (bonus * 0.01); 

                // 상한선 설정 (어그로가 마이너스가 되거나 너무 과하게 깎이는 것 방지 - 필요시 조정)

                Map map = caster.Map;

                if (map != null)
                {
                    // 2. 3타일 내의 모든 모바일 탐색
                    IPooledEnumerable eable = map.GetMobilesInRange(caster.Location, 3);

                    foreach (Mobile m in eable)
                    {
                        // 적대적인 몬스터(BaseCreature)인지 확인
                        if (m is BaseCreature bc && bc.Aggro != null)
                        {
                            // 해당 몬스터의 어그로 테이블에 시전자가 있는지 확인
                            if (bc.Aggro.Table.ContainsKey(caster))
                            {
                                double currentAggro = bc.Aggro.Table[caster];
                                
                                // 어그로 감소 적용: 현재 수치 * (1.0 - 감소율)
                                double newAggro = currentAggro * (1.0 - reducePercent);
                                
                                bc.Aggro.Table[caster] = newAggro;

                                // 이펙트 표시 (개별 몬스터 머리 위)
                                m.FixedParticles(0x376A, 1, 32, 5030, EffectLayer.Head);
                            }
                        }
                    }
                    eable.Free();
                }

                // 3. 시전자 연출
                caster.FixedParticles(0x373A, 10, 15, 5036, EffectLayer.Head);
                caster.PlaySound(0x3BD);
                
                caster.SendMessage($"{reducePercent * 100:F1}%만큼 주변 적들의 어그로를 따돌렸습니다.");
            }

            FinishSequence();
        }
    }
}
