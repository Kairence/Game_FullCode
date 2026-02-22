using System;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Misc
{
    public class ResourceAchievementGump : Gump
    {
        public ResourceAchievementGump(Mobile from) : base(50, 50)
        {
            Account acct = from.Account as Account;
            if (acct == null || acct.Point == null) return;

            Closable = true; Dragable = true;
            AddPage(0);
            
            // 1. 전체 배경: 버튼 공간 확보를 위해 가로를 1100 -> 1200으로 확장
            AddBackground(0, 0, 1200, 785, 9270);
            AddHtml(0, 15, 1200, 30, "<BASEFONT SIZE=6 COLOR=#FFFF00><CENTER>시즌 업적: 전 자원 채취 현황</CENTER></BASEFONT>", false, false);

            // --- 상단 라인 (금속 & 나무) ---
            RenderCategoryHeader(acct, SeasonCount.MetalTotal, "[ 금 속 ]", 40, 55);
            RenderCategoryHeader(acct, SeasonCount.WoodTotal, "[ 나 무 ]", 620, 55); 
            AddImageTiled(20, 75, 1160, 2, 0x2621);

            // 금속 (버튼 ID 1000번대 사용)
            RenderResourceColumn(acct, new string[] { "철", "구리", "청동", "금", "아가파이트", "베라이트", "벨러라이트" }, 
                               new int[] { 0x19B9, 0x19B9, 0x19B9, 0x19B9, 0x19B9, 0x19B9, 0x19B9 }, 
                               new int[] { 0x000, 0x96D, 0x972, 0x8A5, 0x979, 0x89F, 0x8AB }, 40, 85, SeasonCount.MetalStart, true, 1000);

            // 나무 (버튼 ID 1100번대 사용)
            RenderResourceColumn(acct, new string[] { "나무", "떡갈 나무", "물푸레 나무", "주목 나무", "심재 나무", "피 나무", "서리 나무" }, 
                               new int[] { 0x1BDD, 0x1BDD, 0x1BDD, 0x1BDD, 0x1BDD, 0x1BDD, 0x1BDD }, 
                               new int[] { 0x000, 0x7DA, 0x4A7, 0x4A8, 0x4A9, 0x4AA, 0x47F }, 620, 85, SeasonCount.WoodStart, false, 1100);

            // --- 하단 라인 (가죽 & 생선) ---
            RenderCategoryHeader(acct, SeasonCount.LeatherTotal, "[ 가 죽 ]", 40, 405);
            RenderCategoryHeader(acct, SeasonCount.FishTotal, "[ 생 선 ]", 620, 405);
            AddImageTiled(20, 425, 1160, 2, 0x2621);

            // 가죽 (버튼 ID 1200번대 사용)
            RenderResourceColumn(acct, new string[] { "가죽", "질긴 가죽", "거친 가죽", "경화 가죽", "가시 가죽", "뿔 가죽", "미늘 가죽" }, 
                               new int[] { 0x1079, 0x1079, 0x1079, 0x1079, 0x1079, 0x1079, 0x1079 }, 
                               new int[] { 0x000, 0x283, 0x227, 0x1C1, 0x8AC, 0x845, 0x851 }, 40, 435, SeasonCount.LeatherStart, true, 1200);

            // 생선 (버튼 ID 1300번대 사용)
            RenderResourceColumn(acct, new string[] { "송어", "배스", "은어", "붕어", "메기", "대구", "농어" }, 
                               new int[] { 2508, 2509, 2510, 2511, 17606, 17159, 17155 }, 
                               new int[] { 0, 0, 0, 0, 0, 0, 0 }, 620, 435, SeasonCount.FishStart, true, 1300);

            AddImageTiled(20, 750, 1160, 25, 2624);
            AddHtml(20, 753, 1160, 20, "<BASEFONT COLOR=#ffff00><CENTER>항목 우측의 랭킹 버튼을 클릭하여 실시간 순위를 확인하세요! (계정 공유)</CENTER></BASEFONT>", false, false);
        }

        private void RenderCategoryHeader(Account acct, int idx, string title, int x, int y)
        {
            int current = acct.Point[idx];
            double pct = Math.Min(100.0, (double)current * 100.0 / SeasonCount.TotalMaxGoal);
            
            AddHtml(x, y, 540, 20, $"<BASEFONT SIZE=4 COLOR=#ffffff><CENTER>{title} : {current:N0} / 1,000,000,000 ({pct:F1}%)</CENTER></BASEFONT>", false, false);
            
            AddImageTiled(x + 115, y + 18, 310, 3, 0x13BE);
            if (pct > 0)
                AddImageTiled(x + 115, y + 18, (int)(310 * (pct / 100.0)), 3, 0x42);
        }

        private void RenderResourceColumn(Account acct, string[] names, int[] icons, int[] hues, int x, int yStart, int startIndex, bool moveUp, int baseButtonID)
        {
            for (int i = 0; i < names.Length; i++)
            {
                int y = yStart + (i * 45);
                int arrayIndex = startIndex + i;
                int currentCount = (arrayIndex < acct.Point.Length) ? acct.Point[arrayIndex] : 0;
                
                // 슬롯 배경 확장
                AddImageTiled(x, y, 540, 40, 9354);
                AddItem(x + 5, moveUp ? y + 5 : y + 10, icons[i], hues[i]); 
                AddLabel(x + 60, y + 2, 1152, names[i]);
                AddLabel(x + 160, y + 2, 88, $"{currentCount:N0} / {SeasonCount.MaxGoal:N0}"); 

                double pct = Math.Min(100.0, (double)currentCount * 100.0 / SeasonCount.MaxGoal);
                AddImageTiled(x + 60, y + 24, 380, 10, 0x13BE);
                if (pct > 0)
                    AddImageTiled(x + 60, y + 24, (int)(380 * (pct / 100.0)), 10, 0x0805);

                AddLabel(x + 450, y + 18, pct >= 100 ? 0x42 : 1152, $"{pct:F1}%");

                // --- 랭킹 버튼 추가 ---
                // 0x15E3: 돋보기 아이콘
                AddButton(x + 512, y + 12, 0x15E3, 0x15E7, baseButtonID + i, GumpButtonType.Reply, 0);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null) return;

            int bid = info.ButtonID;

            // 랭킹 버튼 처리 (1000 ~ 1399 범위)
            if (bid >= 1000 && bid < 1400)
            {
                int typeOffset = (bid / 100) * 100; // 1000, 1100, 1200, 1300 단위 추출
                int indexInGroup = bid % 100;
                int actualIndex = 0;

                // SeasonCount 클래스의 정의에 따라 인덱스 매칭
                if (typeOffset == 1000) actualIndex = SeasonCount.MetalStart + indexInGroup;
                else if (typeOffset == 1100) actualIndex = SeasonCount.WoodStart + indexInGroup;
                else if (typeOffset == 1200) actualIndex = SeasonCount.LeatherStart + indexInGroup;
                else if (typeOffset == 1300) actualIndex = SeasonCount.FishStart + indexInGroup;

                pm.SendGump(new SeasonRankingGump(pm, RankingType.Resource, actualIndex));
                return;
            }

            sender.Mobile.SendGump(new SeasonMainGump(sender.Mobile));
        }
    }
}