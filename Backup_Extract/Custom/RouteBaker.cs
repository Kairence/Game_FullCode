using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Server;
using Server.Commands;

namespace Server.Misc
{
    public static class RouteBaker
    {
        public static void Initialize()
        {
            CommandSystem.Register("BakeAllRoutes", AccessLevel.Administrator, new CommandEventHandler(BakeAllRoutes_OnCommand));
        }

        [Usage("BakeAllRoutes")]
        private static void BakeAllRoutes_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage(68, "대륙별 검증이 포함된 '진짜' 실크로드 베이킹을 시작합니다...");

            string logsDir = Path.Combine(Core.BaseDirectory, "Logs");
            string csvPath = null;
            if (Directory.Exists(logsDir))
            {
                var files = Directory.GetFiles(logsDir, "EcoGrid_Master_AllMaps_*.csv").OrderByDescending(f => f).ToList();
                if (files.Count > 0) csvPath = files[0];
            }

            System.Threading.ThreadPool.QueueUserWorkItem(state => 
            {
                try
                {
                    if (csvPath == null) { Console.WriteLine("CSV 파일이 없습니다."); return; }
                    var chunkDict = BuildChunkGraph(csvPath);

                    Dictionary<string, Point2D[]> bakedRoutes = new Dictionary<string, Point2D[]>();
                    int totalBaked = 0;

                    Map[] targetMaps = new Map[] { Map.Trammel, Map.Felucca, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };

                    foreach (Map map in targetMaps)
                    {
                        if (map == null || map == Map.Internal) continue;
                        if (!chunkDict.TryGetValue(map.Name.Trim(), out var chunkMap)) continue;

                        // 🌟 [필터 강화] 현재 대륙에 실제로 존재하는 거점만 수집
                        List<NodePoint> waypoints = GatherStrategicWaypoints(map);
                        
                        Console.WriteLine(string.Format("[RouteBaker] {0} 대륙 실제 전략 거점 {1}개 계산...", map.Name, waypoints.Count));

                        for (int i = 0; i < waypoints.Count; i++)
                        {
                            for (int j = i + 1; j < waypoints.Count; j++)
                            {
                                NodePoint start = waypoints[i];
                                NodePoint end = waypoints[j];

                                // 🌟 섬 점프 방지 (거리 제한)
                                if (Utility.GetDistanceToSqrt(start.Location, end.Location) > 3500) continue;

                                bool isNaval = (start.ID.Contains("Docks") || end.ID.Contains("Docks"));
                                Point2D[] path = FindPathHybrid(chunkMap, start.Location, end.Location, isNaval);

                                if (path != null && path.Length > 1)
                                {
                                    bakedRoutes[string.Format("{0}_{1}_{2}", map.Name, start.ID, end.ID)] = path;
                                    totalBaked++;
                                }
                            }
                        }
                        GC.Collect();
                    }

                    SaveRoutesToText(bakedRoutes);
                    Timer.DelayCall(TimeSpan.Zero, () => e.Mobile.SendMessage(68, string.Format("베이킹 완료! 유효 경로 총 {0}개 확보.", totalBaked)));
                }
                catch (Exception ex) { Console.WriteLine(ex); }
            });
        }

        private static List<NodePoint> GatherStrategicWaypoints(Map map)
        {
            List<NodePoint> list = new List<NodePoint>();
            int logicID = (map.MapID == 1) ? 0 : (map.MapID == 0 ? 1 : map.MapID);

            for (int x = 0; x < TownNumber.MaxChunkX; x++)
            {
                for (int y = 0; y < TownNumber.MaxChunkY; y++)
                {
                    var result = TownNumber.GetStrategicChunk(x, y);
                    if (!result.IsValid || result.Chunk == null || result.Chunk.Type == ChunkType.Plains || result.Chunk.Type == ChunkType.Ocean) 
                        continue;

                    string chunkName = result.Chunk.Name;
                    Point3D center = new Point3D(x * 128 + 64, y * 128 + 64, 0);

                    if (result.Chunk.Type == ChunkType.City)
                    {
                        int townID = TownNumber.GetID(center, map);
                        int baseID = townID % 100;
                        
                        // 🌟 전초기지(50 이상)이거나 등록되지 않은 곳이면 가차 없이 스킵!
                        if (townID == 0 || baseID >= 50) continue;
                    }
                    else if (result.Chunk.Type == ChunkType.ChokePoint || result.Chunk.Type == ChunkType.MagicNode)
                    {
                        if (logicID > 1) continue;
                    }

                    list.Add(new NodePoint(string.Format("{0}_{1}_{2}_{3}", result.Chunk.Type, chunkName, x, y), center));
                }
            }

            foreach (RegionCode code in Enum.GetValues(typeof(RegionCode)))
            {
                if (code == RegionCode.None) continue;
                if (!code.ToString().StartsWith(map.Name, StringComparison.OrdinalIgnoreCase)) continue;

                Point3D loc = RegionSaver.GetRegionCenter(code, map);
                if (loc != Point3D.Zero) list.Add(new NodePoint(string.Format("R_{0}", (int)code), loc));
            }

            return list;
        }

