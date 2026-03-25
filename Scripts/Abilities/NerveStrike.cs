using System;
using System.Collections;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class NerveStrike : WeaponAbility
    {
        public static readonly TimeSpan Duration = TimeSpan.FromSeconds(15.0);
        private static readonly Hashtable m_Table = new Hashtable();

        public NerveStrike() { }

        public static bool IsCripple(Mobile m) => m != null && m_Table.Contains(m);

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            damage *= 3; // 200% 추가 공격 (총 300%)

            attacker.PlaySound(0x204);
            defender.FixedEffect(0x376A, 9, 32);
            defender.FixedParticles(0x37C4, 1, 8, 0x13AF, 0, 0, EffectLayer.Waist);
            
            attacker.SendLocalizedMessage(1063356);
            defender.SendLocalizedMessage(1063357);

            BeginCripple(defender, Duration);

            AOS.Damage(defender, attacker, damage, 100, 0, 0, 0, 0, 0, 0);
        }

		public static void BeginCripple(Mobile m, TimeSpan duration)
		{
			// [핵심] 이미 걸려있다면 속도를 원복시킨 후 새로 시작해야 수치가 꼬이지 않음
			if (m_Table.Contains(m))
				EndCripple(m);

			// 타이머 생성 (생성자에서 현재 정상 속도 저장)
			InternalTimer t = new InternalTimer(m, duration);
			m_Table[m] = t;

			m.YellowHealthbar = true;

			if (m is BaseCreature bc)
			{
				// 원본 속도를 저장한 상태에서 2배 느리게 설정
				bc.PassiveSpeed *= 2.0; 
				bc.ActiveSpeed *= 2.0;
			}
			
			t.Start();
			m.Delta(MobileDelta.Attributes);
		}

		public static void EndCripple(Mobile m)
		{
			if (!m_Table.Contains(m)) return;

			InternalTimer t = (InternalTimer)m_Table[m];
			if (t != null) 
			{
				t.Stop();
				// [핵심] 타이머에 저장해둔 '진짜 원본' 속도로 확실히 복구
				if (m is BaseCreature bc)
				{
					bc.PassiveSpeed = t.OldPassive;
					bc.ActiveSpeed = t.OldActive;
				}
			}

			m_Table.Remove(m);
			m.YellowHealthbar = false;
			m.Delta(MobileDelta.Attributes);
			m.SendLocalizedMessage(1060208); // You are no longer weakened.
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile m_Mobile;
			// 속도 복구를 위해 외부(EndCripple)에서 접근 가능하게 설정
			public double OldPassive { get; }
			public double OldActive { get; }

			public InternalTimer(Mobile m, TimeSpan duration) : base(duration) 
			{
				m_Mobile = m;
				if (m is BaseCreature bc)
				{
					// [주의] 이 시점의 bc.PassiveSpeed는 아직 2배 연산 전이어야 함
					OldPassive = bc.PassiveSpeed;
					OldActive = bc.ActiveSpeed;
				}
			}

			protected override void OnTick()
			{
				EndCripple(m_Mobile);
			}
		}
    }
}
