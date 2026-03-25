using System;
using System.Collections;
using Server.Mobiles;

namespace Server.Items
{
    public class DefenseMastery : WeaponAbility
    {
        private static readonly Hashtable m_Table = new Hashtable();

        public DefenseMastery() { }

        // 외부에서 참조할 수 있도록 static 메서드 제공
        public static bool IsPerfectDefense(Mobile m)
        {
            return m != null && m_Table.Contains(m);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            // 시각 효과 및 메시지
            attacker.FixedParticles(0x375A, 1, 17, 0x7F2, 0x3E8, 0x3, EffectLayer.Waist);
            attacker.PlaySound(0x1F2);
            //attacker.SendMessage("완전 방어 자세를 취합니다! (3초간 피해 1 고정)");

            // 기존 타이머가 있다면 중단 (중첩 방지)
            if (m_Table.Contains(attacker))
            {
                Timer t = m_Table[attacker] as Timer;
                if (t != null) t.Stop();
            }

            // 3초 후 테이블에서 제거하는 타이머 등록
            Timer timer = Timer.DelayCall(TimeSpan.FromSeconds(3.0), () => 
            {
                m_Table.Remove(attacker);
                //attacker.SendMessage("완전 방어 상태가 해제되었습니다.");
            });

            m_Table[attacker] = timer;
        }
    }
}
