using System;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

namespace Server.Misc
{
    public class SkillAchievementGump : Gump
    {
        public SkillAchievementGump(Mobile from) : base(30, 50)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null) return;

            Closable = true; Dragable = true;
            AddPage(0);
            
            // 1. 전체 배경: 가로 1250으로 소폭 더 확장 (랭킹 버튼 공간 확보)
            AddBackground(0, 0, 1250, 820, 9270);
            
            AddImageTiled(20, 20, 1210, 40, 2624);
            
            // pm.Young 캐릭터이면서 보너스 기회가 있을 때 타이틀 표시
            string title = (pm.Young && pm.seasonSkillBonus > 0) 
                ? $"<CENTER>SEASON SKILL MASTERY (남은 선택 기회: {pm.seasonSkillBonus}회)</CENTER>" 
                : "<CENTER>SEASON SKILL MASTERY DASHBOARD</CENTER>";
            AddHtml(20, 30, 1210, 25, $"<BASEFONT SIZE=6 COLOR=#FFD700>{title}</BASEFONT>", false, false);

            RenderHeader(40, 65);
            RenderHeader(640, 65); // 두 번째 열 시작 위치 조정

            var skills = SkillInfo.Table;
            int totalSkills = 58; 
            int half = 29;        

            for (int i = 0; i < totalSkills; i++)
            {
                if (i >= skills.Length) break;

                int column = i / half; 
                int row = i % half;
                
                int x = 40 + (column * 600); // 열 간격 확장
                int y = 95 + (row * 23);

                // 슬롯 배경
                AddImageTiled(x, y, 580, 20, 9354);
                
                // --- [A] 스킬 상승 버튼 (Young 전용) ---
                if (pm.Young && pm.seasonSkillBonus > 0 && pm.Skills[i].Base < 50.0)
                {
                    // 0x15E1: 파란색 화살표 버튼 (상승)
                    AddButton(x + 2, y + 2, 0x15E1, 0x15E5, 100 + i, GumpButtonType.Reply, 0);
                }

                // --- [B] 스킬 정보 출력 ---
                string skillName = $"{i + 1}. {skills[i].Name}";
                AddLabel(x + 30, y, 1152, skillName);
                
                int baseColor = pm.Skills[i].Base >= 50.0 ? 0x42 : 0x481;
                AddLabel(x + 180, y, baseColor, pm.Skills[i].Base.ToString("F1"));

                string expStr = $"{((int)pm.SkillList[i]):N0} / {Misc.Util.SkillExp_Calc(pm, i):N0}";
                AddLabel(x + 240, y, 88, expStr);

                double pct = SkillPercent(pm, i);
                AddLabel(x + 420, y, GetPercentColor(pct), $"{pct:F1}%");
                
                // 진행바
                int maxBarWidth = 75; 
                int currentBarWidth = (int)(maxBarWidth * (pct / 100.0));
                AddImageTiled(x + 470, y + 5, maxBarWidth, 10, 0x13BE); 

                if (currentBarWidth > 0)
                {
                    if (currentBarWidth > maxBarWidth) currentBarWidth = maxBarWidth;
                    AddImageTiled(x + 470, y + 5, currentBarWidth, 10, 0x0805); 
                }

                // --- [C] 랭킹 확인 버튼 (전체 공용) ---
                // 0x15E3: 상세 보기 버튼 (랭킹 창 호출)
                AddButton(x + 555, y + 2, 0x15E3, 0x15E7, 200 + i, GumpButtonType.Reply, 0);
            }

            // 하단 상태바 및 메시지
            AddImageTiled(20, 775, 1210, 30, 2624);
            if (pm.Young && pm.seasonSkillBonus > 0)
                AddHtml(20, 782, 1210, 20, "<BASEFONT COLOR=#00FF00><CENTER>왼쪽 화살표: 50.0 즉시 상승(기회 차감) | 오른쪽 버튼: 실시간 스킬 랭킹 확인</CENTER></BASEFONT>", false, false);
            else
                AddHtml(20, 782, 1210, 20, "<BASEFONT COLOR=#00FF00><CENTER>시즌이 종료되기 전까지 랭킹 1위를 목표로 스킬을 연마하세요!</CENTER></BASEFONT>", false, false);
        }

        private void RenderHeader(int x, int y)
        {
            AddLabel(x + 30, y, 0x35, "SKILL NAME");
            AddLabel(x + 180, y, 0x35, "BASE");
            AddLabel(x + 240, y, 0x35, "EXPERIENCE (NOW / NEXT)");
            AddLabel(x + 440, y, 0x35, "PROGRESS");
            AddLabel(x + 550, y, 0x35, "RANK");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null) return;

            int buttonID = info.ButtonID;

            // 1. 스킬 상승 버튼 처리 (100 ~ 157)
            if (buttonID >= 100 && buttonID < 158)
            {
                int skillIndex = buttonID - 100;

                if (pm.Young && pm.seasonSkillBonus > 0)
                {
                    if (pm.Skills[skillIndex].Base < 50.0)
                    {
                        pm.Skills[skillIndex].Base = 50.0;
                        pm.seasonSkillBonus--;
                        pm.SendMessage(0x42, $"{SkillInfo.Table[skillIndex].Name} 스킬 숙련도가 50.0으로 보정되었습니다.");
                    }
                    pm.SendGump(new SkillAchievementGump(pm));
                }
                return;
            }

            // 2. 랭킹 확인 버튼 처리 (200 ~ 257)
			if (buttonID >= 200 && buttonID < 258)
			{
				int skillIndex = buttonID - 200;
				pm.SendGump(new SeasonRankingGump(pm, RankingType.Skill, skillIndex)); // 클래스명 변경됨
				return;
			}

            // 닫기 또는 메인 이동
            sender.Mobile.SendGump(new SeasonMainGump(sender.Mobile));
        }

        private int GetPercentColor(double pct)
        {
            if (pct >= 100) return 0x42; 
            if (pct >= 50) return 0x58;  
            return 1152; 
        }

        private double SkillPercent(PlayerMobile pm, int skill)
        {
            double targetExp = Misc.Util.SkillExp_Calc(pm, skill);
            if (targetExp <= 0) return 0;
            double pct = (pm.SkillList[skill] * 100.0) / targetExp;
            return pct > 100 ? 100 : pct;
        }
    }
}
