using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Accounting;
using Server.Misc;

namespace Server.Misc
{
    public class FamilyVirtueGump : Gump
    {
        private PlayerMobile m_Viewer;
        private int m_Page;
        private int m_SelectedNode;

        private static readonly string[] VirtueNames = new string[]
        {
            "정직 (Honesty)", "연민 (Compassion)", "용맹 (Valor)", "정의 (Justice)",
            "희생 (Sacrifice)", "명예 (Honor)", "영성 (Spirituality)", "겸손 (Humility)"
        };

        private class NodeUI
        {
            public int X, Y;
            public int[] Children;
            public NodeUI(int x, int y, params int[] children) { X = x; Y = y; Children = children; }
        }

        private static readonly NodeUI[] m_Grid = new NodeUI[]
        {
            null,
            new NodeUI(220, 130, 4, 5), new NodeUI(310, 130, 6), new NodeUI(400, 130, 7, 8),
            new NodeUI(130, 200, 9), new NodeUI(220, 200, 10), new NodeUI(310, 200, 10, 11, 12), new NodeUI(400, 200, 12), new NodeUI(490, 200, 13),
            new NodeUI(130, 270, 14), new NodeUI(220, 270, 15), new NodeUI(310, 270, 16), new NodeUI(400, 270, 17), new NodeUI(490, 270, 18),
            new NodeUI(130, 340, 19), new NodeUI(220, 340, 19), new NodeUI(310, 340, 20), new NodeUI(400, 340, 21), new NodeUI(490, 340, 21),
            new NodeUI(220, 410, 22), new NodeUI(310, 410, 23), new NodeUI(400, 410, 24),
            new NodeUI(220, 480, 25), new NodeUI(310, 480, 25), new NodeUI(400, 480, 25),
            new NodeUI(310, 560)
        };

        public FamilyVirtueGump(PlayerMobile viewer, int page, int selectedNode) : base(50, 50)
        {
            m_Viewer = viewer;
            m_Page = page;
            m_SelectedNode = selectedNode;

            if (viewer.Account is not Account acc) return;

            AddPage(0);
            AddBackground(0, 0, 900, 700, 9200);
            AddImageTiled(15, 15, 870, 670, 2624);
            AddAlphaRegion(15, 15, 870, 670);

            AddHtml(25, 25, 400, 25, "<BASEFONT COLOR=#FFFFFF SIZE=5>가문 미덕 스킬 관리 시스템</BASEFONT>", false, false);
            AddHtml(600, 25, 270, 25, String.Format("<BASEFONT COLOR=#FFD700 ALIGN=RIGHT>가문 명예: {0:#,0} Pt</BASEFONT>", acc.Point[0]), false, false);
            AddImageTiled(25, 60, 850, 2, 9107); 

            if (m_Page == 0) RenderMainMenu();
            else RenderSplitPanel(acc);
        }

        private void RenderMainMenu()
        {
            for (int i = 0; i < 8; i++)
            {
                int x = 70 + (i % 2) * 400;
                int y = 120 + (i / 2) * 120;
                AddButton(x, y, 4005, 4007, 101 + i, GumpButtonType.Reply, 0);
                AddHtml(x + 50, y + 5, 300, 40, String.Format("<BASEFONT COLOR=#FFFFFF SIZE=4>{0}</BASEFONT>", VirtueNames[i]), false, false);
            }
        }

