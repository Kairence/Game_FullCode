using System;
using Server;
using Server.Mobiles;
using Server.Regions;
using Server.Items;

namespace Server.Misc
{
    public static class CreatureBalancer
    {
        // 1. 등급 결정 확률 테이블
        public static int[,] MonsterLandTier =
        {
            { 900, 990, 1000 }, // Trammel
            { 0, 800, 900 },    // Felucca
            { 940, 990, 1000 }, // Ilshenar
            { 0, 0, 0 }  
        };

        // 2. 티어 변환 함수 (원본 MonsterGrade 유지)
        public static int MonsterGrade(int grade)
        {
            switch (grade)
            {
                case 1: break;
                case 2: case 3: case 4: case 5: grade = 2; break;
                case 6: grade = 3; break;
                case 7: grade = 4; break;
                case 8: grade = 5; break; // Boss
            }
            return grade;
        }

public static void Apply(BaseCreature bc)
        {
            if (bc == null || bc.Deleted || bc.Grade > 0) return;

            // 예외 처리: 상인, 가드, 펫, 무적 NPC 제외
            if (bc is BaseVendor || bc is BaseGuard || bc.Controlled || bc.Blessed || bc.NoKillAwards)
            {
                bc.Grade = 1;
                bc.Loyalty = 0; // 보너스 없음
                return;
            }

            Map map = bc.Map;
            if (map == null || map == Map.Internal) map = Map.Trammel; 

            // [Step 1] 등급 결정
            if (bc.Grade <= 0)
            {
                if (bc.Boss) bc.Grade = 8;
                else
                {
                    int dice = Utility.RandomMinMax(1, 1000);
                    int landCheck = (map == Map.Felucca) ? 1 : (map == Map.Ilshenar ? 2 : 0);

                    if (dice >= MonsterLandTier[landCheck, 2]) bc.Grade = 7;      // Chief
                    else if (dice >= MonsterLandTier[landCheck, 1]) bc.Grade = 6; // Elite
                    else if (dice >= MonsterLandTier[landCheck, 0]) bc.Grade = 2; // Rare
                    else bc.Grade = 1;                                           // Normal
                }
            }

            // [Step 2] 등급에 따른 강인함(Loyalty) 부여 (기획 반영)
            // Loyalty는 1당 0.1%의 스탯 보너스가 됩니다.
            switch (bc.Grade)
            {
                case 1: bc.Loyalty = Utility.RandomMinMax(0, 100); break;      // 0% ~ 10% 보너스
                case 2: bc.Loyalty = Utility.RandomMinMax(200, 350); break;    // 20% ~ 35% 보너스
                case 6: bc.Loyalty = Utility.RandomMinMax(500, 750); break;    // 50% ~ 75% 보너스
                case 7: bc.Loyalty = Utility.RandomMinMax(800, 900); break;    // 80% ~ 90% 보너스
                case 8: bc.Loyalty = 1000; break;                             // Boss: 100% 보너스 (2배)
                default: bc.Loyalty = 0; break;
            }

            // [Step 3] 외형 및 이름 변경
            ApplyAppearance(bc);

            // [Step 4] 강인함 기반 스탯 보정 (기존 gScalar 로직을 Loyalty로 대체)
            // 공식: 1.0 + (Loyalty / 1000.0)
            double loyaltyScalar = 1.0 + (bc.Loyalty / 1000.0);

            // 기본 스탯 배율 설정
            double sM = loyaltyScalar, dM = loyaltyScalar, iM = loyaltyScalar;
            double hM = loyaltyScalar, staM = loyaltyScalar, manM = loyaltyScalar;

            // [Step 5] AI 성향별 추가 특화 (강인함 보너스 위에 추가 곱연산)
            if (bc.AI == AIType.AI_Mage || bc.AI == AIType.AI_Necro)
            {
                iM *= 1.2; manM *= 1.2; 
            }
            else if (bc.AI == AIType.AI_Archer)
            {
                dM *= 1.2; staM *= 1.2;
            }
            else // Melee
            {
                sM *= 1.1; hM *= 1.1;
            }

            // [Step 6] 종족(Slayer)별 특성 보정 (완화된 수치 적용)
            ApplySlayerBonus(bc, ref sM, ref dM, ref iM, ref hM, ref staM, ref manM);

            // [Step 7] 최종 수치 적용 (오버플로 방지 1억 제한)
            bc.RawStr = (int)Math.Min(100000000, bc.RawStr * sM);
            bc.RawDex = (int)Math.Min(100000000, bc.RawDex * dM);
            bc.RawInt = (int)Math.Min(100000000, bc.RawInt * iM);
            bc.HitsMaxSeed = (int)Math.Min(100000000, bc.HitsMaxSeed * hM);
            bc.StamMaxSeed = (int)Math.Min(100000000, bc.StamMaxSeed * staM);
            bc.ManaMaxSeed = (int)Math.Min(100000000, bc.ManaMaxSeed * manM);

            bc.Hits = bc.HitsMax; bc.Stam = bc.StamMax; bc.Mana = bc.ManaMax;

            // [Step 8] 패시브 스킬 적용 (가장 마지막에 호출하여 배율 연산 오염 방지)
            AnimalPassiveSkillHandler.OnSpawn(bc);
        }

        private static void ApplyAppearance(BaseCreature bc)
        {
            if (bc.NoKillAwards) return;

            int grade = bc.Grade;
            if (grade == 2 && !bc.Name.StartsWith("Rare ")) { bc.Name = "Rare " + bc.Name; bc.Fame += 500; }
            else if (grade == 6 && !bc.Name.StartsWith("Elite ")) { bc.Name = "Elite " + bc.Name; bc.Hue = 1272; bc.Fame += 1500; }
            else if (grade == 7 && !bc.Name.StartsWith("Chief ")) { bc.Name = "Chief " + bc.Name; bc.Hue = 1157; bc.Fame += 3000; }
            else if (grade == 8 && !bc.Name.StartsWith("Boss ")) { bc.Name = "Boss " + bc.Name; }

            if (grade >= 2)
            {
                bc.Delta(MobileDelta.Name);
                bc.Delta(MobileDelta.Hue);
                bc.InvalidateProperties();
            }
        }

        private static void ApplySlayerBonus(BaseCreature bc, ref double sM, ref double dM, ref double iM, ref double hM, ref double staM, ref double manM)
        {
            if (IsSlayer(bc, SlayerName.Silver)) { hM *= 1.1; } 
            else if (IsSlayer(bc, SlayerName.ElementalBan)) { iM *= 1.15; manM *= 1.15; }
            else if (IsSlayer(bc, SlayerName.Exorcism)) { sM *= 1.1; hM *= 1.1; }
            else if (IsSlayer(bc, SlayerName.ArachnidDoom)) { dM *= 1.1; staM *= 1.1; }
            else if (IsSlayer(bc, SlayerName.ReptilianDeath)) { hM *= 1.2; sM *= 1.15; }
            else if (IsSlayer(bc, SlayerName.Fey)) { dM *= 1.2; iM *= 1.2; }

            if (IsSlayer(bc, SlayerName.Repond)) // Humanoid 기술 보정 (스킬은 곱연산 허용)
            {
                for (int i = 0; i < bc.Skills.Length; ++i)
                    bc.Skills[i].Base *= 1.25;
            }
        }

        private static bool IsSlayer(BaseCreature bc, SlayerName name)
        {
            SlayerEntry entry = SlayerGroup.GetEntryByName(name);
            return entry != null && entry.Slays(bc);
        }
    }
}