using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
	public class LightningArrow : WeaponAbility
	{
		public LightningArrow() { }

		// 마나 소모 없음

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (attacker == null || defender == null || !defender.Alive)
				return;

			// 1. 주 타겟 처리 (에너지 100% 피해)
			// 기본 데미지에 300%를 추가하여 총 400%로 설정하거나,
			// 기획 의도에 따라 추가분만 300%라면 3배로 설정 (여기서는 총 4배로 설정)
			int finalDamage = damage * 4;

			defender.BoltEffect(0); // 번개 이펙트
			defender.PlaySound(0x29); // 번개 사운드

			// 주 타겟에게 에너지 100% 피해
			AOS.Damage(defender, attacker, finalDamage, 0, 0, 0, 0, 100, 0, 0);

			// 2. 광역 처리 (피격자 기준 3타일)
			int range = 3;
			List<Mobile> targets = new List<Mobile>();

			// defender.GetMobilesInRange를 사용하여 피격자 주변을 탐색합니다.
			IPooledEnumerable eable = defender.GetMobilesInRange(range);
			foreach (Mobile m in eable)
			{
				// 공격자, 주 타겟 제외 / 공격 가능 대상 확인 / 가시 거리 확인
				if (m != attacker && m != defender && m.Alive && attacker.CanBeHarmful(m, false) && attacker.InLOS(m))
				{
					targets.Add(m);
				}
			}
			eable.Free();

			// 3. 주변 적들에게 에너지 피해 가함
			if (targets.Count > 0)
			{
				foreach (Mobile m in targets)
				{
					m.BoltEffect(0);
					// 주변 적들에게도 동일하게 에너지 100% 속성으로 300% 추가된 피해 전달
					AOS.Damage(m, attacker, finalDamage, 0, 0, 0, 0, 100, 0, 0);
				}

				targets.Clear();
			}
		}
	}
}
