using System;
using Server;
using Server.Mobiles;
using Server.Accounting;
using Server.Items;
using Server.Network;

namespace Server.Misc
{
    public static class FamilySystem
    {
        // 최적화된 읽기 전용 스팬 데이터
        public static ReadOnlySpan<int> DonationGold => [10000, 100000, 1000000, 10000000];
        public static ReadOnlySpan<int> GivePoint => [1, 9, 85, 800];

        // 티어 및 요구 미덕 점수 테이블 (ReadOnlySpan 활용으로 속도 극대화)
        private static ReadOnlySpan<int> TierMaxNodes => [3, 8, 13, 18, 24, 25];
        private static ReadOnlySpan<int> ReqVirtueScores => [50, 200, 450, 850, 1450, 2500];
        private static ReadOnlySpan<int> ReqTotalSpent => [0, 5, 15, 30, 50, 75, 100];

        public static void Configure()
        {
            EventSink.WorldLoad += OnWorldLoad;
            EventSink.VirtueGumpRequest += OnVirtueGumpRequest;
        }

        private static void OnVirtueGumpRequest(VirtueGumpRequestEventArgs e)
        {
            Mobile beholder = e.Beholder;
            Mobile beheld = e.Beheld;

            if (beholder == beheld && beholder is PlayerMobile pm)
            {
                pm.CloseGump(typeof(FamilyVirtueGump));
                pm.SendGump(new FamilyVirtueGump(pm, 0, 0));
            }
            else if (beholder != beheld)
            {
                beholder.SendMessage(0x22, "다른 가문의 내력은 엿볼 수 없습니다.");
            }
        }

        private static void OnWorldLoad()
        {
            if (Server.Event.dc == null)
            {
                foreach (Item item in World.Items.Values)
                {
                    if (item is DonationCheck dc)
                    {
                        Server.Event.dc = dc;
                        break;
                    }
                }
            }
        }

        // --- 가문 스킬 연마 핵심 로직 ---

        public static bool CanUpgrade(PlayerMobile pm, int skillNodeID, out string failReason)
        {
            failReason = "";
            if (pm == null || !(pm.Account is Account acc)) return false;

            FamilySkillNode node = FamilySkillManager.Skills[skillNodeID];
            if (node == null) return false;

            // 1. 마스터 여부
            if (acc.Point[skillNodeID] >= node.MaxLevel)
            {
                failReason = "MASTER";
                return false;
            }

            int pageBase = ((skillNodeID - 401) / 25) * 25 + 400;
            int relID = skillNodeID - pageBase; // 1 ~ 25
            int virtueIdx = (skillNodeID - 401) / 25;

            // 2. 미덕 자격 조건 및 누적 포인트 조건 결정 (Span 루프 활용)
            int tier = 6;
            int reqVirtue = 2500;
            int reqTotal = 100;

            ReadOnlySpan<int> thresholds = TierMaxNodes;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (relID <= thresholds[i])
                {
                    tier = i + 1;
                    reqVirtue = ReqVirtueScores[i];
                    reqTotal = ReqTotalSpent[i];
                    break;
                }
            }

            // 미덕 점수 체크 (Point 1~8)
            if (acc.Point[virtueIdx + 1] < reqVirtue)
            {
                failReason = String.Format("{0}티어 자격 부족 (요구: {1})", tier, reqVirtue);
                return false;
            }

            // 가문 스킬 총 투자 포인트 체크
            int totalSpent = 0;
            for (int i = 401; i <= 600; i++) totalSpent += acc.Point[i];

            if (totalSpent < reqTotal)
            {
                failReason = String.Format("누적 {0}pt 필요", reqTotal);
                return false;
            }

            // 3. 선행 스킬 체크 (부모 노드)
            if (relID >= 4)
            {
                int p1 = 0, p2 = 0;
                if (relID == 4 || relID == 5) p1 = 1;
                else if (relID == 6) p1 = 2;
                else if (relID == 7 || relID == 8) p1 = 3;
                else if (relID >= 9 && relID <= 18) p1 = relID - 5;
                else if (relID == 19) { p1 = 14; p2 = 15; }
                else if (relID == 20) p1 = 16;
                else if (relID == 21) { p1 = 17; p2 = 18; }
                else if (relID >= 22 && relID <= 24) p1 = relID - 3;
                else if (relID == 25) { p1 = 22; p2 = 23; }

                bool parentMet = false;
                if (p1 > 0 && acc.Point[pageBase + p1] >= 3) parentMet = true;
                if (p2 > 0 && acc.Point[pageBase + p2] >= 3) parentMet = true;
                if (relID == 25 && !parentMet && acc.Point[pageBase + 24] >= 3) parentMet = true;

                if (!parentMet)
                {
                    failReason = "선행 스킬 3Lv 필요";
                    return false;
                }
            }

