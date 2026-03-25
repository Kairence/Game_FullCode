using System;

namespace Server.Misc
{
    // [1] 자원 종류
    public enum ResourceType { Mining, Lumberjacking, Fishing, Tanning, Farming }

    // [2] 자원 스폰 기후/지역 조건
    public enum LocationType
    {
        Normal,
        Mine,        // 광산
        Forest,      // 숲
        DeepSea,     // 심해
        Farm_Island, // 섬 지역
        Farm_Remote  // 오지 및 특수 지역
    }

    // [3] 지역 고유 키값 (Map + Region)
   //public readonly record struct ResourceKey(string MapName, string RegionName);

    // [4] 자원 상세 정의 클래스
    public class ResourceDef
    {
        public Type ItemType { get; set; }
        public double MinSkill { get; set; }
        public double MaxSkill { get; set; }
        public LocationType ReqLoc { get; set; }
        public int Weight { get; set; }

        public ResourceDef(Type type, double min, double max, LocationType reqLoc, int weight)
        {
            ItemType = type; 
            MinSkill = min; 
            MaxSkill = max; 
            ReqLoc = reqLoc; 
            Weight = weight;
        }
    }
}
