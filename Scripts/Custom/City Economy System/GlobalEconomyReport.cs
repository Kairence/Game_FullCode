using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Server;
using Server.Regions;

namespace Server.Misc
{
    public static class TownEconomyExporter
    {
        public static void Initialize()
        {
            // [Command] [ExportEconomy] 명령어로 리포트 수동 생성
            Server.Commands.CommandSystem.Register("ExportEconomy", AccessLevel.Administrator, e =>
            {
                ManualExport();
                e.Mobile.SendMessage(0x48, "경제 시스템 분석 보고서가 생성되었습니다. (Data/EconomySystem/Reports)");
            });
        }

        public static void ManualExport()
        {
            string rootPath = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "Reports");
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
            
            string ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string fullPath = Path.Combine(rootPath, $"Economy_Report_{ts}.md");

            // 리포트 생성 전 모든 마을의 지표 최신화 및 속성 파악
            foreach (var town in TownEconomyManager.Towns.Values)
            {
                AnalyzeRegionData(town); // 공식 마을 여부 판정
                town.UpdateBaseWealth(); // 최신 면적/상인수 기반 자본금 갱신
            }

            ExportToMarkdown(fullPath);
        }

        private static void ExportToMarkdown(string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine("# 📊 Kairence UO: 경제 시스템 통합 분석 보고서");
                sw.WriteLine($"> 생성 일시: {DateTime.Now}");
                sw.WriteLine($"> 분석 대상: {TownEconomyManager.Towns.Count}개 지역\n");

                // --- 섹션 1: 마을 규모 요약 ---
                sw.WriteLine("## 🏙️ 마을별 행정 규모 및 자본 현황");
                sw.WriteLine("| 마을 이름 | 등급 | 상인수 | 면적(Tiles) | 현재 자본금 (Total Wealth) | 물가 배율 |");
                sw.WriteLine("| :--- | :---: | :---: | :---: | :--- | :---: |");

                // 자본금(Wealth)이 많은 순서대로 정렬
                var sortedTowns = TownEconomyManager.Towns.Values.OrderByDescending(t => t.Wealth).ToList();

                foreach (var town in sortedTowns)
                {
                    // TotalTiles는 TownNumber에서 실시간으로 계산된 값이 출력됩니다.
                    sw.WriteLine($"| {town.TownName} | {town.TownIndex} | {town.VendorCount}명 | {town.TotalTiles:N0} | **{town.TotalWealthString}** | {town.PriceMultiplier:F2}x |");
                }

                sw.WriteLine("\n---\n## 📦 마을별 창고 재고 리포트 (Carrier AI 참조용)");
                
                foreach (var town in sortedTowns)
                {
                    sw.WriteLine($"### 🏘️ {town.TownName} ({town.TownID}) 재고 현황");
                    sw.WriteLine("| 품목 타입 | 현재 재고 (Stock) | 기본가 (Base) | 실시간 매입가 | 상태 |");
                    sw.WriteLine("| :--- | :---: | :---: | :---: | :--- |");

                    if (town.Warehouse != null && town.Warehouse.Count > 0)
                    {
                        var items = town.Warehouse.Values.OrderBy(i => i.ItemType.Name);
                        foreach (var item in items)
                        {
                            // 현재 물가 배율이 적용된 실제 거래가
                            int estPrice = town.GetPrice(item.ItemType, 1.0);
                            string status = GetStockStatus(item.Stock);

                            sw.WriteLine($"| {item.ItemType.Name} | **{item.Stock:N0}** | {item.BasePrice}g | {estPrice}g | {status} |");
                        }
                    }
                    else
                    {
                        sw.WriteLine("| - | 창고 비어있음 | - | - | - |");
                    }
                    sw.WriteLine("\n");
                }
            }
        }

        private static string GetStockStatus(int stock)
        {
            // 재고 수준에 따른 직관적인 상태 메시지
            if (stock <= 50) return "🔴 부족 (Critical)";
            if (stock <= 200) return "🟠 낮음";
            if (stock >= 1500) return "🟢 과잉 (Surplus)";
            return "⚪ 정상";
        }

        private static void AnalyzeRegionData(TownEconomy town)
        {
            bool isTown = false;

            // 해당 이름의 지역이 시스템 상에 '마을(Town)'으로 정의되어 있는지 확인
            foreach (Region r in Region.Regions)
            {
                if (string.Equals(r.Name, town.TownName, StringComparison.OrdinalIgnoreCase))
                {
                    if (r is TownRegion || r.GetType().Name.Contains("Town"))
                    {
                        isTown = true;
                        break;
                    }
                }
            }
            
            // 공식 마을 여부만 업데이트 (TotalTiles는 TownNumber에서 계산하므로 대입하지 않음)
            town.IsOfficialTown = isTown;
        }
    }
}