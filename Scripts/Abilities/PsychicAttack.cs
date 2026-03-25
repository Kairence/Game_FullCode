using System;
using Server.Mobiles;

namespace Server.Items
{
    public class PsychicAttack : WeaponAbility
    {
        public PsychicAttack()
        {
        }

        // 마나 소모 없음

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 효과 알림 및 시각 효과
            defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
            defender.PlaySound(0x1F8);
            defender.SendLocalizedMessage(1074384); // Your mind is attacked by psychic force!

            // 2. 핵심 로직: 10초 동안 마법 추가 피해 50% 증가
            double duration = 10.0;
            int damageBonusPercent = 50;

            // 요청하신 대로 DateTime.Now를 사용합니다.
            if (defender is PlayerMobile pm)
            {
                pm.psychicTime = DateTime.Now + TimeSpan.FromSeconds(duration);
                pm.psychicDamageDown = damageBonusPercent; 
            }
            else if (defender is BaseCreature bc)
            {
                bc.psychicTime = DateTime.Now + TimeSpan.FromSeconds(duration);
                bc.psychicDamageDown = damageBonusPercent;
            }

            // 3. 에너지 속성 100% 피해 가함
            AOS.Damage(defender, attacker, damage, 0, 0, 0, 0, 100, 0, 0);
        }
    }
}
