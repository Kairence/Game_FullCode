using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;
using Server.Mobiles;

namespace Server.Misc
{
    public class ExAddonSystem
    {
		public static void Initialize()
        {
            Server.Commands.CommandSystem.Register("ExAddon", AccessLevel.Player, new Server.Commands.CommandEventHandler(ExAddon_OnCommand));
            // ★ [추가] 철거 명령어 등록
            Server.Commands.CommandSystem.Register("RemoveExAddon", AccessLevel.Player, new Server.Commands.CommandEventHandler(RemoveExAddon_OnCommand));
        }
        [Usage("ExAddon")]
        [Description("집의 동쪽 면을 전체적으로 3칸 확장합니다.")]
        private static void ExAddon_OnCommand(Server.Commands.CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            BaseHouse house = BaseHouse.FindHouseAt(from);
            
            if (house != null && house.IsOwner(from))
            {
                ExpandEast(from, house);
            }
            else
            {
                from.SendMessage(33, "당신 소유의 집 내부나 마당 위에서 이 명령어를 입력해야 합니다.");
            }
        }

		// =======================================================================
        // ★ [테스트용 명령어] 집 안에서 [RemoveExAddon 입력 시 확장 영토 전체 철거
        // =======================================================================
        [Usage("RemoveExAddon")]
        [Description("집에 확장된 모든 영토를 철거하고 락다운을 반환받습니다.")]
        private static void RemoveExAddon_OnCommand(Server.Commands.CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            BaseHouse house = BaseHouse.FindHouseAt(from);
            
            if (house != null && house.IsOwner(from))
            {
                ClearExpansion(from, house);
            }
            else
            {
                from.SendMessage(33, "당신 소유의 집 내부나 마당 위에서 이 명령어를 입력해야 합니다.");
            }
        }

        // =======================================================================
        // ★ [핵심 로직] 영토 깔끔하게 지우기 (스마트 철거)
        // =======================================================================
        public static void ClearExpansion(Mobile from, BaseHouse house)
        {
            if (from == null || house == null || !house.IsOwner(from)) return;

            // 1. 내 집의 락다운 목록 중에서 '확장 영토 타일'만 쏙쏙 골라냅니다.
            List<Item> toDelete = new List<Item>();
            foreach (Item item in house.LockDowns.Keys)
            {
                if (item is ExAddOnTile tile && tile.LinkedHouse == house)
                {
                    toDelete.Add(item);
                }
            }

            // 2. 지울 게 없다면 안내 메시지 출력
            if (toDelete.Count == 0)
            {
                from.SendMessage(33, "철거할 확장 영토가 없습니다.");
                return;
            }

            // 3. 골라낸 타일들을 락다운에서 해제하고 월드에서 완벽하게 삭제합니다.
            foreach (Item item in toDelete)
            {
                house.LockDowns.Remove(item);
                item.Delete();
            }

            from.SendMessage(68, $"성공적으로 확장 영토를 모두 철거했습니다! ({toDelete.Count}개의 락다운이 반환되었습니다.)");
        }

