using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Regions;
using Server.Spells; // CheckMulti(유저 집 판별)용
using System.Linq;

namespace Server.Misc
{
    // 🌟 생태계 용도 (마마을, 벌목, 사냥, 특수)
    public enum EcoAreaType
    {
        Town,       // 마을 구역 (개, 고양이, 새 등 얌전한 동물 위주)
        Forest,     // 벌목/숲 구역 (사슴, 곰, 늑대 등)
        Hunting,    // 무두질/사냥 구역 (오크, 트롤, 거미 등 포식자/몬스터)
        Special     // 특수/마법 구역 (차후 자원/이벤트 연동용)
    }

    // 🌟 기후 및 지형 (북극, 사막, 열대 등)
    public enum EcoClimateType
    {
        Temperate,  // 일반/온대 (기본 풀밭, 일반 숲)
        Arctic,     // 북극/설원 (눈 바닥, 얼음)
        Tropical,   // 열대/정글 (정글 타일, 습함)
        Desert,     // 사막 (모래 바닥)
        Coastal,    // 해안가 (모래, 물가 인접)
        Swamp,      // 늪지대 (진흙, 독)
        Volcanic,   // 화산/화염 (용암 근처)
        Void        // 공허/마계 (TerMur 특수 지형 등)
    }

    public class EcoNode : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public string ZoneId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EcoAreaType AreaType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EcoClimateType ClimateType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; }

        [Constructable]
        public EcoNode() : base(0x11EA) // 🌟 던전 노드(보라색 크리스탈)와 구분되게 '화분/식물' 또는 '초록색 잎' 모양 사용
        {
            Movable = false;
            Visible = false;
            Name = "Ecosystem Spawn Node";
            ZoneId = "Unknown";
            AreaType = EcoAreaType.Forest; // 기본은 숲
            ClimateType = EcoClimateType.Temperate; // 기본은 온대
            SpawnRange = 15; // 🌟 생태계는 던전보다 넓게 퍼져야 함
            HomeRange = 25;  // 🌟 배회 반경도 넓게
        }

        public EcoNode(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                from.SendGump(new EcoNodeGump(from, this));
        }

        // 🌟 [핵심] 던전 노드보다 훨씬 정교한 야외 스폰 위치 판독기
        public Point3D? GetValidSpawnLocation()
        {
            if (Map == null || Map == Map.Internal) return null;
            Region nodeRegion = Region.Find(Location, Map);

            for (int i = 0; i < 15; i++) // 최대 15번 시도
            {
                int rx = X + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int ry = Y + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int rz = Map.GetAverageZ(rx, ry);

                Point3D testLoc = new Point3D(rx, ry, rz);

                // 1. 해당 좌표에 스폰이 가능한가? (시야, Z축, 바닥 판정)
                if (Map.CanSpawnMobile(rx, ry, rz))
                {
                    // 2. 다른 Region으로 넘어가지 않았는가?
                    if (nodeRegion == Region.Find(testLoc, Map))
                    {
                        // 3. 유저가 지은 집(House/Multi) 내부인가? -> 야생동물이 남의 집 안에 스폰되는 것 방지
                        if (!SpellHelper.CheckMulti(testLoc, Map))
                        {
                            // 4. [뭉침 방지] 해당 좌표 반경 1칸 이내에 이미 다른 모바일이 있는가?
                            bool isCrowded = false;
                            IPooledEnumerable eable = Map.GetMobilesInRange(testLoc, 1);
                            foreach (Mobile m in eable) { isCrowded = true; break; }
                            eable.Free();

                            // 뭉쳐있지 않으면 그 자리에 스폰 확정!
                            if (!isCrowded) return testLoc;
                        }
                    }
                }
            }
            return null; // 적당한 자리를 못 찾으면 이번 틱은 스폰 포기 (다음에 다시 시도)
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
            writer.Write(ZoneId ?? string.Empty);
            writer.Write((int)AreaType);
            writer.Write((int)ClimateType);
            writer.Write(SpawnRange);
            writer.Write(HomeRange);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            ZoneId = reader.ReadString();
            AreaType = (EcoAreaType)reader.ReadInt();
            ClimateType = (EcoClimateType)reader.ReadInt();
            SpawnRange = reader.ReadInt();
            HomeRange = reader.ReadInt();
        }
				// EcoNode.cs 파일 내부에 추가
		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			if (Parent != null || Map == null || Map == Map.Internal) return;

			// 🌟 [수정 포인트] 이미 제대로 된 이름이 붙어 있다면(Unknown이 아니라면) 
			// 리전 체크나 이름 갱신 로직을 아예 건너뜁니다.
			if (!string.IsNullOrEmpty(this.ZoneId) && this.ZoneId != "Unknown")
			{
				// 이름은 그대로 두고 캐시만 갱신해서 ZM에 바로 반영되게 합니다.
				EcosystemManager.Zones?.Values.ToList().ForEach(z => z?.CacheNodes());
				return; 
			}

			// --- 아래는 이름이 "Unknown"일 때만 실행되는 로직 ---
			Region r = Region.Find(Location, Map);
			if (r == null || string.IsNullOrEmpty(r.Name)) return;

			// 주변 중복 노드 삭제 로직 (기존과 동일)
			var toDelete = Map.GetItemsInRange(Location, 1)
				.Where(i => (i is DungeonNode || i is EcoNode) && i != this).ToList();
			foreach (var item in toDelete) item.Delete();

			// 이름표 갱신 (Unknown일 때만 실행됨)
			string cleanReg = r.Name.ToLower();
			string bestMatch = EcosystemManager.Zones?.Keys
				.FirstOrDefault(k => k.ToLower().Contains(cleanReg) || cleanReg.Contains(k.ToLower()));

			if (!string.IsNullOrEmpty(bestMatch))
			{
				this.ZoneId = bestMatch;
			}

			EcosystemManager.Zones?.Values.ToList().ForEach(z => z?.CacheNodes());
		}
    }

    // ========================================================================
    // 생태계 노드 세팅 Gump
    // ========================================================================
    public class EcoNodeGump : Gump
    {
        private readonly EcoNode m_Node;

        public EcoNodeGump(Mobile from, EcoNode node) : base(100, 100)
        {
            m_Node = node;
            from.CloseGump(typeof(EcoNodeGump));

            AddPage(0);
            AddBackground(0, 0, 450, 400, 9270);
            AddHtml(10, 10, 430, 20, "<CENTER>야외 생태계(Ecosystem) 노드 세팅</CENTER>", false, false);

            AddHtml(20, 50, 100, 20, "현재 생태 구역:", false, false);
            AddLabel(120, 50, 68, node.ZoneId);
            
            // 용도(AreaType) 설정
            AddHtml(20, 90, 100, 20, "생태계 용도:", false, false);
            AddRadio(120, 90, 208, 209, node.AreaType == EcoAreaType.Town, 10); AddLabel(145, 90, 0, "마을");
            AddRadio(220, 90, 208, 209, node.AreaType == EcoAreaType.Forest, 11); AddLabel(245, 90, 0, "벌목/숲");
            AddRadio(320, 90, 208, 209, node.AreaType == EcoAreaType.Hunting, 12); AddLabel(345, 90, 0, "사냥터");
            AddRadio(120, 115, 208, 209, node.AreaType == EcoAreaType.Special, 13); AddLabel(145, 115, 0, "특수 구역");

            // 기후(Climate) 설정
            AddHtml(20, 155, 100, 20, "기후 및 환경:", false, false);
            AddRadio(120, 155, 208, 209, node.ClimateType == EcoClimateType.Temperate, 20); AddLabel(145, 155, 0, "일반/온대");
            AddRadio(220, 155, 208, 209, node.ClimateType == EcoClimateType.Arctic, 21); AddLabel(245, 155, 1152, "설원/북극");
            AddRadio(320, 155, 208, 209, node.ClimateType == EcoClimateType.Tropical, 22); AddLabel(345, 155, 68, "열대/정글");
            AddRadio(120, 180, 208, 209, node.ClimateType == EcoClimateType.Desert, 23); AddLabel(145, 180, 53, "사막");
            AddRadio(220, 180, 208, 209, node.ClimateType == EcoClimateType.Coastal, 24); AddLabel(245, 180, 89, "해안가");
            AddRadio(320, 180, 208, 209, node.ClimateType == EcoClimateType.Swamp, 25); AddLabel(345, 180, 167, "늪지대");
            AddRadio(120, 205, 208, 209, node.ClimateType == EcoClimateType.Volcanic, 26); AddLabel(145, 205, 33, "화산/지하");
            AddRadio(220, 205, 208, 209, node.ClimateType == EcoClimateType.Void, 27); AddLabel(245, 205, 275, "공허/TerMur");

            // 반경 설정
            AddHtml(20, 255, 150, 20, "스폰 탐색 반경:", false, false);
            AddBackground(150, 255, 50, 20, 9300);
            AddTextEntry(150, 255, 50, 20, 0, 30, node.SpawnRange.ToString());

            AddHtml(20, 285, 150, 20, "몬스터 배회 반경:", false, false);
            AddBackground(150, 285, 50, 20, 9300);
            AddTextEntry(150, 285, 50, 20, 0, 31, node.HomeRange.ToString());

            AddButton(180, 340, 2128, 2129, 1, GumpButtonType.Reply, 0); // OK
        }



        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Node == null || m_Node.Deleted) return;

            if (info.ButtonID == 1) // OK 버튼
            {
                if (int.TryParse(info.GetTextEntry(30)?.Text, out int sRange)) m_Node.SpawnRange = sRange;
                if (int.TryParse(info.GetTextEntry(31)?.Text, out int hRange)) m_Node.HomeRange = hRange;

                foreach (int switchId in info.Switches)
                {
                    if (switchId >= 10 && switchId <= 13) m_Node.AreaType = (EcoAreaType)(switchId - 10);
                    if (switchId >= 20 && switchId <= 27) m_Node.ClimateType = (EcoClimateType)(switchId - 20);
                }

                // 매니저에 실시간 동기화
                foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();
                sender.Mobile.SendMessage(68, "생태계 설정이 저장되었습니다.");
            }
        }
    }
}