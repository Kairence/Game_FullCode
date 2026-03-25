using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    public enum JobTier { Beginner, Intermediate, Advanced, Special }
    public enum JobType { Delivery, Gathering, Hunting }

    public class PartTimeJob
    {
        public string TownName { get; set; }
        public JobTier Tier { get; set; }
        public JobType Type { get; set; }
        public Type NpcType { get; set; } 
        public Type TargetType { get; set; }
        public int RequiredAmount { get; set; }
        public int CurrentAmount { get; set; }
        public bool IsCompleted => CurrentAmount >= RequiredAmount;

        public PartTimeJob(string town, JobTier tier, JobType type, Type npcType, Type targetType, int reqAmount)
        { TownName = town; Tier = tier; Type = type; NpcType = npcType; TargetType = targetType; RequiredAmount = reqAmount; CurrentAmount = 0; }

        public PartTimeJob(GenericReader reader)
        {
            int v = reader.ReadInt(); TownName = reader.ReadString(); Tier = (JobTier)reader.ReadInt(); Type = (JobType)reader.ReadInt();
            string n = reader.ReadString(); if (!string.IsNullOrEmpty(n)) NpcType = ScriptCompiler.FindTypeByFullName(n);
            string t = reader.ReadString(); if (!string.IsNullOrEmpty(t)) TargetType = ScriptCompiler.FindTypeByFullName(t);
            RequiredAmount = reader.ReadInt(); CurrentAmount = reader.ReadInt();
        }

        public void Serialize(GenericWriter writer)
        { writer.Write(0); writer.Write(TownName); writer.Write((int)Tier); writer.Write((int)Type); writer.Write(NpcType?.FullName ?? ""); writer.Write(TargetType?.FullName ?? ""); writer.Write(RequiredAmount); writer.Write(CurrentAmount); }
    }

    public class PartTimeAccountProfile
    {
        public string AccountName { get; set; }
        public int AvailableCharges { get; set; }
        public DateTime CooldownEnd { get; set; }
        public int TotalCompleted { get; set; }
        public PartTimeJob CurrentJob { get; set; }

        public PartTimeAccountProfile(string accountName) { AccountName = accountName; AvailableCharges = 1; CooldownEnd = DateTime.MinValue; TotalCompleted = 0; }
        public PartTimeAccountProfile(GenericReader reader)
        {
            int v = reader.ReadInt(); AccountName = reader.ReadString(); AvailableCharges = reader.ReadInt(); CooldownEnd = reader.ReadDateTime(); TotalCompleted = reader.ReadInt();
            if (reader.ReadBool()) CurrentJob = new PartTimeJob(reader);
        }

        public void Serialize(GenericWriter writer)
        { writer.Write(0); writer.Write(AccountName); writer.Write(AvailableCharges); writer.Write(CooldownEnd); writer.Write(TotalCompleted); writer.Write(CurrentJob != null); if (CurrentJob != null) CurrentJob.Serialize(writer); }
    }

    public static class PartTimeManager
    {
        public static Dictionary<string, PartTimeAccountProfile> Profiles { get; private set; } = new();
        private static DateTime m_LastResetDate;

        public static void Configure() { EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; }
        public static void Initialize() { Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), CheckMidnight); }

        private static void CheckMidnight() { if (m_LastResetDate.Date != DateTime.Now.Date) PerformReset(); }

        private static void PerformReset()
        {
            m_LastResetDate = DateTime.Now.Date;
            foreach (var p in Profiles.Values) p.AvailableCharges = Math.Min(7, p.AvailableCharges + 1);
            Console.WriteLine("PartTimeManager: Daily Reset Done.");
        }

        public static PartTimeAccountProfile GetProfile(Mobile m)
        {
            if (m?.Account == null) return null;
            if (!Profiles.TryGetValue(m.Account.Username, out var p)) Profiles[m.Account.Username] = p = new(m.Account.Username);
            return p;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "PartTimeSystem", "Profiles.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true);
                writer.Write(0); writer.Write(m_LastResetDate); writer.Write(Profiles.Count);
                foreach (var p in Profiles.Values) p.Serialize(writer);
                writer.Close(); // CS1674 해결
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "PartTimeSystem", "Profiles.bin");
            if (!File.Exists(path)) return;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));
                int v = reader.ReadInt(); m_LastResetDate = reader.ReadDateTime(); int count = reader.ReadInt();
                for (int i = 0; i < count; i++) { var p = new PartTimeAccountProfile(reader); Profiles[p.AccountName] = p; }
                reader.Close(); // CS1674 해결
            }
        }
    }
}
