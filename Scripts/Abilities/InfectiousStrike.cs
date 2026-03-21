using System;
using Server.Mobiles;

namespace Server.Items
{
    public class InfectiousStrike : WeaponAbility
    {
        public InfectiousStrike()
        {
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 시각 효과
            defender.PlaySound(0xDD);
            defender.FixedParticles(0x3728, 244, 25, 9941, 1266, 0, EffectLayer.Waist);

            // 2. 독 스킬 기반 시작 독 등급 결정
            double poisonSkill = attacker.Skills.Poisoning.Value;
            int poisonLevel = 0;

            if (poisonSkill >= 200.0)      poisonLevel = 4;
            else if (poisonSkill >= 150.0) poisonLevel = 3;
            else if (poisonSkill >= 100.0) poisonLevel = 2;
            else if (poisonSkill >= 50.0)  poisonLevel = 1;
            else                           poisonLevel = 0;

            // 3. 데미지 배율 결정 (독을 교체하기 "전" 상태 기준으로 배율 확정)
            // 이미 중독 상태면 40%, 아니면 25%
            double multiplier = defender.Poisoned ? 0.40 : 0.25;
            double bonusPercent = (poisonLevel + 1) * multiplier;
            int extraPoisonDamage = (int)(damage * bonusPercent);

            // 4. 핵심: 상위 독 교체 및 강제 적용 로직
            Poison newPoison = Poison.GetPoison(poisonLevel);

            if (defender.Poisoned && defender.Poison != null)
            {
                // 현재 독보다 내 독이 더 강할 때만 교체
                if (poisonLevel > defender.Poison.Level)
                {
                    defender.ApplyPoison(attacker, newPoison); // 새 독 주입
                }
            }
            else
            {
                // 중독 상태가 아니면 즉시 적용
                defender.ApplyPoison(attacker, newPoison);
            }

            // 5. 독 속성 추가 피해 가함
            if (extraPoisonDamage > 0)
            {
                AOS.Damage(defender, attacker, extraPoisonDamage, 0, 0, 0, 100, 0, 0, 0);
            }
        }
    }
}