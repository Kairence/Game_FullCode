using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Commands;
using Server.Network;

namespace Server.Misc
{
    public class ResourceAdminGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("ResourceAdmin", AccessLevel.Administrator, new CommandEventHandler(OnCommand));
        }

        [Usage("ResourceAdmin")]
        [Description("서버의 전체 자원 구역 상태를 확인합니다.")]
        private static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new ResourceAdminGump());
        }

        public ResourceAdminGump() : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 700, 500, 5054);
            AddHtml(10, 10, 680, 20, "<CENTER><B>전역 자원 풀(Pool) 모니터링 시스템</B></CENTER>", false, false);

            AddHtml(20, 40, 100, 20, "대륙 (Map)", false, false);
            AddHtml(120, 40, 150, 20, "구역 (Region)", false, false);
            AddHtml(280, 40, 100, 20, "종류 (Type)", false, false);
            AddHtml(390, 40, 100, 20, "기후 (LocType)", false, false);
            AddHtml(500, 40, 150, 20, "잔여량 / 최대량 (%)", false, false);

            int y = 60;
            int page = 1;
            int count = 0;

            AddPage(page);

            foreach (KeyValuePair<ResourceKey, ResourcePool> kvp in ResourceManager.Pools)
            {
                ResourcePool pool = kvp.Value;

                if (count > 0 && count % 20 == 0)
                {
                    AddButton(650, 460, 4005, 4007, 0, GumpButtonType.Page, page + 1);
                    page++;
                    AddPage(page);
                    AddButton(650, 20, 4014, 4016, 0, GumpButtonType.Page, page - 1);
                    y = 60;
                }

                double percent = ((double)pool.CurrentCapacity / pool.MaxCapacity) * 100.0;
                int color = percent < 50.0 ? 33 : percent > 90.0 ? 68 : 0;

                AddLabel(20, y, color, pool.MapName);
                AddLabel(120, y, color, pool.RegionName);
                AddLabel(280, y, color, pool.Type.ToString());
                AddLabel(390, y, color, pool.LocType.ToString());
                AddLabel(500, y, color, $"{pool.CurrentCapacity} / {pool.MaxCapacity} ({percent:F1}%)");

                y += 20;
                count++;
            }
        }
    }
}