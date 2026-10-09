using System;
using Server;
using Server.Mobiles;
using Server.Items;
namespace Server.Misc
{
    public static class CombatMastery
    {
        public const int MaxLevel = 100;

        public static void Configure()
        {
            // 서버 구동 시 필요한 초기화 작업이 있다면 이곳에 작성합니다.
            // 예: 이벤트 등록 등
        }

        // =========================================================
        // 1. 경험치 및 레벨 공식 (HarvestMastery 곡선과 동일)
        // =========================================================
        public static int GetNextExp(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            return (currentLevel + 1) * (currentLevel + 1) * 25;
        }

        public static int GetLevel(int totalExp)
        {
            int level = 0;
            while (level < MaxLevel && totalExp >= GetNextExp(level))
            {
                level++;
            }
            return level;
        }

        // =========================================================
        // 2. 몬스터 처치 시 핵심 엔진 (AOS.cs 또는 BaseCreature.OnDeath 연동)
        // =========================================================
        public static void OnKilled(PlayerMobile pm, BaseCreature bc)
        {
            if (pm == null || bc == null || bc.Deleted)
                return;

            // 1. 개별 몬스터 도감(MonsterKills) 업데이트
            string className = bc.GetType().Name;
            if (!pm.MonsterKills.ContainsKey(className))
                pm.MonsterKills[className] = 0;
            
            pm.MonsterKills[className] += 1; // 1킬 = 1포인트 (10000포인트 = 1레벨)

            // 2. 슈퍼 슬레이어(SlayerData) 업데이트
            int sIdx = GetSlayerCategoryIndex(bc);
            if (sIdx != -1 && pm.SlayerData != null && pm.SlayerData.Length > sIdx)
            {
                pm.SlayerData[sIdx] += 1;
            }

            // 3. 등급(GradeData) 업데이트
            int gIdx = GetGradeIndex(bc.Grade);
            if (gIdx != -1 && pm.GradeData != null && pm.GradeData.Length > gIdx)
            {
                pm.GradeData[gIdx] += 1;
            }
        }

        // =========================================================
        // 3. [핵심] 등급 영구 패시브를 장비 배열에 적용 (PlayerMobile 연동)
        // =========================================================
        // 이 함수는 PlayerMobile.cs의 UpdateEquipOptions() 내에서 
        // 기존 장비 옵션들이 모두 합산된 직후에 단 한 번 호출됩니다.
        public static void ApplyGradePassiveOptions(PlayerMobile pm, int[] equipOptions)
        {
            if (pm.GradeData == null || equipOptions == null) return;

            // ---------------------------------------------------------
            // [인덱스 0: 일반 (Normal) 보너스] - 기획 삭제됨 (현재 GradeData[0]은 희귀부터 시작할 수 있으나, 기존 기획을 보존하려면 0번 인덱스는 미사용)
            // ---------------------------------------------------------

            // ---------------------------------------------------------
            // [인덱스 1: 희귀 (Rare) 보너스] - 체급 뻥튀기
            // ---------------------------------------------------------
            int rareLv = pm.GradeData.Length > 1 ? GetLevel(pm.GradeData[1]) : 0;
            if (rareLv >= 1)  equipOptions[CustomOption.AllStat] += rareLv * 10000; // 매 1렙: 힘/민/지 1 증가
            if (rareLv >= 10) equipOptions[CustomOption.AllRes] += (rareLv / 10) * 100000; // 매 10렙: 체/기/마 10 증가
            if (rareLv >= 25) equipOptions[CustomOption.Luck] += (rareLv / 25) * 250000; // 매 25렙: 행운 25 증가
            if (rareLv >= 100) 
            {
                // 올스탯(7종) 100 증가: 힘민지(+100), 체기마(+100), 행운(+100)
                equipOptions[CustomOption.AllStat] += 1000000; 
                equipOptions[CustomOption.AllRes] += 1000000;
                equipOptions[CustomOption.Luck] += 1000000;
            }

            // ---------------------------------------------------------
            // [인덱스 2: 엘리트 (Elite) 보너스] - 속도와 치명타
            // ---------------------------------------------------------
            int eliteLv = pm.GradeData.Length > 2 ? GetLevel(pm.GradeData[2]) : 0;
            if (eliteLv >= 1) equipOptions[CustomOption.AllSpeed] += eliteLv * 1000; // 매 1렙: 모든 속도 0.1% 증가
            if (eliteLv >= 10) 
            {
                int critChance = (eliteLv / 10) * 10000; // 매 10렙: 치명 확률 1% 증가
                equipOptions[CustomOption.WeaponCriChance] += critChance;
                equipOptions[CustomOption.SpellCriChance] += critChance;
            }
            if (eliteLv >= 25) equipOptions[CustomOption.AllPlus_35_44] += (eliteLv / 25) * 500000; // 매 25렙: 최종 피해 50 증가
            // 100렙 5% 확률 전체피해 2배는 데미지 타격 엔진(AOS.cs)에서 GetEliteDoubleDamageChance()로 개별 처리

            // ---------------------------------------------------------
            // [인덱스 3: 치프 (Chief) 보너스] - 파괴력과 방어
            // ---------------------------------------------------------
            int chiefLv = pm.GradeData.Length > 3 ? GetLevel(pm.GradeData[3]) : 0;
            if (chiefLv >= 1) equipOptions[CustomOption.AllPlus_35_44] += chiefLv * 50000; // 매 1렙: 최종 피해 5 증가
            if (chiefLv >= 10) equipOptions[CustomOption.AllStat] += (chiefLv / 10) * 200000; // 매 10렙: 힘/민/지 20 증가
            if (chiefLv >= 25) equipOptions[CustomOption.AllGain] += (chiefLv / 25) * 10000; // 매 25렙: 적 처치 시 체기마 1 회복
            if (chiefLv >= 100) equipOptions[CustomOption.AllArmor] += 50000; // 100렙: 모든 방어력 5 증가

            // ---------------------------------------------------------
            // [인덱스 4: 보스 (Boss) 보너스] - 파밍 극대화
            // ---------------------------------------------------------
            int bossLv = pm.GradeData.Length > 4 ? GetLevel(pm.GradeData[4]) : 0;
            if (bossLv >= 1) equipOptions[CustomOption.Luck] += bossLv * 50000; // 매 1렙: 행운 5 증가
            if (bossLv >= 10) equipOptions[CustomOption.Gold] += (bossLv / 10) * 100000; // 매 10렙: 기본 골드 10 추가
            // 25렙 아이템 드랍 +1, 100렙 장비 드랍 +1 은 루팅 엔진(LootPack.cs)에서 GetBossExtraDrops()로 개별 처리

            // ---------------------------------------------------------
            // [인덱스 5: 네임드 (Named) 보너스] - 미구현
            // ---------------------------------------------------------
        }

