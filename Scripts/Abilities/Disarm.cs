using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;

namespace Server.Items
{
	// 1. static 제거 및 WeaponAbility 상속
	public class Disarm : WeaponAbility
	{
		// 모든 Disarm 인스턴스가 공유하는 면역 명단
		public static List<Mobile> m_DisarmImmunity = new List<Mobile>();

		public Disarm() { }

		public static bool IsDisarmImmune(Mobile m)
		{
			return m_DisarmImmunity != null && m_DisarmImmunity.Contains(m);
		}

		// 2. 메서드명을 OnHit으로 변경 (클래스명 충돌 방지)
		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (attacker == null || defender == null || !defender.Alive)
				return;

			// 1. 면역 체크
			if (IsDisarmImmune(defender))
			{
				attacker.SendMessage("상대가 아직 무장 해제에 면역 상태입니다.");
				return;
			}

			// 2. 효과 알림 및 시각 효과
			attacker.SendLocalizedMessage(1060157); // You disarm your opponent!
			defender.SendLocalizedMessage(1060158); // Your weapon has been disarmed!

			defender.PlaySound(0x3B9);
			defender.FixedParticles(0x37BE, 232, 25, 9948, EffectLayer.LeftHand);

			// 3. 공격력 감소 디버프 적용 (10초 고정)
			TimeSpan duration = TimeSpan.FromSeconds(10.0);
			int damageReduction = 50; // 50% 감소

			if (defender is PlayerMobile pm)
			{
				pm.disarmtime = DateTime.Now + duration;
				pm.disarmweak = damageReduction;
			}
			else if (defender is BaseCreature bc)
			{
				bc.disarmtime = DateTime.Now + duration;
				bc.disarmweak = damageReduction;
			}

			// 4. 면역 부여 (디버프 종료 후 다시 걸리지 않게 하려면 시간을 더 길게 잡으세요)
			AddDisarmImmunity(defender, duration + TimeSpan.FromSeconds(5.0)); // 15초 면역
		}

		public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
		{
			OnHit(attacker, defender, damage);
		}

		public static void AddDisarmImmunity(Mobile m, TimeSpan duration)
		{
			if (m == null)
				return;

			if (!m_DisarmImmunity.Contains(m))
				m_DisarmImmunity.Add(m);

			// 지정된 시간 후 면역 제거
			Timer.DelayCall(
				duration,
				() =>
				{
					if (m_DisarmImmunity.Contains(m))
						m_DisarmImmunity.Remove(m);
				}
			);
		}
	}
}
