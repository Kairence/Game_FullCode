using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;

namespace Server.Misc
{
    public static class VirtualAdventurerManager
    {
        public static List<VirtualAdventurer> IdleAdventurers { get; set; } = [];
        public static List<AdventurerParty> ActiveParties { get; set; } = [];
        
        private static Timer _tickTimer;
        public static DateTime LastTickTime { get; set; }

        // [추가] 모험가 저장 데이터 경로 지정
        private static string SavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "VirtualAdventurers.bin");

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        // [추가] 데이터 파일로 내보내기 (Save)
        private static void OnSave(WorldSaveEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            using (FileStream bin = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                GenericWriter writer = new BinaryFileWriter(bin, true);
                writer.Write(0); // version
                
                writer.Write(IdleAdventurers.Count);
                foreach (var adv in IdleAdventurers) adv.Serialize(writer);

                writer.Write(ActiveParties.Count);
                foreach (var party in ActiveParties) party.Serialize(writer);
                
                writer.Close();
            }
        }

        // [수정] 서버 부팅 시 데이터 불러오기 (Load) 및 초기 셋업
        private static void OnLoad()
        {
            if (File.Exists(SavePath))
            {
                using (FileStream bin = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    GenericReader reader = new BinaryFileReader(new BinaryReader(bin));
                    int version = reader.ReadInt();

                    int idleCount = reader.ReadInt();
                    for (int i = 0; i < idleCount; i++) IdleAdventurers.Add(new VirtualAdventurer(reader));

                    int partyCount = reader.ReadInt();
                    for (int i = 0; i < partyCount; i++) ActiveParties.Add(new AdventurerParty(reader));
                }
            }

            // 로드 후에도 대기열/파티가 비어있다면, 첫 구동이거나 초기화된 상태이므로 스폰 진행
            if (IdleAdventurers.Count == 0 && ActiveParties.Count == 0)
            {
                foreach (var town in TownEconomyManager.Towns.Values)
                {
                    SpawnInitialAdventurers(town, 15);
                }
            }

            _tickTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), GlobalTick);
            Console.WriteLine("[Adventurer] 가상 모험가 시스템 가동 시작 및 데이터 로드 완료.");
        }

        public static void SpawnInitialAdventurers(TownEconomy town, int amount)
        {
            NpcJobClass[] advJobs = 
            [
                NpcJobClass.Knight, NpcJobClass.Paladin, NpcJobClass.Halberdier, NpcJobClass.Assassin, 
                NpcJobClass.Healer_Master, NpcJobClass.Priest, NpcJobClass.Wizard, NpcJobClass.Necromancer, 
                NpcJobClass.Bard, NpcJobClass.Lutanist
            ];

            for (int i = 0; i < amount; i++)
            {
                var job = advJobs[Utility.Random(advJobs.Length)];
                var rank = (NobilityRank)Utility.RandomMinMax((int)NobilityRank.Commoner, (int)NobilityRank.Knight);
                
                var adv = new VirtualAdventurer(job, rank)
                {
                    Gold = Utility.RandomMinMax(2000, 5000)
                };
                
                adv.EquipMissingLayers(town);
                IdleAdventurers.Add(adv);
            }
        }

        public static void GlobalTick()
        {
            LastTickTime = DateTime.Now; 
            if (TownEconomyManager.Towns.Count == 0) return;

            // 1. 모든 활성 파티 상태 모니터링 (복사본 순회로 에러 방지)
            foreach (var party in ActiveParties.ToList()) 
            {
                var currentTown = TownEconomyManager.Towns.Values.FirstOrDefault();
                party.HourlyRoutine(currentTown);

                if (party.Members.Count == 0)
                {
                    ActiveParties.Remove(party);
                    Console.WriteLine("[Adventurer] 파티가 전멸하여 해산되었습니다.");
                    continue;
                }

                // [로그 추가] 파티별 현재 상태 상세 출력
                string leadName = party.Members[0].Name;
                
                switch (party.State)
                {
                    case AdventurerState.Traveling:
                        string from = party.CurrentNode?.Name ?? "Unknown";
                        string to = party.TargetNode?.Name ?? "Unknown";
                        Console.WriteLine($"[행군 중] '{leadName}' 파티: {from} -> {to} 이동 중... (남은 시간: {party.TravelHoursRemaining}시간)");
                        break;

                    case AdventurerState.Exploring:
                        Console.WriteLine($"[탐험 중] '{leadName}' 파티: {party.CurrentNode?.Name} 구역에서 전투 중!");
                        break;

                    case AdventurerState.Resting:
                        Console.WriteLine($"[휴식 중] '{leadName}' 파티: {party.CurrentNode?.Name}에서 정비 및 휴식 중...");
                        break;
                }
            }

            // 2. 신규 파티 결성 (기존 로직 유지)
            // 활성 파티가 너무 많아 로그가 도배되는 것을 막기 위해 파티 수 제한(예: 10개)을 두는 것도 방법입니다.
            if (IdleAdventurers.Count >= 2 && ActiveParties.Count < 10) 
            {
                var towns = TownEconomyManager.Towns.Values.ToList();
                var startTown = towns[Utility.Random(towns.Count)]; 
                WorldNode townNode = new WorldNode(startTown.Name, WorldNodeType.Town, startTown.Facet, startTown.Center, startTown.Center, 1);
                
                var newParty = AdventurerParty.TryFormBalancedParty(IdleAdventurers, townNode);
                if (newParty != null)
                {
                    ActiveParties.Add(newParty);
                    // 초기 던전 설정
                    WorldNode targetDungeon = new WorldNode("Despise", WorldNodeType.Dungeon, Map.Trammel, new Point3D(1295, 1080, 0), new Point3D(5482, 574, 0), 5);
                    newParty.SetDestination(targetDungeon);
                }
            }
        }
    }
}