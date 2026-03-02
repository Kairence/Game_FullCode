using System;
using Server.Mobiles;

namespace Server.Items
{
	public class Dismount : WeaponAbility
	{
		public Dismount() { }

		public override int BaseMana => 10;

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!this.Validate(attacker) || defender == null || !defender.Alive)
				return;

			// 스테미너 체크 (기본 소모)
			if (!this.CalculateStam(attacker, Misc.Util.SPMStam[5, 0], Misc.Util.SPMStam[5, 1], 0, false))
				return;

			IMount mount = defender.Mount;
			bool isFlying = defender.Flying;

			// 1. 상대가 타고 있거나 날고 있는 경우에만 로직 실행
			if (mount != null || isFlying)
			{
				// 시각 및 사운드 효과
				defender.PlaySound(0x140);
				defender.FixedParticles(0x3728, 10, 15, 9955, EffectLayer.Waist);

				// 낙마 실행 (말에서 내리게 함)
				DoDismount(attacker, defender, mount, 10.0);

				// 2. 핵심 기획: 10초 동안 받는 피해 50% 증가 디버프 적용
				if (defender is PlayerMobile pm)
				{
					pm.disarmtime = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
					pm.disarmweak = 50; // 50% 피해 증가
				}
				else if (defender is BaseCreature bc)
				{
					bc.disarmtime = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
					bc.disarmweak = 50;
				}

				attacker.SendMessage("상대를 낙마시키고 10초간 취약 상태(피해 50% 증가)로 만들었습니다!");
			}

			// 기본 공격 데미지는 BaseWeapon에서 처리되므로 여기선 추가 Damage 호출 없음
		}

		public static void DoDismount(Mobile attacker, Mobile defender, IMount mount, double delay)
		{
			attacker.SendLocalizedMessage(1060082); // 낙마 성공 메시지

			if (defender is PlayerMobile pm)
			{
				pm.SetMountBlock(BlockMountType.Dazed, TimeSpan.FromSeconds(delay), true);
			}
			else if (mount != null)
			{
				mount.Rider = null;
			}
		}

		public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics)
		{
			OnHit(attacker, defender, damage);
		}
	}
}
