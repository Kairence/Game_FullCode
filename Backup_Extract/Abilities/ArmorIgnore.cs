using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class ArmorIgnore : WeaponAbility
    {
        public ArmorIgnore()
        {
        }

        // 기본 매개변수가 없는 OnHit을 오버라이드하여 패시브에서도 호출 가능하게 합니다.
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            // 부모 클래스의 Validate나 마나/기력 체크가 필요하다면 여기서 수행
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 시각 및 사운드 효과
            attacker.SendLocalizedMessage(1060076); // Your attack penetrates their armor!
            defender.SendLocalizedMessage(1060077); // The blow penetrated your armor!

            defender.PlaySound(0x56);
            defender.FixedParticles(0x3728, 200, 25, 9942, EffectLayer.Waist);

            // 2. 데미지 계산: 무기 피해의 200% (baseDamage는 이미 계산되어 들어온 값 기준)
            int bonusDamage = damage * 2;

            // 3. 방어 무시 피해 (Direct Damage)
            // 인자: defender, attacker, damage, phys, fire, cold, pois, nrgy, chaos, direct
            AOS.Damage(defender, attacker, bonusDamage, 0, 0, 0, 0, 0, 0, 100);
        }

        // WeaponAbility의 가상 메서드 형식에 맞춘 오버로딩
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}
