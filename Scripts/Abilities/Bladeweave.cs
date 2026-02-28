using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    // 1. static 제거 및 WeaponAbility 상속
    public class Bladeweave : WeaponAbility
    {
        public Bladeweave()
        {
        }

        // 2. 메서드명을 OnHit으로 변경 (클래스명과의 충돌 방지 및 오버라이드)
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 메시지 및 시각 효과
            attacker.SendLocalizedMessage(1063168); // You attack with lightning precision!
            defender.SendLocalizedMessage(1063169); // Your opponent's quick strike causes extra damage!
            
            defender.PlaySound(0x51D);
            defender.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);

            // 2. 기획 핵심 로직: 무기 피해의 300% 추가 피해 (damage 인자 활용)
            int bonusDamage = damage * 3;

            // 3. 데미지 즉시 가함
            // 인자: defender, attacker, damage, phys, fire, cold, pois, nrgy, chaos, direct
            AOS.Damage(defender, attacker, bonusDamage, 100, 0, 0, 0, 0, 0, 0);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}