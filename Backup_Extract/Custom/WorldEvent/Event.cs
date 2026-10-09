using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Items;

namespace Server
{
    public class Event
    {
        // 기존 필드 유지
        public int ServerEvent = 0;
        public bool TGEvent = false;
        public bool VacanceEvent = false;
        public bool ChristmasEvent = false;
        public bool NewyearEvent = false;
        public bool StartEvent = true;
        public static DungeonCheck dungeoncheck = null;
        public static FirstSkillCheck fsc = null;
        public static RespawnCheck rc = null;
        public static DonationCheck dc = null;
        public static LottoCheck lc = null;
        public bool PaintedCaves = false;

        public DateTime PaintedCavesStart = DateTime.Now;
        public int PaintedCavesRound = 1;

        // 신규 시스템 저장소
        public static Dictionary<string, int>[] WeeklyVP { get; private set; }
        public static Dictionary<string, int> HolidayClaims { get; private set; }

        public static void Configure()
        {
            WeeklyVP = new Dictionary<string, int>[4];
            for (int i = 0; i < 4; i++) 
            {
                WeeklyVP[i] = new Dictionary<string, int>();
            }

            HolidayClaims = new Dictionary<string, int>();

            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string folder = Path.Combine("Saves", "Events");
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filePath = Path.Combine(folder, "ServerEvents.bin");

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(fs, true);
                
                writer.Write(1); // 파일 버전 (현재 1)

                // 1. 주간 승점(VP) 데이터 저장
                for (int i = 0; i < 4; i++)
                {
                    writer.Write(WeeklyVP[i].Count);
                    foreach (KeyValuePair<string, int> kvp in WeeklyVP[i])
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value);
                    }
                }

                // 2. 공휴일 보상 수령 데이터 저장
                writer.Write(HolidayClaims.Count);
                foreach (KeyValuePair<string, int> kvp in HolidayClaims)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }

                writer.Flush();
                writer.Close();
            }
        }

        private static void OnLoad()
        {
            string filePath = Path.Combine("Saves", "Events", "ServerEvents.bin");
            if (!File.Exists(filePath))
            {
                return;
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(fs));
                
                try
                {
                    int version = reader.ReadInt();

                    // 데이터 로드 전 기존 딕셔너리 초기화 (중복 로드 방지)
                    for (int i = 0; i < 4; i++) WeeklyVP[i].Clear();
                    HolidayClaims.Clear();

                    // 버전 0 이상의 공통 데이터 (WeeklyVP)
                    if (version >= 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int count = reader.ReadInt();
                            for (int j = 0; j < count; j++)
                            {
                                string key = reader.ReadString();
                                int val = reader.ReadInt();
                                WeeklyVP[i][key] = val;
                            }
                        }
                    }

                    // 버전 1에서 추가된 데이터 (HolidayClaims)
                    // EndOfStream 예외 방지를 위해 잔여 데이터 확인
                    if (version >= 1 && fs.Position < fs.Length)
                    {
                        int holidayCount = reader.ReadInt();
                        for (int j = 0; j < holidayCount; j++)
                        {
                            string key = reader.ReadString();
                            int val = reader.ReadInt();
                            HolidayClaims[key] = val;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Event 시스템 로드 중 오류 발생: " + ex.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public void PaintedCavesEvent(int Stage = 0)
        {
            if (Stage == 1)
            {
                Static stagewall1 = new Static(2272);
                stagewall1.MoveToWorld(new Point3D(6267, 879, 1), Map.Felucca);
                Static stagewall2 = new Static(2272);
                stagewall2.MoveToWorld(new Point3D(6267, 880, 1), Map.Felucca);
                Static stagewall3 = new Static(2272);
                stagewall3.MoveToWorld(new Point3D(6267, 878, -1), Map.Felucca);
                Static stagewall4 = new Static(2272);
                stagewall4.MoveToWorld(new Point3D(6267, 879, -2), Map.Felucca);
                Static stagewall5 = new Static(2272);
                stagewall5.MoveToWorld(new Point3D(6267, 880, -1), Map.Felucca);
            }
        }
    }
}