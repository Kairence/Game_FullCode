using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"C:\Kairence_UO\4.0\Scripts\Custom\City Economy System\NPC Group\VirtualAdventurerSystem.cs";
        string content = File.ReadAllText(path);
        
        string findTarget = "var questDz = DungeonManager.ZoneList.FirstOrDefault(z => z.RCode.ToString() == acceptedJob.RegionName && z.IsActive);";
        int idx = content.IndexOf(findTarget);
        if (idx == -1) { Console.WriteLine("NOT FOUND"); return; }
        
        string blockEnd = "}\r\n                }";
        int endIdx = content.IndexOf(blockEnd, idx);
        if (endIdx == -1) { Console.WriteLine("END NOT FOUND"); return; }
        
        string newCode = @"
                    if (acceptedJob.Category == JobCategory.EcoHunting)
                    {
                        var ecoZone = Server.Misc.EcosystemManager.ZoneList.FirstOrDefault(z => z.ZoneId == acceptedJob.RegionName);
                        if (ecoZone != null && ecoZone.Nodes.Count > 0)
                        {
                            Point3D dest = ecoZone.Nodes[0].Location;
                            TargetNode = new WorldNode(ecoZone.ZoneId, ecoZone.RCode, WorldNodeType.Dungeon, ecoZone.Facet, dest, dest, 100);
                            this.AcceptedJobReward = acceptedJob.RewardGold; 
                            Console.WriteLine($"[Quest] {Members[0].Name} 파티가 방치된 생태계 사냥 '{acceptedJob.Title}' 의뢰를 수락하고 앞마당(청크)으로 향합니다.");
                        }
                    }
                    else
                    {
                        var questDz = DungeonManager.ZoneList.FirstOrDefault(z => z.RCode.ToString() == acceptedJob.RegionName && z.IsActive);
                        if (questDz != null)
                        {
                            Point3D dest = questDz.GetCenterLocation();
                            if (dest == Point3D.Zero) dest = RegionSaver.GetRegionCenter(questDz.RCode, questDz.Facet);

                            if (dest != Point3D.Zero)
                            {
                                TargetNode = new WorldNode(questDz.ZoneId, questDz.RCode, WorldNodeType.Dungeon, questDz.Facet, dest, dest, questDz.CurrentDifficulty);
                                this.AcceptedJobReward = acceptedJob.RewardGold; 
                                Console.WriteLine($"[Quest] {Members[0].Name} 파티가 방치된 '{acceptedJob.Title}' 의뢰를 수락하고 출발 준비를 마쳤습니다.");
                            }
                        }
                    }";
                    
        string result = content.Substring(0, idx) + newCode.TrimStart() + content.Substring(endIdx + 2);
        File.WriteAllText(path, result, System.Text.Encoding.UTF8);
        Console.WriteLine("SUCCESS");
    }
}
