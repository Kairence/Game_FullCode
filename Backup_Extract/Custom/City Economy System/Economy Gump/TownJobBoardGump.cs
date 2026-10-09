using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Engines.Quests;

namespace Server.Misc
{
    public class TownJobBoardGump : Gump
    {
        private readonly PlayerMobile m_Viewer;
        private readonly PartTimeAccountProfile m_Profile;
        private readonly TownEconomy m_Town;

        public TownJobBoardGump(PlayerMobile viewer, PartTimeAccountProfile profile, TownEconomy town) : base(50, 50)
        {
            m_Viewer = viewer; m_Profile = profile; m_Town = town;
            viewer.CloseGump(typeof(TownJobBoardGump));
            SetupGumpLayout();
        }

        private void SetupGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 800, 600, 9270);
            AddAlphaRegion(10, 10, 780, 580);

            string townName = m_Town != null ? m_Town.TownName : "알 수 없는";
            
            AddHtml(0, 25, 800, 30, $"<center><basefont color=#FFD700>{townName} 마을 파트타임 게시판</basefont></center>", false, false);

            string tColor = GetTierHexColor(m_Profile.CurrentTier);
            AddHtml(50, 75, 400, 20, $"<basefont color=#FFFFFF>현재 등급: </basefont><basefont color={tColor}><b>{GetTierString(m_Profile.CurrentTier)}</b></basefont>", false, false);
            AddHtml(550, 75, 200, 20, $"<basefont color=#FFD700><div align=right>남은 횟수: <b>{m_Profile.AvailableCharges}회</b></div></basefont>", false, false);

            AddImageTiled(40, 105, 720, 30, 2624);
            AddImageTiled(40, 140, 720, 3, 96);

            JobCategory[] categories = Enum.GetValues<JobCategory>();
            int tabX = 35;

            // [수정1] 탭 페이지 시작 번호를 UO 기본 시작 페이지인 '1'로 맞춥니다. (c * 100 + 1)
            for (int c = 0; c < categories.Length; c++)
            {
                JobCategory cat = categories[c];
                int basePage = c * 100 + 1; 
                
                AddButton(tabX, 110, 0x4B9, 0x4BA, 0, GumpButtonType.Page, basePage);
                AddHtml(tabX + 15, 110, 90, 20, $"<center><basefont color=#FFFFFF>{GetCategoryString(cat)}</basefont></center>", false, false);
                tabX += 105;
            }

