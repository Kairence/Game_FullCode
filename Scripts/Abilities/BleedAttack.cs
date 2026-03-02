using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	// 1. static 제거 및 WeaponAbility 상속
	public class BleedAttack : WeaponAbility
	{
		public BleedAttack() { }

		// 2. 메서드명을 OnHit으로 변경하여 클래스명과의 충돌 방지 및 오버라이드
		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (attacker == null || defender == null || !defender.Alive)
				return;

			// 1. 보스 및 네임드(레이드급) 체크 - 면역 처리
			if (defender is BaseCreature bc)
			{
				// 면역 조건: BleedImmune 설정이 되어있거나, 등급(Grade) 8 이상 보스
				if (bc.BleedImmune || bc.Grade >= 8)
				{
					attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
					return;
				}
			}

			// 2. 효과 알림
			attacker.SendLocalizedMessage(1060159); // Your target is bleeding!
			defender.SendLocalizedMessage(1060160); // You are bleeding!

			if (defender is PlayerMobile)
			{
				defender.LocalOverheadMessage(MessageType.Regular, 0x21, 1060757); // You are bleeding profusely
			}

			// 3. 시각 효과
			defender.PlaySound(0x133);
			defender.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);

			// 바닥에 피 효과
			Blood blood = new Blood();
			blood.ItemID = Utility.Random(0x122A, 5);
			blood.MoveToWorld(defender.Location, defender.Map);

			// 4. 기획 핵심 로직: 현재 체력의 5% 감소 (Direct Damage)
			int bleedDamage = (int)(defender.Hits * 0.05);

			// 최소 데미지 보정
			if (bleedDamage < 1)
				bleedDamage = 1;

			// 5. 방어 무시(Direct) 피해 입히기
			// 인자: defender, attacker, damage, phys, fire, cold, pois, nrgy, chaos, direct
			// 기획하신대로 방어 무시를 위해 마지막 인자(direct)에 100을 할당합니다.
			AOS.Damage(defender, attacker, bleedDamage, 0, 0, 0, 0, 0, 0, 100);
		}

		public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
		{
			OnHit(attacker, defender, damage);
		}
	}
}
