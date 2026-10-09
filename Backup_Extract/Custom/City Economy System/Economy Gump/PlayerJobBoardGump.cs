using System;
using System.Linq;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Multis;
using Server.Mobiles;

namespace Server.Misc
{
    public class PlayerJobBoardGump : Gump
    {
        private Mobile              m_From;
        private BaseHouse           m_House;
        private int                 m_Page;
        private int                 m_Category; // JobCategory 인덱스
        private int                 m_SortMode;
        private List<VirtualCitizen> m_Candidates;

        public enum JobCategory
        {
            All = 0,
            Blacksmith,
            Tailor,
            Carpenter,
            Tinker,
            Cook,
            Alchemist,
            Scribe,
            Fletcher,
            Farmer,
            Fisher,
            Miner,
            Lumberjack,
            Bard,
            Inn,
            Tanner,
            Other
        }

        private static readonly string[] CatLabels = {
            "전체",
            "대장장이",   
            "재봉사",     
            "목수",       
            "팅커",       
            "요리사",     
            "연금술사",   
            "기록술사",   
            "활제작사",   
            "농부",       
            "어부",       
            "광부",       
            "나무꾼",     
            "음유시인",
            "여관",
            "무두장"
        };

        // 정렬 모드
        private static readonly string[] SortLabels = { "등급 높은순", "등급 낮은순", "일당 낮은순", "일당 높은순", "마을 가까운순" };

        public PlayerJobBoardGump(Mobile from, BaseHouse house, int page = 0, int cat = 0, int sort = 0)
            : base(40, 30)
        {
            m_From     = from;
            m_House    = house;
            m_Page     = page;
            m_Category = cat;
            m_SortMode = sort;

            Closable = true; Disposable = true; Dragable = true; Resizable = false;

            // ── 후보 수집 (카테고리 기준 필터) ──
            m_Candidates = new List<VirtualCitizen>();

            foreach (var town in TownEconomyManager.Towns.Values.Where(t => t.Facet == m_House.Map))
            {
                if (town.Citizens == null) continue;
                foreach (var c in town.Citizens)
                {
                    if (c.Age < 18) continue;
                    
                    if (m_Category == 0 || (int)GetJobCategory(c.JobClass) == m_Category)
                        m_Candidates.Add(c);
                }
            }

            // ── 정렬 ──
            switch (m_SortMode)
            {
                case 1:  m_Candidates = m_Candidates.OrderBy(c => c.Rank).ThenBy(c => ChunkDist(c)).ToList(); break;
                case 2:  m_Candidates = m_Candidates.OrderBy(c => CalcWage(c)).ThenBy(c => c.Rank).ToList(); break;
                case 3:  m_Candidates = m_Candidates.OrderByDescending(c => CalcWage(c)).ThenBy(c => c.Rank).ToList(); break;
                case 4:  m_Candidates = m_Candidates.OrderBy(c => ChunkDist(c)).ThenByDescending(c => c.Rank).ToList(); break;
                default: m_Candidates = m_Candidates.OrderByDescending(c => c.Rank).ThenBy(c => ChunkDist(c)).ToList(); break;
            }

            DrawUI();
        }

