using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

namespace Server.Misc
{
    public class HouseEmployeeScheduleGump : Gump
    {
        private Mobile m_From;
        private VirtualCitizen m_Citizen;

        public HouseEmployeeScheduleGump(Mobile from, VirtualCitizen citizen) : base(50, 50)
        {
            m_From = from;
            m_Citizen = citizen;

            AddPage(0);
            AddBackground(0, 0, 500, 480, 9270);
            AddAlphaRegion(10, 10, 480, 460);

            AddHtml(0, 20, 500, 20, $"<CENTER><BASEFONT COLOR=#FDB913>가상 시민 24시간 스케줄 관리: {citizen.Name}</BASEFONT></CENTER>", false, false);
            AddHtml(20, 50, 460, 40, "<BASEFONT COLOR=#CCCCCC>각 시간대별로 시민이 수행할 업무를 지정하세요. 설정된 시간에 맞춰 해당 시설(농장, 광산 등)로 출근하여 일합니다.</BASEFONT>", false, false);

            if (citizen.Schedule == null) citizen.Schedule = new CitizenScheduleProfile();

            // 0~11시 (왼쪽)
            for (int i = 0; i < 12; i++)
            {
                int y = 100 + (i * 25);
                AddLabel(30, y, 1152, $"{i:D2}:00 ~ {i+1:D2}:00");
                AddButton(130, y + 2, 4005, 4007, 100 + i, GumpButtonType.Reply, 0); // 상태 변경 버튼
                AddLabel(165, y, GetColor(citizen.Schedule.Hours[i]), GetTypeName(citizen.Schedule.Hours[i]));
            }

            // 12~23시 (오른쪽)
            for (int i = 12; i < 24; i++)
            {
                int y = 100 + ((i - 12) * 25);
                AddLabel(270, y, 1152, $"{i:D2}:00 ~ {i+1:D2}:00");
                AddButton(370, y + 2, 4005, 4007, 100 + i, GumpButtonType.Reply, 0); // 상태 변경 버튼
                AddLabel(405, y, GetColor(citizen.Schedule.Hours[i]), GetTypeName(citizen.Schedule.Hours[i]));
            }

            AddButton(210, 430, 247, 248, 1, GumpButtonType.Reply, 0); // 확인/저장
        }

        private string GetTypeName(SimulationSystemType type)
        {
            switch (type)
            {
                case SimulationSystemType.None: return "휴식 / 자유시간";
                case SimulationSystemType.Farming: return "농장 노동 (Farming)";
                case SimulationSystemType.Fishing: return "양식장 관리 (Fishing)";
                case SimulationSystemType.Mining: return "광산 채굴 (Mining)";
                case SimulationSystemType.Lumberjacking: return "벌목장 관리 (Lumber)";
                case SimulationSystemType.Husbandry: return "목축장 관리 (Husbandry)";
                default: return "알 수 없음";
            }
        }

        private int GetColor(SimulationSystemType type)
        {
            switch (type)
            {
                case SimulationSystemType.None: return 999;     // 회색
                case SimulationSystemType.Farming: return 68;   // 녹색
                case SimulationSystemType.Fishing: return 92;   // 파란색
                case SimulationSystemType.Mining: return 33;    // 갈색/빨강
                case SimulationSystemType.Lumberjacking: return 43; // 진녹색
                case SimulationSystemType.Husbandry: return 53; // 노란색
                default: return 999;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1 || info.ButtonID == 0) // 확인 또는 우클릭 종료
            {
                m_From.SendMessage(68, $"{m_Citizen.Name}의 스케줄이 저장되었습니다.");
                // 필요 시 다시 메인 직원 관리창 띄우기
                return;
            }

            if (info.ButtonID >= 100 && info.ButtonID < 124)
            {
                int hour = info.ButtonID - 100;
                
                // 현재 타입에서 다음 타입으로 순환 (0 -> 1 -> 2 -> 3 -> 4 -> 5 -> 0)
                int currentType = (int)m_Citizen.Schedule.Hours[hour];
                currentType++;
                if (currentType > 5) currentType = 0; // SimulationSystemType.Husbandry 가 마지막

                m_Citizen.Schedule.Hours[hour] = (SimulationSystemType)currentType;

                // UI 리프레시
                m_From.SendGump(new HouseEmployeeScheduleGump(m_From, m_Citizen));
            }
        }
    }
}
