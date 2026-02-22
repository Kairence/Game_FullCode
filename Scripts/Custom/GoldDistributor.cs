using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
using Server.Engines.PartySystem;

namespace Server.Misc
{
    public static class GoldDistributor
    {
        public static void Distribute(BaseCreature bc, List<DamageStore> rights)
        {
            if (bc == null || rights == null || rights.Count == 0) return;

            // 1. 유효한 대상(파티원 포함) 목록 추출
            HashSet<Mobile> killers = GetValidKillers(rights);
            if (killers.Count == 0) return;

            // 2. 전체 보상 풀(Pool) 결정 및 배분
            DetermineTotalRewards(bc, killers);
        }

        private static HashSet<Mobile> GetValidKillers(List<DamageStore> rights)
        {
            HashSet<Mobile> killers = new HashSet<Mobile>();
            foreach (DamageStore ds in rights)
            {
                if (!ds.m_HasRight || ds.m_Mobile == null) continue;

                Party party = Party.Get(ds.m_Mobile);
                if (party != null)
                {
                    foreach (PartyMemberInfo info in party.Members)
                    {
                        Mobile m = info.Mobile;
                        // 같은 맵, 30타일 이내, 살아있는 파티원 포함
                        if (m != null && m.Alive && m.Map == ds.m_Mobile.Map && ds.m_Mobile.InRange(m.Location, 30))
                            killers.Add(m);
                    }
                }
                else if (ds.m_Mobile.Alive) killers.Add(ds.m_Mobile);
            }
            return killers;
        }

        private static void DetermineTotalRewards(BaseCreature bc, HashSet<Mobile> killers)
        {
            int killerCount = killers.Count;
            
            // [A] 전체 보상 풀 계산
            int totalGoldPool = 10 + Utility.RandomMinMax(bc.Fame / 30, bc.Fame / 15);
            int totalFame = bc.Fame / 100;
            int totalKarma = -bc.Karma / 100;

            // 등급 보너스 (Grade 시스템 연동)
            if (bc.Grade >= 6) totalGoldPool = (int)(totalGoldPool * 1.5);
            if (bc.Boss) totalGoldPool *= 2;

            // N분의 1 분배량 계산
            int shareGold = totalGoldPool / killerCount;
            int shareFame = totalFame / killerCount;
            int shareKarma = totalKarma / killerCount;
            int extraGold = totalGoldPool % killerCount; // 잔돈

            // [B] 드랍 개수 결정 (아이템)
            int dropCount = 0;
            if (bc.Boss) dropCount = Utility.RandomMinMax(7, 12);
            else if (bc.Grade == 7) dropCount = Utility.RandomMinMax(4, 8);
            else if (bc.Grade == 6) dropCount = Utility.RandomMinMax(2, 5);
            else if (bc.Grade >= 2) dropCount = Utility.RandomMinMax(1, 3);
            else dropCount = Utility.RandomMinMax(0, 1);

            // [C] 실제 배분 루프
            int i = 0;
            foreach (Mobile m in killers)
            {
                if (!(m is PlayerMobile) || !m.Alive) continue;
                PlayerMobile pm = (PlayerMobile)m;

                // 1. 실버 포인트 지급
                double silverBonus = 1000 + pm.SilverPoint[2] * 50 + AosAttributes.GetValue(pm, AosAttribute.LowerAmmoCost);
                int get_silverpoint = (int)(bc.Fame * 0.33 * silverBonus / 1000);
                pm.Getsilverpoint(get_silverpoint);

                // 2. 명성 및 카르마 지급
                if (shareFame > 0) Titles.AwardFame(pm, shareFame, true);
                if (shareKarma != 0) Titles.AwardKarma(pm, shareKarma, true);

                // 3. 골드 지급 (첫 번째 사람에게 잔돈 포함)
                int myGold = shareGold + (i == 0 ? extraGold : 0);
                if (myGold > 0)
                {
                    double individualBonus = 1.0 + (AosAttributes.GetValue(pm, AosAttribute.NightSight) * 0.001);
                    int finalGold = (int)(myGold * individualBonus);
                    pm.AddToBackpack(new Gold(finalGold));
                    pm.SendMessage(0x48, "파티 분배: {0} gold, {1} fame, {2} karma", finalGold, shareFame, shareKarma);
                }
				//몬스터 템 드랍. 테스트 중이기 때문에 무조건 장비 1개 드랍으로 진행
				double totalChance = (bc.Fame / 100.0);

				// 계산된 점수를 기반으로 등급 결정 및 옵션 부여 호출
				// pm(플레이어) 정보는 필요한 경우 참조용으로만 전달
				//Misc.ItemOptionCreator.ItemCreator(droppedItem, totalChance, pm);				

                // 4. 특별 아이템 드랍 (기존 확률 유지)
				/*
                for (int k = 0; k < dropCount; ++k)
                {
                    if (Utility.RandomMinMax(1, 1000) <= 100)
                        DropSpecialItem(pm, bc);
                }
				*/

                i++;
            }
        }

        private static void DropSpecialItem(PlayerMobile pm, BaseCreature bc)
        {
            Type type = (Utility.RandomDouble() < 0.99) ? Util.MonsterDropItem(bc) : Util.MonsterHiddenDropItem(bc);
            if (type == null) return;

            Item item = Loot.Construct(type);
            if (item != null)
            {
                pm.AddToBackpack(item);
                pm.SendMessage(0x35, "특별한 아이템을 획득했습니다: {0}", Util.GetName(item));
            }
        }
    }
}