        // ─── NpcJobClass → 카테고리 매핑 ───
        private static JobCategory GetJobCategory(NpcJobClass job)
        {
            switch (job)
            {
                // 대장장이 계열
                case NpcJobClass.Blacksmith: case NpcJobClass.Smelter: case NpcJobClass.PigIronWorker:
                    return JobCategory.Blacksmith;

                // 무두장 (TasteIdentification 연동)
                case NpcJobClass.LeatherTanner:
                    return JobCategory.Tanner;

                // 재봉사 계열
                case NpcJobClass.Tailor: case NpcJobClass.Weaver: case NpcJobClass.Spinner:
                case NpcJobClass.ThreadMaker: case NpcJobClass.ClothUnraveler:
                case NpcJobClass.Dyer_Producer:
                    return JobCategory.Tailor;

                // 목수 계열
                case NpcJobClass.Carpenter_Producer: case NpcJobClass.Sawyer: case NpcJobClass.ShaftMaker:
                case NpcJobClass.BarkProcessor: case NpcJobClass.BarrelMaker_Base: case NpcJobClass.BoxMaker_Base:
                case NpcJobClass.Shipwright_Master:
                    return JobCategory.Carpenter;

                // 팅커 계열
                case NpcJobClass.Tinker: case NpcJobClass.NailMaker: case NpcJobClass.AxleMaker:
                case NpcJobClass.GearCutter: case NpcJobClass.SpringMaker: case NpcJobClass.HingeMaker:
                case NpcJobClass.SextantPartMaker: case NpcJobClass.ClockPartMaker:
                case NpcJobClass.GlassBlower: case NpcJobClass.CandleDipper:
                case NpcJobClass.GemCutter: case NpcJobClass.JewelryBaseMaker: case NpcJobClass.BeadMaker:
                    return JobCategory.Tinker;

                // 여관 계열
                case NpcJobClass.InnKeeper: case NpcJobClass.Barmaid: case NpcJobClass.Cellarman_Entertainer:
                    return JobCategory.Inn;

                // 요리사 계열
                case NpcJobClass.Cook_Entertainer: case NpcJobClass.PizzaChef_Producer:
                case NpcJobClass.Miller: case NpcJobClass.Butcher_Expert: case NpcJobClass.PoultryProcessor:
                case NpcJobClass.Vintner_Base: case NpcJobClass.OilPresser_Producer:
                case NpcJobClass.Scullion:
                    return JobCategory.Cook;

                // 연금술사 계열
                case NpcJobClass.Alchemist: case NpcJobClass.PotionMaker: case NpcJobClass.Transmuter:
                case NpcJobClass.ApprenticeAlchemist: case NpcJobClass.DarkApothecary:
                case NpcJobClass.ReagentRefiner: case NpcJobClass.AshProcessor: case NpcJobClass.BoneGrinder:
                    return JobCategory.Alchemist;

                // 기록술사 계열
                case NpcJobClass.Scribe_Mage: case NpcJobClass.Scribe_Scholar:
                case NpcJobClass.Copyist_Mage: case NpcJobClass.Librarian:
                case NpcJobClass.ScrollPresser: case NpcJobClass.MapPresser:
                case NpcJobClass.InkProducer: case NpcJobClass.ScrollBundler:
                    return JobCategory.Scribe;

                // 활제작사
                case NpcJobClass.Bowyer:
                    return JobCategory.Fletcher;

                // 농부 계열
                case NpcJobClass.GrainFarmer: case NpcJobClass.VegetableFarmer: case NpcJobClass.GourdFarmer:
                case NpcJobClass.Orchardist: case NpcJobClass.CitrusGrower: case NpcJobClass.VineyardWorker:
                case NpcJobClass.BerryPicker: case NpcJobClass.Herbalist: case NpcJobClass.Beekeeper:
                case NpcJobClass.Shepherd: case NpcJobClass.Swineherd: case NpcJobClass.PoultryFarmer:
                case NpcJobClass.CattleDrover: case NpcJobClass.StableHand: case NpcJobClass.DairyWorker:
                case NpcJobClass.GooseHerd: case NpcJobClass.DonkeyDriver: case NpcJobClass.HorseGroom_Basic:
                    return JobCategory.Farmer;

                // 어부 계열
                case NpcJobClass.CoastalFisher: case NpcJobClass.DeepSeaFisher_Basic:
                case NpcJobClass.Crabber: case NpcJobClass.OysterDiver_Basic:
                case NpcJobClass.SeaweedCollector: case NpcJobClass.DeepSeaFisher:
                    return JobCategory.Fisher;

                // 광부 계열
                case NpcJobClass.SurfaceMiner: case NpcJobClass.SandDigger:
                case NpcJobClass.StoneQuarryman: case NpcJobClass.FlintKnapper:
                case NpcJobClass.SaltGatherer:
                    return JobCategory.Miner;

                // 나무꾼 계열
                case NpcJobClass.Woodcutter: case NpcJobClass.BarkCollector:
                    return JobCategory.Lumberjack;

                // 음유시인 계열
                case NpcJobClass.Bard: case NpcJobClass.Harper: case NpcJobClass.Lutanist:
                case NpcJobClass.Drummer: case NpcJobClass.Tambourinist: case NpcJobClass.Dancer:
                case NpcJobClass.Acrobat: case NpcJobClass.Juggler: case NpcJobClass.Actor:
                    return JobCategory.Bard;

                // 기타
                default:
                    return JobCategory.Other;
            }
        }

        // ─── 카테고리 한글명 (직종 컬럼에 표시) ───
        private static string CategoryLabel(NpcJobClass job)
        {
            int cat = (int)GetJobCategory(job);
            if (cat >= 1 && cat < CatLabels.Length) return CatLabels[cat];
            return "기타";
        }

        // ─── 보조 메서드 ───
        private int ChunkDist(VirtualCitizen c)
        {
            var town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == c.TargetRegionName);
            if (town == null) return 9999;
            int dx = (town.Center.X / 128) - (m_House.Location.X / 128);
            int dy = (town.Center.Y / 128) - (m_House.Location.Y / 128);
            return Math.Abs(dx) + Math.Abs(dy);
        }
        private int BaseWage(NpcRank r)
        {
            switch (r) { case NpcRank.Master: return 5000; case NpcRank.Expert: return 3000; case NpcRank.Journeyman: return 1500; default: return 800; }
        }
        private int CalcWage(VirtualCitizen c) => BaseWage(c.Rank) + ChunkDist(c) * 2;

