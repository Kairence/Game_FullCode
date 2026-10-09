using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Misc
{
    public static class BlackMarketEngine
    {
        // ==============================================================================
        // 1. 코어(Core) 제재: 장물은 일반 상점에 팔거나 개인 집에 락다운할 수 없음
        // ==============================================================================
        
        /// <summary>
        /// BaseVendor.cs 나 상점 판매 로직에서 호출하여 장물인지 검사합니다.
        /// </summary>
        public static bool CanSellToVendor(Item item, Mobile vendor)
        {
            // [수정] 레지스트리 검색 대신, 아이템 자체가 CityStolenItem인지 검사
            if (item is CityStolenItem)
            {
                // 장물아비(Fence/Thief) 직업군 NPC에게만 팔 수 있음
                if (vendor is BaseVendor bv && bv.Title != null && bv.Title.ToLower().Contains("fence"))
                    return true;

                return false; 
            }
            return true;
        }

        /// <summary>
        /// BaseHouse.cs의 LockDown() 함수 내부에서 호출하여 장물의 설치를 막습니다.
        /// </summary>
        public static bool CanLockDown(Item item, Mobile m)
        {
            // [수정] 아이템 자체가 CityStolenItem이면 즉시 차단
            if (item is CityStolenItem csi)
            {
                m.SendMessage(33, $"이것은 {csi.VictimHouse} 가문에서 도난당한 장물이라 떳떳하게 집에 배치할 수 없습니다!");
                return false;
            }
            return true;
        }

        // ==============================================================================
        // 2. 장물 세탁 (Laundering) 시스템
        // ==============================================================================
        
        /// <summary>
        /// 도둑 길드마스터 등 특정 NPC에게 돈을 주고 장물 꼬리표를 뗍니다.
        /// </summary>
        public static void LaunderStolenItem(Mobile thief, Item item)
        {
            // [수정] 아이템이 CityStolenItem 껍데기가 아니면 빠져나감
            if (!(item is CityStolenItem csi))
            {
                thief.SendMessage("이것은 장물이 아닙니다.");
                return;
            }

            // 아이템 가치의 30%를 세탁 수수료로 책정 (임시 계산)
            int itemValue = 100 * item.Amount; // 실제로는 TownEconomy 물가 연동 필요
            int launderFee = Math.Max(100, (int)(itemValue * 0.3));

            if (Banker.Withdraw(thief, launderFee))
            {
                // 🌟 [핵심] 껍데기 파괴 및 진짜 아이템(원본) 복원
                try
                {
                    Item cleanItem = (Item)Activator.CreateInstance(csi.OriginalType);
                    cleanItem.Hue = csi.Hue;
                    cleanItem.Amount = csi.Amount;
                    
                    // 가방에 깨끗한 원본 아이템을 넣고, 장물 껍데기는 삭제
                    thief.AddToBackpack(cleanItem);
                    csi.Delete();

                    thief.SendMessage(68, $"{launderFee} gp를 지불하고 장물 꼬리표를 제거했습니다. 이제 합법적인 아이템입니다.");
                    Effects.PlaySound(thief.Location, thief.Map, 0x037); // 금화 소리
                }
                catch
                {
                    thief.SendMessage(33, "세탁 과정에서 알 수 없는 오류가 발생했습니다.");
                }
            }
            else
            {
                thief.SendMessage(33, $"장물을 세탁하려면 {launderFee} gp가 필요합니다.");
            }
        }

        // ==============================================================================
        // 3. 암시장 파트타임 (Black Market Quests) 생성
        // ==============================================================================
        
        /// <summary>
        /// 매일 밤, 각 마을에 랜덤하게 암시장 의뢰(장물 매입)를 생성합니다.
        /// </summary>
        public static void GenerateBlackMarketRequests(TownEconomy town)
        {
            if (town == null) return;

            // 마을당 하루 1~2개의 암시장 의뢰만 생성
            int questCount = Utility.RandomMinMax(1, 2);

            for (int i = 0; i < questCount; i++)
            {
                // 주로 값비싼 자원이나 귀족 사치품을 타겟으로 함
                Type targetType = Utility.RandomList(typeof(IronIngot), typeof(GoldIngot), typeof(Board), typeof(Diamond));
                int amount = Utility.RandomMinMax(50, 200);

                // 상점가 대비 단 15%의 가격만 쳐주지만, 훔친 물건을 유일하게 합법 처분할 창구
                int blackMarketPrice = Math.Max(1, (int)(town.GetPrice(targetType) * 0.15));
                int totalReward = blackMarketPrice * amount;

                string title = $"[암시장] 묻지마 {targetType.Name} 매입";

                // 의뢰 등록 (기존 파트타임 매니저 사용)
                // IssuerHouse(발주처)를 null로 주어 시스템/어둠의 길드 발주임을 명시
                PartTimeManager.CreateAIRequest(town.TownName, title, JobCategory.Menial, targetType, amount, totalReward, null);
                
                Console.WriteLine($"[BlackMarket] {town.TownName}에 {targetType.Name} {amount}개 매입 의뢰가 올라왔습니다.");
            }
        }
    }
}