            for (int c = 0; c < categories.Length; c++)
            {
                JobCategory cat = categories[c];
                List<TownJobRequest> catJobs = GetValidJobs(cat);
                
                int itemsPerPage = 12;
                int totalPages = Math.Max(1, (int)Math.Ceiling(catJobs.Count / (double)itemsPerPage));

                for (int p = 0; p < totalPages; p++)
                {
                    // [수정2] 각 탭의 실제 내용 페이지 번호도 Page 1부터 시작하도록 매핑
                    int currentPage = c * 100 + p + 1;
                    AddPage(currentPage);

                    int listY = 160;

                    for (int i = p * itemsPerPage; i < (p + 1) * itemsPerPage && i < catJobs.Count; i++)
                    {
                        TownJobRequest req = catJobs[i];
                        int globalIndex = PartTimeManager.ActiveRequests.IndexOf(req);

                        string originColor = req.Origin == JobOrigin.TownPublic ? "#32CD32" : "#1E90FF";
                        string itemColor = GetTierHexColor(req.Tier);
                        string originText = req.Origin == JobOrigin.TownPublic ? "마을 공용" : "시민 의뢰";
                        
                        string content = $"<basefont color={originColor}>[{originText}]</basefont> <basefont color={itemColor}>[{GetTierString(req.Tier)}] {req.Title}</basefont> <basefont color=#FFD700>({req.RewardGold:N0}gp)</basefont>";

                        AddImage(50, listY + 2, 2510);
                        AddHtml(75, listY, 600, 20, content, false, false);

                        if (req.IsFullyBooked)
                        {
                            AddImage(680, listY + 2, 9720); 
                            AddHtml(705, listY, 80, 20, "<basefont color=#A9A9A9>[진행 중]</basefont>", false, false);
                        }
                        else if (PartTimeManager.CanAcceptJob(m_Viewer, req))
                        {
                            AddButton(680, listY + 2, 2117, 2118, 100 + globalIndex, GumpButtonType.Reply, 0);
                            AddHtml(705, listY, 60, 20, "<basefont color=#FFFFFF>수락</basefont>", false, false);
                        }
                        else
                        {
                            AddImage(680, listY + 2, 9720); 
                            AddHtml(705, listY, 80, 20, "<basefont color=#A9A9A9>자격 미달</basefont>", false, false);
                        }

                        listY += 30;
                    }

                    if (catJobs.Count == 0)
                        AddHtml(0, 300, 800, 20, $"<center><basefont color=#A9A9A9>현재 등록된 {GetCategoryString(cat)} 의뢰가 없습니다.</basefont></center>", false, false);

                    int bottomY = 550;
                    if (p > 0)
                    {
                        AddButton(330, bottomY + 2, 4014, 4015, 0, GumpButtonType.Page, currentPage - 1);
                        AddHtml(365, bottomY, 50, 20, "<basefont color=#FFFFFF>이전</basefont>", false, false);
                    }
                    if (p < totalPages - 1)
                    {
                        AddButton(450, bottomY + 2, 4005, 4006, 0, GumpButtonType.Page, currentPage + 1);
                        AddHtml(410, bottomY, 50, 20, "<basefont color=#FFFFFF>다음</basefont>", false, false);
                    }
                    AddHtml(0, bottomY, 800, 20, $"<center><basefont color=#FFD700>- {p + 1} / {totalPages} -</basefont></center>", false, false);
                }
            }
        }

        private List<TownJobRequest> GetValidJobs(JobCategory cat)
        {
            List<TownJobRequest> validJobs = new();
            foreach (var req in PartTimeManager.ActiveRequests)
            {
                if (req.IsAIAssigned) continue;
                
                // [수정3] 문자열 공백이나 대소문자 차이로 퀘스트가 증발하는 현상 원천 차단
                if (m_Town != null && !string.IsNullOrEmpty(req.TownName))
                {
                    if (!req.TownName.Equals(m_Town.TownName, StringComparison.OrdinalIgnoreCase)) 
                        continue;
                }
                
                if (req.Category == cat) validJobs.Add(req);
            }
            return validJobs;
        }

        private string GetTierHexColor(JobTier tier)
        {
            return tier switch { JobTier.Beginner => "#FFFFFF", JobTier.Intermediate => "#00BFFF", JobTier.Advanced => "#00FA9A", JobTier.Special => "#FFD700", _ => "#A9A9A9" };
        }

        private string GetTierString(JobTier tier)
        {
            return tier switch { JobTier.Beginner => "초급", JobTier.Intermediate => "중급", JobTier.Advanced => "상급", JobTier.Special => "특수", _ => "알 수 없음" };
        }

        private string GetCategoryString(JobCategory cat)
        {
            return cat switch { JobCategory.Menial => "잡일", JobCategory.Gathering => "자원 채집", JobCategory.Crafting => "제작/납품", JobCategory.Delivery => "물류/호위", JobCategory.EcoHunting => "생태 정화", JobCategory.DungeonHunting => "던전 토벌", JobCategory.BlackMarket => "암시장", _ => "기타" };
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Viewer == null || m_Viewer.Deleted) return;

            if (info.ButtonID >= 100)
            {
                int idx = info.ButtonID - 100;
                if (idx >= 0 && idx < PartTimeManager.ActiveRequests.Count)
                {
                    if (m_Profile.AvailableCharges <= 0)
                    {
                        m_Viewer.SendMessage(0x22, "오늘 수행할 수 있는 업무 횟수를 모두 소모했습니다.");
                        return;
                    }

                    if (QuestHelper.HasQuest(m_Viewer, typeof(PartTimeQuest)))
                    {
                        m_Viewer.SendMessage(0x22, "이미 진행 중인 파트타임 업무가 있습니다.");
                        return;
                    }

                    TownJobRequest req = PartTimeManager.ActiveRequests[idx];
                    
                    if (req.IsFullyBooked)
                    {
                        m_Viewer.SendMessage(0x22, "방금 전 다른 누군가가 이 의뢰를 수락했습니다.");
                        return;
                    }

                    PartTimeQuest quest = new PartTimeQuest(new PartTimeJob(req));
                    quest.Owner = m_Viewer;

                    if (QuestHelper.CanOffer(m_Viewer, quest, true))
                        m_Viewer.SendGump(new MondainQuestGump(quest));
                }
            }
        }
    }
}