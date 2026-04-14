using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public class RefineFilterSystem
    {
        // 유저별 -> 보석 인덱스별 -> [제외할 옵션 ID 리스트 (우선순위 순서)]
        public static Dictionary<Mobile, Dictionary<int, List<int>>> Profiles = new();

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static Dictionary<int, List<int>> GetProfile(Mobile m)
        {
            if (!Profiles.TryGetValue(m, out var profile))
            {
                profile = new Dictionary<int, List<int>>();
                Profiles[m] = profile;
            }
            return profile;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "RefineFilters");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string file = Path.Combine(path, "Filters.bin");
            
            // GenericWriter를 열고(false: 덮어쓰기 모드)
            GenericWriter writer = new BinaryFileWriter(file, false);
            try
            {
                writer.Write(0); // 버전

                writer.Write(Profiles.Count);
                foreach (var kvp in Profiles)
                {
                    writer.Write(kvp.Key); // Mobile
                    writer.Write(kvp.Value.Count); // 세팅된 보석 개수
                    
                    foreach (var gemKvp in kvp.Value)
                    {
                        writer.Write(gemKvp.Key); // 보석 Index
                        writer.Write(gemKvp.Value.Count); // 제외된 옵션 개수
                        foreach (int opt in gemKvp.Value)
                        {
                            writer.Write(opt); // 옵션 ID
                        }
                    }
                }
            }
            finally
            {
                // [핵심] 버퍼에 남아있는 데이터를 강제로 파일에 쓰고 스트림을 닫습니다.
                // 이걸 빼먹어서 파일 끝이 잘려나가 에러가 발생한 것입니다.
                writer.Close(); 
            }
        }

        private static void OnLoad()
        {
            string file = Path.Combine(Core.BaseDirectory, "Saves", "RefineFilters", "Filters.bin");
            if (!File.Exists(file)) return;

            try
            {
                using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                GenericReader reader = new BinaryFileReader(new BinaryReader(fs));
                
                int version = reader.ReadInt();
                int count = reader.ReadInt();

                for (int i = 0; i < count; i++)
                {
                    Mobile m = reader.ReadMobile();
                    int gemCount = reader.ReadInt();

                    var profile = new Dictionary<int, List<int>>();
                    for (int j = 0; j < gemCount; j++)
                    {
                        int gemIndex = reader.ReadInt();
                        int optCount = reader.ReadInt();
                        
                        List<int> opts = []; // C# 12 문법
                        for (int k = 0; k < optCount; k++)
                        {
                            opts.Add(reader.ReadInt());
                        }
                        profile[gemIndex] = opts;
                    }

                    // Mobile 데이터가 유효할 때만 저장
                    if (m != null) Profiles[m] = profile;
                }
            }
            catch (Exception ex)
            {
                // 데이터가 망가졌더라도 서버가 부팅 중에 다운되는 것을 막습니다.
                Console.WriteLine($"[RefineFilterSystem] 프로필 로드 실패. 기존 파일이 손상되었습니다: {ex.Message}");
            }
        }
    }
}