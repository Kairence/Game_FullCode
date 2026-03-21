using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class ConcussionBlow : WeaponAbility
    {
        public ConcussionBlow()
        {
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {

            // 1. 광역 타격 대상 수집 (방어자 기준 2타일 내 모든 적)
            List<Mobile> targets = new List<Mobile>();
            IPooledEnumerable eable = defender.GetMobilesInRange(2);

            foreach (Mobile m in eable)
            {
                if (m != attacker && m.Alive && attacker.CanBeHarmful(m, false) && attacker.InLOS(m))
                {
                    targets.Add(m);
                }
            }
            eable.Free();

            // 2. 타겟별 계산 및 적용
            foreach (Mobile m in targets)
            {
                bool isBoss = (m is BaseCreature bc && (bc.Grade >= 8));

                if (isBoss)
                {
                    // [보스/네임드] 250% 추가 공격
                    damage = (int)(damage * 2.5);
                }
                else
                {
                    // [일반 대상] 100% 추가 공격 (총 200%)

                    // [핵심] 현재 체력의 3%를 계산하여 데미지에 합산
                    int hpBonus = (int)(m.Hits * 0.03);
                    damage += Math.Max(1, hpBonus);

                    // [핵심] 기력과 마나는 수치에서 즉시 3% 감소
                    m.Stam -= (int)(m.Stam * 0.03);
                    m.Mana -= (int)(m.Mana * 0.03);
                }

                // 시각 및 사운드 효과
                m.PlaySound(0x1E1);
                m.FixedParticles(0x3049, 1, 0, 9946, EffectLayer.Head);
                m.SendLocalizedMessage(1060091); // You feel disoriented!

                // 3. 최종 통합 데미지 적용
                AOS.Damage(m, attacker, damage, 100, 0, 0, 0, 0, 0, 0);
            }

            attacker.SendLocalizedMessage(1060165); // You have delivered a concussion!
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}