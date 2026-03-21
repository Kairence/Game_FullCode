using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class CrushingBlow : WeaponAbility
    {
        public CrushingBlow()
        {
        }
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            // 1. 시각 효과 및 사운드
            attacker.SendLocalizedMessage(1060090); // You have delivered a crushing blow!
            
            defender.PlaySound(0x213);
            defender.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);
            defender.FixedParticles(0x36B0, 1, 14, 9950, 0, 0, EffectLayer.Waist);

            // 2. 스턴 지속 시간 설정 (기본 5초)
            double duration = 5.0;

            // 몬스터 등급에 따른 점감 적용
            if (defender is BaseCreature bc)
            {
                duration *= Misc.Util.MonsterTierCrowdControlRecovery(bc);
            }

            // 3. 스턴(마비) 적용
            defender.Paralyze(TimeSpan.FromSeconds(duration));

            // 4. 무기 피해의 200% 가함
            int finalDamage = damage * 2;

            // 5. 물리 피해 가함
            AOS.Damage(defender, attacker, finalDamage, 100, 0, 0, 0, 0, 0, 0);
        }
    }
}