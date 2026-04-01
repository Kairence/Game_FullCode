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
        // 지역별로 번식 대기 중인 가축 수를 저장하는 딕셔너리
        private static Dictionary<string, int> m_PendingLivestock = new();

        public static void HandlePlantBreeding(BaseFarmItem plant, int currentTick)
        {
            if (plant == null || plant.Stage != CropStage.Mature || plant.IsPollinated)
                return;

            // 성장 촉진 상태(IsAccelerated)라면 15틱, 아니면 30틱마다 체크
            int checkInterval = plant.IsAccelerated ? 15 : 30;
            if (currentTick % checkInterval != 0) return;

            // 주변 2칸 이내의 아이템 탐색
            IPooledEnumerable eable = plant.Map.GetItemsInRange(plant.Location, 2);
            foreach (Item item in eable)
            {
                // 주인이 같고 다 자란(Mature) 다른 작물이 근처에 있다면 10% 확률로 수분 발생
                if (item is BaseFarmItem other && other != plant && other.Owner == plant.Owner)
                {
                    if (other.Stage == CropStage.Mature && Utility.RandomDouble() < 0.10)
                    {
                        plant.IsPollinated = true;
                        
                        // 교배 로직: 새로운 1세대 식물 타입을 무작위로 선택하여 할당
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

            // 해당 지역 내의 특정 가축(소, 양, 닭, 돼지)을 필터링하여 종류별로 그룹화
            var livestock = reg.GetMobiles().OfType<BaseCreature>()
                .Where(c => c is Cow || c is Sheep || c is Chicken || c is Pig)
                .GroupBy(c => c.GetType());

            foreach (var group in livestock)
            {
                // 2마리당 1쌍으로 계산하여 번식 확률 적용
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
            bool onPloughedField = false;
            
            // 현재 위치에 경작된 밭(FarmPloughedComponent) 타일이 있는지 확인
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
            // 현재 플레이어가 소유한 작물 수 계산
            int currentCount = World.Items.Values.OfType<BaseFarmItem>().Count(b => b.Owner == from);
            
            // 식재 제한: 기본 5개 + Herding(목동) 스킬 10당 1개 추가
            int limit = 5 + (int)(from.Skills[SkillName.Herding].Value / 10); 
            return currentCount < limit;
        }

        public static void GiveXP(Mobile from, int amount)
        {
            // Herding 스킬 숙련도 체크 (필요 시 주석 해제)
            // from.CheckSkill(SkillName.Herding, 0, 120); 
        }
    }
}