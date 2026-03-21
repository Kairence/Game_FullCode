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

        public static void HandlePlantBreeding(BaseFarmItem plant, int currentTick)
        {
            if (plant == null || plant.Stage != CropStage.Mature || plant.IsPollinated)
                return;

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
                        // 수정: CrossedType에 실제 타입을 넣지 않고 열거형 정보를 활용하거나 
                        // 아래처럼 원예 시스템의 특정 값을 지정합니다.
                        plant.CrossedType = PlantTypeInfo.RandomFirstGeneration(); 
                        plant.Name += " (교배됨)";
                        break;
                    }
                }
            }
            eable.Free();
        }

        public static void HandleLivestockBreeding(string regionKey, Map map, Region reg)
        {
            if (reg == null || map == null) return;

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
                        if (!m_PendingLivestock.ContainsKey(regionKey)) m_PendingLivestock[regionKey] = 0;
                        m_PendingLivestock[regionKey]++;
                    }
                }
            }
        }
		public static bool IsPlantable(Mobile from, Point3D loc, Map map)
		{
			bool onPloughedField = false;
			
			IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
			foreach (Item item in eable)
			{
				if (item is FarmPloughedComponent) // 2번 타일(경작지) 확인
				{
					onPloughedField = true;
					break;
				}
			}
			eable.Free();

			if (!onPloughedField)
			{
				from.SendMessage("경작된 밭(이랑 타일) 위에만 씨앗을 심을 수 있습니다.");
				return false;
			}
			
			return true; // 여기에 기존 개수 제한(CanPlant) 로직을 합치면 됩니다.
		}
        public static int GetPendingCount(string regionKey) => m_PendingLivestock.GetValueOrDefault(regionKey, 0);

        // CanPlant는 하나만 남깁니다. (중복 삭제)
        public static bool CanPlant(Mobile from)
        {
            int currentCount = World.Items.Values.OfType<BaseFarmItem>().Count(b => b.Owner == from);
            int limit = 5 + (int)(from.Skills[SkillName.Herding].Value / 10); // 기본 5개 + 스킬당 추가
            return currentCount < limit;
        }
        public static void GiveXP(Mobile from, int amount)
        {
            // Herding 스킬 상승 체크
            //from.CheckSkill(SkillName.Herding, 0, 120); 
        }
    }
}