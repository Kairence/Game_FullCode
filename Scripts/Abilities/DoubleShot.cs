using System;
using Server.Mobiles;

namespace Server.Items
{
    public class DoubleShot : WeaponAbility
    {
        public DoubleShot()
        {
        }

        // 마나 소모 없음 (이전 요청 사항 유지)

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 효과 알림 및 시각 효과
            attacker.SendLocalizedMessage(1063348); // You launch two shots at once!
            defender.SendLocalizedMessage(1063349); // You're attacked with a barrage of shots!

            defender.PlaySound(0x3BB);
            defender.FixedEffect(0x37B9, 244, 25);

            // 2. 핵심 로직: 특수기 2종 연속 호출
            
            // [1] 출혈 공격 (Bleed Attack)
            // 상대를 출혈 상태로 만들어 지속 피해를 입힙니다.
            WeaponAbility bleedAttack = WeaponAbility.BleedAttack;
            if (bleedAttack != null)
            {
                bleedAttack.OnHit(attacker, defender, damage);
            }

            // [2] 독 바르기 (Infectious Strike)
            // 우리가 앞서 수정한 로직(독 레벨당 보너스 피해 + 상위 독 교체)이 그대로 적용됩니다.
            WeaponAbility infectiousStrike = WeaponAbility.InfectiousStrike;
            if (infectiousStrike != null)
            {
                infectiousStrike.OnHit(attacker, defender, damage);
            }

            attacker.SendMessage("출혈과 독의 연쇄 공격이 적중했습니다!");
        }
        // 미스 시 재시도 로직은 복잡성을 줄이기 위해 제거하거나 
        // 기존 베이스를 유지하고 싶으시면 그대로 두셔도 됩니다.
    }
}