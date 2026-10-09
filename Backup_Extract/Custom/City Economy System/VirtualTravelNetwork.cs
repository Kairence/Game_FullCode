using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;

namespace Server.Misc
{
    // 유저님 코드에서 요구하는 반환 구조체
    public struct TravelPlan
    {
        public bool IsPossible { get; set; }
        public int TotalCost { get; set; }
        public int TotalTicks { get; set; }
    }

    public static class VirtualTravelNetwork
    {
        // 메모리에 상주할 간선망 데이터
        // Key: "MapName_StartID_EndID" 
        private static Dictionary<string, Point2D[]> m_BakedRoutes = new(StringComparer.OrdinalIgnoreCase);

        // 노드(RegionCode) 등록 여부 캐싱
        private static HashSet<RegionCode> m_RegisteredNodes = new();

        public static void Initialize()
        {
            LoadTravelRoutes();
        }

        private static void LoadTravelRoutes()
        {
            string filePath = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "TravelRoutes.txt");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("[VirtualTravelNetwork] Warning: TravelRoutes.txt 파일이 없습니다. BakeAllRoutes 명령어를 실행하세요.");
                return;
            }

            try
            {
                m_BakedRoutes.Clear();
                m_RegisteredNodes.Clear();

                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    string[] parts = line.Split(':');
                    if (parts.Length != 2) continue;

                    string routeKey = parts[0].Trim();
                    string[] coords = parts[1].Split(';');
                    List<Point2D> path = new List<Point2D>();

                    foreach (string c in coords)
                    {
                        string[] xy = c.Split(',');
                        if (xy.Length == 2 && int.TryParse(xy[0], out int x) && int.TryParse(xy[1], out int y))
                        {
                            path.Add(new Point2D(x, y));
                        }
                    }

                    if (path.Count > 0)
                    {
                        m_BakedRoutes[routeKey] = path.ToArray();

                        // Key 분석 (예: Trammel_R_45_City_Britain_11_12)
                        // 유저님의 코드에서 RegionCode로 노드를 판별하므로, R_xx 형태의 코드를 추출해 등록합니다.
                        ExtractAndRegisterRegionCodes(routeKey);
                    }
                }

                Console.WriteLine($"[VirtualTravelNetwork] 성공! 총 {m_BakedRoutes.Count}개의 실크로드 경로를 로드했습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VirtualTravelNetwork] 로드 에러: {ex.Message}");
            }
        }

        private static void ExtractAndRegisterRegionCodes(string key)
        {
            // 노드 등록 헬퍼: R_123 형태의 텍스트가 있으면 RegionCode로 캐싱
            string[] tokens = key.Split('_');
            for (int i = 0; i < tokens.Length - 1; i++)
            {
                if (tokens[i] == "R" && int.TryParse(tokens[i + 1], out int codeVal))
                {
                    m_RegisteredNodes.Add((RegionCode)codeVal);
                }
            }
        }

        // 유저님 코드: !VirtualTravelNetwork.IsNodeRegistered(CurrentNode.RCode)
        public static bool IsNodeRegistered(RegionCode code)
        {
            return m_RegisteredNodes.Contains(code);
        }

        // ==============================================================================
        // 🎯 핵심: 유저님 코드가 호출하는 길찾기 및 비용 계산 엔진
        // ==============================================================================
        public static TravelPlan CalculateBestRoute(RegionCode start, RegionCode end, int partyWealth, bool allMounted)
        {
            // 1. 등록되지 않은 오지 탐험일 경우 (유저님 코드의 폴백 로직으로 넘김)
            if (!IsNodeRegistered(start) || !IsNodeRegistered(end))
            {
                return new TravelPlan { IsPossible = false }; 
            }

            // [임시 구현] 현재는 전체 그래프 다익스트라 대신, 베이킹된 경로 중 직접 연결된 것만 찾습니다.
            // (추후 노드-노드 간 A* 그래프 탐색을 추가할 수 있습니다)
            string searchKey1 = $"R_{(int)start}";
            string searchKey2 = $"R_{(int)end}";

            Point2D[] bestPath = null;
            bool isNavalRoute = false;

            // 딕셔너리에서 두 노드를 포함하는 경로 스캔
            foreach (var kvp in m_BakedRoutes)
            {
                if (kvp.Key.Contains(searchKey1) && kvp.Key.Contains(searchKey2))
                {
                    bestPath = kvp.Value;
                    // 경로 키에 선착장(Ferry/Docks)이 포함되어 있으면 해상 경로로 판정
                    isNavalRoute = kvp.Key.Contains("Ferry") || kvp.Key.Contains("Docks") || kvp.Key.Contains("Ocean");
                    break;
                }
            }

            if (bestPath == null)
            {
                return new TravelPlan { IsPossible = false }; // 연결된 길이 없음
            }

            // 2. 틱(시간) 및 비용 계산
            int distance = bestPath.Length; // 128x128 청크 징검다리 개수
            
            // 징검다리 1개(128타일)당 약 1틱(30분) 소요 (기마 시 절반)
            int calculatedTicks = Math.Max(1, distance); 
            if (allMounted) calculatedTicks = Math.Max(1, calculatedTicks / 2);

            int calculatedCost = 0;

            // ⛵ 유저님이 원하신 해상 이동(여객선) 비용 차감 로직!
            if (isNavalRoute)
            {
                // 해상 노선은 거리에 비례하여 여객선 승선비(Ferry Cost)가 발생합니다.
                calculatedCost = distance * 50; 

                // 돈이 없으면 배를 못 탑니다.
                if (partyWealth < calculatedCost)
                {
                    return new TravelPlan { IsPossible = false }; 
                }

                // 배를 타면 이동 속도가 조금 더 빠름 (시간 단축)
                calculatedTicks = Math.Max(1, calculatedTicks - 1); 
            }
            else
            {
                // 육로 이동의 기본 식비/도로 통행료 (소액)
                calculatedCost = distance * 5; 
            }

            // 최대 10틱 제한 (안전장치)
            calculatedTicks = Math.Min(10, calculatedTicks);

            return new TravelPlan 
            { 
                IsPossible = true, 
                TotalCost = calculatedCost, 
                TotalTicks = calculatedTicks 
            };
        }
    }
}