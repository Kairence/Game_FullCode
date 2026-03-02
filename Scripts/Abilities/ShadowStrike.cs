using System;
using Server.Mobiles;

namespace Server.Items
{
	public class ShadowStrike : WeaponAbility
	{
		public ShadowStrike() { }

		// ���� �Ҹ� ����

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (attacker == null || defender == null || !defender.Alive)
				return;

			// 1. ȿ�� �˸� �� �ð� ȿ��
			attacker.SendLocalizedMessage(1060078); // You strike and hide in the shadows!
			defender.SendLocalizedMessage(1060166); // You feel disoriented!

			Effects.SendLocationParticles(
				EffectItem.Create(attacker.Location, attacker.Map, EffectItem.DefaultDuration),
				0x376A,
				8,
				12,
				9943
			);
			attacker.PlaySound(0x482);
			defender.FixedEffect(0x37BE, 20, 25);

			// 2. �ٽ� ����: ���� ������ 200% �߰� (�� 300% ����)
			int finalDamage = damage * 3;

			// 3. AOS.Damage ��Ŀ� ���� ��׷� 0 ����
			// ���� ����: target, from, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, keepAlive, type, aggro
			AOS.Damage(defender, attacker, finalDamage, false, 100, 0, 0, 0, 0, 0, 0, false, 0, 100);
		}
	}
}
