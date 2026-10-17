using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using Server.Commands;

namespace Server.Custom
{
    public static class HouseCraftSystem
    {
        // 메모리 상의 플레이어별 설정 (서버 세션 유지)
        private static readonly Dictionary<Serial, bool> m_HouseResourceEnabled = new Dictionary<Serial, bool>();
        private static readonly Dictionary<Serial, bool> m_AutoStoreEnabled = new Dictionary<Serial, bool>();
        private static readonly Dictionary<Serial, Container> m_StoreContainers = new Dictionary<Serial, Container>();

        public static void Initialize()
        {
            CommandSystem.Register("집자원", AccessLevel.Player, new CommandEventHandler(OnToggleHouseResource));
            CommandSystem.Register("제작보관", AccessLevel.Player, new CommandEventHandler(OnToggleAutoStore));
        }

        [Usage("집자원")]
        [Description("집 자원 자동 연동 제작 토글")]
        public static void OnToggleHouseResource(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile pm)
            {
                bool current = IsHouseResourceEnabled(pm);
                SetHouseResourceEnabled(pm, !current);
                pm.SendMessage(!current ? 0x42 : 0x22, $"[하우징 자원 연동] 집 안 시큐어 상자 자원 사용이 {(!current ? "활성화" : "비활성화")}되었습니다.");
            }
        }

