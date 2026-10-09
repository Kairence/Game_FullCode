using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class TalonStrike : WeaponAbility
    {
        public TalonStrike()
        {
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            // 1. 상대 기준 1타일 내의 주변 적 수집
            List<Mobile> targets = new List<Mobile>();
            IPooledEnumerable eable = defender.GetMobilesInRange(1);

            foreach (Mobile m in eable)
            {
                if (m != attacker && m.Alive && attacker.CanBeHarmful(m, false) && attacker.InLOS(m))
                {
                    targets.Add(m);
                }
            }
            eable.Free();

            // 2. 데미지 결정 로직
            // targets에는 defender 자신도 포함되어 있으므로, 1명(본인)만 있으면 주변에 적이 없는 것임.
            int finalDamage = damage;

            if (targets.Count <= 1)
            {
                // [주변에 적이 없음] 300% 공격 (기본 100% + 추가 200%)
                finalDamage = damage * 3;
            }
            else
            {
                // [주변에 적이 있음] 200% 공격 (기본 100% + 추가 100%)
                finalDamage = damage * 2;
            }

            // 3. 타겟별 데미지 적용 (광역 타격)
            foreach (Mobile m in targets)
            {
                // 시각 효과 및 사운드
                m.PlaySound(0x133); // 날카로운 타격음
                m.FixedParticles(0x373A, 1, 17, 0x26BC, 0x662, 0, EffectLayer.Waist);
                
                m.SendLocalizedMessage(1063359); // Your attacker delivers a talon strike!

                // 통합 데미지 적용 (물리 100%)
                AOS.Damage(m, attacker, finalDamage, 100, 0, 0, 0, 0, 0, 0);
            }

            attacker.SendLocalizedMessage(1063358); // You deliver a talon strike!
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}