        private void RenderSplitPanel(Account acc)
        {
            AddHtml(25, 75, 200, 25, String.Format("<BASEFONT COLOR=#00FF00 SIZE=4>{0} 트리</BASEFONT>", VirtueNames[m_Page - 1]), false, false);
            AddButton(25, 640, 4014, 4016, 1000, GumpButtonType.Reply, 0);
            AddHtml(65, 642, 100, 25, "<BASEFONT COLOR=#FFFFFF>뒤로가기</BASEFONT>", false, false);

            for (int i = 1; i <= 25; i++)
            {
                if (m_Grid[i]?.Children != null)
                {
                    foreach (int child in m_Grid[i].Children)
                        DrawPipe(m_Grid[i].X, m_Grid[i].Y, m_Grid[child].X, m_Grid[child].Y);
                }
            }

            int baseID = 400 + ((m_Page - 1) * 25);
            for (int i = 1; i <= 25; i++)
            {
                int skillID = baseID + i;
                FamilySkillNode node = FamilySkillManager.Skills[skillID];
                if (node == null || m_Grid[i] == null) continue;

                int lv = acc.Point[skillID];
                int x = m_Grid[i].X; int y = m_Grid[i].Y;
                bool isSelected = (m_SelectedNode == skillID);

                if (isSelected) AddImage(x - 15, y - 15, 2360); 

                if (lv >= node.MaxLevel) AddImage(x - 5, y - 5, 2361);
                else AddButton(x - 10, y - 10, 2474, 2475, skillID, GumpButtonType.Reply, 0); 

                AddHtml(x - 50, y + 15, 100, 20, String.Format("<BASEFONT COLOR=#FFFFFF><CENTER>Lv.{0}</CENTER></BASEFONT>", lv), false, false);
            }

            AddImageTiled(580, 70, 2, 600, 9105); 
            RenderDetailPanel(acc);
        }

        private void RenderDetailPanel(Account acc)
        {
            if (m_SelectedNode < 401 || m_SelectedNode > 600)
            {
                AddHtml(600, 100, 270, 100, "<BASEFONT COLOR=#888888>좌측 트리에서 스킬을 선택하면 상세 정보가 표시됩니다.</BASEFONT>", false, false);
                return;
            }

            FamilySkillNode node = FamilySkillManager.Skills[m_SelectedNode];
            int lv = acc.Point[m_SelectedNode];
            
            // 데이터 연산을 위한 인덱스 추출
            int virtueIdx = (m_SelectedNode - 401) / 25;
            int relID = (m_SelectedNode - 401) % 25 + 1;
            int currentScore = acc.Point[virtueIdx + 1];

            // 6티어 요구치 및 티어 계산
            int tier = GetTier(relID);
            int reqScore = GetRequiredScore(tier);
            bool virtueOk = currentScore >= reqScore;
            
            bool canUpgrade = FamilySystem.CanUpgrade(m_Viewer, m_SelectedNode, out string failReason);

            // 1. 스킬 타이틀 및 레벨 정보
            AddHtml(600, 80, 270, 30, String.Format("<BASEFONT COLOR=#00FFFF SIZE=5>{0}</BASEFONT>", node.Name), false, false);
            AddHtml(600, 115, 270, 20, String.Format("<BASEFONT COLOR=#FFFFFF>현재 레벨: {0} / 최대 {1} ({2}티어)</BASEFONT>", lv, node.MaxLevel, tier), false, false);

            // 2. 강화 효과 상세 (Cliloc 변수 치환 로직 포함)
            AddHtml(600, 160, 270, 25, "<BASEFONT COLOR=#FFD700>◈ 강화 효과 상세</BASEFONT>", false, false);
            string effectText = "";
            for (int i = 0; i < node.OptIDs.Length; i++)
            {
                int optID = node.OptIDs[i];
                int valPerLv = node.OptValuesPerLevel[i];
                double currentVal = (valPerLv * lv) / 10000.0;
                double nextVal = (valPerLv * (lv + 1)) / 10000.0;
                
                string optName = GetOptionName(optID); 
                string valString = "";
                
                if (lv == 0)
                    valString = String.Format("<BASEFONT COLOR=#888888>0</BASEFONT> → <BASEFONT COLOR=#00FF00>{0}</BASEFONT>", nextVal);
                else if (lv < node.MaxLevel)
                    valString = String.Format("{0} → <BASEFONT COLOR=#00FF00>{1}</BASEFONT>", currentVal, nextVal);
                else
                    valString = String.Format("<BASEFONT COLOR=#FFD700>{0} (마스터)</BASEFONT>", currentVal);

                if (optName.Contains("~1_val~") || optName.Contains("~1_VAL~"))
                {
                    string replaced = optName.Replace("~1_val~", valString).Replace("~1_VAL~", valString);
                    effectText += String.Format("• {0}<BR>", replaced);
                }
                else
                {
                    effectText += String.Format("• {0}: {1}<BR>", optName, valString);
                }
            }
            AddHtml(610, 185, 260, 110, String.Format("<BASEFONT COLOR=#BBBBBB>{0}</BASEFONT>", effectText), false, false);

            // 3. 연마 요구 조건 (미덕 자격 및 비용)
            AddHtml(600, 310, 270, 25, "<BASEFONT COLOR=#FFD700>◈ 연마 요구 조건</BASEFONT>", false, false);
            
            string vColor = virtueOk ? "#00FF00" : "#FF0000";
            string vStatus = virtueOk ? "자격 증명 완료" : String.Format("자격 부족 ({0}/{1})", currentScore, reqScore);
            string costText = lv >= node.MaxLevel ? "최대 레벨 도달" : String.Format("소모 비용: {0:#,0} Pt", FamilySystem.CalculateSkillCost(acc));

            AddHtml(610, 340, 260, 120, String.Format("<BASEFONT COLOR=#FFFFFF>{0}티어 요구 미덕 점수: </BASEFONT><BASEFONT COLOR={1}>{2}</BASEFONT><BR><BASEFONT COLOR=#FF0000>{3}</BASEFONT><BR><BASEFONT COLOR=#FFFFFF>{4}</BASEFONT>", 
                tier, vColor, vStatus, virtueOk ? failReason : "", costText), false, false);

            // 4. 연마 버튼 렌더링
            if (canUpgrade)
            {
                AddButton(600, 480, 247, 248, 9999, GumpButtonType.Reply, 0); 
                AddHtml(680, 485, 150, 25, "<BASEFONT COLOR=#FFFFFF SIZE=4>스킬 연마 시작</BASEFONT>", false, false);
            }
        }

