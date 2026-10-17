using System;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    /// <summary>
    /// 궁술의 극의: 힘의 화살 - CombatEngine에서 계산된 결과에 따라 이펙트만 출력합니다.
    /// </summary>
    public class ForceArrow : WeaponAbility
    {
        public ForceArrow()
        {
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {

            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 치명타 100% 추가 피해 적용 (총 200%)
            int finalDamage = (int)(damage * 2.0);
            AOS.Damage(defender, attacker, finalDamage, 100, 0, 0, 0, 0, 0, 0);

            // 2. 힘의 화살 발동 시각 효과 (파란색 전기 스파크 계열)
            defender.FixedParticles(0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head);
            defender.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);

            // 3. 전용 사운드 출력 (힘이 실린 날카로운 타격음)
            defender.PlaySound(0x1E1); 

            // 4. 시스템 메시지 출력
            attacker.SendLocalizedMessage(1074381); // You fire an arrow of pure force.
            defender.SendLocalizedMessage(1074382); // You are struck by a force arrow!

            // 5. 숙련도 체감을 위한 오버헤드 메시지
            attacker.LocalOverheadMessage(MessageType.Regular, 0x481, false, "[힘의 화살]");
        }

        // 기존 하위 호환을 위한 오버라이드
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics)
        {
            OnHit(attacker, defender, damage);
        }
    }
}
