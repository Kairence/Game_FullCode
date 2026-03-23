using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public enum MealSize { Light, Heavy, Luxury }

    public static class VirtualCitizenAI
    {
        // ====================================================================
        // 🕒 1. 메인 스케줄러 (30분 전역 타이머에서 6, 12, 18, 24시에 호출)
        // ====================================================================
        public static void ProcessQuarterlyRoutine(VirtualCitizen agent, TownEconomy town, int currentHour)
        {
            if (agent == null || town == null || agent.IsExpired) return;

            // 에이전트 상태 판별 (튜플 사용)
            var (isNightShift, isOverworked) = CheckAgentState(agent);

            // 올빼미족(NightShift)은 스케줄 시간을 논리적으로 반전시킴
            int logicalHour = isNightShift ? (currentHour + 12) % 24 : currentHour;
            if (logicalHour == 0) logicalHour = 24;

            // 과로 상태(Overworked)면 18시, 24시에도 강제 노동
            if (isOverworked && (logicalHour == 18 || logicalHour == 24))
            {
                ProcessProduction(agent, town, currentHour); // 휴식 스킵, 강제 노동
                agent.Satisfaction = Math.Max(0, agent.Satisfaction - 10); // 스트레스/만족도 하락
                return;
            }

            // [4분기 논리 스케줄링]
            switch (logicalHour)
            {
                case 6: // 아침
                    ProcessMeal(agent, town, MealSize.Light);
                    break;
                
                case 12: // 점심
                    ProcessProduction(agent, town, currentHour);
                    ProcessMeal(agent, town, MealSize.Heavy);
                    break;
                
                case 18: // 저녁
                    ProcessProduction(agent, town, currentHour);
                    ProcessMeal(agent, town, MealSize.Luxury);
                    ProcessSocialAndLuxury(agent, town); // 스트레스 해소
                    break;
                
                case 24: // 심야
                    ProcessNightNeeds(agent, town); // 양초/만족도 체크
                    ProcessRest(agent);
                    break;
            }
        }

        // ====================================================================
        // 📊 2. 상태 판별 (교대근무, 과로)
        // ====================================================================
        private static (bool IsNightShift, bool IsOverworked) CheckAgentState(VirtualCitizen agent)
        {
            int groupID = ((int)agent.JobClass / 100) * 100;
            
            // 유흥업(800), 범죄자(1100)는 밤에 일하는 올빼미족
            bool isNight = (groupID == 800 || groupID == 1100);
            
            // 개인 골드가 10 이하이거나 가족 자본이 바닥나면 과로 상태 진입 (아동/노인 제외)
            bool isOverworked = agent.IsProductive && (agent.Gold <= 10 || (agent.Family != null && agent.Family.SharedWealth <= 50));

            return (isNight, isOverworked);
        }

        // ====================================================================
        // 🍖 3. 식사 로직 (MealSize에 따른 지출 및 만족도)
        // ====================================================================
        // ====================================================================
        // 🍖 3. 식사 로직 (테스트용: 무조건 '빵(BreadLoaf)'만 소비)
        // ====================================================================
        private static void ProcessMeal(VirtualCitizen agent, TownEconomy town, MealSize size)
        {
            // 테스트를 위해 모든 식단 카테고리를 '빵' 하나로 통일합니다.
            Type targetFood = typeof(BreadLoaf);

            // 1. 창고에 빵 재고가 1개 이상 있는지 확인
            if (GetStock(town, targetFood) > 0)
            {
                // 2. 빵의 현재 동적 물가 시세 가져오기
                int currentPrice = town.GetPrice(targetFood);

                // 3. 빵값 지불 시도 (개인 골드 -> 가족 자산 순으로 확인)
                if (TryConsumeBudget(agent, currentPrice))
                {
                    // 4. 결제 성공! 창고에서 빵 1개 차감 & 마을 금고에 돈 입금
                    ConsumeResource(town, targetFood, 1);
                    town.Wealth += currentPrice;

                    // 5. 포만감 증가 (식사 규모에 따라 차등 회복)
                    int fullness = size switch { MealSize.Light => 15000, MealSize.Heavy => 30000, MealSize.Luxury => 50000, _ => 15000 };
                    agent.Hunger = Math.Min(100000, agent.Hunger + fullness);
                    
                    // 사치스러운 식사면 만족도 추가 보너스
                    if (size == MealSize.Luxury) agent.Satisfaction = Math.Min(100, agent.Satisfaction + 10);
                    else agent.Satisfaction = Math.Min(100, agent.Satisfaction + 2);

                    return; // 식사 성공했으므로 여기서 종료
                }
            }

            // [식사 실패] 창고에 빵이 없거나, 빵을 살 돈이 없는 경우
            // 카르마(도덕성)가 높은 NPC는 조금 참지만, 낮으면 스트레스가 폭발합니다.
            if (agent.Karma > 5000) agent.Satisfaction = Math.Max(0, agent.Satisfaction - 5);
            else agent.Satisfaction = Math.Max(0, agent.Satisfaction - 20);
        }

        // ====================================================================
        // 🛠️ 4. 생산 엔진 (야간 페널티 및 노약자 패널티 적용)
        // ====================================================================
        private static void ProcessProduction(VirtualCitizen agent, TownEconomy town, int realHour)
        {
            if (agent.IsChild) return; // 아동은 노동 금지

            double efficiency = agent.Potential;

            // 노년기(Elder)는 생산 효율 절반
            if (agent.IsElder) efficiency *= 0.5;

            // 밤(18시~06시)에 일할 때 횃불이 없으면 효율 50% 급락
            if (realHour == 24 || realHour == 6)
            {
                if (GetStock(town, typeof(Torch)) > 0) ConsumeResource(town, typeof(Torch), 1);
                else efficiency *= 0.5;
            }

            int groupID = ((int)agent.JobClass / 100) * 100;

            // 무스킬 기초 노동 (땔감, 횃불)
            if (Utility.RandomDouble() < 0.15)
            {
                if (GetStock(town, typeof(Log)) > 5)
                {
                    ConsumeResource(town, typeof(Log), 1);
                    ProduceResource(town, typeof(Kindling), 1);
                    agent.Fame += 1;
                }
            }
            else
            {
                // 전문 노동 (Herding, TasteID 등 기존 ProductionSystem 로직 통합)
                SkillName targetSkill = groupID switch
                {
                    100 => SkillName.Herding,
                    200 => SkillName.TasteID,
                    _ => SkillName.Mining // 임시 기본값
                };

                if (agent.Skills.TryGetValue(targetSkill, out double skillVal))
                {
                    if (Utility.RandomDouble() < (skillVal / 100.0))
                    {
                        // 결과물 창고 적재
                        ProduceResource(town, typeof(RawRibs), (int)(10 * efficiency)); 
                        if (skillVal < 100.0) agent.Skills[targetSkill] += 0.1;
                        agent.Fame += 5;
                    }
                }
            }
        }

        // ====================================================================
        // 💎 5. 사회, 사치, 스트레스 해소 (연령별 최우선 소비)
        // ====================================================================
        private static void ProcessSocialAndLuxury(VirtualCitizen agent, TownEconomy town)
        {
            int groupID = ((int)agent.JobClass / 100) * 100;
            ItemTag targetTag = ItemTag.Luxury; // 기본값

            // 연령별 우선순위 타겟 변경
            if (agent.IsChild) targetTag = ItemTag.Essential; // 아동은 교육/장난감
            else if (agent.IsElder) targetTag = ItemTag.Reagent; // 노인은 건강/포션

            // 성인은 직업군에 따라 사치품, 도구, 무기 소비
            if (agent.IsProductive)
            {
                targetTag = groupID switch
                {
                    200 or 900 => ItemTag.Tool,
                    300 => ItemTag.Armament,
                    _ => ItemTag.Luxury
                };
            }

            // 플레이어 벤더에서 구매 시도 (VirtualEconomyAI 연동)
            bool bought = VirtualEconomyAI.TryShopFromPlayerVendor(agent, town, targetTag, town.PriceMultiplier);
            
            if (bought)
            {
                // 사치품 구매 성공 시 스트레스 해소 및 Fame 상승
                agent.Satisfaction = Math.Min(100, agent.Satisfaction + 15);
                if (agent.Family != null) agent.Family.Prestige += 5; // 가문 위신 상승
            }
        }

        // ====================================================================
        // 🕯️ 6. 야간 생필품 (양초) 및 휴식 로직
        // ====================================================================
        private static void ProcessNightNeeds(VirtualCitizen agent, TownEconomy town)
        {
            int groupID = ((int)agent.JobClass / 100) * 100;

            // 귀족(500)과 장인(200)은 밤에 양초가 없으면 만족도 감소
            if (groupID == 500 || groupID == 200)
            {
                if (GetStock(town, typeof(Candle)) > 0)
                {
                    ConsumeResource(town, typeof(Candle), 1);
                }
                else
                {
                    agent.Satisfaction = Math.Max(0, agent.Satisfaction - 10);
                }
            }
        }

        private static void ProcessRest(VirtualCitizen agent)
        {
            // 수면 시 소폭의 만족도 회복
            agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);
        }

        // --- 헬퍼 메서드 (예산 소모, 창고 입출고) ---
        private static bool TryConsumeBudget(VirtualCitizen agent, int amount)
        {
            if (agent.Gold >= amount) { agent.Gold -= amount; return true; }
            if (agent.Family != null && agent.Family.SharedWealth >= amount) { agent.Family.SharedWealth -= amount; return true; }
            return false;
        }

        private static int GetStock(TownEconomy town, Type type) => town.Warehouse.TryGetValue(type, out var item) ? item.Stock : 0;
        private static void ConsumeResource(TownEconomy town, Type type, int amount) { if (town.Warehouse.TryGetValue(type, out var item)) item.Stock = Math.Max(0, item.Stock - amount); }
        private static void ProduceResource(TownEconomy town, Type type, int amount) { if (town.Warehouse.TryGetValue(type, out var item)) item.Stock += amount; else town.Warehouse[type] = new WarehouseItem(type, amount, 10); }
    }
}