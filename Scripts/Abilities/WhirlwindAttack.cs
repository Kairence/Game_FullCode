using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Spells;

namespace Server.Items
{
    public class WhirlwindAttack : WeaponAbility
    {
        public WhirlwindAttack()
        {
        }

        public override int BaseMana => 25;

        // 핵심: 300% 추가 공격 = 기본(100%) + 추가(300%) = 총 400% 위력
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics)
        {
            if (!this.Validate(attacker) || defender == null)
                return;

            // 스테미너 체크 및 소모
            bool bonus = attacker.Skills.Tactics.Value >= 100.0;
            if (!this.CalculateStam(attacker, Misc.Util.SPMStam[12, 0], Misc.Util.SPMStam[12, 1], level, bonus))
                return;

            // 1. 사거리(Tile) 결정
            int tile = 2; // 기본 2타일
            if (attacker.FindItemOnLayer(Layer.TwoHanded) is BaseWeapon) tile++; // 양손무기 +1
            if (attacker.FindItemOnLayer(Layer.TwoHanded) is BaseShield) tile++; // 방패(일부 엔진) +1

            // 2. 데미지 계산: 기획하신 "300% 추가 공격" 적용
            // 기본 데미지(damage)의 4배(400%)로 설정
            int finalDamage = damage * 4; 

            // 3. 대상 수집
            List<Mobile> targets = new List<Mobile>();
            IPooledEnumerable eable = attacker.GetMobilesInRange(tile);

            foreach (Mobile m in eable)
            {
                // 자기 자신 제외, 살아있는 적, 공격 가능 대상, 시야(LOS) 확인
                if (m != attacker && m.Alive && m.CanBeHarmful(attacker, false) && attacker.InLOS(m) &&
                    SpellHelper.ValidIndirectTarget(attacker, m))
                {
                    targets.Add(m);
                }
            }
            eable.Free();

            // 4. 타격 실행
            if (targets.Count > 0)
            {
                attacker.FixedEffect(0x3728, 10, 15);
                attacker.PlaySound(0x2A1);
                attacker.SendLocalizedMessage(1060161); // The whirling attack strikes a target!

                foreach (Mobile m in targets)
                {
                    // 100% 물리 피해 (물리 저항에 깎임)
                    // 만약 방어무시 300%라면 마지막 인자를 100으로 변경하세요.
                    AOS.Damage(m, attacker, finalDamage, 100, 0, 0, 0, 0, 0, 0);
                    
                    m.SendLocalizedMessage(1060162); // You are struck by the whirling attack!
                }
                
                ColUtility.Free(targets);
            }
        }
    }
}