        // --- 헬퍼 함수 구역 (최적화를 위해 단순 분기 처리) ---
        private int GetTier(int nodeNum)
        {
            if (nodeNum <= 3) return 1;
            if (nodeNum <= 8) return 2;
            if (nodeNum <= 13) return 3;
            if (nodeNum <= 18) return 4;
            if (nodeNum <= 24) return 5;
            return 6;
        }

        private int GetRequiredScore(int tier)
        {
            return tier switch { 1 => 50, 2 => 200, 3 => 450, 4 => 850, 5 => 1450, 6 => 2500, _ => 0 };
        }

        private string GetOptionName(int optID)
        {
            int clilocNumber = ItemOptionCreator.BaseCliloc + optID;
            string optionName = Misc.ClilocData.GetString(clilocNumber);
            return string.IsNullOrEmpty(optionName) ? "옵션 (" + optID + ")" : optionName;
        }

        private void DrawPipe(int x1, int y1, int x2, int y2)
        {
            y1 += 15; y2 -= 15;
            if (x1 == x2) AddImageTiled(x1, y1, 2, y2 - y1, 9105);
            else
            {
                int midY = y1 + (y2 - y1) / 2;
                AddImageTiled(x1, y1, 2, midY - y1, 9105);
                AddImageTiled(Math.Min(x1, x2), midY, Math.Abs(x1 - x2) + 2, 2, 9107);
                AddImageTiled(x2, midY, 2, y2 - midY, 9105);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Viewer == null || m_Viewer.Deleted) return;
            int bid = info.ButtonID;
            if (bid == 0) return;
            if (bid == 1000) { m_Viewer.SendGump(new FamilyVirtueGump(m_Viewer, 0, 0)); return; }
            if (bid >= 101 && bid <= 108) { m_Viewer.SendGump(new FamilyVirtueGump(m_Viewer, bid - 100, 0)); return; }
            
            if (bid >= 401 && bid <= 600)
            {
                m_Viewer.SendGump(new FamilyVirtueGump(m_Viewer, m_Page, bid));
                return;
            }

            if (bid == 9999 && m_SelectedNode != 0)
            {
                FamilySystem.UpgradeFamilySkill(m_Viewer, m_SelectedNode);
                m_Viewer.SendGump(new FamilyVirtueGump(m_Viewer, m_Page, m_SelectedNode));
            }
        }
    }
}