            // 4. 고티어 스킬 총합 제한
            if (relID >= 19 && pm.Skills.Total < 5000)
            {
                failReason = "스킬 총합 부족";
                return false;
            }

            return true;
        }

        public static int CalculateSkillCost(Account acc)
        {
            int totalLevel = 0;
            for (int i = 401; i <= 600; i++) totalLevel += acc.Point[i];
            return 1000 + (totalLevel * 100);
        }

        public static void UpgradeFamilySkill(PlayerMobile pm, int skillNodeID)
        {
            if (pm == null || !(pm.Account is Account acc)) return;

            if (!CanUpgrade(pm, skillNodeID, out string failReason))
            {
                pm.SendMessage(0x22, failReason);
                return;
            }

            int cost = CalculateSkillCost(acc);

            if (acc.Point[0] >= cost)
            {
                acc.Point[0] -= cost;
                acc.Point[skillNodeID] += 1;
                
                pm.SendMessage(0x42, "{0} 연마 완료! (잔여 가문 명예: {1:#,0} Pt)", FamilySkillManager.Skills[skillNodeID].Name, acc.Point[0]);
                pm.UpdateEquipOptions();
            }
            else
            {
                pm.SendMessage(0x22, "가문 명예 점수가 부족합니다. (필요: {0:#,0} Pt)", cost);
            }
        }

        // --- 기존 기부 및 랭킹 시스템 ---

        public static void ProcessDonation(PlayerMobile pm, int type)
        {
            if (pm == null || pm.Account == null) return;

            int goldAmount = DonationGold[type];
            int pointAmount = GivePoint[type];

            if (Banker.GetBalance(pm) >= goldAmount)
            {
                Banker.Withdraw(pm, goldAmount, true);
                Account acc = pm.Account as Account;
                
                acc.Point[0] += pointAmount;
                Contribute(acc.Username, goldAmount, FamilyCompType.Wealth, false);

                pm.SendMessage(0x42, "가문에 {0:#,0} 골드를 기부하여 명예 점수를 올렸습니다.", goldAmount);
            }
            else
            {
                pm.SendMessage(0x22, "은행 잔고가 부족합니다.");
            }
        }

        public static void RollScratcher(PlayerMobile pm)
        {
            int dice = Utility.RandomMinMax(1, 10000);
            int reward = 0;

            if (dice <= 25) reward = 1000000;
            else if (dice <= 125) reward = 100000;
            else if (dice <= 1125) reward = 10000;

            if (reward > 0)
            {
                Banker.Deposit(pm, reward);
                if (reward == 1000000)
                    World.Broadcast(0x42, false, "{0}님이 즉석 복권에서 100만 골드에 당첨되셨습니다!", pm.Name);
                else
                    pm.SendMessage(0x42, "복권 당첨! {0:#,0} 골드를 획득했습니다.", reward);
            }
            else
            {
                pm.SendMessage(0x22, "운이 없네요.");
            }
        }

        public static void Contribute(string name, int baseScore, FamilyCompType type, bool isNpc)
        {
            DonationCheck dc = Server.Event.dc;
            if (dc == null || baseScore <= 0 || DateTime.Now >= dc.RespawnTime) return;

            DayOfWeek day = DateTime.Now.DayOfWeek;
            double mult = (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) ? 2.5 : 1.0;
            int score = (int)(baseScore * mult);

            int cat = (int)type;
            int idx = -1;

            for (int i = 0; i < 100; i++) 
            { 
                if (dc.RankingNames[cat][i] == name) 
                { 
                    idx = i; 
                    break; 
                } 
            }

            if (idx != -1) 
            { 
                dc.RankingScores[cat][idx] += score; 
                SortRanking(cat, idx); 
            }
            else if (score > dc.RankingScores[cat][99])
            {
                dc.RankingNames[cat][99] = name;
                dc.RankingScores[cat][99] = score;
                dc.IsNpc[cat][99] = isNpc;
                SortRanking(cat, 99);
            }
        }

        private static void SortRanking(int cat, int index)
        {
            DonationCheck dc = Server.Event.dc;
            for (int i = index; i > 0; i--)
            {
                if (dc.RankingScores[cat][i] > dc.RankingScores[cat][i - 1])
                {
                    string n = dc.RankingNames[cat][i - 1]; 
                    dc.RankingNames[cat][i - 1] = dc.RankingNames[cat][i]; 
                    dc.RankingNames[cat][i] = n;

                    int s = dc.RankingScores[cat][i - 1]; 
                    dc.RankingScores[cat][i - 1] = dc.RankingScores[cat][i]; 
                    dc.RankingScores[cat][i] = s;

                    bool npc = dc.IsNpc[cat][i - 1]; 
                    dc.IsNpc[cat][i - 1] = dc.IsNpc[cat][i]; 
                    dc.IsNpc[cat][i] = npc;
                } 
                else break;
            }
        }
    }
}