        [Usage("제작보관")]
        [Description("제작품 지정 상자 자동 보관 토글")]
        public static void OnToggleAutoStore(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile pm)
            {
                bool current = IsAutoStoreEnabled(pm);
                SetAutoStoreEnabled(pm, !current);
                pm.SendMessage(!current ? 0x42 : 0x22, $"[제작품 자동 보관] 완성품 자동 상자 보관이 {(!current ? "활성화" : "비활성화")}되었습니다.");
            }
        }

        public static bool IsHouseResourceEnabled(Mobile m)
        {
            if (m == null) return false;
            return m_HouseResourceEnabled.TryGetValue(m.Serial, out bool enabled) && enabled;
        }

        public static void SetHouseResourceEnabled(Mobile m, bool val)
        {
            if (m != null)
                m_HouseResourceEnabled[m.Serial] = val;
        }

        public static bool IsAutoStoreEnabled(Mobile m)
        {
            if (m == null) return false;
            return m_AutoStoreEnabled.TryGetValue(m.Serial, out bool enabled) && enabled;
        }

        public static void SetAutoStoreEnabled(Mobile m, bool val)
        {
            if (m != null)
                m_AutoStoreEnabled[m.Serial] = val;
        }

        public static Container GetStoreContainer(Mobile m)
        {
            if (m == null) return null;
            if (m_StoreContainers.TryGetValue(m.Serial, out Container c))
            {
                if (c != null && !c.Deleted)
                    return c;
                m_StoreContainers.Remove(m.Serial);
            }
            return null;
        }

        public static void SetStoreContainer(Mobile m, Container c)
        {
            if (m != null)
            {
                if (c == null)
                    m_StoreContainers.Remove(m.Serial);
                else
                    m_StoreContainers[m.Serial] = c;
            }
        }

        // 플레이어가 유효한 권한(소유자, 부소유자, 친구)을 가진 집 안에 있는지 확인
        public static BaseHouse GetValidHouse(Mobile m)
        {
            if (m == null || m.Deleted || m.Map == null || m.Map == Map.Internal)
                return null;

            BaseHouse house = BaseHouse.FindHouseAt(m);
            if (house == null || !house.IsActive)
                return null;

            if (house.IsOwner(m) || house.IsCoOwner(m) || house.IsFriend(m))
                return house;

            return null;
        }

        // 접근 가능한 집 안의 시큐어 상자 목록 조회
        public static List<Container> GetAccessibleSecures(Mobile m, BaseHouse house)
        {
            var list = new List<Container>();
            if (house == null || house.Secures == null)
                return list;

            for (int i = 0; i < house.Secures.Count; i++)
            {
                SecureInfo info = house.Secures[i];
                if (info != null && info.Item is Container c && !c.Deleted)
                {
                    if (house.HasSecureAccess(m, info))
                    {
                        list.Add(c);
                    }
                }
            }

            return list;
        }

        // 집 시큐어 상자들에 보유 중인 특정 재료 총합 수량 계산
        public static int GetAvailableAmountInHouse(Mobile m, Type[] types)
        {
            BaseHouse house = GetValidHouse(m);
            if (house == null) return 0;

            List<Container> secures = GetAccessibleSecures(m, house);
            int total = 0;

            for (int i = 0; i < secures.Count; i++)
            {
                Container c = secures[i];
                for (int t = 0; t < types.Length; t++)
                {
                    total += c.GetAmount(types[t], true);
                }
            }

            return total;
        }

        // 가방에 부족한 수량만큼 집 시큐어 상자들에서 자원 차감
        public static bool ConsumeFromHouse(Mobile m, Type[] types, int needAmount)
        {
            if (needAmount <= 0) return true;

            BaseHouse house = GetValidHouse(m);
            if (house == null) return false;

            List<Container> secures = GetAccessibleSecures(m, house);
            int remaining = needAmount;

            for (int i = 0; i < secures.Count && remaining > 0; i++)
            {
                Container c = secures[i];
                for (int t = 0; t < types.Length && remaining > 0; t++)
                {
                    int inBox = c.GetAmount(types[t], true);
                    if (inBox > 0)
                    {
                        int toConsume = Math.Min(inBox, remaining);
                        c.ConsumeTotal(types[t], toConsume, true);
                        remaining -= toConsume;
                    }
                }
            }

            return remaining == 0;
        }

        // 완성된 아이템을 지정된 집 시큐어 상자로 자동 적재
        public static bool TryAutoStore(Mobile m, Item item)
        {
            if (m == null || item == null || item.Deleted)
                return false;

            if (!IsAutoStoreEnabled(m))
                return false;

            BaseHouse house = GetValidHouse(m);
            if (house == null)
                return false;

            Container targetBox = GetStoreContainer(m);
            if (targetBox == null || targetBox.Deleted)
                return false;

            // 상자가 동일한 집 안에 있는지 확인
            if (BaseHouse.FindHouseAt(targetBox) != house)
                return false;

            // 상자 용량/무게 한도 검사
            if (targetBox.TotalItems >= targetBox.MaxItems || (targetBox.TotalWeight + item.Weight) > targetBox.MaxWeight)
            {
                m.SendMessage(0x22, "[보관함 초과] 지정된 보관함의 용량이 가득 차 아이템이 가방으로 들어왔습니다.");
                return false;
            }

            targetBox.DropItem(item);
            string itemName = !string.IsNullOrEmpty(item.Name) ? item.Name : "제작품";
            m.SendMessage(0x42, $"[제작품 수납] {itemName}이(가) 지정된 보관함으로 자동 보관되었습니다.");
            return true;
        }

        // 보관함 타겟 지정 클래스
        public class StoreContainerTarget : Target
        {
            public StoreContainerTarget() : base(8, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (from == null || from.Deleted)
                    return;

                if (targeted is Container c)
                {
                    BaseHouse house = GetValidHouse(from);
                    if (house == null)
                    {
                        from.SendMessage(0x22, "자신의 집(또는 권한이 있는 집) 안에서만 보관함을 지정할 수 있습니다.");
                        return;
                    }

                    if (BaseHouse.FindHouseAt(c) != house)
                    {
                        from.SendMessage(0x22, "집 안에 위치한 상자만 보관함으로 지정할 수 있습니다.");
                        return;
                    }

                    if (c.IsChildOf(from.Backpack))
                    {
                        from.SendMessage(0x22, "개인 가방 안의 주머니는 지정할 수 없습니다. 집 안의 고정/시큐어 상자를 선택하세요.");
                        return;
                    }

                    SetStoreContainer(from, c);
                    SetAutoStoreEnabled(from, true);
                    string name = !string.IsNullOrEmpty(c.Name) ? c.Name : c.ItemData.Name;
                    from.SendMessage(0x42, $"[보관함 설정 완료] '{name}'이(가) 제작품 자동 보관함으로 등록되었습니다.");

                    if (from is PlayerMobile pm)
                    {
                        pm.SendGump(new UserControlGump(pm, UserControlTab.Utility));
                    }
                }
                else
                {
                    from.SendMessage(0x22, "상자(Container) 아이템만 보관함으로 지정할 수 있습니다.");
                }
            }
        }
    }
}
