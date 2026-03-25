using System;
using System.Collections.Generic;
using System.Linq; 
using Server;

namespace Server.Misc
{
    public class ResourcePool
    {
        public string MapName { get; set; }
        public string RegionName { get; set; }
        public ResourceType Type { get; set; }
        public LocationType LocType { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public int SizeCategory { get; set; } // 1: 소형, 2: 중형, 3: 대형

        // 현재 구역에 스폰되기로 결정된 자원(1~3종)만 담는 딕셔너리
        public Dictionary<Type, int> AvailableResources { get; set; } = new();

        // 고갈(100% 소진) 시 부과되는 쿨타임
        public DateTime DepletionCooldown { get; set; } = DateTime.MinValue;
        public ResourcePool(string mapName, string regionName, ResourceType type, LocationType locType, int max, int size)
        {
            MapName = mapName; 
            RegionName = regionName; 
            Type = type; 
            LocType = locType;
            MaxCapacity = max; 
            CurrentCapacity = 0; 
            SizeCategory = Math.Max(1, size);
            
            RollActiveResources(); // 생성 시 자원 종류만 결정해 둠

            // ★ [핵심 수정] 자원 종류(Type)에 따라 서버 오픈 시 초기 매장량을 다르게 세팅!
            if (Type == ResourceType.Mining)
            {
                // 1. 광물(Ore): 바위가 번식할 리 없으니 시작부터 100% 꽉 채워둡니다.
                Regenerate(MaxCapacity);
            }
            else if (Type == ResourceType.Fishing)
            {
                // 2. 낚시(Fish): 생태계 포식자 스폰을 위해 5%의 씨앗만 주고 서서히 번식하게 둡니다.
                Regenerate(MaxCapacity / 20);
            }
            else if (Type == ResourceType.Lumberjacking)
            {
                // 3. 나무(Wood): 숲의 초기 상태. (너무 비어있으면 벌목꾼이 힘드니 절반인 50%부터 시작하게 세팅)
                Regenerate(MaxCapacity / 2); 
            }
            else
            {
                // 혹시 모를 기타 자원은 기본적으로 꽉 채움
                Regenerate(MaxCapacity);
            }
        }

        public bool CanGather() => CurrentCapacity > 0 && DateTime.Now >= DepletionCooldown;
		// ResourceManager.cs
		public static void RegisterFarmingRegions()
		{
			foreach (Map map in Map.AllMaps)
			{
				if (map == null || map == Map.Internal) continue;

				foreach (Region r in map.Regions.Values) // .Values를 추가하여 KeyValuePair 에러 해결
				{
					if (r == null || string.IsNullOrEmpty(r.Name)) continue;

					string name = r.Name.ToLower();
					// 농경지 키워드 필터링
					if (name.Contains("farm") || name.Contains("field") || name.Contains("wheatfield"))
					{
						// 밀도 50%로 용량 설정
						int capacity = Math.Max(10, r.Area.Length / 2); 
						ResourceManager.RegisterPool(map.Name, r.Name, ResourceType.Farming, LocationType.Farm_Remote, capacity, 1);
					}
				}
			}
		}
		// ===================================================================================
        // [신규] 지역의 이름과 컨셉에 맞춰 가중치(Weight)를 동적으로 변경하는 함수
        // ===================================================================================
        private int GetLocalWeight(ResourceDef def)
        {
            int weight = def.Weight;
            string rName = RegionName.ToLower();
            string typeName = def.ItemType.Name.ToLower();

            // 벌목(Lumberjacking) 생태계 컨셉 필터
            if (Type == ResourceType.Lumberjacking)
            {
                bool isArctic = rName.Contains("ice") || rName.Contains("snow") || rName.Contains("glacier") || rName.Contains("dagger") || rName.Contains("winter");
                
                // 1. 북극/설원 컨셉
                if (isArctic)
                {
                    if (typeName.Contains("frostwood")) return weight * 20; // 서리나무 확률 대폭발
                    if (typeName == "log") return weight; // 일반 나무(Log)는 생존
                    return 0; // 나머지 고급 나무(오크, 유, 블러드 등)는 얼어 죽어서 안 나옴 (가중치 0)
                }
                else
                {
                    // 설원이 아닌 일반 숲에서 서리나무는 멸종 (가중치 0)
                    if (typeName.Contains("frostwood")) return 0; 
                }

                // 2. 유(Yew) 마을 컨셉
                if (rName.Contains("yew") && typeName.Contains("yew")) return weight * 15;

                // 3. 블러드우드 (늪, 어둠, 타락한 곳)
                if ((rName.Contains("swamp") || rName.Contains("bog") || rName.Contains("blood") || rName.Contains("dark")) && typeName.Contains("bloodwood"))
                    return weight * 15;

                // 4. 하트우드 (마법, 요정, 일쉐나 대륙)
                if ((rName.Contains("spirit") || rName.Contains("elf") || rName.Contains("wisp") || MapName == "Ilshenar") && typeName.Contains("heartwood"))
                    return weight * 15;

                // 5. 애쉬우드 (화산, 사막, 잿빛 구역)
                if ((rName.Contains("fire") || rName.Contains("ash") || rName.Contains("desert")) && typeName.Contains("ash"))
                    return weight * 15;
            }

            // 추후 Mining(광물)도 특정 동굴(예: Destard = 발로라이트 증가 등) 기획이 생기면 여기에 추가 가능!
            
            return weight;
        }

        // ===================================================================================
        // [수정] 자원 리셋 시 '지역 컨셉 가중치'를 반영하여 1~3종을 뽑습니다.
        // ===================================================================================
        public void RollActiveResources()
        {
            AvailableResources.Clear();
            if (!ResourceManager.Defs.ContainsKey(Type)) return;

            List<ResourceDef> validDefs = ResourceManager.Defs[Type]
                .Where(d => d.ReqLoc == LocationType.Normal || d.ReqLoc == LocType).ToList();

            if (validDefs.Count == 0) return;

            // 1. 기본 자원(철, 일반 나무)은 100% 무조건 포함
            ResourceDef baseDef = validDefs[0]; 
            AvailableResources[baseDef.ItemType] = 0;

            // 2. 추가 0~2종 뽑기 (지역별 가중치 적용)
            int extraTypesCount = Utility.RandomMinMax(0, 2); 
            
            // 유효한 고급 자원들만 걸러서(가중치가 0인 애들은 탈락) 리스트를 만듦
            List<Tuple<ResourceDef, int>> localDefs = new List<Tuple<ResourceDef, int>>();
            foreach (var def in validDefs.Skip(1))
            {
                int w = GetLocalWeight(def);
                if (w > 0) localDefs.Add(new Tuple<ResourceDef, int>(def, w));
            }

            for (int i = 0; i < extraTypesCount && localDefs.Count > 0; i++)
            {
                int totalWeight = localDefs.Sum(d => d.Item2);
                int roll = Utility.Random(totalWeight);
                int cur = 0;
                
                for (int j = 0; j < localDefs.Count; j++)
                {
                    cur += localDefs[j].Item2;
                    if (roll < cur)
                    {
                        AvailableResources[localDefs[j].Item1.ItemType] = 0;
                        localDefs.RemoveAt(j); // 중복 픽 방지
                        break;
                    }
                }
            }
        }

        // ===================================================================================
        // [수정] 1분마다 회복될 때도 '지역 컨셉 가중치' 비율에 맞춰 매장량을 분배합니다.
        // ===================================================================================
        public void Regenerate(int tickAmount)
        {
            if (DateTime.Now < DepletionCooldown) return;

            if (CurrentCapacity <= 0) RollActiveResources();
            if (CurrentCapacity >= MaxCapacity) return;

            int oldCapacity = CurrentCapacity;
            CurrentCapacity += (tickAmount / SizeCategory);
            if (CurrentCapacity > MaxCapacity) CurrentCapacity = MaxCapacity;

            int restoredAmount = CurrentCapacity - oldCapacity;
            if (restoredAmount <= 0) return;

            // 현재 스폰 리스트에 있는 자원들의 '지역 맞춤 가중치'를 불러옴
            List<Tuple<ResourceDef, int>> activeDefs = new List<Tuple<ResourceDef, int>>();
            foreach (Type t in AvailableResources.Keys)
            {
                var def = ResourceManager.GetDef(Type, t);
                if (def != null) 
                {
                    int w = GetLocalWeight(def);
                    activeDefs.Add(new Tuple<ResourceDef, int>(def, w));
                }
            }

            int totalW = activeDefs.Sum(d => d.Item2);
            if (totalW <= 0) return;

            for (int i = 0; i < restoredAmount; i++)
            {
                int r = Utility.Random(totalW);
                int c = 0;
                foreach (var tuple in activeDefs)
                {
                    c += tuple.Item2;
                    if (r < c)
                    {
                        AvailableResources[tuple.Item1.ItemType]++;
                        break;
                    }
                }
            }
        }

        // ★ [복구 완료] 유저가 채광/벌목을 성공했을 때 호출 (Capacity 깎기 및 고갈 체크)
        public void ConsumeResource(Type itemType)
        {
            if (AvailableResources.ContainsKey(itemType) && AvailableResources[itemType] > 0)
            {
                AvailableResources[itemType]--;
                CurrentCapacity--;

                // 100% 소진되었을 때 30분 쿨타임 발동
                if (CurrentCapacity <= 0)
                {
                    CurrentCapacity = 0;
                    DepletionCooldown = DateTime.Now.AddMinutes(30.0); // 30분간 회복 불가
                    AvailableResources.Clear(); // 싹 비움
                }
            }
        }

        // 직렬화 (저장)
        public void Serialize(GenericWriter writer)
        {
            writer.Write(1); // Version 1
            writer.Write(DepletionCooldown); 

            writer.Write(CurrentCapacity);
            writer.Write(AvailableResources.Count);
            foreach (var kvp in AvailableResources)
            {
                writer.Write(kvp.Key.FullName);
                writer.Write(kvp.Value);
            }
        }

        // 역직렬화 (로드)
        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            if (version >= 1)
            {
                DepletionCooldown = reader.ReadDateTime(); 
            }

            CurrentCapacity = reader.ReadInt();
            int count = reader.ReadInt();
            AvailableResources.Clear();
            for (int i = 0; i < count; i++)
            {
                Type type = ScriptCompiler.FindTypeByFullName(reader.ReadString());
                int amount = reader.ReadInt();
                if (type != null) AvailableResources[type] = amount;
            }
            if (CurrentCapacity > MaxCapacity) CurrentCapacity = MaxCapacity;
        }
    }
}
