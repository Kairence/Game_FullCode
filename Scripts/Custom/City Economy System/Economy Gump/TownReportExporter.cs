using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Server;

namespace Server.Misc
{
    public static class GlobalEconomyReport
    {
        private static string SavePath = "Data/Reports/GlobalEconomy/";

        public static void GenerateMasterReport(List<TownEconomy> allTowns)
        {
            if (allTowns == null || allTowns.Count == 0) return;

            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            string fileName = $"Global_Economy_Master_{DateTime.Now:yyyyMMdd_HHmm}.md";
            StringBuilder sb = new StringBuilder();

            // 1. 대륙 전체 요약 (Macro Statistics)
            long totalWealth = allTowns.Sum(t => t.Wealth);
            int totalPop = allTowns.Sum(t => t.Citizens.Count);

            sb.AppendLine("# 🌍 Kairence UO: 전 대륙 통합 경제 지표 보고서");
            sb.AppendLine($"**분석 시점**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"**총 통화량**: {totalWealth / 100000000.0:F2} Platinum");
            sb.AppendLine($"**총 거주 인구**: {totalPop:N0} 명");
            sb.AppendLine($"**활성화 도시**: {allTowns.Count} 개소");
            sb.AppendLine("\n---");

            // 2. 마을별 핵심 지표 비교 (EconomyAdminGump 데이터 기반)
            sb.AppendLine("## 🏛️ 마을별 경제 현황 요약");
            sb.AppendLine("| 마을 이름 | 등급 | 재정 (Plat) | 인구 | 주요 생산직 | 만족도 |");
            sb.AppendLine("| :--- | :---: | :---: | :---: | :---: | :---: |");

            foreach (var town in allTowns)
            {
                double plat = town.Wealth / 100000000.0;
                string topJob = town.JobBirthWeights.OrderByDescending(x => x.Value).FirstOrDefault().Key.ToString();
                
                sb.AppendLine($"| {town.TownName} | {town.TownIndex} | {plat:F2}P | {town.Citizens.Count} | {topJob} | {GetSatisfactionStars(town)} |");
            }

            // 3. 글로벌 자원 모니터링 (Crisis Watch)
            sb.AppendLine("\n## ⚠️ 대륙 자원 위기 경보 (Crisis Watch)");
            sb.AppendLine("각 마을별로 재고가 10% 미만으로 떨어진 핵심 자원들입니다.");
            sb.AppendLine("| 마을 | 부족 자원 | 현재 수량 | 긴급도 |");
            sb.AppendLine("| :--- | :--- | :---: | :---: |");

            foreach (var town in allTowns)
            {
                foreach (var res in town.Warehouse)
                {
                    if (res.Value.Stock < 50) // 임계치 이하
                    {
                        sb.AppendLine($"| {town.TownName} | {res.Key} | {res.Value} | 🚨 **위험** |");
                    }
                }
            }

            // 4. Platinum 경제 집중도 분석
            var richest = allTowns.OrderByDescending(t => t.Wealth).FirstOrDefault();
            sb.AppendLine($"\n> **💡 경제 분석가 주석**: 현재 대륙의 부는 **{richest.TownName}**에 집중되어 있습니다. " +
                          $"자원 불균형 해소를 위해 {richest.TownName}의 잉여 자본을 타 마을로 유도하는 무역 로직이 권장됩니다.");

            File.WriteAllText(Path.Combine(SavePath, fileName), sb.ToString());
            Console.WriteLine($"[Global Report] 전 대륙 통합 보고서 생성 완료: {fileName}");
        }

        private static string GetSatisfactionStars(TownEconomy town)
        {
            // 평균 만족도 기반 별점 표시
            double avg = town.Citizens.Average(c => c.Satisfaction);
            if (avg > 80) return "⭐⭐⭐⭐⭐";
            if (avg > 60) return "⭐⭐⭐⭐";
            if (avg > 40) return "⭐⭐⭐";
            return "⭐⭐";
        }
    }
}
