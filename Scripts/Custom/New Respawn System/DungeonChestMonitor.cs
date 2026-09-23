using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Commands;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public static class DungeonChestMonitor
    {
        // 던전 구역별 상자 리스트 명부
        public static Dictionary<RegionCode, List<LockableContainer>> ActiveChests = new Dictionary<RegionCode, List<LockableContainer>>();

        private static string SavePath => Path.Combine("Saves", "Custom", "DungeonChests.bin");

        public static void Initialize()
        {
            CommandSystem.Register("ChestMonitor", AccessLevel.GameMaster, new CommandEventHandler(OnChestMonitorCommand));
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        // ==============================================================================
        // [로직] 상자 등록 및 조회 (자동 정리 포함)
        // ==============================================================================
        public static void RegisterChest(RegionCode code, LockableContainer chest)
        {
            if (!ActiveChests.ContainsKey(code))
                ActiveChests[code] = new List<LockableContainer>();

            if (!ActiveChests[code].Contains(chest))
                ActiveChests[code].Add(chest);
        }

        public static List<LockableContainer> GetValidChests(RegionCode code)
        {
            if (!ActiveChests.ContainsKey(code))
                return new List<LockableContainer>();

            // 삭제(파괴/루팅)되거나 널이 된 상자를 리스트에서 자동 정리(Prune)
            ActiveChests[code].RemoveAll(c => c == null || c.Deleted);
            
            return ActiveChests[code];
        }

        // ==============================================================================
        // [세이브 & 로드] 상자 인스턴스 유지
        // ==============================================================================
        private static void OnSave(WorldSaveEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            using (FileStream fs = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write((int)0); // Version

                // 유효한 상자만 남기기 위해 전체 정리 수행
                var validKeys = ActiveChests.Keys.ToList();
                foreach (var k in validKeys)
                    ActiveChests[k].RemoveAll(c => c == null || c.Deleted);

                writer.Write(ActiveChests.Count);
                foreach (var kvp in ActiveChests)
                {
                    writer.Write((int)kvp.Key);
                    writer.Write(kvp.Value.Count);
                    foreach (var chest in kvp.Value)
                    {
                        writer.Write(chest.Serial.Value);
                    }
                }
            }
        }

        private static void OnLoad()
        {
            if (!File.Exists(SavePath)) return;

            using (FileStream fs = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                int version = reader.ReadInt32();

                int dictCount = reader.ReadInt32();
                for (int i = 0; i < dictCount; i++)
                {
                    RegionCode code = (RegionCode)reader.ReadInt32();
                    int listCount = reader.ReadInt32();
                    
                    List<LockableContainer> chestList = new List<LockableContainer>();
                    for (int j = 0; j < listCount; j++)
                    {
                        int serialVal = reader.ReadInt32();
                        Item item = World.FindItem(serialVal);
                        if (item != null && item is LockableContainer chest && !chest.Deleted)
                        {
                            chestList.Add(chest);
                        }
                    }
                    if (chestList.Count > 0)
                        ActiveChests[code] = chestList;
                }
            }
        }

        // ==============================================================================
        // [모니터링 UI] 관리자 테스트용 명령어
        // ==============================================================================
        [Usage("ChestMonitor")]
        [Description("던전 내 가상 모험가 보물상자 현황을 모니터링합니다.")]
        private static void OnChestMonitorCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new ChestMonitorGump());
        }
    }

    public class ChestMonitorGump : Gump
    {
        public ChestMonitorGump() : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 700, 500, 9270);
            AddHtml(0, 15, 700, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>던전 보물상자(모험가 무덤) 모니터링 센터</CENTER>", false, false);

            int y = 50;
            AddHtml(20, y, 150, 20, "<BASEFONT COLOR=#FFFF00>던전(Region)</BASEFONT>", false, false);
            AddHtml(180, y, 100, 20, "<BASEFONT COLOR=#FFFF00>좌표</BASEFONT>", false, false);
            AddHtml(300, y, 100, 20, "<BASEFONT COLOR=#FFFF00>자물쇠 렙</BASEFONT>", false, false);
            AddHtml(400, y, 100, 20, "<BASEFONT COLOR=#FFFF00>함정 데미지</BASEFONT>", false, false);
            AddHtml(520, y, 100, 20, "<BASEFONT COLOR=#FFFF00>누적 골드</BASEFONT>", false, false);
            AddHtml(630, y, 50, 20, "<BASEFONT COLOR=#FFFF00>이동</BASEFONT>", false, false);

            y += 25;
            int count = 0;

            foreach (var kvp in DungeonChestMonitor.ActiveChests)
            {
                var validChests = DungeonChestMonitor.GetValidChests(kvp.Key);
                foreach (var chest in validChests)
                {
                    if (y > 450) break; // 간단한 페이징 생략 (상단 노출용)

                    AddHtml(20, y, 150, 20, $"<BASEFONT COLOR=#FFFFFF>{kvp.Key}</BASEFONT>", false, false);
                    AddHtml(180, y, 100, 20, $"<BASEFONT COLOR=#FFFFFF>{chest.X}, {chest.Y}</BASEFONT>", false, false);
                    AddHtml(300, y, 100, 20, $"<BASEFONT COLOR=#FFFFFF>{chest.LockLevel}</BASEFONT>", false, false);
                    AddHtml(400, y, 100, 20, $"<BASEFONT COLOR=#FFFFFF>{chest.TrapPower}</BASEFONT>", false, false);
                    
                    int goldAmount = chest.Items.OfType<Gold>().Sum(g => g.Amount);
                    AddHtml(520, y, 100, 20, $"<BASEFONT COLOR=#00FF00>{goldAmount}g</BASEFONT>", false, false);
                    
                    AddButton(630, y, 4005, 4007, 1000 + chest.Serial.Value, GumpButtonType.Reply, 0);
                    
                    y += 25;
                    count++;
                }
            }
            if (count == 0)
                AddHtml(20, y, 400, 20, "<BASEFONT COLOR=#AAAAAA>현재 활성화된 던전 보물상자가 없습니다.</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID >= 1000)
            {
                int serialVal = info.ButtonID - 1000;
                Item chest = World.FindItem(serialVal);
                if (chest != null && !chest.Deleted)
                {
                    sender.Mobile.MoveToWorld(chest.Location, chest.Map);
                    sender.Mobile.SendMessage("해당 잭팟 보물상자로 이동했습니다.");
                    sender.Mobile.SendGump(new ChestMonitorGump()); // 이동 후 다시 열어줌
                }
                else
                {
                    sender.Mobile.SendMessage("해당 상자는 누군가 루팅했거나 이미 파괴되었습니다.");
                }
            }
        }
    }
}
