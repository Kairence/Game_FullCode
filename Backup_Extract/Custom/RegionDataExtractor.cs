using System;
using System.IO;
using System.Linq;
using System.Xml; // XDocument 대신 XmlDocument 사용 (CS1069 에러 방지)
using System.Collections.Generic;
using Server;
using Server.Commands;

namespace Server.Misc // 요청하신 대로 네임스페이스 변경
{
    public class RegionDataExtractor
    {
        public static void Initialize()
        {
            CommandSystem.Register("ExRe", AccessLevel.Administrator, new CommandEventHandler(OnExtractRegions));
        }

        private static void OnExtractRegions(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            // 보통 ServUO의 물리 Region 데이터는 Data/Regions.xml 경로에 있습니다.
            string xmlPath = Path.Combine(Core.BaseDirectory, "Data", "Regions.xml");

            if (!File.Exists(xmlPath))
            {
                from.SendMessage(33, $"Regions.xml 파일을 찾을 수 없습니다: {xmlPath}");
                return;
            }

            from.SendMessage(68, "Regions.xml 데이터 분석 및 GoGump 교차 검증을 시작합니다...");

            // 추출된 코드는 서버 Logs 폴더에 저장됩니다. (시간 계산은 Now 사용)
            string outputPath = Path.Combine(Core.BaseDirectory, "Logs", $"RegionSaver_Extracted_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using StreamWriter writer = new(outputPath);
            writer.WriteLine("// ==============================================================================");
            writer.WriteLine("// 🌟 자동 추출된 RegionSaver 데이터베이스 (XML + GoGump 하이브리드 매핑)");
            writer.WriteLine("// 이 배열 데이터를 복사하여 RegionSaver.m_Regions 에 그대로 덮어씌우세요.");
            writer.WriteLine("// ==============================================================================");

            // XmlDocument 로드 (에러가 잦은 XDocument 대체)
            XmlDocument doc = new();
            doc.Load(xmlPath);

            int successCount = 0;
            int failCount = 0;

            // 열거형(Enum) 이름들을 배열로 캐싱
            string[] enumNames = Enum.GetNames(typeof(RegionCode));

            // 모든 Facet 노드 탐색
            XmlNodeList? facetNodes = doc.SelectNodes("//Facet");
            if (facetNodes == null) return;

            foreach (XmlNode facetNode in facetNodes)
            {
                string facetName = facetNode.Attributes?["name"]?.Value ?? "";
                Map map = GetMap(facetName);
                
                if (map == Map.Internal) continue;

                // 해당 Facet 하위의 모든 region 노드 탐색
                XmlNodeList? regionNodes = facetNode.SelectNodes(".//region");
                if (regionNodes == null) continue;

                foreach (XmlNode regionNode in regionNodes)
                {
                    string xmlRegionName = regionNode.Attributes?["name"]?.Value ?? "Unknown";

                    // Z축 기본값은 울온 최대/최저 고도로 설정
                    int minZ = -255;
                    int maxZ = 255;

                    // zrange 탐색
                    XmlNode? zNode = regionNode.SelectSingleNode("zrange");
                    if (zNode != null)
                    {
                        var minParsed = ParseXmlInt(zNode.Attributes?["min"]?.Value);
                        if (minParsed.Success) minZ = minParsed.Value;

                        var maxParsed = ParseXmlInt(zNode.Attributes?["max"]?.Value);
                        if (maxParsed.Success) maxZ = maxParsed.Value;
                    }

                    // 모든 rect 조각 순회
                    XmlNodeList? rectNodes = regionNode.SelectNodes("rect");
                    if (rectNodes == null) continue;

                    foreach (XmlNode rectNode in rectNodes)
                    {
                        var parsedX = ParseXmlInt(rectNode.Attributes?["x"]?.Value);
                        var parsedY = ParseXmlInt(rectNode.Attributes?["y"]?.Value);
                        var parsedWidth = ParseXmlInt(rectNode.Attributes?["width"]?.Value);
                        var parsedHeight = ParseXmlInt(rectNode.Attributes?["height"]?.Value);

                        if (!parsedX.Success || !parsedY.Success || !parsedWidth.Success || !parsedHeight.Success)
                        {
                            continue;
                        }

                        int x = parsedX.Value;
                        int y = parsedY.Value;
                        int width = parsedWidth.Value;
                        int height = parsedHeight.Value;

                        int endX = x + width;
                        int endY = y + height;
                        
                        // 사각형의 중앙점 계산
                        int centerX = x + (width / 2);
                        int centerY = y + (height / 2);

                        // 중심 좌표를 이용해 GoGump(LocationTree)에서 층수 및 상세 구역명 추론
                        Point3D centerPoint = new(centerX, centerY, map.GetAverageZ(centerX, centerY));
                        string goGumpName = NewSpawnManager.GetGoGumpZoneName(centerPoint, map);

                        // 튜플을 사용하여 Enum 매칭 결과 반환
                        (bool isMatched, string matchedEnumName) = FindMatchingRegionCode(enumNames, goGumpName, xmlRegionName, facetName);

                        if (isMatched)
                        {
                            writer.WriteLine($"            new(Map.{map.Name}, {x}, {y}, {endX}, {endY}, {minZ}, {maxZ}, RegionCode.{matchedEnumName}),");
                            successCount++;
                        }
                        else
                        {
                            writer.WriteLine($"            // [매칭실패] new(Map.{map.Name}, {x}, {y}, {endX}, {endY}, {minZ}, {maxZ}, RegionCode.None), // XML:{xmlRegionName} / GoGump:{goGumpName}");
                            failCount++;
                        }
                    }
                }
            }

            from.SendMessage(68, $"추출 완료! 성공: {successCount}건 / 수동 확인 필요: {failCount}건");
            from.SendMessage(68, "서버의 Logs 폴더에 추출된 txt 파일을 확인하세요.");
        }

        private static (bool IsMatched, string EnumName) FindMatchingRegionCode(string[] enumNames, string goGumpName, string xmlName, string facetName)
        {
            string cleanGoGump = CleanStringForMatch(goGumpName);
            string cleanXml = CleanStringForMatch(xmlName);
            string cleanFacet = CleanStringForMatch(facetName);

            // 1순위: GoGump 경로 매칭
            if (!string.IsNullOrEmpty(cleanGoGump) && cleanGoGump != "unknown")
            {
                var exactGoGumpMatch = enumNames.FirstOrDefault(e => CleanStringForMatch(e).Contains(cleanGoGump) || cleanGoGump.Contains(CleanStringForMatch(e)));
                if (exactGoGumpMatch != null) return (true, exactGoGumpMatch);
            }

            // 2순위: XML Region Name 매칭
            var xmlMatch = enumNames.FirstOrDefault(e => CleanStringForMatch(e) == $"{cleanFacet}{cleanXml}" || CleanStringForMatch(e).EndsWith(cleanXml));
            if (xmlMatch != null) return (true, xmlMatch);

            // 3순위: 부분 매칭
            var partialMatch = enumNames.FirstOrDefault(e => CleanStringForMatch(e).Contains(cleanXml));
            if (partialMatch != null) return (true, partialMatch);

            return (false, string.Empty);
        }

        private static string CleanStringForMatch(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return new string(input.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();
        }

        private static (bool Success, int Value) ParseXmlInt(string? value)
        {
            if (int.TryParse(value, out int result)) return (true, result);
            return (false, 0);
        }

        private static Map GetMap(string facetName)
        {
            return facetName.ToLower() switch
            {
                "trammel" => Map.Trammel,
                "felucca" => Map.Felucca,
                "ilshenar" => Map.Ilshenar,
                "malas" => Map.Malas,
                "tokuno" => Map.Tokuno,
                "termur" => Map.TerMur,
                _ => Map.Internal
            };
        }
    }
}