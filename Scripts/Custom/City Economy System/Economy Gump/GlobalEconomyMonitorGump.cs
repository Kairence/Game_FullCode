using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Multis;

namespace Server.Misc
{
    public class GlobalEconomyMonitorGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("EcoMonitor", AccessLevel.Administrator, new CommandEventHandler(OnCommand));
        }

        [Usage("EcoMonitor")]
        [Description("글로벌 골드 및 실물 자원 대시보드를 엽니다.")]
        private static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new GlobalEconomyMonitorGump(e.Mobile));
        }

        private const int ColorWhite = 1152;
        private const int ColorGold = 53;
        private const int ColorGreen = 68;
        private const int ColorRed = 33;
        private const int ColorLightBlue = 89;

        public GlobalEconomyMonitorGump(Mobile from) : base(50, 50)
        {
            from.CloseGump(typeof(GlobalEconomyMonitorGump));

            AddBackground(0, 0, 800, 600, 9270);
            AddAlphaRegion(10, 10, 780, 580);

            // 공통 헤더 및 뒤로가기 연동 버튼
            AddButton(20, 20, 0x15E3, 0x15E7, 888, GumpButtonType.Reply, 0); 
            AddLabel(45, 19, ColorGold, "◀ NPC 경제 관리로 돌아가기");

            AddImageTiled(20, 45, 760, 2, 9651);
            AddLabel(320, 50, ColorGold, "◆ 거시 경제 모니터링 보드 ◆");
            AddImageTiled(20, 75, 760, 2, 9651);

            // ==============================================================================
            // [Page 1: 골드(화폐) 경제 모니터]
            // ==============================================================================
            AddPage(1);
            AddButton(680, 48, 0x15E1, 0x15E5, 0, GumpButtonType.Page, 2); 
            AddLabel(705, 47, ColorGreen, "자원(재료) 보기 ▶");

            RenderGoldPage();

            // ==============================================================================
            // [Page 2: 실물 자원(재료) 모니터]
            // ==============================================================================
            AddPage(2);
            AddButton(25, 48, 0x15E3, 0x15E7, 0, GumpButtonType.Page, 1);
            AddLabel(45, 47, ColorGold, "◀ 골드(화폐) 보기");

            RenderResourcePage();
        }

        private void RenderGoldPage()
        {
            long totalTownWealth = TownEconomyManager.Towns.Values.Sum(t => (long)t.Wealth);

            var userGoldList = new List<(string Name, long Wealth)>();
            foreach (var p in World.Mobiles.Values.OfType<PlayerMobile>())
            {
                userGoldList.Add((p.Name, GetTotalGold(p)));
            }

            var topUsers = userGoldList.OrderByDescending(u => u.Wealth).Take(10).ToList();
            long top10UserWealth = topUsers.Sum(u => u.Wealth);

            long totalCitizenWealth = 0;
            long totalAdventurerWealth = 0;
            var rankWealth = new Dictionary<NobilityRank, long>();
            foreach (NobilityRank rank in Enum.GetValues(typeof(NobilityRank))) rankWealth[rank] = 0;

            foreach (var town in TownEconomyManager.Towns.Values)
            {
                foreach (var citizen in town.Citizens)
                {
                    rankWealth[citizen.RankLevel] += citizen.Gold;
                    totalCitizenWealth += citizen.Gold;
                }
            }

            if (VirtualAdventurerManager.ActiveParties != null)
            {
                foreach (var party in VirtualAdventurerManager.ActiveParties)
                    foreach (var adv in party.Members)
                        totalAdventurerWealth += adv.Gold;
            }

            long totalNpcWealth = totalCitizenWealth + totalAdventurerWealth;
            long serverTotalWealth = totalTownWealth + top10UserWealth + totalNpcWealth;

            AddLabel(30, 95, ColorLightBlue, "[ 3대 화폐 경제 주체 총합 (Macro Indicators) ]");
            AddLabel(50, 125, ColorWhite, "마을 금고 총합 (Towns):");     AddLabel(220, 125, ColorGold, $"{totalTownWealth:N0} GP");
            AddLabel(300, 125, ColorWhite, "랭킹 10위 유저 (Top 10):");   AddLabel(480, 125, ColorGold, $"{top10UserWealth:N0} GP");
            AddLabel(550, 125, ColorWhite, "NPC 총 자산 (NPCs):");        AddLabel(700, 125, ColorGold, $"{totalNpcWealth:N0} GP");
            
            AddLabel(50, 165, ColorGreen, $"▶ 서버 내 유통 중인 총 골드 추산: {serverTotalWealth:N0} GP");

            AddImageTiled(20, 200, 760, 2, 9651);
            AddLabel(30, 215, ColorLightBlue, "[ NPC 생태계 부의 분배 현황 ]");

            int y = 245;
            AddLabel(50, y, ColorGold, "계층 (Class)"); AddLabel(250, y, ColorGold, "자본금 (Wealth)"); AddLabel(450, y, ColorGold, "비율 (%)");
            y += 25;

            foreach (var kvp in rankWealth.OrderByDescending(k => k.Value))
            {
                if (kvp.Value == 0) continue;
                double ratio = totalNpcWealth > 0 ? (double)kvp.Value / totalNpcWealth * 100 : 0;
                AddLabel(50, y, ColorWhite, $"{kvp.Key}"); AddLabel(250, y, ColorWhite, $"{kvp.Value:N0} GP"); AddLabel(450, y, ColorWhite, $"{ratio:F1} %");
                y += 20;
            }

            y += 10;
            double advRatio = totalNpcWealth > 0 ? (double)totalAdventurerWealth / totalNpcWealth * 100 : 0;
            AddLabel(50, y, 43, "모험가 (Adventurers)"); AddLabel(250, y, 43, $"{totalAdventurerWealth:N0} GP"); AddLabel(450, y, 43, $"{advRatio:F1} %");

            y += 40;
            AddImageTiled(20, y, 760, 2, 9651);
            AddLabel(30, y + 15, ColorLightBlue, "[ 마을별 재정 현황 ]");
            y += 45;

            AddLabel(50, y, ColorGold, "마을 이름"); AddLabel(250, y, ColorGold, "보유 자본 (Wealth)"); AddLabel(450, y, ColorGold, "인구 (시민)");
            y += 25;

            var towns = TownEconomyManager.Towns.Values.OrderByDescending(t => t.Wealth).Take(7).ToList();
            foreach (var town in towns)
            {
                AddLabel(50, y, ColorWhite, $"{(town.Facet != null ? town.Facet.Name : "Unknown")} Town");
                AddLabel(250, y, town.Wealth < town.BaseWealth * 0.5 ? ColorRed : ColorWhite, $"{town.Wealth:N0} GP");
                AddLabel(450, y, ColorWhite, $"{town.Citizens.Count} 명");
                y += 20;
            }

            AddCommonButtons();
        }

        private void RenderResourcePage()
        {
            var userResources = new List<(string Name, long Ores, long Ingots, long Logs, long Boards, long Hides, long Leathers)>();
            
            foreach (var p in World.Mobiles.Values.OfType<PlayerMobile>())
            {
                long ores = GetDeepItemAmount(p, typeof(BaseOre));
                long ingots = GetDeepItemAmount(p, typeof(BaseIngot));
                long logs = GetDeepItemAmount(p, typeof(Log));
                long boards = GetDeepItemAmount(p, typeof(Board));
                // [수정] Hide -> Hides 로 변경
                long hides = GetDeepItemAmount(p, typeof(Hides));
                long leathers = GetDeepItemAmount(p, typeof(BaseLeather));

                if ((ores + ingots + logs + boards + hides + leathers) > 0)
                {
                    userResources.Add((p.Name, ores, ingots, logs, boards, hides, leathers));
                }
            }

            long tUserOres = userResources.Sum(u => u.Ores);
            long tUserIngots = userResources.Sum(u => u.Ingots);
            long tUserLogs = userResources.Sum(u => u.Logs);
            long tUserBoards = userResources.Sum(u => u.Boards);
            long tUserHides = userResources.Sum(u => u.Hides);
            long tUserLeathers = userResources.Sum(u => u.Leathers);

            long tTownOres = 0, tTownIngots = 0, tTownLogs = 0, tTownBoards = 0, tTownHides = 0, tTownLeathers = 0;

            foreach (var town in TownEconomyManager.Towns.Values)
            {
                foreach (var wItem in town.Warehouse.Values)
                {
                    Type t = wItem.ItemType;
                    if (t.IsSubclassOf(typeof(BaseOre)) || t == typeof(BaseOre)) tTownOres += wItem.Stock;
                    else if (t.IsSubclassOf(typeof(BaseIngot)) || t == typeof(BaseIngot)) tTownIngots += wItem.Stock;
                    else if (t.IsSubclassOf(typeof(Log)) || t == typeof(Log)) tTownLogs += wItem.Stock;
                    else if (t.IsSubclassOf(typeof(Board)) || t == typeof(Board)) tTownBoards += wItem.Stock;
                    // [수정] Hide -> Hides 로 변경
                    else if (t.IsSubclassOf(typeof(Hides)) || t == typeof(Hides)) tTownHides += wItem.Stock;
                    else if (t.IsSubclassOf(typeof(BaseLeather)) || t == typeof(BaseLeather)) tTownLeathers += wItem.Stock;
                }
            }

            AddLabel(30, 95, ColorLightBlue, "[ 글로벌 실물 자원 비축량 (가공 전 / 가공 후 분리) ]");
            
            int y = 125;
            AddLabel(150, y, ColorGold, "유저 (딥스캔)"); AddLabel(320, y, ColorGold, "마을 (Warehouse)");
            AddLabel(470, y, ColorGold, "유저 (딥스캔)"); AddLabel(640, y, ColorGold, "마을 (Warehouse)");
            y += 25;

            AddLabel(50, y, ColorWhite, "광석 (Ores):"); 
            AddLabel(150, y, ColorWhite, $"{tUserOres:N0}"); AddLabel(320, y, ColorWhite, $"{tTownOres:N0}");
            AddLabel(400, y, ColorWhite, "▶ 잉곳 (Ingots):"); 
            AddLabel(470, y, ColorWhite, $"{tUserIngots:N0}"); AddLabel(640, y, ColorWhite, $"{tTownIngots:N0}");
            y += 20;
            
            AddLabel(50, y, ColorWhite, "나무 (Logs):"); 
            AddLabel(150, y, ColorWhite, $"{tUserLogs:N0}"); AddLabel(320, y, ColorWhite, $"{tTownLogs:N0}");
            AddLabel(400, y, ColorWhite, "▶ 판자 (Boards):"); 
            AddLabel(470, y, ColorWhite, $"{tUserBoards:N0}"); AddLabel(640, y, ColorWhite, $"{tTownBoards:N0}");
            y += 20;
            
            AddLabel(50, y, ColorWhite, "원피 (Hides):"); 
            AddLabel(150, y, ColorWhite, $"{tUserHides:N0}"); AddLabel(320, y, ColorWhite, $"{tTownHides:N0}");
            AddLabel(400, y, ColorWhite, "▶ 가죽 (Leathers):"); 
            AddLabel(470, y, ColorWhite, $"{tUserLeathers:N0}"); AddLabel(640, y, ColorWhite, $"{tTownLeathers:N0}");

            AddImageTiled(20, 225, 760, 2, 9651);
            AddLabel(30, 240, ColorLightBlue, "[ 계정별 자원 비축량 TOP 10 (집+벤더 포함 딥스캔) ]");

            y = 270;
            AddLabel(40, y, ColorGold, "유저명");
            AddLabel(180, y, ColorGold, "Ores");
            AddLabel(260, y, ColorGold, "Ingots");
            AddLabel(350, y, ColorGold, "Logs");
            AddLabel(440, y, ColorGold, "Boards");
            AddLabel(530, y, ColorGold, "Hides");
            AddLabel(620, y, ColorGold, "Leathers");
            y += 25;

            var topHoarders = userResources.OrderByDescending(u => u.Ores + u.Ingots + u.Logs + u.Boards + u.Hides + u.Leathers).Take(10).ToList();

            if (topHoarders.Count == 0)
            {
                AddLabel(40, y, 999, "기록된 자원 데이터가 없습니다.");
            }
            else
            {
                foreach (var h in topHoarders)
                {
                    AddLabel(40, y, ColorWhite, h.Name);
                    AddLabel(180, y, h.Ores > 10000 ? ColorRed : ColorWhite, $"{h.Ores:N0}");
                    AddLabel(260, y, h.Ingots > 10000 ? ColorRed : ColorWhite, $"{h.Ingots:N0}");
                    AddLabel(350, y, h.Logs > 10000 ? ColorRed : ColorWhite, $"{h.Logs:N0}");
                    AddLabel(440, y, h.Boards > 10000 ? ColorRed : ColorWhite, $"{h.Boards:N0}");
                    AddLabel(530, y, h.Hides > 10000 ? ColorRed : ColorWhite, $"{h.Hides:N0}");
                    AddLabel(620, y, h.Leathers > 10000 ? ColorRed : ColorWhite, $"{h.Leathers:N0}");
                    y += 20;
                }
            }

            AddCommonButtons();
        }

        private void AddCommonButtons()
        {
            AddButton(350, 560, 247, 248, 0, GumpButtonType.Reply, 0); 
            AddButton(410, 560, 4011, 4013, 1, GumpButtonType.Reply, 0); 
            AddLabel(445, 562, ColorWhite, "새로고침");
        }

        private long GetTotalGold(PlayerMobile p)
        {
            long gold = 0;
            if (p.BankBox != null) gold += p.BankBox.GetAmount(typeof(Gold), true);
            if (p.Backpack != null) gold += p.Backpack.GetAmount(typeof(Gold), true);
            return gold;
        }

        private long GetDeepItemAmount(PlayerMobile p, Type baseType)
        {
            long amount = 0;
            
            // 1. 은행 및 가방
            if (p.BankBox != null) amount += CountItemsByType(p.BankBox, baseType);
            if (p.Backpack != null) amount += CountItemsByType(p.Backpack, baseType);

            // 2. 소유한 집 바닥(LockDowns) 및 창고(Secures)
            var houses = BaseHouse.GetHouses(p);
            foreach (var house in houses)
            {
                if (house == null) continue;

                // [수정] Dictionary 형태인 LockDowns의 Keys만 순회하여 아이템 객체만 가져옵니다. (CS0030 해결)
                foreach (Item item in house.LockDowns.Keys)
                {
                    if (item.GetType() == baseType || item.GetType().IsSubclassOf(baseType))
                        amount += item.Amount;
                    
                    if (item is Container c)
                        amount += CountItemsByType(c, baseType);
                }

                foreach (SecureInfo secure in house.Secures)
                {
                    if (secure.Item is Container c)
                        amount += CountItemsByType(c, baseType);
                }
            }

            // 3. 소유한 상인(Vendor) 가방
            foreach (Mobile m in World.Mobiles.Values.OfType<PlayerVendor>())
            {
                if (m is PlayerVendor vendor && vendor.Owner == p && vendor.Backpack != null)
                {
                    amount += CountItemsByType(vendor.Backpack, baseType);
                }
            }

            return amount;
        }

        private long CountItemsByType(Container cont, Type baseType)
        {
            if (cont == null) return 0;
            long total = 0;
            foreach (Item item in cont.FindItemsByType(baseType, true))
            {
                total += item.Amount;
            }
            return total;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) 
                sender.Mobile.SendGump(new GlobalEconomyMonitorGump(sender.Mobile));
            else if (info.ButtonID == 888) 
            {
                // 여기는 유저님 서버의 NPC 경제 메인 Gump 클래스명을 넣어주시면 됩니다.
                // sender.Mobile.SendGump(new EconomyGlobalNpcGump(sender.Mobile));
                sender.Mobile.SendMessage("NPC 경제 관리 창으로 돌아갑니다. (명령어를 다시 입력해주세요)");
            }
        }
    }
}