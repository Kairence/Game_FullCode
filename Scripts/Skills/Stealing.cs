#region References
using System;
using System.Collections.Generic;

using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Targeting;
using Server.Engines.VvV;
#endregion

namespace Server.SkillHandlers
{
    public delegate void ItemStolenEventHandler(ItemStolenEventArgs e);

    public class Stealing
    {
        public static void Initialize()
        {
            SkillInfo.Table[33].Callback = OnUse;
        }

        public static event ItemStolenEventHandler ItemStolen;

        public static readonly bool ClassicMode = false;
        public static readonly bool SuspendOnMurder = false;

        public static bool IsInGuild(Mobile m)
        {
            if (m is PlayerMobile pm)
            {
                return pm.NpcGuild == NpcGuild.ThievesGuild;
            }
            return false;
        }

        public static bool IsInnocentTo(Mobile from, Mobile to)
        {
            return Notoriety.Compute(from, to) == Notoriety.Innocent;
        }

        private class StealingTarget : Target
        {
            private readonly Mobile m_Thief;

            public StealingTarget(Mobile thief) : base(1, false, TargetFlags.None)
            {
                m_Thief = thief;
                AllowNonlocal = true;
            }

            // ref bool caught 대신 Tuple을 반환하여 속도 최적화 및 ref 사용 배제
            private (Item StolenItem, bool IsCaught) TryStealItem(Item toSteal)
            {
                Item stolen = null;
                bool caught = false;

                object root = toSteal.RootParent;

                StealableArtifactsSpawner.StealableInstance si = null;
                
                if (toSteal.Parent == null || !toSteal.Movable)
                {
                    if (toSteal is AddonComponent addonComponent)
                    {
                        si = StealableArtifactsSpawner.GetStealableInstance(addonComponent.Addon);
                    }
                    else
                    {
                        si = StealableArtifactsSpawner.GetStealableInstance(toSteal);
                    }
                }

                if (!IsEmptyHanded(m_Thief))
                {
                    m_Thief.SendLocalizedMessage(1005584); // Both hands must be free to steal.
                }
                else if (root is Mobile rootMob && rootMob.Player && !IsInGuild(m_Thief))
                {
                    m_Thief.SendLocalizedMessage(1005596); // You must be in the thieves guild to steal from other players.
                }
                else if (SuspendOnMurder && root is Mobile suspendMob && suspendMob.Player && IsInGuild(m_Thief) && m_Thief.Kills > 0)
                {
                    m_Thief.SendLocalizedMessage(502706); // You are currently suspended from the thieves guild.
                }
                else if (root is BaseVendor vendor && vendor.IsInvulnerable)
                {
                    m_Thief.SendLocalizedMessage(1005598); // You can't steal from shopkeepers.
                }
                else if (root is PlayerVendor)
                {
                    m_Thief.SendLocalizedMessage(502709); // You can't steal from vendors.
                }
                else if (!m_Thief.CanSee(toSteal))
                {
                    m_Thief.SendLocalizedMessage(500237); // Target can not be seen.
                }
                else if (m_Thief.Backpack == null || !m_Thief.Backpack.CheckHold(m_Thief, toSteal, false, true))
                {
                    m_Thief.SendLocalizedMessage(1048147); // Your backpack can't hold anything else.
                }
                #region Sigils
                else if (toSteal is Sigil sig)
                {
                    PlayerState pl = PlayerState.Find(m_Thief);
                    Faction faction = (pl == null ? null : pl.Faction);

                    if (!m_Thief.InRange(toSteal.GetWorldLocation(), 1))
                    {
                        m_Thief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
                    }
                    else if (root != null) // not on the ground
                    {
                        m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                    }
                    else if (faction != null)
                    {
                        if (!m_Thief.CanBeginAction(typeof(IncognitoSpell)))
                        {
                            m_Thief.SendLocalizedMessage(1010581); // You cannot steal the sigil when you are incognito
                        }
                        else if (DisguiseTimers.IsDisguised(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage(1010583); // You cannot steal the sigil while disguised
                        }
                        else if (!m_Thief.CanBeginAction(typeof(PolymorphSpell)))
                        {
                            m_Thief.SendLocalizedMessage(1010582); // You cannot steal the sigil while polymorphed
                        }
                        else if (TransformationSpellHelper.UnderTransformation(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage(1061622); // You cannot steal the sigil while in that form.
                        }
                        else if (AnimalForm.UnderTransformation(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage(1063222); // You cannot steal the sigil while mimicking an animal.
                        }
                        else if (pl.IsLeaving)
                        {
                            m_Thief.SendLocalizedMessage(1005589); // You are currently quitting a faction and cannot steal the town sigil
                        }
                        else if (sig.IsBeingCorrupted && sig.LastMonolith != null && sig.LastMonolith.Faction == faction)
                        {
                            m_Thief.SendLocalizedMessage(1005590); // You cannot steal your own sigil
                        }
                        else if (sig.IsPurifying)
                        {
                            m_Thief.SendLocalizedMessage(1005592); // You cannot steal this sigil until it has been purified
                        }
                        else if (m_Thief.CheckTargetSkill(SkillName.Stealing, toSteal, 80.0, 80.0))
                        {
                            if (Sigil.ExistsOn(m_Thief))
                            {
                                m_Thief.SendLocalizedMessage(1010258); // The sigil has gone back to its home location because you already have a sigil.
                            }
                            else if (m_Thief.Backpack == null || !m_Thief.Backpack.CheckHold(m_Thief, sig, false, true))
                            {
                                m_Thief.SendLocalizedMessage(1010259); // The sigil has gone home because your backpack is full
                            }
                            else
                            {
                                if (sig.IsBeingCorrupted)
                                {
                                    sig.GraceStart = DateTime.UtcNow; // begin grace period
                                }

                                m_Thief.SendLocalizedMessage(1010586); // YOU STOLE THE SIGIL!!! (woah, calm down now)

                                if (sig.LastMonolith != null && sig.LastMonolith.Sigil != null)
                                {
                                    sig.LastMonolith.Sigil = null;
                                    sig.LastStolen = DateTime.UtcNow;
                                }

                                return (sig, caught);
                            }
                        }
                        else
                        {
                            m_Thief.SendLocalizedMessage(1005594); // You do not have enough skill to steal the sigil
                        }
                    }
                    else
                    {
                        m_Thief.SendLocalizedMessage(1005588); // You must join a faction to do that
                    }
                }
                #endregion
                #region VvV Sigils
                else if (toSteal is VvVSigil vvvsig && ViceVsVirtueSystem.Instance != null)
                {
                    VvVPlayerEntry entry = ViceVsVirtueSystem.Instance.GetPlayerEntry<VvVPlayerEntry>(m_Thief);

                    if (!m_Thief.InRange(toSteal.GetWorldLocation(), 1))
                    {
                        m_Thief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
                    }
                    else if (root != null) // not on the ground
                    {
                        m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                    }
                    else if (entry != null)
                    {
                        if (!m_Thief.CanBeginAction(typeof(IncognitoSpell)))
                        {
                            m_Thief.SendLocalizedMessage(1010581); // You cannot steal the sigil when you are incognito
                        }
                        else if (DisguiseTimers.IsDisguised(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage(1010583); // You cannot steal the sigil while disguised
                        }
                        else if (!m_Thief.CanBeginAction(typeof(PolymorphSpell)))
                        {
                            m_Thief.SendLocalizedMessage(1010582); // You cannot steal the sigil while polymorphed
                        }
                        else if (TransformationSpellHelper.UnderTransformation(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage(1061622); // You cannot steal the sigil while in that form.
                        }
                        else if (AnimalForm.UnderTransformation(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage(1063222); // You cannot steal the sigil while mimicking an animal.
                        }
                        else if (m_Thief.CheckTargetSkill(SkillName.Stealing, toSteal, 100.0, 120.0))
                        {
                            if (m_Thief.Backpack == null || !m_Thief.Backpack.CheckHold(m_Thief, vvvsig, false, true))
                            {
                                m_Thief.SendLocalizedMessage(1010259); // The sigil has gone home because your backpack is full
                            }
                            else
                            {
                                m_Thief.SendLocalizedMessage(1010586); // YOU STOLE THE SIGIL!!! (woah, calm down now)
                                vvvsig.OnStolen(entry);
                                return (vvvsig, caught);
                            }
                        }
                        else
                        {
                            m_Thief.SendLocalizedMessage(1005594); // You do not have enough skill to steal the sigil
                        }
                    }
                    else
                    {
                        m_Thief.SendLocalizedMessage(1155415); // Only participants in Vice vs Virtue may use this item.
                    }
                }
                #endregion
                else if (si == null && (toSteal.Parent == null || !toSteal.Movable) && !ItemFlags.GetStealable(toSteal))
                {
                    m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                }
                else if ((toSteal.LootType == LootType.Newbied || toSteal.CheckBlessed(root)) && !ItemFlags.GetStealable(toSteal))
                {
                    m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                }
                else if (Core.AOS && si == null && toSteal is Container && !ItemFlags.GetStealable(toSteal))
                {
                    m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                }
                else if (!m_Thief.InRange(toSteal.GetWorldLocation(), 1))
                {
                    m_Thief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
                }
                else if (si != null && m_Thief.Skills[SkillName.Stealing].Value < 100.0)
                {
                    m_Thief.SendLocalizedMessage(1060025, "", 0x66D); // You're not skilled enough to attempt the theft of this item.
                }
                else if (toSteal.Parent is Mobile)
                {
                    m_Thief.SendLocalizedMessage(1005585); // You cannot steal items which are equiped.
                }
                else if (root == m_Thief)
                {
                    m_Thief.SendLocalizedMessage(502704); // You catch yourself red-handed.
                }
                else if (root is Mobile staffMob && staffMob.IsStaff())
                {
                    m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                }
                else if (root is Mobile targetMob && !m_Thief.CanBeHarmful(targetMob))
                {
                    // CanBeHarmful will handle the messaging internally if false
                }
                else if (root is Corpse)
                {
                    m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                }
                else
                {
                    double w = toSteal.Weight + toSteal.TotalWeight;

                    if (w > 10)
                    {
                        m_Thief.SendMessage("That is too heavy to steal.");
                    }
                    else
                    {
                        if (toSteal.Stackable && toSteal.Amount > 1)
                        {
                            int maxAmount = (int)((m_Thief.Skills[SkillName.Stealing].Value / 10.0) / toSteal.Weight);

                            if (maxAmount < 1)
                            {
                                maxAmount = 1;
                            }
                            else if (maxAmount > toSteal.Amount)
                            {
                                maxAmount = toSteal.Amount;
                            }

                            int amount = Utility.RandomMinMax(1, maxAmount);

                            if (amount >= toSteal.Amount)
                            {
                                int pileWeight = (int)Math.Ceiling(toSteal.Weight * toSteal.Amount);
                                pileWeight *= 10;

                                if (m_Thief.CheckTargetSkill(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
                                {
                                    stolen = toSteal;
                                }
                            }
                            else
                            {
                                int pileWeight = (int)Math.Ceiling(toSteal.Weight * amount);
                                pileWeight *= 10;

                                if (m_Thief.CheckTargetSkill(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
                                {
                                    stolen = Mobile.LiftItemDupe(toSteal, toSteal.Amount - amount);

                                    if (stolen == null)
                                    {
                                        stolen = toSteal;
                                    }
                                }
                            }
                        }
                        else
                        {
                            int iw = (int)Math.Ceiling(w);
                            iw *= 10;

                            if (m_Thief.CheckTargetSkill(SkillName.Stealing, toSteal, iw - 22.5, iw + 27.5))
                            {
                                stolen = toSteal;
                            }
                        }

                        // Non-movable stealable (not in fillable container) items cannot result in the stealer getting caught
                        if (stolen != null && (root is FillableContainer || stolen.Movable))
                        {
                            double skillValue = m_Thief.Skills[SkillName.Stealing].Value;

                            if (root is FillableContainer)
                            {
                                caught = (Utility.Random((int)(skillValue / 2.5)) == 0); // 1 of 48 chance at 120
                            }
                            else
                            {
                                caught = (skillValue < Utility.Random(150));
                            }
                        }
                        else
                        {
                            caught = false;
                        }

                        if (stolen != null)
                        {
                            m_Thief.SendLocalizedMessage(502724); // You succesfully steal the item.

                            ItemFlags.SetTaken(stolen, true);
                            ItemFlags.SetStealable(stolen, false);
                            stolen.Movable = true;

                            InvokeItemStolen(new ItemStolenEventArgs(stolen, m_Thief));

                            if (si != null)
                            {
                                toSteal.Movable = true;
                                si.Item = null;
                            }
                        }
                        else
                        {
                            m_Thief.SendLocalizedMessage(502723); // You fail to steal the item.
                        }
                    }
                }

                return (stolen, caught);
            }

            protected override void OnTarget(Mobile from, object target)
            {
                from.RevealingAction();

                Item stolen = null;
                object root = null;
                bool caught = false;

                if (target is Item targetItem)
                {
                    root = targetItem.RootParent;
                    var result = TryStealItem(targetItem);
                    stolen = result.StolenItem;
                    caught = result.IsCaught;
                }
                else if (target is Mobile targetMob)
                {
                    Container pack = targetMob.Backpack;

                    if (pack != null && pack.Items.Count > 0)
                    {
                        int randomIndex = Utility.Random(pack.Items.Count);

                        root = targetMob;
                        var result = TryStealItem(pack.Items[randomIndex]);
                        stolen = result.StolenItem;
                        caught = result.IsCaught;
                    }

                    #region Monster Stealables
                    if (targetMob is BaseCreature creature && from is PlayerMobile pm)
                    {
                        Server.Engines.CreatureStealing.StealingHandler.HandleSteal(creature, pm);
                    }
                    #endregion
                }
                else
                {
                    m_Thief.SendLocalizedMessage(502710); // You can't steal that!
                }

                if (stolen != null)
                {
                    if (stolen is AddonComponent component)
                    {
                        if (component.Addon is BaseAddon addon)
                        {
                            from.AddToBackpack(addon.Deed);
                            addon.Delete();
                        }
                    }
                    else
                    {
                        from.AddToBackpack(stolen);
                    }

                    if (!(stolen is Container || stolen.Stackable))
                    {
                        // do not return stolen containers or stackable items
                        StolenItem.Add(stolen, m_Thief, root as Mobile);
                    }
                }

                if (caught)
                {
                    if (root == null)
                    {
                        m_Thief.CriminalAction(false);
                    }
                    else if (root is Corpse corpseRoot && corpseRoot.IsCriminalAction(m_Thief))
                    {
                        m_Thief.CriminalAction(false);
                    }
                    else if (root is Mobile mobRoot)
                    {
                        if (!IsInGuild(mobRoot) && IsInnocentTo(m_Thief, mobRoot))
                        {
                            m_Thief.CriminalAction(false);
                        }

                        string message = String.Format("You notice {0} trying to steal from {1}.", m_Thief.Name, mobRoot.Name);

                        foreach (NetState ns in m_Thief.GetClientsInRange(8))
                        {
                            if (ns.Mobile != m_Thief)
                            {
                                ns.Mobile.SendMessage(message);
                            }
                        }
                    }
                }
                else if (root is Corpse corpseRoot && corpseRoot.IsCriminalAction(m_Thief))
                {
                    m_Thief.CriminalAction(false);
                }

                if (root is Mobile victimMob && victimMob.Player && m_Thief is PlayerMobile thiefPlayer && 
                    IsInnocentTo(m_Thief, victimMob) && !IsInGuild(victimMob))
                {
                    thiefPlayer.PermaFlags.Add(victimMob);
                    thiefPlayer.Delta(MobileDelta.Noto);
                }
            }
        }

        public static bool IsEmptyHanded(Mobile from)
        {
            if (from.FindItemOnLayer(Layer.OneHanded) != null)
            {
                return false;
            }

            if (from.FindItemOnLayer(Layer.TwoHanded) != null)
            {
                return false;
            }

            return true;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if (!IsEmptyHanded(m))
            {
                m.SendLocalizedMessage(1005584); // Both hands must be free to steal.
            }
            else
            {
                m.Target = new StealingTarget(m);
                m.RevealingAction();

                m.SendLocalizedMessage(502698); // Which item do you want to steal?
            }

            return TimeSpan.FromSeconds(10.0);
        }

        public static void InvokeItemStolen(ItemStolenEventArgs e)
        {
            ItemStolen?.Invoke(e);
        }
    }

    public class StolenItem
    {
        public static readonly TimeSpan StealTime = TimeSpan.FromMinutes(2.0);

        private readonly Item m_Stolen;
        private readonly Mobile m_Thief;
        private readonly Mobile m_Victim;
        private DateTime m_Expires;

        public Item Stolen { get { return m_Stolen; } }
        public Mobile Thief { get { return m_Thief; } }
        public Mobile Victim { get { return m_Victim; } }
        public DateTime Expires { get { return m_Expires; } }

        public bool IsExpired { get { return (DateTime.UtcNow >= m_Expires); } }

        public StolenItem(Item stolen, Mobile thief, Mobile victim)
        {
            m_Stolen = stolen;
            m_Thief = thief;
            m_Victim = victim;

            m_Expires = DateTime.UtcNow + StealTime;
        }

        // 제네릭 Queue를 사용하여 캐스팅 성능 최적화
        private static readonly Queue<StolenItem> m_Queue = new Queue<StolenItem>();

        public static void Add(Item item, Mobile thief, Mobile victim)
        {
            Clean();
            m_Queue.Enqueue(new StolenItem(item, thief, victim));
        }

        public static bool IsStolen(Item item)
        {
            var result = CheckStolen(item);
            return result.IsStolen;
        }

        // ref out을 배제하기 위해 Tuple 반환 방식으로 수정
        public static (bool IsStolen, Mobile Victim) CheckStolen(Item item)
        {
            Clean();

            foreach (StolenItem si in m_Queue)
            {
                if (si.m_Stolen == item && !si.IsExpired)
                {
                    return (true, si.m_Victim);
                }
            }

            return (false, null);
        }

        public static void ReturnOnDeath(Mobile killed, Container corpse)
        {
            Clean();

            foreach (StolenItem si in m_Queue)
            {
                if (si.m_Stolen.RootParent == corpse && si.m_Victim != null && !si.IsExpired)
                {
                    if (si.m_Victim.AddToBackpack(si.m_Stolen))
                    {
                        si.m_Victim.SendLocalizedMessage(1010464); // the item that was stolen is returned to you.
                    }
                    else
                    {
                        si.m_Victim.SendLocalizedMessage(1010463); // the item that was stolen from you falls to the ground.
                    }

                    si.m_Expires = DateTime.UtcNow; // such a hack
                }
            }
        }

        public static void Clean()
        {
            // Peek 후 꺼내는 대신 조건에 맞으면 즉시 Dequeue 되도록 구조 최적화
            while (m_Queue.Count > 0 && m_Queue.Peek().IsExpired)
            {
                m_Queue.Dequeue();
            }
        }
    }

    public class ItemStolenEventArgs : EventArgs
    {
        public Item Item { get; set; }
        public Mobile Mobile { get; set; }

        public ItemStolenEventArgs(Item item, Mobile thief)
        {
            Mobile = thief;
            Item = item;
        }
    }
}