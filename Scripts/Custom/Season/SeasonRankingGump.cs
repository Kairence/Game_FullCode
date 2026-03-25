using System;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Accounting;
using System.Text.RegularExpressions;

namespace Server.Misc
{
    public enum RankingType { Skill, Resource, Monster }

    public class SeasonRankingGump : Gump
    {
        private RankingType m_Type;
        private int m_Index;

        public SeasonRankingGump(PlayerMobile from, RankingType type, int index) : base(150, 150)
        {
            m_Type = type;
            m_Index = index;

            string titleName = GetTitleName(index);

            Closable = true; Dragable = true;
            AddPage(0);
            AddBackground(0, 0, 450, 580, 9270);
            AddImageTiled(20, 20, 410, 40, 2624);
            
            // 타이틀 출력: 예) [Animal Taming] RANKING (TOP 10)
            AddHtml(20, 30, 410, 25, $"<BASEFONT SIZE=5 COLOR=#FFD700><CENTER>{titleName} RANKING (TOP 10)</CENTER></BASEFONT>", false, false);

            // 데이터 수집 로직
            var rankings = GetRankData(type, index);

            // 헤더
            AddLabel(50, 70, 0x35, "RANK");
            AddLabel(120, 70, 0x35, "NAME");
            AddLabel(320, 70, 0x35, type == RankingType.Skill ? "VALUE" : "COUNT");
            AddImageTiled(20, 90, 410, 2, 9354);

            double myValue = GetMyValue(from, type, index);
            int myRank = rankings.FindIndex(x => x.Name == from.Name) + 1;

            // 리스트 출력 (TOP 10)
            for (int i = 0; i < 10; i++)
            {
                int y = 100 + (i * 35);
                AddImageTiled(20, y, 410, 30, i % 2 == 0 ? 9354 : 9274);

                if (i < rankings.Count)
                {
                    var data = rankings[i];
                    int color = (data.Name == from.Name) ? 0x42 : 1152;
                    AddLabel(60, y + 5, color, (i + 1).ToString());
                    AddLabel(120, y + 5, color, data.Name);
                    AddLabel(330, y + 5, color, type == RankingType.Skill ? data.Value.ToString("F1") : data.Value.ToString("N0"));
                }
                else
                {
                    AddLabel(60, y + 5, 0x384, (i + 1).ToString());
                    AddLabel(120, y + 5, 0x384, "- Empty -");
                }
            }

            // 하단 내 정보
            AddImageTiled(20, 460, 410, 60, 2624);
            string valStr = type == RankingType.Skill ? myValue.ToString("F1") : myValue.ToString("N0");
            string myInfo = myRank > 0 
                ? $"나의 현재 순위: <BASEFONT COLOR=#00FF00>{myRank}위</BASEFONT> ({valStr})"
                : "랭킹 데이터가 없습니다.";
            
            AddHtml(30, 478, 390, 25, $"<BASEFONT SIZE=4 COLOR=#FFFFFF><CENTER>{myInfo}</CENTER></BASEFONT>", false, false);
            
            // 돌아가기 버튼
            AddButton(185, 530, 247, 248, 1, GumpButtonType.Reply, 0);
        }

        private string GetTitleName(int index)
        {
            switch (m_Type)
            {
                case RankingType.Skill: 
                    return SkillInfo.Table[index].Name;

                case RankingType.Resource:
                    return GetResourceName(index);

                case RankingType.Monster:
                    var list = MonsterDropHandler.GetRegisteredList();
                    if (index >= 0 && index < list.Count)
                        return Regex.Replace(list[index], "([a-z])([A-Z])", "$1 $2"); // CamelCase 분리
                    return "Monster";
            }
            return "Unknown";
        }

        // 자원 인덱스에 따른 명칭 반환 (SeasonCount 기준)
        private string GetResourceName(int index)
        {
            if (index >= SeasonCount.MetalStart && index < SeasonCount.MetalStart + 7)
                return new string[] { "철", "구리", "청동", "금", "아가파이트", "베라이트", "벨러라이트" }[index - SeasonCount.MetalStart];
            
            if (index >= SeasonCount.WoodStart && index < SeasonCount.WoodStart + 7)
                return new string[] { "나무", "떡갈 나무", "물푸레 나무", "주목 나무", "심재 나무", "피 나무", "서리 나무" }[index - SeasonCount.WoodStart];

            if (index >= SeasonCount.LeatherStart && index < SeasonCount.LeatherStart + 7)
                return new string[] { "가죽", "질긴 가죽", "거친 가죽", "경화 가죽", "가시 가죽", "뿔 가죽", "미늘 가죽" }[index - SeasonCount.LeatherStart];

            if (index >= SeasonCount.FishStart && index < SeasonCount.FishStart + 7)
                return new string[] { "송어", "배스", "은어", "붕어", "메기", "대구", "농어" }[index - SeasonCount.FishStart];

            return "자원";
        }

        private List<RankEntry> GetRankData(RankingType type, int index)
        {
            var list = new List<RankEntry>();
            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m is PlayerMobile pm && pm.Young)
                {
                    double val = GetMyValue(pm, type, index);
                    if (val > 0) list.Add(new RankEntry(pm.Name, val));
                }
            }
            return list.OrderByDescending(x => x.Value).ToList();
        }

        private double GetMyValue(PlayerMobile pm, RankingType type, int index)
        {
            switch (type)
            {
                case RankingType.Skill: return pm.Skills[index].Base;
                case RankingType.Resource:
                    Account acct = pm.Account as Account;
                    return (acct != null && index < acct.Point.Length) ? acct.Point[index] : 0;
                case RankingType.Monster:
                    // TODO: MonsterDropHandler에서 실제 킬수 데이터 연동
                    return 0; 
            }
            return 0;
        }

        public class RankEntry
        {
            public string Name; public double Value;
            public RankEntry(string n, double v) { Name = n; Value = v; }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                switch (m_Type)
                {
                    case RankingType.Skill: sender.Mobile.SendGump(new SkillAchievementGump(sender.Mobile)); break;
                    case RankingType.Resource: sender.Mobile.SendGump(new ResourceAchievementGump(sender.Mobile)); break;
                    case RankingType.Monster: sender.Mobile.SendGump(new MonsterDropHandlerGump(sender.Mobile, 0)); break;
                }
            }
        }
    }
}
