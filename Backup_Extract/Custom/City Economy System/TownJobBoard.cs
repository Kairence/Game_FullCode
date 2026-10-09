using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Misc;
using Server.Commands;

namespace Server.Items
{
    public class TownJobBoardItem : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int OverrideTownID { get; set; } = 0;

        [Constructable]
        public TownJobBoardItem() : base(0x1E5E)
        {
            Movable = false;
            Name = "마을 파트타임 게시판";
        }

        public TownJobBoardItem(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from is not PlayerMobile pm)
                return;

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446); // 너무 멉니다.
                return;
            }

            int townID = OverrideTownID > 0 ? OverrideTownID : TownNumber.GetID(this.Location, this.Map);

            if (townID <= 0)
            {
                pm.SendMessage(0x22, "이 게시판은 소속 마을 정보가 설정되어 있지 않습니다.");
                return;
            }

            TownEconomy townEconomy = null;
            if (TownEconomyManager.Towns.ContainsKey(townID))
            {
                townEconomy = TownEconomyManager.Towns[townID];
            }

            if (townEconomy == null)
            {
                townEconomy = new TownEconomy(townID, 0);
                TownEconomyManager.Towns[townID] = townEconomy;    
            }

            if (string.IsNullOrEmpty(townEconomy.TownName))
                townEconomy.TownName = TownNumber.GetName(townID) ?? $"Town_{townID}";

            // 🌟 [제가 낸 버그 수정] if문 밖으로 완전히 빼냈습니다!
            // 이제 기존 마을이든 새 마을이든 게시판을 누르면 무조건 퀘스트가 리필됩니다.
            PartTimeManager.ForceGenerateForTown(townEconomy);

            PartTimeAccountProfile profile = PartTimeManager.GetProfile(pm);
            pm.SendGump(new TownJobBoardGump(pm, profile, townEconomy));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(OverrideTownID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            OverrideTownID = reader.ReadInt();
        }

        // ==============================================================================
        // [jobboard] 명령어: 생성 시 OverrideTownID 자동 주입
        // ==============================================================================
        public static void Initialize()
        {
            CommandSystem.Register("jobboard", AccessLevel.Administrator, new CommandEventHandler(OnGenerateBoards));
        }

        [Usage("jobboard")]
        [Description("트라멜 전 지역의 마을 파트타임 게시판을 일괄 생성하고 마을 ID를 고정합니다.")]
        private static void OnGenerateBoards(CommandEventArgs e)
        {
            var boardData = new (string TownName, int ItemID, int X, int Y, int Z)[]
            {
                ("브리튼", 7774, 1431, 1693, 0),
                ("부케니어스 덴", 7774, 2734, 2192, 0),
                ("코브", 7775, 2233, 1195, 0),
                ("젤롬", 7775, 1328, 3772, 0),
                ("마진시아", 7775, 3727, 2063, 5),
                ("미녹", 7774, 2501, 560, 0),
                ("문글로우", 7774, 4474, 1176, 0),
                ("뉴젤롬", 7775, 3768, 1317, 0),
                ("헤븐", 7775, 3492, 2577, 15),
                ("서펜트 홀드", 7775, 2890, 3480, 15),
                ("스카라 브레", 7774, 596, 2152, 0),
                ("트린식", 7775, 1819, 2826, 0),
                ("베스퍼", 7775, 2896, 678, 0),
                ("윈드", 7774, 5342, 88, 15),
                ("유", 7774, 653, 816, 0),
                ("파푸아", 7775, 5672, 3140, 12),
                ("델루시아", 7774, 5275, 3988, 37)
            };

            int count = 0;
            Map map = Map.Trammel;

            foreach (var data in boardData)
            {
                Point3D loc = new Point3D(data.X, data.Y, data.Z);
                
                // 1. 해당 좌표의 마을 ID를 즉시 계산
                int detectedID = TownNumber.GetID(loc, map);

                // 중복 체크
                bool exists = false;
                IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
                foreach (Item item in eable)
                {
                    if (item is TownJobBoardItem) { exists = true; break; }
                }
                eable.Free();

                if (exists) continue;

                // 2. 게시판 생성 및 ID 주입
                TownJobBoardItem board = new TownJobBoardItem
                {
                    ItemID = data.ItemID,
                    Name = $"{data.TownName} 파트타임 게시판",
                    OverrideTownID = detectedID // 생성 시 마을 ID 고정
                };

                board.MoveToWorld(loc, map);
                
                if (detectedID <= 0)
                {
                    e.Mobile.SendMessage(33, $"경고: {data.TownName} 좌표에서 마을 ID를 인식하지 못했습니다. (좌표 체크 필요)");
                }
                
                count++;
            }

            e.Mobile.SendMessage(63, $"총 {count}개의 마을 게시판이 생성 및 자동 연동되었습니다.");
        }
    }
}