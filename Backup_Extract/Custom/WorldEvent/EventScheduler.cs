using System;
using System.Collections.Generic;
using Server;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public enum HolidayType
    {
        None,
        NewYear,        // 신정 (태양의 축제)
        Seolnal,        // 설날 (달의 축제)
        Samil,          // 삼일절 (독립의 함성)
        Sikmok,         // 식목일 (세계수의 날)
        BuddhasBirthday,// 석가탄신일 (자비의 등불)
        ChildrensDay,   // 어린이날 (새싹의 날)
        ParentsDay,     // 어버이날 (은혜의 날)
        Hyeonchung,     // 현충일 (추모의 날)
        Jeheon,         // 제헌절 (법치의 날)
        Gwangbok,       // 광복절 (해방의 날)
        Chuseok,        // 추석 (대수확제)
        WifeBirthday,   // 아내 생일 (최고 권력자의 날)
        Gaecheon,       // 개천절 (하늘이 열린 날)
        Hangeul,        // 한글날 (문화의 날)
        Christmas       // 성탄절 (율 축제)
    }

    public static class EventScheduler
    {
        // 일반 상자용 공통 축제 색상 (팔레트 내 밝고 화사한 색상들)
        public static readonly int[] CommonFestiveHues = new int[] { 1154, 1281, 1282, 1301, 1161 };

        // 🌟 기념일별 데이터 (색상 및 명칭) 통합 관리
        public static (int RareHue, string Name) GetHolidayData(HolidayType type)
        {
            return type switch
            {
                HolidayType.NewYear         => (1160, "태양의 축제"),      // fire (강렬한 오렌지)
                HolidayType.Seolnal         => (1150, "달의 축제"),        // ice_hue_4 (영롱한 달빛)
                HolidayType.Samil           => (1156, "독립의 함성"),      // dark red (투쟁의 붉은색)
                HolidayType.Sikmok          => (1166, "세계수의 날"),      // Greenlight (생명력의 연두)
                HolidayType.BuddhasBirthday => (1259, "자비의 등불"),      // Goldlight (자비로운 황금빛)
                HolidayType.ChildrensDay    => (1159, "새싹의 날"),        // Purplelight (발랄한 연보라)
                HolidayType.ParentsDay      => (1167, "은혜의 날"),        // Pink/Purple (카네이션 느낌)
                HolidayType.Hyeonchung      => (1153, "추모의 날"),        // Silver/Blue (경건한 은빛)
                HolidayType.Jeheon          => (1282, "법치의 날"),        // Cyan (공적인 신뢰감)
                HolidayType.Gwangbok        => (1281, "해방의 날"),        // Deep Blue (태극기의 감색)
                HolidayType.Chuseok         => (1259, "대수확제"),        // Brown/Gold (익은 곡식의 금색)
                HolidayType.WifeBirthday    => (1165, "최고 권력자의 날"),  // Firelight (화려한 핑크레드)
                HolidayType.Gaecheon        => (1500, "하늘이 열린 날"),    // Orange (밝은 하늘의 서광)
                HolidayType.Hangeul         => (1176, "문화의 날"),        // Soft Gold (고서적/종이의 느낌)
                HolidayType.Christmas       => (1152, "율 축제"),          // Ice White (순백의 눈)
                _                           => (1169, "기념일")
            };
        }

        public static void ProcessHourlyTick()
        {
            AccumulateHourlyVictoryPoints();
        }

        public static void ProcessDailyTick()
        {
        }

        public static bool CheckAndClaimHoliday(Account acc, string holidayName)
        {
            if (acc == null || string.IsNullOrEmpty(holidayName))
                return false;

            string claimKey = acc.Username + "_" + holidayName;
            int currentYear = DateTime.Now.Year;

            if (Server.Event.HolidayClaims.ContainsKey(claimKey))
            {
                if (Server.Event.HolidayClaims[claimKey] >= currentYear)
                    return false;

                Server.Event.HolidayClaims[claimKey] = currentYear;
            }
            else
            {
                Server.Event.HolidayClaims.Add(claimKey, currentYear);
            }

            return true;
        }

        public static HolidayType GetCurrentHoliday()
        {
            DateTime now = DateTime.Now;
            int m = now.Month;
            int d = now.Day;

            // 1. 양력 기념일 판정
            if (m == 1 && d <= 7) return HolidayType.NewYear;
            if (m == 3 && d <= 7) return HolidayType.Samil;
            if (m == 4 && d >= 1 && d <= 7) return HolidayType.Sikmok;
            if (m == 5 && d <= 7) return HolidayType.ChildrensDay;
            if (m == 5 && d >= 8 && d <= 15) return HolidayType.ParentsDay;
            if (m == 6 && d <= 7) return HolidayType.Hyeonchung;
            if (m == 7 && d >= 15 && d <= 21) return HolidayType.Jeheon;
            if (m == 8 && d >= 13 && d <= 19) return HolidayType.Gwangbok;
            if (m == 10 && d <= 3) return HolidayType.Gaecheon;
            if (m == 10 && d >= 7 && d <= 13) return HolidayType.Hangeul;
            if (m == 12 && d >= 20 && d <= 31) return HolidayType.Christmas;

            // 2. 음력 기념일 판정
            if (LunarHelper.IsLunarRange(now, 1, 1, 5)) return HolidayType.Seolnal;
            if (LunarHelper.IsLunarRange(now, 4, 8, 5)) return HolidayType.BuddhasBirthday;
            if (LunarHelper.IsLunarRange(now, 8, 15, 5)) return HolidayType.Chuseok;
            if (LunarHelper.IsLunarRange(now, 9, 26, 5)) return HolidayType.WifeBirthday;

            return HolidayType.None;
        }

        private static void AccumulateHourlyVictoryPoints()
        {
            DonationCheck dc = Server.Event.dc;
            if (dc == null || DateTime.Now >= dc.RespawnTime) return;

            DayOfWeek today = DateTime.Now.DayOfWeek;
            bool isWeekend = (today == DayOfWeek.Saturday || today == DayOfWeek.Sunday);

            for (int cat = 0; cat < 4; cat++)
            {
                for (int rank = 0; rank < 100; rank++)
                {
                    string name = dc.RankingNames[cat][rank];
                    if (string.IsNullOrEmpty(name)) continue;

                    int baseVp = 0;
                    if (rank == 0) baseVp = 100;
                    else if (rank == 1) baseVp = 80;
                    else if (rank == 2) baseVp = 60;
                    else if (rank < 10) baseVp = 40;
                    else if (rank < 30) baseVp = 30;
                    else if (rank < 50) baseVp = 20;
                    else if (rank < 100) baseVp = 10;
                    else baseVp = 4;

                    int finalVp = isWeekend ? (baseVp * 25) / 10 : baseVp;

                    if (Server.Event.WeeklyVP[cat].ContainsKey(name))
                        Server.Event.WeeklyVP[cat][name] += finalVp;
                    else
                        Server.Event.WeeklyVP[cat].Add(name, finalVp);
                }
            }
        }
    }
}