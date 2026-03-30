using System;
using Server.Items;

namespace Server.Misc
{
    public static class BioFoodRegistry
    {
        // 영양 카테고리 정의
        public enum NutritionGroup 
        { 
            Meat, Carbs, Fish, VeggieFruit, Stew, Pie, Sweets, Dairy, Raw 
        }

        // 아이템의 명시적 Type을 검사하여 (체중, 대사, 집중, 감각, 적응) 튜플 반환
        public static (int w, int m, int f, int p, int a) GetBioBonuses(Food food)
        {
            // C# 패턴 매칭을 이용한 Type 직접 검사 (문자열 검색 O(n) -> 타입 매칭 O(1) 최적화)
            NutritionGroup group = food switch
            {
                // === 1. 육류 (단백질/지방) ===
                Bacon or SlabOfBacon or CookedBird or RoastPig or Sausage or Ham or 
                Ribs or LambLeg or ChickenLeg or Hamburger or HotDog or CookableSausage or 
                PulledPorkPlatter or PulledPorkSandwich or TurkeyDinner or RoastDuck or 
                RoastTurkey or RoastChicken or TurkeyLeg 
                    => NutritionGroup.Meat,

                // === 2. 어류 (오메가3/고단백) ===
                FishSteak or TroutFishSteak or BassFishSteak or ShinerFishSteak or 
                CrucianCarpFishSteak or CatFishSteak or CodFishSteak or PerchFishSteak or 
                FerringFishSteak or TunaFishSteak 
                    => NutritionGroup.Fish,

                // === 3. 곡물 (복합 탄수화물) ===
                BreadLoaf or FrenchBread or BasketOfRolls or DinnerRoll 
                    => NutritionGroup.Carbs,

                // === 4. 디저트/당류 (정크푸드) ===
                Cake or Cookies or Muffins or CandyCane or GingerBreadCookie 
                    => NutritionGroup.Sweets,

                // === 5. 파이/피자 (탄단지 복합 조리) ===
                CheesePizza or SausagePizza or FruitPie or MeatPie or PumpkinPie or 
                ApplePie or PeachCobbler or Quiche or SweetPotatoPie or SliceOfPie 
                    => NutritionGroup.Pie,

                // === 6. 유제품/알 (칼슘) ===
                CheeseWheel or CheeseWedge or CheeseSlice or FriedEggs 
                    => NutritionGroup.Dairy,

                // === 7. 채소/과일 (비타민) ===
                HoneydewMelon or YellowGourd or GreenGourd or EarOfCorn or Turnip or 
                MashedSweetPotatoes 
                    => NutritionGroup.VeggieFruit,

                // === 8. 국물/소스 (수분/전해질) ===
                GibletGravey 
                    => NutritionGroup.Stew,

                // === 9. 생식 및 기타 식재료 ===
                // 사과, 당근 등 분류되지 않은 순수 Food 아이템은 모두 여기로 빠집니다.
                // (CookableFood는 Food 클래스가 아니므로 제외됨)
                _   => NutritionGroup.Raw
            };

            int ff = food.FillFactor;

            // 1 FF 단위 수치 배분 공식 (단위: 1% = 10,000)
            return group switch
            {
                // 근육량(대사) 증가, 체온 유지(적응) 미세 상승.
                NutritionGroup.Meat         => (w: ff * 400, m: ff * 500, f: 0,       p: 0,       a: ff * 100),
                
                // 두뇌(집중, 감각) 활성화. 체중 증가는 상대적으로 적음.
                NutritionGroup.Fish         => (w: ff * 200, m: ff * 300, f: ff * 200, p: ff * 500, a: 0),
                
                // 뇌 에너지원(집중) 극대화. 소화(대사) 보조.
                NutritionGroup.Carbs        => (w: ff * 400, m: ff * 100, f: ff * 500, p: 0,       a: 0),
                
                // 시력(감각) 및 환경 저항(적응) 극대화. 초저칼로리.
                NutritionGroup.VeggieFruit  => (w: ff * 100, m: ff * 100, f: ff * 100, p: ff * 300, a: ff * 400),
                
                // [양날의 검] 집중력 일시 폭발이나, 살이 찌고 대사/적응 깎임.
                NutritionGroup.Sweets       => (w: ff * 800, m: ff *-200, f: ff * 600, p: ff *-100, a: ff *-200),
                
                // 체온 유지 및 극한 환경 극복(적응) 최상급.
                NutritionGroup.Stew         => (w: ff * 300, m: ff * 200, f: ff * 100, p: ff * 100, a: ff * 600),
                
                // 탄단지가 뭉쳐 모든 스탯이 고르게 오르나 살이 찌기 쉬움.
                NutritionGroup.Pie          => (w: ff * 600, m: ff * 300, f: ff * 300, p: 0,       a: ff * 200),
                
                // 기초 체력(대사) 및 뼈 건강(적응).
                NutritionGroup.Dairy        => (w: ff * 400, m: ff * 300, f: ff * 100, p: ff * 100, a: ff * 100),
                
                // 날 것: 오직 체중만 증가
                _ /* Raw */                 => (w: ff * 500, m: 0,       f: 0,       p: 0,       a: 0)
            };
        }

        // [신규] 직관적이고 담백한 아이템 설명 텍스트 조합
        public static string GetFlavorText(int w, int m, int f, int p, int a)
        {
            string desc = "";

            // 1. 체중(Weight) 수치에 따른 첫인상 (칼로리 묘사)
            if (w >= 6000) 
                desc += "열량이 매우 높아 보인다. ";
            else if (w >= 4000) 
                desc += "열량이 꽤 높아 보인다. ";
            else 
                desc += "가볍게 배를 채우기 좋아 보인다. ";

            // 2. 정크푸드(마이너스 스탯 존재) 예외 처리
            if (m < 0 || a < 0)
            {
                desc += "달콤하지만 건강에는 좋지 않을 것 같다.";
                return desc.Trim();
            }

            // 3. 가장 수치가 높은 특수 스탯 1개에 대한 직관적 설명
            int max = Math.Max(Math.Max(m, f), Math.Max(p, a));

            if (max > 0)
            {
                if (max == f) 
                    desc += "먹으면 머리가 맑아지고 집중력이 오를 것 같다."; // 생산, 마법 등 확률 보정
                else if (max == m) 
                    desc += "먹으면 활력이 생기고 기력이 빠르게 회복될 것 같다."; // 전투, 회복 보정
                else if (max == p) 
                    desc += "먹으면 주의력이 높아져 탐색에 도움이 될 것 같다."; // 채집, 함정, 상위 티어 보정
                else if (max == a) 
                    desc += "먹으면 든든해져 험한 환경에서도 잘 버틸 수 있을 것 같다."; // 던전, 기후 패널티 보정
            }

            return desc.Trim();
        }
    }
}