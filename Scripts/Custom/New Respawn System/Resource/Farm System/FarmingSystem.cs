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
                        // ����: CrossedType�� ���� Ÿ���� ���� �ʰ� ������ ������ Ȱ���ϰų� 
                        // �Ʒ�ó�� ���� �ý����� Ư�� ���� �����մϴ�.
                        plant.CrossedType = PlantTypeInfo.RandomFirstGeneration(); 
                        plant.Name += " (�����)";
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
				if (item is FarmPloughedComponent) // 2�� Ÿ��(������) Ȯ��
				{
					onPloughedField = true;
					break;
				}
			}
			eable.Free();

			if (!onPloughedField)
			{
				from.SendMessage("���۵� ��(�̶� Ÿ��) ������ ������ ���� �� �ֽ��ϴ�.");
				return false;
			}
			
			return true; // ���⿡ ���� ���� ����(CanPlant) ������ ��ġ�� �˴ϴ�.
		}
        public static int GetPendingCount(string regionKey) => m_PendingLivestock.GetValueOrDefault(regionKey, 0);

        // CanPlant�� �ϳ��� ����ϴ�. (�ߺ� ����)
        public static bool CanPlant(Mobile from)
        {
            int currentCount = World.Items.Values.OfType<BaseFarmItem>().Count(b => b.Owner == from);
            int limit = 5 + (int)(from.Skills[SkillName.Herding].Value / 10); // �⺻ 5�� + ��ų�� �߰�
            return currentCount < limit;
        }
        public static void GiveXP(Mobile from, int amount)
        {
            // Herding ��ų ��� üũ
            //from.CheckSkill(SkillName.Herding, 0, 120); 
        }
    }
}
