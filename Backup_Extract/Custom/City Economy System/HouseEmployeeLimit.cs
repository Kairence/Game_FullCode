using System;
using Server;
using Server.Multis;
using Server.Mobiles;
using System.Linq;

namespace Server.Misc
{
    public static class HouseEmployeeLimit
    {
        // 원장님이 기획하신 직업당 최대 고용 한도
        public const int MaxEmployeesPerJob = 3;

        // 원장님이 수동으로 지정하신 클래식 하우스 최대 고용 한도 (감성 밸런스)
        public static int GetMaxEmployeeLimit(this BaseHouse house)
        {
            if (house == null) return 0;

            // 1. 커스텀 하우스 (맞춤 집) - 가로/세로 중 짧은 길이 적용 (최대 24명)
            if (house is HouseFoundation)
            {
                int width = house.Region.Area.Select(r => r.Width).Max();
                int height = house.Region.Area.Select(r => r.Height).Max();
                int limit = Math.Min(width, height);
                return Math.Min(limit, 24); // 최대 캐슬 사이즈(24)로 캡
            }

            // 2. 클래식 하우스 - 수동 지정 테이블
            Type type = house.GetType();
            
            if (type == typeof(SmallOldHouse)) return 6;
            if (type == typeof(SmallShop)) return 9;
            if (type == typeof(TwoStoryVilla)) return 9;
            if (type == typeof(LogCabin)) return 9;
            if (type == typeof(SmallTower)) return 12;
            if (type == typeof(TwoStoryHouse)) return 12;
            if (type == typeof(SandStonePatio)) return 14;
            if (type == typeof(GuildHouse)) return 15;
            if (type == typeof(Tower)) return 16;
            if (type == typeof(LargePatioHouse)) return 17;
            if (type == typeof(Keep)) return 18;
            if (type == typeof(LargeMarbleHouse)) return 20;
            if (type == typeof(Castle)) return 24;

            // 기타 클래식 집의 경우 예외 처리
            return Math.Min(24, Math.Max(3, (int)Math.Ceiling(house.GetNewVendorSystemMaxVendors() / 5.0)));
        }
    }
}
