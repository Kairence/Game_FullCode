using System;
using System.Collections.Generic;
using System.IO;
using Server;

namespace Server.Misc
{
    public enum OutpostType
    {
        FarmStake,
        MiningCamp,
        LumberTent,
        FishingBuoy,
        TanningRack
    }

    public class OutpostInfo
    {
        public Mobile Owner { get; set; }
        public OutpostType Type { get; set; }
        public Point3D Location { get; set; }
        public Map Facet { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public DateTime LastRefreshed { get; set; }

        public bool IsDecayed => DateTime.Now - LastRefreshed > TimeSpan.FromDays(7.0);

        public OutpostInfo(Mobile owner, OutpostType type, Point3D loc, Map map)
        {
            Owner = owner;
            Type = type;
            Location = loc;
            Facet = map;
            CreatedOn = DateTime.Now;
            LastRefreshed = DateTime.Now;
        }

        public void Refresh()
        {
            LastRefreshed = DateTime.Now;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); 
            writer.Write(Owner);
            writer.Write((int)Type);
            writer.Write(Location);
            writer.Write(Facet);
            writer.Write(CreatedOn);
            writer.Write(LastRefreshed);
        }

        public OutpostInfo(GenericReader reader)
        {
            int version = reader.ReadInt();
            Owner = reader.ReadMobile();
            Type = (OutpostType)reader.ReadInt();
            Location = reader.ReadPoint3D();
            Facet = reader.ReadMap();
            CreatedOn = reader.ReadDateTime();
            LastRefreshed = reader.ReadDateTime();
        }
    }

    public static class OutpostManager
    {
        public static List<OutpostInfo> Outposts { get; private set; } = new List<OutpostInfo>();

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void RegisterOutpost(Mobile owner, OutpostType type, Point3D loc, Map map)
        {
            Outposts.Add(new OutpostInfo(owner, type, loc, map));
        }

        public static void RemoveOutpost(OutpostInfo info)
        {
            Outposts.Remove(info);
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string folder = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, "Outposts.bin");
            
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true); // CS1674 해결: using 제거
                
                writer.Write(0); 

                Outposts.RemoveAll(o => o.IsDecayed || o.Owner == null || o.Owner.Deleted);

                writer.Write(Outposts.Count);
                foreach (OutpostInfo info in Outposts)
                {
                    info.Serialize(writer);
                }

                writer.Close(); // 반드시 수동으로 닫아줍니다.
            }
        }

        private static void OnLoad()
        {
            string filePath = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "Outposts.bin");
            if (!File.Exists(filePath))
                return;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream)); // CS1674 해결: using 제거
                
                int version = reader.ReadInt();
                int count = reader.ReadInt();
                
                for (int i = 0; i < count; i++)
                {
                    Outposts.Add(new OutpostInfo(reader));
                }

                reader.Close(); // 반드시 수동으로 닫아줍니다.
            }
        }
    }
}
