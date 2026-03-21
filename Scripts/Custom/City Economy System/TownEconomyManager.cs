using System;
using System.Collections.Generic;
using System.IO;
using Server;

namespace Server.Misc
{
    public class TownEconomyManager
    {
        // [★ 분리] 이제 마을 데이터는 여기서 관리합니다.
        public static Dictionary<string, TownEconomy> Towns = new Dictionary<string, TownEconomy>();

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        private static string SavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "TownEconomy.bin");

        public static void OnSave(WorldSaveEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            using (FileStream fs = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(fs, true);

                writer.Write(0); // TownEconomy 전용 버전
                writer.Write(Towns.Count);

                foreach (TownEconomy town in Towns.Values)
                {
                    town.Serialize(writer); // TownEconomy.cs에 작성하신 Serialize 호출
                }

                writer.Close();
            }
            Console.WriteLine($"[{DateTime.Now}] 마을 경제: {Towns.Count}개 지역 데이터 저장 완료.");
        }

        public static void OnLoad()
        {
            if (!File.Exists(SavePath))
            {
                // 세이브 파일이 없으면 XML에서 초기 데이터를 긁어옵니다.
                TownInventoryData.LoadFromXml(); 
                return;
            }

            using (FileStream fs = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(fs));

                int version = reader.ReadInt();
                int count = reader.ReadInt();

                for (int i = 0; i < count; i++)
                {
                    // Deserialize에서 TownName을 읽으므로 생성자엔 빈 값을 넣습니다.
                    TownEconomy town = new TownEconomy("", Point3D.Zero, Map.Internal, 0);
                    town.Deserialize(reader);
                    Towns[town.TownName] = town;
                }

                reader.Close();
            }
            Console.WriteLine($"[{DateTime.Now}] 마을 경제: {Towns.Count}개 지역 데이터 복구 완료.");
        }
    }
}