        private static Point2D[] FindPathHybrid(Dictionary<string, ChunkNode> chunkMap, Point3D start, Point3D end, bool isNaval)
        {
            PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>();
            Dictionary<string, PathNode> allNodes = new Dictionary<string, PathNode>();

            ChunkNode startChunk = GetNearestChunk(chunkMap, start.X, start.Y);
            if (startChunk == null) return null;

            PathNode startNode = new PathNode(startChunk, 0, null);
            openSet.Enqueue(startNode);
            allNodes[startChunk.Key] = startNode;

            int[][] dirs = new int[][] { new int[]{0,-1}, new int[]{1,0}, new int[]{0,1}, new int[]{-1,0} };

            while (openSet.Count > 0)
            {
                PathNode current = openSet.Dequeue();
                if (Utility.GetDistanceToSqrt(new Point3D(current.Chunk.Center.X, current.Chunk.Center.Y, 0), end) < 160)
                    return ReconstructPath(current, start, end);

                foreach (var dir in dirs)
                {
                    string key = string.Format("{0}_{1}", current.Chunk.X + dir[0], current.Chunk.Y + dir[1]);
                    if (!chunkMap.TryGetValue(key, out ChunkNode next)) continue;

                    bool isWater = next.RegionCode.Contains("Ocean") || next.RegionCode.Contains("Water");
                    if (!isNaval && isWater) continue; 
                    if (isNaval && !isWater) continue; 

                    double cost = current.G + 1.0;
                    if (!allNodes.TryGetValue(key, out PathNode neighbor) || cost < neighbor.G)
                    {
                        neighbor = new PathNode(next, cost, current);
                        allNodes[key] = neighbor;
                        openSet.Enqueue(neighbor);
                    }
                }
            }
            return null;
        }

        private static ChunkNode GetNearestChunk(Dictionary<string, ChunkNode> chunkMap, int x, int y)
        {
            string key = string.Format("{0}_{1}", x / 128, y / 128);
            if (chunkMap.TryGetValue(key, out var node)) return node;
            Point3D target = new Point3D(x, y, 0);
            return chunkMap.Values.OrderBy(c => Utility.GetDistanceToSqrt(new Point3D(c.Center.X, c.Center.Y, 0), target)).FirstOrDefault();
        }

        private static Point2D[] ReconstructPath(PathNode endNode, Point3D start, Point3D end)
        {
            List<Point2D> path = new List<Point2D>();
            path.Add(new Point2D(end.X, end.Y));
            PathNode curr = endNode;
            while (curr != null) 
            { 
                // 🌟 좌표 중복 방지: 마지막에 넣은 좌표와 같으면 스킵
                Point2D p = curr.Chunk.Center;
                if (path.Count == 0 || (path[path.Count-1].X != p.X || path[path.Count-1].Y != p.Y))
                    path.Add(p);
                curr = curr.Parent; 
            }
            path.Add(new Point2D(start.X, start.Y));
            path.Reverse();
            return path.ToArray();
        }

        private static Dictionary<string, Dictionary<string, ChunkNode>> BuildChunkGraph(string csvPath)
        {
            var dict = new Dictionary<string, Dictionary<string, ChunkNode>>(StringComparer.OrdinalIgnoreCase);
            string[] lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++) {
                string[] p = lines[i].Split(',');
                if (p.Length < 6) continue;
                string m = p[0].Trim();
                if (!dict.ContainsKey(m)) dict[m] = new Dictionary<string, ChunkNode>();
                int cx, cy, px, py;
                if (int.TryParse(p[1], out cx) && int.TryParse(p[2], out cy) && int.TryParse(p[3], out px) && int.TryParse(p[4], out py)) {
                    dict[m][string.Format("{0}_{1}", cx, cy)] = new ChunkNode { X = cx, Y = cy, Center = new Point2D(px, py), RegionCode = p[5].Trim() };
                }
            }
            return dict;
        }

        private static void SaveRoutesToText(Dictionary<string, Point2D[]> routes)
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "EconomySystem", "TravelRoutes.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8)) {
                foreach (var kvp in routes) {
                    sw.WriteLine(string.Format("{0}:{1}", kvp.Key, string.Join(";", kvp.Value.Select(p => string.Format("{0},{1}", p.X, p.Y)))));
                }
            }
        }

        private class ChunkNode { public int X, Y; public Point2D Center; public string RegionCode; public string Key { get { return string.Format("{0}_{1}", X, Y); } } }
        private class NodePoint { public string ID; public Point3D Location; public NodePoint(string id, Point3D loc) { ID = id; Location = loc; } }
        private class PathNode : IComparable<PathNode> {
            public ChunkNode Chunk; public double G; public PathNode Parent;
            public PathNode(ChunkNode chunk, double g, PathNode parent) { Chunk = chunk; G = g; Parent = parent; }
            public int CompareTo(PathNode other) { return G.CompareTo(other.G); }
        }
        private class PriorityQueue<T> where T : IComparable<T> {
            private List<T> d = new List<T>(); public int Count { get { return d.Count; } }
            public void Enqueue(T i) { d.Add(i); d.Sort(); }
            public T Dequeue() { T i = d[0]; d.RemoveAt(0); return i; }
            public void Update(T i) { d.Sort(); }
        }
    }
}