        // =========================================================
        // 4. [타격 시 발동] 개별 도감(Bestiary) 보너스 조회 함수
        // =========================================================
        public static double GetBestiaryAllDamage(int level) => level >= 1 ? level * 0.001 : 0.0; // 매 1렙: 모든 피해 0.1%
        public static int GetBestiaryFinalDamage(int level) => level >= 10 ? (level / 10) * 1 : 0; // 매 10렙: 최종 피해 1
        public static int GetBestiaryCritDamage(int level) => level >= 25 ? (level / 25) * 25 : 0; // 매 25렙: 치명 추가 피해 25
        public static bool CheckBestiaryResistIgnore(int level) => level >= 100 && Utility.RandomDouble() < 0.05; // 100렙: 5% 확률 저항 무시

        // =========================================================
        // 5. [타격 시 발동] 종족(Slayer) 보너스 조회 함수
        // =========================================================
        public static double GetSlayerAllDamage(int level) => level >= 1 ? level * 0.001 : 0.0; // 매 1렙: 모든 피해 0.1%
        public static int GetSlayerFinalDamage(int level) => level >= 10 ? (level / 10) * 2 : 0; // 매 10렙: 최종 피해 2
        public static double GetSlayerDamageReduction(int level) => level >= 25 ? 0.05 : 0.0; // 매 25렙: 받는 피해 5% 감소 (100렙 20%)
        public static bool CheckSlayerMaxRoll(int level) => level >= 100 && Utility.RandomDouble() < 0.20; // 100렙: 20% 주사위 최대치

        // =========================================================
        // 6. [타격/루팅 시 발동] 100레벨 전용 및 특수 보너스 조회 함수
        // =========================================================
        public static bool CheckEliteDoubleDamage(int level) => level >= 100 && Utility.RandomDouble() < 0.05; // 5% 전체 피해 2배
        
        public static int GetBossExtraItemDrops(int level) => level >= 25 ? 1 : 0; // 아이템 드랍 +1
        public static int GetBossExtraEquipDrops(int level) => level >= 100 ? 1 : 0; // 장비 드랍 +1

        // =========================================================
        // 7. 헬퍼 메서드: 인덱스 변환 로직
        // =========================================================
        public static int GetSlayerCategoryIndex(BaseCreature bc)
        {
            if (SlayerGroup.GetEntryByName(SlayerName.Repond) != null && SlayerGroup.GetEntryByName(SlayerName.Repond).Slays(bc)) return 0;
            if (SlayerGroup.GetEntryByName(SlayerName.Silver) != null && SlayerGroup.GetEntryByName(SlayerName.Silver).Slays(bc)) return 1;
            if (SlayerGroup.GetEntryByName(SlayerName.ElementalBan) != null && SlayerGroup.GetEntryByName(SlayerName.ElementalBan).Slays(bc)) return 2;
            if (SlayerGroup.GetEntryByName(SlayerName.Exorcism) != null && SlayerGroup.GetEntryByName(SlayerName.Exorcism).Slays(bc)) return 3;
            if (SlayerGroup.GetEntryByName(SlayerName.ArachnidDoom) != null && SlayerGroup.GetEntryByName(SlayerName.ArachnidDoom).Slays(bc)) return 4;
            if (SlayerGroup.GetEntryByName(SlayerName.ReptilianDeath) != null && SlayerGroup.GetEntryByName(SlayerName.ReptilianDeath).Slays(bc)) return 5;
            if (SlayerGroup.GetEntryByName(SlayerName.Fey) != null && SlayerGroup.GetEntryByName(SlayerName.Fey).Slays(bc)) return 6;
            return 7; // Others
        }

        public static int GetGradeIndex(int grade)
        {
            // 유저님 서버의 몬스터 Grade(1~9 등) 수치에 따라 0~5번 인덱스로 매핑
            // (0: 일반, 1: 희귀, 2: 엘리트, 3: 치프, 4: 보스, 5: 네임드)
            if (grade <= 1) return 0;       
            if (grade <= 3) return 1;       
            if (grade <= 5) return 2;       
            if (grade == 6) return 3;       
            if (grade >= 7 && grade <= 8) return 4; 
            if (grade >= 9) return 5;       
            return -1;
        }
    }
}