using System;
using System.IO;
using System.Linq;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System.Collections.Generic;
using Server.Multis; // [수정1] BaseHouse 사용을 위한 네임스페이스 추가
using Server.Accounting;

namespace Server.Misc
{
    public static class GlobalEconomyMonitor
    {
        public static double GII { get; private set; } = 1.0;
        
        public static long TotalUserWealth { get; private set; } = 0;
        public static long TotalTownWealth { get; private set; } = 0;

        // [신규] 경제 시스템 제어 변수
        public static bool EnableEconomyEngine { get; set; } = false; 
        public static long TownWealthOffset { get; set; } = 0; // 보정값 (가산/감산)

        private static string SavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "GlobalEconomyReport.bin");

        public static void Initialize()
        {
            CommandSystem.Register("EcoMonitor", AccessLevel.GameMaster, e =>
            {
                GenerateUserReport(); // 열기 직전에 최신 데이터 갱신
                e.Mobile.SendGump(new EconomyAdminGump(e.Mobile));
            });
        }

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            GenerateUserReport();

            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            using FileStream bin = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            GenericWriter writer = new BinaryFileWriter(bin, true);
            
            writer.Write(2); // Version 2 (제어 변수 추가)
            writer.Write(TotalUserWealth);
            writer.Write(TotalTownWealth);
            writer.Write(GII);
            writer.Write(EnableEconomyEngine);
            writer.Write(TownWealthOffset);
            
