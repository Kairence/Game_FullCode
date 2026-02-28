using System;
using Server.Mobiles;

namespace Server.Items
{
    public class MortalStrike : WeaponAbility
    {
        public MortalStrike()
        {
        }

        // 마나 소모 없음

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 시각 효과 및 사운드
            attacker.FixedParticles(0x37BE, 1, 5, 0x26BD, 0x0, 0x1, EffectLayer.Waist);
            defender.FixedParticles(0x37BE, 1, 5, 0x26BD, 0, 0x1, EffectLayer.Waist);
            attacker.PlaySound(0x510);

            // 2. 핵심 로직: 무기 데미지의 4배 (400%)
            int finalDamage = damage * 4;

            // 3. 물리 피해 100% 가함
            AOS.Damage(defender, attacker, finalDamage, 100, 0, 0, 0, 0, 0, 0);

            // 추가적인 힐 차단 디버프 등은 기획에 따라 제거된 상태입니다.
        }
    }
}