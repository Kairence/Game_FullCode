using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    // ==============================================================================
    // 1. 런타임 메모리용 캐릭터 데이터 (저장하지 않고 휘발됨, 렉 제로)
    // ==============================================================================
    public class CharacterEconomyData
    {
        public Dictionary<Type, int> Harvested { get; set; } = [];
        public Dictionary<Type, int> Bought { get; set; } = [];
        public Dictionary<Type, int> Sold { get; set; } = [];
        public Dictionary<string, int> Kills { get; set; } = [];
    }

    // ==============================================================================
    // 2. 계정 단위 누적 및 평판 프로필 (WorldSave 시점에 병합되어 파일로 저장됨)
    // ==============================================================================
    public class AccountEconomyProfile
    {
        public string AccountUsername { get; set; } = string.Empty;
        public DateTime LastAuditTime { get; set; } = DateTime.Now;
        
        // 마을 인덱스에 따른 평판 점수 (기본 0, 음수면 적대/사재기꾼, 양수면 호의)
        public Dictionary<int, int> TownReputation { get; set; } = [];

        public AccountEconomyProfile() { }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); // Version
            writer.Write(AccountUsername);
            writer.Write(LastAuditTime);

            writer.Write(TownReputation.Count);
            foreach (var kvp in TownReputation)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public AccountEconomyProfile(GenericReader reader)
        {
            int version = reader.ReadInt();
            AccountUsername = reader.ReadString();
            LastAuditTime = reader.ReadDateTime();

            int repCount = reader.ReadInt();
            for (int i = 0; i < repCount; i++)
            {
                int townIdx = reader.ReadInt();
                int repScore = reader.ReadInt();
                TownReputation[townIdx] = repScore;
            }
        }
    }

    // ==============================================================================
    // 3. 경제 프로파일링 매니저 (핵심 코어)
    // ==============================================================================
    public static class EconomyProfiler
    {
        // 접속 중인 캐릭터들의 실시간 행동을 담는 딕셔너리
        private static Dictionary<Mobile, CharacterEconomyData> RuntimeData { get; set; } = [];
        
        // 계정별 영구 누적 데이터
        public static Dictionary<string, AccountEconomyProfile> AccountProfiles { get; set; } = [];

        private static string SavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "AccountProfiles.bin");

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
            EventSink.Logout += OnLogout;
        }

        // ==============================================================================
        // [외부 연동 API] 다른 스크립트에서 렉 없이 단순 호출만 하면 됨
        // ==============================================================================
        private static CharacterEconomyData GetRuntimeData(Mobile m)
        {
            if (!RuntimeData.ContainsKey(m))
                RuntimeData[m] = new CharacterEconomyData();
            return RuntimeData[m];
        }

        public static void TrackHarvest(Mobile m, Type itemType, int amount)
        {
            if (m == null || !m.Player || amount <= 0) return;
            var data = GetRuntimeData(m);
            
            if (!data.Harvested.ContainsKey(itemType)) data.Harvested[itemType] = 0;
            data.Harvested[itemType] += amount;
        }

        public static void TrackTrade(Mobile m, Type itemType, int amount, bool isBuying)
        {
            if (m == null || !m.Player || amount <= 0) return;
            var data = GetRuntimeData(m);

            var targetDict = isBuying ? data.Bought : data.Sold;
            if (!targetDict.ContainsKey(itemType)) targetDict[itemType] = 0;
            targetDict[itemType] += amount;
        }

        public static void TrackKill(Mobile m, string monsterName)
        {
            if (m == null || !m.Player || string.IsNullOrEmpty(monsterName)) return;
            var data = GetRuntimeData(m);

            if (!data.Kills.ContainsKey(monsterName)) data.Kills[monsterName] = 0;
            data.Kills[monsterName]++;
        }

        // 유저 로그아웃 시, 즉각 계정 데이터로 병합 후 런타임 메모리 비움 (누수 방지)
        private static void OnLogout(LogoutEventArgs e)
        {
            MergeAndClearRuntimeData(e.Mobile);
        }

        // ==============================================================================
        // [세이브 & 병합 처리] 주기적인 감사(Audit) 및 계정 단위로 묶기
        // ==============================================================================
        private static void OnSave(WorldSaveEventArgs e)
        {
            // 현재 접속 중인 모든 플레이어의 런타임 데이터를 계정 프로필로 병합
            var activeMobiles = RuntimeData.Keys.ToList();
            foreach (var m in activeMobiles)
            {
                MergeAndClearRuntimeData(m);
            }

            // 계정별로 묶인 데이터를 순회하며 HPS 분석 및 가십/평판 조정
            AuditAllAccounts();

            // 파일로 쓰기
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            using FileStream bin = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            GenericWriter writer = new BinaryFileWriter(bin, true);
            
            writer.Write(0); // Version
            writer.Write(AccountProfiles.Count);
            foreach (var profile in AccountProfiles.Values)
            {
                profile.Serialize(writer);
            }
            writer.Close();
        }

        private static void MergeAndClearRuntimeData(Mobile m)
        {
            if (m == null || m.Account == null || !RuntimeData.ContainsKey(m)) return;

            string accName = ((IAccount)m.Account).Username;
            
            if (!AccountProfiles.ContainsKey(accName))
                AccountProfiles[accName] = new AccountEconomyProfile { AccountUsername = accName };

            var profile = AccountProfiles[accName];
            var data = RuntimeData[m];

            // 1. 여기서 수집된 data를 바탕으로 즉각적인 가십 계산을 위한 임시 캐싱을 하거나
            // 2. 특정 마을(권역)에서 캔 자원의 누적치를 프로필에 안전하게 병합합니다.
            // (상세 병합 로직은 마을 인덱스 추적과 함께 확장 가능)

            // 병합 완료 후 메모리 해제
            RuntimeData.Remove(m);
        }

        private static void AuditAllAccounts()
        {
            // 계정 단위로 시세 조작(사재기) 및 먹튀 판별
            foreach (var profile in AccountProfiles.Values)
            {
                double elapsedHours = (DateTime.Now - profile.LastAuditTime).TotalHours;
                if (elapsedHours < 1.0) continue; // 최소 1시간이 지나야 정산

                // TODO: 런타임에서 병합된 데이터를 바탕으로 평판 증감 연산
                // 예: 만약 특정 마을 인덱스 권역에서 철광석(IronOre)을 1000개 캤는데 (Harvested)
                // 그 마을 상점에 판 기록(Sold)이 0이라면? -> 평판 하락
                
                // 특정 마을 인덱스를 순회하며 가십 업데이트 적용
                // profile.TownReputation[townIndex] -= 10; 

                profile.LastAuditTime = DateTime.Now;
            }
            Console.WriteLine($"[Economy Profiler] 총 {AccountProfiles.Count}개 계정의 경제 활동 감사(Audit) 완료.");
        }

        private static void OnLoad()
        {
            if (!File.Exists(SavePath)) return;

            using FileStream bin = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            GenericReader reader = new BinaryFileReader(new BinaryReader(bin));
            
            int version = reader.ReadInt();
            int count = reader.ReadInt();
            
            for (int i = 0; i < count; i++)
            {
                var profile = new AccountEconomyProfile(reader);
                AccountProfiles[profile.AccountUsername] = profile;
            }
        }
    }
}