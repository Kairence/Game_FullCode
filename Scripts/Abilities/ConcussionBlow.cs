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
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 5초 스턴 (몬스터 등급별 점감 적용)
            double duration = 5.0;
            if (defender is BaseCreature bc)
            {
                duration *= Misc.CreatureBalancer.MonsterTierCrowdControlRecovery(bc);
            }
            defender.Freeze(TimeSpan.FromSeconds(duration));

            // 2. 시각 및 사운드 효과
            defender.PlaySound(0x1E1);
            defender.FixedParticles(0x3049, 1, 0, 9946, EffectLayer.Head);
            attacker.SendLocalizedMessage(1060165); // You have delivered a concussion!
            defender.SendLocalizedMessage(1060091); // You feel disoriented!

            // 3. 무기 피해 100% 물리 피해 적용
            AOS.Damage(defender, attacker, damage, 100, 0, 0, 0, 0, 0, 0);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}