        private static string RankLabel(NpcRank r)
        {
            switch (r) { case NpcRank.Master: return "명인"; case NpcRank.Expert: return "숙련"; case NpcRank.Journeyman: return "견습"; default: return "초급"; }
        }
        private static string RankColor(NpcRank r)
        {
            switch (r) { case NpcRank.Master: return "#FF7777"; case NpcRank.Expert: return "#FFCC44"; case NpcRank.Journeyman: return "#77FF77"; default: return "#CCCCCC"; }
        }

        // ─────────────────────────────────────
        //  UI 렌더링
        // ─────────────────────────────────────
        private void DrawUI()
        {
            int total      = m_Candidates.Count;
            int perPage    = 8;
            int totalPages = Math.Max(1, (total + perPage - 1) / perPage);
            if (m_Page >= totalPages) m_Page = totalPages - 1;
            int start = m_Page * perPage;
            int end   = Math.Min(start + perPage, total);

            int maxSlots  = m_House.GetMaxEmployeeLimit();
            int usedSlots = RetailVendor.RetailVendors.Count(v =>
                v.Owner == m_From && v.Map == m_House.Map && m_House.Region.Contains(v.Location));

            // ── 배경 ──
            AddBackground(0, 0, 640, 530, 9270);
            AddAlphaRegion(8, 8, 624, 514);

            // ── 제목 ──
            AddHtml(0, 14, 640, 24,
                "<CENTER><BASEFONT SIZE=5 COLOR=#FFD700>직원 채용 게시판</BASEFONT></CENTER>",
                false, false);

            // ── 고용 현황 ──
            string slotColor = usedSlots >= maxSlots ? "#FF5555" : "#88FF88";
            AddHtml(14, 40, 500, 20,
                $"<BASEFONT COLOR=#BBBBBB>현재 고용: </BASEFONT><BASEFONT COLOR={slotColor}>{usedSlots} / {maxSlots} 명</BASEFONT>" +
                $"<BASEFONT COLOR=#777777>  |  정렬: </BASEFONT><BASEFONT COLOR=#FFEEAA>{SortLabels[m_SortMode]}</BASEFONT>",
                false, false);

            // ── 구분선 ──
            AddImageTiled(10, 60, 620, 2, 2624);

            // ── 카테고리 탭 (2줄, 16개 탭) ──
            int tabW = 76;
            for (int k = 0; k < CatLabels.Length; k++)
            {
                bool sel = (m_Category == k);
                string col = sel ? "#FFD700" : "#888888";

                int row = k / 8;
                int col_idx = k % 8;
                int tx = 16 + col_idx * tabW;
                int ty = 63 + row * 18;

                if (sel)
                    AddImageTiled(tx, ty + 14, tabW - 4, 2, 2624);

                AddButton(tx, ty, 2361, 2361, 10 + k, GumpButtonType.Reply, 0);
                AddHtml(tx, ty, tabW, 18,
                    $"<CENTER><BASEFONT COLOR={col}>{CatLabels[k]}</BASEFONT></CENTER>",
                    false, false);
            }

            // ── 헤더 구분선 ──
            int headerY = 100;
            AddImageTiled(10, headerY - 2, 620, 2, 2624);

            // ── 컬럼 헤더 ──
            AddButton(14, headerY + 2, 2361, 2361, 30, GumpButtonType.Reply, 0);
            AddHtml(14, headerY, 150, 20,
                $"<BASEFONT COLOR={(m_SortMode <= 1 ? "#FFD700" : "#888888")}>이름 / 등급</BASEFONT>", false, false);

            AddHtml(170, headerY, 100, 20, "<BASEFONT COLOR=#888888>직종</BASEFONT>", false, false);

            AddButton(280, headerY + 2, 2361, 2361, 32, GumpButtonType.Reply, 0);
            AddHtml(280, headerY, 120, 20,
                $"<BASEFONT COLOR={(m_SortMode == 4 ? "#FFD700" : "#888888")}>출신 마을</BASEFONT>", false, false);

            AddButton(420, headerY + 2, 2361, 2361, 31, GumpButtonType.Reply, 0);
            AddHtml(420, headerY, 100, 20,
                $"<BASEFONT COLOR={(m_SortMode == 2 || m_SortMode == 3 ? "#FFD700" : "#888888")}>일당</BASEFONT>", false, false);

            AddHtml(540, headerY, 55, 20, "<BASEFONT COLOR=#888888>고용</BASEFONT>", false, false);

            AddImageTiled(10, headerY + 18, 620, 2, 2624);

            // ── 후보자 목록 ──
            int y = headerY + 24;
            for (int i = start; i < end; i++)
            {
                VirtualCitizen c = m_Candidates[i];
                int  dist        = ChunkDist(c);
                int  wage        = CalcWage(c);

                // 홀수 행 배경
                if (i % 2 != 0)
                    AddImageTiled(10, y - 1, 620, 32, 9274);

                // 이름/등급
                string rc = RankColor(c.Rank);
                string rl = RankLabel(c.Rank);
                AddHtml(14, y + 3, 152, 22,
                    $"<BASEFONT COLOR={rc}>[{rl}]</BASEFONT>  <BASEFONT COLOR=#FFFFFF>{c.Name}</BASEFONT>",
                    false, false);

                // 직종 (스킬 그룹)
                AddHtml(170, y + 3, 105, 22,
                    $"<BASEFONT COLOR=#DDDDDD>{CategoryLabel(c.JobClass)}</BASEFONT>",
                    false, false);

                // 출신 마을
                string distStr = dist <= 1 ? "근처" : dist <= 3 ? "보통" : "먼 곳";
                string distCol = dist <= 1 ? "#88FF88" : dist <= 3 ? "#FFDD88" : "#FF9999";
                AddHtml(280, y + 3, 135, 22,
                    $"<BASEFONT COLOR=#CCCCCC>{c.TargetRegionName}</BASEFONT>  <BASEFONT COLOR={distCol}>({distStr})</BASEFONT>",
                    false, false);

                // 일당
                AddHtml(420, y + 3, 100, 22,
                    $"<BASEFONT COLOR=#FFEE44>{wage:N0} G</BASEFONT>",
                    false, false);

                // 고용 버튼
                AddButton(540, y + 2, 4005, 4007, 100 + i, GumpButtonType.Reply, 0);
                AddHtml(561, y + 3, 40, 22,
                    "<BASEFONT COLOR=#AAFFAA>고용</BASEFONT>",
                    false, false);

                y += 34;
            }

            // ── 목록 끝 구분선 ──
            AddImageTiled(10, y + 2, 620, 2, 2624);

            // ── 페이지 ──
            if (m_Page > 0)
            {
                AddButton(14, 500, 4014, 4016, 1, GumpButtonType.Reply, 0);
                AddHtml(38, 502, 80, 20, "<BASEFONT COLOR=#AAAAAA>이전</BASEFONT>", false, false);
            }
            AddHtml(0, 502, 640, 20,
                $"<CENTER><BASEFONT COLOR=#666666>{m_Page + 1} / {totalPages}  (총 {total}명)</BASEFONT></CENTER>",
                false, false);
            if (m_Page < totalPages - 1)
            {
                AddButton(598, 500, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddHtml(550, 502, 48, 20, "<BASEFONT COLOR=#AAAAAA>다음</BASEFONT>", false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;

            if (info.ButtonID == 1)
            { m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, m_Page - 1, m_Category, m_SortMode)); return; }
            if (info.ButtonID == 2)
            { m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, m_Page + 1, m_Category, m_SortMode)); return; }

            // 카테고리 탭
            if (info.ButtonID >= 10 && info.ButtonID < 10 + CatLabels.Length)
            {
                m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, 0, info.ButtonID - 10, m_SortMode));
                return;
            }

            // 정렬
            if (info.ButtonID == 30) { int n = m_SortMode == 0 ? 1 : 0; m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, 0, m_Category, n)); return; }
            if (info.ButtonID == 31) { int n = m_SortMode == 2 ? 3 : 2; m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, 0, m_Category, n)); return; }
            if (info.ButtonID == 32) { m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, 0, m_Category, 4)); return; }

            // 고용
            if (info.ButtonID >= 100)
            {
                int idx = info.ButtonID - 100;
                if (idx < 0 || idx >= m_Candidates.Count) return;

                VirtualCitizen target = m_Candidates[idx];
                int maxSlots  = m_House.GetMaxEmployeeLimit();
                int usedSlots = RetailVendor.RetailVendors.Count(v =>
                    v.Owner == m_From && v.Map == m_House.Map && m_House.Region.Contains(v.Location));

                if (usedSlots >= maxSlots)
                {
                    m_From.SendMessage(0x22, "고용 슬롯이 가득 찼습니다.");
                    m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, m_Page, m_Category, m_SortMode));
                    return;
                }

                int wage = BaseWage(target.Rank) + ChunkDist(target) * 2;
                m_From.SendMessage(0x35, $"[{target.Name}] 을(를) 일당 {wage:N0} G 에 고용했습니다!");
                m_From.SendGump(new PlayerJobBoardGump(m_From, m_House, m_Page, m_Category, m_SortMode));
            }
        }
    }
}
