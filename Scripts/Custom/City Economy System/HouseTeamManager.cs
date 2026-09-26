using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Multis;
using Server.Mobiles;

namespace Server.Misc
{
    // 모든 주택 고용 NPC가 공통으로 상속받을 인터페이스
    public interface IHouseEmployee
    {
        Guid EmployeeID { get; }
        NpcJobClass JobClass { get; }
        NpcRank EmployeeRank { get; }
        Mobile EmployeeMobile { get; }
        int DailyWage { get; }
    }

    public static class HouseTeamManager
    {
        // 특정 집에 있는 특정 부서(JobClass)의 팀원 목록을 등급이 높은 순으로 가져옴
        public static List<IHouseEmployee> GetDepartmentTeam(BaseHouse house, NpcJobClass jobClass)
        {
            var list = new List<IHouseEmployee>();
            if (house == null) return list;

            foreach (var m in house.Region.GetMobiles())
            {
                if (m is IHouseEmployee emp && emp.EmployeeID != Guid.Empty && emp.JobClass == jobClass)
                {
                    list.Add(emp);
                }
            }
            // 랭크(등급)가 높은 순서대로 정렬 -> 0번 인덱스가 무조건 '팀장'
            return list.OrderByDescending(e => e.EmployeeRank).ToList();
        }

        // 해당 NPC가 발휘할 수 있는 능력치 배율을 리턴 (팀장: 100%, 팀원: 40%)
        public static double GetEmployeeEfficiencyMultiplier(IHouseEmployee employee)
        {
            if (employee.EmployeeMobile == null) return 0.0;
            
            BaseHouse house = BaseHouse.FindHouseAt(employee.EmployeeMobile);
            if (house == null) return 0.0;

            var team = GetDepartmentTeam(house, employee.JobClass);
            int index = team.FindIndex(e => e.EmployeeID == employee.EmployeeID);

            if (index == 0) return 1.0; // 팀장 (100%)
            if (index == 1 || index == 2) return 0.4; // 팀원 (40%)
            
            return 0.0; // 3명 초과 버그 방지
        }

        // 특정 부서가 해당 집에서 발휘하는 총 합산 효율 (최대 1.8배 = 180%)
        public static double GetTotalDepartmentEfficiency(BaseHouse house, NpcJobClass jobClass)
        {
            var team = GetDepartmentTeam(house, jobClass);
            double total = 0.0;
            for (int i = 0; i < Math.Min(team.Count, 3); i++)
            {
                if (i == 0) total += 1.0;
                else total += 0.4;
            }
            return total;
        }
        
        // 직급에 맞는 호칭 반환 (예: "RetailMerchant 팀장")
                // 제작 스킬에 맞는 부서(JobClass)를 매핑하고 해당 부서의 총 효율을 반환
                // 집 객체(Serial)별 커스텀 가격 저장소 (Key: HouseSerial, Value: Dictionary<SkillName, CustomPrice>)
        public static Dictionary<Serial, Dictionary<SkillName, int>> CustomPrices = new();

        public static int GetBasePriceFor100Charges(BaseHouse house, SkillName skill)
        {
            if (house == null) return 100;
            List<NpcJobClass> matchingJobs = new List<NpcJobClass>();
            switch (skill)
            {
                case SkillName.Blacksmith: matchingJobs.Add(NpcJobClass.Blacksmith); break;
                case SkillName.Tailoring: matchingJobs.Add(NpcJobClass.Tailor); break;
                case SkillName.Carpentry: matchingJobs.Add(NpcJobClass.Carpenter_Producer); break;
                case SkillName.Tinkering: matchingJobs.Add(NpcJobClass.Tinker); break;
                case SkillName.Alchemy: matchingJobs.Add(NpcJobClass.Alchemist); matchingJobs.Add(NpcJobClass.PotionMaker); break;
                case SkillName.Cooking: matchingJobs.Add(NpcJobClass.Cook_Entertainer); matchingJobs.Add(NpcJobClass.PizzaChef_Producer); break;
                case SkillName.Inscribe: matchingJobs.Add(NpcJobClass.Scribe_Mage); matchingJobs.Add(NpcJobClass.Scribe_Scholar); break;
                case SkillName.Fletching: matchingJobs.Add(NpcJobClass.Bowyer); break;
            }

            int totalWage = 0;
            foreach (var job in matchingJobs)
            {
                var team = GetDepartmentTeam(house, job);
                foreach (var emp in team)
                {
                    totalWage += emp.DailyWage;
                }
            }
            
            int basePrice = totalWage / 10; // (총 일당 / 1000) * 100회
            return Math.Max(100, basePrice);
        }

        public static int GetActualPriceFor100Charges(BaseHouse house, SkillName skill)
        {
            if (house == null) return 100;
            
            int basePrice = GetBasePriceFor100Charges(house, skill);
            
            if (CustomPrices.TryGetValue(house.Serial, out var prices) && prices.TryGetValue(skill, out int customPrice))
            {
                // 상하한선 검증 (-50% ~ +400%)
                int minPrice = (int)(basePrice * 0.5);
                int maxPrice = (int)(basePrice * 5.0); // +400% means 500% total
                
                if (customPrice < minPrice) customPrice = minPrice;
                if (customPrice > maxPrice) customPrice = maxPrice;
                return customPrice;
            }
            return basePrice;
        }

