using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public static class EcosystemHarvester
    {
        // 1. 직업에 따른 ResourceType 매핑
        private static ResourceType? GetResourceTypeForJob(NpcJobClass job)
        {
            return job switch
            {
                NpcJobClass.SurfaceMiner or NpcJobClass.StoneQuarryman or NpcJobClass.SandDigger => ResourceType.Mining,
                NpcJobClass.Woodcutter => ResourceType.Lumberjacking,
                NpcJobClass.CoastalFisher or NpcJobClass.DeepSeaFisher_Basic or NpcJobClass.DeepSeaFisher or NpcJobClass.Crabber => ResourceType.Fishing,
                NpcJobClass.GrainFarmer or NpcJobClass.VegetableFarmer or NpcJobClass.GourdFarmer or NpcJobClass.Orchardist => ResourceType.Farming,
                _ => null
            };
        }

        // 2. 가축 사육 예외 처리
        private static (bool IsBreeder, Type AnimalType) CheckDomesticBreeding(NpcJobClass job)
        {
            return job switch
            {
                NpcJobClass.StableHand or NpcJobClass.PackLeader_Merchant or NpcJobClass.PackLeader_Warrior
                    => (true, Random.Shared.Next(2) == 0 ? typeof(PackHorse) : typeof(PackLlama)),
                NpcJobClass.HorseGroom_Basic or NpcJobClass.StableBroker => (true, typeof(Horse)),
                _ => (false, null)
            };
        }

        // 3. 물리 노드 매칭 로직 (사냥, 물 조달, 약초 한정)
        private static bool IsNodeMatch(Item node, NpcJobClass job)
        {
            string className = node.GetType().Name;
            return job switch
            {
                NpcJobClass.Trapper or NpcJobClass.BirdHunter or NpcJobClass.BigGameHunter or NpcJobClass.Poacher_Criminal => className == "AnimalNode",
                NpcJobClass.WaterCarrier => className == "WaterNode" || className == "WellNode",
                NpcJobClass.Herbalist or NpcJobClass.MushroomGatherer or NpcJobClass.BerryPicker => className == "HerbNode" || className == "PlantNode",
                _ => false
            };
        }

        private static IEnumerable GetSpawnedEntities(Item node)
        {
            var type = node.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = type.GetField("m_Spawned", flags) ?? type.GetField("Spawned", flags);
            if (field != null && field.GetValue(node) is IEnumerable list) return list;
            var prop = type.GetProperty("Spawned", flags);
            if (prop != null && prop.GetValue(node) is IEnumerable pList) return pList;
            return null;
        }

        // 4. 수확 핵심 로직 (TryHarvest)
// =======================================================================
		// [수정] 매개변수를 NpcJobClass에서 VirtualAgent로 변경하여 스킬 접근
		// =======================================================================
		public static (bool Success, Type ItemType, int Amount) TryHarvest(Map map, VirtualAgent agent)
		{
			NpcJobClass job = agent.JobClass;
			if (map == null || map == Map.Internal) return (false, null, 0);

			// [A] 가축 사육 예외 처리
			var breeding = CheckDomesticBreeding(job);
			if (breeding.IsBreeder)
			{
				if (Random.Shared.Next(100) < 25) 
				{
					agent.CheckSkillGain(); // 성공 시 스킬업
					return (true, breeding.AnimalType, 1);
				}
				return (false, null, 0);
			}

			// =======================================================================
			// [핵심] 스킬에 비례한 수확량 산출 (스킬 10당 1개 + 보너스 최대 5개)
			// =======================================================================
			int minAmount = (int)(agent.PrimarySkill / 20.0); // 100스킬 시 최소 5개
			int maxAmount = (int)(agent.PrimarySkill / 10.0) + 5; // 100스킬 시 최대 15개
			int gatherAmount = Math.Max(1, Random.Shared.Next(minAmount, maxAmount + 1));

			// [B] 자원 시스템(ResourcePool) 연동
			ResourceType? resType = GetResourceTypeForJob(job);
			if (resType.HasValue)
			{
				var activePools = ResourceManager.Pools.Values
					.Where(p => p.MapName == map.Name && p.Type == resType.Value && p.CanGather())
					.ToList();

				if (activePools.Count > 0)
				{
					var pool = activePools[Random.Shared.Next(activePools.Count)];
					var availableItems = pool.AvailableResources.Where(kvp => kvp.Value > 0).ToList();
					
					if (availableItems.Count > 0)
					{
						var selected = availableItems[Random.Shared.Next(availableItems.Count)];
						Type itemType = selected.Key;
						
						// 스킬로 증가된 수량(gatherAmount)만큼 매장량을 감소시킴 (유저와 치열한 경쟁)
						for (int i = 0; i < gatherAmount; i++) pool.ConsumeResource(itemType); 
						
						Type finalItemType = resType.Value switch
						{
							ResourceType.Mining => typeof(IronOre),
							ResourceType.Lumberjacking => typeof(Log),
							ResourceType.Fishing => typeof(Fish),
							ResourceType.Farming => itemType, 
							_ => typeof(IronOre)
						};

						agent.CheckSkillGain(); // 수확 성공 시 스킬업
						return (true, finalItemType, gatherAmount); 
					}
				}
				return (false, null, 0);
			}

			// [C] 기존 물리적 생태계 연동 (사냥꾼, 약초꾼 등)
			var validNodes = World.Items.Values
				.Where(n => n.Map == map && IsNodeMatch(n, job))
				.ToList();

			if (validNodes.Count == 0) return (false, null, 0);

			var activeNodes = validNodes.Where(n => 
			{
				var list = GetSpawnedEntities(n);
				return list != null && list.GetEnumerator().MoveNext();
			}).ToList();

			if (activeNodes.Count == 0) return (false, null, 0);

			var targetNode = activeNodes[Random.Shared.Next(activeNodes.Count)];

			bool isHuntingJob = job switch 
			{
				NpcJobClass.Trapper or NpcJobClass.BirdHunter or NpcJobClass.BigGameHunter or NpcJobClass.Poacher_Criminal => true,
				_ => false
			};

			if (isHuntingJob)
			{
				bool playerNear = false;
				var eable = map.GetMobilesInRange(targetNode.Location, 24);
				foreach (var m in eable)
				{
					if (m.Player && m.AccessLevel == AccessLevel.Player)
					{
						playerNear = true;
						break;
					}
				}
				eable.Free();

				if (playerNear) return (false, null, 0);
			}

			var spawnedList = GetSpawnedEntities(targetNode);
			if (spawnedList != null)
			{
				object targetEntity = null;
				foreach (var ent in spawnedList)
				{
					targetEntity = ent;
					break;
				}

				if (targetEntity != null)
				{
					Type resTypePhysical = null;
					int amount = 1;

					if (targetEntity is Item item)
					{
						resTypePhysical = item.GetType();
						amount = item.Amount;
						item.Delete();
					}
					else if (targetEntity is BaseCreature creature)
					{
						if (creature is BaseMount)
						{
							resTypePhysical = creature.GetType();
							amount = 1; // 탈것은 1마리로 고정
						}
						else
						{
							resTypePhysical = Random.Shared.Next(2) == 0 ? typeof(Hides) : typeof(RawRibs);
							// 몬스터 기본 가죽 수치에 내 스킬 기반 보너스(gatherAmount)를 합산!
							amount = Math.Max(1, creature.Hides > 0 ? creature.Hides : 5) + gatherAmount;
						}
						creature.Delete();
					}

					if (resTypePhysical != null) 
					{
						agent.CheckSkillGain(); // 수확 성공 시 스킬업
						// 물리 아이템 채집(약초 등) 시에도 기본 스폰 수량에 내 스킬 보너스 합산
						int totalAmount = (targetEntity is Item) ? amount + gatherAmount : amount;
						return (true, resTypePhysical, totalAmount);
					}
				}
			}

			return (false, null, 0);
		}
    }
}