            writer.Close();
        }

        // [수정2] Gump 클래스에 있던 함수들을 GlobalEconomyMonitor 클래스 안으로 복구
        public static Dictionary<string, (int charCount, long totalWealth)> GetUserWealthDistribution()
        {
            var accountData = new Dictionary<string, (int charCount, long totalWealth)>();

            // 1. [핵심 패치] Account 객체에서 직접 통화(골드+플래티넘) 추출
			foreach (var account in Accounts.GetAccounts().OfType<Account>())
			{
				if (account.AccessLevel > AccessLevel.Player) 
					continue;

				string accName = account.Username;

				// [최종 확정 로직] 
				// TotalCurrency 자체가 (플래티넘 + 골드 비율)이므로 Threshold만 곱하면 
				// 1원 단위까지 완벽하게 통합 골드로 환산됩니다.
				long accountWealth = (long)Math.Round(account.TotalCurrency * Account.CurrencyThreshold);

				// 계정 내 캐릭터 수 카운트
				int charCount = 0;
				for (int i = 0; i < account.Length; ++i)
				{
					if (account[i] != null) charCount++;
				}

				accountData[accName] = (charCount, accountWealth);
			}

            // 2. 플레이어 벤더 스캔 (개인 상인이 들고 있는 판매 대금)
            // (이 돈은 아직 Account.TotalCurrency에 편입되지 않은 상태이므로 따로 더해줍니다)
            foreach (PlayerVendor pv in World.Mobiles.Values.OfType<PlayerVendor>())
            {
                if (pv.Owner != null && pv.Owner.Account != null && pv.Owner.Account.AccessLevel == AccessLevel.Player)
                {
                    string accName = pv.Owner.Account.Username;
                    if (accountData.ContainsKey(accName))
                    {
                        var current = accountData[accName];
                        accountData[accName] = (current.charCount, current.totalWealth + pv.HoldGold);
                    }
                }
            }

            // 3. 집(House) 금고 및 락다운 스캔 (은행에 넣지 않은 '물리적 골드 아이템' 스캔)
            foreach (Item item in World.Items.Values.OfType<Gold>())
            {
                if (item.Map == null || item.Map == Map.Internal)
                    continue;

                BaseHouse house = null;

                // 바닥 락다운
                if (item.RootParent == null && item.IsLockedDown)
                {
                    house = BaseHouse.FindHouseAt(item);
                }
                // 잠긴 상자(Secure/LockedDown)
                else if (item.RootParent is BaseContainer container && (container.IsLockedDown || container.IsSecure))
                {
                    house = BaseHouse.FindHouseAt(container);
                }

                if (house != null && house.Owner != null && house.Owner.Account != null && house.Owner.Account.AccessLevel == AccessLevel.Player)
                {
                    string accName = house.Owner.Account.Username;
                    if (accountData.ContainsKey(accName))
                    {
                        var current = accountData[accName];
                        accountData[accName] = (current.charCount, current.totalWealth + item.Amount);
                    }
                }
            }

            return accountData;
        }

        public static void GenerateUserReport()
        {
            // 중앙 집계 데이터 호출
            var distribution = GetUserWealthDistribution();
            
            // 유저 총 자산은 모든 계정의 wealth를 더한 값
            TotalUserWealth = distribution.Values.Sum(v => v.totalWealth);

            long currentTownWealth = 0;
            foreach (var town in TownEconomyManager.Towns.Values)
            {
                currentTownWealth += town.Wealth;
            }
            TotalTownWealth = Math.Max(1L, currentTownWealth);

            long adjustedTownWealth = Math.Max(1L, TotalTownWealth + TownWealthOffset);
            double rawGII = (double)TotalUserWealth / adjustedTownWealth;
            
            GII = Math.Max(1.0, rawGII); 
        }

        private static void OnLoad()
        {
            if (File.Exists(SavePath))
            {
                using FileStream bin = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                GenericReader reader = new BinaryFileReader(new BinaryReader(bin));
                
                int version = reader.ReadInt();
                TotalUserWealth = reader.ReadLong();
                TotalTownWealth = reader.ReadLong();
                
                if (version <= 1) reader.ReadLong(); // 구버전 BaseServerWealth 버림
                
                GII = reader.ReadDouble();

                if (version >= 2)
                {
                    EnableEconomyEngine = reader.ReadBool();
                    TownWealthOffset = reader.ReadLong();
                }
            }

            if (EnableEconomyEngine) ApplyGlobalInflation();
            else Console.WriteLine($"[Economy Engine] 관전 모드: GII({GII:F2}) 조치를 적용하지 않습니다.");
        }

        private static void ApplyGlobalInflation()
        {
            long adjustedTownWealth = Math.Max(1L, TotalTownWealth + TownWealthOffset);
            double hoardingIndex = (double)TotalUserWealth / adjustedTownWealth;
            bool isHoardingDetected = hoardingIndex > 3.0;

            foreach (var town in TownEconomyManager.Towns.Values)
            {
                town.BaseWealth = (long)(town.BaseWealth * GII);
                town.Wealth = town.BaseWealth; 

                if (isHoardingDetected)
                {
                    Type[] essentialMats = [typeof(IronOre), typeof(Log), typeof(WheatSheaf), typeof(Hides)];
                    int pumpAmount = (int)(1000 * hoardingIndex);

                    foreach (Type mat in essentialMats)
                    {
                        if (!town.Warehouse.ContainsKey(mat))
                            town.Warehouse[mat] = new WarehouseItem(mat, 1000, 10);
                        
                        town.Warehouse[mat].Stock += pumpAmount;
                    }

                    foreach (var citizen in town.Citizens)
                        citizen.Potential = Math.Min(5.0, citizen.Potential + (hoardingIndex * 0.1));
                }
            }

            Console.WriteLine($"[Economy Engine] 인플레이션(GII: {GII:F2}) 적용 완료.");
        }
    }

    // =========================================================================
    // 인게임 관리용 UI (Gump)
    // =========================================================================
    public class EconomyUserWealthGump : Gump
    {
        private Mobile m_From;
        private int m_MapIndex;
        private int m_TPage;
        private int m_Page;

        public EconomyUserWealthGump(Mobile from, int mapIdx, int tPage, int page = 0) : base(100, 100)
        {
            m_From = from;
            m_MapIndex = mapIdx;
            m_TPage = tPage;
            m_Page = page;

            from.CloseGump(typeof(EconomyUserWealthGump));

            AddPage(0);
            AddBackground(0, 0, 500, 500, 9270);
            AddAlphaRegion(10, 10, 480, 480);

            AddHtml(10, 15, 480, 25, "<CENTER><BASEFONT SIZE='6' COLOR='#FDB913'>유저 계정별 자산 순위 (Top Wealth)</BASEFONT></CENTER>", false, false);
            
            // 돌아가기 버튼 (ID: 999)
            AddButton(20, 15, 4014, 4016, 999, GumpButtonType.Reply, 0); 
            AddLabel(55, 15, 1152, "관리 메인으로");

            // [수정3] 이제 정상적으로 GlobalEconomyMonitor 클래스의 중앙 메서드를 호출합니다.
            var accountData = GlobalEconomyMonitor.GetUserWealthDistribution();

            // 자산(Wealth) 기준 내림차순 정렬
            var sortedList = accountData.OrderByDescending(kvp => kvp.Value.totalWealth).ToList();

            // 2. 헤더 라인
            int y = 60;
            AddImageTiled(20, y + 25, 460, 2, 9277); 
            AddLabel(30, y, 1152, "순위");
            AddLabel(90, y, 1152, "계정명 (Account)");
            AddLabel(250, y, 1152, "보유 캐릭터");
            AddLabel(340, y, 1152, "총 자산 (Gold)");

            // 3. 리스트 출력 (페이지당 13개)
            int start = m_Page * 13;
            int end = Math.Min(start + 13, sortedList.Count);

            for (int i = start; i < end; i++)
            {
                y += 28;
                var kvp = sortedList[i];
                int rank = i + 1;
                string accName = kvp.Key;
                int chars = kvp.Value.charCount;
                long wealth = kvp.Value.totalWealth;

                // 1위(금), 2위(은), 3위(동) 색상 차등 부여
                int rankColor = rank switch { 1 => 53, 2 => 89, 3 => 243, _ => 0x481 };
                int nameColor = wealth >= 100_000_000L ? 68 : 1152; // 1억 이상 거부는 닉네임 녹색 처리

                AddLabel(30, y, rankColor, $"{rank}위");
                AddLabel(90, y, nameColor, accName);
                AddLabel(265, y, 1152, $"{chars} 명");
                AddLabel(340, y, rankColor, $"{wealth:N0} g");
            }

            // 4. 페이징 버튼
            if (m_Page > 0) AddButton(180, 450, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddLabel(220, 450, 1152, $"{m_Page + 1} / {Math.Max(1, (sortedList.Count - 1) / 13 + 1)}");
            if (end < sortedList.Count) AddButton(270, 450, 4005, 4007, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            // 닫기 또는 뒤로가기 누를 시 메인 관리 창 복귀
            if (info.ButtonID == 0 || info.ButtonID == 999) 
            {
                m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, 0, m_TPage, 0));
                return;
            }

            if (info.ButtonID == 1) m_Page--;
            else if (info.ButtonID == 2) m_Page++;

            m_From.SendGump(new EconomyUserWealthGump(m_From, m_MapIndex, m_TPage, m_Page));
        }
    }
}