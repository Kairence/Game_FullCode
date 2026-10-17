using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Spells;
using Server.Engines.PartySystem;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// 둔기 200 극의: 뇌진탕 일격 (Concussion Blow Ultimate)
    /// 상대의 3타일 내 모든 적에게 150% 추가 공격 + 체력/기력/마나 5% 하락 (보스는 200% 추가 공격)
    /// </summary>
    public class FrenziedWhirlwind : WeaponAbility
    {
        public FrenziedWhirlwind()
        {
        }

        public override bool IsAreaAbility => true;

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null)
                return;

            Point3D center = defender != null ? defender.Location : attacker.Location;
            Map map = attacker.Map;
            if (map == null)
                return;

            // 1. 3타일 내 적대 대상 수집
            List<Mobile> targets = new List<Mobile>();
            IPooledEnumerable eable = map.GetMobilesInRange(center, 3);
            foreach (Mobile m in eable)
            {
                if (m != attacker && m.Alive && attacker.CanBeHarmful(m, false) && attacker.InLOS(m))
                {
                    targets.Add(m);
                }
            }
            eable.Free();

            if (targets.Count == 0 && defender != null && defender.Alive)
            {
                targets.Add(defender);
            }

            attacker.FixedEffect(0x3728, 10, 15);
            attacker.PlaySound(0x2A1);

            // 2. 타겟별 피해 및 스탯 감소 적용
            foreach (Mobile m in targets)
            {
                bool isBoss = (m is BaseCreature bc && (bc.Grade >= 8));
                int finalDamage = damage;

                if (isBoss)
                {
                    // [보스/네임드] 200% 추가 공격 (총 300%)
                    finalDamage = (int)(damage * 3.0);
                }
                else
                {
                    // [일반 대상] 150% 추가 공격 (총 250%)
                    finalDamage = (int)(damage * 2.5);

                    // 현재 체력, 기력, 마나 5% 하락
                    int hpLoss = (int)(m.Hits * 0.05);
                    finalDamage += Math.Max(1, hpLoss);

                    m.Stam = Math.Max(0, m.Stam - (int)(m.Stam * 0.05));
                    m.Mana = Math.Max(0, m.Mana - (int)(m.Mana * 0.05));
                }

                m.PlaySound(0x1E1);
                m.FixedParticles(0x3049, 1, 0, 9946, EffectLayer.Head);
                m.SendLocalizedMessage(1060091); // You feel disoriented!

                AOS.Damage(m, attacker, finalDamage, 100, 0, 0, 0, 0, 0, 0);
            }

            attacker.SendMessage(0x35, $"[뇌진탕 일격] 3타일 내 적 {targets.Count}명에게 극의를 적중시켰습니다!");
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}
