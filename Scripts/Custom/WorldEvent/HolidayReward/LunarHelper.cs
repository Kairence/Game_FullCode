using System;
using System.Globalization;

namespace Server.Misc
{
    public static class LunarHelper
    {
        private static readonly KoreanLunisolarCalendar m_Calendar = new KoreanLunisolarCalendar();

        // 현재 날짜가 특정 음력 날짜 범위에 있는지 확인
        public static bool IsLunarRange(DateTime now, int targetMonth, int targetDay, int rangeDays)
        {
            try
            {
                int lYear = m_Calendar.GetYear(now);
                int lMonth = m_Calendar.GetMonth(now);
                int lDay = m_Calendar.GetDayOfMonth(now);
                bool isLeap = m_Calendar.IsLeapMonth(lYear, lMonth);

                if (isLeap) return false; // 윤달은 제외

                // 명절 당일 기준으로 앞뒤 범위를 체크
                // 음력은 단순 수치 비교보다 당일 여부가 중요하므로 당일 기준 근접일 계산
                if (lMonth == targetMonth)
                {
                    int diff = lDay - targetDay;
                    return diff >= 0 && diff <= rangeDays;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}