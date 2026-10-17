using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public class MobileResourceData
    {
        public int WoundHits { get; set; }
        public int BuffHitsLock { get; set; }
        public int WoundMana { get; set; }
        public int BuffManaLock { get; set; }
        public int WoundStam { get; set; }
        public int BuffStamLock { get; set; }

        public bool HasAny => WoundHits > 0 || BuffHitsLock > 0 ||
                              WoundMana > 0 || BuffManaLock > 0 ||
                              WoundStam > 0 || BuffStamLock > 0;
    }

    public static class MobileResourceSystem
    {
        private static readonly Dictionary<Mobile, MobileResourceData> m_Registry = new Dictionary<Mobile, MobileResourceData>();
        private static readonly string FilePath = System.IO.Path.Combine("Saves", "ResourceLocks", "Persistence.bin");

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void Initialize()
        {
            EventSink.Login += OnLogin;
            EventSink.PlayerDeath += OnPlayerDeath;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(0); // version

                    // Deleted 되었거나 잠금/상흔이 전혀 없는 항목 정리
                    var list = new List<KeyValuePair<Mobile, MobileResourceData>>();
                    foreach (var kvp in m_Registry)
                    {
                        if (kvp.Key != null && !kvp.Key.Deleted && kvp.Value != null && kvp.Value.HasAny)
                        {
                            list.Add(kvp);
                        }
                    }

                    writer.Write(list.Count);
                    foreach (var entry in list)
                    {
                        writer.Write(entry.Key);
                        writer.Write(entry.Value.WoundHits);
                        writer.Write(entry.Value.BuffHitsLock);
                        writer.Write(entry.Value.WoundMana);
                        writer.Write(entry.Value.BuffManaLock);
                        writer.Write(entry.Value.WoundStam);
                        writer.Write(entry.Value.BuffStamLock);
                    }
                });
        }

        private static void OnLoad()
        {
            Persistence.Deserialize(
                FilePath,
                reader =>
                {
                    int version = reader.ReadInt();
                    switch (version)
                    {
                        case 0:
                        {
                            int count = reader.ReadInt();
                            for (int i = 0; i < count; ++i)
                            {
                                Mobile m = reader.ReadMobile();
                                int wHits = reader.ReadInt();
                                int bHits = reader.ReadInt();
                                int wMana = reader.ReadInt();
                                int bMana = reader.ReadInt();
                                int wStam = reader.ReadInt();
                                int bStam = reader.ReadInt();

                                if (m != null && !m.Deleted)
                                {
                                    var data = new MobileResourceData
                                    {
                                        WoundHits = wHits,
                                        BuffHitsLock = bHits,
                                        WoundMana = wMana,
                                        BuffManaLock = bMana,
                                        WoundStam = wStam,
                                        BuffStamLock = bStam
                                    };
                                    m_Registry[m] = data;
                                }
                            }
                            break;
                        }
                    }
                });
        }

        private static void OnLogin(LoginEventArgs e)
        {
            if (e.Mobile != null)
            {
                Sync(e.Mobile);
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), () => { if (e.Mobile != null && !e.Mobile.Deleted) Sync(e.Mobile); });
                Timer.DelayCall(TimeSpan.FromSeconds(3.0), () => { if (e.Mobile != null && !e.Mobile.Deleted) Sync(e.Mobile); });
            }
        }

        private static void OnPlayerDeath(PlayerDeathEventArgs e)
        {
            // 사망 시 외상(검은피) 전액 치유 (부활 시 새 생명으로 시작)
            if (e.Mobile != null)
            {
                CureAllWounds(e.Mobile);
            }
        }

        public static MobileResourceData GetData(Mobile m, bool create = false)
        {
            if (m == null || m.Deleted)
                return null;

            if (m_Registry.TryGetValue(m, out MobileResourceData data))
                return data;

            if (create)
            {
                data = new MobileResourceData();
                m_Registry[m] = data;
                return data;
            }

            return null;
        }

        #region Getters
        public static int GetWoundHits(Mobile m) => GetData(m)?.WoundHits ?? 0;
        public static int GetBuffHitsLock(Mobile m) => GetData(m)?.BuffHitsLock ?? 0;

        public static int GetWoundMana(Mobile m) => GetData(m)?.WoundMana ?? 0;
        public static int GetBuffManaLock(Mobile m) => GetData(m)?.BuffManaLock ?? 0;

        public static int GetWoundStam(Mobile m) => GetData(m)?.WoundStam ?? 0;
        public static int GetBuffStamLock(Mobile m) => GetData(m)?.BuffStamLock ?? 0;

        public static int GetEffectiveHitsMax(Mobile m)
        {
            if (m == null) return 1;
            int lockTotal = GetBuffHitsLock(m) + GetWoundHits(m);
            return Math.Max(1, m.HitsMax - lockTotal);
        }

        public static int GetEffectiveManaMax(Mobile m)
        {
            if (m == null) return 0;
            int lockTotal = GetBuffManaLock(m) + GetWoundMana(m);
            return Math.Max(0, m.ManaMax - lockTotal);
        }

        public static int GetEffectiveStamMax(Mobile m)
        {
            if (m == null) return 0;
            int lockTotal = GetBuffStamLock(m) + GetWoundStam(m);
            return Math.Max(0, m.StamMax - lockTotal);
        }
        #endregion

        #region Wounds (피격 상흔)
        public static void SetWoundHits(Mobile m, int amount)
        {
            if (m == null || m.Deleted) return;
            MobileResourceData data = GetData(m, amount > 0);
            if (data != null)
            {
                int maxAllowedWound = Math.Max(0, m.HitsMax - data.BuffHitsLock - 1);
                data.WoundHits = Math.Clamp(amount, 0, maxAllowedWound);
                int effMax = GetEffectiveHitsMax(m);
                if (m.Hits > effMax) m.Hits = effMax;
                else if (m.Hits < effMax) m.Hits = m.Hits;
                Sync(m);
            }
        }

        public static void SetWoundMana(Mobile m, int amount)
        {
            if (m == null || m.Deleted) return;
            MobileResourceData data = GetData(m, amount > 0);
            if (data != null)
            {
                int maxAllowedWound = Math.Max(0, m.ManaMax - data.BuffManaLock);
                data.WoundMana = Math.Clamp(amount, 0, maxAllowedWound);
                int effMax = GetEffectiveManaMax(m);
                if (m.Mana > effMax) m.Mana = effMax;
                else if (m.Mana < effMax) m.Mana = m.Mana;
                Sync(m);
            }
        }

        public static void SetWoundStam(Mobile m, int amount)
        {
            if (m == null || m.Deleted) return;
            MobileResourceData data = GetData(m, amount > 0);
            if (data != null)
            {
                int maxAllowedWound = Math.Max(0, m.StamMax - data.BuffStamLock);
                data.WoundStam = Math.Clamp(amount, 0, maxAllowedWound);
                int effMax = GetEffectiveStamMax(m);
                if (m.Stam > effMax) m.Stam = effMax;
                else if (m.Stam < effMax) m.Stam = m.Stam;
                Sync(m);
            }
        }

        public static void AddWoundHits(Mobile m, int amount)
        {
            if (m == null || m.Deleted || !m.Alive || amount <= 0)
                return;

            MobileResourceData data = GetData(m, true);
            int maxAllowedWound = Math.Max(0, m.HitsMax - data.BuffHitsLock - 1); // 최소 1의 체력은 남김
            data.WoundHits = Math.Clamp(data.WoundHits + amount, 0, maxAllowedWound);

            int effMax = GetEffectiveHitsMax(m);
            if (m.Hits > effMax)
                m.Hits = effMax;

            Sync(m);
        }

        public static void AddWoundMana(Mobile m, int amount)
        {
            if (m == null || m.Deleted || !m.Alive || amount <= 0)
                return;

            MobileResourceData data = GetData(m, true);
            int maxAllowedWound = Math.Max(0, m.ManaMax - data.BuffManaLock);
            data.WoundMana = Math.Clamp(data.WoundMana + amount, 0, maxAllowedWound);

            int effMax = GetEffectiveManaMax(m);
            if (m.Mana > effMax)
                m.Mana = effMax;

            Sync(m);
        }

        public static void AddWoundStam(Mobile m, int amount)
        {
            if (m == null || m.Deleted || !m.Alive || amount <= 0)
                return;

            MobileResourceData data = GetData(m, true);
            int maxAllowedWound = Math.Max(0, m.StamMax - data.BuffStamLock);
            data.WoundStam = Math.Clamp(data.WoundStam + amount, 0, maxAllowedWound);

            int effMax = GetEffectiveStamMax(m);
            if (m.Stam > effMax)
                m.Stam = effMax;

            Sync(m);
        }

        public static void CheckRegenRestart(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive) return;
            if (m.Hits < GetEffectiveHitsMax(m)) m.Hits = m.Hits;
            if (m.Mana < GetEffectiveManaMax(m)) m.Mana = m.Mana;
            if (m.Stam < GetEffectiveStamMax(m)) m.Stam = m.Stam;
        }

        public static void CureWounds(Mobile m, int hitsAmount, int stamAmount, int manaAmount)
        {
            if (m == null || m.Deleted)
                return;

            MobileResourceData data = GetData(m);
            if (data == null)
                return;

            if (hitsAmount > 0) data.WoundHits = Math.Max(0, data.WoundHits - hitsAmount);
            if (stamAmount > 0) data.WoundStam = Math.Max(0, data.WoundStam - stamAmount);
            if (manaAmount > 0) data.WoundMana = Math.Max(0, data.WoundMana - manaAmount);

            if (!data.HasAny)
                m_Registry.Remove(m);

            CheckRegenRestart(m);
            Sync(m);
        }

        public static void CureAllWounds(Mobile m)
        {
            if (m == null || m.Deleted)
                return;

            MobileResourceData data = GetData(m);
            if (data == null)
                return;

            data.WoundHits = 0;
            data.WoundStam = 0;
            data.WoundMana = 0;

            if (!data.HasAny)
                m_Registry.Remove(m);

            CheckRegenRestart(m);
            Sync(m);
        }
        #endregion

        #region Buff Locks (버프 잠금)
        public static void SetBuffHitsLock(Mobile m, int amount)
        {
            if (m == null || m.Deleted) return;
            MobileResourceData data = GetData(m, amount > 0);
            if (data != null)
            {
                data.BuffHitsLock = Math.Clamp(amount, 0, Math.Max(0, m.HitsMax - 1));
                int effMax = GetEffectiveHitsMax(m);
                if (m.Hits > effMax) m.Hits = effMax;
                else if (m.Hits < effMax) m.Hits = m.Hits;
                Sync(m);
            }
        }

        public static void SetBuffManaLock(Mobile m, int amount)
        {
            if (m == null || m.Deleted) return;
            MobileResourceData data = GetData(m, amount > 0);
            if (data != null)
            {
                data.BuffManaLock = Math.Clamp(amount, 0, m.ManaMax);
                int effMax = GetEffectiveManaMax(m);
                if (m.Mana > effMax) m.Mana = effMax;
                else if (m.Mana < effMax) m.Mana = m.Mana;
                Sync(m);
            }
        }

        public static void SetBuffStamLock(Mobile m, int amount)
        {
            if (m == null || m.Deleted) return;
            MobileResourceData data = GetData(m, amount > 0);
            if (data != null)
            {
                data.BuffStamLock = Math.Clamp(amount, 0, m.StamMax);
                int effMax = GetEffectiveStamMax(m);
                if (m.Stam > effMax) m.Stam = effMax;
                else if (m.Stam < effMax) m.Stam = m.Stam;
                Sync(m);
            }
        }
        #endregion

        #region Network Sync
        public static void Sync(Mobile m)
        {
            if (m == null || m.Map == null || m.Map == Map.Internal)
                return;

            MobileResourceData data = GetData(m);
            int wHits = data?.WoundHits ?? 0;
            int bHits = data?.BuffHitsLock ?? 0;
            int wMana = data?.WoundMana ?? 0;
            int bMana = data?.BuffManaLock ?? 0;
            int wStam = data?.WoundStam ?? 0;
            int bStam = data?.BuffStamLock ?? 0;

            var pkt = new MobileResourceLockPacket(m, wHits, bHits, wMana, bMana, wStam, bStam);
            pkt.Acquire();

            try
            {
                if (m.NetState != null)
                {
                    m.NetState.Send(pkt);
                }

                IPooledEnumerable eable = m.Map.GetClientsInRange(m.Location, 18);
                foreach (NetState state in eable)
                {
                    if (state != null && state != m.NetState && state.Mobile != null)
                    {
                        state.Send(pkt);
                    }
                }
                eable.Free();
            }
            finally
            {
                pkt.Release();
            }
        }
        #endregion
    }
}
