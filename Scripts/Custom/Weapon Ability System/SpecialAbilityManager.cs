using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public class SpecialAbilityManager
    {
        // 모든 특수기 풀 (인덱스 유지)
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
            WeaponAbility.MortalStrike      // 15
        };

        // 전술 수치 기반 매핑 테이블 (행: 무기ID 0~9, 열: Tier 0~3)
        // 요청하신 정렬 순서대로 행(Row)을 배치했습니다.
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

        // 1. [OPL용] 특정 무기 ID의 모든 기술 이름 반환
        public static string[] GetAbilityNames(int typeID)
        {
            if (typeID < 0 || typeID > 9) return new string[] { "None", "None", "None", "None" };

            string[] names = new string[4];
            for (int i = 0; i < 4; i++)
            {
                int index = _AbilityMap[typeID, i];
                WeaponAbility ability = AbilityPool[index];
                names[i] = (ability != null) ? ability.GetType().Name : "None";
            }
            return names;
        }

        // 2. [전투용] 전술 수치에 따른 누적 시전 (int typeID로 변경)
        public static void ExecuteChainAbilities(int typeID, Mobile attacker, Mobile defender, int damage)
        {
            if (typeID < 0 || typeID > 9) return;

            double tactics = attacker.Skills.Tactics.Value;
            int maxTier = (tactics >= 200) ? 3 : (tactics >= 150) ? 2 : (tactics >= 100) ? 1 : (tactics >= 50) ? 0 : -1;

            if (maxTier == -1) return;

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