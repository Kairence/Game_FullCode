using System;
using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;
using Server.Regions;
using Server.Engines.Plants;
using Server.Misc;

namespace Server.Items
{
    public static class FarmingSystem
    {
        private static Dictionary<string, int> m_PendingLivestock = new();

        // 🌟 [추가 1] 대륙 OFF 시 해당 맵의 모든 물리적 작물 아이템을 일괄 제거하는 청소 함수 (NewSpawnManager 연동용)
        public static int ClearMapCrops(Map map)
        {
            if (map == null || map == Map.Internal) return 0;
            
            int count = 0;
            var crops = World.Items.Values.OfType<BaseFarmItem>().Where(i => i.Map == map).ToList();
            
            for (int i = 0; i < crops.Count; i++)
            {
                crops[i].Delete();
                count++;
            }
            return count;
        }

        public static void HandlePlantBreeding(BaseFarmItem plant, int currentTick)
        {
            if (plant == null || plant.Deleted || plant.Map == null) return;
            
            // 🌟 [추가 2] 중앙 킬 스위치 연동: 대륙이 꺼져있으면 식물 성장 및 수분 연산 완전 차단
            if (!NewSpawnManager.ActiveMaps.GetValueOrDefault(plant.Map, true)) return;

            if (plant.Stage != CropStage.Mature || plant.IsPollinated) return;

            int checkInterval = plant.IsAccelerated ? 15 : 30;
            if (currentTick % checkInterval != 0) return;

            IPooledEnumerable eable = plant.Map.GetItemsInRange(plant.Location, 2);
            foreach (Item item in eable)
            {
                if (item is BaseFarmItem other && other != plant && other.Owner == plant.Owner)
                {
                    if (other.Stage == CropStage.Mature && Utility.RandomDouble() < 0.10)
                    {
                        plant.IsPollinated = true;
                        plant.CrossedType = PlantTypeInfo.RandomFirstGeneration(); 
                        plant.Name += " (교배종)";
                        break;
                    }
                }
            }
            eable.Free();
        }

        public static void HandleLivestockBreeding(string regionKey, Map map, Region reg)
        {
            if (reg == null || map == null) return;
            
            // 🌟 [추가 2] 중앙 킬 스위치 연동: 대륙이 꺼져있으면 가축 번식 연산 차단
            if (!NewSpawnManager.ActiveMaps.GetValueOrDefault(map, true)) return;

            var livestock = reg.GetMobiles().OfType<BaseCreature>()
                .Where(c => c is Cow || c is Sheep || c is Chicken || c is Pig)
                .GroupBy(c => c.GetType());

            foreach (var group in livestock)
            {
                int pairs = group.Count() / 2;
                for (int i = 0; i < pairs; i++)
                {
                    if (Utility.RandomDouble() < 0.10)
                    {
                        if (!m_PendingLivestock.ContainsKey(regionKey)) 
                            m_PendingLivestock[regionKey] = 0;
                            
                        m_PendingLivestock[regionKey]++;
                    }
                }
            }
        }

        public static bool IsPlantable(Mobile from, Point3D loc, Map map)
        {
            // 🌟 [추가 3] 자원 모니터링 시스템(ResourceSystem) 귀속: 해당 지역의 지력(FarmCap) 체크
            var chunkInfo = EcoGridDatabase.GetChunkAt(map, loc.X, loc.Y);
            if (chunkInfo.IsValid)
            {
                // ResourceType.Farming (혹은 유저님이 설정한 농사 Enum)을 통해 해당 그리드의 자원 풀에 접근
                ResourceKey farmKey = new ResourceKey(map.Name, chunkInfo.Data.Code.ToString(), ResourceType.Farming);
                if (ResourceManager.Pools.TryGetValue(farmKey, out var pool))
                {
                    if (pool.CurrentCapacity <= 0)
                    {
                        from.SendMessage("이 지역의 지력이 모두 고갈되어 더 이상 작물을 심을 수 없습니다.");
                        return false; // 지력 한도 초과 시 식재 차단
                    }
                    
                    // (선택 사항) 작물을 심을 때 지력을 1 소모하게 하려면 여기에 주석 해제
                    // pool.CurrentCapacity -= 1; 
                }
            }

            bool onPloughedField = false;
            
            IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
            foreach (Item item in eable)
            {
                if (item is FarmPloughedComponent) 
                {
                    onPloughedField = true;
                    break;
                }
            }
            eable.Free();

            if (!onPloughedField)
            {
                from.SendMessage("경작된 밭(이랑 타일) 위에서만 씨앗을 심을 수 있습니다.");
                return false;
            }
            
            return true;
        }

        public static int GetPendingCount(string regionKey) => m_PendingLivestock.GetValueOrDefault(regionKey, 0);

        public static bool CanPlant(Mobile from)
        {
            int currentCount = World.Items.Values.OfType<BaseFarmItem>().Count(b => b.Owner == from);
            int limit = 5 + (int)(from.Skills[SkillName.Herding].Value / 10); 
            return currentCount < limit;
        }

        public static void GiveXP(Mobile from, int amount)
        {
            // from.CheckSkill(SkillName.Herding, 0, 120); 
        }
    }
}