        public static void SetCustomPrice(BaseHouse house, SkillName skill, int price)
        {
            if (house == null) return;
            if (!CustomPrices.ContainsKey(house.Serial))
                CustomPrices[house.Serial] = new Dictionary<SkillName, int>();
                
            CustomPrices[house.Serial][skill] = price;
        }

                // ==============================================================================
        // AI 스캔용 청크 기반 탐색 유틸리티
        // ==============================================================================
        
        // 특정 청크(128x128) 내에 있는 최고 효율의 유저 선술집(여관/바드)을 찾음
        public static BaseHouse FindBestPlayerTavernInChunk(Map map, Point3D loc)
        {
            if (map == null || map == Map.Internal) return null;

            int cx = loc.X / 128;
            int cy = loc.Y / 128;
            BaseHouse bestHouse = null;
            double highestSynergy = 0.0;

            // 주의: 서버의 모든 집을 순회하되, 청크 필터로 즉시 컷오프 (O(1)급 속도)
            foreach (var item in Server.World.Items.Values)
            {
                if (item is BaseHouse house && house.Public && house.Map == map)
                {
                    int hcx = house.Location.X / 128;
                    int hcy = house.Location.Y / 128;
                    // 마을 + 주변 8방향 근교 청크 (3x3) 포함 스캔
                    if (Math.Abs(hcx - cx) <= 1 && Math.Abs(hcy - cy) <= 1)
                    {
                        double bardSynergy = GetTotalDepartmentEfficiency(house, NpcJobClass.Bard);
                        double innSynergy = GetTotalDepartmentEfficiency(house, NpcJobClass.InnKeeper);
                        double total = bardSynergy + innSynergy;

                        if (total > highestSynergy)
                        {
                            highestSynergy = total;
                            bestHouse = house;
                        }
                    }
                }
            }
            return bestHouse;
        }

        // 특정 청크 내에서 필요한 아이템을 팔고 있는 유저 상점(RetailVendor) 탐색
        public static Mobile FindPlayerShopWithItemInChunk(Map map, Point3D loc, Type itemType)
        {
            if (map == null || map == Map.Internal || itemType == null) return null;

            int cx = loc.X / 128;
            int cy = loc.Y / 128;

            foreach (var m in Server.World.Mobiles.Values)
            {
                if (m is Server.Mobiles.RetailVendor vendor && vendor.Map == map)
                {
                    if ((vendor.Location.X / 128) == cx && (vendor.Location.Y / 128) == cy)
                    {
                        // 판매 목록에 해당 아이템이 있는지 검사
                        foreach (var mi in vendor.MarketItems)
                        {
                            if (mi.RealItem != null && !mi.RealItem.Deleted && mi.RealItem.GetType() == itemType)
                            {
                                return vendor;
                            }
                        }
                    }
                }
            }
            return null;
        }


        public static double GetCraftingSynergy(BaseHouse house, SkillName skill)
        {
            if (house == null) return 0.0;

            List<NpcJobClass> matchingJobs = new List<NpcJobClass>();
            switch (skill)
            {
                case SkillName.Blacksmith: matchingJobs.Add(NpcJobClass.Blacksmith); break;
                case SkillName.Tailoring: matchingJobs.Add(NpcJobClass.Tailor); break;
                case SkillName.Carpentry: matchingJobs.Add(NpcJobClass.Carpenter_Producer); break;
                case SkillName.Tinkering: matchingJobs.Add(NpcJobClass.Tinker); break;
                case SkillName.Alchemy: 
                    matchingJobs.Add(NpcJobClass.Alchemist); 
                    matchingJobs.Add(NpcJobClass.PotionMaker); 
                    break;
                case SkillName.Cooking: 
                    matchingJobs.Add(NpcJobClass.Cook_Entertainer); 
                    matchingJobs.Add(NpcJobClass.PizzaChef_Producer); 
                    break;
                case SkillName.Inscribe: 
                    matchingJobs.Add(NpcJobClass.Scribe_Mage); 
                    matchingJobs.Add(NpcJobClass.Scribe_Scholar); 
                    break;
                case SkillName.Fletching: matchingJobs.Add(NpcJobClass.Bowyer); break;
            }

            double highestSynergy = 0.0;
            foreach (var job in matchingJobs)
            {
                double syn = GetTotalDepartmentEfficiency(house, job);
                if (syn > highestSynergy) highestSynergy = syn;
            }

            return highestSynergy;
        }
        public static string GetJobTitle(IHouseEmployee employee)
        {
            if (employee.EmployeeMobile == null) return "직원";
            
            BaseHouse house = BaseHouse.FindHouseAt(employee.EmployeeMobile);
            if (house == null) return "직원";

            var team = GetDepartmentTeam(house, employee.JobClass);
            int index = team.FindIndex(e => e.EmployeeID == employee.EmployeeID);

            // 임시로 영문명 사용. 이후 Localize 연동 가능.
            string jobName = employee.JobClass.ToString(); 
            
            if (index == 0) return $"{jobName} 팀장";
            return $"{jobName} 팀원";
        }
    }
}