        // =======================================================================
        // ★ [핵심 로직] 동쪽 통째로 확장 & 스마트 철거
        // =======================================================================
        public static void ExpandEast(Mobile from, BaseHouse house)
        {
            if (from == null || house == null || !house.IsOwner(from)) return;

            Map map = house.Map;
            int baseZ = house.Z;
            int foundationZ = house.Z + 7;

            // 1. 집의 물리적 Y축 범위와 진짜 동쪽 끝점(X) 찾기 (투명 타일 무시)
            int hMinY = int.MaxValue, hMaxY = int.MinValue, hMaxX = int.MinValue;
            MultiComponentList mcl = house.Components;

            for (int x = 0; x < mcl.Width; ++x)
            {
                for (int y = 0; y < mcl.Height; ++y)
                {
                    StaticTile[] tiles = mcl.Tiles[x][y];
                    
                    if (tiles.Length > 0 || (house is Castle && mcl.Width > 0 && mcl.Height > 0))
                    {
                        int absY = house.Y + mcl.Min.Y + y;
                        if (absY < hMinY) hMinY = absY;
                        if (absY > hMaxY) hMaxY = absY;

                        foreach (var tile in tiles)
                        {
                            int id = tile.ID & TileData.MaxItemValue;
                            ItemData data = TileData.ItemTable[id];

                            if ((data.Flags & TileFlag.Roof) != 0) continue; 
                            if (id >= 0xB95 && id <= 0xC0E || id >= 0xC43 && id <= 0xC44) continue; 

                            int absX = house.X + mcl.Min.X + x;
                            if (absX > hMaxX) hMaxX = absX;
                            break; 
                        }
                    }
                }
            }

            // 2. 현재 마당 상태 파악 및 확장 구역 설정
            int currentMaxX = hMaxX;
            foreach (Item item in house.LockDowns.Keys)
            {
                if (item is ExAddOnTile tile && tile.LinkedHouse == house && tile.X > currentMaxX)
                    currentMaxX = tile.X;
            }

            int expandWidth = 3; 
            int startX = currentMaxX + 1;
            int endX = currentMaxX + expandWidth;

            // 3. 락다운 여유 체크 및 장애물 검사
            int estimatedLockdowns = ((hMaxY - hMinY + 1) * expandWidth) * 2;
            if (house.LockDownCount + estimatedLockdowns > house.MaxLockDowns)
            {
                from.SendMessage(33, $"락다운 수치가 부족합니다. (최소 {estimatedLockdowns} 필요)");
                return;
            }

            for (int x = startX; x <= endX; x++)
            {
                for (int y = hMinY; y <= hMaxY; y++)
                {
                    if (!map.CanFit(x, y, baseZ, 20, true, false))
                    {
                        from.SendMessage(33, $"장애물이 있어 더 이상 동쪽으로 확장할 수 없습니다. 좌표: {x}, {y}");
                        return;
                    }
                    if (BaseHouse.FindHouseAt(new Point3D(x, y, baseZ), map, 16) != null)
                    {
                        from.SendMessage(33, "해당 방향에 다른 건물이 있어 확장할 수 없습니다.");
                        return;
                    }
                }
            }

            // 4. ★ [수정됨] 스마트 철거 (남쪽 모서리는 살려서 직진 타일로 변경!)
            List<Item> oldWallsToDelete = new List<Item>();
            foreach (Item item in house.LockDowns.Keys)
            {
                if (item is ExAddOnTile tile && tile.LinkedHouse == house && tile.X == currentMaxX && tile.Z == baseZ)
                {
                    if (tile.ItemID == 100) // 기존 동쪽 직진 벽이라면
                    {
                        oldWallsToDelete.Add(tile); // 삭제 리스트에 추가
                    }
                    else if (tile.ItemID == 101) // 기존 남동쪽 모서리 벽이라면
                    {
                        tile.ItemID = 99; // 💡 지우지 않고 남쪽 직진 벽(99)으로 그래픽만 슬쩍 변경!
                    }
                }
            }
            foreach (Item wall in oldWallsToDelete)
            {
                house.LockDowns.Remove(wall);
                wall.Delete();
            }

            // 5. ★ [수정됨] 새로운 마당 3칸 건설 (북쪽 99번 타일 추가)
            int count = 0;
            for (int x = startX; x <= endX; x++)
            {
                for (int y = hMinY; y <= hMaxY; y++)
                {
                    Point3D floorLoc = new Point3D(x, y, baseZ);
                    Point3D dirtLoc = new Point3D(x, y, foundationZ);

                    // A. 흙바닥 깔기
                    CreateExTile(house, from, 0x31F4, dirtLoc, map); count++;

                    // B. 가장자리 판별 후 돌벽 세우기
                    bool exposeEast = (x == endX);
                    bool exposeSouth = (y == hMaxY);
                    bool exposeNorth = (y == hMinY);

                    // 북쪽 막기 (유저 요청: 99번 타일)
                    if (exposeNorth) { CreateExTile(house, from, 99, floorLoc, map); count++; }

                    // 남쪽 및 동쪽 모서리 처리
                    if (exposeEast && exposeSouth) { CreateExTile(house, from, 101, floorLoc, map); count++; }
                    else
                    {
                        if (exposeEast) { CreateExTile(house, from, 100, floorLoc, map); count++; }
                        if (exposeSouth) { CreateExTile(house, from, 99, floorLoc, map); count++; }
                    }
                }
            }
            from.SendMessage(68, $"동쪽으로 영토가 성공적으로 확장되었습니다! ({count}개의 락다운 소모)");
        }

        // 타일 생성 헬퍼
        private static void CreateExTile(BaseHouse house, Mobile from, int itemID, Point3D loc, Map map)
        {
            ExAddOnTile tile = new ExAddOnTile(house, itemID);
            tile.MoveToWorld(loc, map);
            house.LockDowns.Add(tile, from);
            tile.IsLockedDown = true;
        }

        // =======================================================================
        // ★ [시큐어 작동을 위한 필수 헬퍼] 
        // =======================================================================
        public static bool CheckVirtualYard(BaseHouse house, Point3D loc)
        {
            if (house == null) return false;
            // 💡 맵 검색 대신 내 집의 락다운 리스트를 직접 뒤져서 100% 정확하게 잡아냅니다.
            foreach (Item item in house.LockDowns.Keys)
            {
                if (item is ExAddOnTile tile && tile.X == loc.X && tile.Y == loc.Y)
                    return true;
            }
            return false;
        }

        public static BaseHouse FindHouseByYard(Point3D loc, Map map)
        {
            if (map == null || map == Map.Internal) return null;
            IPooledEnumerable eable = map.GetItemsInRange(loc, 0); 
            foreach (Item item in eable)
            {
                if (item is ExAddOnTile tile && tile.LinkedHouse != null && !tile.LinkedHouse.Deleted)
                {
                    eable.Free();
                    return tile.LinkedHouse; 
                }
            }
            eable.Free();
            return null;
        }
    }

    public class ExAddOnTile : Item
    {
        private BaseHouse m_House;
        [CommandProperty(AccessLevel.GameMaster)]
        public BaseHouse LinkedHouse { get { return m_House; } }

        [Constructable]
        public ExAddOnTile(BaseHouse house, int itemID) : base(itemID) 
        {
            Name = "확장된 터"; Movable = false; m_House = house;
        }
        public ExAddOnTile(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_House); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); m_House = reader.ReadItem() as BaseHouse; }
    }
}