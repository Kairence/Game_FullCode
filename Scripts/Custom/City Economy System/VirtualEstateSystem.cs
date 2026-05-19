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

                // 🌟 [핵심 패치 추가] 가옥 내부의 물리적 가구(PlacedFurniture)를 서버 세상에서 완전히 파기
                if (HouseData.Interior != null && HouseData.Interior.PlacedFurniture != null)
                {
                    var furnitures = HouseData.Interior.PlacedFurniture.ToList();
                    foreach (var furniture in furnitures)
                    {
                        if (furniture != null && !furniture.Deleted)
                            furniture.Delete();
                    }
                    HouseData.Interior.PlacedFurniture.Clear();
                }
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

            Timer.DelayCall(TimeSpan.Zero, () =>
            {
                if (TownEconomyManager.Towns.TryGetValue(townID, out var town))
                {
                    TownData = town;
                    string searchKey = HouseName.Replace("의 가택", "").Trim();
                    HouseData = town.Houses.FirstOrDefault(h => h.HouseName == searchKey || h.HouseName == HouseName);

                    if (HouseData != null)
                    {
                        HouseData.EstateSign = this;
                        
                        if (!IsConstructionFinished && TownData != null) 
                        {
                            ConstructionStarter.Resume(this);
                        }
                        else if (IsConstructionFinished) 
                        {
                            if (HouseData.Interior == null)
                                HouseData.Interior = new VirtualHouseInterior(HouseData);
                            
                            int multiID = HouseData.MultiID;
                            if (multiID <= 0)
                            {
                                Console.WriteLine($"[HousingSystem] '{HouseName}' 가옥의 MultiID가 유효하지 않아 3D 매트릭스 복구를 생략합니다.");
                                return;
                            }
                            
                            var components = MultiData.GetComponents(multiID).List.Select(t => ((int)t.m_ItemID, (int)t.m_OffsetX, (int)t.m_OffsetY, (int)t.m_OffsetZ)).ToArray();
                            HouseData.Interior.GenerateMatrix(components);

                            if (HouseData.Interior.PlacedFurniture == null)
                                HouseData.Interior.PlacedFurniture = new List<Item>();
                            else
                                HouseData.Interior.PlacedFurniture.Clear();

                            IPooledEnumerable eable = this.Map.GetItemsInRange(this.Location, 12);
                            foreach (Item item in eable)
                            {
                                if (item is Container && !item.Deleted && !item.Movable)
                                {
                                    HouseData.Interior.PlacedFurniture.Add(item);
                                }
                            }
                            eable.Free();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[HousingSystem] 주인을 잃은 유령 가옥 철거됨: {HouseName}");
                        this.DestroyEstate();
                    }
                }
                else
                {
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

            IPooledEnumerable<Item> eable = _sign.Map.GetItemsInRange(_sign.Location, 20);
            foreach (Item item in eable)
            {
                if (item is LockedDoor door && !_sign.AttachedDoors.Contains(door))
                {
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

            // 🌟 [신규 추가] 집 공사가 끝났으므로 3D 인테리어 매트릭스 생성
            if (_sign.HouseData.Interior == null)
            {
                _sign.HouseData.Interior = new VirtualHouseInterior(_sign.HouseData);
            }
            
            _sign.HouseData.Interior.GenerateMatrix(_blueprint);
			Console.WriteLine($"[HousingSystem] '{_sign.HouseData.HouseName}' 가문의 3D 인테리어 매트릭스 구축 완료! 위치: {_sign.Map} ({_sign.X}, {_sign.Y}, {_sign.Z})");
        }
    }

    public static class ConstructionStarter
    {
        public static void Resume(VirtualEstateSign sign)
        {
            if (sign == null || sign.HouseData == null) return;
            int multiID = sign.HouseData.MultiID;
            
            if (multiID <= 0) return; // 텐트 및 비정상 ID 차단
            
            var components = MultiData.GetComponents(multiID).List.Select(t => ((int)t.m_ItemID, (int)t.m_OffsetX, (int)t.m_OffsetY, (int)t.m_OffsetZ)).ToArray();
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
            (0x01F4, -1, -1, 0),
            (0x01F1,  0, -1, 0),
            (0x01F5,  1, -1, 0),
            (0x01F0, -1,  0, 0),
            (0x01F3,  0,  0, 0),
            (0x01F2,  1,  0, 0),
            (0x01F6, -1,  1, 0),
            (0x01F7,  1,  1, 0),
            (0x01F8, -1,  2, 0),
            (0x01F9,  1,  2, 0),
            (0x0A59,  0,  1, 0),
            (0x0FAC,  0,  3, 0) 
        ];
    }

    // ==============================================================================
    // 🌟 [신규 추가] 3D 인테리어 매트릭스 및 가구 배치 AI 시스템
    // ==============================================================================
    public class VirtualHouseInterior
    {
        public VirtualHouse House { get; private set; }
        
        // Key: Z축(층의 높이), Value: 해당 층의 2D 그리드 상태
        // 0: 불가/동선(벽, 문, 계단 앞), 1: 빈 바닥, 2: 테이블 표면, 3: 가구 점유, 4: 의자
        public Dictionary<int, int[,]> FloorGrids { get; private set; }
        
        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        // 집에 물리적으로 배치된 락다운(Lockdown) 가구들 (도둑 타겟)
        public List<Item> PlacedFurniture { get; set; } = new List<Item>();

        public VirtualHouseInterior(VirtualHouse house)
        {
            House = house;
            FloorGrids = new Dictionary<int, int[,]>();
        }

        /// <summary>
        /// 집이 완공될 때 Blueprint를 스캔하여 3D 인테리어 매트릭스를 생성합니다.
        /// </summary>
        public void GenerateMatrix((int ItemID, int X, int Y, int Z)[] blueprint)
        {
            if (blueprint == null || blueprint.Length == 0) return;

            MinX = blueprint.Min(t => t.X);
            MinY = blueprint.Min(t => t.Y);
            int MaxX = blueprint.Max(t => t.X);
            int MaxY = blueprint.Max(t => t.Y);
            
            Width = MaxX - MinX + 1;
            Height = MaxY - MinY + 1;
            FloorGrids.Clear();

            var zGroups = blueprint
                .Where(t => 
                {
                    ItemData id = TileData.ItemTable[t.ItemID & TileData.MaxItemValue];
                    return (id.Flags & TileFlag.Surface) != 0 && (id.Flags & TileFlag.Impassable) == 0;
                })
                .GroupBy(t => t.Z)
                .Where(g => g.Count() >= 4) 
                .Select(g => g.Key)
                .OrderBy(z => z)
                .ToList();

            // 🌟 [패치 1] 텐트처럼 도면에 바닥 타일이 없는 경우, 강제로 0층(맨땅)을 1(빈 공간)로 칠해줍니다.
            if (zGroups.Count == 0)
            {
                zGroups.Add(0);
                int[,] fallbackGrid = new int[Width, Height];
                for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    fallbackGrid[x, y] = 1;
                FloorGrids[0] = fallbackGrid;
            }
            else
            {
                foreach (int zLevel in zGroups)
                {
                    FloorGrids[zLevel] = new int[Width, Height];
                }
            }

            foreach (var tile in blueprint)
            {
                int localX = tile.X - MinX;
                int localY = tile.Y - MinY;
                ItemData id = TileData.ItemTable[tile.ItemID & TileData.MaxItemValue];
                
                var targetZ = zGroups.Where(z => z <= tile.Z).OrderByDescending(z => z).FirstOrDefault();
                if (!FloorGrids.ContainsKey(targetZ)) continue;
                
                var grid = FloorGrids[targetZ];

                // 이미 1로 채워진 fallbackGrid가 아닐 때만 Surface 타일을 1로 마킹
                if (zGroups.Count > 1 || grid[localX, localY] == 0)
                {
                    if ((id.Flags & TileFlag.Surface) != 0 && (id.Flags & TileFlag.Impassable) == 0)
                    {
                        grid[localX, localY] = 1;
                    }
                }
                
                // 장애물, 벽, 문은 0으로 막아버림
                if ((id.Flags & TileFlag.Wall) != 0 || (id.Flags & TileFlag.Impassable) != 0 || (id.Flags & TileFlag.Door) != 0)
                {
                    grid[localX, localY] = 0;
                }
            }

            ProtectPathways(blueprint, zGroups);
        }

        private void ProtectPathways((int ItemID, int X, int Y, int Z)[] blueprint, List<int> zGroups)
        {
            foreach (var tile in blueprint)
            {
                ItemData id = TileData.ItemTable[tile.ItemID & TileData.MaxItemValue];
                bool isDoorOrStair = (id.Flags & TileFlag.Door) != 0 || tile.ItemID == 0x07A3; // 계단 타일 예시

                if (isDoorOrStair)
                {
                    var targetZ = zGroups.Where(z => z <= tile.Z).OrderByDescending(z => z).FirstOrDefault();
                    if (!FloorGrids.ContainsKey(targetZ)) continue;
                    
                    var grid = FloorGrids[targetZ];
                    int cx = tile.X - MinX;
                    int cy = tile.Y - MinY;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = cx + dx;
                            int ny = cy + dy;
                            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                            {
                                grid[nx, ny] = 0;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 특정 층에서 가구를 놓을 '가장 예쁜(벽에 붙은) 빈 공간'을 찾아 반환합니다.
        /// 1층(floorIdx=0)은 공방/접객용, 2층 이상은 귀중품 금고용으로 우선 스캔합니다.
        /// </summary>
        public (bool Success, Point3D Location) FindBestPlacementSpot(int floorIdx = 0)
        {
            if (FloorGrids.Count == 0) return (false, Point3D.Zero);

            // 층 인덱스가 범위를 벗어나면 가장 꼭대기 층 선택
            var orderedFloors = FloorGrids.Keys.OrderBy(z => z).ToList();
            if (floorIdx >= orderedFloors.Count) floorIdx = orderedFloors.Count - 1;
            
            int targetFloorZ = orderedFloors[floorIdx];
            var grid = FloorGrids[targetFloorZ];
            List<Point2D> candidateSpots = new List<Point2D>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (grid[x, y] == 1) // 1: 빈 바닥
                    {
                        // 벽(0)과 인접해 있는지 확인 (가구는 벽에 붙어야 예쁨)
                        bool touchesWall = false;
                        if (x == 0 || grid[x - 1, y] == 0) touchesWall = true;
                        if (x == Width - 1 || grid[x + 1, y] == 0) touchesWall = true;
                        if (y == 0 || grid[x, y - 1] == 0) touchesWall = true;
                        if (y == Height - 1 || grid[x, y + 1] == 0) touchesWall = true;

                        if (touchesWall) candidateSpots.Add(new Point2D(x, y));
                    }
                }
            }

            if (candidateSpots.Count > 0)
            {
                Point2D spot = candidateSpots[Utility.Random(candidateSpots.Count)];
                grid[spot.X, spot.Y] = 3; // 배치 완료 마킹 (더 이상 겹치지 않게 보호)

                Point3D worldLoc = new Point3D(House.EstateSign.X + MinX + spot.X, House.EstateSign.Y + MinY + spot.Y, targetFloorZ);
                return (true, worldLoc);
            }

            // 만약 원하는 층이 꽉 찼다면 바로 아래층도 빈자리를 찾아봄
            if (floorIdx > 0) return FindBestPlacementSpot(floorIdx - 1);
            
            return (false, Point3D.Zero);
        }
    }
}