using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Multis; 

namespace Server.Misc
{
    public static class VirtualEstateSystem
    {
        public static int GetBaseMultiPrice(int multiID) => multiID switch
        {
            0x006A => 35000,
            0x0068 or 0x006C or 0x006E => 36500,
            0x0064 or 0x0066 => 36750,
            0x00A0 => 50250,
            0x00A2 => 52250,
            0x0098 => 73250,
            0x009C => 76250,
            0x009A => 81250,
            0x009E => 113500,
            0x008C => 129000,
            0x0074 => 131250,
            0x0096 => 160250,
            0x0076 => 162500,
            0x0078 => 162750,
            0x007A => 366250,
            0x007C => 562500,
            0x007E => 865000,
            _ => 35000
        };
    }

    public class VirtualEstateSign : Item
    {
        public VirtualHouse HouseData { get; set; }
        public TownEconomy TownData { get; set; }
        public string HouseName { get; set; }
        public List<Static> AttachedTiles { get; set; } = new();
        public List<LockedDoor> AttachedDoors { get; set; } = new(); 
        public bool IsConstructionFinished { get; set; }
        public int BuildIndex { get; set; }

        public VirtualEstateSign(string name, VirtualHouse house, TownEconomy town) : base(0x0BD2) 
        {
            HouseName = name;
            Movable = false;
            Visible = false;
            HouseData = house;
            TownData = town;
            BuildIndex = 0;
        }

        public VirtualEstateSign(Serial serial) : base(serial) 
        { 
            AttachedTiles = new List<Static>();
            AttachedDoors = new List<LockedDoor>();
        }

