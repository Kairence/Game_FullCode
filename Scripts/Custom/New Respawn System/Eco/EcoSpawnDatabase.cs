using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    // 스폰할 몬스터/동물의 타입과 출현 확률(Weight)을 정의
    public readonly record struct EcoSpawnDef(Type MobType, int Weight);

    public static class EcoSpawnDatabase
    {
        // 1. [바이옴] 기후(Climate)와 용도(Area)에 따른 범용 야생 풀
        private static readonly Dictionary<(EcoAreaType, EcoClimateType), List<EcoSpawnDef>> Biomes = new();

        // 2. [키워드 랜드마크] 구역 이름에 특정 단어("orc", "swamp")가 포함되면 발동
        private static readonly Dictionary<string, List<EcoSpawnDef>> KeywordLandmarks = new();

        // 🌟 3. [초희귀 랜드마크] 전 대륙에서 딱 "지정된 구역 이름"과 100% 일치할 때만 발동!
        private static readonly Dictionary<string, List<EcoSpawnDef>> ExactLandmarks = new();

        // 🌟 [추가] 가장 무난한 폴백 풀 (매번 생성하지 않고 캐싱)
        private static List<EcoSpawnDef> m_FallbackPool;

        public static void Initialize()
        {
            SetupBiomes();
            SetupKeywordLandmarks();
            SetupExactLandmarks(); // 🌟 신규: 유니콘, 기린 등 희귀종 전용

            // 최후의 보루 풀을 메모리에 고정 캐싱
            Biomes.TryGetValue((EcoAreaType.Forest, EcoClimateType.Temperate), out m_FallbackPool);
        }

        // ==============================================================================
        // 🦄 1. [초희귀/특수 구역] 전 대륙 통틀어 딱 지정된 곳에서만 등장!
        // ==============================================================================
        private static void SetupExactLandmarks()
        {
            // [예시 1] 트라멜 영성(Spirituality) 신전
            // 새(5000), 토끼(4990)가 쏟아져 나오지만, 0.1%(10)의 확률로 '유니콘'이 스폰됨!
            AddExact("trammel_shrine_spirituality", 
                new(typeof(Bird), 5000), new(typeof(Rabbit), 4990), new(typeof(Unicorn), 10));

            // [예시 2] 일쉐나 명예(Honor) 신전
            // 여기서는 유니콘 대신 '기린(Kirin)'이 극도로 희귀하게 등장
            AddExact("ilshenar_shrine_honor", 
                new(typeof(Bird), 5000), new(typeof(Rabbit), 4990), new(typeof(Kirin), 10));

            // [예시 3] 펠루카 로스트랜드 특수 늪지대 (Hopper's Bog)
            // 일반 늪지대 바이옴을 무시하고, 오직 이곳에서만 '실버 서펀트'와 '거대 두꺼비' 군락 형성
            AddExact("felucca_lostlands_hoppersbog",
                new(typeof(Alligator), 500), new(typeof(GiantToad), 400), new(typeof(SilverSerpent), 100));
        }

        // ==============================================================================
        // 🏰 2. [키워드 랜드마크] 이름에 단어만 포함되어도 발동 (범용 던전/캠프)
        // ==============================================================================
        private static void SetupKeywordLandmarks()
        {
            // 오크 캠프 (모든 맵의 "orc"가 들어간 구역)
            AddKeyword("orc", new(typeof(Orc), 40), new(typeof(OrcCaptain), 10), new(typeof(OrcishMage), 5));
            
            // 가고일 시티/폐허
            AddKeyword("gargoyle", new(typeof(Gargoyle), 30), new(typeof(StoneGargoyle), 10));

            // 요모츠 광산
            AddKeyword("yomotsu", new(typeof(YomotsuWarrior), 20), new(typeof(YomotsuPriest), 5));

            // 일반 신전들 (영성/명예 신전을 제외한 나머지 평범한 신전들)
            AddKeyword("shrine", new(typeof(Wisp), 10), new(typeof(Pixie), 10), new(typeof(Bird), 80));
        }

        // ==============================================================================
        // 🌍 3. [바이옴] 기후와 용도에 따른 밑바탕 야생 생태계
        // ==============================================================================
        private static void SetupBiomes()
        {
            // 🏡 Town (마을)
            AddBiome(EcoAreaType.Town, EcoClimateType.Temperate, new(typeof(Cat), 20), new(typeof(Dog), 20), new(typeof(Bird), 60));
            AddBiome(EcoAreaType.Town, EcoClimateType.Desert, new(typeof(Cat), 20), new(typeof(DesertOstard), 10));

            // 🌲 Forest (숲/벌목지)
            AddBiome(EcoAreaType.Forest, EcoClimateType.Temperate, 
                new(typeof(Rabbit), 300), new(typeof(Hind), 200), new(typeof(GreatHart), 100), new(typeof(BlackBear), 50), new(typeof(TimberWolf), 50));
            
            AddBiome(EcoAreaType.Forest, EcoClimateType.Arctic, 
                new(typeof(SnowLeopard), 100), new(typeof(PolarBear), 100), new(typeof(Walrus), 150));

            AddBiome(EcoAreaType.Forest, EcoClimateType.Tropical, 
                new(typeof(Panther), 150), new(typeof(Gorilla), 100), new(typeof(TropicalBird), 200));

            // ⚔️ Hunting (사냥터)
            AddBiome(EcoAreaType.Hunting, EcoClimateType.Temperate, new(typeof(DireWolf), 20), new(typeof(GrizzlyBear), 15), new(typeof(GiantSpider), 10));
            AddBiome(EcoAreaType.Hunting, EcoClimateType.Swamp, new(typeof(Alligator), 20), new(typeof(Slime), 15), new(typeof(BogThing), 5));
            AddBiome(EcoAreaType.Hunting, EcoClimateType.Desert, new(typeof(Scorpion), 25), new(typeof(Snake), 20), new(typeof(GiantSerpent), 5));
            AddBiome(EcoAreaType.Hunting, EcoClimateType.Volcanic, new(typeof(LavaLizard), 20), new(typeof(FireElemental), 10), new(typeof(HellHound), 15));
            AddBiome(EcoAreaType.Hunting, EcoClimateType.Void, new(typeof(MyrmidexDrone), 20), new(typeof(Najasaurus), 5));
        }

        // --- 내부 헬퍼 메서드 ---
        private static void AddExact(string exactName, params EcoSpawnDef[] mobs)
        {
            // 🌟 RegionCode 치환 매칭을 위해 언더바(_)와 공백을 모두 날려버리고 등록
            ExactLandmarks[exactName.ToLower().Replace("_", "").Replace(" ", "")] = new List<EcoSpawnDef>(mobs);
        }

        private static void AddKeyword(string keyword, params EcoSpawnDef[] mobs)
        {
            KeywordLandmarks[keyword.ToLower().Replace("_", "").Replace(" ", "")] = new List<EcoSpawnDef>(mobs);
        }

        private static void AddBiome(EcoAreaType area, EcoClimateType climate, params EcoSpawnDef[] mobs)
        {
            if (!Biomes.ContainsKey((area, climate))) Biomes[(area, climate)] = new List<EcoSpawnDef>();
            Biomes[(area, climate)].AddRange(mobs);
        }

        // ==============================================================================
        // 🌟 [최적화 마스터 엔진 1] 노드가 태어날 때(생성/로드) 딱 한 번만 불려서 배열 포인터를 캐싱!
        // ==============================================================================
        public static List<EcoSpawnDef> GetPoolFor(EcoNode node)
        {
            if (node.RCode == RegionCode.None)
            {
                if (Biomes.TryGetValue((node.AreaType, node.ClimateType), out var fallbackBiome))
                    return fallbackBiome;
                return m_FallbackPool;
            }

            // RegionCode Enum을 소문자로 만들고 언더바 제거 (예: trammelshrinehonor)
            string lowerZone = node.RCode.ToString().ToLower().Replace("_", "");

            // 🌟 1순위: [초희귀/특수 구역]
            if (ExactLandmarks.TryGetValue(lowerZone, out var exactPool))
                return exactPool;

            // 🌟 2순위: [키워드 구역]
            foreach (var kvp in KeywordLandmarks)
            {
                if (lowerZone.Contains(kvp.Key)) return kvp.Value;
            }

            // 🌟 3순위: 일반 [바이옴(기후+용도)] 환경 적용
            if (Biomes.TryGetValue((node.AreaType, node.ClimateType), out var biomePool))
                return biomePool;

            // 최후의 보루
            return m_FallbackPool;
        }

        // ==============================================================================
        // 🌟 [최적화 마스터 엔진 2] 매 틱마다 노드가 부르는 고속 룰렛 (문자열 연산 Zero)
        // ==============================================================================
        public static Type RollFromPool(List<EcoSpawnDef> pool)
        {
            // 안전장치
            if (pool == null || pool.Count == 0) return typeof(Rabbit);

            // =========================================================
            // 🎲 확률(Weight) 기반 룰렛 돌리기 (극악 확률 스폰 구현)
            // =========================================================
            int totalWeight = pool.Sum(p => p.Weight);
            int roll = Utility.Random(totalWeight);
            int current = 0;

            foreach (var def in pool)
            {
                current += def.Weight;
                if (roll < current) return def.MobType;
            }

            return pool[0].MobType;
        }
    }
}