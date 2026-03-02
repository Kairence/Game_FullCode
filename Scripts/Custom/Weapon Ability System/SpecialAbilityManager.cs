using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
	public class SpecialAbilityManager
	{
		// ��� Ư���� Ǯ (�ε��� ����)
		public static WeaponAbility[] AbilityPool = new WeaponAbility[]
		{
			WeaponAbility.ArmorIgnore, // 0
			WeaponAbility.BleedAttack, // 1
			WeaponAbility.Disarm, // 2
			WeaponAbility.Bladeweave, // 3
			WeaponAbility.CrushingBlow, // 4
			WeaponAbility.ParalyzingBlow, // 5
			WeaponAbility.WhirlwindAttack, // 6
			WeaponAbility.Dismount, // 7
			WeaponAbility.ConcussionBlow, // 8
			WeaponAbility.PsychicAttack, // 9
			WeaponAbility.InfectiousStrike, // 10
			WeaponAbility.ShadowStrike, // 11
			WeaponAbility.DoubleStrike, // 12
			WeaponAbility.MovingShot, // 13
			WeaponAbility.LightningArrow, // 14
			WeaponAbility.MortalStrike, // 15
		};

		// ���� ��ġ ��� ���� ���̺� (��: ����ID 0~9, ��: Tier 0~3)
		// ��û�Ͻ� ���� ������� ��(Row)�� ��ġ�߽��ϴ�.
		private static readonly int[,] _AbilityMap = new int[,]
		{
			/* 0: �Ѽ� ��   */{ 0, 1, 2, 3 },
			/* 1: ��� ��   */{ 0, 4, 5, 6 },
			/* 2: ����      */{ 4, 1, 7, 6 },
			/* 3: �Ѽ� �б� */{ 8, 5, 9, 2 },
			/* 4: ��� �б� */{ 4, 8, 7, 6 },
			/* 5: �Ѽ� ��� */{ 0, 10, 11, 12 },
			/* 6: ��� ��� */{ 0, 5, 1, 12 },
			/* 7: Ȱ        */{ 1, 10, 12, 14 },
			/* 8: ����      */{ 0, 4, 5, 15 },
			/* 9: �Ǽ�      */{ 8, 5, 2, 9 },
		};

		// 1. [OPL��] Ư�� ���� ID�� ��� ��� �̸� ��ȯ
		public static string[] GetAbilityNames(int typeID)
		{
			if (typeID < 0 || typeID > 9)
				return new string[] { "None", "None", "None", "None" };

			string[] names = new string[4];
			for (int i = 0; i < 4; i++)
			{
				int index = _AbilityMap[typeID, i];
				WeaponAbility ability = AbilityPool[index];
				names[i] = (ability != null) ? ability.GetType().Name : "None";
			}
			return names;
		}

		// 2. [������] ���� ��ġ�� ���� ���� ���� (int typeID�� ����)
		public static void ExecuteChainAbilities(int typeID, Mobile attacker, Mobile defender, int damage)
		{
			if (typeID < 0 || typeID > 9)
				return;

			double tactics = attacker.Skills.Tactics.Value;
			int maxTier =
				(tactics >= 200) ? 3
				: (tactics >= 150) ? 2
				: (tactics >= 100) ? 1
				: (tactics >= 50) ? 0
				: -1;

			if (maxTier == -1)
				return;

			for (int i = 0; i <= maxTier; i++)
			{
				int poolIndex = _AbilityMap[typeID, i];
				WeaponAbility ability = AbilityPool[poolIndex];
				if (ability != null)
				{
					ability.OnHit(attacker, defender, damage);
				}
			}
		}
	}
}
