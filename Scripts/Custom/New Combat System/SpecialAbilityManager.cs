using System;
using System.Collections;
using Server.Mobiles;
using Server.Network;
using Server.Items;

namespace Server.Misc
{
    public class SpecialAbilityManager
    {
        // 모든 특수기 풀 (신규 극의 5종 포함 20번까지 확장)
        public static WeaponAbility[] AbilityPool = new WeaponAbility[]
        {
            WeaponAbility.ArmorIgnore,      // 0
            WeaponAbility.BleedAttack,      // 1
            WeaponAbility.Disarm,           // 2
            WeaponAbility.Bladeweave,       // 3
            WeaponAbility.CrushingBlow,     // 4
            WeaponAbility.ParalyzingBlow,   // 5
            WeaponAbility.WhirlwindAttack,  // 6
            WeaponAbility.Dismount,         // 7
            WeaponAbility.ConcussionBlow,   // 8
            WeaponAbility.PsychicAttack,    // 9
            WeaponAbility.InfectiousStrike, // 10
            WeaponAbility.ShadowStrike,     // 11
            WeaponAbility.DoubleStrike,     // 12
            WeaponAbility.MovingShot,       // 13
            WeaponAbility.LightningArrow,   // 14
            WeaponAbility.MortalStrike,     // 15
            /* 신규 무구의 극의 5종 */
            WeaponAbility.NerveStrike,      // 16 (신경 공격 - 검/도끼)
            WeaponAbility.ConcussionBlow, // 17 (뇌진탕 일격 - 둔기)
            WeaponAbility.TalonStrike,      // 18 (갈퀴 발톱 - 펜싱)
            WeaponAbility.ForceArrow,       // 19 (힘의 화살 - 궁술)
            WeaponAbility.DefenseMastery  // 20 (완전 방어 - 맨손)
        };

        // 전술 매핑 (Row: 무기ID, Col: 전술 티어 50, 100, 150, 200)
        private static readonly int[,] _AbilityMap = new int[,]
        {
            /* 0: 한손 검   */ { 0, 1, 2, 3 },
            /* 1: 양손 검   */ { 0, 4, 5, 6 },
            /* 2: 도끼      */ { 4, 1, 7, 6 },
            /* 3: 한손 둔기 */ { 8, 5, 9, 2 },
            /* 4: 양손 둔기 */ { 4, 8, 7, 6 },
            /* 5: 한손 펜싱 */ { 0, 10, 11, 12 },
            /* 6: 양손 펜싱 */ { 0, 5, 1, 12 },
            /* 7: 활        */ { 1, 10, 12, 14 },
            /* 8: 석궁      */ { 0, 4, 5, 15 },
            /* 9: 맨손      */ { 8, 5, 2, 9 }
        };

        public static void ExecuteChainAbilities(int typeID, Mobile attacker, Mobile defender, int damage)
        {
            if (typeID < 0 || typeID > 9) return;

            // 1. [전술 50~200] 기존 4단계 연쇄 시전
            double tactics = attacker.Skills.Tactics.Value;
            int maxTier = (tactics >= 200) ? 3 : (tactics >= 150) ? 2 : (tactics >= 100) ? 1 : (tactics >= 50) ? 0 : -1;

            if (maxTier != -1)
            {
                for (int i = 0; i <= maxTier; i++)
                {
                    WeaponAbility ability = AbilityPool[_AbilityMap[typeID, i]];
                    if (ability != null)
                        ability.OnHit(attacker, defender, damage);
                }
            }

            // 2. [무기술 200] 별도 체크하여 보너스 극의 시전
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            if (atkWeapon != null && attacker.Skills[atkWeapon.Skill].Value >= 200.0)
            {
                int ultimateIdx = GetUltimateIndex(typeID);
                if (ultimateIdx >= 16)
                {
                    WeaponAbility ultimate = AbilityPool[ultimateIdx];
                    if (ultimate != null)
                        ultimate.OnHit(attacker, defender, damage);
                }
            }
        }

        private static int GetUltimateIndex(int typeID)
        {
            if (typeID <= 2) return 16;      // 검/도끼 -> 신경 공격
            if (typeID <= 4) return 17;      // 둔기 -> 뇌진탕 일격
            if (typeID <= 6) return 18;      // 펜싱 -> 갈퀴 발톱
            if (typeID <= 8) return 19;      // 궁술 -> 힘의 화살
            if (typeID == 9) return 20;      // 맨손 -> 완전 방어
            return -1;
        }
    }
}
