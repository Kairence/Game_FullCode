using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class EconomyOptionPricer
    {
        // ==============================================================================
        // [시장가] NPC 판매 및 모험가/유저 간 거래 시 적용 (하이엔드 가치)
        // ==============================================================================
        public static int CalculateRetailValue(int basePrice, List<(int OptionID, int RawAmount)> options)
        {
            if (options == null || options.Count == 0) return basePrice;

            double totalOptionValue = 0;
            int validOptionCount = Math.Min(options.Count, 5); 

            foreach (var (optId, rawAmount) in options)
            {
                int amount = rawAmount / 10000;
                if (amount <= 0) amount = 1;

                totalOptionValue += GetSingleOptionValue(optId, amount);
            }

            double synergyMultiplier = validOptionCount switch
            {
                1 => 1.10, 2 => 1.25, 3 => 1.45, 4 => 1.70, _ => 2.00 
            };

            // [개선] 매직 아이템 기본 프리미엄 (깡통 가격의 2배 보장)
            long magicBasePrice = basePrice * 2;
            long finalPrice = magicBasePrice + (long)(totalOptionValue * synergyMultiplier);
            
            return (int)Math.Min(finalPrice, 2_000_000_000); 
        }

        // ==============================================================================
        // [상점 매입가] 유저/모험가가 NPC 벤더에게 처분할 때 (인플레 방지 및 최저가 보장)
        // ==============================================================================
        public static int CalculatePawnValue(int basePrice, List<(int OptionID, int RawAmount)> options)
        {
            // 마법 옵션이 없는 일반 깡통 장비는 무자비하게 반값 처리
            if (options == null || options.Count == 0) return Math.Max(1, basePrice / 2);

            double totalOptionValue = 0;
            foreach (var (optId, rawAmount) in options)
            {
                int amount = rawAmount / 10000;
                if (amount <= 0) amount = 1;
                
                totalOptionValue += GetSingleOptionValue(optId, amount);
            }

            // [개선] 매직 아이템 상점 최저가 보장 (깡통 가격의 1.5배)
            // 예: 100골드짜리 무기에 옵션이 붙으면 무조건 150골드 + @ 부터 시작
            long magicBasePawnPrice = (long)(basePrice * 1.5);
            long pawnPrice = magicBasePawnPrice + (long)(totalOptionValue * 0.03);

            // 매입가 상한선 50,000 골드
            return (int)Math.Min(pawnPrice, 50000); 
        }

        private static double GetSingleOptionValue(int optionID, int amount)
        {
            return optionID switch
            {
                CustomOption.Str or CustomOption.Dex or CustomOption.Int 
                    => (amount * 200) + (Math.Pow(amount, 1.5) * 20),
                CustomOption.Hits or CustomOption.Stam or CustomOption.Mana 
                    => (amount * 150) + (Math.Pow(amount, 1.4) * 15),
                CustomOption.Luck 
                    => (amount * 10) + (Math.Pow(amount / 10.0, 1.5) * 5),
                CustomOption.AllStat or CustomOption.AllRes 
                    => (amount * 1500) + (Math.Pow(amount, 2) * 50),
                CustomOption.WeaponDamage or CustomOption.SpellDamage or CustomOption.AllDamage 
                    => (amount * 300) + (Math.Pow(amount, 2) * 25),
                CustomOption.SwingSpeed or CustomOption.SpellSpeed or CustomOption.AllSpeed 
                    => (amount * 400) + (Math.Pow(amount, 2) * 30),
                _ => (amount * 150) + (Math.Pow(amount, 1.2) * 5)
            };
        }

        public static int EvaluateItemMarketPrice(TownEconomy town, Item item, List<(int OptionID, int RawAmount)> extractedOptions)
        {
            return CalculateRetailValue(town.GetPrice(item.GetType()), extractedOptions);
        }

        public static int EvaluateItemPawnPrice(TownEconomy town, Item item, List<(int OptionID, int RawAmount)> extractedOptions)
        {
            return CalculatePawnValue(town.GetPrice(item.GetType()), extractedOptions);
        }
    }
}