        public void DestroyEstate()
		{
			if (HouseData != null)
			{
				var chunk = VirtualHousingRegistry.Chunks.FirstOrDefault(c => c.Facet == this.Map && c.Bounds.Contains(new Point2D(this.X, this.Y)));
				if (chunk != null) chunk.OccupiedLots.RemoveAll(lot => lot.HouseName == HouseData.HouseName);
			}

			// 🌟 [안전 패치] ToList()를 사용하여 리스트를 복사한 뒤 삭제해야 루프 에러가 없습니다.
			if (AttachedTiles != null)
			{
				var tiles = AttachedTiles.ToList();
				foreach (var tile in tiles)
				{
					if (tile != null && !tile.Deleted)
						tile.Delete();
				}
				AttachedTiles.Clear();
			}

			if (AttachedDoors != null)
			{
				var doors = AttachedDoors.ToList();
				foreach (var door in doors)
				{
					if (door != null && !door.Deleted)
						door.Delete();
				}
				AttachedDoors.Clear();
			}

			this.Delete();
		}

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)5); 
            writer.Write(HouseName);
            writer.Write(IsConstructionFinished);
            writer.Write(BuildIndex); 
            writer.Write(TownData != null ? TownData.TownID : 0);
            writer.WriteItemList(AttachedTiles, true);
            writer.WriteItemList(AttachedDoors, true);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            HouseName = reader.ReadString(); 
            IsConstructionFinished = reader.ReadBool();
            if (version >= 2) BuildIndex = reader.ReadInt();
            int townID = (version >= 3) ? reader.ReadInt() : 0;
            if (version >= 4)
            {
                AttachedTiles = reader.ReadStrongItemList<Static>();
                AttachedDoors = reader.ReadStrongItemList<LockedDoor>();
            }

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () => 
            {
                if (TownEconomyManager.Towns.TryGetValue(townID, out var town))
                {
                    TownData = town;
                    string searchKey = HouseName.Replace("의 가택", "").Trim();
                    HouseData = town.Houses.FirstOrDefault(h => h.HouseName == searchKey || h.HouseName == HouseName);

                    // 🌟 [핵심 패치] 가문 데이터 연결 및 고아 가옥(Ghost House) 철거 로직
                    if (HouseData != null)
                    {
                        HouseData.EstateSign = this;
                        
                        if (!IsConstructionFinished && TownData != null) 
                            ConstructionStarter.Resume(this);
                    }
                    else
                    {
                        // 주인을 찾지 못했다면 이 집은 무효한 데이터이므로 즉시 월드에서 철거!
                        Console.WriteLine($"[HousingSystem] 주인을 잃은 유령 가옥 철거됨: {HouseName}");
                        this.DestroyEstate();
                    }
                }
                else
                {
                    // 마을 데이터 자체가 날아갔을 경우에도 철거
                    Console.WriteLine($"[HousingSystem] 소속 마을을 찾을 수 없는 가옥 철거됨: {HouseName}");
                    this.DestroyEstate();
                }
            });
        }
        
        public override void OnDoubleClick(Mobile from)
        {
            if (!IsConstructionFinished) { from.SendMessage("아직 공사가 진행 중입니다."); return; }
            if (from == null || HouseData == null || TownData == null) return;
            if (!from.InRange(this.GetWorldLocation(), 3)) { from.SendLocalizedMessage(500446); return; }
            from.CloseGump(typeof(VirtualEstateGump));
            from.SendGump(new VirtualEstateGump(from, HouseData, TownData));
        }
    }

    public class VirtualEstateGump : Gump
    {
        private readonly Mobile m_Viewer;
        private readonly VirtualHouse m_House;
        private readonly TownEconomy m_Town;
        private readonly int m_PremiumPrice;

        public VirtualEstateGump(Mobile viewer, VirtualHouse house, TownEconomy town) : base(50, 50)
        {
            m_Viewer = viewer;
            m_House = house;
            m_Town = town;
            int baseMultiPrice = VirtualEstateSystem.GetBaseMultiPrice(house.MultiID);
            m_PremiumPrice = (int)(baseMultiPrice * 1.5) + (house.Prestige * 10);
            SetupGumpLayout();
        }

        private void SetupGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 400, 320, 5054);
            AddImageTiled(10, 10, 380, 20, 2624);
            AddAlphaRegion(10, 10, 380, 300);
            
            AddHtml(10, 12, 380, 20, $"<center><color=#ffffff>부동산 정보: {m_House.HouseName} 가문</color></center>", false, false);
            int y = 40;
            AddHtml(15, y, 370, 20, $"<color=#f0e68c>가주 직업:</color> {m_House.PrimaryJob}", false, false);
            AddHtml(15, y += 25, 370, 20, $"<color=#f0e68c>가문 작위:</color> {m_House.PrimaryRank}", false, false);
            AddHtml(15, y += 25, 370, 20, $"<color=#f0e68c>가문 명성:</color> {m_House.Prestige}", false, false);
            AddHtml(15, y += 25, 370, 20, $"<color=#f0e68c>가문 입지:</color> Zone {m_House.ZoneID}", false, false);

            string facilities = "";
            if (m_House.HasGarden) facilities += "텃밭 ";
            if (m_House.HasWorkshop) facilities += "공방 ";
            if (m_House.HasBarracks) facilities += "병영 ";
            if (string.IsNullOrEmpty(facilities)) facilities = "없음";
            
            AddHtml(15, y += 25, 370, 20, $"<color=#f0e68c>부속 시설:</color> {facilities}", false, false);
            AddImageTiled(10, y += 30, 380, 2, 2624);

            AddHtml(15, y += 15, 370, 20, $"<color=#ffffff>강제 매수 (프리미엄 지불): {m_PremiumPrice:N0} gp</color>", false, false);
            AddButton(15, y += 25, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtml(50, y, 300, 20, "<color=#ffd700>즉시 구매하여 가문 쫓아내기</color>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Viewer == null || m_Viewer.Deleted) return;
            if (info.ButtonID == 1) ExecuteBuyout();
        }

        private void ExecuteBuyout()
        {
            if (!Banker.Withdraw(m_Viewer, m_PremiumPrice))
            {
                m_Viewer.SendMessage($"자금이 부족합니다. 은행에 {m_PremiumPrice:N0} gp가 필요합니다.");
                return;
            }

            m_Viewer.SendMessage($"{m_House.HouseName} 가문의 영토를 매입했습니다. 기존 건축물이 철거됩니다.");
            m_House.TotalWealth += m_PremiumPrice; 
            m_House.Prestige = Math.Max(0, m_House.Prestige - 50); 
            
            if (m_House.EstateSign != null)
            {
                m_House.EstateSign.DestroyEstate(); 
                m_House.EstateSign = null;
            }
            m_Viewer.CloseGump(typeof(VirtualEstateGump));
        }
    }

    public class ConstructionTimer : Timer
    {
        private readonly VirtualEstateSign _sign;
        private readonly (int ItemID, int X, int Y, int Z)[] _blueprint;
        private const int TilesPerTick = 10;

        public ConstructionTimer(VirtualEstateSign sign, (int ItemID, int X, int Y, int Z)[] blueprint) 
            : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
        {
            _sign = sign;
            _blueprint = blueprint;
        }

        protected override void OnTick()
        {
            if (_sign == null || _sign.Deleted) { Stop(); return; }
            int placedInThisTick = 0;
            while (placedInThisTick < TilesPerTick && _sign.BuildIndex < _blueprint.Length)
            {
                var tileData = _blueprint[_sign.BuildIndex];
                Point3D buildLoc = new(_sign.X + tileData.X, _sign.Y + tileData.Y, _sign.Z + tileData.Z);
                try {
                    int rawID = tileData.ItemID;
                    if ((rawID & 0x3FFF) != 0x0001) {
                        Static newTile = new(rawID);
                        newTile.MoveToWorld(buildLoc, _sign.Map);
                        _sign.AttachedTiles.Add(newTile);
                        if (placedInThisTick == 0) Effects.PlaySound(buildLoc, _sign.Map, 0x23D);
                        placedInThisTick++;
                    }
                } catch { }
                _sign.BuildIndex++; 
            }
            if (_sign.BuildIndex >= _blueprint.Length) 
            { 
                _sign.IsConstructionFinished = true; 
                _sign.Visible = true; 
                _sign.ItemID = 0x0BD2; 
                ProcessPostConstruction(); 
                Stop(); 
            }
        }

        private void ProcessPostConstruction()
        {
            if (_sign == null || _sign.Deleted || _sign.HouseData == null) return;
            
            var chunk = VirtualHousingRegistry.Chunks.FirstOrDefault(c => c.Facet == _sign.Map && c.Bounds.Contains(new Point2D(_sign.X, _sign.Y)));
            if (chunk != null) _sign.HouseData.ZoneID = chunk.ZoneID;

            List<Static> tilesToRemove = [];
            bool signMoved = false;

            // 🌟 [수정] 맵에 이미 TownSocietyEngine이 깔아놓은 문이 있는지 먼저 스캔합니다.
            // (중복 생성을 막고, 유실된 문을 장부에 강제로 귀속시킵니다.)
            IPooledEnumerable<Item> eable = _sign.Map.GetItemsInRange(_sign.Location, 20);
            foreach (Item item in eable)
            {
                if (item is LockedDoor door && !_sign.AttachedDoors.Contains(door))
                {
                    // 이 문이 이 집의 범위 내에 있다면 장부에 추가
                    _sign.AttachedDoors.Add(door);
                }
            }
            eable.Free();

            foreach (var tile in _sign.AttachedTiles.ToList())
            {
                if (tile == null || tile.Deleted || tile.Name == "공사중") continue;
                int rawID = tile.ItemID;
                ItemData id = TileData.ItemTable[rawID & TileData.MaxItemValue];

                if (!signMoved && (rawID == 0x0B98 || rawID == 0x0BD0 || rawID == 0x0BD2 || (id.Name != null && id.Name.ToLower().Contains("sign"))))
                {
                    _sign.MoveToWorld(tile.Location, tile.Map);
                    tilesToRemove.Add(tile); signMoved = true; continue;
                }

                // 🌟 [중요] 여기서 문을 또 생성(new LockedDoor)하지 마세요! 
                // 이미 TownSocietyEngine에서 생성했으므로, 여기서는 중복된 'Static' 타일만 지워줍니다.
                if ((id.Flags & TileFlag.Door) != 0)
                {
                    tilesToRemove.Add(tile); 
                }
            }

            foreach (var t in _sign.AttachedTiles.Where(t => t.Name == "공사중").ToList()) tilesToRemove.Add(t);
            
            // 실제 타일 삭제 루프
            foreach (var t in tilesToRemove) 
            { 
                _sign.AttachedTiles.Remove(t); 
                t.Delete(); 
            }
        }
    }

    public static class ConstructionStarter
    {
        public static void Resume(VirtualEstateSign sign)
        {
            if (sign == null || sign.HouseData == null) return;
            int multiID = sign.HouseData.MultiID;
            var components = (multiID <= 0) ? CustomBlueprintManager.TentBlueprint : 
                             MultiData.GetComponents(multiID).List.Select(t => ((int)t.m_ItemID, (int)t.m_OffsetX, (int)t.m_OffsetY, (int)t.m_OffsetZ)).ToArray();
            var sorted = components.OrderBy(t => t.Item4).ThenBy(t => t.Item2).ThenBy(t => t.Item3).Select(t => (t.Item1, t.Item2, t.Item3, t.Item4)).ToArray();
            new ConstructionTimer(sign, sorted).Start();
        }

        public static void StartFromMulti(VirtualEstateSign sign, MultiTileEntry[] multiTiles)
        {
            if (sign == null || multiTiles == null) return;
            sign.BuildIndex = 0;
            var sorted = multiTiles.OrderBy(t => t.m_OffsetZ).ThenBy(t => t.m_OffsetX).ThenBy(t => t.m_OffsetY).Select(t => ((int)t.m_ItemID, (int)t.m_OffsetX, (int)t.m_OffsetY, (int)t.m_OffsetZ)).ToArray();
            SetupBarricades(sign, sorted);
            new ConstructionTimer(sign, sorted).Start();
        }
        
        private static void SetupBarricades(VirtualEstateSign sign, (int ItemID, int X, int Y, int Z)[] blueprint)
        {
            if (blueprint.Length == 0) return;
            int minX = blueprint.Min(t => t.X) - 1, maxX = blueprint.Max(t => t.X) + 1;
            int minY = blueprint.Min(t => t.Y) - 1, maxY = blueprint.Max(t => t.Y) + 1;
            for (int x = minX; x <= maxX; x++) { for (int y = minY; y <= maxY; y++) { if (x == minX || x == maxX || y == minY || y == maxY) { Static b = new(0x008A) { Name = "공사중", Hue = 1175, Movable = false }; b.MoveToWorld(new Point3D(sign.X + x, sign.Y + y, sign.Z), sign.Map); sign.AttachedTiles.Add(b); } } }
        }
    }

    public static class CustomBlueprintManager
    {
        public static readonly (int ItemID, int X, int Y, int Z)[] TentBlueprint = 
        [ 
            // =========================================================
            // 🏕️ 클래식 텐트 프레임 (Z축은 모두 0으로 바닥에 고정!)
            // =========================================================
            
            // 뒷면 (북쪽)
            (0x01F4, -1, -1, 0), // 북서쪽 모서리
            (0x01F1,  0, -1, 0), // 북쪽 중앙 벽
            (0x01F5,  1, -1, 0), // 북동쪽 모서리

            // 중간 (동서 벽과 중앙 기둥)
            (0x01F0, -1,  0, 0), // 서쪽(좌측) 벽
            (0x01F3,  0,  0, 0), // 🪵 텐트 중앙 나무 기둥 (Wooden Pole)
            (0x01F2,  1,  0, 0), // 동쪽(우측) 벽

            // 앞면 입구 (남쪽)
            (0x01F6, -1,  1, 0), // 남서쪽 입구 펄럭이는 천막
            (0x01F7,  1,  1, 0), // 남동쪽 입구 펄럭이는 천막
            
            // 입구 앞쪽 지지대 (더 남쪽으로 뻗어나간 텐트 끈)
            (0x01F8, -1,  2, 0), // 남서쪽 끝 지지대
            (0x01F9,  1,  2, 0), // 남동쪽 끝 지지대

            // =========================================================
            // 🔥 내부 및 외부 소품 (텐트의 디테일을 살려주는 장식)
            // =========================================================
            
            (0x0A59,  0,  1, 0), // 🛏️ 텐트 입구 쪽에 깔아둔 침낭 (Bedroll)
            (0x0FAC,  0,  3, 0)  // 🔥 텐트 바로 앞(남쪽)에 피워둔 모닥불 (Fire pit)
        ];
    }
}