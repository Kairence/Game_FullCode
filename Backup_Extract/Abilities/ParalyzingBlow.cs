using System;
using System.Collections.Generic; // Dictionary 사용을 위해 추가
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class ParalyzingBlow : WeaponAbility
    {
        // 마비 종료 시간을 추적하기 위한 저장소
        private static Dictionary<Mobile, DateTime> m_ParalyzeTable = new Dictionary<Mobile, DateTime>();

        public ParalyzingBlow()
        {
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;

            double maxDuration = 30.0;
            double addDuration = 10.0;

            if (defender is BaseCreature bc)
                addDuration *= Misc.Util.MonsterTierCrowdControlRecovery(bc);

            DateTime now = DateTime.UtcNow;
            double finalSeconds = addDuration;

            // 1. 중첩 로직 (테이블 확인)
            if (defender.Paralyzed && m_ParalyzeTable.ContainsKey(defender))
            {
                DateTime end = m_ParalyzeTable[defender];
                
                if (end > now)
                {
                    TimeSpan remaining = end - now;
                    // 이미 마비 중이면 50% 효율로 합산
                    finalSeconds = Math.Min(maxDuration, remaining.TotalSeconds + (addDuration * 0.5));
                }
            }

            // 2. 마비 적용 및 테이블 갱신
            defender.Paralyze(TimeSpan.FromSeconds(finalSeconds));
            m_ParalyzeTable[defender] = now + TimeSpan.FromSeconds(finalSeconds);

            // 3. 타이머가 끝난 후 테이블에서 제거 (메모리 관리)
            Timer.DelayCall(TimeSpan.FromSeconds(finalSeconds), () => {
                if (m_ParalyzeTable.ContainsKey(defender) && m_ParalyzeTable[defender] <= DateTime.UtcNow)
                    m_ParalyzeTable.Remove(defender);
            });

            // 효과 알림
            if (!defender.Paralyzed) // 새로 걸린 경우만 메시지
            {
                attacker.SendLocalizedMessage(1060163); 
                defender.SendLocalizedMessage(1060164);
            }

            defender.FixedEffect(0x376A, 9, 32);
            defender.PlaySound(0x204);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
            OnHit(attacker, defender, damage);
        }
    }
}
