using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Accounting;
using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    // 1. 작위 등급 (총 9단계)
    public enum NobilityRank
    {
        Commoner = 0,   // 평민
        Knight = 1,     // 기사
        SubBaronet = 2, // 준훈작
        Baronet = 3,    // 훈작
        SubBaron = 4,   // 준남작
        Baron = 5,      // 남작
        Viscount = 6,   // 자작
        Count = 7,      // 백작
        Marquis = 8     // 후작
    }

    // 2. 작위 정보 및 마을 오픈 권한 헬퍼
    public class RankInfo
    {
        public static int GetRequiredMerit(NobilityRank rank)
        {
            switch (rank)
            {
                case NobilityRank.Commoner: return 0;
                case NobilityRank.Knight: return 700;
                case NobilityRank.SubBaronet: return 1750;
                case NobilityRank.Baronet: return 3600;
                case NobilityRank.SubBaron: return 6400;
                case NobilityRank.Baron: return 13500;
                case NobilityRank.Viscount: return 27000;
                case NobilityRank.Count: return 60000;
                case NobilityRank.Marquis: return 150000;
                default: return 0;
            }
        }

        // 유저의 작위가 해당 마을(townName)에 기부/투자할 권한이 있는지 확인
        public static bool IsTownOpenedFor(NobilityRank rank, string townName)
        {
            if (rank == NobilityRank.Marquis) return true; // 후작은 프리패스

            // 작위별 오픈되는 마을 목록 (문자열은 TownEconomy의 TownName과 일치해야 함)
            List<string> allowedTowns = new List<string> { "Cove", "Moonglow", "Yew" }; // 평민
            
            if (rank >= NobilityRank.Knight) allowedTowns.Add("Minoc");
            if (rank >= NobilityRank.SubBaronet) allowedTowns.Add("Skara Brae");
            if (rank >= NobilityRank.Baronet) allowedTowns.Add("Jhelom");
            if (rank >= NobilityRank.SubBaron) allowedTowns.Add("New Magincia");
            if (rank >= NobilityRank.Baron) allowedTowns.Add("Vesper");
            if (rank >= NobilityRank.Viscount) allowedTowns.Add("Trinsic");
            if (rank >= NobilityRank.Count) allowedTowns.Add("Britain");

            return allowedTowns.Contains(townName);
        }

        public static string GetOpenTowns(NobilityRank rank)
        {
            if (rank == NobilityRank.Marquis) return "모든 마을 오픈";
            
            switch (rank)
            {
                case NobilityRank.Commoner: return "코브, 문글로우, 유";
                case NobilityRank.Knight: return "미녹";
                case NobilityRank.SubBaronet: return "스카라 브레";
                case NobilityRank.Baronet: return "젤롬";
                case NobilityRank.SubBaron: return "신 매진시아";
                case NobilityRank.Baron: return "베스퍼";
                case NobilityRank.Viscount: return "트린식";
                case NobilityRank.Count: return "브리튼";
                default: return "없음";
            }
        }
    }

    // 3. 계정별 영토 및 공훈 프로필 데이터
   // 3. 계정별 영토 및 공훈 프로필 데이터
    public class TerritoryProfile
    {
        public int ContributionPoints { get; set; } // 누적 공훈(Merit)
        public NobilityRank Rank { get; set; }      // 현재 직위
        public int BonusTiles { get; set; }         // 시즌 보상 등 추가 영토 칸
        public int UsedTiles { get; set; }          // 현재 건설된 확장 영토가 차지한 칸 수

        public TerritoryProfile()
        {
            Rank = NobilityRank.Commoner;
            ContributionPoints = 0;
            BonusTiles = 0;
            UsedTiles = 0;
        }

        // 작위별 기본 제공 영토 칸 수 (기획서 반영)
        public int GetRankBaseTiles() => Rank switch
        {
            NobilityRank.Commoner => 10,       // [기획] 평민 10칸
            NobilityRank.Knight => 50,         // [기획] 기사 50칸
            NobilityRank.SubBaronet => 100,
            NobilityRank.Baronet => 150,
            NobilityRank.SubBaron => 200,
            NobilityRank.Baron => 300,         // [기획] 남작 300칸
            NobilityRank.Viscount => 500,
            NobilityRank.Count => 700,
            NobilityRank.Marquis => 1000,      // [기획] 후작 1000칸
            _ => 10
        };

        public int TotalMaxTiles => GetRankBaseTiles() + BonusTiles;
        public int AvailableTiles => Math.Max(0, TotalMaxTiles - UsedTiles);

        // 공훈 상승 시 작위 자동 갱신
        public bool UpdateRank()
        {
            NobilityRank newRank = NobilityRank.Commoner;
            foreach (NobilityRank rank in Enum.GetValues(typeof(NobilityRank)))
            {
                if (ContributionPoints >= RankInfo.GetRequiredMerit(rank))
                    newRank = rank;
            }

            if (Rank != newRank)
            {
                Rank = newRank;
                return true; // 승급 발생
            }
            return false;
        }
    }

	// 4. 영토 및 작위 코어 매니저
    public class RankAndTerritorySystem
    {
        public static Dictionary<string, TerritoryProfile> m_Profiles = new Dictionary<string, TerritoryProfile>();

        public static void Initialize()
        {
            EventSink.WorldSave += new WorldSaveEventHandler(OnSave);
            EventSink.WorldLoad += new WorldLoadEventHandler(OnLoad);

            CommandSystem.Register("MyRank", AccessLevel.Player, new CommandEventHandler(MyRank_OnCommand));
            CommandSystem.Register("SetMerit", AccessLevel.GameMaster, new CommandEventHandler(SetMerit_OnCommand));
        }

        public static TerritoryProfile GetProfile(Account account)
        {
            if (account == null) return null;
            string accName = account.Username;
            if (!m_Profiles.ContainsKey(accName)) m_Profiles[accName] = new TerritoryProfile();
            return m_Profiles[accName];
        }

        // ====================================================================
        // [기획 추가] 작위별 가문 창고 기본 용량 반환 헬퍼
        // ====================================================================
        public static int GetRankBaseCapacity(NobilityRank rank) => rank switch
        {
            NobilityRank.Commoner => 100,      // [기획] 평민 100칸
            NobilityRank.Knight => 500,        // [기획] 기사 500칸
            NobilityRank.SubBaronet => 1000,
            NobilityRank.Baronet => 2000,
            NobilityRank.SubBaron => 3500,
            NobilityRank.Baron => 5000,        // [기획] 남작 5000칸
            NobilityRank.Viscount => 6500,
            NobilityRank.Count => 8000,
            NobilityRank.Marquis => 10000,     // [기획] 후작 10000칸
            _ => 100
        };

		// RankAndTerritorySystem.cs 내부에 아래 메서드들을 다시 추가하세요.
		[Usage("MyRank")]
		[Description("내 작위와 영토 한도를 확인합니다.")]
		private static void MyRank_OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;
			TerritoryProfile profile = GetProfile(from.Account as Account);
			if (profile != null)
			{
				from.SendMessage(88, $"[내 작위] : {profile.Rank}");
				from.SendMessage(68, $"[공훈 수치] : {profile.ContributionPoints}");
				from.SendMessage(55, $"[오픈 권한] : {RankInfo.GetOpenTowns(profile.Rank)}");
			}
		}

		[Usage("SetMerit <amount>")]
		[Description("자신의 계정에 공훈(Merit)을 설정합니다.")]
		private static void SetMerit_OnCommand(CommandEventArgs e)
		{
			if (e.Arguments.Length == 1)
			{
				int amount = Utility.ToInt32(e.Arguments[0]);
				TerritoryProfile profile = GetProfile(e.Mobile.Account as Account);
				if (profile != null)
				{
					profile.ContributionPoints = amount;
					profile.UpdateRank();
					e.Mobile.SendMessage($"공훈이 {amount}로 설정되었습니다.");
				}
			}
		}

        #region [세이브 / 로드 로직 - 작위 프로필 전용]
        
        private static string SavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "RankTerritory.bin");

        private static void OnSave(WorldSaveEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            FileStream bin = null;
            try
            {
                bin = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
                BinaryFileWriter writer = new BinaryFileWriter(bin, true); 

                writer.Write(0); // Version
                writer.Write(m_Profiles.Count);
                foreach (var kvp in m_Profiles)
                {
                    writer.Write(kvp.Key);
                    writer.Write((int)kvp.Value.Rank);
                    writer.Write(kvp.Value.ContributionPoints);
                    writer.Write(kvp.Value.BonusTiles);
                    writer.Write(kvp.Value.UsedTiles);
                }
                writer.Close();
            }
            finally { bin?.Dispose(); }
        }

        private static void OnLoad()
        {
            if (!File.Exists(SavePath)) return;

            FileStream bin = null;
            try
            {
                bin = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin)); 

                int version = reader.ReadInt();
                int count = reader.ReadInt();
                for (int i = 0; i < count; ++i)
                {
                    string accName = reader.ReadString();
                    TerritoryProfile profile = new TerritoryProfile();
                    profile.Rank = (NobilityRank)reader.ReadInt();
                    profile.ContributionPoints = reader.ReadInt();
                    profile.BonusTiles = reader.ReadInt();
                    profile.UsedTiles = reader.ReadInt();
                    m_Profiles[accName] = profile;
                }
                reader.Close();
            }
            finally { bin?.Dispose(); }
        }
        #endregion
    }
}
