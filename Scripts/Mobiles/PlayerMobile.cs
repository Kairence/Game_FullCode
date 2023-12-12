#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.BulkOrders;
using Server.Engines.CannedEvil;
using Server.Engines.CityLoyalty;
using Server.Engines.Craft;
using Server.Engines.Help;
using Server.Engines.PartySystem;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Engines.Shadowguard;
using Server.Engines.VoidPool;
using Server.Engines.VvV;
using Server.Engines.XmlSpawner2;
using Server.Ethics;
using Server.Factions;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Movement;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Services.Virtues;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.First;
using Server.Spells.Fifth;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.SkillMasteries;
using Server.Spells.Spellweaving;
using Server.Engines.SphynxFortune;
using Server.Engines.VendorSearching;
using Server.Targeting;

using RankDefinition = Server.Guilds.RankDefinition;
using Server.Engines.Fellowship;
#endregion

namespace Server.Mobiles
{
	#region Enums
	[Flags]
	public enum PlayerFlag
	{
		None = 0x00000000,
		Glassblowing = 0x00000001,
		Masonry = 0x00000002,
		SandMining = 0x00000004,
		StoneMining = 0x00000008,
		ToggleMiningStone = 0x00000010,
		KarmaLocked = 0x00000020,
		AutoRenewInsurance = 0x00000040,
		UseOwnFilter = 0x00000080,
		PublicMyRunUO = 0x00000100,
		PagingSquelched = 0x00000200,
		Young = 0x00000400,
		AcceptGuildInvites = 0x00000800,
		DisplayChampionTitle = 0x00001000,
		HasStatReward = 0x00002000,
		Bedlam = 0x00010000,
		LibraryFriend = 0x00020000,
		Spellweaving = 0x00040000,
		GemMining = 0x00080000,
		ToggleMiningGem = 0x00100000,
		BasketWeaving = 0x00200000,
		AbyssEntry = 0x00400000,
		ToggleClippings = 0x00800000,
        ToggleCutClippings = 0x01000000,
		ToggleCutReeds = 0x02000000,
		MechanicalLife = 0x04000000,
        Unused = 0x08000000,
        ToggleCutTopiaries = 0x10000000,
        HasValiantStatReward = 0x20000000,
        RefuseTrades = 0x40000000,
    }

    [Flags]
    public enum ExtendedPlayerFlag
    {
        Unused                      = 0x00000001,
        ToggleStoneOnly             = 0x00000002,
        CanBuyCarpets               = 0x00000004,
        VoidPool                    = 0x00000008,
        DisabledPvpWarning          = 0x00000010,
    }

	public enum NpcGuild
	{
		None,
		MagesGuild,
		WarriorsGuild,
		ThievesGuild,
		RangersGuild,
		HealersGuild,
		MinersGuild,
		MerchantsGuild,
		TinkersGuild,
		TailorsGuild,
		FishermensGuild,
		BardsGuild,
		BlacksmithsGuild
	}

	public enum SolenFriendship
	{
		None,
		Red,
		Black
	}
	#endregion

	public partial class PlayerMobile : Mobile, IHonorTarget
	{
		public static List<PlayerMobile> Instances { get; private set; }

		static PlayerMobile()
		{
			Instances = new List<PlayerMobile>(0x1000);
		}

		#region Mount Blocking
		public void SetMountBlock(BlockMountType type, TimeSpan duration, bool dismount)
		{
			if (dismount)
			{
                BaseMount.Dismount(this, this, type, duration, false);
			}
			else
			{
                BaseMount.SetMountPrevention(this, type, duration);
			}
		}
		#endregion

		#region Stygian Abyss
        public override void ToggleFlying()
        {
            if (Race != Race.Gargoyle)
                return;

            if (Frozen)
            {
                SendLocalizedMessage(1060170); // You cannot use this ability while frozen.
                return;
            }

            if (!Flying)
            {
                if (BeginAction(typeof(FlySpell)))
                {
                    if (this.Spell is Spell)
                        ((Spell)this.Spell).Disturb(DisturbType.Unspecified, false, false);

                    Spell spell = new FlySpell(this);
                    spell.Cast();

                    Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
                }
                else
                {
                    LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
                }
            }
            else if (IsValidLandLocation(Location, Map))
            {
                if (BeginAction(typeof(FlySpell)))
                {
                    if (this.Spell is Spell)
                        ((Spell)this.Spell).Disturb(DisturbType.Unspecified, false, false);

                    Animate(AnimationType.Land, 0);
                    Flying = false;
                    BuffInfo.RemoveBuff(this, BuffIcon.Fly);

                    Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
                }
                else
                {
                    LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
                }
            }
            else
                LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113081); // You may not land here.
        }

        public static bool IsValidLandLocation(Point3D p, Map map)
        {
            return map.CanFit(p.X, p.Y, p.Z, 16, false, false);
        }
        #endregion

        private class CountAndTimeStamp
		{
			private int m_Count;
			private DateTime m_Stamp;

			public DateTime TimeStamp { get { return m_Stamp; } }

			public int Count
			{
				get { return m_Count; }
				set
				{
					m_Count = value;
					m_Stamp = DateTime.UtcNow;
				}
			}
		}

		private DesignContext m_DesignContext;

		private NpcGuild m_NpcGuild;
		private DateTime m_NpcGuildJoinTime;
		private TimeSpan m_NpcGuildGameTime;
		private PlayerFlag m_Flags;
        private ExtendedPlayerFlag m_ExtendedFlags;
		private int m_Profession;

		private int m_NonAutoreinsuredItems;
		// number of items that could not be automaitically reinsured because gold in bank was not enough

		/*
		* a value of zero means, that the mobile is not executing the spell. Otherwise,
		* the value should match the BaseMana required
		*/
		private int m_ExecutesLightningStrike; // move to Server.Mobiles??

		private DateTime m_LastOnline;
		private RankDefinition m_GuildRank;
        private bool m_NextEnhanceSuccess;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool NextEnhanceSuccess { get { return m_NextEnhanceSuccess; } set { m_NextEnhanceSuccess = value; } }

        private int m_GuildMessageHue, m_AllianceMessageHue;

		private List<Mobile> m_AutoStabled;
		private List<Mobile> m_AllFollowers;
		private List<Mobile> m_RecentlyReported;

        public bool UseSummoningRite { get; set; }

        #region Points System
        private PointsSystemProps _PointsSystemProps;
        private SkillsSyetemProps _SkillsSyetemProps;
        private PointGoldSyetemProps _PointGoldSyetemProps;
        private PointSilverSyetemProps _PointSilverSyetemProps;
        private PointEquipSyetemProps _PointEquipSyetemProps;
        private PointArtifactSyetemProps _PointArtifactSyetemProps;
        private PointMonsterSyetemProps _PointMonsterSyetemProps;
        private PlayerMagicTimerSystemProps _PlayerMagicTimerSystemProps;
        private PlayerUtilTimerSystemProps _PlayerUtilTimerSystemProps;
        private BODProps _BODProps;
        private AccountGoldProps _AccountGold;

       [CommandProperty(AccessLevel.GameMaster)]
        public PlayerMagicTimerSystemProps MagicTimersystems
        {
            get
            {
                if (_PlayerMagicTimerSystemProps == null)
                    _PlayerMagicTimerSystemProps = new PlayerMagicTimerSystemProps(this);

                return _PlayerMagicTimerSystemProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlayerUtilTimerSystemProps UtilTimerSystems
        {
            get
            {
                if (_PlayerUtilTimerSystemProps == null)
                    _PlayerUtilTimerSystemProps = new PlayerUtilTimerSystemProps(this);

                return _PlayerUtilTimerSystemProps;
            }
            set
            {
            }
        }
		//포인트 시스템
	    [CommandProperty(AccessLevel.GameMaster)]
        public PointGoldSyetemProps PointGoldSystems
        {
            get
            {
                if (_PointGoldSyetemProps == null)
                    _PointGoldSyetemProps = new PointGoldSyetemProps(this);

                return _PointGoldSyetemProps;
            }
            set
            {
            }
        }	
	    [CommandProperty(AccessLevel.GameMaster)]
        public PointSilverSyetemProps PointSilverSystems
        {
            get
            {
                if (_PointSilverSyetemProps == null)
                    _PointSilverSyetemProps = new PointSilverSyetemProps(this);

                return _PointSilverSyetemProps;
            }
            set
            {
            }
        }
	    [CommandProperty(AccessLevel.GameMaster)]
        public PointEquipSyetemProps PointEquipSystems
        {
            get
            {
                if (_PointEquipSyetemProps == null)
                    _PointEquipSyetemProps = new PointEquipSyetemProps(this);

                return _PointEquipSyetemProps;
            }
            set
            {
            }
        }
		[CommandProperty(AccessLevel.GameMaster)]
        public PointArtifactSyetemProps PointArtifactSystems
        {
            get
            {
                if (_PointArtifactSyetemProps == null)
                    _PointArtifactSyetemProps = new PointArtifactSyetemProps(this);

                return _PointArtifactSyetemProps;
            }
            set
            {
            }
        }
		[CommandProperty(AccessLevel.GameMaster)]
        public PointMonsterSyetemProps PointMonsterSystems
        {
            get
            {
                if (_PointMonsterSyetemProps == null)
                    _PointMonsterSyetemProps = new PointMonsterSyetemProps(this);

                return _PointMonsterSyetemProps;
            }
            set
            {
            }
        }
	    [CommandProperty(AccessLevel.GameMaster)]
        public SkillsSyetemProps SkillSystems
        {
            get
            {
                if (_SkillsSyetemProps == null)
                    _SkillsSyetemProps = new SkillsSyetemProps(this);

                return _SkillsSyetemProps;
            }
            set
            {
            }
        }	
        [CommandProperty(AccessLevel.GameMaster)]
        public PointsSystemProps PointSystems
        {
            get
            {
                if (_PointsSystemProps == null)
                    _PointsSystemProps = new PointsSystemProps(this);

                return _PointsSystemProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODProps BODData
        {
            get
            {
                if (_BODProps == null)
                {
                    _BODProps = new BODProps(this);
                }

                return _BODProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AccountGoldProps AccountGold
        {
            get
            {
                if (_AccountGold == null)
                {
                    _AccountGold = new AccountGoldProps(this);
                }

                return _AccountGold;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountSovereigns
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.Sovereigns;
                }

                return 0;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetSovereigns(value);
                }
            }
        }

        public bool DepositSovereigns(int amount)
        {
            var acct = Account as Account;

            if (acct != null)
            {
                return acct.DepositSovereigns(amount);
            }

            return false;
        }

        public bool WithdrawSovereigns(int amount)
        {
            var acct = Account as Account;

            if (acct != null)
            {
                return acct.WithdrawSovereigns(amount);
            }

            return false;
        }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountCharacterSlotsBonus
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.CharacterSlotsBonus;
                }

                return 0;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetCharacterSlotsBonus(value);
                }
            }
        }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountHouseSlotsBonus
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.HouseSlotsBonus;
                }

                return 0;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetHouseSlotsBonus(value);
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountLoginBonus
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.LoginBonus;
                }

                return 0;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetLoginBonus(value);
                }
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountPoint
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.Point[0];
                }

                return 0;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetPoint(value);
                }
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AccountDayTime
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.Daychecktime;
                }

                return DateTime.Now;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetDaychecktime();
                }
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AccountWeekTime
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.Weekchecktime;
                }

                return DateTime.Now;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetWeekchecktime();
                }
            }
        }

        #endregion

		private bool m_EvalCast;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool EvalCast { get { return m_EvalCast; } set { m_EvalCast = value; }}
		
        #region Getters & Setters
        public List<Mobile> RecentlyReported { get { return m_RecentlyReported; } set { m_RecentlyReported = value; } }

		public List<Mobile> AutoStabled { get { return m_AutoStabled; } }

		public bool NinjaWepCooldown { get; set; }

		public List<Mobile> AllFollowers
		{
			get
			{
				if (m_AllFollowers == null)
				{
					m_AllFollowers = new List<Mobile>();
				}

				return m_AllFollowers;
			}
		}

		[CommandProperty(AccessLevel.GameMaster, true)]
		public RankDefinition GuildRank
		{
			get
			{
				if (AccessLevel >= AccessLevel.GameMaster)
				{
					return RankDefinition.Leader;
				}
				else
				{
					return m_GuildRank;
				}
			}
			set { m_GuildRank = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int GuildMessageHue { get { return m_GuildMessageHue; } set { m_GuildMessageHue = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AllianceMessageHue { get { return m_AllianceMessageHue; } set { m_AllianceMessageHue = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Profession { get { return m_Profession; } set { m_Profession = value; } }

		public int StepsTaken { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public NpcGuild NpcGuild { get { return m_NpcGuild; } set { m_NpcGuild = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NpcGuildJoinTime { get { return m_NpcGuildJoinTime; } set { m_NpcGuildJoinTime = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextBODTurnInTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastOnline { get { return m_LastOnline; } set { m_LastOnline = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public long LastMoved { get { return LastMoveTime; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NpcGuildGameTime { get { return m_NpcGuildGameTime; } set { m_NpcGuildGameTime = value; } }

		public int ExecutesLightningStrike { get { return m_ExecutesLightningStrike; } set { m_ExecutesLightningStrike = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToothAche { get { return BaseSweet.GetToothAche(this); } set { BaseSweet.SetToothAche(this, value, true); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool MechanicalLife { get { return GetFlag(PlayerFlag.MechanicalLife); } set { SetFlag(PlayerFlag.MechanicalLife, value); } }
		#endregion

		#region PlayerFlags
		public PlayerFlag Flags { get { return m_Flags; } set { m_Flags = value; } }
        public ExtendedPlayerFlag ExtendedFlags { get { return m_ExtendedFlags; } set { m_ExtendedFlags = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PagingSquelched { get { return GetFlag(PlayerFlag.PagingSquelched); } set { SetFlag(PlayerFlag.PagingSquelched, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Glassblowing { get { return GetFlag(PlayerFlag.Glassblowing); } set { SetFlag(PlayerFlag.Glassblowing, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Masonry { get { return GetFlag(PlayerFlag.Masonry); } set { SetFlag(PlayerFlag.Masonry, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SandMining { get { return GetFlag(PlayerFlag.SandMining); } set { SetFlag(PlayerFlag.SandMining, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StoneMining { get { return GetFlag(PlayerFlag.StoneMining); } set { SetFlag(PlayerFlag.StoneMining, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool GemMining { get { return GetFlag(PlayerFlag.GemMining); } set { SetFlag(PlayerFlag.GemMining, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool BasketWeaving { get { return GetFlag(PlayerFlag.BasketWeaving); } set { SetFlag(PlayerFlag.BasketWeaving, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleMiningStone { get { return GetFlag(PlayerFlag.ToggleMiningStone); } set { SetFlag(PlayerFlag.ToggleMiningStone, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AbyssEntry { get { return GetFlag(PlayerFlag.AbyssEntry); } set { SetFlag(PlayerFlag.AbyssEntry, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleMiningGem { get { return GetFlag(PlayerFlag.ToggleMiningGem); } set { SetFlag(PlayerFlag.ToggleMiningGem, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool KarmaLocked { get { return GetFlag(PlayerFlag.KarmaLocked); } set { SetFlag(PlayerFlag.KarmaLocked, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AutoRenewInsurance { get { return GetFlag(PlayerFlag.AutoRenewInsurance); } set { SetFlag(PlayerFlag.AutoRenewInsurance, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UseOwnFilter { get { return GetFlag(PlayerFlag.UseOwnFilter); } set { SetFlag(PlayerFlag.UseOwnFilter, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AcceptGuildInvites { get { return GetFlag(PlayerFlag.AcceptGuildInvites); } set { SetFlag(PlayerFlag.AcceptGuildInvites, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasStatReward { get { return GetFlag(PlayerFlag.HasStatReward); } set { SetFlag(PlayerFlag.HasStatReward, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasValiantStatReward { get { return GetFlag(PlayerFlag.HasValiantStatReward); } set { SetFlag(PlayerFlag.HasValiantStatReward, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RefuseTrades
        {
            get { return GetFlag(PlayerFlag.RefuseTrades); }
            set { SetFlag(PlayerFlag.RefuseTrades, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisabledPvpWarning
        {
            get { return GetFlag(ExtendedPlayerFlag.DisabledPvpWarning); }
            set { SetFlag(ExtendedPlayerFlag.DisabledPvpWarning, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CanBuyCarpets
        {
            get { return GetFlag(ExtendedPlayerFlag.CanBuyCarpets); }
            set { SetFlag(ExtendedPlayerFlag.CanBuyCarpets, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool VoidPool
        {
            get { return GetFlag(ExtendedPlayerFlag.VoidPool); }
            set { SetFlag(ExtendedPlayerFlag.VoidPool, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleStoneOnly
        {
            get { return GetFlag(ExtendedPlayerFlag.ToggleStoneOnly); }
            set { SetFlag(ExtendedPlayerFlag.ToggleStoneOnly, value); }
        }

        #region Plant system
        [CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleClippings { get { return GetFlag(PlayerFlag.ToggleClippings); } set { SetFlag(PlayerFlag.ToggleClippings, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleCutReeds { get { return GetFlag(PlayerFlag.ToggleCutReeds); } set { SetFlag(PlayerFlag.ToggleCutReeds, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleCutClippings { get { return GetFlag(PlayerFlag.ToggleCutClippings); } set { SetFlag(PlayerFlag.ToggleCutClippings, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutTopiaries { get { return GetFlag(PlayerFlag.ToggleCutTopiaries); } set { SetFlag(PlayerFlag.ToggleCutTopiaries, value); } }

        private DateTime m_SSNextSeed;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SSNextSeed { get { return m_SSNextSeed; } set { m_SSNextSeed = value; } }

		private DateTime m_SSSeedExpire;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SSSeedExpire { get { return m_SSSeedExpire; } set { m_SSSeedExpire = value; } }

		private Point3D m_SSSeedLocation;

		public Point3D SSSeedLocation { get { return m_SSSeedLocation; } set { m_SSSeedLocation = value; } }

		private Map m_SSSeedMap;

		public Map SSSeedMap { get { return m_SSSeedMap; } set { m_SSSeedMap = value; } }
        #endregion

        #endregion

        #region Auto Arrow Recovery
        private readonly Dictionary<Type, int> m_RecoverableAmmo = new Dictionary<Type, int>();

		public Dictionary<Type, int> RecoverableAmmo { get { return m_RecoverableAmmo; } }

		public void RecoverAmmo()
		{
			if (Core.SE && Alive)
			{
				foreach (var kvp in m_RecoverableAmmo)
				{
					if (kvp.Value > 0)
					{
						Item ammo = null;

						try
						{
							ammo = Activator.CreateInstance(kvp.Key) as Item;
						}
						catch
						{ }

						if (ammo != null)
						{
							string name = ammo.Name;
							ammo.Amount = kvp.Value;

							if (name == null)
							{
								if (ammo is Arrow)
								{
									name = "arrow";
								}
								else if (ammo is Bolt)
								{
									name = "bolt";
								}
							}

							if (name != null && ammo.Amount > 1)
							{
								name = String.Format("{0}s", name);
							}

							if (name == null)
							{
								name = String.Format("#{0}", ammo.LabelNumber);
							}

							PlaceInBackpack(ammo);
							SendLocalizedMessage(1073504, String.Format("{0}\t{1}", ammo.Amount, name)); // You recover ~1_NUM~ ~2_AMMO~.
						}
					}
				}

				m_RecoverableAmmo.Clear();
			}
		}
		#endregion

        #region Reward Stable Slots
        [CommandProperty(AccessLevel.GameMaster)]
        public int RewardStableSlots { get; set; }
        #endregion

        private DateTime m_AnkhNextUse;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime AnkhNextUse { get { return m_AnkhNextUse; } set { m_AnkhNextUse = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextGemOfSalvationUse { get; set; }

        #region Mondain's Legacy
        [CommandProperty(AccessLevel.GameMaster)]
		public bool Bedlam { get { return GetFlag(PlayerFlag.Bedlam); } set { SetFlag(PlayerFlag.Bedlam, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool LibraryFriend { get { return GetFlag(PlayerFlag.LibraryFriend); } set { SetFlag(PlayerFlag.LibraryFriend, value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Spellweaving { get { return GetFlag(PlayerFlag.Spellweaving); } set { SetFlag(PlayerFlag.Spellweaving, value); } }
		#endregion

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan DisguiseTimeLeft { get { return DisguiseTimers.TimeRemaining(this); } }

		private DateTime m_PeacedUntil;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime PeacedUntil { get { return m_PeacedUntil; } set { m_PeacedUntil = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public override string TitleName
		{
			get
			{
                string name;

                if (Fame >= 10000)
                    name = String.Format("{0} {1}", Female ? "Lady" : "Lord", RawName);
                else
                    name = RawName;

                return name;
			}
		}

		#region Scroll of Alacrity
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime AcceleratedStart { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName AcceleratedSkill { get; set; }
		#endregion

		public static Direction GetDirection4(Point3D from, Point3D to)
		{
			int dx = from.X - to.X;
			int dy = from.Y - to.Y;

			int rx = dx - dy;
			int ry = dx + dy;

			Direction ret;

			if (rx >= 0 && ry >= 0)
			{
				ret = Direction.West;
			}
			else if (rx >= 0 && ry < 0)
			{
				ret = Direction.South;
			}
			else if (rx < 0 && ry < 0)
			{
				ret = Direction.East;
			}
			else
			{
				ret = Direction.North;
			}

			return ret;
		}

		public override bool OnDroppedItemToWorld(Item item, Point3D location)
		{
			if (!base.OnDroppedItemToWorld(item, location))
			{
				return false;
			}

			if (Core.AOS)
			{
				IPooledEnumerable mobiles = Map.GetMobilesInRange(location, 0);

				foreach (Mobile m in mobiles)
				{
					if (m.Z >= location.Z && m.Z < location.Z + 16)
					{
						mobiles.Free();
						return false;
					}
				}

				mobiles.Free();
			}

			BounceInfo bi = item.GetBounce();

			if (bi != null && (!Core.SA || AccessLevel > AccessLevel.Counselor))
			{
				Type type = item.GetType();

				if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
				{
					var objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

					if (objs != null && objs.Length > 0)
					{
						FlipableAttribute fp = objs[0] as FlipableAttribute;

						if (fp != null)
						{
							var itemIDs = fp.ItemIDs;

							Point3D oldWorldLoc = bi.m_WorldLoc;
							Point3D newWorldLoc = location;

							if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
							{
								Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

								if (itemIDs.Length == 2)
								{
									switch (dir)
									{
										case Direction.North:
										case Direction.South:
											item.ItemID = itemIDs[0];
											break;
										case Direction.East:
										case Direction.West:
											item.ItemID = itemIDs[1];
											break;
									}
								}
								else if (itemIDs.Length == 4)
								{
									switch (dir)
									{
										case Direction.South:
											item.ItemID = itemIDs[0];
											break;
										case Direction.East:
											item.ItemID = itemIDs[1];
											break;
										case Direction.North:
											item.ItemID = itemIDs[2];
											break;
										case Direction.West:
											item.ItemID = itemIDs[3];
											break;
									}
								}
							}
						}
					}
				}
			}

			return true;
		}

        public override int GetPacketFlags()
		{
			int flags = base.GetPacketFlags();

            return flags;
		}

		public override int GetOldPacketFlags()
		{
			int flags = base.GetOldPacketFlags();

			return flags;
		}

		public bool GetFlag(PlayerFlag flag)
		{
			return ((m_Flags & flag) != 0);
		}

		public void SetFlag(PlayerFlag flag, bool value)
		{
			if (value)
			{
				m_Flags |= flag;
			}
			else
			{
				m_Flags &= ~flag;
			}
		}

        public bool GetFlag(ExtendedPlayerFlag flag)
        {
            return ((m_ExtendedFlags & flag) != 0);
        }

        public void SetFlag(ExtendedPlayerFlag flag, bool value)
        {
            if (value)
            {
                m_ExtendedFlags |= flag;
            }
            else
            {
                m_ExtendedFlags &= ~flag;
            }
        }

		public DesignContext DesignContext { get { return m_DesignContext; } set { m_DesignContext = value; } }

		public static void Initialize()
		{
			if (FastwalkPrevention)
			{
				PacketHandlers.RegisterThrottler(0x02, MovementThrottle_Callback);
			}

			EventSink.Login += OnLogin;
			EventSink.Logout += OnLogout;
			EventSink.Connected += EventSink_Connected;
			EventSink.Disconnected += EventSink_Disconnected;

            #region Enchanced Client
            EventSink.TargetedSkill += Targeted_Skill;
            EventSink.EquipMacro += EquipMacro;
            EventSink.UnequipMacro += UnequipMacro;
            #endregion

            if (Core.SE)
			{
				Timer.DelayCall(TimeSpan.Zero, CheckPets);
			}
		}

        #region Enhanced Client
        private static void Targeted_Skill(TargetedSkillEventArgs e)
        {
            Mobile from = e.Mobile;
            int SkillId = e.SkillID;
            IEntity target = e.Target;

            if (from == null || target == null)
                return;

            from.TargetLocked = true;

            if (e.SkillID == 35)
            {
                AnimalTaming.DisableMessage = true;
                AnimalTaming.DeferredTarget = false;
            }

            if (from.UseSkill(e.SkillID) && from.Target != null)
            {
                from.Target.Invoke(from, target);
            }

            if (e.SkillID == 35)
            {
                AnimalTaming.DeferredTarget = true;
                AnimalTaming.DisableMessage = false;
            }

            from.TargetLocked = false;
        }

        public static void EquipMacro(EquipMacroEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null && pm.Backpack != null && pm.Alive && e.List != null && e.List.Count > 0)
            {
                Container pack = pm.Backpack;

                e.List.ForEach(serial =>
                {
                    Item item = pack.Items.FirstOrDefault(i => i.Serial == serial);

                    if (item != null)
                    {
                        Item toMove = pm.FindItemOnLayer(item.Layer);

                        if (toMove != null)
                        {
                            //pack.DropItem(toMove);
                            toMove.Internalize();

                            if (!pm.EquipItem(item))
                            {
                                pm.EquipItem(toMove);
                            }
                            else
                            {
                                pack.DropItem(toMove);
                            }
                        }
                        else
                        {
                            pm.EquipItem(item);
                        }
                    }
                });
            }
        }

        public static void UnequipMacro(UnequipMacroEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null && pm.Backpack != null && pm.Alive && e.List != null && e.List.Count > 0)
            {
                Container pack = pm.Backpack;

                List<Item> worn = new List<Item>(pm.Items);

                foreach (var item in worn)
                {
                    if (e.List.Contains((int)item.Layer))
                    {
                        pack.TryDropItem(pm, item, false);
                    }
                }

                ColUtility.Free(worn);
            }
        }
        #endregion

		//int petFollowers = 0;
		
        private static void CheckPets()
        {
            foreach (PlayerMobile pm in World.Mobiles.Values.OfType<PlayerMobile>())
            {
                if (((!pm.Mounted || (pm.Mount != null && pm.Mount is EtherealMount)) &&
                     (pm.AllFollowers.Count > pm.AutoStabled.Count)) ||
                    (pm.Mounted && (pm.AllFollowers.Count > (pm.AutoStabled.Count + 1))))
                {
                    pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
                }
            }
        }

		public override void OnSkillInvalidated(Skill skill)
		{
			if (Core.AOS && skill.SkillName == SkillName.MagicResist)
			{
				UpdateResistances();
			}
		}

        public override int GetMaxResistance(ResistanceType type)
        {
            if (IsStaff())
            {
                return 100;
            }
			int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
			if( magicResist >= 2000 )
				magicResist = 4000;
			else if( magicResist >= 1000 )
				magicResist += 500;
			
			return 50 + magicResist / 100;
			/*
            int max = base.GetMaxResistance(type);
            int refineBonus = BaseArmor.GetRefinedResist(this, type);

            if (refineBonus != 0)
            {
                max += refineBonus;
            }
            else
            {
                max += Spells.Mysticism.StoneFormSpell.GetMaxResistBonus(this);
            }

            if (Core.ML && Race == Race.Elf && type == ResistanceType.Energy)
            {
                max += 5; //Intended to go after the 60 max from curse
            }

            if (type != ResistanceType.Physical && 60 < max && Spells.Fourth.CurseSpell.UnderEffect(this))
            {
                max -= 10;
                //max = 60;
            }

            if ((type == ResistanceType.Fire || type == ResistanceType.Poison) && CorpseSkinSpell.IsUnderEffects(this))
            {
                max = CorpseSkinSpell.GetResistMalus(this);
            }

            return max;
			*/
        }

		public void Getgoldpoint(int getgoldpoint, bool harvest = true, bool quest = false)
		{
			if( !quest )
			{
				getgoldpoint = Server.Misc.Util.TiredCheck( this, Hunger, getgoldpoint, 0);
				double luckcheck = Utility.RandomDouble();
				if ( luckcheck < 0.0001 ) //+ killers[j].Luck * 0.000001 )
				{
					getgoldpoint *= 10;
					SendMessage( "엄청난 행운이 함께합니다!!!" );
				}
				else if ( luckcheck < 0.001 ) // + killers[j].Luck * 0.000001 )
				{
					getgoldpoint = (int)( getgoldpoint * 2.5 );
					SendMessage( "큰 행운이 함께합니다!!" );
				}
				else if ( luckcheck < 0.01 ) // + killers[j].Luck * 0.00001 )
				{
					getgoldpoint = (int)( getgoldpoint * 1.5 );
					SendMessage( "행운이 함께합니다!" );
				}
			}
			if( harvest )
			{
				double exp_bonus = 100;
				if( !quest )
					exp_bonus += GoldPoint[2] + AosAttributes.GetValue(this, AosAttribute.LowerRegCost) * 0.1;
				getgoldpoint = (int)( exp_bonus * getgoldpoint);
				getgoldpoint /= 100;
				Misc.Util.LevelUpEffect(this, getgoldpoint, 0);
			}
			else
			{
				double exp_bonus = 100;
				if( !quest )
					exp_bonus += GoldPoint[12] + AosAttributes.GetValue(this, AosAttribute.LowerManaCost) * 0.1;
				getgoldpoint = (int)( exp_bonus * getgoldpoint);
				getgoldpoint /= 100;
				Misc.Util.LevelUpEffect(this, getgoldpoint, 1);
			}
		}
		public void Getsilverpoint(int getsilverpoint, bool quest = false)
		{
			if( !quest )
			{
				double luckcheck = Utility.RandomDouble();
				if ( luckcheck < 0.0001 ) //+ killers[j].Luck * 0.000001 )
				{
					getsilverpoint *= 10;
					SendMessage( "엄청난 행운이 함께합니다!!!" );
				}
				else if ( luckcheck < 0.001 ) // + killers[j].Luck * 0.000001 )
				{
					getsilverpoint = (int)( getsilverpoint * 2.5 );
					SendMessage( "큰 행운이 함께합니다!!" );
				}
				else if ( luckcheck < 0.01 ) // + killers[j].Luck * 0.00001 )
				{
					getsilverpoint = (int)( getsilverpoint * 1.5 );
					SendMessage( "행운이 함께합니다!" );
				}
				double exp_bonus = 100 + SilverPoint[2] * 2 + AosAttributes.GetValue(this, AosAttribute.LowerAmmoCost) * 0.1;
				if( Map != Map.Trammel )
					exp_bonus += 50;
				getsilverpoint = (int)( exp_bonus * getsilverpoint);
				getsilverpoint /= 100;
			}
			Misc.Util.LevelUpEffect(this, getsilverpoint, 2);
		}
		
        public override void ComputeResistances()
        {
            base.ComputeResistances();

            for (int i = 0; i < Resistances.Length; ++i)
            {
                Resistances[i] = 0;
            }
		
            Resistances[0] += BasePhysicalResistance + AosWeaponAttributes.GetValue(this, AosWeaponAttribute.ResistPhysicalBonus ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.AllResist ); // + SilverPoint[27];
            Resistances[1] += BaseFireResistance + AosWeaponAttributes.GetValue(this, AosWeaponAttribute.ResistFireBonus ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.AllResist ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.ElementalResist ); // + SilverPoint[27];
            Resistances[2] += BaseColdResistance + AosWeaponAttributes.GetValue(this, AosWeaponAttribute.ResistColdBonus ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.AllResist ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.ElementalResist ); // + SilverPoint[27];
            Resistances[3] += BasePoisonResistance + AosWeaponAttributes.GetValue(this, AosWeaponAttribute.ResistPoisonBonus ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.AllResist ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.ElementalResist ); // + SilverPoint[27];
            Resistances[4] += BaseEnergyResistance + AosWeaponAttributes.GetValue(this, AosWeaponAttribute.ResistEnergyBonus ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.AllResist ) + AosArmorAttributes.GetValue(this, AosArmorAttribute.ElementalResist ); // + SilverPoint[27];
			Resistances[5] += BaseChaosResistance;
			Resistances[6] += BaseDirectResistance;
			
            for (int i = 0; ResistanceMods != null && i < ResistanceMods.Count; ++i)
            {
                ResistanceMod mod = ResistanceMods[i];
                int v = (int)mod.Type;

                if (v >= 0 && v < Resistances.Length)
                {
                    Resistances[v] += mod.Offset;
                }
            }

            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item.CheckPropertyConfliction(this))
                {
                    continue;
                }

                ISetItem setItem = item as ISetItem;

                Resistances[0] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Physical) : item.PhysicalResistance;
                Resistances[1] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Fire) : item.FireResistance;
                Resistances[2] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Cold) : item.ColdResistance;
                Resistances[3] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Poison) : item.PoisonResistance;
                Resistances[4] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Energy) : item.EnergyResistance;
            }

            for (int i = 0; i < Resistances.Length; ++i)
            {
                int min = GetMinResistance((ResistanceType)i);
                int max = GetMaxResistance((ResistanceType)i);

                if (max < min)
                {
                    max = min;
                }

                if (Resistances[i] > max)
                {
                    Resistances[i] = max;
                }
                else if (Resistances[i] < min)
                {
                    Resistances[i] = min;
                }
            }
        }

		protected override void OnRaceChange(Race oldRace)
		{
            if (oldRace == Race.Gargoyle && Flying)
            {
                Flying = false;
                SendSpeedControl(SpeedControlType.Disable);
                BuffInfo.RemoveBuff(this, BuffIcon.Fly);
            }
            else if (oldRace != Race.Gargoyle && Race == Race.Gargoyle && Mounted)
            {
                Mount.Rider = null;
            }

			ValidateEquipment();
			UpdateResistances();
		}

		public override int MaxWeight { get { return 2500 + Misc.Util.Level( GoldPoint[0] ) * 30; } }//(int)( ( Skills.Begging.Base + Skills.Camping.Base + Skills.Fishing.Base + Skills.Herding.Base + Skills.Mining.Base + Skills.Lockpicking.Base + Skills.Lumberjacking.Base + Skills.RemoveTrap.Base + Skills.Stealing.Base + Skills.Stealing.Base ) * 5 ); }  }
		
		private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

		public override void OnNetStateChanged()
		{
			m_LastGlobalLight = -1;
			m_LastPersonalLight = -1;
		}

		public override void ComputeBaseLightLevels(out int global, out int personal)
		{
			global = LightCycle.ComputeLevelFor(this);

			bool racialNightSight = (Core.ML && Race == Race.Elf);

			if (LightLevel < 21 && (AosAttributes.GetValue(this, AosAttribute.NightSight) > 0 || racialNightSight))
			{
				personal = 21;
			}
			else
			{
				personal = LightLevel;
			}
		}

		public override void CheckLightLevels(bool forceResend)
		{
			NetState ns = NetState;

			if (ns == null)
			{
				return;
			}

			int global, personal;

			ComputeLightLevels(out global, out personal);

			if (!forceResend)
			{
				forceResend = (global != m_LastGlobalLight || personal != m_LastPersonalLight);
			}

			if (!forceResend)
			{
				return;
			}

			m_LastGlobalLight = global;
			m_LastPersonalLight = personal;

			ns.Send(GlobalLightLevel.Instantiate(global));
			ns.Send(new PersonalLightLevel(this, personal));
		}

        public override bool SendSpeedControl(SpeedControlType type)
        {
            AnimalFormContext context = AnimalForm.GetContext(this);

            if (context != null && context.SpeedBoost)
            {
                switch (type)
                {
                    case SpeedControlType.WalkSpeed: return base.SendSpeedControl(SpeedControlType.WalkSpeedFast);
                    case SpeedControlType.Disable: return base.SendSpeedControl(SpeedControlType.MountSpeed);
                }
            }

            return base.SendSpeedControl(type);
        }

		public override int GetMinResistance(ResistanceType type)
		{
			return 0;
			int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
			int min = int.MinValue;

			if (magicResist >= 1000)
			{
				min = 40 + ((magicResist - 1000) / 50);
			}
			else if (magicResist >= 400)
			{
				min = (magicResist - 400) / 15;
			}

			return Math.Max(MinPlayerResistance, Math.Min(MaxPlayerResistance, min));
		}

        #region City Loyalty
        public override int GetResistance(ResistanceType type)
        {
            int resistance = base.GetResistance(type) + SphynxFortune.GetResistanceBonus(this, type);

            if (CityLoyaltySystem.HasTradeDeal(this, TradeDeal.SocietyOfClothiers))
            {
                resistance++;
                 return Math.Min(resistance, GetMaxResistance(type));
            }

            return resistance;
        }
        #endregion

        public override void OnManaChange(int oldValue)
		{
			base.OnManaChange(oldValue);
			if (m_ExecutesLightningStrike > 0)
			{
				if (Mana < m_ExecutesLightningStrike)
				{
					SpecialMove.ClearCurrentMove(this);
				}
			}
		}

		private static void OnLogin(LoginEventArgs e)
		{
			Mobile from = e.Mobile;
			IAccount a = from.Account;
			if( a.Count > 0 )
			{
				for (int i = 0; i < a.Length; ++i)
				{
					Mobile check = a[i];

					if (check != null && check.Map != Map.Internal && check != from && check.NetState != null)
					{
						check.NetState.Dispose();
					}
				}			
			}
			CheckAtrophies(from);

			if (AccountHandler.LockdownLevel > AccessLevel.VIP)
			{
				string notice;

				Account acct = from.Account as Account;

				if (acct == null || !acct.HasAccess(from.NetState))
				{
					if (from.IsPlayer())
					{
						notice = "The server is currently under lockdown. No players are allowed to log in at this time.";
					}
					else
					{
						notice = "The server is currently under lockdown. You do not have sufficient access level to connect.";
					}

					Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Disconnect), from);
				}
				else if (from.AccessLevel >= AccessLevel.Administrator)
				{
					notice =
						"The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
				}
				else
				{
					notice = "The server is currently under lockdown. You have sufficient access level to connect.";
				}

				from.SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
				return;
			}

			if (from is PlayerMobile)
			{
				((PlayerMobile)from).ClaimAutoStabledPets();
                ((PlayerMobile)from).ValidateEquipment();
				if( !((PlayerMobile)from).IsStaff() && ((PlayerMobile)from).Map != Map.Trammel )
					((PlayerMobile)from).PlayerMove();
                ReportMurdererGump.CheckMurderer(from);
				if( ((PlayerMobile)from).SkillsCap == 1000000 )
					((PlayerMobile)from).SkillsCap = 15000;
			}
            else if (Siege.SiegeShard && from.Map == Map.Trammel && from.AccessLevel == AccessLevel.Player)
            {
                from.Map = Map.Felucca;
            }

            if (((from.Map == Map.Trammel && from.Region.IsPartOf("Blackthorn Castle")) || PointsSystem.FellowshipData.Enabled && from.Region.IsPartOf("BlackthornDungeon") || from.Region.IsPartOf("Ver Lor Reg")) && from.Player && from.AccessLevel == AccessLevel.Player && from.CharacterOut)
            {
                StormLevelGump menu = new StormLevelGump(from);
                menu.BeginClose();
                from.SendGump(menu);
            }

            if (from.NetState != null && from.NetState.IsEnhancedClient && from.Mount is EtherealMount)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), mount =>
                {
                    if (mount.IsChildOf(from.Backpack))
                    {
                        mount.Rider = from;
                    }
                }, 
                (EtherealMount)from.Mount);
            }

            from.CheckStatTimers();
        }
		private static readonly int[,] m_StatUp = new int[,]
		{
			//	str	dex	int	luc	hit	stm	mana
			{ 250, 550, 550, 0, 800, 800, 800 },  //Alchemy
			{ 1200, 1100, 500, 0, 1000, 1000, 200 },  //Anatomy
			{ 200, 700, 1500, 0, 600, 800, 1200 },  //Animal Lore
			{ 600, 600, 600, 0, 1000, 700, 250 },  //Item Identification
			{ 500, 500, 1250, 0, 100, 100, 1300 },  //Arms Lore
			{ 650, 650, 100, 0, 2000, 300, 50 },  //Parrying
			{ 600, 600, 800, 0, 700, 1000, 50 },  //Begging
			{ 1000, 400, 200, 0, 900, 1200, 50 },  //Blacksmithy
			{ 400, 1500, 100, 0, 700, 1000, 50 },  //Bowcraft/Fletching
			{ 200, 800, 1300, 0, 800, 800, 1100 },  //Peacemaking
			{ 800, 800, 300, 0, 1000, 800, 50 },  //Camping
			{ 900, 1000, 500, 0, 1000, 300, 50 },  //Carpentry
			{ 200, 600, 1800, 0, 500, 600, 50 },  //Cartography
			{ 740, 740, 740, 0, 740, 740, 50 },  //Cooking
			{ 250, 600, 1200, 0, 300, 700, 700 },  //Detecting Hidden
			{ 100, 800, 1000, 0, 700, 1400, 1000 },  //Discordance
			{ 50, 50, 2000, 0, 300, 300, 1050 },  //Evaluating Intelligence
			{ 900, 1100, 800, 0, 1100, 700, 400 },  //Healing
			{ 740, 740, 740, 0, 740, 740, 50 },  //Fishing
			{ 800, 800, 1000, 0, 800, 800, 800 },  //Belief(Forensic Evaluation)
			{ 600, 600, 800, 0, 900, 800, 50 },  //Farming(Herding)
			{ 50, 2000, 50, 0, 600, 950, 100 },  //Hiding
			{ 1200, 400, 1300, 0, 1200, 200, 700 },  //Provocation
			{ 50, 500, 1000, 0, 300, 100, 1800 },  //Inscription
			{ 500, 1100, 900, 0, 600, 600, 50 },  //Lockpicking
			{ 200, 200, 1900, 0, 600, 200, 1900 },  //Magery
			{ 50, 50, 1600, 0, 1200, 1200, 900 },  //Resisting Spells
			{ 500, 500, 1400, 0, 1200, 1200, 200 },  //Tactics
			{ 50, 1900, 500, 0, 600, 1900, 50 },  //Avoid(Snooping)
			{ 100, 800, 1800, 0, 700, 600, 1000 },  //Musicianship
			{ 100, 400, 1800, 0, 300, 1200, 1200 },  //Poisoning
			{ 900, 1400, 500, 0, 700, 1400, 100 },  //Archery
			{ 200, 200, 1000, 0, 1800, 100, 1700 },  //Spirit Speak
			{ 200, 2000, 700, 0, 300, 500, 50 },  //Stealing
			{ 300, 900, 1700, 0, 300, 500, 50 },  //Tailoring
			{ 500, 900, 900, 0, 1500, 600, 600 },  //Animal Taming
			{ 1200, 1000, 600, 0, 400, 500, 50 },  //Tanning(Taste Identification)
			{ 500, 1100, 1200, 0, 200, 500, 250 },  //Tinkering
			{ 300, 800, 300, 0, 300, 2000, 50 },  //Tracking
			{ 1000, 800, 1200, 0, 400, 1000, 600 },  //Veterinary
			{ 1100, 1100, 800, 0, 900, 900, 200 },  //Swordsmanship
			{ 2000, 200, 400, 0, 800, 300, 50 },  //Mace Fighting
			{ 200, 1000, 300, 0, 200, 2000, 50 },  //Fencing
			{ 900, 700, 600, 0, 1300, 1300, 200 },  //Wrestling
			{ 2000, 300, 100, 0, 900, 400, 50 },  //Lumberjacking
			{ 800, 400, 100, 0, 2000, 400, 50 },  //Mining
			{ 100, 100, 1000, 0, 500, 50, 2000 },  //Meditation
			{ 300, 1800, 1700, 0, 400, 600, 200 },  //Stealth
			{ 400, 600, 1800, 0, 500, 1500, 200 },  //Remove Trap
			{ 600, 800, 1200, 0, 800, 700, 900 },  //Necromancy
			{ 1400, 1400, 400, 0, 1300, 400, 100 },  //Smash(Focus)
			{ 700, 700, 700, 0, 1500, 700, 700 },  //Chivalry
			{ 1600, 1000, 200, 0, 1400, 700, 100 },  //Bushido
			{ 300, 1900, 700, 0, 600, 1000, 500 },  //Ninjitsu
			{ 200, 200, 1900, 0, 600, 200, 1900 },  //Spellweaving
			{ 100, 100, 900, 0, 600, 50, 2000 },  //Mysticism
			{ 100, 100, 1500, 0, 600, 50, 1400 },  //Imbuing
			{ 900, 1400, 500, 0, 700, 1400, 100 }  //Throwing
			/*
			{	250,550,550,0,	800,800,800	},	//Alchemy
			{	1200,1100,500,0,1000,1000,200},	//Anatomy
			{	200,700,1500,0,	8,	7,	2	},	//Animal Lore
			{	0,	0,	0,	0,	10,	5,	0	},	//Item Identification
			{	0,	0,	0,	0,	7,	2,	8	},	//Arms Lore
			{	8,	2,	0,	0,	10,	0,	0	},	//Parrying
			{	0,	0,	0,	0,	8,	7,	0	},	//Begging
			{	5,	0,	0,	0,	13,	2,	0	},	//Blacksmithy
			{	0,	0,	0,	0,	12,	3,	0	},	//Bowcraft/Fletching
			{	0,	0,	0,	0,	9,	4,	4	},	//Peacemaking
			{	0,	0,	0,	0,	1,	5,	0	},	//Camping
			{	0,	0,	0,	0,	12,	3,	0	},	//Carpentry
			{	0,	0,	0,	0,	6,	8,	1	},	//Cartography
			{	0,	0,	0,	0,	10,	5,	1	},	//Cooking
			{	0,	0,	0,	0,	10,	4,	4	},	//Detecting Hidden
			{	0,	0,	0,	0,	10,	5,	2	},	//Discordance
			{	0,	0,	10,	0,	6,	0,	0	},	//Evaluating Intelligence
			{	0,	0,	0,	0,	14,	6,	0	},	//Healing
			{	0,	0,	0,	0,	12,	3,	0	},	//Fishing
			{	2,	2,	6,	0,	10,	0,	0	},	//Forensic Evaluation
			{	0,	0,	0,	0,	11,	6,	0	},	//Herding
			{	0,	0,	0,	0,	9,	7,	2	},	//Hiding
			{	0,	0,	0,	0,	8,	4,	5	},	//Provocation
			{	0,	0,	0,	0,	6,	1,	8	},	//Inscription
			{	0,	0,	0,	0,	12,	6,	0	},	//Lockpicking
			{	2,	0,	8,	0,	6,	0,	0	},	//Magery
			{	5,	0,	5,	0,	10,	0,	0	},	//Resisting Spells
			{	4,	6,	0,	0,	10,	0,	0	},	//Tactics
			{	0,	0,	0,	0,	11,	5,	2	},	//Snooping
			{	0,	0,	0,	0,	8,	2,	7	},	//Musicianship
			{	0,	0,	0,	0,	15,	3,	0	},	//Poisoning
			{	3,	7,	0,	0,	9,	0,	0	},	//Archery
			{	0,	0,	0,	0,	11,	0,	5	},	//Spirit Speak
			{	0,	0,	0,	0,	10,	8,	0	},	//Stealing
			{	0,	2,	0,	0,	7,	8,	0	},	//Tailoring
			{	0,	0,	0,	0,	13,	3,	1	},	//Animal Taming
			{	0,	0,	0,	0,	13,	1,	1	},	//Taste Identification
			{	0,	0,	0,	0,	6,	9,	0	},	//Tinkering
			{	0,	0,	0,	0,	8,	10,	0	},	//Tracking
			{	0,	0,	0,	0,	9,	2,	6	},	//Veterinary
			{	5,	5,	0,	0,	10,	0,	0	},	//Swordsmanship
			{	6,	4,	0,	0,	10,	0,	0	},	//Mace Fighting
			{	4,	6,	0,	0,	10,	0,	0	},	//Fencing
			{	7,	3,	0,	0,	10,	0,	0	},	//Wrestling
			{	0,	0,	0,	0,	14,	1,	0	},	//Lumberjacking
			{	0,	0,	0,	0,	14,	1,	0	},	//Mining
			{	0,	0,	0,	0,	6,	0,	10	},	//Meditation
			{	0,	5,	0,	0,	8,	5,	0	},	//Stealth
			{	0,	0,	0,	0,	11,	7,	0	},	//Remove Trap
			{	5,	0,	5,	0,	6,	0,	0	},	//Necromancy
			{	0,	5,	5,	0,	10,	0,	0	},	//Focus
			{	3,	5,	2,	0,	10,	0,	0	},	//Chivalry
			{	7,	3,	0,	0,	10,	0,	0	},	//Bushido
			{	1,	6,	3,	0,	8,	0,	0	},	//Ninjitsu
			{	0,	0,	10,	0,	6,	0,	0	},	//Spellweaving
			{	0,	0,	10,	0,	6,	0,	0	},	//Mysticism
			{	0,	0,	0,	0,	6,	0,	9	},	//Imbuing
			{	0,	0,	0,	0,	11,	7,	0	}	//Throwing
			{ 	0,	0,	5,	10,	0,	1,	10	}, //alchemy
			{ 	3,	9,	3,	0,	35,	0,	0	}, //anatomy
			{ 	0,	0,	10,	20,	5,	10,	5	}, //animal lore
			{ 	5,	0,	3,	10,	0,	0,	7	}, //item iden
			{ 	0,	0,	0,	20,	5,	0,	0	}, //arms lore
			{ 	20,	20,	0,	0,	10,	0,	0	}, //parrying
			{ 	0,	0,	0,	40,	5,	5,	0	}, //begging
			{ 	10,	3,	0,	2,	5,	5,	0	}, //blacksmithy
			{ 	3,	7,	0,	0,	10,	5,	0	}, //bowcraft
			{ 	0,	0,	20,	30,	0,	0,	0	}, //peachmaking
			{ 	7,	5,	7,	6,	10,	15,	0	}, //camping
			{ 	7,	3,	0,	0,	10,	5,	0	}, //carpentry
			{ 	0,	5,	5,	0,	0,	15,	0	}, //cartography
			{ 	4,	4,	4,	5,	4,	4,	0	}, //cooking
			{ 	0,	20,	10,	5,	0,	10,	5	}, //detecting hidden
			{ 	0,	0,	20,	30,	0,	0,	0	}, //discodance
			{ 	0,	0,	10,	0,	0,	0,	40	}, //Evaluating
			{ 	0,	5,	0,	0,	45,	0,	0	}, //Healing
			{ 	10,	5,	10,	5,	5,	5,	10	}, //fishing
			{ 	0,	0,	50,	0,	0,	1,	0	}, //forensic evaluation
			{ 	0,	12,	0,	0,	21,	17,	0	}, //herding
			{ 	0,	5,	0,	0,	0,	45,	0	}, //hiding
			{ 	0,	0,	20,	30,	0,	3,	0	}, //privocation
			{ 	0,	5,	10,	0,	0,	0,	10	}, //inscription
			{ 	5,	20,	0,	10,	0,	15,	0	}, //lockpicking
			{ 	0,	0,	15,	10,	0,	0,	25	}, //magery
			{ 	0,	0,	20,	0,	20,	0,	10	}, //resisting
			{ 	35,	15,	0,	0,	0,	0,	0	}, //tactics
			{ 	0,	10,	0,	15,	0,	25,	0	}, //snooping
			{ 	0,	0,	20,	0,	0,	0,	0	}, //musicianship
			{ 	0,	10,	0,	15,	0,	0,	0	}, //poisoning
			{ 	25,	20,	0,	5,	0,	0,	0	}, //archery
			{ 	0,	0,	0,	0,	0,	0,	25	}, //spirit speak
			{ 	0,	30,	10,	5,	0,	5,	0	}, //stealing
			{ 	0,	5,	0,	0,	10,	10,	0	}, //tailoring
			{ 	0,	0,	5,	10,	30,	5,	0	}, //animal training
			{ 	0,	0,	5,	5,	5,	10,	0	}, //taste iden
			{ 	0,	8,	8,	8,	0,	1,	0	}, //tinkering
			{ 	0,	20,	20,	0,	0,	10,	0	}, //tracking
			{ 	0,	15,	0,	2,	0,	30,	5	}, //veterinary
			{ 	30,	10,	5,	1,	3,	1,	0	}, //sword
			{ 	40,	0,	0,	5,	4,	1,	0	}, //mace
			{ 	21,	19,	0,	6,	3,	1,	0	}, //fencing
			{ 	30,	10,	0,	0,	9,	1,	0	}, //wrestling
			{ 	30,	10,	1,	0,	6,	4,	3	}, //lumberjacking
			{ 	30,	0,	0,	0,	10,	10,	0	}, //mining
			{ 	0,	0,	0,	0,	0,	0,	25	}, //meditation
			{ 	0,	15,	0,	10,	0,	25,	0	}, //stealth
			{ 	0,	5,	10, 5,	10,	20,	0	}, //remove trap
			{ 	0,	0,	10,	10,	0,	0,	30	}, //necromancy
			{ 	0,	0,	0,	0,	20,	20,	10	}, //focus
			{ 	10,	5,	5,	0,	30,	0,	0	}, //chivalry
			{ 	25,	0,	0,	0,	20,	5,	0	}, //bushido
			{ 	0,	15,	15,	0,	0,	10,	10	}, //ninjitsu
			{ 	0,	0,	10,	0,	0,	0,	40	}, //spellweaving
			{ 	0,	0,	5,	5,	0,	0,	45	}, //mysticism
			{ 	0,	0,	0,	17,	0,	0,	8	}, //imbuing
			{ 	25,	20,	0,	5,	0,	0,	0	} //throwing
			*/
		};
		private bool m_NoDeltaRecursion;

		public void ValidateEquipment()
		{
			if (m_NoDeltaRecursion || Map == null || Map == Map.Internal)
			{
				return;
			}

			if (Items == null)
			{
				return;
			}

			m_NoDeltaRecursion = true;
			Timer.DelayCall(TimeSpan.Zero, ValidateEquipment_Sandbox);
		}

		private void ValidateEquipment_Sandbox()
		{
			try
			{
				if (Map == null || Map == Map.Internal)
				{
					return;
				}

				var items = Items;

				if (items == null)
				{
					return;
				}

				bool moved = false;

				int str = Str;
				int dex = Dex;
				int intel = Int;

				#region Factions
				int factionItemCount = 0;
				#endregion

				Mobile from = this;

				#region Ethics
				Ethic ethic = Ethic.Find(from);
				#endregion

                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    Item item = items[i];

                    #region Ethics
                    if ((item.SavedFlags & 0x100) != 0)
                    {
                        if (item.Hue != Ethic.Hero.Definition.PrimaryHue)
                        {
                            item.SavedFlags &= ~0x100;
                        }
                        else if (ethic != Ethic.Hero)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                            continue;
                        }
                    }
                    else if ((item.SavedFlags & 0x200) != 0)
                    {
                        if (item.Hue != Ethic.Evil.Definition.PrimaryHue)
                        {
                            item.SavedFlags &= ~0x200;
                        }
                        else if (ethic != Ethic.Evil)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                            continue;
                        }
                    }
                    #endregion

                    bool morph = from.FindItemOnLayer(Layer.Earrings) is MorphEarrings;

                    if (item is BaseWeapon)
                    {
                        BaseWeapon weapon = (BaseWeapon)item;

                        bool drop = false;

                        if (dex < AOS.Scale2(weapon.DexRequirement, 1000 - weapon.GetLowerStatReq()))
                        {
                            drop = true;
                        }
                        else if (str < AOS.Scale2(weapon.StrRequirement, 1000 - weapon.GetLowerStatReq()))
                        {
                            drop = true;
                        }
                        else if (intel < AOS.Scale2(weapon.IntRequirement, 1000 - weapon.GetLowerStatReq()))
                        {
                            drop = true;
                        }
                        else if (weapon.RequiredRace != null && weapon.RequiredRace != Race && !morph)
                        {
                            drop = true;
                        }

                        if (drop)
                        {
                            string name = weapon.Name;

                            if (name == null)
                            {
                                name = String.Format("#{0}", weapon.LabelNumber);
                            }

                            from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
                            from.AddToBackpack(weapon);
                            moved = true;
                        }
                    }
                    else if (item is BaseArmor)
                    {
                        BaseArmor armor = (BaseArmor)item;

                        bool drop = false;

                        if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (armor.RequiredRace != null && armor.RequiredRace != Race && !morph)
                        {
                            drop = true;
                        }
                        else
                        {
                            int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
                            int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
                            int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

                            if (dex < dexReq || (dex + dexBonus) < 1)
                            {
                                drop = true;
                            }
                            else if (str < strReq || (str + strBonus) < 1)
                            {
                                drop = true;
                            }
                            else if (intel < intReq || (intel + intBonus) < 1)
                            {
                                drop = true;
                            }
                        }

                        if (drop)
                        {
                            string name = armor.Name;

                            if (name == null)
                            {
                                name = String.Format("#{0}", armor.LabelNumber);
                            }

                            if (armor is BaseShield)
                            {
                                from.SendLocalizedMessage(1062003, name); // You can no longer equip your ~1_SHIELD~
                            }
                            else
                            {
                                from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~
                            }

                            from.AddToBackpack(armor);
                            moved = true;
                        }
                    }
                    else if (item is BaseClothing)
                    {
                        BaseClothing clothing = (BaseClothing)item;

                        bool drop = false;

                        if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (clothing.RequiredRace != null && clothing.RequiredRace != Race && !morph)
                        {
                            drop = true;
                        }
                        else
                        {
                            int strBonus = clothing.ComputeStatBonus(StatType.Str);
                            int strReq = clothing.ComputeStatReq(StatType.Str);

                            if (str < strReq || (str + strBonus) < 1)
                            {
                                drop = true;
                            }
                        }

                        if (drop)
                        {
                            string name = clothing.Name;

                            if (name == null)
                            {
                                name = String.Format("#{0}", clothing.LabelNumber);
                            }

                            from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

                            from.AddToBackpack(clothing);
                            moved = true;
                        }
                    }
                    else if (item is BaseQuiver)
                    {
                        if (Race == Race.Gargoyle)
                        {
                            from.AddToBackpack(item);

                            from.SendLocalizedMessage(1062002, "quiver"); // You can no longer wear your ~1_ARMOR~
                            moved = true;
                        }
                    }

                    FactionItem factionItem = FactionItem.Find(item);

                    if (factionItem != null)
                    {
                        bool drop = false;

                        PlayerState state = PlayerState.Find(this);
                        Faction ourFaction = null;

                        if (state != null)
                            ourFaction = state.Faction;

                        if (ourFaction == null || ourFaction != factionItem.Faction)
                        {
                            drop = true;
                        }
                        else if (state != null && state.Rank.Rank < factionItem.MinRank)
                        {
                            drop = true;
                        }
                        else if (++factionItemCount > FactionItem.GetMaxWearables(this))
                        {
                            drop = true;
                        }

                        if (drop)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                        }
                    }

                    #region Vice Vs Virtue
                    IVvVItem vvvItem = item as IVvVItem;

                    if (vvvItem != null && vvvItem.IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(from))
                    {
                        from.AddToBackpack(item);
                        moved = true;
                    }
                    #endregion
                }

				if (moved)
				{
					from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				m_NoDeltaRecursion = false;
			}
		}

		public override void Delta(MobileDelta flag)
		{
			base.Delta(flag);

			if ((flag & MobileDelta.Stat) != 0)
			{
				ValidateEquipment();
			}

            InvalidateProperties();
		}

		private static void Disconnect(object state)
		{
			NetState ns = ((Mobile)state).NetState;

			if (ns != null)
			{
				ns.Dispose();
			}
		}

		private static void OnLogout(LogoutEventArgs e)
		{
            PlayerMobile pm = e.Mobile as PlayerMobile;

			//if(pm == null)
			//	return;

            #region Scroll of Alacrity
            if (pm.AcceleratedStart > DateTime.UtcNow)
			{
				pm.AcceleratedStart = DateTime.UtcNow;
                ScrollOfAlacrity.AlacrityEnd(pm);
			}
			#endregion

            //BaseFamiliar.OnLogout(pm);

            //BaseEscort.DeleteEscort(pm);
        }

		//스킬에 의한 스텟 최대값
		private readonly int StatUpMax = 1000000;
		private int m_SkillFollows;
        public int SkillFollows { get { return m_SkillFollows; } set { m_SkillFollows = value; InvalidateProperties(); } }
		private int m_PetFollows;
        public int PetFollows { get { return m_PetFollows; } set { m_PetFollows = value; InvalidateProperties(); } }
		private int m_EquipFollows;
        public int EquipFollows { get { return m_EquipFollows; } set { m_EquipFollows = value; InvalidateProperties(); } }

		public double poisonattacker = 0.0;
		
		public bool connection_check = false;
		
		private static void EventSink_Connected(ConnectedEventArgs e)
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm != null)
			{
				pm.m_SessionStart = DateTime.UtcNow;

				if (pm.m_Quest != null)
				{
					pm.m_Quest.StartTimer();
				}

				#region Mondain's Legacy
				QuestHelper.StartTimer(pm);
				#endregion

				pm.BedrollLogout = false;
                pm.BlanketOfDarknessLogout = false;
				pm.Tired -= Server.Misc.Util.RestCal( pm.LastOnline, DateTime.UtcNow );
                pm.LastOnline = DateTime.UtcNow;
				if( pm.Tired < -108000 )
					pm.Tired = -108000;
				
				//가문 스킬 체크
				Account acc = pm.Account as Account;

				if( acc != null && acc.Count > 1 )
				{
					double AccountSkillSum = 0.0;
					for ( int i = 0; i < 58; i++)
					{
						for (int j = 0; j < acc.Length; ++j)
						{
							Mobile check = acc[j];
							if (check != null && check != pm )
								AccountSkillSum += check.Skills[i].Base * 0.1;
						}
						SkillName skill = (SkillName)Enum.ToObject(typeof(SkillName), i);
						SkillMod sk = new DefaultSkillMod(skill, true, AccountSkillSum);
						sk.ObeyCap = true;
						pm.AddSkillMod(sk);
						AccountSkillSum = 0.0;
					}
				}
				
				
				/*
				if( pm.StamMax > pm.Stam )
				{
					int timecheck = Util.TimeValue(pm.LastOnline, DateTime.UtcNow);
					pm.TimerList[66] += timecheck;
				}
				*/
				/*
				pm.SilverPointbyEquipCheck = 0;
				for(int i = 1; i < pm.SilverPoint.Length; i++)
				{
					pm.SilverPointbyEquipCheck += pm.SilverPointScore[pm.SilverPoint[i]];
				}
				pm.SilverPointbyEquipRank = pm.SilverPointbyEquipSort();
				*/

				if( !pm.connection_check )
				{
					pm.connection_check = true;
					pm.PlayerCount();
					pm.ResetMonth();
					pm.ResetWeek();
					pm.ResetDay();
					pm.BuffCount();
				}
				//if( pm.DeathCheck < 1 )
				//	pm.DeathCheck = 1;
				/*
				//실버 포인트 이전
				if( pm.GatePoint != 0 )
				{
					pm.SilverPoint[0] += pm.GatePoint;
					pm.GatePoint = 0;
				}
				*/
				/*
				//악세사리, 마법책 포인트 이전
				if( pm.EquipPoint[9] >= 4 && pm.EquipPoint[19] == 0 && pm.EquipPoint[20] == 0 && pm.EquipPoint[21] == 0 )
				{
					pm.EquipPoint[9] /= 4;
					pm.EquipPoint[19] = pm.EquipPoint[9];
					pm.EquipPoint[20] = pm.EquipPoint[9];
					pm.EquipPoint[21] = pm.EquipPoint[9];
				}
				if( pm.EquipPoint[17] >= 4 && pm.EquipPoint[22] == 0 && pm.EquipPoint[23] == 0 && pm.EquipPoint[24] == 0 )
				{
					pm.EquipPoint[17] /= 4;
					pm.EquipPoint[22] = pm.EquipPoint[17];
					pm.EquipPoint[23] = pm.EquipPoint[17];
					pm.EquipPoint[24] = pm.EquipPoint[17];
				}
				*/
			}
			//주별 오류 수정

			/*
			if( pm.WeekTime.Month != 7 )
			{
				pm.WeekTime = new DateTime(2020, 7, 1, 0, 0, 0);
			}
			if( pm.MonthTime.Day != 1 )
				pm.MonthTime = new DateTime(2020, 8, 1, 0, 0, 0);
			*/
			//스킬을 스텟으로 변환
			for( int i = 0; i < m_StatUp.GetLength(1); i++)
			{
				//pm.SkillbyStat[i] = 0;
				for( int j = 0; j < m_StatUp.GetLength(0); j++)
				{
					if( pm.SkillbyStat[i] < m_StatUp[j, i] * (int)( pm.Skills[j].Value * 10 ) )
						pm.SkillbyStat[i] = m_StatUp[j, i] * (int)( pm.Skills[j].Value * 10);
				}
			}
			/*
			for( int i = 0; i < pm.SkillbyStat.Length; i++)
			{
				if( pm.SkillbyStat[i] < pm.StatUpMax )
				{
					pm.SkillFull = false;
					break;
				}
			}
			*/
			if( !pm.GoldAndSilverPointReturntoSkill )
			{
				int returnPoint = 0;
				while( pm.Skills.Forensics.Base >= 0.1 )
				{
					pm.Skills.Forensics.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 19)) / 10;
				}
				pm.SendMessage("법의학 스킬이 초기화 되고 {0}점의 생산 포인트를 환원 받았습니다.", returnPoint);
				pm.GoldPoint[0] += returnPoint;

				returnPoint = 0;
				while( pm.Skills.Camping.Base >= 0.1 )
				{
					pm.Skills.Camping.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 10)) / 10;
				}
				pm.SendMessage("야영술 스킬이 초기화 되고 {0}점의 생산 포인트를 환원 받았습니다.", returnPoint);
				pm.GoldPoint[0] += returnPoint;

				returnPoint = 0;
				while( pm.Skills.Tracking.Base >= 0.1 )
				{
					pm.Skills.Tracking.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 38)) / 10;
				}
				pm.SendMessage("추적술 스킬이 초기화 되고 {0}점의 생산 포인트를 환원 받았습니다.", returnPoint);
				pm.GoldPoint[0] += returnPoint;

				returnPoint = 0;
				while( pm.Skills.Imbuing.Base >= 0.1 )
				{
					pm.Skills.Imbuing.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 56)) / 10;
				}
				pm.SendMessage("임뷰잉 스킬이 초기화 되고 {0}점의 생산 포인트를 환원 받았습니다.", returnPoint);
				pm.GoldPoint[0] += returnPoint;

				returnPoint = 0;
				while( pm.Skills.Tactics.Base >= 0.1 )
				{
					pm.Skills.Tactics.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 27)) / 10;
				}
				pm.SendMessage("전술 스킬이 초기화 되고 {0}점의 전투 포인트를 환원 받았습니다.", returnPoint);
				pm.SilverPoint[0] += returnPoint;
				
				returnPoint = 0;
				while( pm.Skills.EvalInt.Base >= 0.1 )
				{
					pm.Skills.EvalInt.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 16)) / 10;
				}
				pm.SendMessage("지능 평가 스킬이 초기화 되고 {0}점의 전투 포인트를 환원 받았습니다.", returnPoint);
				pm.SilverPoint[0] += returnPoint;

				returnPoint = 0;
				while( pm.Skills.Chivalry.Base >= 0.1 )
				{
					pm.Skills.Chivalry.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 51)) / 10;
				}
				pm.SendMessage("기사도 스킬이 초기화 되고 {0}점의 전투 포인트를 환원 받았습니다.", returnPoint);
				pm.SilverPoint[0] += returnPoint;
				
				returnPoint = 0;
				while( pm.Skills.Ninjitsu.Base >= 0.1 )
				{
					pm.Skills.Ninjitsu.Base -= 0.1;
					returnPoint += (int)( Misc.Util.SkillExp_Calc(pm, 53)) / 10;
				}
				pm.SendMessage("닌자술 스킬이 초기화 되고 {0}점의 전투 포인트를 환원 받았습니다.", returnPoint);
				pm.SilverPoint[0] += returnPoint;

				pm.GoldAndSilverPointReturntoSkill = true;
			}
			//해부학 체크
			if( !pm.AnatomytoTasteID )
			{
				if( pm.Skills.Anatomy.Base > pm.Skills.TasteID.Base )
				{
					pm.SendMessage("해부학 스킬이 맛감정 스킬로 전이 되었습니다.");
					pm.Skills.TasteID.Base = pm.Skills.Anatomy.Base;
					pm.Skills.Anatomy.Base = 0;
				}
				pm.AnatomytoTasteID = true;
			}
			//던전 포인트 교환
			if( !pm.EquiptoDungeon )
			{
				int returnPoint = 0;
				for( int i = 0; i < pm.EquipPoint.Length; i++ )
				{
					returnPoint += pm.EquipPoint[i];
					pm.EquipPoint[i] = 0;
				}
				BankCheck bc = new BankCheck ( returnPoint );
				pm.AddToBackpack( bc );				
				pm.EquiptoDungeon = true;
			}
			//생산 & 전투 포인트 리셋. 0번 사용
			if( !pm.StatReset[0] )
			{
				int returnPoint = 0;
				for( int i = 1; i < pm.GoldPoint.Length; i++ )
				{
					while( pm.GoldPoint[i] > 0 )
					{
						returnPoint += Server.Misc.Levels.GoldExp(pm.GoldPoint[i] -1);
						pm.GoldPoint[i]--;
					}
				}
				pm.GoldPoint[0] += returnPoint;
				returnPoint = 0;
				for( int i = 1; i < pm.SilverPoint.Length; i++ )
				{
					while( pm.SilverPoint[i] > 0 )
					{
						returnPoint += Server.Misc.Levels.GoldExp(pm.SilverPoint[i] -1);
						pm.SilverPoint[i]--;
					}
				}
				pm.SilverPoint[0] += returnPoint;
				pm.StatReset[0] = true;
				pm.DeathCheck = 0;
			}
			//마법(지능 평가, 명상, 마법 저항, 주문 조합, 신비술, 강령술) 스킬 리셋. 1번 사용
			if( !pm.StatReset[1] )
			{
				pm.Skills.EvalInt.Base = 0;
				pm.Skills.Meditation.Base = 0;
				pm.Skills.MagicResist.Base = 0;
				pm.Skills.Spellweaving.Base = 0;
				pm.Skills.Mysticism.Base = 0;
				pm.Skills.Necromancy.Base = 0;
				pm.StatReset[1] = true;
			}
			//스킬 캡 200까지 한계 돌파 2번 사용
			if ( !pm.StatReset[2] )
			{
				for (var i = 0; i < Enum.GetNames(typeof(SkillName)).Length; ++i)
					pm.Skills[i].Cap = 200.0;
				
				pm.StatReset[2] = true;
			}
			//활동 포인트 변환 3번 사용
			if ( !pm.StatReset[3] )
			{
				if( pm.EquipPoint[0] > 0 )
					Banker.Deposit(pm, pm.EquipPoint[0] * 100, true);
				pm.EquipPoint[0] = 10;
				pm.StatReset[3] = true;
			}
			//생산 & 전투 포인트 리셋. 4번 사용
			if( !pm.StatReset[4] )
			{
				int returnPoint = 0;
				for( int i = 1; i < pm.GoldPoint.Length; i++ )
				{
					while( pm.GoldPoint[i] > 0 )
					{
						if( i == 1 )
							returnPoint += pm.GoldPoint[i] * 50;
						else if( i == 2 )
							returnPoint += pm.GoldPoint[i] * 250;
						else if( i == 3 || i == 4 )
							returnPoint += pm.GoldPoint[i] * pm.GoldPoint[i] * 10000;
						pm.GoldPoint[i]--;
					}
				}
				pm.GoldPoint[0] += returnPoint;
				returnPoint = 0;

				for( int i = 1; i < pm.SilverPoint.Length; i++ )
				{
					while( pm.SilverPoint[i] > 0 )
					{
						if( i >= 1 && i <= 7 )
							returnPoint += pm.SilverPoint[i] * 50;
						else if( i >= 8 && i <= 10 )
							returnPoint += pm.SilverPoint[i] * 5000;
						else if( i == 11 )
							returnPoint += pm.SilverPoint[i] * pm.SilverPoint[i] * 10000;
						else if( i <= 37 )
						{
							if( pm.SilverPoint[i] == 1 )
								returnPoint += 100;
							else if( pm.SilverPoint[i] == 2 )
								returnPoint += 150000;
							else if( pm.SilverPoint[i] == 3 )
								returnPoint += 500000;
							else if( pm.SilverPoint[i] == 4 )
								returnPoint += 1500000;
							else if( pm.SilverPoint[i] == 5 )
								returnPoint += 5000000;
						}
						pm.SilverPoint[i]--;
					}
				}
				pm.SilverPoint[0] += returnPoint;
				pm.StatReset[4] = true;
				pm.DeathCheck = 0;
			}
			//생산 포인트 => 채집, 제작 포인트로 변환
			if( !pm.StatReset[5] )
			{
				if( pm.GoldPoint[0] > 0 )
				{
					pm.GoldPoint[49] = pm.GoldPoint[0];
					for(int i = 0; i < 48; i++)
						pm.GoldPoint[i] = 0;
				}
				pm.StatReset[5] = true;
			}
			//모래 캐기 추가로 인한 저장 밀기
			if( !pm.StatReset[7] )
			{
				for(int i = 35; i >= 27; i--)
				{
					pm.HarvestPoint[i + 3] = pm.HarvestPoint[i];
					pm.HarvestPoint[i] = 0;
				}
				pm.HarvestPoint[29] = 0;
				for(int i = 26; i >= 18; i--)
				{
					pm.HarvestPoint[i + 2] = pm.HarvestPoint[i];
					pm.HarvestPoint[i] = 0;
				}
				pm.HarvestPoint[19] = 0;
				for(int i = 17; i >= 9; i--)
				{
					pm.HarvestPoint[i + 1] = pm.HarvestPoint[i];
					pm.HarvestPoint[i] = 0;
				}
				pm.HarvestPoint[9] = 0;
				
				pm.StatReset[7] = true;
			}
			if( !pm.StatReset[8] )
			{
				for(int i = 0; i >= pm.ArtifactPoint.Length; i--)
				{
					pm.ArtifactPoint[i] = 0;
				}
				pm.StatReset[8] = true;
			}
			//포인트 오류 수정
			if( !pm.StatReset[9] )
			{
				Account acc = pm.Account as Account;
				for(int i = 0; i < 4; ++i)
				{
					if( acc.Point[857 + i] != 0 )
					{
						acc.Point[862 + i] += acc.Point[857 + i];
						acc.Point[857 + i] = 0;
					}
				}
				pm.StatReset[9] = true;
			}
			if( !pm.StatReset[10] )
			{
				Account acc = pm.Account as Account;
				if( acc.TeachingBonus != 0 )
				{
					acc.TeachingBonus = 0;
				}

				pm.StatReset[10] = true;
			}
			
			//스킬 스텟 신규 갱신
			if( !pm.StatReset[11] )
			{
				for( int i = 0; i < m_StatUp.GetLength(1); i++)
				{
					pm.SkillbyStat[i] = 0;
					for( int j = 0; j < m_StatUp.GetLength(0); j++)
					{
						if( pm.SkillbyStat[i] < m_StatUp[j, i] * (int)( pm.Skills[j].Value * 10 ) )
							pm.SkillbyStat[i] = m_StatUp[j, i] * (int)( pm.Skills[j].Value * 10);
					}
				}
				pm.StatReset[11] = true;
			}
			
			//세트 아이템 체크
			Misc.Util.SetOption(pm);
			
			//펫 체크
			if( pm.Region is Server.Regions.TownRegion )
			{
				int count = 0;
				var list = new List<Mobile>();
				foreach ( Mobile m in World.Mobiles.Values )
				{
					if ( m is BaseCreature )
					{
						BaseCreature bc = m as BaseCreature;
						if( bc.ControlMaster == pm )
						{
							list.Add( m );
						}
					}
				}
				for ( int i = 0; i < list.Count; ++i )
				{
					Mobile tar = (Mobile)list[i];
					if( tar is BaseCreature )
					{
						BaseCreature bc = tar as BaseCreature;
						//pm.PetFollows += bc.ControlSlots;
						//pm.Followers += bc.ControlSlots;
						if ( bc is BaseMount )
						{
							BaseMount bm = bc as BaseMount;
							if( bm.Rider == null )
								bc.MoveToWorld( pm.Location, pm.Map);
						}
						else
							bc.MoveToWorld( pm.Location, pm.Map);
					}
				}
			}

			pm.SkillsTotal_Check = (int)e.Mobile.SkillsTotal;
			//pm.Followers += pm.EquipCheck();

			DisguiseTimers.StartTimer(e.Mobile);

			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(ClearSpecialMovesCallback), e.Mobile);

			//특수기 초기화
			WeaponAbility a = WeaponAbility.GetCurrentAbility(pm);
			if( a != null )
				WeaponAbility.ClearCurrentAbility(pm);
		}

		public int[] SilverPointScore = { 0, 1, 3, 6, 11, 18, 28, 43, 68, 108, 168, 268, 418, 668, 1068, 1668, 2668, 4168, 6668, 11668, 21668 };

		public int[] SilverPointRealScore = { 0, 1, 2, 3, 5, 7, 10, 15, 25, 40, 60, 100, 150, 250, 400, 600, 1000, 1500, 2500, 5000, 10000 };

		public bool realHidden = false;
		
		/*
		public int[] silverRankScore = { 0, 2, 123, 410, 1640, 6150, 16400, 41000, 102500 };
		
		public int SilverPointbyEquipSort()
		{
			if( m_SilverPointbyEquipCheck >= 102500 )
				return 8;
			else if( m_SilverPointbyEquipCheck >= 41000 )
				return 7;
			else if( m_SilverPointbyEquipCheck >= 16400 )
				return 6;
			else if( m_SilverPointbyEquipCheck >= 6150 )
				return 5;
			else if( m_SilverPointbyEquipCheck >= 1640 )
				return 4;
			else if( m_SilverPointbyEquipCheck >= 410 )
				return 3;
			else if( m_SilverPointbyEquipCheck >= 123 )
				return 2;
			else if( m_SilverPointbyEquipCheck >= 2 )
				return 1;
			else
				return 0;
		}
		*/
		public int[] silverRankScore = { 0, 2, 3075, 8200, 30750, 102500 };
		
		public int SilverPointbyEquipSort()
		{
			if( m_SilverPointbyEquipCheck >= silverRankScore[5] )
				return 8;
			else if( m_SilverPointbyEquipCheck >= silverRankScore[4] )
				return 7;
			else if( m_SilverPointbyEquipCheck >= silverRankScore[3] )
				return 6;
			else if( m_SilverPointbyEquipCheck >= silverRankScore[2] )
				return 5;
			else if( m_SilverPointbyEquipCheck >= silverRankScore[1] )
				return 4;
			else
				return 0;
		}
		
		public bool SkillFull = true;

		private void PvPMove()
		{
			Point3D loc = new Point3D( 3793, 2772, 6 );

			BaseCreature.TeleportPets( this, loc, Map.Trammel );
			MoveToWorld( loc, Map.Trammel );
		}
		public void PlayerMove()
		{
			PlayerMove(true);
		}
		public void PlayerMove(bool cityteleporter)
		{
			Point3D loc = new Point3D( 2499, 924, 0 );
			UnEquipCheck();
			//던전 안일 경우 스타룸으로 이동
			/*
			if( !( Location.X > 5125 && Location.Y > 1765 > && Location.X < 5165 && Location.Y < 1775 ) && DeathCheck > 0 && ( this.Region is DungeonRegion || this.Region.Name == "Ancient Lair" ) )
				loc = new Point3D( 5140, 1773, 0 );
			else
			{
				*/
				switch ( m_SaveTown )
				{
					case 1:
						loc = new Point3D( 1431, 1696, 0 );
						break;
					case 2:
						loc = new Point3D( 2715, 2108, 0 );
						break;
					case 3:
						loc = new Point3D( 2234, 1198, 0 );
						break;
					case 4:
						loc = new Point3D( 1383, 3815, 0 );
						break;
					case 5:
						loc = new Point3D( 3721, 2066, 12 );
						break;
					case 6:
						loc = new Point3D( 2465, 528, 15 );
						break;
					case 7:
						loc = new Point3D( 4442, 1122, 5 );
						break;
					case 8:
						loc = new Point3D( 3756, 1280, 5 );
						break;
					case 9:
						loc = new Point3D( 2990, 3413, 15 );
						break;
					case 10:
						loc = new Point3D( 610, 2195, 0 );
						break;
					case 11:
						loc = new Point3D( 1868, 2779, 0 );
						break;
					case 12:
						loc = new Point3D( 2887, 710, 0 );
						break;
					case 13:
						loc = new Point3D( 546, 992, 0 );
						break;
					case 14:
						loc = new Point3D( 3499, 2571, 14 );					
						break;
				//}
			}
			Map map = this.Map;
			if( map == Map.Felucca || m_SaveTown != 0 )
				map = Map.Trammel;

			//사망 시 해제 코드
			if( Paralyzed )
				Paralyzed = false;
			if( Frozen )
				Frozen = false;
			if( Poison != null )
				Poison = null;
			
			MoveToWorld( loc, Map.Trammel );
			if ( cityteleporter )
			BaseCreature.KillPets( this, loc, map );
		}

		public DateTime FireField = DateTime.Now;		

		
		//사망 이동 위치
		public int MacroCheck = 0;
		public void DeathMove()
		{
			if( Hunger < 50000 )
				Hunger -= 25000;
			else
				Hunger /= 2;
			if( Hunger < 0 )
				Hunger = 0;

			SavagePaintExpiration = TimeSpan.Zero;

			PolymorphSpell.StopTimer(this);
			IncognitoSpell.StopTimer(this);
			DisguiseTimers.RemoveTimer(this);

            WeakenSpell.RemoveEffects(this);
            ClumsySpell.RemoveEffects(this);
            FeeblemindSpell.RemoveEffects(this);
            CurseSpell.RemoveEffect(this);
            Spells.Second.ProtectionSpell.EndProtection(this);
			MeleeDamageAbsorb = 0;
			MagicDamageAbsorb = 0;
            EndAction(typeof(PolymorphSpell));
			EndAction(typeof(IncognitoSpell));
			Meditating = false;

			MeerMage.StopEffect(this, false);

            BaseEscort.DeleteEscort(this);

            if (Flying)
			{
				Flying = false;
				BuffInfo.RemoveBuff(this, BuffIcon.Fly);
			}
            
            if (m_BuffTable != null)
			{
				var list = new List<BuffInfo>();

				foreach (BuffInfo buff in m_BuffTable.Values)
				{
					if (!buff.RetainThroughDeath)
					{
						list.Add(buff);
					}
				}

				for (int i = 0; i < list.Count; i++)
				{
					RemoveBuff(list[i]);
				}
			}
			//if( DeathCheck > 0 )
			//	Freeze( TimeSpan.FromSeconds( 60.0 * DeathCheck ) );
			/*
			if( DeathCheck > 0 )
			{
				//DeathCheck--;
				//10초 뒤 회복
				m_MoongateTime = DateTime.Now + TimeSpan.FromSeconds(10.0);
				Hunger -= 5000;
				if( Hunger < 0 )
					Hunger = 0;
			}
			else
			{
				Young = true;
				//스킬 패널티
				Hunger = 0;
				Fame = 0;
				Freeze( TimeSpan.FromSeconds( 30.0 ) );
				/*
				int fanelty = 1000;
				//활동 포인트
				if( ActionPoint >= 100000 + GoldPoint[13] * 2500 )
					Freeze( TimeSpan.FromSeconds( fanelty ));
				else if( ActionPoint > 99000 )
				{
					fanelty = 100000 - ActionPoint;
					ActionPoint = 100000;
					Freeze( TimeSpan.FromSeconds( fanelty ));
				}
				else
					ActionPoint -= fanelty;
			}
			*/
			PlayerMove();
			/*
			}
			else
			{
				if( DeathCheck < 0 )
				{
					
					DeathCheck = 0;
				}
			}
			*/
		}
		
		public int PotionDefense = 0;
		public int PotionPower = 0;
		public bool MediCheck = false;
		private bool TiredDebuff = false;
		
		//유저 카운트
		public long castingdelay = Core.TickCount;
		
		private void BuffCount()
		{
			int tired_Grade = 0;

			if( Hunger >= 50000 )
			{
				BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.FishPie, 1046506, 1046507));
				BuffInfo.RemoveBuff(this, BuffIcon.UnknownTomato);
			}
			else if( Hunger >= 10000 )
			{
				BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.UnknownTomato, 1046500, 1046501));
				BuffInfo.RemoveBuff(this, BuffIcon.FishPie);
			}
			else
			{
				BuffInfo.RemoveBuff(this, BuffIcon.UnknownTomato);
				BuffInfo.RemoveBuff(this, BuffIcon.FishPie);
			}
			if( m_Tired <= -80000 )
				tired_Grade = 1;
			else if( m_Tired <= - 10000 )
				tired_Grade = 2;
			else if( m_Tired == 0 )
				tired_Grade = 0;
			/*
			else if( m_Tired >= 5000 && m_Tired < 10000 )
				tired_Grade = 3;
			else if( m_Tired >= 10000 && m_Tired < 20000 )
				tired_Grade = 4;
			else if( m_Tired >= 20000 )
				tired_Grade = 5;
			else
				tired_Grade = 0;
			*/
			switch(tired_Grade)
			{
				case 1:
				{
					BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.GiftOfRenewal, 1046489 + tired_Grade, 1046494 + tired_Grade));
					BuffInfo.RemoveBuff(this, BuffIcon.FistsOfFury);
					BuffInfo.RemoveBuff(this, BuffIcon.UnknownDebuff);
					BuffInfo.RemoveBuff(this, BuffIcon.FactionStatLoss);
					BuffInfo.RemoveBuff(this, BuffIcon.Sleep);
					break;
				}
				case 2:
				{
					BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.FistsOfFury, 1046489 + tired_Grade, 1046494 + tired_Grade));
					BuffInfo.RemoveBuff(this, BuffIcon.GiftOfRenewal);
					BuffInfo.RemoveBuff(this, BuffIcon.UnknownDebuff);
					BuffInfo.RemoveBuff(this, BuffIcon.FactionStatLoss);
					BuffInfo.RemoveBuff(this, BuffIcon.Sleep);
					break;
				}
				/*
				case 3:
				{
					BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.UnknownDebuff, 1046489 + tired_Grade, 1046494 + tired_Grade));
					BuffInfo.RemoveBuff(this, BuffIcon.GiftOfRenewal);
					BuffInfo.RemoveBuff(this, BuffIcon.FistsOfFury);
					BuffInfo.RemoveBuff(this, BuffIcon.FactionStatLoss);
					BuffInfo.RemoveBuff(this, BuffIcon.Sleep);
					break;
				}
				case 4:
				{
					BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.FactionStatLoss, 1046489 + tired_Grade, 1046494 + tired_Grade));
					BuffInfo.RemoveBuff(this, BuffIcon.GiftOfRenewal);
					BuffInfo.RemoveBuff(this, BuffIcon.FistsOfFury);
					BuffInfo.RemoveBuff(this, BuffIcon.UnknownDebuff);
					BuffInfo.RemoveBuff(this, BuffIcon.Sleep);
					break;
				}
				case 5:
				{
					BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.Sleep, 1046489 + tired_Grade, 1046494 + tired_Grade));
					BuffInfo.RemoveBuff(this, BuffIcon.GiftOfRenewal);
					BuffInfo.RemoveBuff(this, BuffIcon.FistsOfFury);
					BuffInfo.RemoveBuff(this, BuffIcon.UnknownDebuff);
					BuffInfo.RemoveBuff(this, BuffIcon.FactionStatLoss);
					break;
				}
				default:
				{
					BuffInfo.RemoveBuff(this, BuffIcon.GiftOfRenewal);
					BuffInfo.RemoveBuff(this, BuffIcon.FistsOfFury);
					BuffInfo.RemoveBuff(this, BuffIcon.UnknownDebuff);
					BuffInfo.RemoveBuff(this, BuffIcon.FactionStatLoss);
					BuffInfo.RemoveBuff(this, BuffIcon.Sleep);
					break;
				}
				*/
			}			
			Timer.DelayCall( TimeSpan.FromSeconds( 30.0 ), new TimerCallback( BuffCount ) );
		}
		
		private int sub_hits = 0;
		private int sub_stam = 0;
		private int sub_mana = 0;
		
		private void PlayerCount()
		{
			//0~63 : 마법 스킬트리
			//64 : 몬스터 전투 시간
			//65 : 유저 전투 시간
			//66 : 스테미나 회복 시간
			//67 : 코마 시간
			//68 : 배 이동 시간
			//69 : 방어 물약 시간
			//70 : 강화 물약 시간
			//71 : 일 대기 시간
			//72 : 침대 사용 시간
			//73 : 리젠 체크

			for( int i = 0; i < m_TimerList.Length; i++)
			{
				if( m_TimerList[i] >= 1 )
					m_TimerList[i] -= 1;
			}
			
			//메크로 체크
			MacroCheck--;
			if( MacroCheck < 0 )
				MacroCheck = 0;
			if( MacroCheck > 250 )
				DeathMove();
			/*
			if( m_TimerList[66] >= m_StamTimeUp)
			{
				int timediv = m_TimerList[66] % m_StamTimeUp;
				m_TimerList[66] -= m_StamTimeUp * timediv;
				Stam += timediv;
				if( StamMax == Stam )
					m_TimerList[66] = 0;
				else
				m_TimerList[66] += 1;
			}
			*/

			//던전 체크
			//m_Tired += Util.DungeonTried( Location.X, Location.Y );
			//if( Hunger <= 0 )
			//	m_Tired += Util.DungeonTried( Location.X, Location.Y );

			
			//집, 마을 체크
			BaseHouse house = BaseHouse.FindHouseAt(this);
			if( !IsStaff() && ( Poisoned || ( this.Hidden && !( house != null && house.IsOwner(this) ) ) ) )
			{
				m_TimerList[64] = 100;
				//m_TimerList[65] = 300;
			}

			if ( m_Coma )
			{
				this.Frozen = true;
				if ( m_TimerList[67] == 1 );
				{
					//m_ComaTime = 600;
					this.Blessed = false;
					m_Coma = false;
					this.Frozen = false;
					//m_TimerList[67] = 0;
					this.DeathMove();
				}
			}
			else
			{
				if( m_TimerList[73] == 0 )
				{
					int regen = 1 + AosAttributes.GetValue(this, AosAttribute.RegenHits) + SilverPoint[24] * 2;
					if( Hidden )
						regen = 0;
					
					sub_hits += regen;
					
					if( sub_hits >= 10 )
					{
						regen = sub_hits / 10;
						sub_hits -= regen * 10;
						Hits += regen;
					}
					
					regen = 1 + AosAttributes.GetValue(this, AosAttribute.RegenStam) + SilverPoint[25];
					if( Hidden )
						regen = 0;

					sub_stam += regen;

					if( sub_stam >= 10 )
					{
						regen = sub_stam / 10;
						sub_stam -= regen * 10;
						Stam += regen;
					}

					regen = 1 + AosAttributes.GetValue(this, AosAttribute.RegenMana) + SilverPoint[26];
					if( Hidden )
						regen = 0;

					sub_mana += regen;

					if( sub_mana >= 10 )
					{
						regen = sub_mana / 10;
						sub_mana -= regen * 10;
						Mana += regen;
					}

					int minusMana = 0;
					if( MeleeDamageAbsorb == 1 || MeleeDamageAbsorb == 2 )
						minusMana = 1;
					else if( MeleeDamageAbsorb == 3 )
						minusMana = 2;

					if( MagicDamageAbsorb == 1 )
						minusMana += 3;
					
					var list = new List<Mobile>();
					foreach ( Mobile m in World.Mobiles.Values )
					{
						if ( m is BaseCreature )
						{
							BaseCreature bc = m as BaseCreature;
							if( bc.SummonMaster == this )
							{
								list.Add( m );
							}
						}
					}
					if( list.Count > 0 )
						minusMana += list.Count;
					
					if( Mana < minusMana )
					{
						Mana = 0;
						MeleeDamageAbsorb = 0;
                        BuffInfo.RemoveBuff(this, BuffIcon.ReactiveArmor);
						BuffInfo.RemoveBuff(this, BuffIcon.Protection);
						BuffInfo.RemoveBuff(this, BuffIcon.ArchProtection);						
						BuffInfo.RemoveBuff(this, BuffIcon.MagicReflection);						
						SpellHelper.SummonCheckHigh( this );	
					}
					else
						Mana -= minusMana;
					
					m_TimerList[73] = 10;
				}
			}
			
			
			if( m_TimerList[69] == 1 )
			{
				this.SendMessage("방어 물약의 효력이 끝났습니다.");
				this.PotionDefense = 0;
			}
			
			if( m_TimerList[70] == 1 )
			{
				this.SendMessage("강화 물약의 효력이 끝났습니다.");
				this.PotionPower = 0;
			}
			
			if( m_TimerList[71] == 0 && Loop && LastTarget != null )
			{
				if( LastTarget is BaseTool )
				{
					BaseTool tool = LastTarget as BaseTool;
					if( tool != null && !tool.Deleted && tool.UsesRemaining > 0)
						CraftLoopCheck();
					else
						Loop = false;
				}
				else
					LastTarget.OnDoubleClick( this );
				if( LoopCount > 0 )
				{
					LoopCount--;
					if( LoopCount == 0 )
						Loop = false;
				}
			}

			if( m_ShipCheck != 0 && m_TimerList[68] <= 0 )
			{
				Point3D p = new Point3D( 3045, 829, -2 );
				m_SaveTown = 12;
				switch ( m_ShipCheck )
				{
					case 1:
					{
						p = new Point3D( 1451, 1766, -2 );
						m_SaveTown = 1;
						break;
					}
					case 2:
					{
						p = new Point3D( 1495, 1761, -2 );
						m_SaveTown = 1;
						break;
					}
					case 3:
					{
						p = new Point3D( 2751, 2156, -2 );
						m_SaveTown = 2;
						break;
					}
					case 4:
					{
						p = new Point3D( 1134, 3695, -2 );
						m_SaveTown = 4;
						break;
					}
					case 5:
					{
						p = new Point3D( 1521, 3990, -2 );
						m_SaveTown = 4;
						break;
					}
					case 6:
					{
						p = new Point3D( 1504, 3710, -2 );
						m_SaveTown = 4;
						break;
					}
					case 7:
					{
						p = new Point3D( 3709, 2297, -2 );
						m_SaveTown = 5;
						break;
					}
					case 8:
					{
						p = new Point3D( 4428, 1026, -2 );
						m_SaveTown = 7;
						break;
					}
					case 9:
					{
						p = new Point3D( 3522, 2593, 1 );
						m_SaveTown = 14;
						break;
					}
					case 10:
					{
						p = new Point3D( 2937, 3414, 1 );
						m_SaveTown = 9;
						break;
					}
					case 11:
					{
						p = new Point3D( 645, 2245, -2 );
						m_SaveTown = 10;
						break;
					}
					case 12:
					{
						p = new Point3D( 2086, 2856, -2 );
						m_SaveTown = 11;
						break;
					}
				}
				m_ShipCheck = 0;
				BaseCreature.TeleportPets(this, p, Map.Trammel, true);
				this.MoveToWorld( p, Map.Trammel );
			}

			//스킬 최대치
			//SkillsCap = 1000000;
			FollowersMax = 5; /* Fame / 1000 + */ //ArtifactPoint[12]; //+ GoldPoint[14] + SilverPoint[41];
			
			//스킬을 스텟으로 변환
			if( SkillsTotal_Check != SkillsTotal )
			{
				/*
				SkillsTotalbonus = SkillsTotal_Bonus();
				if( SkillsTotalbonus >= 70 )
					SkillsTotalbonus = 70;
				*/
				for( int i = 0; i < m_StatUp.GetLength(1); i++)
				{
					for( int j = 0; j < m_StatUp.GetLength(0); j++)
					{
						if( m_SkillbyStat[i] < m_StatUp[j, i] * (int)( Skills[j].Value * 10) )
						{
							m_SkillbyStat[i] = m_StatUp[j, i] * (int)( Skills[j].Value * 10);
						}
					}
				}	
				/*
				for( int i = 0; i < m_SkillbyStat.Length; i++)
				{
					if( m_SkillbyStat[i] < StatUpMax )
					{
						SkillFull = false;
						break;
					}
				}
				*/
				Delta(MobileDelta.Stat);
				ProcessDelta();
				if( Hits > HitsMax )
					Hits = HitsMax;
				if( Stam > StamMax )
					Stam = StamMax;
				if( Mana > ManaMax )
					Mana = ManaMax;
				SkillsTotal_Check = SkillsTotal;
			}
			
			//추종자 체크 코드
			//Followers = SkillFollows + PetFollows + EquipFollows;

			//가방 최대 수량 체크
            Container pack = Backpack;
			pack.MaxItems = 150 + Misc.Util.Level( GoldPoint[10] );
			//pm.Delta(MobileDelta.Stat);
			//pm.ProcessDelta();

			//음식 자동 먹기
			if( AutoFood != null && AutoFood is Food )
			{
				Food food = AutoFood as Food;
				if( food != null )
				{
					if( Hunger + food.FillFactor * 100 <= 100000 - FoodPercent * 100 )
					{
						bool notmagicalfood = true;
						if( food is BaseMagicalFood || !food.Stackable || food.Amount <= 1 )
							notmagicalfood = false;
						food.TryEat(this, notmagicalfood );
					}
				}
				else
					AutoFood = null;
			}
			
			Timer.DelayCall( TimeSpan.FromSeconds( 0.1 ), new TimerCallback( PlayerCount ) );
		}

		public CraftSystem m_CraftSystem = null;
		//public CraftItem m_CraftItem = null;
		
		public int FoodPercent = 250;
		public int LoopCount = 50000;
		
		public void CraftLoopCheck()
		{
			if( m_CraftSystem == null )
			{
				Loop = false;
			}
			else if( LastTarget is ITool )
			{
				CraftContext context = m_CraftSystem.GetContext(this);
				ITool m_Tool = LastTarget as ITool;
				CraftItem item = context.LastMade;

				if (item != null)
				{
					if (item.TryCraft != null)
					{
						item.TryCraft(this, item, m_Tool);
						return;
					}

					int num = m_CraftSystem.CanCraft(this, m_Tool, item.ItemType);

					if (num > 0)
					{
						SendGump(new CraftGump(this, m_CraftSystem, m_Tool, num));
					}
					else
					{
						Type type = null;

						context = m_CraftSystem.GetContext(this);

						if (context != null)
						{
							CraftSubResCol res = (item.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes);
							int resIndex = (item.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

							if (resIndex >= 0 && resIndex < res.Count)
								type = res.GetAt(resIndex).ItemType;
						}

						m_CraftSystem.CreateItem(this, item.ItemType, type, m_Tool, item);
					}					
				}
			}
		}
		
		//자동화 체크
		public bool Loop = false;
		public bool LoopCheck = false;
		private Item m_AutoFood;
		[CommandProperty( AccessLevel.GameMaster )]
		public Item AutoFood
		{
			get{ return m_AutoFood;}
			set{ m_AutoFood = value;}
		}
		
		public object m_LastObject;
		[CommandProperty( AccessLevel.GameMaster )]
		public object LastObject
		{
			get{ return m_LastObject;}
			set{ m_LastObject = value;}
		}

		private Item[] m_ItemSave = new Item[100];
		public Item[] ItemSave
		{
			get{ return m_ItemSave;}
			set{ m_ItemSave = value;}
		}

		private string[] m_MonsterSave = new string[100];
		public string[] MonsterSave
		{
			get{ return m_MonsterSave;}
			set{ m_MonsterSave = value;}
		}
		
		private Mobile[] m_PlayerSave = new Mobile[100];
		public Mobile[] PlayerSave
		{
			get{ return m_PlayerSave;}
			set{ m_PlayerSave = value;}
		}
		
		private Mobile[] m_PetSave = new Mobile[100];
		public Mobile[] PetSave
		{
			get{ return m_PetSave;}
			set{ m_PetSave = value;}
		}
		
		private Item m_LastTarget;
		[CommandProperty( AccessLevel.GameMaster )]
		public Item LastTarget
		{
			get{ return m_LastTarget;}
			set{ m_LastTarget = value;}
		}

		
		private int[] m_ItemSetOption = new int[100];
		public int[] ItemSetOption
		{
			get{ return m_ItemSetOption;}
			set{ m_ItemSetOption = value; InvalidateProperties();}
		}
		private int[] m_ItemSetValue = new int[100];
		public int[] ItemSetValue
		{
			get{ return m_ItemSetValue;}
			set{ m_ItemSetValue = value; InvalidateProperties();}
		}

		private int[] m_ItemSetSaveValue = new int[500];
		public int[] ItemSetSaveValue
		{
			get{ return m_ItemSetSaveValue;}
			set{ m_ItemSetSaveValue = value; InvalidateProperties();}
		}
		
		//전투 포인트 총합
		private int m_SilverPointbyEquipCheck;
		[CommandProperty( AccessLevel.GameMaster )]
		public int SilverPointbyEquipCheck
		{
			get{ return m_SilverPointbyEquipCheck;}
			set{ m_SilverPointbyEquipCheck = value;}
		}
		//전투 포인트 등급
		private int m_SilverPointbyEquipRank;
		[CommandProperty( AccessLevel.GameMaster )]
		public int SilverPointbyEquipRank
		{
			get{ return m_SilverPointbyEquipRank;}
			set{ m_SilverPointbyEquipRank = value;}
		}

		//스킬 응답 코드
		public override void SendSkillMessage()
		{
			if (this.NextActionMessage - Core.TickCount >= 0)
			{
				return;
			}

			//double ticks = .Parse(NextSkillTime.ToString());
			//ticks /= 100;
			//TimeSpan time = TimeSpan.FromMilliseconds(NextSkillTime);
			//DateTime startdate = new DateTime(NextSkillTime);//new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTime.Now);
			//startdate = startdate.AddMilliseconds(NextSkillTime).ToLocalTime();

			this.NextActionMessage = Core.TickCount + 125;
			
			string nexttime = "스킬의 재사용 시간이 " + Util.TickCal(NextSkillTime) + " 남았습니다";
			SendMessage(nexttime);

			//SendLocalizedMessage(500118); // You must wait a few moments to use another skill.
		}
		public int disarmcount = 0;
		
		public bool disarmcheck = false;
		private int SkillsTotal_Bonus()
		{
			return (int)Math.Sqrt( SkillsTotal / 10 );
			
		}
		private	int SkillsTotalbonus = 0;

		private int static_SkillFollwers = 0;
		private int savelight = 0;
		
		public void Login_Reward(Account acc)
		{
			if( Util.Equip_Login[ acc.LoginBonus ] > 0 )
			{
				//Banker.Deposit( this, Util.Equip_Login[ acc.LoginBonus ] );
				acc.Point[0] += Util.Equip_Login[ acc.LoginBonus ];
				SendMessage( "당신은 {0} 가문 포인트를 획득합니다.", Util.Equip_Login[ acc.LoginBonus ] );
			}
			acc.LoginBonus++;
		}
		
		private void ResetDay()
		{
			if( DateTime.Now >= m_DayTime )
			{
				m_DayTime = DateTime.Now.Date + TimeSpan.FromDays( 1 );
				Account acc = Account as Account;

				//계정 로그인 체크
				if( acc.Daychecktime <= DateTime.Now )
				{
					Login_Reward(acc);
					acc.Daychecktime = m_DayTime;
				}
				Young = false;

			}
			Timer.DelayCall( TimeSpan.FromSeconds( 5 ), new TimerCallback( ResetDay ) );
		}

		private void ResetWeek()
		{
			if( DateTime.Now >= m_WeekTime )
			{
				int weekcheck = Misc.Util.WeekCal();
				m_WeekTime = DateTime.Now.Date + TimeSpan.FromDays( weekcheck );
				
				//주별 리셋 변수
				//m_PlayerPoint = 10000;
				DeathCheck = 0;
				BODContext context = BulkOrderSystem.GetContext(this);
				//var context = BulkOrderSystem.GetContext(Player, false);
				context.Entries[BODType.Smith].CachedDeeds = 1;
				context.Entries[BODType.Tailor].CachedDeeds = 1;
				context.Entries[BODType.Alchemy].CachedDeeds = 1;
				context.Entries[BODType.Inscription].CachedDeeds = 1;
				context.Entries[BODType.Tinkering].CachedDeeds = 1;
				context.Entries[BODType.Cooking].CachedDeeds = 1;
				context.Entries[BODType.Fletching].CachedDeeds = 1;
				context.Entries[BODType.Carpentry].CachedDeeds = 1;
				
				m_FarmCheck = 0;

				//계정 로그인 체크
				Account acc = this.Account as Account;
				if( acc.Weekchecktime <= DateTime.Now && acc.Count >= 1 )
				{
					acc.LoginBonus = 0;
					acc.Weekchecktime = m_WeekTime;
					acc.Point[0] += 9 + acc.Count;
				}
				EquipPoint[0] = 10;
				acc.Point[0] += Fame / 500;
				Fame = 0;

				//m_ActionPoint = 0;
			}
			Timer.DelayCall( TimeSpan.FromSeconds( 5 ), new TimerCallback( ResetWeek ) );
		}
		
		public bool skillpage = false;
		private void ResetMonth()
		{
			if( DateTime.Now >= m_MonthTime )
			{
				m_MonthTime = DateTime.Now.Date + TimeSpan.FromDays( Misc.Util.MonthCal() );
			}
			Timer.DelayCall( TimeSpan.FromSeconds( 5 ), new TimerCallback( ResetMonth ) );
		}
		
		private static void ClearSpecialMovesCallback(object state)
		{
			Mobile from = (Mobile)state;

			SpecialMove.ClearAllMoves(from);
		}

        private static void EventSink_Disconnected(DisconnectedEventArgs e)
		{
			Mobile from = e.Mobile;
			DesignContext context = DesignContext.Find(from);

			
			if (context != null)
			{
				/* Client disconnected
				*  - Remove design context
				*  - Eject all from house
				*  - Restore relocated entities
				*/
				// Remove design context
				DesignContext.Remove(from);

				// Eject all from house
				from.RevealingAction();

				foreach (Item item in context.Foundation.GetItems())
				{
					item.Location = context.Foundation.BanLocation;
				}

				foreach (Mobile mobile in context.Foundation.GetMobiles())
				{
					mobile.Location = context.Foundation.BanLocation;
				}

				// Restore relocated entities
				context.Foundation.RestoreRelocatedEntities();
			}
			PlayerMobile pm = e.Mobile as PlayerMobile;
			BaseHouse house = BaseHouse.FindHouseAt(pm);
			if ( pm.Hidden && !(house != null && house.IsOwner(pm)) && !pm.IsStaff() )
			{
				pm.RevealingAction();
				pm.Hidden = false;
			}

			if (pm != null)
			{
				pm.m_GameTime += (DateTime.UtcNow - pm.m_SessionStart);

				if (pm.m_Quest != null)
				{
					pm.m_Quest.StopTimer();
				}

				#region Mondain's Legacy
				QuestHelper.StopTimer(pm);
				#endregion

				pm.PetFollows = 0;
				pm.EquipFollows = 0;
				pm.SkillFollows = 0;
				pm.m_SpeechLog = null;
				pm.LastOnline = DateTime.UtcNow;

				WeaponAbility a = WeaponAbility.GetCurrentAbility(pm);
				if( a != null )
					WeaponAbility.ClearCurrentAbility(pm);                //pm.AutoStablePets();
				
				//가문 스킬 체크
				Account acc = pm.Account as Account;

				if( acc != null && acc.Count > 1 )
				{
					double AccountSkillSum = 0.0;
					for ( int i = 0; i < 58; i++)
					{
						for (int j = 0; j < acc.Length; ++j)
						{
							Mobile check = acc[j];
							if (check != null && check != pm )
								AccountSkillSum += check.Skills[i].Base * 0.1;
						}
						SkillName skill = (SkillName)Enum.ToObject(typeof(SkillName), i);
						SkillMod sk = new DefaultSkillMod(skill, true, AccountSkillSum * -1);
						sk.ObeyCap = true;
						pm.AddSkillMod(sk);
						AccountSkillSum = 0.0;
					}
				}
			}

			DisguiseTimers.StopTimer(from);
		}

		public override void RevealingAction()
		{
			realHidden = false;
			if (m_DesignContext != null)
			{
				return;
			}

			InvisibilitySpell.RemoveTimer(this);

			base.RevealingAction();
		}

		public override void OnHiddenChanged()
		{
			base.OnHiddenChanged();

			RemoveBuff(BuffIcon.Invisibility);
			//Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

			if (!Hidden)
			{
				RemoveBuff(BuffIcon.HidingAndOrStealth);
			}
			else // if( !InvisibilitySpell.HasTimer( this ) )
			{
				BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655)); //Hidden/Stealthing & You Are Hidden
			}
		}

		public override void OnSubItemAdded(Item item)
		{
			if (AccessLevel < AccessLevel.GameMaster && item.IsChildOf(Backpack))
			{
				int curWeight = BodyWeight + TotalWeight;

                if (curWeight > MaxWeight)
				{
                    SendLocalizedMessage(1019035, true, String.Format(" : {0} / {1}", curWeight, MaxWeight));
				}
			}
		}

        public override void OnSubItemRemoved(Item item)
        {
            if (Server.Engines.UOStore.UltimaStore.HasPendingItem(this))
                Timer.DelayCall<PlayerMobile>(TimeSpan.FromSeconds(1.5), Server.Engines.UOStore.UltimaStore.CheckPendingItem, this);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            if (aggressor is BaseCreature && ((BaseCreature)aggressor).ControlMaster != null && ((BaseCreature)aggressor).ControlMaster != this)
            {
                Mobile aggressiveMaster = ((BaseCreature)aggressor).ControlMaster;

                if (NotorietyHandlers.CheckAggressor(Aggressors, aggressor))
                {
                    Aggressors.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));
                    aggressiveMaster.Delta(MobileDelta.Noto);

                    if (NotorietyHandlers.CheckAggressed(aggressor.Aggressed, this))
                        aggressiveMaster.Aggressed.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));

                    if (aggressiveMaster is PlayerMobile || (aggressiveMaster is BaseCreature && !((BaseCreature)aggressiveMaster).IsMonster))
                    {
                        BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, this, true));
                        BuffInfo.AddBuff(aggressiveMaster, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, aggressiveMaster, true));
                    }
                }
            }
        }

        public override void DoHarmful(IDamageable damageable, bool indirect)
        {
            base.DoHarmful(damageable, indirect);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == Faction.Facet) && damageable is Mobile)
            {
                ViceVsVirtueSystem.CheckHarmful(this, (Mobile)damageable);
            }
        }

        public override void DoBeneficial(Mobile target)
        {
            base.DoBeneficial(target);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == Faction.Facet) && target != null)
            {
                ViceVsVirtueSystem.CheckBeneficial(this, target);
            }
        }

		public override bool CanBeHarmful(IDamageable damageable, bool message, bool ignoreOurBlessedness)
		{
            Mobile target = damageable as Mobile;

			if (m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null))
			{
				return false;
			}

			#region Mondain's Legacy
			if (Peaced)
			{
				//!+ TODO: message
				return false;
			}
			#endregion

			if ((target is BaseVendor && ((BaseVendor)target).IsInvulnerable) || target is PlayerVendor || target is TownCrier)
			{
				if (message)
				{
					if (target.Title == null)
					{
						SendMessage("{0} the vendor cannot be harmed.", target.Name);
					}
					else
					{
						SendMessage("{0} {1} cannot be harmed.", target.Name, target.Title);
					}
				}

				return false;
			}

            if (damageable is IDamageableItem && !((IDamageableItem)damageable).CanDamage)
            {
                if (message)
                    SendMessage("That cannot be harmed.");

                return false;
            }

			return base.CanBeHarmful(damageable, message, ignoreOurBlessedness);
		}

		public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
		{
			if (m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null))
			{
				return false;
			}

			return base.CanBeBeneficial(target, message, allowDead);
		}

		public override bool CheckContextMenuDisplay(IEntity target)
		{
			return (m_DesignContext == null);
		}

		public override void OnItemAdded(Item item)
		{
			base.OnItemAdded(item);

			if (item is BaseArmor || item is BaseWeapon)
			{
				Hits = Hits;
				Stam = Stam;
				Mana = Mana;
			}

			if (NetState != null)
			{
				CheckLightLevels(false);
			}
		}

        private BaseWeapon m_LastWeapon;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseWeapon LastWeapon { get { return m_LastWeapon; } set { m_LastWeapon = value; } }

		private DateTime m_Another_Land;
        [CommandProperty(AccessLevel.GameMaster)]
		public DateTime Another_Land
		{
			get{ return m_Another_Land;}
			set{ m_Another_Land = value; InvalidateProperties();}
		}
		
		public override void OnItemRemoved(Item item)
		{
			base.OnItemRemoved(item);

			if (item is BaseArmor || item is BaseWeapon)
			{
				Hits = Hits;
				Stam = Stam;
				Mana = Mana;
			}

            if (item is BaseWeapon)
            {
                m_LastWeapon = item as BaseWeapon;
            }

			if (NetState != null)
			{
				CheckLightLevels(false);
			}
		}

		public override double ArmorRating
		{
			get
			{
				//BaseArmor ar;
				double rating = 0.0;

				AddArmorRating(ref rating, NeckArmor);
				AddArmorRating(ref rating, HandArmor);
				AddArmorRating(ref rating, HeadArmor);
				AddArmorRating(ref rating, ArmsArmor);
				AddArmorRating(ref rating, LegsArmor);
				AddArmorRating(ref rating, ChestArmor);
				AddArmorRating(ref rating, ShieldArmor);

				return VirtualArmor + VirtualArmorMod + rating;
			}
		}

		private void AddArmorRating(ref double rating, Item armor)
		{
			BaseArmor ar = armor as BaseArmor;

			if (ar != null && (!Core.AOS || ar.ArmorAttributes.MageArmor == 0))
			{
				rating += ar.ArmorRatingScaled;
			}
		}

		//스텟 설정
		#region [Stats]Max
		[CommandProperty(AccessLevel.GameMaster)]
		public override int HitsMax { get {	return Math.Min( AosAttributes.GetValue(this, AosAttribute.BonusHits) + SilverPoint[21] * 50 + SkillbyStat[4] / 1000 , 9999 ); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public override int StamMax { get { return Math.Min( AosAttributes.GetValue(this, AosAttribute.BonusStam) + SilverPoint[22] * 20 + SkillbyStat[5] / 1000, 9999 ); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public override int ManaMax { get { return Math.Min( AosAttributes.GetValue(this, AosAttribute.BonusMana) + SilverPoint[23] * 20 + SkillbyStat[6] / 1000, 9999 ); } }
		#endregion
		
		#region Stat Getters/Setters
		[CommandProperty(AccessLevel.GameMaster)]
		public override int Str
		{
			get
			{
				if (Core.ML && IsPlayer())
				{
					return Math.Min( 100 + SilverPoint[28] * 5 + AosAttributes.GetValue( this, AosAttribute.BonusStr) + SkillbyStat[0] / 1000, 9999 );
					//return Math.Min(base.Str, StrMaxCap);
				}

				return base.Str;
			}
			set { base.Str = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Dex
		{
			get
			{
				if (Core.ML && IsPlayer())
				{
					return Math.Min( 100 + SilverPoint[29] * 5 + AosAttributes.GetValue( this, AosAttribute.BonusDex) + SkillbyStat[1] / 1000, 9999 );
					//return Math.Min(base.Dex, DexMaxCap);
				}

				return base.Dex;
			}
			set { base.Dex = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Int
		{
			get
			{
				if (Core.ML && IsPlayer())
				{
					return Math.Min( 100 + SilverPoint[30] * 5 + AosAttributes.GetValue( this, AosAttribute.BonusInt) + SkillbyStat[2] / 1000, 9999 );
					//return Math.Min(base.Int, IntMaxCap);
				}

				return base.Int;
			}
			set { base.Int = value; }
		}

		#endregion

        public long NextPassiveDetectHidden { get; set; }

		public override bool Move(Direction d)
		{
			NetState ns = NetState;

			if (ns != null)
			{
				if (HasGump(typeof(ResurrectGump)))
				{
					if (Alive)
					{
						CloseGump(typeof(ResurrectGump));
					}
					else
					{
						SendLocalizedMessage(500111); // You are frozen and cannot move.
						return false;
					}
				}
			}

			int speed = ComputeMovementSpeed(d);

			bool res;

			if (!Alive)
			{
				MovementImpl.IgnoreMovableImpassables = true;
			}

			res = base.Move(d);

			MovementImpl.IgnoreMovableImpassables = false;

			if (!res)
			{
				return false;
			}

			m_NextMovementTime += speed;

            if (!Siege.SiegeShard && Core.TickCount - NextPassiveDetectHidden >= 0)
            {
                DetectHidden.DoPassiveDetect(this);
                NextPassiveDetectHidden = Core.TickCount + (int)TimeSpan.FromSeconds(2).TotalMilliseconds;
            }
			return true;
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			DesignContext context = m_DesignContext;

			if (context == null)
			{
                bool check = base.CheckMovement(d, out newZ);

                if (check && Sigil.ExistsOn(this, true) && !Server.Engines.VvV.VvVSigil.CheckMovement(this, d))
                {
                    SendLocalizedMessage(1155414); // You may not remove the sigil from the battle region!
                    return false;
                }

                return check;
			}

			HouseFoundation foundation = context.Foundation;

			newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

			int newX = X, newY = Y;
			Movement.Movement.Offset(d, ref newX, ref newY);

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			return (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map);
		}

        public override void OnHitsChange(int oldValue)
        {
            if (Race == Race.Gargoyle)
            {
                if (Hits <= HitsMax / 2)
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.Berserk, 1080449, 1115021, String.Format("{0}\t{1}", GetRacialBerserkBuff(false), GetRacialBerserkBuff(true)), false));
                    Delta(MobileDelta.WeaponDamage);
                }
                else if (oldValue < Hits && Hits > HitsMax / 2)
                {
                    BuffInfo.RemoveBuff(this, BuffIcon.Berserk);
                    Delta(MobileDelta.WeaponDamage);
                }
            }

            base.OnHitsChange(oldValue);
        }

        /// <summary>
        /// Returns Racial Berserk value, for spell or melee
        /// </summary>
        /// <param name="spell">true for spell damage, false for damage increase (melee)</param>
        /// <returns></returns>
        public virtual int GetRacialBerserkBuff(bool spell)
        {
            if (Race != Race.Gargoyle || Hits > HitsMax / 2)
                return 0;

            return 20;
        }

        public override void OnHeal(ref int amount, Mobile from)
        {
            base.OnHeal(ref amount, from);

            if (from == null)
                return;

            BestialSetHelper.OnHeal(this, from, ref amount);

            if (Core.SA && amount > 0 && from != null && from != this)
            {
                for (int i = Aggressed.Count - 1; i >= 0; i--)
                {
                    var info = Aggressed[i];

                    if (info.Defender.InRange(Location, Core.GlobalMaxUpdateRange) && info.Defender.DamageEntries.Any(de => de.Damager == this))
                    {
                        info.Defender.RegisterDamage(amount, from);
                    }

                    if (info.Defender.Player && from.CanBeHarmful(info.Defender, false))
                    {
                        from.DoHarmful(info.Defender, true);
                    }
                }

                for (int i = Aggressors.Count - 1; i >= 0; i--)
                {
                    var info = Aggressors[i];

                    if (info.Attacker.InRange(Location, Core.GlobalMaxUpdateRange) && info.Attacker.DamageEntries.Any(de => de.Damager == this))
                    {
                        info.Attacker.RegisterDamage(amount, from);
                    }

                    if (info.Attacker.Player && from.CanBeHarmful(info.Attacker, false))
                    {
                        from.DoHarmful(info.Attacker, true);
                    }
                }
            }
        }

		public override bool AllowItemUse(Item item)
		{
			return DesignContext.Check(this);
		}

		public SkillName[] AnimalFormRestrictedSkills { get { return m_AnimalFormRestrictedSkills; } }

		private readonly SkillName[] m_AnimalFormRestrictedSkills = new[]
		{
			SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics, SkillName.Inscribe,
			SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking, SkillName.Provocation, SkillName.RemoveTrap,
			SkillName.SpiritSpeak, SkillName.Stealing, SkillName.TasteID
		};

		public override bool AllowSkillUse(SkillName skill)
		{
			if (AnimalForm.UnderTransformation(this))
			{
				for (int i = 0; i < m_AnimalFormRestrictedSkills.Length; i++)
				{
					if (m_AnimalFormRestrictedSkills[i] == skill)
					{
						#region Mondain's Legacy
						AnimalFormContext context = AnimalForm.GetContext(this);

						if (skill == SkillName.Stealing && context.StealingMod != null && context.StealingMod.Value > 0)
						{
							continue;
						}
						#endregion

						SendLocalizedMessage(1070771); // You cannot use that skill in this form.
						return false;
					}
				}
			}

			return DesignContext.Check(this);
		}

		private bool m_LastProtectedMessage;
		private int m_NextProtectionCheck = 10;

		public virtual void RecheckTownProtection()
		{
			m_NextProtectionCheck = 10;

			GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));
			bool isProtected = (reg != null && !reg.IsDisabled());

			if (isProtected != m_LastProtectedMessage)
			{
				if (isProtected)
				{
					SendLocalizedMessage(500112); // You are now under the protection of the town guards.
				}
				else
				{
					SendLocalizedMessage(500113); // You have left the protection of the town guards.
				}

				m_LastProtectedMessage = isProtected;
			}
		}

		public override void MoveToWorld(Point3D loc, Map map)
		{
			base.MoveToWorld(loc, map);

			RecheckTownProtection();
		}

		public override void SetLocation(Point3D loc, bool isTeleport)
		{
			if (!isTeleport && IsPlayer() && !Flying)
			{
				// moving, not teleporting
				int zDrop = (Location.Z - loc.Z);

				if (zDrop > 20) // we fell more than one story
				{
					Hits -= ((zDrop / 20) * 10) - 5; // deal some damage; does not kill, disrupt, etc
                    SendMessage("Ouch!");
				}
			}

			base.SetLocation(loc, isTeleport);

			if (isTeleport || --m_NextProtectionCheck == 0)
			{
				RecheckTownProtection();
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
            //base.GetContextMenuEntries(from, list);

            list.Add(new PaperdollEntry(this));

            if (from == this)
			{
                if (Core.HS && Alive)
                {
                    list.Add(new SearchVendors(this));
                }

                BaseHouse house = BaseHouse.FindHouseAt(this);

                if (house != null)
                {
					if (house.IsCoOwner(this))
					{
						list.Add(new CallbackEntry(6205, ReleaseCoOwnership));
					}
				}

				/*
                if (Core.SA)
                {
                    list.Add(new TitlesMenuEntry(this));
				}

                if (Alive && Core.SA)
                {
                    list.Add(new Engines.Points.LoyaltyRating(this));
                }

                list.Add(new OpenBackpackEntry(this));

                if (Alive && InsuranceEnabled)
                {
                    if (Core.SA)
                    {
                        list.Add(new CallbackEntry(1114299, OpenItemInsuranceMenu));
                    }

                    list.Add(new CallbackEntry(6201, ToggleItemInsurance));

                    if (!Core.SA)
                    {
                        if (AutoRenewInsurance)
                        {
                            list.Add(new CallbackEntry(6202, CancelRenewInventoryInsurance));
                        }
                        else
                        {
                            list.Add(new CallbackEntry(6200, AutoRenewInventoryInsurance));
                        }
                    }
                }
                else if (Siege.SiegeShard)
                {
                    list.Add(new CallbackEntry(3006168, SiegeBlessItem));
                }
				*/
                if (Core.ML && Alive)
                {
                    QuestHelper.GetContextMenuEntries(list);

                    if (!Core.SA && m_RewardTitles.Count > 0)
                    {
                        list.Add(new CallbackEntry(6229, ShowChangeTitle));
                    }
                }
                if (Alive && Core.SA)
                {
					list.Add(new CallbackEntry( 1049593, CityPoint ));
					//list.Add(new CallbackEntry( 1152190, CityPoint ));
                    //list.Add(new Server.Engines.Points.LoyaltyRating(this));
                }
                list.Add(new OpenBackpackEntry(this));

                if (m_Quest != null)
                {
                    m_Quest.GetContextMenuEntries(list);
                }

				if (house != null)
                {
                    if (Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
                    {
                        list.Add(new CallbackEntry(6204, GetVendor));
                    }

                    if (house.IsAosRules)
                    {
                        list.Add(new CallbackEntry(6207, LeaveHouse));
                    }
                }

				if (Core.HS)
				{
					list.Add(new CallbackEntry(RefuseTrades ? 1154112 : 1154113, ToggleTrades)); // Allow Trades / Refuse Trades				
				}

				if (m_JusticeProtectors.Count > 0)
				{
					list.Add(new CallbackEntry(6157, CancelProtection));
				}

                #region Void Pool
                if (VoidPool || Region.IsPartOf<VoidPoolRegion>())
                {
                    var controller = Map == Map.Felucca ? VoidPoolController.InstanceFel : VoidPoolController.InstanceTram;

                    if (controller != null)
                    {
						if (!VoidPool)
						{
							VoidPool = true;
						}

						list.Add(new VoidPoolInfo(this, controller));
                    }
                }
                #endregion

                #region TOL Shadowguard
				if (ShadowguardController.GetInstance(Location, Map) != null)
				{
					list.Add(new ExitEntry(this));
				}
				#endregion

                if (Core.UOR && !Core.SA && Alive)
				{
					list.Add(new CallbackEntry(6210, ToggleChampionTitleDisplay));
				}

				if (DisabledPvpWarning)
				{
					list.Add(new CallbackEntry(1113797, EnablePvpWarning));
				}
			}
			else
			{
                if (Core.HS)
                {
                    BaseGalleon galleon = BaseGalleon.FindGalleonAt(from.Location, from.Map);

                    if (galleon != null && galleon.IsOwner(from))
                        list.Add(new ShipAccessEntry(this, from, galleon));
                }

                if (Alive && Core.AOS)
				{
					Party theirParty = from.Party as Party;
					Party ourParty = Party as Party;

					if (theirParty == null && ourParty == null)
					{
						list.Add(new AddToPartyEntry(from, this));
					}
					else if (theirParty != null && theirParty.Leader == from)
					{
						if (ourParty == null)
						{
							list.Add(new AddToPartyEntry(from, this));
						}
						else if (ourParty == theirParty)
						{
							list.Add(new RemoveFromPartyEntry(from, this));
						}
					}
				}
				/*
                if (Core.TOL && from.InRange(this, 10))
                {
                    list.Add(new CallbackEntry(1077728, () => OpenTrade(from))); // Trade
                }
				*/
				BaseHouse curhouse = BaseHouse.FindHouseAt(this);

				if (curhouse != null)
				{
					if (Alive && Core.AOS && curhouse.IsAosRules && curhouse.IsFriend(from))
					{
						list.Add(new EjectPlayerEntry(from, this));
					}
				}
			}
		}

		private void CancelProtection()
		{
			for (int i = 0; i < m_JusticeProtectors.Count; ++i)
			{
				Mobile prot = m_JusticeProtectors[i];

				string args = String.Format("{0}\t{1}", Name, prot.Name);

				prot.SendLocalizedMessage(1049371, args);
				// The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
				SendLocalizedMessage(1049371, args);
				// The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
			}

			m_JusticeProtectors.Clear();
		}

		#region Insurance
		private void ToggleItemInsurance()
		{
			if (!CheckAlive())
			{
				return;
			}

			BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
			SendLocalizedMessage(1060868); // Target the item you wish to toggle insurance status on <ESC> to cancel
		}

		private bool CanInsure(Item item)
		{
			#region Mondain's Legacy
			if (item is BaseQuiver && item.LootType == LootType.Regular)
			{
				return true;
			}
			#endregion

			if (((item is Container) && !(item is BaseQuiver)) || item is BagOfSending || item is KeyRing || item is MountItem)
			{
				return false;
			}

			if ((item is Spellbook && item.LootType == LootType.Blessed) || item is Runebook || item is PotionKeg ||
				item is Sigil)
			{
				return false;
			}

            if (item is BaseBalmOrLotion || item is GemOfSalvation || item is SeedOfLife || item is ManaDraught)
            {
                return false;
            }

			if (item.Stackable)
			{
				return false;
			}

			if (item.LootType == LootType.Cursed)
			{
				return false;
			}

			if (item.ItemID == 0x204E) // death shroud
			{
				return false;
			}

			if (item.LootType == LootType.Blessed)
				return false;

			return true;
		}

        private void ToggleItemInsurance_Callback(Mobile from, object obj)
        {
            if (!CheckAlive())
                return;

            ToggleItemInsurance_Callback(from, obj as Item, true);
        }

        private void ToggleItemInsurance_Callback(Mobile from, Item item, bool target)
        {
            if (item == null || !item.IsChildOf(this))
            {
                if (target)
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));

                SendLocalizedMessage(1060871, "", 0x23); // You can only insure items that you have equipped or that are in your backpack
            }
            else if (item.Insured)
            {
                item.Insured = false;

                SendLocalizedMessage(1060874, "", 0x35); // You cancel the insurance on the item

                if (target)
                {
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                    SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
                }
            }
            else if (!CanInsure(item))
            {
                if (target)
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));

                SendLocalizedMessage(1060869, "", 0x23); // You cannot insure that
            }
            else
            {
                if (!item.PayedInsurance)
                {
                    int cost = GetInsuranceCost(item);

                    if (Banker.Withdraw(from, cost))
                    {
                        SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                        item.PayedInsurance = true;
                    }
                    else
                    {
                        SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        return;
                    }
                }

                item.Insured = true;

                SendLocalizedMessage(1060873, "", 0x23); // You have insured the item

                if (target)
                {
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                    SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
                }
            }
        }

        public int GetInsuranceCost(Item item)
        {
            var imbueWeight = Imbuing.GetTotalWeight(item, -1, false, false);
            int cost = 600; // this handles old items, set items, etc

            if (item.GetType().IsAssignableFrom(typeof(Factions.FactionItem)))
                cost = 800;
            else if (imbueWeight > 0)
                cost = Math.Min(800, Math.Max(10, imbueWeight));
            else if (Mobiles.GenericBuyInfo.BuyPrices.ContainsKey(item.GetType()))
                cost = Math.Min(800, Math.Max(10, Mobiles.GenericBuyInfo.BuyPrices[item.GetType()]));
            else if (item.LootType == LootType.Newbied)
                cost = 10;

            var negAttrs = RunicReforging.GetNegativeAttributes(item);

            if (negAttrs != null && negAttrs.Prized > 0)
                cost *= 2;

            if (Region != null)
                cost = (int)(cost * Region.InsuranceMultiplier);

            return cost;
        }

        private void AutoRenewInventoryInsurance()
		{
			if (!CheckAlive())
			{
				return;
			}

			SendLocalizedMessage(1060881, "", 0x23); // You have selected to automatically reinsure all insured items upon death
			AutoRenewInsurance = true;
		}

		private void CancelRenewInventoryInsurance()
		{
			if (!CheckAlive())
			{
				return;
			}

			if (Core.SE)
			{
				if (!HasGump(typeof(CancelRenewInventoryInsuranceGump)))
				{
					SendGump(new CancelRenewInventoryInsuranceGump(this, null));
				}
			}
			else
			{
				SendLocalizedMessage(1061075, "", 0x23); // You have cancelled automatically reinsuring all insured items upon death
				AutoRenewInsurance = false;
			}
		}

        #region Siege Bless Item
        private Item _BlessedItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public Item BlessedItem { get { return _BlessedItem; } set { _BlessedItem = value; } }

        private void SiegeBlessItem()
        {
            if (_BlessedItem != null && _BlessedItem.Deleted)
                _BlessedItem = null;

            BeginTarget(2, false, TargetFlags.None, (from, targeted) =>
            {
                Siege.TryBlessItem(this, targeted);
            });
        }

        public override bool Drop(Point3D loc)
        {
            if (!Siege.SiegeShard || _BlessedItem == null)
                return base.Drop(loc);

            Item item = Holding;
            bool drop = base.Drop(loc);

            if (item != null && drop && item.Parent == null && _BlessedItem != null && _BlessedItem == item)
            {
                _BlessedItem = null;
                item.LootType = LootType.Regular;

                SendLocalizedMessage(1075292, item.Name != null ? item.Name : "#" + item.LabelNumber.ToString()); // ~1_NAME~ has been unblessed.
            }

            return drop;
        }
        #endregion

        private class CancelRenewInventoryInsuranceGump : Gump
		{
			private readonly PlayerMobile m_Player;
            private readonly ItemInsuranceMenuGump m_InsuranceGump;

			public CancelRenewInventoryInsuranceGump(PlayerMobile player, ItemInsuranceMenuGump insuranceGump)
				: base(250, 200)
			{
				m_Player = player;
                m_InsuranceGump = insuranceGump;

                AddBackground(0, 0, 240, 142, 0x13BE);
				AddImageTiled(6, 6, 228, 100, 0xA40);
				AddImageTiled(6, 116, 228, 20, 0xA40);
				AddAlphaRegion(6, 6, 228, 142);

				AddHtmlLocalized(8, 8, 228, 100, 1071021, 0x7FFF, false, false);
				// You are about to disable inventory insurance auto-renewal.

				AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

				AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(148, 118, 450, 20, 1071022, 0x7FFF, false, false); // DISABLE IT!
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (!m_Player.CheckAlive())
				{
					return;
				}

				if (info.ButtonID == 1)
				{
					m_Player.SendLocalizedMessage(1061075, "", 0x23);
					// You have cancelled automatically reinsuring all insured items upon death
					m_Player.AutoRenewInsurance = false;
				}
				else
				{
					m_Player.SendLocalizedMessage(1042021); // Cancelled.
				}

                if (m_InsuranceGump != null)
                    m_Player.SendGump(m_InsuranceGump.NewInstance());
            }
		}

        private void OpenItemInsuranceMenu()
        {
            if (!CheckAlive())
                return;

            List<Item> items = new List<Item>();

            foreach (Item item in Items)
            {
                if (DisplayInItemInsuranceGump(item))
                    items.Add(item);
            }

            Container pack = Backpack;

            if (pack != null)
                items.AddRange(pack.FindItemsByType<Item>(true, DisplayInItemInsuranceGump));

            // TODO: Investigate item sorting

            CloseGump(typeof(ItemInsuranceMenuGump));

            if (items.Count == 0)
                SendLocalizedMessage(1114915, "", 0x35); // None of your current items meet the requirements for insurance.
            else
                SendGump(new ItemInsuranceMenuGump(this, items.ToArray()));
        }

        private bool DisplayInItemInsuranceGump(Item item)
        {
            if (item.Parent is LockableContainer && ((LockableContainer)item.Parent).Locked)
                return false;

            return ((item.Visible || AccessLevel >= AccessLevel.GameMaster) && (item.Insured || CanInsure(item)));
        }

		public int SkillGumpPage = 0;
		public int MonsterFeatGumpPage = 0;
		public int HarvestGumpPage = 0;
		public int CraftGumpPage = 0;
		
        private class ItemInsuranceMenuGump : Gump
        {
            private PlayerMobile m_From;
            private Item[] m_Items;
            private bool[] m_Insure;
            private int m_Page;

            public ItemInsuranceMenuGump(PlayerMobile from, Item[] items)
                : this(from, items, null, 0)
            {
            }

            public ItemInsuranceMenuGump(PlayerMobile from, Item[] items, bool[] insure, int page)
                : base(25, 50)
            {
                m_From = from;
                m_Items = items;

                if (insure == null)
                {
                    insure = new bool[items.Length];

                    for (int i = 0; i < items.Length; ++i)
                        insure[i] = items[i].Insured;
                }

                m_Insure = insure;
                m_Page = page;

                AddPage(0);

                AddBackground(0, 0, 520, 510, 0x13BE);
                AddImageTiled(10, 10, 500, 30, 0xA40);
                AddImageTiled(10, 50, 500, 355, 0xA40);
                AddImageTiled(10, 415, 500, 80, 0xA40);
                AddAlphaRegion(10, 10, 500, 485);

                AddButton(15, 470, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, 472, 80, 20, 1011012, 0x7FFF, false, false); // CANCEL

                if (from.AutoRenewInsurance)
                    AddButton(360, 10, 9723, 9724, 1, GumpButtonType.Reply, 0);
                else
                    AddButton(360, 10, 9720, 9722, 1, GumpButtonType.Reply, 0);

                AddHtmlLocalized(395, 14, 105, 20, 1114122, 0x7FFF, false, false); // AUTO REINSURE

                AddButton(395, 470, 0xFA5, 0xFA6, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(430, 472, 50, 20, 1006044, 0x7FFF, false, false); // OK

                AddHtmlLocalized(10, 14, 150, 20, 1114121, 0x7FFF, false, false); // <CENTER>ITEM INSURANCE MENU</CENTER>

                AddHtmlLocalized(45, 54, 70, 20, 1062214, 0x7FFF, false, false); // Item
                AddHtmlLocalized(250, 54, 70, 20, 1061038, 0x7FFF, false, false); // Cost
                AddHtmlLocalized(400, 54, 70, 20, 1114311, 0x7FFF, false, false); // Insured

                int balance = Banker.GetBalance(from);
                int cost = 0;

                for (int i = 0; i < items.Length; ++i)
                {
                    if (insure[i])
                        cost += m_From.GetInsuranceCost(items[i]);
                }

                AddHtmlLocalized(15, 420, 300, 20, 1114310, 0x7FFF, false, false); // GOLD AVAILABLE:
                AddLabel(215, 420, 0x481, balance.ToString());
                AddHtmlLocalized(15, 435, 300, 20, 1114123, 0x7FFF, false, false); // TOTAL COST OF INSURANCE:
                AddLabel(215, 435, 0x481, cost.ToString());

                if (cost != 0)
                {
                    AddHtmlLocalized(15, 450, 300, 20, 1114125, 0x7FFF, false, false); // NUMBER OF DEATHS PAYABLE:
                    AddLabel(215, 450, 0x481, (balance / cost).ToString());
                }

                for (int i = page * 4, y = 72; i < (page + 1) * 4 && i < items.Length; ++i, y += 75)
                {
                    Item item = items[i];
                    Rectangle2D b = ItemBounds.Table[item.ItemID];

                    AddImageTiledButton(40, y, 0x918, 0x918, 0, GumpButtonType.Page, 0, item.ItemID, item.Hue, 40 - b.Width / 2 - b.X, 30 - b.Height / 2 - b.Y);
                    AddItemProperty(item.Serial);

                    if (insure[i])
                    {
                        AddButton(400, y, 9723, 9724, 100 + i, GumpButtonType.Reply, 0);
                        AddLabel(250, y, 0x481, m_From.GetInsuranceCost(item).ToString());
                    }
                    else
                    {
                        AddButton(400, y, 9720, 9722, 100 + i, GumpButtonType.Reply, 0);
                        AddLabel(250, y, 0x66C, m_From.GetInsuranceCost(item).ToString());
                    }
                }

                if (page >= 1)
                {
                    AddButton(15, 380, 0xFAE, 0xFAF, 3, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(50, 380, 450, 20, 1044044, 0x7FFF, false, false); // PREV PAGE
                }

                if ((page + 1) * 4 < items.Length)
                {
                    AddButton(400, 380, 0xFA5, 0xFA7, 4, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(435, 380, 70, 20, 1044045, 0x7FFF, false, false); // NEXT PAGE
                }
            }

            public ItemInsuranceMenuGump NewInstance()
            {
                return new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page);
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (info.ButtonID == 0 || !m_From.CheckAlive())
                    return;

                switch (info.ButtonID)
                {
                    case 1: // Auto Reinsure
                        {
                            if (m_From.AutoRenewInsurance)
                            {
                                if (!m_From.HasGump(typeof(CancelRenewInventoryInsuranceGump)))
                                    m_From.SendGump(new CancelRenewInventoryInsuranceGump(m_From, this));
                            }
                            else
                            {
                                m_From.AutoRenewInventoryInsurance();
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
                            }

                            break;
                        }
                    case 2: // OK
                        {
                            m_From.SendGump(new ItemInsuranceMenuConfirmGump(m_From, m_Items, m_Insure, m_Page));

                            break;
                        }
                    case 3: // Prev
                        {
                            if (m_Page >= 1)
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page - 1));

                            break;
                        }
                    case 4: // Next
                        {
                            if ((m_Page + 1) * 4 < m_Items.Length)
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page + 1));

                            break;
                        }
                    default:
                        {
                            int idx = info.ButtonID - 100;

                            if (idx >= 0 && idx < m_Items.Length)
                                m_Insure[idx] = !m_Insure[idx];

                            m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));

                            break;
                        }
                }
            }
        }

        private class ItemInsuranceMenuConfirmGump : Gump
        {
            private PlayerMobile m_From;
            private Item[] m_Items;
            private bool[] m_Insure;
            private int m_Page;

            public ItemInsuranceMenuConfirmGump(PlayerMobile from, Item[] items, bool[] insure, int page)
                : base(250, 200)
            {
                m_From = from;
                m_Items = items;
                m_Insure = insure;
                m_Page = page;

                AddBackground(0, 0, 240, 142, 0x13BE);
                AddImageTiled(6, 6, 228, 100, 0xA40);
                AddImageTiled(6, 116, 228, 20, 0xA40);
                AddAlphaRegion(6, 6, 228, 142);

                AddHtmlLocalized(8, 8, 228, 100, 1114300, 0x7FFF, false, false); // Do you wish to insure all newly selected items?

                AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

                AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(148, 118, 450, 20, 1073996, 0x7FFF, false, false); // ACCEPT
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (!m_From.CheckAlive())
                    return;

                if (info.ButtonID == 1)
                {
                    for (int i = 0; i < m_Items.Length; ++i)
                    {
                        Item item = m_Items[i];

                        if (item.Insured != m_Insure[i])
                            m_From.ToggleItemInsurance_Callback(m_From, item, false);
                    }
                }
                else
                {
                    m_From.SendLocalizedMessage(1042021); // Cancelled.
                    m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
                }
            }
        }

        #endregion
		
        private void ToggleTrades()
        {
            RefuseTrades = !RefuseTrades;
        }

        private void GetVendor()
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (CheckAlive() && house != null && house.IsOwner(this) && house.InternalizedVendors.Count > 0)
			{
				CloseGump(typeof(ReclaimVendorGump));
				SendGump(new ReclaimVendorGump(house));
			}
		}

        private void LeaveHouse()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null)
            {
                Location = house.BanLocation;
            }
        }

        private void ReleaseCoOwnership()
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsCoOwner(this))
			{
                SendGump(new WarningGump(1060635, 30720, 1062006, 32512, 420, 280, new WarningGumpCallback(ClearCoOwners_Callback), house));
            }
		}

        public void ClearCoOwners_Callback(Mobile from, bool okay, object state)
        {
            BaseHouse house = (BaseHouse)state;

            if (house.Deleted)
                return;

            if (okay && house.IsCoOwner(from))
            {
                if (house.CoOwners != null)
                    house.CoOwners.Remove(from);

                from.SendLocalizedMessage(501300); // You have been removed as a house co-owner.
            }
        }

        private void EnablePvpWarning()
        {
            DisabledPvpWarning = false;
            SendLocalizedMessage(1113798); // Your PvP warning query has been re-enabled.
        }
		public string CityRank( int rank )
		{

			switch ( rank )
			{
				case 1 : return "평민";
				case 2 : return "기사";
				case 3 : return "준훈작";
				case 4 : return "훈작";
				case 5 : return "준남작";
				case 6 : return "남작";
				case 7 : return "자작";
				case 8 : return "백작";
				case 9 : return "후작";
				default : return "작위 없음";
			}
		}
		
		private void CityPoint()
		{
            CloseGump(typeof(CityPointGump));
            SendGump(new CityPointGump(this));
		}
        private delegate void ContextCallback();

		private class CallbackEntry : ContextMenuEntry
		{
			private readonly ContextCallback m_Callback;

			public CallbackEntry(int number, ContextCallback callback)
				: this(number, -1, callback)
			{ }

			public CallbackEntry(int number, int range, ContextCallback callback)
				: base(number, range)
			{
				m_Callback = callback;
			}

			public override void OnClick()
			{
				if (m_Callback != null)
				{
					m_Callback();
				}
			}
		}

		public override void OnAosSingleClick(Mobile from)
		{
			if (ViewOPL)
			{
				base.OnAosSingleClick(from);
			}
			else if (from.Region.OnSingleClick(from, this))
			{
				OnSingleClick(from);
			}
		}

		public override void DisruptiveAction()
		{
			if (Meditating)
			{
				RemoveBuff(BuffIcon.ActiveMeditation);
			}

			base.DisruptiveAction();
		}

        public override bool Meditating
        {
            set
            {
                base.Meditating = value;
                if (value == false)
                {
                    RemoveBuff(BuffIcon.ActiveMeditation);
                }
            }
        }

		public override void OnDoubleClick(Mobile from)
		{
			if (this == from && !Warmode)
			{
				IMount mount = Mount;

				if (mount != null && !DesignContext.Check(this))
				{
					return;
				}
			}

			base.OnDoubleClick(from);
		}

		public override void DisplayPaperdollTo(Mobile to)
		{
			if (DesignContext.Check(this))
			{
				base.DisplayPaperdollTo(to);
			}
		}

		private static bool m_NoRecursion;

		public override bool CheckEquip(Item item)
		{
			if (!base.CheckEquip(item))
			{
				return false;
			}

            Region r = Region.Find(Location, Map);

            if (r is Server.Engines.ArenaSystem.ArenaRegion)
            {
                if (!((Server.Engines.ArenaSystem.ArenaRegion)r).AllowItemEquip(this, item))
                {
                    return false;
                }
            }

			#region Factions
			FactionItem factionItem = FactionItem.Find(item);

			if (factionItem != null)
			{
                PlayerState state = PlayerState.Find(this);
                Faction faction = null;

                if (state != null)
                {
                    faction = state.Faction;
                }

				if (faction == null)
				{
					SendLocalizedMessage(1010371); // You cannot equip a faction item!
					return false;
				}
				else if (faction != factionItem.Faction)
				{
					SendLocalizedMessage(1010372); // You cannot equip an opposing faction's item!
					return false;
				}
                else if (state != null && state.Rank.Rank < factionItem.MinRank)
                {
                    SendLocalizedMessage(1094804); // You are not high enough in rank to equip this item.
                    return false;
                }
                else
                {
                    int maxWearables = FactionItem.GetMaxWearables(this);

                    for (int i = 0; i < Items.Count; ++i)
                    {
                        Item equiped = Items[i];

                        if (item != equiped && FactionItem.Find(equiped) != null)
                        {
                            if (--maxWearables == 0)
                            {
                                SendLocalizedMessage(1010373); // You do not have enough rank to equip more faction items!
                                return false;
                            }
                        }
                    }
                }
			}
			#endregion

            #region Vice Vs Virtue
            IVvVItem vvvItem = item as IVvVItem;

            if (vvvItem != null && vvvItem.IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(this))
            {
                return false;
            }
            #endregion

			if (AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && HasTrade)
			{
				BounceInfo bounce = item.GetBounce();

				if (bounce != null)
				{
					if (bounce.m_Parent is Item)
					{
						Item parent = (Item)bounce.m_Parent;

						if (parent == Backpack || parent.IsChildOf(Backpack))
						{
							return true;
						}
					}
					else if (bounce.m_Parent == this)
					{
						return true;
					}
				}

				SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
				return false;
			}

			return true;
		}

        public override bool OnDragLift(Item item)
        {
            if (item is IPromotionalToken && ((IPromotionalToken)item).GumpType != null)
            {
                Type t = ((IPromotionalToken)item).GumpType;

                if (HasGump(t))
                    CloseGump(t);
            }

            return base.OnDragLift(item);
        }

		public override bool CheckTrade(
			Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			int msgNum = 0;

			if (_BlessedItem != null && _BlessedItem == item)
			{
				msgNum = 1075282; // You cannot trade a blessed item.
			}

			if (msgNum == 0 && cont == null)
			{
				if (to.Holding != null)
				{
					msgNum = 1062727; // You cannot trade with someone who is dragging something.
				}
				else if (HasTrade)
				{
					msgNum = 1062781; // You are already trading with someone else!
				}
				else if (to.HasTrade)
				{
					msgNum = 1062779; // That person is already involved in a trade
				}
                else if (to is PlayerMobile && ((PlayerMobile)to).RefuseTrades)
                {
                    msgNum = 1154111; // ~1_NAME~ is refusing all trades.
                }
                else if (item is IFactionItem && ((IFactionItem)item).FactionItemState != null)
                {
                    msgNum = 1094803; // This faction reward is bound to you, and cannot be traded.
                }
			}

			if (msgNum == 0 && item != null)
			{
				if (cont != null)
				{
					plusItems += cont.TotalItems;
					plusWeight += cont.TotalWeight;
				}

				if (Backpack == null || !Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
				{
					msgNum = 1004040; // You would not be able to hold this if the trade failed.
				}
				else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
				{
					msgNum = 1004039; // The recipient of this trade would not be able to carry 
				}
				else
				{
					msgNum = CheckContentForTrade(item);
				}
			}

			if (msgNum == 0)
			{
				return true;
			}

			if (!message)
			{
				return false;
			}

			if (msgNum == 1154111)
			{
                if (to != null)
                {
                    SendLocalizedMessage(msgNum, to.Name);
                }
			}
			else
			{
				SendLocalizedMessage(msgNum);
			}

			return false;
		}

		private static int CheckContentForTrade(Item item)
		{
			if (item is TrapableContainer && ((TrapableContainer)item).TrapType != TrapType.None)
			{
				return 1004044; // You may not trade trapped items.
			}

			if (StolenItem.IsStolen(item))
			{
				return 1004043; // You may not trade recently stolen items.
			}

			if (item is Container)
			{
				foreach (Item subItem in item.Items)
				{
					int msg = CheckContentForTrade(subItem);

					if (msg != 0)
					{
						return msg;
					}
				}
			}

			return 0;
		}

		public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			if (!base.CheckNonlocalDrop(from, item, target))
			{
				return false;
			}

			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			Container pack = Backpack;
			if (from == this && HasTrade && (target == pack || target.IsChildOf(pack)))
			{
				BounceInfo bounce = item.GetBounce();

				if (bounce != null && bounce.m_Parent is Item)
				{
					Item parent = (Item)bounce.m_Parent;

					if (parent == pack || parent.IsChildOf(pack))
					{
						return true;
					}
				}

				SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
				return false;
			}

			return true;
		}

		protected override void OnLocationChange(Point3D oldLocation)
		{
			CheckLightLevels(false);

			DesignContext context = m_DesignContext;

			if (context == null || m_NoRecursion)
			{
				return;
			}

			m_NoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			int newX = X, newY = Y;
			int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			if (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map)
			{
				if (Z != newZ)
				{
					Location = new Point3D(X, Y, newZ);
				}

				m_NoRecursion = false;
				return;
			}

			Location = new Point3D(foundation.X, foundation.Y, newZ);
			Map = foundation.Map;

			m_NoRecursion = false;
		}

		public override bool OnMoveOver(Mobile m) //m이 이동 시도, 없는게 기다리는 케릭
		{
			if (!Alive || !m.Alive || IsDeadBondedPet || m.IsDeadBondedPet || (Hidden && IsStaff()) || (m.Hidden && m.IsStaff()) )
				return true;
			
			//DungeonRegion bcdungeon = (DungeonRegion)m.Region.GetRegion(typeof(DungeonRegion));
			if( m is BaseCreature )
			{
				BaseCreature bc = m as BaseCreature;
				if( bc.IsMonster )
					return true;
			}

			DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));
			if( dungeon == null )
				return true;
			
			return false;
			
			/*
			if( m is PlayerMobile && bcdungeon != null )
				return false;
			if (m is BaseCreature )
			{
				BaseCreature bc = m as BaseCreature;
				if( bc.IsMonster )
					return true;
				else if( bcdungeon != null )
					return false;
			}
			if( dungeon != null )
				return false;

			return base.OnMoveOver(m);
			*/
		}

		public override bool CheckShove(Mobile shoved)
		{
			//투과
			if( IsStaff() )
				return true;
			
			DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));			

			if (!shoved.Alive || !Alive || shoved.IsDeadBondedPet || IsDeadBondedPet)
			{
				return true;
			}
			else if (shoved.Hidden && shoved.IsStaff())
			{
				return true;
			}
			if( shoved is BaseCreature && dungeon != null )
				return false;
			else if( shoved is PlayerMobile )
				return true;
			if (TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
			{
				return true;
			}

			return base.CheckShove(shoved);
		}

		protected override void OnMapChange(Map oldMap)
		{
            ViceVsVirtueSystem.OnMapChange(this);

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.OnMapChange(this, oldMap);
            }

			if ((Map != Faction.Facet && oldMap == Faction.Facet) || (Map == Faction.Facet && oldMap != Faction.Facet))
			{
				InvalidateProperties();
			}

            BaseGump.CheckCloseGumps(this);
            
			DesignContext context = m_DesignContext;

			if (context == null || m_NoRecursion)
			{
				return;
			}

			m_NoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			if (Map != foundation.Map)
			{
				Map = foundation.Map;
			}

			m_NoRecursion = false;
		}

		public override void OnBeneficialAction(Mobile target, bool isCriminal)
		{
			if (m_SentHonorContext != null)
			{
				m_SentHonorContext.OnSourceBeneficialAction(target);
			}

            if (Siege.SiegeShard && isCriminal)
            {
                Criminal = true;
                return;
            }

			base.OnBeneficialAction(target, isCriminal);
		}

        public override bool IsBeneficialCriminal(Mobile target)
        {
            if (!target.Criminal && target is BaseCreature && ((BaseCreature)target).GetMaster() == this)
                return false;

            return base.IsBeneficialCriminal(target);
        }

		public override void OnDamage(int amount, Mobile from, bool willKill)
		{
			/*
			BandageContext c = BandageContext.GetContext( this );
			if ( c != null )
				c.Slip();
			*/
			/*
			int disruptThreshold;

			if (!Core.AOS)
			{
				disruptThreshold = 0;
			}
			else if (from != null && from.Player)
			{
				disruptThreshold = 19;
			}
			else
			{
				disruptThreshold = 26;
			}

            if (Core.SA)
            {
                disruptThreshold += Dex / 12;
            }

			if (amount > disruptThreshold)
			{
				BandageContext c = BandageContext.GetContext(this);

				if (c != null)
				{
					c.Slip();
				}
			}

			if (Confidence.IsRegenerating(this))
			{
				Confidence.StopRegenerating(this);
			}
			*/
			if (m_ReceivedHonorContext != null)
			{
				m_ReceivedHonorContext.OnTargetDamaged(from, amount);
			}
			if (m_SentHonorContext != null)
			{
				m_SentHonorContext.OnSourceDamaged(from, amount);
			}

			if (willKill && from is PlayerMobile)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(10), ((PlayerMobile)@from).RecoverAmmo);
			}

			#region Mondain's Legacy
			if (InvisibilityPotion.HasTimer(this))
			{
				InvisibilityPotion.Iterrupt(this);
			}
			#endregion

            UndertakersStaff.TryRemoveTimer(this);

			base.OnDamage(amount, from, willKill);
		}

		public override void Resurrect()
		{
			bool wasAlive = Alive;

			base.Resurrect();

			if (Alive && !wasAlive)
			{
				Item deathRobe = new DeathRobe();

				if (!EquipItem(deathRobe))
				{
					deathRobe.Delete();
				}

                if (NetState != null /*&& NetState.IsEnhancedClient*/)
                {
                    Waypoints.RemoveHealers(this, Map);
                }

                #region Scroll of Alacrity
                if (AcceleratedStart > DateTime.UtcNow)
				{
					BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.ArcaneEmpowerment, 1078511, 1078512, AcceleratedSkill.ToString()));
				}
				#endregion
			}
		}

		public override double RacialSkillBonus
		{
			get
			{
				if (Core.ML && Race == Race.Human)
				{
					return 0.0;
				}

				return 0;
			}
		}

        public override double GetRacialSkillBonus(SkillName skill)
        {
            if (Core.ML && Race == Race.Human)
                return 0.0;

            if (Core.SA && Race == Race.Gargoyle)
            {
                if (skill == SkillName.Imbuing)
                    return 0.0;

                if (skill == SkillName.Throwing)
                    return 0.0;
            }

            return RacialSkillBonus;
        }

		public override void OnWarmodeChanged()
		{
			if (!Warmode)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(10), RecoverAmmo);
			}
		}

		private Mobile m_InsuranceAward;
		private int m_InsuranceCost;
		private int m_InsuranceBonus;

		private List<Item> m_EquipSnapshot;

		public List<Item> EquipSnapshot { get { return m_EquipSnapshot; } }

		private bool FindItems_Callback(Item item)
		{
			if (!item.Deleted && (item.LootType == LootType.Blessed || item.Insured))
			{
				if (Backpack != item.Parent)
				{
					return true;
				}
			}
			return false;
		}

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Criminal
        {
            get
            {
                return base.Criminal;
            }
            set
            {
                bool crim = base.Criminal;
                base.Criminal = value;

                if (value != crim)
                {
                    if (value)
                        BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.CriminalStatus, 1153802, 1153828));
                    else
                        BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);
                }
            }
        }
		public void HelpSay()
		{
			if ( this.Alive && m_Coma && m_TimerList[67] > 0 )
				this.Say("Help me!!!");
			Timer.DelayCall( TimeSpan.FromSeconds( 15.0 ), new TimerCallback( HelpSay ) );
		}
        public override bool OnBeforeDeath()
        {
            NetState state = NetState;

            if (state != null)
            {
                state.CancelAllTrades();
            }

            if (Criminal)
                BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);

			Party party = Engines.PartySystem.Party.Get( this );

			if( this.Map == Map.Trammel && this.Location.X >= 3760 && this.Location.X <= 3784 && this.Location.Y >= 2761 && this.Location.Y <= 2776 && this.Location.Z == 5 )
			{
				PvPMove();
				return false;
			}
			else if ( party != null && party.Members.Count >= 2 && !m_Coma && m_TimerList[67] == 0 )
			{
				this.Frozen = true;
				this.Blessed = true;
				m_Coma = true;
				m_TimerList[67] = 600;
				HelpSay();
				return false;
			}
			else
			{
				this.DeathMove();
				return false;
			}
			/*	
			else if ( party == null || party.Members.Count < 2 || ( m_Coma == true && m_TimerList[67] == 0 ) )
			{
				this.DeathMove();
				return false;
			}
			*/
            return base.OnBeforeDeath();
        }
		
		public void UnEquipCheck()
		{
			Fury = 0;
			FuryActive = false;
			Server.Spells.Bushido.Evasion.EndEvasion(this);
			Server.Spells.Chivalry.DivineFurySpell.RemoveEffects(this);
			Server.Spells.Chivalry.ConsecrateWeaponSpell.RemoveEffects(this);
			Server.Spells.Bushido.Confidence.EndConfidence(this);
			Server.Spells.Bushido.CounterAttack.StopCountering(this);
			Server.Spells.Chivalry.EnemyOfOneSpell.RemoveEffect(this);
		}
		
		private bool CheckInsuranceOnDeath(Item item)
		{
            if (Young)
				return false;

			if (InsuranceEnabled && item.Insured)
			{
                int insuredAmount = GetInsuranceCost(item);

				if (AutoRenewInsurance)
				{
                    int cost = (m_InsuranceAward == null ? insuredAmount : insuredAmount / 2);

					if (Banker.Withdraw(this, cost))
					{
						m_InsuranceCost += cost;
						item.PayedInsurance = true;
						SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
					}
					else
					{
						SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
						item.PayedInsurance = false;
						item.Insured = false;
						m_NonAutoreinsuredItems++;
					}
				}
				else
				{
					item.PayedInsurance = false;
					item.Insured = false;
				}

				if (m_InsuranceAward != null)
				{
					if (Banker.Deposit(m_InsuranceAward, insuredAmount / 2))
					{
						if (m_InsuranceAward is PlayerMobile)
						{
							((PlayerMobile)m_InsuranceAward).m_InsuranceBonus += insuredAmount / 2;
						}
					}
				}

				return true;
			}

			return false;
		}

		public override DeathMoveResult GetParentMoveResultFor(Item item)
		{
			if (CheckInsuranceOnDeath(item) && !Young)
			{
				return DeathMoveResult.MoveToBackpack;
			}

			DeathMoveResult res = base.GetParentMoveResultFor(item);

			if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
			{
				res = DeathMoveResult.MoveToBackpack;
			}

			return res;
		}

		public override DeathMoveResult GetInventoryMoveResultFor(Item item)
		{
			if (CheckInsuranceOnDeath(item) && !Young)
			{
				return DeathMoveResult.MoveToBackpack;
			}

			DeathMoveResult res = base.GetInventoryMoveResultFor(item);

			if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
			{
				res = DeathMoveResult.MoveToBackpack;
			}

			return res;
		}

		public override void OnDeath(Container c)
		{
            if (NetState != null /*&& NetState.IsEnhancedClient*/)
            {
                Waypoints.OnDeath(this);
            }

			Mobile m = FindMostRecentDamager(false);
            PlayerMobile killer = m as PlayerMobile;

            if (killer == null && m is BaseCreature)
            {
                killer = ((BaseCreature)m).GetMaster() as PlayerMobile;
            }

			if (m_NonAutoreinsuredItems > 0)
			{
				SendLocalizedMessage(1061115);
			}

			base.OnDeath(c);

			m_EquipSnapshot = null;

			HueMod = -1;
			NameMod = null;
			SavagePaintExpiration = TimeSpan.Zero;

			SetHairMods(-1, -1);

			PolymorphSpell.StopTimer(this);
			IncognitoSpell.StopTimer(this);
			DisguiseTimers.RemoveTimer(this);

            WeakenSpell.RemoveEffects(this);
            ClumsySpell.RemoveEffects(this);
            FeeblemindSpell.RemoveEffects(this);
            CurseSpell.RemoveEffect(this);
            Spells.Second.ProtectionSpell.EndProtection(this);


            EndAction(typeof(PolymorphSpell));
			EndAction(typeof(IncognitoSpell));

			MeerMage.StopEffect(this, false);

            BaseEscort.DeleteEscort(this);

            #region Stygian Abyss
            if (Flying)
			{
				Flying = false;
				BuffInfo.RemoveBuff(this, BuffIcon.Fly);
			}
			#endregion

			StolenItem.ReturnOnDeath(this, c);

			if (m_PermaFlags.Count > 0)
			{
				m_PermaFlags.Clear();

				if (c is Corpse)
				{
					((Corpse)c).Criminal = true;
				}

				if (Stealing.ClassicMode)
				{
					Criminal = true;
				}
			}

            if (killer != null && Murderer && DateTime.UtcNow >= killer.m_NextJustAward)
            {
                // This scales 700.0 skill points to 1000 valor points
                int pointsToGain = (int)(SkillsTotal / 7);

                // This scales 700.0 skill points to 7 minutes wait
                int minutesToWait = Math.Max(1, (int)(SkillsTotal / 1000));

                bool gainedPath = false;

                if (VirtueHelper.Award(m, VirtueName.Justice, pointsToGain, ref gainedPath))
                {
                    if (gainedPath)
                    {
                        m.SendLocalizedMessage(1049367); // You have gained a path in Justice!
                    }
                    else
                    {
                        m.SendLocalizedMessage(1049363); // You have gained in Justice.
                    }

                    m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
                    m.PlaySound(0x1F7);

                    killer.m_NextJustAward = DateTime.UtcNow + TimeSpan.FromMinutes(minutesToWait);
                }
            }

			if (m_InsuranceAward is PlayerMobile)
			{
				PlayerMobile pm = (PlayerMobile)m_InsuranceAward;

				if (pm.m_InsuranceBonus > 0)
				{
					pm.SendLocalizedMessage(1060397, pm.m_InsuranceBonus.ToString());
					// ~1_AMOUNT~ gold has been deposited into your bank box.
				}
			}

			if (Young)
			{
				if (YoungDeathTeleport())
				{
					Timer.DelayCall(TimeSpan.FromSeconds(2.5), SendYoungDeathNotice);
				}
			}

			Faction.HandleDeath(this, killer);

			Guilds.Guild.HandleDeath(this, killer);
            
            if (m_BuffTable != null)
			{
				var list = new List<BuffInfo>();

				foreach (BuffInfo buff in m_BuffTable.Values)
				{
					if (!buff.RetainThroughDeath)
					{
						list.Add(buff);
					}
				}

				for (int i = 0; i < list.Count; i++)
				{
					RemoveBuff(list[i]);
				}
			}

			#region Stygian Abyss
			if (Region.IsPartOf("Abyss") && SSSeedExpire > DateTime.UtcNow)
			{
				SendGump(new ResurrectGump(this, ResurrectMessage.SilverSapling));
			}

            if (LastKiller is BaseVoidCreature)
                ((BaseVoidCreature)LastKiller).Mutate(VoidEvolution.Killing);
			#endregion
		}

		private List<Mobile> m_PermaFlags;
		private readonly List<Mobile> m_VisList;
		private readonly Hashtable m_AntiMacroTable;
		private TimeSpan m_GameTime;
		private TimeSpan m_ShortTermElapse;
		private TimeSpan m_LongTermElapse;
		private DateTime m_SessionStart;
		private DateTime m_SavagePaintExpiration;
		private SkillName m_Learning = (SkillName)(-1);

		public SkillName Learning { get { return m_Learning; } set { m_Learning = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan SavagePaintExpiration
		{
			get
			{
				TimeSpan ts = m_SavagePaintExpiration - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
				{
					ts = TimeSpan.Zero;
				}

				return ts;
			}
			set { m_SavagePaintExpiration = DateTime.UtcNow + value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NextSmithBulkOrder
		{
			get
			{
                return BulkOrderSystem.GetNextBulkOrder(BODType.Smith, this);
			}
			set
			{
                BulkOrderSystem.SetNextBulkOrder(BODType.Smith, this, value);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NextTailorBulkOrder
		{
			get
			{
                return BulkOrderSystem.GetNextBulkOrder(BODType.Tailor, this);
			}
			set
			{
                BulkOrderSystem.SetNextBulkOrder(BODType.Tailor, this, value);
			}
		}

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextAlchemyBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Alchemy, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Alchemy, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextInscriptionBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Inscription, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Inscription, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextTinkeringBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Tinkering, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Tinkering, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextFletchingBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Fletching, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Fletching, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextCarpentryBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Carpentry, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Carpentry, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextCookingBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Cooking, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Cooking, this, value);
            }
        }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastEscortTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastPetBallTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastPotionTime { get; set; }

		public PlayerMobile()
		{
			Instances.Add(this);

			m_AutoStabled = new List<Mobile>();

			#region Mondain's Legacy
			//m_Quests = new List<BaseQuest>();
			//m_Chains = new Dictionary<QuestChain, BaseChain>();
			m_DoneQuests = new List<QuestRestartInfo>();
			m_Collections = new Dictionary<Collection, int>();
			m_RewardTitles = new List<object>();

			m_PeacedUntil = DateTime.UtcNow;
			#endregion

			m_VisList = new List<Mobile>();
			m_PermaFlags = new List<Mobile>();
			m_AntiMacroTable = new Hashtable();
			m_RecentlyReported = new List<Mobile>();

			//m_BOBFilter = new BOBFilter();

			m_GameTime = TimeSpan.Zero;
			m_ShortTermElapse = TimeSpan.FromHours(8.0);
			m_LongTermElapse = TimeSpan.FromHours(40.0);

			m_JusticeProtectors = new List<Mobile>();
			m_GuildRank = RankDefinition.Lowest;

			m_ChampionTitles = new ChampionTitleInfo();
		}

		public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
		{
			if (Alive)
			{
				return false;
			}

			if (Core.ML && Skills[SkillName.SpiritSpeak].Value >= 100.0)
			{
				return false;
			}

			if (Core.AOS)
			{
				for (int i = 0; i < hears.Count; ++i)
				{
					Mobile m = hears[i];

					if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
					{
						return false;
					}
				}
			}

			return base.MutateSpeech(hears, ref text, ref context);
		}

		public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
		{
			if (Guilds.Guild.NewGuildSystem && (type == MessageType.Guild || type == MessageType.Alliance))
			{
				Guild g = Guild as Guild;
				if (g == null)
				{
					SendLocalizedMessage(1063142); // You are not in a guild!
				}
				else if (type == MessageType.Alliance)
				{
					if (g.Alliance != null && g.Alliance.IsMember(g))
					{
						//g.Alliance.AllianceTextMessage( hue, "[Alliance][{0}]: {1}", Name, text );
						g.Alliance.AllianceChat(this, text);
						SendToStaffMessage(this, "[Alliance]: {0}", text);

						m_AllianceMessageHue = hue;
					}
					else
					{
						SendLocalizedMessage(1071020); // You are not in an alliance!
					}
				}
				else //Type == MessageType.Guild
				{
					m_GuildMessageHue = hue;

					g.GuildChat(this, text);
					SendToStaffMessage(this, "[Guild]: {0}", text);
				}
			}
			else
			{
				base.DoSpeech(text, keywords, type, hue);
			}
		}

		private static void SendToStaffMessage(Mobile from, string text)
		{
			Packet p = null;

			foreach (NetState ns in from.GetClientsInRange(8))
			{
				Mobile mob = ns.Mobile;

				if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > from.AccessLevel)
				{
					if (p == null)
					{
						p =
							Packet.Acquire(
								new UnicodeMessage(
									from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language, from.Name, text));
					}

					ns.Send(p);
				}
			}

			Packet.Release(p);
		}

		private static void SendToStaffMessage(Mobile from, string format, params object[] args)
		{
			SendToStaffMessage(from, String.Format(format, args));
		}

        #region Poison
        public override void OnCured(Mobile from, Poison oldPoison)
        {
            BuffInfo.RemoveBuff(this, BuffIcon.Poison);
        }

        public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
		{
			if (!Alive || poison == null)
			{
				return ApplyPoisonResult.Immune;
			}

            //Skill Masteries
            if (Spells.SkillMasteries.ResilienceSpell.UnderEffects(this) && 0.25 > Utility.RandomDouble())
            {
                return ApplyPoisonResult.Immune;
            }

			if (EvilOmenSpell.TryEndEffect(this))
			{
				poison = PoisonImpl.IncreaseLevel(poison);
			}

            //Skill Masteries
            if ((this.Poison == null || this.Poison.Level < poison.Level) && ToleranceSpell.OnPoisonApplied(this))
            {
                poison = PoisonImpl.DecreaseLevel(poison);

                if (poison == null || poison.Level <= 0)
                {
                    PrivateOverheadMessage(MessageType.Regular, 0x3F, 1053092, this.NetState); // * You feel yourself resisting the effects of the poison *
                    return ApplyPoisonResult.Immune;
                }
            }

			ApplyPoisonResult result = base.ApplyPoison(from, poison);

			if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer)
			{
				(PoisonTimer as PoisonImpl.PoisonTimer).From = from;
			}

			return result;
		}

		public override bool CheckPoisonImmunity(Mobile from, Poison poison)
		{
			if (Young)
			{
				return true;
			}

			return base.CheckPoisonImmunity(from, poison);
		}

		public override void OnPoisonImmunity(Mobile from, Poison poison)
		{
			if (Young)
			{
				SendLocalizedMessage(502808);
				// You would have been poisoned, were you not new to the land of Britannia. Be careful in the future.
			}
			else
			{
				base.OnPoisonImmunity(from, poison);
			}
		}
		#endregion

		public PlayerMobile(Serial s)
			: base(s)
		{
			Instances.Add(this);

			m_VisList = new List<Mobile>();
			m_AntiMacroTable = new Hashtable();
		}

		public List<Mobile> VisibilityList { get { return m_VisList; } }

		public List<Mobile> PermaFlags { get { return m_PermaFlags; } }

        public override int Luck { get { return Math.Min( 100 + SilverPoint[31] * 5 + AosAttributes.GetValue(this, AosAttribute.Luck) + TenthAnniversarySculpture.GetLuckBonus(this) + SkillbyStat[3] / 1000, 9999 ); } }

        public int RealLuck
		{ 
            get
            {
                int facetBonus = !Siege.SiegeShard && this.Map == Map.Felucca ? RandomItemGenerator.FeluccaLuckBonus : 0;

                return Luck + FountainOfFortune.GetLuckBonus(this) + facetBonus;
            }
        }

		public override bool IsHarmfulCriminal(IDamageable damageable)
		{
            Mobile target = damageable as Mobile;

			if (Stealing.ClassicMode && target is PlayerMobile && ((PlayerMobile)target).m_PermaFlags.Count > 0)
			{
				int noto = Notoriety.Compute(this, target);

				if (noto == Notoriety.Innocent)
				{
					target.Delta(MobileDelta.Noto);
				}

				return false;
			}

			if (target is BaseCreature && ((BaseCreature)target).InitialInnocent && !((BaseCreature)target).Controlled)
			{
				return false;
			}

			if (Core.ML && target is BaseCreature && ((BaseCreature)target).Controlled && ((BaseCreature)target).ControlMaster == this)
			{
				return false;
			}

            if (target is BaseCreature && ((BaseCreature)target).Summoned && ((BaseCreature)target).SummonMaster == this)
            {
                return false;
            }

			return base.IsHarmfulCriminal(damageable);
		}

		public bool AntiMacroCheck(Skill skill, object obj)
		{
			if (obj == null || m_AntiMacroTable == null || IsStaff())
			{
				return true;
			}

			Hashtable tbl = (Hashtable)m_AntiMacroTable[skill];
			if (tbl == null)
			{
				m_AntiMacroTable[skill] = tbl = new Hashtable();
			}

			CountAndTimeStamp count = (CountAndTimeStamp)tbl[obj];
			if (count != null)
			{
				if (count.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
				{
					count.Count = 1;
					return true;
				}
				else
				{
					++count.Count;
					if (count.Count <= SkillCheck.Allowance)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				tbl[obj] = count = new CountAndTimeStamp();
				count.Count = 1;

				return true;
			}
		}
		//유저 변수 모음
		private DateTime m_ParryTime;
		public DateTime ParryTime
		{
			get{ return m_ParryTime; }
			set{ m_ParryTime = value;}
		}
		
		private int m_ActionPoint;
		[CommandProperty( AccessLevel.GameMaster )]
		public int ActionPoint
		{
			get{ return m_ActionPoint; }
			set{ m_ActionPoint = value;InvalidateProperties();}
		}

		private bool m_FirstSkill;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool FirstSkill
		{
			get{ return m_FirstSkill; }
			set{ m_FirstSkill = value;}
		}


		private bool m_Coma;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Coma
		{
			get{ return m_Coma; }
			set{ m_Coma = value;}
		}
		/*
		private int m_ComaTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public int ComaTime
		{
			get{ return m_ComaTime; }
			set{ m_ComaTime = value;}
		}
		*/

		private double m_Tired;
		[CommandProperty( AccessLevel.GameMaster )]
		public double Tired
		{
			get{ return m_Tired; }
			set{ m_Tired = value;}
		}
		
		private int m_DungeonFear;
		[CommandProperty( AccessLevel.GameMaster )]
		public int DungeonFear
		{
			get{ return m_DungeonFear; }
			set{ m_DungeonFear = value;}
		}
		private int m_DungeonCheck;
		[CommandProperty( AccessLevel.GameMaster )]
		public int DungeonCheck
		{
			get{ return m_DungeonCheck; }
			set{ m_DungeonCheck = value;}
		}

		private int m_Level;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Level
		{
			get{ return m_Level;}
			set{ m_Level = value; InvalidateProperties();}
		}
		private int m_Exp;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Exp
		{
			get{ return m_Exp;}
			set{ m_Exp = value; InvalidateProperties();}
		}
		private int m_ShipCheck;
		[CommandProperty( AccessLevel.GameMaster )]
		public int ShipCheck
		{
			get{ return m_ShipCheck;}
			set{ m_ShipCheck = value; InvalidateProperties();}
		}

		private int m_FarmCheck;
		[CommandProperty( AccessLevel.GameMaster )]
		public int FarmCheck
		{
			get{ return m_FarmCheck;}
			set{ m_FarmCheck = value; InvalidateProperties();}
		}
		
		private DateTime m_FarmTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime FarmTime
		{
			get{ return m_FarmTime; }
			set{ m_FarmTime = value; }
		}

		private DateTime m_DayTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime DayTime
		{
			get{ return m_DayTime; }
			set{ m_DayTime = value; }
		}	
		public int ShipUse = 0;
		
		private DateTime m_WeekTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime WeekTime
		{
			get{ return m_WeekTime; }
			set{ m_WeekTime = value; }
		}	

		private bool m_FreeSkill;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool FreeSkill
		{
			get{ return m_FreeSkill; }
			set{ m_FreeSkill = value; }
		}	
		
		private DateTime m_MonthTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime MonthTime
		{
			get{ return m_MonthTime; }
			set{ m_MonthTime = value; }
		}	

		private int m_DeathCheck;
		[CommandProperty( AccessLevel.GameMaster )]
		public int DeathCheck
		{
			get{ return m_DeathCheck;}
			set{ m_DeathCheck = value; InvalidateProperties();}
		}
	
		//작위 변수
		private int m_PlayerPoint;
		[CommandProperty( AccessLevel.GameMaster )]
		public int PlayerPoint
		{
			get{ return m_PlayerPoint;}
			set{ m_PlayerPoint = value; InvalidateProperties();}
		}

		//실버 포인트
		private int m_GatePoint;
		[CommandProperty( AccessLevel.GameMaster )]
		public int GatePoint
		{
			get{ return m_GatePoint;}
			set{ m_GatePoint = value; InvalidateProperties();}
		}

		private DateTime m_MoongateTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime MoongateTime
		{
			get{ return m_MoongateTime; }
			set{ m_MoongateTime = value; InvalidateProperties(); }
		}

		//해부학 교환
		private bool m_AnatomytoTasteID;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool AnatomytoTasteID 
		{ 
			get { return m_AnatomytoTasteID; } 
			set { m_AnatomytoTasteID = value; InvalidateProperties(); } 
		}
		//생산, 전투 포인트 반환
		private bool m_GoldAndSilverPointReturntoSkill;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool GoldAndSilverPointReturntoSkill 
		{ 
			get { return m_GoldAndSilverPointReturntoSkill; } 
			set { m_GoldAndSilverPointReturntoSkill = value; InvalidateProperties(); } 
		}
		//장비 포인트를 던전 포인트로 변환
		private bool m_EquiptoDungeon;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool EquiptoDungeon 
		{ 
			get { return m_EquiptoDungeon; } 
			set { m_EquiptoDungeon = value; InvalidateProperties(); } 
		}
		//마지막 귀환 도시
		private int m_SaveTown;
		[CommandProperty( AccessLevel.GameMaster )]
		public int SaveTown 
		{ 
			get { return m_SaveTown; } 
			set { m_SaveTown = value; InvalidateProperties(); } 
		}

		private int m_StamTimeUp = 36000;
		[CommandProperty( AccessLevel.GameMaster )]
		public int StamTimeUp 
		{ 
			get { return m_StamTimeUp; } 
			set { m_StamTimeUp = value; InvalidateProperties(); } 
		}
		
		//붕대 사용
		private int m_UseBandage;
		[CommandProperty( AccessLevel.GameMaster )]
		public int UseBandage 
		{ 
			get { return m_UseBandage; } 
			set { m_UseBandage = value; InvalidateProperties(); } 
		}
		
		//스킬 정의
		private double[] m_SkillList = new double[58];
		public double[] SkillList
		{
			get{ return m_SkillList;}
			set{ m_SkillList = value; InvalidateProperties();}
		}
		
		private int[] m_SkillbyStat = new int[7];
		[CommandProperty( AccessLevel.GameMaster )]
		public int[] SkillbyStat
		{
			get{ return m_SkillbyStat; }
			set{ m_SkillbyStat = value; InvalidateProperties();}
		}
		
		private int m_SkillsTotal_Check;
		[CommandProperty( AccessLevel.GameMaster )]
		public int SkillsTotal_Check
		{
			get{ return m_SkillsTotal_Check; }
			set{ m_SkillsTotal_Check = value; InvalidateProperties();}
		}
		//로그인 체크
		private bool m_TodayLogin;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool TodayLogin 
		{ 
			get { return m_TodayLogin; } 
			set { m_TodayLogin = value; InvalidateProperties(); } 
		}
		
		public bool SkillUpCheck(double skillvalue, int skill, double gain)
		{
			m_SkillList[skill] += gain;
			
			if( m_SkillList[skill] < 0 )
				m_SkillList[skill] = 0;

			if( m_SkillList[skill] >= Misc.Util.SkillExp_Calc(this, skill) )
			{
				m_SkillList[skill] = 0;

				return true;
			}
			return false;
		}
		
		//포인트 정의
		private int[] m_GoldPoint = new int[50];
		public int[] GoldPoint
		{
			get{ return m_GoldPoint;}
			set{ m_GoldPoint = value; InvalidateProperties();}
		}

		private int[] m_HarvestPoint = new int[1000];
		public int[] HarvestPoint
		{
			get{ return m_HarvestPoint;}
			set{ m_HarvestPoint = value; InvalidateProperties();}
		}

		private int[] m_CraftPoint = new int[1000];
		public int[] CraftPoint
		{
			get{ return m_CraftPoint;}
			set{ m_CraftPoint = value; InvalidateProperties();}
		}
		
		private bool[] m_StatReset = new bool[10000];
		public bool[] StatReset
		{
			get{ return m_StatReset;}
			set{ m_StatReset = value; InvalidateProperties();}
		}

		private bool[] m_BuffCheck = new bool[100];
		public bool[] BuffCheck
		{
			get { return m_BuffCheck; }
			set { m_BuffCheck = value; InvalidateProperties();}
		}
		
		private int[] m_SilverPoint = new int[100];
		public int[] SilverPoint
		{
			get{ return m_SilverPoint;}
			set{ m_SilverPoint = value; InvalidateProperties();}
		}
		private int[] m_EquipPoint = new int[40];
		public int[] EquipPoint
		{
			get{ return m_EquipPoint;}
			set{ m_EquipPoint = value; InvalidateProperties();}
		}
		private int[] m_ArtifactPoint = new int[80];
		public int[] ArtifactPoint
		{
			get{ return m_ArtifactPoint;}
			set{ m_ArtifactPoint = value; InvalidateProperties();}
		}
		private int[] m_MonsterPoint = new int[50000];
		public int[] MonsterPoint
		{
			get{ return m_MonsterPoint;}
			set{ m_MonsterPoint = value; InvalidateProperties();}
		}
		
		//장비 해체 저장
		private bool[] m_EquipMeltingOptionRank = new bool[6];
		public bool[] EquipMeltingOptionRank
		{
			get{ return m_EquipMeltingOptionRank;}
			set{ m_EquipMeltingOptionRank = value; InvalidateProperties();}
		}
		private bool[] m_EquipMeltingOptionTier = new bool[7];
		public bool[] EquipMeltingOptionTier
		{
			get{ return m_EquipMeltingOptionTier;}
			set{ m_EquipMeltingOptionTier = value; InvalidateProperties();}
		}
		private bool[] m_EquipMeltingOptionNamed = new bool[2];
		public bool[] EquipMeltingOptionNamed
		{
			get{ return m_EquipMeltingOptionNamed;}
			set{ m_EquipMeltingOptionNamed = value; InvalidateProperties();}
		}
		
		private bool m_EquipMeltingOptionBag;
		public bool EquipMeltingOptionBag
		{
			get{ return m_EquipMeltingOptionBag;}
			set{ m_EquipMeltingOptionBag = value; InvalidateProperties();}
		}
		//퀘스트 사용 테그
		//0 ~ 4999 : 채집
		//5000 ~ 9999 : 제작
		//10000 ~ 19999 : 전투
		//20000 ~ 24999 : 최초 테이밍 몬스터
		//25000 ~ 29999 : 최초 훔친 아이템
		//30000 ~ 39999 : 탐험(미지 지역 발견)
		//40000 ~ 44999 : 택배(배달)
		//45000 ~ 49999 : 그 외 핵심 퀘스트
		//0 ~ 8 : 채집 주요 퀘스트
		//코베투스 - 코브
		//데스파이즈 - 브리튼
		//디싯 - 문글로우
		//쉐임 - 스카라 브레
		//오크 던전 - 유
		//롱 던전 - 미녹
		//아이스 던전 - 젤롬
		//파이어 던전 - 서펜드 홀드
		//히스로스 던전 - 누젤롬
		//데스타드 던전 - 트린식
		//10000 : 코베투스 1티어 퀘스트 - 전투 포인트 1
		//10001 : 코베투스 2티어 퀘스트 - 전투 포인트 2
		//10002 : 코베투스 3티어 퀘스트 - 전투 포인트 3
		//10003 : 데스파이즈 1티어 퀘스트 - 전투 포인트 1
		//10004 : 데스파이즈 2티어 퀘스트 - 전투 포인트 2
		//10005 : 데스파이즈 3티어 퀘스트 - 전투 포인트 3
		//10006 : 디싯 1티어 퀘스트 - 전투 포인트 1
		//10007 : 디싯 2티어 퀘스트 - 전투 포인트 2
		//10008 : 디싯 3티어 퀘스트 - 전투 포인트 3
		//10006 : 쉐임 1티어 퀘스트 - 전투 포인트 1
		//10007 : 쉐임 2티어 퀘스트 - 전투 포인트 2
		//10008 : 쉐임 3티어 퀘스트 - 전투 포인트 3
		//10009 : 오크 던전 1티어 퀘스트 - 전투 포인트 1
		//10010 : 오크 던전 2티어 퀘스트 - 전투 포인트 2
		//10011 : 오크 던전 3티어 퀘스트 - 전투 포인트 3
		//10012 : 롱 던전 1티어 퀘스트 - 전투 포인트 1
		//10013 : 롱 던전 2티어 퀘스트 - 전투 포인트 2
		//10014 : 롱 던전 3티어 퀘스트 - 전투 포인트 3
		//10015 : 아이스 던전 1티어 퀘스트 - 전투 포인트 1
		//10016 : 아이스 던전 2티어 퀘스트 - 전투 포인트 2
		//10017 : 아이스 던전 3티어 퀘스트 - 전투 포인트 3
		//10018 : 파이어 던전 1티어 퀘스트 - 전투 포인트 1
		//10019 : 파이어 던전 2티어 퀘스트 - 전투 포인트 2
		//10020 : 파이어 던전 3티어 퀘스트 - 전투 포인트 3
		//10021 : 히스로스 던전 1티어 퀘스트 - 전투 포인트 1
		//10022 : 히스로스 던전 2티어 퀘스트 - 전투 포인트 2
		//10023 : 히스로스 던전 3티어 퀘스트 - 전투 포인트 3
		//10024 : 데스타드 던전 1티어 퀘스트 - 전투 포인트 1
		//10025 : 데스타드 던전 2티어 퀘스트 - 전투 포인트 2
		//10026 : 데스타드 던전 3티어 퀘스트 - 전투 포인트 3 (도합 60)
		
		private bool[] m_QuestCheck = new bool[50000];
		public bool[] QuestCheck
		{
			get{ return m_QuestCheck;}
			set{ m_QuestCheck = value; InvalidateProperties();}
		}
		
		//타이머 정의
		private int[] m_TimerList = new int[100];
		public int[] TimerList
		{
			get{ return m_TimerList;}
			set{ m_TimerList = value; InvalidateProperties();}
		}

		//마을 포인트 정의
		/*
			0 : Moonglow,
			1 : Britain,
			2 : Jhelom,
			3 : Yew,
			4 : Minoc,
			5 : Trinsic,
			6 : SkaraBrae,
			7 : NewMagincia,
			8 : Vesper
		*/
		private int[] m_City = new int[9];
		public int[] City
		{
			get{ return m_City;}
			set{ m_City = value; InvalidateProperties();}
		}
	
		private void RevertHair()
		{
			SetHairMods(-1, -1);
		}

		public BOBFilter BOBFilter
        {
            get
            {
                return BulkOrderSystem.GetBOBFilter(this);
            }
        }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 60:
				{
					for (int i = 0; i < 100; i++)
					{
						m_ItemSetOption[i] = reader.ReadInt();
						m_ItemSetValue[i] = reader.ReadInt();
					}					
					for (int i = 0; i < 500; i++)
					{
						m_ItemSetSaveValue[i] = reader.ReadInt();
					}					
					goto case 59;
				}
				case 59:
				{
					m_EquipMeltingOptionBag = reader.ReadBool();
					goto case 58;
				}
				case 58:
				{
					for (int i = 0; i < 6; i++)
					{
						m_EquipMeltingOptionRank[i] = reader.ReadBool();
					}
					for (int i = 0; i < 7; i++)
					{
						m_EquipMeltingOptionTier[i] = reader.ReadBool();
					}
					for (int i = 0; i < 2; i++)
					{
						m_EquipMeltingOptionNamed[i] = reader.ReadBool();
					}
					goto case 57;
				}
				case 57:
				{
					m_AutoFood = reader.ReadItem();
					goto case 56;
				}
				case 56:
				{
					m_Another_Land = reader.ReadDateTime();
					goto case 55;
				}
				case 55:
				{
					for (int i = 0; i < 1000; i++)
					{
						m_HarvestPoint[i] = reader.ReadInt();
						m_CraftPoint[i] = reader.ReadInt();
					}
					goto case 54;
				}
				case 54:
				{
					for (int i = 0; i < 50000; i++)
					{
						m_QuestCheck[i] = reader.ReadBool();
					}
					goto case 53;
				}
				case 53:
				{
					for( int i = 0; i < 100; i++ )
					{
						m_MonsterSave[i] = reader.ReadString();
					}
					goto case 52;
				}
				case 52:
				{
					for( int i = 0; i < 100; i++ )
					{
						m_ItemSave[i] = reader.ReadItem();
						m_PlayerSave[i] = reader.ReadMobile();
						m_PetSave[i] = reader.ReadMobile();
					}
					goto case 51;
				}
				case 51:
				{
					for (int i = 0; i < 50000; i++)
					{
						m_MonsterPoint[i] = reader.ReadInt();
					}
					goto case 50;
				}
				case 50:
				{
					for (int i = 0; i < 100; i++)
					{
						m_BuffCheck[i] = reader.ReadBool();
					}
					for (int i = 0; i < 10000; i++)
					{
						m_StatReset[i] = reader.ReadBool();
					}
					goto case 49;
				}
                case 49:
				{
                    m_Tired = reader.ReadDouble();
                    goto case 48;
				}
                case 48:
				{
                    m_TodayLogin = reader.ReadBool();
                    goto case 47;
				}
				case 47:
				{
					m_EquiptoDungeon = reader.ReadBool();
					for (int i = 20; i < 50; i++)
					{
						m_GoldPoint[i] = reader.ReadInt();
					}
					goto case 46;
				}
				case 46:
				{
					m_GoldAndSilverPointReturntoSkill = reader.ReadBool();					
					goto case 45;
				}
				case 45:
				{
					m_AnatomytoTasteID = reader.ReadBool();					
					goto case 44;
				}
                case 44:
				{
                    m_FreeSkill = reader.ReadBool();
					m_WeekTime = reader.ReadDateTime();
                    goto case 43;
				}
                case 43:
				{
                    m_ActionPoint = reader.ReadInt();
                    m_FirstSkill = reader.ReadBool();
                    goto case 42;
				}
				case 42:
				{
					for (int i = 0; i < 20; i++)
					{
						m_GoldPoint[i] = reader.ReadInt();
					}
					for (int i = 0; i < 100; i++)
					{
						m_SilverPoint[i] = reader.ReadInt();
					}
					for (int i = 0; i < 40; i++)
					{
						m_EquipPoint[i] = reader.ReadInt();
					}
					for (int i = 0; i < 80; i++)
					{
						m_ArtifactPoint[i] = reader.ReadInt();
					}
					goto case 41;
				}
				case 41:
				{
					for (int i = 0; i < 7; i++)
					{
						m_SkillbyStat[i] = reader.ReadInt();
					}
					for (int i = 0; i < 9; i++)
					{
						m_City[i] = reader.ReadInt();
					}
					m_StamTimeUp = reader.ReadInt();
					m_MoongateTime = reader.ReadDateTime();
					m_ShipCheck = reader.ReadInt();
					m_YoungTime = reader.ReadDateTime();
					for (int i = 0; i < 100; i++)
					{
						m_TimerList[i] = reader.ReadInt();
					}
					if( Frozen )
						YellowHealthbar = true;
					else
						YellowHealthbar = false;
					m_SaveTown = reader.ReadInt();
					m_GatePoint = reader.ReadInt();
					m_PlayerPoint = reader.ReadInt();
					for (int i = 0; i < 58; i++)
					{
						m_SkillList[i] = reader.ReadDouble();
					}
					m_DungeonCheck = reader.ReadInt();
					m_DungeonFear = reader.ReadInt();
					m_Coma = reader.ReadBool();
					//m_ComaTime = reader.ReadInt();
					m_DayTime = reader.ReadDateTime();
					m_WeekTime = reader.ReadDateTime();
					m_DeathCheck = reader.ReadInt();
					m_FarmTime = reader.ReadDateTime();
					m_Level = reader.ReadInt();
					m_Exp = reader.ReadInt();
					m_FarmCheck = reader.ReadInt();					
					goto case 40;
				}
                case 40: // Version 40, moved gauntlet points, virtua artys and TOT turn ins to PointsSystem
                case 39: // Version 39, removed ML quest save/load
                case 38:
                    NextGemOfSalvationUse = reader.ReadDateTime();
                    goto case 37;
                case 37:
                    m_ExtendedFlags = (ExtendedPlayerFlag)reader.ReadInt();
				    goto case 36;
                case 36:
                    RewardStableSlots = reader.ReadInt();
                    goto case 35;
                case 35: // Siege Blessed Item
                    _BlessedItem = reader.ReadItem();
                    goto case 34;
                    // Version 34 - new BOD System
                case 34:
                case 33:
                    {
                        ExploringTheDeepQuest = (ExploringTheDeepQuestChain)reader.ReadInt();
                        goto case 31;
                    }
                case 32:
                case 31:
                    {
                        DisplayGuildTitle = version > 31 && reader.ReadBool();
                        m_FameKarmaTitle = reader.ReadString();
                        m_PaperdollSkillTitle = reader.ReadString();
                        m_OverheadTitle = reader.ReadString();
                        m_SubtitleSkillTitle = reader.ReadString();

                        m_CurrentChampTitle = reader.ReadString();
                        m_CurrentVeteranTitle = reader.ReadInt();
                        goto case 30;
                    }
                case 30: goto case 29;
				case 29:
					{
                        if (version < 40)
                        {
                            PointsSystem.DoomGauntlet.SetPoints(this, reader.ReadDouble());
                        }

						m_SSNextSeed = reader.ReadDateTime();
						m_SSSeedExpire = reader.ReadDateTime();
						m_SSSeedLocation = reader.ReadPoint3D();
						m_SSSeedMap = reader.ReadMap();

                        if (version < 30)
                        {
                            reader.ReadLong(); // Old m_LevelExp
                            int points = (int)reader.ReadLong();
                            if (points > 0)
                            {
                                Server.Engines.Points.PointsSystem.QueensLoyalty.ConvertFromOldSystem(this, points);
                            }

                            reader.ReadInt(); // Old m_Level
                            reader.ReadString(); // Old m_ExpTitle
                        }

                        if (version < 40)
                        {
                            PointsSystem.VirtueArtifacts.SetPoints(this, reader.ReadInt());
                        }

                        if (version < 39)
                        {
                            List<BaseQuest> quests = QuestReader.Quests(reader, this);
                            Dictionary<QuestChain, BaseChain> dic = QuestReader.Chains(reader);

                            if (quests != null && quests.Count > 0)
                                MondainQuestData.QuestData[this] = quests;

                            if (dic != null && dic.Count > 0)
                                MondainQuestData.ChainData[this] = dic;
                        }

                        m_Collections = new Dictionary<Collection, int>();
						m_RewardTitles = new List<object>();

						for (int i = reader.ReadInt(); i > 0; i--)
						{
							m_Collections.Add((Collection)reader.ReadInt(), reader.ReadInt());
						}

						for (int i = reader.ReadInt(); i > 0; i--)
						{
							m_RewardTitles.Add(QuestReader.Object(reader));
						}

						m_SelectedTitle = reader.ReadInt();

						goto case 28;
					}
				case 28:
					{
						m_PeacedUntil = reader.ReadDateTime();

						goto case 27;
					}
				case 27:
					{
						m_AnkhNextUse = reader.ReadDateTime();

						goto case 26;
					}
				case 26:
					{
						m_AutoStabled = reader.ReadStrongMobileList();

						goto case 25;
					}
				case 25:
					{
						int recipeCount = reader.ReadInt();

						if (recipeCount > 0)
						{
							m_AcquiredRecipes = new Dictionary<int, bool>();

							for (int i = 0; i < recipeCount; i++)
							{
								int r = reader.ReadInt();
								if (reader.ReadBool()) //Don't add in recipies which we haven't gotten or have been removed
								{
									m_AcquiredRecipes.Add(r, true);
								}
							}
						}
						goto case 24;
					}
				case 24:
					{
						m_LastHonorLoss = reader.ReadDeltaTime();
						goto case 23;
					}
				case 23:
					{
						m_ChampionTitles = new ChampionTitleInfo(reader);
						goto case 22;
					}
				case 22:
					{
						m_LastValorLoss = reader.ReadDateTime();
						goto case 21;
					}
				case 21:
					{
                        if (version < 40)
                        {
                            PointsSystem.TreasuresOfTokuno.Convert(this, reader.ReadEncodedInt(), reader.ReadInt());
                        }
						goto case 20;
					}
				case 20:
					{
						m_AllianceMessageHue = reader.ReadEncodedInt();
						m_GuildMessageHue = reader.ReadEncodedInt();

						goto case 19;
					}
				case 19:
					{
						int rank = reader.ReadEncodedInt();
						int maxRank = RankDefinition.Ranks.Length - 1;
						if (rank > maxRank)
						{
							rank = maxRank;
						}

						m_GuildRank = RankDefinition.Ranks[rank];
						m_LastOnline = reader.ReadDateTime();
						goto case 18;
					}
				case 18:
					{
						m_SolenFriendship = (SolenFriendship)reader.ReadEncodedInt();

						goto case 17;
					}
				case 17: // changed how DoneQuests is serialized
				case 16:
					{
						m_Quest = QuestSerializer.DeserializeQuest(reader);

						if (m_Quest != null)
						{
							m_Quest.From = this;
						}

						int count = reader.ReadEncodedInt();

						if (count > 0)
						{
							m_DoneQuests = new List<QuestRestartInfo>();

							for (int i = 0; i < count; ++i)
							{
								Type questType = QuestSerializer.ReadType(QuestSystem.QuestTypes, reader);
								DateTime restartTime;

								if (version < 17)
								{
									restartTime = DateTime.MaxValue;
								}
								else
								{
									restartTime = reader.ReadDateTime();
								}

								m_DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
							}
						}

						m_Profession = reader.ReadEncodedInt();
						goto case 15;
					}
				case 15:
					{
						m_LastCompassionLoss = reader.ReadDeltaTime();
						goto case 14;
					}
				case 14:
					{
						m_CompassionGains = reader.ReadEncodedInt();

						if (m_CompassionGains > 0)
						{
							m_NextCompassionDay = reader.ReadDeltaTime();
						}

						goto case 13;
					}
				case 13: // just removed m_PayedInsurance list
				case 12:
					{
                        if(version < 34)
						    BulkOrderSystem.SetBOBFilter(this, new BOBFilter(reader));
						goto case 11;
					}
				case 11:
					{
						if (version < 13)
						{
							var payed = reader.ReadStrongItemList();

							for (int i = 0; i < payed.Count; ++i)
							{
								payed[i].PayedInsurance = true;
							}
						}

						goto case 10;
					}
				case 10:
					{
						if (reader.ReadBool())
						{
							m_HairModID = reader.ReadInt();
							m_HairModHue = reader.ReadInt();
							m_BeardModID = reader.ReadInt();
							m_BeardModHue = reader.ReadInt();
						}

						goto case 9;
					}
				case 9:
					{
						SavagePaintExpiration = reader.ReadTimeSpan();

						if (SavagePaintExpiration > TimeSpan.Zero)
						{
							BodyMod = (Female ? 184 : 183);
							HueMod = 0;
						}

						goto case 8;
					}
				case 8:
					{
						m_NpcGuild = (NpcGuild)reader.ReadInt();
						m_NpcGuildJoinTime = reader.ReadDateTime();
						m_NpcGuildGameTime = reader.ReadTimeSpan();
						goto case 7;
					}
				case 7:
					{
						m_PermaFlags = reader.ReadStrongMobileList();
						goto case 6;
					}
				case 6:
					{
                        if(version < 34)
						    reader.ReadTimeSpan();
						goto case 5;
					}
				case 5:
					{
                        if(version < 34)
						    reader.ReadTimeSpan();
						goto case 4;
					}
				case 4:
					{
						m_LastJusticeLoss = reader.ReadDeltaTime();
						m_JusticeProtectors = reader.ReadStrongMobileList();
						goto case 3;
					}
				case 3:
					{
						m_LastSacrificeGain = reader.ReadDeltaTime();
						m_LastSacrificeLoss = reader.ReadDeltaTime();
						m_AvailableResurrects = reader.ReadInt();
						goto case 2;
					}
				case 2:
					{
						m_Flags = (PlayerFlag)reader.ReadInt();
						goto case 1;
					}
				case 1:
					{
						m_LongTermElapse = reader.ReadTimeSpan();
						m_ShortTermElapse = reader.ReadTimeSpan();
						m_GameTime = reader.ReadTimeSpan();
						goto case 0;
					}
				case 0:
					{
						if (version < 26)
						{
							m_AutoStabled = new List<Mobile>();
						}
						break;
					}
			}
			
			if (version < 29)
			{
				m_SSNextSeed = m_SSSeedExpire = DateTime.UtcNow;
				m_SSSeedLocation = Point3D.Zero;
			}

			if (m_RecentlyReported == null)
			{
				m_RecentlyReported = new List<Mobile>();
			}

			#region Mondain's Legacy
			/*if (m_Quests == null)
			{
				m_Quests = new List<BaseQuest>();
			}

			if (m_Chains == null)
			{
				m_Chains = new Dictionary<QuestChain, BaseChain>();
			}*/

			if (m_DoneQuests == null)
			{
				m_DoneQuests = new List<QuestRestartInfo>();
			}

			if (m_Collections == null)
			{
				m_Collections = new Dictionary<Collection, int>();
			}

			if (m_RewardTitles == null)
			{
				m_RewardTitles = new List<object>();
			}
			#endregion

			// Professions weren't verified on 1.0 RC0
			if (!CharacterCreation.VerifyProfession(m_Profession))
			{
				m_Profession = 0;
			}

			if (m_PermaFlags == null)
			{
				m_PermaFlags = new List<Mobile>();
			}

			if (m_JusticeProtectors == null)
			{
				m_JusticeProtectors = new List<Mobile>();
			}

			if (m_GuildRank == null)
			{
				m_GuildRank = RankDefinition.Member;
				//Default to member if going from older version to new version (only time it should be null)
			}

			if (m_LastOnline == DateTime.MinValue && Account != null)
			{
				m_LastOnline = ((Account)Account).LastLogin;
			}

			if (m_ChampionTitles == null)
			{
				m_ChampionTitles = new ChampionTitleInfo();
			}

			var list = Stabled;

			for (int i = 0; i < list.Count; ++i)
			{
				BaseCreature bc = list[i] as BaseCreature;

				if (bc != null)
				{
					bc.IsStabled = true;
					bc.StabledBy = this;
				}
			}

			CheckAtrophies(this);

			if (Hidden) //Hiding is the only buff where it has an effect that's serialized.
			{
				AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
			}

			if (_BlessedItem != null)
			{
				Timer.DelayCall(
				b =>
				{
					if (_BlessedItem == b && b.RootParent != this)
					{
						_BlessedItem = null;
					}
				},
				_BlessedItem);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			//cleanup our anti-macro table
			foreach (Hashtable t in m_AntiMacroTable.Values)
			{
				ArrayList remove = new ArrayList();
				foreach (CountAndTimeStamp time in t.Values)
				{
					if (time.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
					{
						remove.Add(time);
					}
				}

				for (int i = 0; i < remove.Count; ++i)
				{
					t.Remove(remove[i]);
				}
			}

			CheckKillDecay();
            CheckAtrophies(this);

			base.Serialize(writer);

			writer.Write(60); // version

			for (int i = 0; i < 100; i++)
			{
				writer.Write( (int) m_ItemSetOption[i] );
				writer.Write( (int) m_ItemSetValue[i] );
			}			
			for (int i = 0; i < 500; i++)
			{
				writer.Write( (int) m_ItemSetSaveValue[i] );
			}			

			
			writer.Write( (bool) m_EquipMeltingOptionBag );

			for (int i = 0; i < 6; i++)
			{
				writer.Write( (bool) m_EquipMeltingOptionRank[i] );
			}			
			for (int i = 0; i < 7; i++)
			{
				writer.Write( (bool) m_EquipMeltingOptionTier[i] );
			}			
			for (int i = 0; i < 2; i++)
			{
				writer.Write( (bool) m_EquipMeltingOptionNamed[i] );
			}
			
			writer.Write( (Item)m_AutoFood);
			writer.Write( (DateTime)m_Another_Land );
			
			
			for (int i = 0; i < 1000; i++)
			{
				writer.Write( (int) m_HarvestPoint[i] );
				writer.Write( (int) m_CraftPoint[i] );
			}
			
			
			for (int i = 0; i < 50000; i++)
			{
				writer.Write( (bool) m_QuestCheck[i] );
			}			
			
			for (int i = 0; i < 100; i++)
			{
				writer.Write( (string)m_MonsterSave[i] );
			}
			//m_ItemSave, m_MonsterSave, m_PlayerSave, m_PetSave
			for (int i = 0; i < 100; i++)
			{
				writer.Write( (Item)m_ItemSave[i] );
				writer.Write( (Mobile)m_PlayerSave[i] );
				writer.Write( (Mobile)m_PetSave[i] );
			}
			for (int i = 0; i < 50000; i++)
			{
				writer.Write( (int) m_MonsterPoint[i] );
			}			

			for (int i = 0; i < 100; i++)
			{
				writer.Write( (bool) m_BuffCheck[i] );
			}
			for (int i = 0; i < 10000; i++)
			{
				writer.Write( (bool) m_StatReset[i] );
			}

			writer.Write( (double) m_Tired );
			
			writer.Write( (bool) m_TodayLogin );

			writer.Write( (bool) m_EquiptoDungeon );
			
			for (int i = 20; i < 50; i++)
			{
				writer.Write( (int) m_GoldPoint[i] );
			}

			writer.Write( (bool) m_GoldAndSilverPointReturntoSkill );

			writer.Write( (bool) m_AnatomytoTasteID );
			
			writer.Write( (bool) m_FreeSkill );
			writer.Write( (DateTime) m_WeekTime );
			
			writer.Write( (int) m_ActionPoint );
			writer.Write( (bool) m_FirstSkill );

			for (int i = 0; i < 20; i++)
			{
				writer.Write( (int) m_GoldPoint[i] );
			}
			for (int i = 0; i < 100; i++)
			{
				writer.Write( (int) m_SilverPoint[i] );
			}
			for (int i = 0; i < 40; i++)
			{
				writer.Write( (int) m_EquipPoint[i] );
			}
			for (int i = 0; i < 80; i++)
			{
				writer.Write( (int) m_ArtifactPoint[i] );
			}			
			
			for (int i = 0; i < 7; i++)
			{
				writer.Write( (int) m_SkillbyStat[i] );
			}			
			
			for (int i = 0; i < 9; i++)
			{
				writer.Write( (int) m_City[i] );
			}

			writer.Write( (int) m_StamTimeUp );
			
			writer.Write( (DateTime) m_MoongateTime );

			writer.Write( (int) m_ShipCheck );

			writer.Write( (DateTime) m_YoungTime );
			
			for (int i = 0; i < 100; i++)
			{
				writer.Write( (int) m_TimerList[i] );
			}
			writer.Write( (int) m_SaveTown );
			//writer.Write( (int) m_MonsterCombatCheck );
			//writer.Write( (int) m_PlayerCombatCheck );
			writer.Write( (int) m_GatePoint );
			writer.Write( (int) m_PlayerPoint );

			for (int i = 0; i < 58; i++)
			{
				writer.Write( (double) m_SkillList[i] );
			}

			writer.Write( (int) m_DungeonCheck );
			writer.Write( (int) m_DungeonFear );
			writer.Write( (bool) m_Coma );
			//writer.Write( (int) m_ComaTime );
			writer.Write( (DateTime) m_DayTime );
			writer.Write( (DateTime) m_WeekTime );
			writer.Write( (int) m_DeathCheck );
			writer.Write( (DateTime) m_FarmTime );
			writer.Write( (int) m_Level );
			writer.Write( (int) m_Exp );
			writer.Write( (int) m_FarmCheck );

            writer.Write((DateTime)NextGemOfSalvationUse);

            writer.Write((int)m_ExtendedFlags);

            writer.Write(RewardStableSlots);
			
			if (_BlessedItem != null && _BlessedItem.RootParent != this)
			{
				_BlessedItem = null;
			}

            writer.Write(_BlessedItem);

            writer.Write((int)ExploringTheDeepQuest);

            // Version 31/32 Titles
            writer.Write(DisplayGuildTitle);
            writer.Write(m_FameKarmaTitle);
            writer.Write(m_PaperdollSkillTitle);
            writer.Write(m_OverheadTitle);
            writer.Write(m_SubtitleSkillTitle);
            writer.Write(m_CurrentChampTitle);
            writer.Write(m_CurrentVeteranTitle);

            // Version 30 open to take out old Queens Loyalty Info

			#region Plant System
			writer.Write(m_SSNextSeed);
			writer.Write(m_SSSeedExpire);
			writer.Write(m_SSSeedLocation);
			writer.Write(m_SSSeedMap);
			#endregion

            #region Mondain's Legacy

            if (m_Collections == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(m_Collections.Count);

				foreach (var pair in m_Collections)
				{
					writer.Write((int)pair.Key);
					writer.Write(pair.Value);
				}
			}

			if (m_RewardTitles == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(m_RewardTitles.Count);

				for (int i = 0; i < m_RewardTitles.Count; i++)
				{
					QuestWriter.Object(writer, m_RewardTitles[i]);
				}
			}

			writer.Write(m_SelectedTitle);
			#endregion

			// Version 28
			writer.Write(m_PeacedUntil);
			writer.Write(m_AnkhNextUse);
			writer.Write(m_AutoStabled, true);

			if (m_AcquiredRecipes == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(m_AcquiredRecipes.Count);

				foreach (var kvp in m_AcquiredRecipes)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}
			}

			writer.WriteDeltaTime(m_LastHonorLoss);

			ChampionTitleInfo.Serialize(writer, m_ChampionTitles);

			writer.Write(m_LastValorLoss);

			writer.WriteEncodedInt(m_AllianceMessageHue);
			writer.WriteEncodedInt(m_GuildMessageHue);

			writer.WriteEncodedInt(m_GuildRank.Rank);
			writer.Write(m_LastOnline);

			writer.WriteEncodedInt((int)m_SolenFriendship);

			QuestSerializer.Serialize(m_Quest, writer);

			if (m_DoneQuests == null)
			{
				writer.WriteEncodedInt(0);
			}
			else
			{
				writer.WriteEncodedInt(m_DoneQuests.Count);

				for (int i = 0; i < m_DoneQuests.Count; ++i)
				{
					QuestRestartInfo restartInfo = m_DoneQuests[i];

					QuestSerializer.Write(restartInfo.QuestType, QuestSystem.QuestTypes, writer);
					writer.Write(restartInfo.RestartTime);
				}
			}

			writer.WriteEncodedInt(m_Profession);

			writer.WriteDeltaTime(m_LastCompassionLoss);

			writer.WriteEncodedInt(m_CompassionGains);

			if (m_CompassionGains > 0)
			{
				writer.WriteDeltaTime(m_NextCompassionDay);
			}

			bool useMods = (m_HairModID != -1 || m_BeardModID != -1);

			writer.Write(useMods);

			if (useMods)
			{
				writer.Write(m_HairModID);
				writer.Write(m_HairModHue);
				writer.Write(m_BeardModID);
				writer.Write(m_BeardModHue);
			}

			writer.Write(SavagePaintExpiration);

			writer.Write((int)m_NpcGuild);
			writer.Write(m_NpcGuildJoinTime);
			writer.Write(m_NpcGuildGameTime);

			writer.Write(m_PermaFlags, true);

			writer.WriteDeltaTime(m_LastJusticeLoss);
			writer.Write(m_JusticeProtectors, true);

			writer.WriteDeltaTime(m_LastSacrificeGain);
			writer.WriteDeltaTime(m_LastSacrificeLoss);
			writer.Write(m_AvailableResurrects);

			writer.Write((int)m_Flags);

			writer.Write(m_LongTermElapse);
			writer.Write(m_ShortTermElapse);
			writer.Write(GameTime);
		}

		public static void CheckAtrophies(Mobile m)
		{
			SacrificeVirtue.CheckAtrophy(m);
			JusticeVirtue.CheckAtrophy(m);
			CompassionVirtue.CheckAtrophy(m);
			ValorVirtue.CheckAtrophy(m);

			if (m is PlayerMobile)
			{
				ChampionTitleInfo.CheckAtrophy((PlayerMobile)m);
			}
		}

		public void CheckKillDecay()
		{
			if (m_ShortTermElapse < GameTime)
			{
				m_ShortTermElapse += TimeSpan.FromHours(8);
				if (ShortTermMurders > 0)
				{
					--ShortTermMurders;
				}
			}

			if (m_LongTermElapse < GameTime)
			{
				m_LongTermElapse += TimeSpan.FromHours(40);
				if (Kills > 0)
				{
					--Kills;
				}
			}
		}

		public void ResetKillTime()
		{
			m_ShortTermElapse = GameTime + TimeSpan.FromHours(8);
			m_LongTermElapse = GameTime + TimeSpan.FromHours(40);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SessionStart { get { return m_SessionStart; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan GameTime
		{
			get
			{
				if (NetState != null)
				{
					return m_GameTime + (DateTime.UtcNow - m_SessionStart);
				}
				else
				{
					return m_GameTime;
				}
			}
		}

		public override bool CanSee(Mobile m)
		{
            if (m is IConditionalVisibility && !((IConditionalVisibility)m).CanBeSeenBy(this))
                return false;

			if (m is CharacterStatue)
			{
				((CharacterStatue)m).OnRequestedAnimation(this);
			}

			if (m is PlayerMobile && ((PlayerMobile)m).m_VisList.Contains(this))
			{
				return true;
			}

			return base.CanSee(m);
		}

		public override bool CanSee(Item item)
		{
            if (item is IConditionalVisibility && !((IConditionalVisibility)item).CanBeSeenBy(this))
                return false;

			if (m_DesignContext != null && m_DesignContext.Foundation.IsHiddenToCustomizer(this, item))
			{
				return false;
			}
            else if (AccessLevel == AccessLevel.Player)
            {
                Region r = item.GetRegion();

                if (r is BaseRegion && !((BaseRegion)r).CanSee(this, item))
                {
                    return false;
                }
            }

			return base.CanSee(item);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Instances.Remove(this);

			Faction faction = Faction.Find(this);

			if (faction != null)
			{
				faction.RemoveMember(this);
			}

			BaseHouse.HandleDeletion(this);

			DisguiseTimers.RemoveTimer(this);
		}

		public override bool NewGuildDisplay { get { return Guilds.Guild.NewGuildSystem; } }

		public delegate void PlayerPropertiesEventHandler(PlayerPropertiesEventArgs e);

		public static event PlayerPropertiesEventHandler PlayerProperties;

		public class PlayerPropertiesEventArgs : EventArgs
		{
			public PlayerMobile Player = null;
			public ObjectPropertyList PropertyList = null;

			public PlayerPropertiesEventArgs(PlayerMobile player, ObjectPropertyList list)
			{
				Player = player;
				PropertyList = list;
			}
		}
		public int Fury = 0;
		public bool FuryActive = false;
		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);
			string town = "없음";
			string goodfood = "없음";
			string bedfood = "없음";
			if( Region.IsPartOf( "Covetous" ) )
				town = "던전 - 코베투스";
			else if( m_SaveTown == 1 )
				town = "마을 - 브리튼";
			else if( m_SaveTown == 2 )
				town = "마을 - 부케니어스 덴";
			else if( m_SaveTown == 3 )
				town = "마을 - 코브";
			else if( m_SaveTown == 4 )
				town = "마을 - 젤롬";
			else if( m_SaveTown == 5 )
				town = "마을 - 마진시아";
			else if( m_SaveTown == 6 )
				town = "마을 - 미녹";
			else if( m_SaveTown == 7 )
				town = "마을 - 문글로우";
			else if( m_SaveTown == 8 )
				town = "마을 - 누젤롬";
			else if( m_SaveTown == 9 )
				town = "마을 - 서펜트 홀드";
			else if( m_SaveTown == 10 )
				town = "마을 - 스카라 브레";
			else if( m_SaveTown == 11 )
				town = "마을 - 트린식";
			else if( m_SaveTown == 12 )
				town = "마을 - 베스퍼";
			else if( m_SaveTown == 13 )
				town = "마을 - 유";
			else if( m_SaveTown == 14 )
				town = "마을 - 뉴 헤이븐";

			//list.Add( 1060658,  "배고픔\t{0}% \n 먹고 싶은 음식: {1} \n 먹기 싫은 음식: {2} \n 활동 포인트: {3} / 10000 \n 저장된 장소: {4}", 100 - this.Hunger, goodfood, bedfood, m_PlayerPoint, town );

			//배고픔 체크
			
			/*
			int Maxhunger = 50000;
			if( Skills.TasteID.Value >= 100 )
				Maxhunger += 15000 + (int)( Skills.TasteID.Value * 500 );
			else if( Skills.TasteID.Value > 0 )
				Maxhunger += (int)( Skills.TasteID.Value * 500 );

			int hungerPrecent = ( Maxhunger - Hunger ) * 100 / Maxhunger;

			if ( hungerPrecent >= 95 )
				list.Add( 1060658, "배고픔\t{0}", "<basefont color=#FF0000>치명적인 상태<basefont color=#FFFFFF>" );
			else if ( hungerPrecent >= 60 )
				list.Add( 1060658, "배고픔\t{0}", "<basefont color=#FF5E00>배고픈 상태<basefont color=#FFFFFF>" );
			else if ( hungerPrecent >= 30 )
				list.Add( 1060658, "배고픔\t{0}", "<basefont color=#68D5ED>조금 배고픈 상태<basefont color=#FFFFFF>" );
			else if (hungerPrecent >= 10 )
				list.Add( 1060658, "배고픔\t{0}", "<basefont color=#00A000>활발한 상태<basefont color=#FFFFFF>" );
			*/
			/*			
			
			if( Hunger > 10000 )
				ex_hunger = ( ( Hunger - 10000 ) * 0.01 ) / 10;
			else
				hunger = (10000 - Hunger) * 0.01;
			
			if( ex_hunger > 0 )
			{
				//if( DeathCheck != 0 )
					list.Add( 1060658,  "과식\t{0}% \n 저장된 장소: {1}", ex_hunger, town );
				//else
				//	list.Add( 1060658,  "과식\t{0}% \n 저장된 장소: {1} \n <basefont color=#FF0000>보호 모드 발동<basefont color=#FFFFFF>", ex_hunger, town );
			}
			else
			{
				//if( DeathCheck != 0 )
					list.Add( 1060658,  "배고픔\t{0}% \n 저장된 장소: {1}", hunger, town );
				//else
				//	list.Add( 1060658,  "배고픔\t{0}% \n 저장된 장소: {1} \n <basefont color=#FF0000>보호 모드 발동<basefont color=#FFFFFF>", hunger, town );
			}
			//피로도 체크
			if ( m_Tired >= 20000 )
				list.Add( 1060659, "피로도\t{0}", "<basefont color=#FF0000>치명적인 상태<basefont color=#FFFFFF>" );
			else if ( m_Tired >= 10000 )
				list.Add( 1060659, "피로도\t{0}", "<basefont color=#FF5E00>피곤한 상태<basefont color=#FFFFFF>" );
			else if ( m_Tired >= 5000 )
				list.Add( 1060659, "피로도\t{0}", "<basefont color=#68D5ED>조금 피곤한 상태<basefont color=#FFFFFF>" );
			else if (m_Tired >= 2500 )
				list.Add( 1060659, "피로도\t{0}", "<basefont color=#00A000>활발한 상태<basefont color=#FFFFFF>" );
			*/
			
			/*
			string free = "";
			if( m_MoongateTime > DateTime.Now )
				free = " + 사망 회복 중";
			
			if( DeathCheck >= 3 )
				list.Add( 1060659, "사망 보호 횟수\t{0}" + free, m_DeathCheck );
			else if( DeathCheck > 0 ) 
				list.Add( 1060659, "<basefont color=#FF5E00>사망 보호 횟수\t{0}<basefont color=#FFFFFF>" + free, m_DeathCheck );
			//else
			//	list.Add( 1060659, "<basefont color=#FF0000><basefont color=#FFFFFF>" , m_DeathCheck );

			if ( DungeonFear >= 36 )
				list.Add( 1060660, "던전 공포\t{0}", "<basefont color=#FF0000>치명적인 상태<basefont color=#FFFFFF>" );
			else if ( DungeonFear >= 28 )
				list.Add( 1060660, "던전 공포\t{0}", "<basefont color=#FF5E00>두려운 상태<basefont color=#FFFFFF>" );
			else if ( DungeonFear >= 24 )
				list.Add( 1060660, "던전 공포\t{0}", "조금 두려운 상태" );

			//활동 포인트
			//list.Add( 1060661, "활동 포인트\t{0}", /* 스킬: {2}" , m_ActionPoint/* , SkillFollows */ //);

			//추종자 정보
			//list.Add( 1060661, "장비\t{0}, 펫: {1}", /* 스킬: {2}" , */EquipFollows, PetFollows/* , SkillFollows */ );
			
			//else
			//	list.Add( 1060661, "레벨\t{0}, 요구 경험치: {1}", m_Level, Levels.ExpCal( this ) );

            if (Core.SA)
            {
                if (m_SubtitleSkillTitle != null)
                    list.Add(1042971, m_SubtitleSkillTitle);

                if (m_CurrentVeteranTitle > 0)
                    list.Add(m_CurrentVeteranTitle);
            }

			#region Mondain's Legacy Titles
			if (Core.ML && m_RewardTitles != null && m_SelectedTitle > -1)
			{
				if (m_SelectedTitle < m_RewardTitles.Count)
				{
					if (m_RewardTitles[m_SelectedTitle] is int)
					{
                        string cust = null;

                        if ((int)m_RewardTitles[m_SelectedTitle] == 1154017 && CityLoyaltySystem.HasCustomTitle(this, out cust))
                        {
                            list.Add(1154017, cust); // ~1_TITLE~ of ~2_CITY~
                        }
						else
                            list.Add((int)m_RewardTitles[m_SelectedTitle]);
					}
					else if (m_RewardTitles[m_SelectedTitle] is string)
					{
                        list.Add(1070722, (string)m_RewardTitles[m_SelectedTitle]);
					}
				}
			}
			#endregion

			if (Map == Faction.Facet)
			{
				PlayerState pl = PlayerState.Find(this);

				if (pl != null)
				{
					Faction faction = pl.Faction;

					if (faction.Commander == this)
					{
						list.Add(1042733, faction.Definition.PropName); // Commanding Lord of the ~1_FACTION_NAME~
					}
					else if (pl.Sheriff != null)
					{
						list.Add(1042734, "{0}\t{1}", pl.Sheriff.Definition.FriendlyName, faction.Definition.PropName);
						// The Sheriff of  ~1_CITY~, ~2_FACTION_NAME~
					}
					else if (pl.Finance != null)
					{
						list.Add(1042735, "{0}\t{1}", pl.Finance.Definition.FriendlyName, faction.Definition.PropName);
						// The Finance Minister of ~1_CITY~, ~2_FACTION_NAME~
					}
					else if (pl.MerchantTitle != MerchantTitle.None)
					{
						list.Add(1060776, "{0}\t{1}", MerchantTitles.GetInfo(pl.MerchantTitle).Title, faction.Definition.PropName);
						// ~1_val~, ~2_val~
					}
					else
					{
						list.Add(1060776, "{0}\t{1}", pl.Rank.Title, faction.Definition.PropName); // ~1_val~, ~2_val~
					}
				}
			}

			if (Core.ML)
			{
				for (int i = AllFollowers.Count - 1; i >= 0; i--)
				{
					BaseCreature c = AllFollowers[i] as BaseCreature;

					if (c != null && c.ControlOrder == OrderType.Guard)
					{
						list.Add(501129); // guarded
						break;
					}
				}
			}

            if (TestCenter.Enabled && Core.TOL)
            {
                Server.Engines.VvV.VvVPlayerEntry entry = Server.Engines.Points.PointsSystem.ViceVsVirtue.GetPlayerEntry<Server.Engines.VvV.VvVPlayerEntry>(this);

                list.Add(String.Format("Kills: {0} / Deaths: {1} / Assists: {2}", // no cliloc for this!
                    entry == null ? "0" : entry.Kills.ToString(), entry == null ? "0" : entry.Deaths.ToString(), entry == null ? "0" : entry.Assists.ToString()));

                list.Add(1060415, AosAttributes.GetValue(this, AosAttribute.AttackChance).ToString()); // hit chance increase ~1_val~%
                list.Add(1060408, AosAttributes.GetValue(this, AosAttribute.DefendChance).ToString()); // defense chance increase ~1_val~%
                list.Add(1060486, AosAttributes.GetValue(this, AosAttribute.WeaponSpeed).ToString()); // swing speed increase ~1_val~%
                list.Add(1060401, AosAttributes.GetValue(this, AosAttribute.WeaponDamage).ToString()); // damage increase ~1_val~%
                list.Add(1060483, AosAttributes.GetValue(this, AosAttribute.SpellDamage).ToString()); // spell damage increase ~1_val~%
                list.Add(1060433, AosAttributes.GetValue(this, AosAttribute.LowerManaCost).ToString()); // lower mana cost
            }

			if (PlayerProperties != null)
			{
				PlayerProperties(new PlayerPropertiesEventArgs(this, list));
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (Map == Faction.Facet)
			{
				PlayerState pl = PlayerState.Find(this);

				if (pl != null)
				{
					string text;
					bool ascii = false;

					Faction faction = pl.Faction;

					if (faction.Commander == this)
					{
						text = String.Concat(
							Female ? "(Commanding Lady of the " : "(Commanding Lord of the ", faction.Definition.FriendlyName, ")");
					}
					else if (pl.Sheriff != null)
					{
						text = String.Concat(
							"(The Sheriff of ", pl.Sheriff.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
					}
					else if (pl.Finance != null)
					{
						text = String.Concat(
							"(The Finance Minister of ", pl.Finance.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
					}
					else
					{
						ascii = true;

						if (pl.MerchantTitle != MerchantTitle.None)
						{
							text = String.Concat(
								"(", MerchantTitles.GetInfo(pl.MerchantTitle).Title.String, ", ", faction.Definition.FriendlyName, ")");
						}
						else
						{
							text = String.Concat("(", pl.Rank.Title.String, ", ", faction.Definition.FriendlyName, ")");
						}
					}

					int hue = (Faction.Find(from) == faction ? 98 : 38);

					PrivateOverheadMessage(MessageType.Label, hue, ascii, text, from.NetState);
				}
			}

			base.OnSingleClick(from);
		}

		public int AllowedMeditationSteps = 0;
		
		protected override bool OnMove(Direction d)
		{
            if (Party != null && NetState != null)
            {
                Waypoints.UpdateToParty(this);
            }

			if (!Core.SE)
			{
				return base.OnMove(d);
			}

			if (IsStaff())
			{
				return true;
			}

			/*
			if( Meditating )
			{
				if ( Mounted || Mana < 1)
				{
					DisruptiveAction();
					//Meditating = false;
				}
				else if (AllowedMeditationSteps-- <= 0 && Mana >= 6)
				{
					Meditation.OnUse(this);
				}
				Mana--;
			}
			*/
			if (Hidden && DesignContext.Find(this) == null) //Hidden & NOT customizing a house
			{
				if (!Mounted && AllowedStealthSteps >= 1)
				{
					bool running = (d & Direction.Running) != 0;

					if( Stam < 1 )
						RevealingAction();
					else if (running)
					{
						RevealingAction();
					}
					else if (AllowedStealthSteps-- <= 0 && Stam >= 6)
					{
						Stealth.OnUse(this);
					}
					Stam--;
				}
				else
				{
					RevealingAction();
				}
			}

			#region Mondain's Legacy
			if (InvisibilityPotion.HasTimer(this))
			{
				InvisibilityPotion.Iterrupt(this);
			}
			#endregion

			return true;
		}

		public bool BedrollLogout { get; set; }
        public bool BlanketOfDarknessLogout { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
		public override bool Paralyzed
		{
			get { return base.Paralyzed; }
			set
			{
				base.Paralyzed = value;

				if (value)
				{
					AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827)); //Paralyze/You are frozen and can not move
				}
				else
				{
					RemoveBuff(BuffIcon.Paralyze);
				}
			}
		}

		#region Ethics
		private Player m_EthicPlayer;

		[CommandProperty(AccessLevel.GameMaster)]
		public Player EthicPlayer { get { return m_EthicPlayer; } set { m_EthicPlayer = value; } }
		#endregion

		#region Factions
		public PlayerState FactionPlayerState { get; set; }
		#endregion
        
		#region Quests
		private QuestSystem m_Quest;
		private List<QuestRestartInfo> m_DoneQuests;
		private SolenFriendship m_SolenFriendship;

		public QuestSystem Quest { get { return m_Quest; } set { m_Quest = value; } }

		public List<QuestRestartInfo> DoneQuests { get { return m_DoneQuests; } set { m_DoneQuests = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public SolenFriendship SolenFriendship { get { return m_SolenFriendship; } set { m_SolenFriendship = value; } }
        #endregion

        #region Mondain's Legacy
        /*private List<BaseQuest> m_Quests;
		private Dictionary<QuestChain, BaseChain> m_Chains;

		public List<BaseQuest> Quests { get { return m_Quests; } }
        public Dictionary<QuestChain, BaseChain> Chains { get { return m_Chains; } }*/
        public List<BaseQuest> Quests
        {
            get
            {
                return MondainQuestData.GetQuests(this);
            }
        }

        public Dictionary<QuestChain, BaseChain> Chains
        {
            get
            {
                return MondainQuestData.GetChains(this);
            }
        }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Peaced
		{
			get
			{
				if (m_PeacedUntil > DateTime.UtcNow)
				{
					return true;
				}

				return false;
			}
		}

		private Dictionary<Collection, int> m_Collections;
		private List<object> m_RewardTitles;
		private int m_SelectedTitle;

		public Dictionary<Collection, int> Collections { get { return m_Collections; } }

		public List<object> RewardTitles { get { return m_RewardTitles; } }

        public int SelectedTitle { get { return m_SelectedTitle; } }

        public bool RemoveRewardTitle(object o, bool silent)
        {
            if (m_RewardTitles.Contains(o))
            {
                int i = m_RewardTitles.IndexOf(o);

                if (i == m_SelectedTitle)
                    SelectRewardTitle(-1, silent);
                else if (i > m_SelectedTitle)
                    SelectRewardTitle(m_SelectedTitle - 1, silent);

                m_RewardTitles.Remove(o);

                return true;
            }

            return false;
        }

		public int GetCollectionPoints(Collection collection)
		{
			if (m_Collections == null)
			{
				m_Collections = new Dictionary<Collection, int>();
			}

			int points = 0;

			if (m_Collections.ContainsKey(collection))
			{
				m_Collections.TryGetValue(collection, out points);
			}

			return points;
		}

		public void AddCollectionPoints(Collection collection, int points)
		{
			if (m_Collections == null)
			{
				m_Collections = new Dictionary<Collection, int>();
			}

			if (m_Collections.ContainsKey(collection))
			{
				m_Collections[collection] += points;
			}
			else
			{
				m_Collections.Add(collection, points);
			}
		}

		public void SelectRewardTitle(int num, bool silent = false)
		{
			if (num == -1)
			{
				m_SelectedTitle = num;

                if (!silent) 
					SendLocalizedMessage(1074010); // You elect to hide your Reward Title.
			}
            else if (num < m_RewardTitles.Count && num >= -1)
            {
                if (m_SelectedTitle != num)
                {
                    m_SelectedTitle = num;

                    if (m_RewardTitles[num] is int && !silent)
                    {
                        SendLocalizedMessage(1074008, "#" + (int)m_RewardTitles[num]);
                        // You change your Reward Title to "~1_TITLE~".
                    }
                    else if (m_RewardTitles[num] is string && !silent)
                    {
                        SendLocalizedMessage(1074008, (string)m_RewardTitles[num]); // You change your Reward Title to "~1_TITLE~".
                    }
                }
                else if (!silent)
                {
                    SendLocalizedMessage(1074009); // You decide to leave your title as it is.
                }
            }

			InvalidateProperties();
		}

		public bool AddRewardTitle(object title)
		{
			if (m_RewardTitles == null)
			{
				m_RewardTitles = new List<object>();
			}

			if (title != null && !m_RewardTitles.Contains(title))
			{
				m_RewardTitles.Add(title);

				InvalidateProperties();
				return true;
			}

			return false;
		}

		public void ShowChangeTitle()
		{
			SendGump(new SelectTitleGump(this, m_SelectedTitle));
		}
		#endregion

        #region Titles
        private string m_FameKarmaTitle;
        private string m_PaperdollSkillTitle;
        private string m_SubtitleSkillTitle;
        private string m_CurrentChampTitle;
        private string m_OverheadTitle;
        private int m_CurrentVeteranTitle;

        public string FameKarmaTitle
        {
            get { return m_FameKarmaTitle; }
            set { m_FameKarmaTitle = value; InvalidateProperties(); }
        }

        public string PaperdollSkillTitle
        {
            get { return m_PaperdollSkillTitle; }
            set { m_PaperdollSkillTitle = value; InvalidateProperties(); }
        }

        public string SubtitleSkillTitle
        {
            get { return m_SubtitleSkillTitle; }
            set { m_SubtitleSkillTitle = value; InvalidateProperties(); }
        }

        public string CurrentChampTitle
        {
            get { return m_CurrentChampTitle; }
            set { m_CurrentChampTitle = value; InvalidateProperties(); }
        }

        public string OverheadTitle
        {
            get { return m_OverheadTitle; }
            set { m_OverheadTitle = value; InvalidateProperties(); }
        }

        public int CurrentVeteranTitle
        {
            get { return m_CurrentVeteranTitle; }
            set { m_CurrentVeteranTitle = value; InvalidateProperties(); }
        }

		public override bool ShowAccessTitle
		{
			get
			{
				switch (AccessLevel)
				{
					case AccessLevel.VIP:
					case AccessLevel.Counselor:
					case AccessLevel.GameMaster:
					case AccessLevel.Seer:
						return true;
				}

				return false;
			}
		}

        public override void AddNameProperties(ObjectPropertyList list)
        {           
            string prefix = "";

            if (ShowFameTitle && Fame >= 10000)
            {
                prefix = Female ? "Lady" : "Lord";
            }

            string suffix = "";

            if (PropertyTitle && Title != null && Title.Length > 0)
            {
                suffix = Title;
            }

            BaseGuild guild = Guild;
			/*
            bool vvv = Server.Engines.VvV.ViceVsVirtueSystem.IsVvV(this) && (ViceVsVirtueSystem.EnhancedRules || this.Map == Faction.Facet);

            if (m_OverheadTitle != null)
            {
                if (vvv)
                {
                    suffix = "[VvV]";
                }
                else
                {
                    int loc = Utility.ToInt32(m_OverheadTitle);

                    if (loc > 0)
                    {
                        if (CityLoyaltySystem.ApplyCityTitle(this, list, prefix, loc))
                            return;
                    }
                    else if (suffix.Length > 0)
                    {
                        suffix = String.Format("{0} {1}", suffix, m_OverheadTitle);
                    }
                    else
                    {
                        suffix = String.Format("{0}", m_OverheadTitle);
                    }
                }
            }
            else if (guild != null && DisplayGuildAbbr)
            {
                if (vvv)
                {
                    suffix = String.Format("[{0}] [VvV]", Utility.FixHtml(guild.Abbreviation));
                }
                else if (suffix.Length > 0)
                {
                    suffix = String.Format("{0} [{1}]", suffix, Utility.FixHtml(guild.Abbreviation));
                }
                else
                {
                    suffix = String.Format("[{0}]", Utility.FixHtml(guild.Abbreviation));
                }
            }
            else if (vvv)
            {
                suffix = "[VvV]";
            }
			*/
            suffix = ApplyNameSuffix(suffix);
			string name = Name;

            list.Add(1050045, "{0} \t{1}\t {2}", prefix, name, suffix); // ~1_PREFIX~~2_NAME~~3_SUFFIX~

            if (guild != null && DisplayGuildTitle)
            {
                string title = GuildTitle;

                if (title == null)
                {
                    title = "";
                }
                else
                {
                    title = title.Trim();
                }

                if (title.Length > 0)
                {
                    list.Add("{0}, {1}", Utility.FixHtml(title), Utility.FixHtml(guild.Name));
                }
            }
        }
        #endregion

		public override void OnKillsChange(int oldValue)
		{
			if (Young && Kills > oldValue)
			{
				Account acc = Account as Account;

				if (acc != null)
				{
					acc.RemoveYoungStatus(0);
				}
			}
		}

		public override void OnKarmaChange(int oldValue)
		{
            EpiphanyHelper.OnKarmaChange(this);
		}

		public override void OnSkillChange(SkillName skill, double oldBase)
		{
			if (Young)
			{
                if (SkillsTotal >= 4500 && (!Core.AOS && Skills[skill].Base >= 80.0))
                {
                    Account acc = Account as Account;

                    if (acc != null)
                    {
                        acc.RemoveYoungStatus(1019036);
                        // You have successfully obtained a respectable skill level, and have outgrown your status as a young player!
                    }
                }
			}

            if (skill != SkillName.Alchemy && Skills.CurrentMastery == skill && Skills[skill].Value < MasteryInfo.MinSkillRequirement)
            {
                //SendLocalizedMessage(1156236, String.Format("{0}\t{1}", MasteryInfo.MinSkillRequirement.ToString(), Skills[skill].Info.Name)); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that mastery.

                SkillName mastery = Skills.CurrentMastery;
                Skills.CurrentMastery = SkillName.Alchemy;

                Server.Spells.SkillMasteries.MasteryInfo.OnMasteryChanged(this, mastery);
            }

            TransformContext context = TransformationSpellHelper.GetContext(this);

            if (context != null)
            {
                TransformationSpellHelper.CheckCastSkill(this, context);
            }
		}

		public override void OnAccessLevelChanged(AccessLevel oldLevel)
		{
			if (IsPlayer())
			{
				IgnoreMobiles = false;
			}
			else
			{
				IgnoreMobiles = true;
			}
		}

		public override void OnDelete()
		{
			Instances.Remove(this);

			if (m_ReceivedHonorContext != null)
			{
				m_ReceivedHonorContext.Cancel();
			}
			if (m_SentHonorContext != null)
			{
				m_SentHonorContext.Cancel();
			}
		}

		#region Fastwalk Prevention
		private static bool FastwalkPrevention = true; // Is fastwalk prevention enabled?

		private static int FastwalkThreshold = 400; // Fastwalk prevention will become active after 0.4 seconds

		private long m_NextMovementTime;
		private bool m_HasMoved;

        public long NextMovementTime { get { return m_NextMovementTime; } }

		public virtual bool UsesFastwalkPrevention { get { return IsPlayer(); } }

		public override int ComputeMovementSpeed(Direction dir, bool checkTurning)
		{
			if (checkTurning && (dir & Direction.Mask) != (Direction & Direction.Mask))
			{
				return RunMount; // We are NOT actually moving (just a direction change)
			}

			TransformContext context = TransformationSpellHelper.GetContext(this);

			if (context != null)
			{
                if ((!Core.SA && context.Type == typeof(ReaperFormSpell)) || (!Core.HS && context.Type == typeof(Server.Spells.Mysticism.StoneFormSpell)))
                {
                    return WalkFoot;
                }
			}

			bool running = ((dir & Direction.Running) != 0);

			bool onHorse = Mount != null || Flying;

			AnimalFormContext animalContext = AnimalForm.GetContext(this);

			if (onHorse || (animalContext != null && animalContext.SpeedBoost))
			{
				return (running ? RunMount : WalkMount);
			}

			return (running ? RunFoot : WalkFoot);
		}

		public static bool MovementThrottle_Callback(NetState ns, out bool drop)
		{
			drop = false;

			PlayerMobile pm = ns.Mobile as PlayerMobile;

			if (pm == null || !pm.UsesFastwalkPrevention)
			{
				return true;
			}

			if (!pm.m_HasMoved)
			{
				// has not yet moved
				pm.m_NextMovementTime = Core.TickCount;
				pm.m_HasMoved = true;
				return true;
			}

			long ts = pm.m_NextMovementTime - Core.TickCount;

			if (ts < 0)
			{
				// been a while since we've last moved
				pm.m_NextMovementTime = Core.TickCount;
				return true;
			}

			return (ts < FastwalkThreshold);
		}
		#endregion

		#region Hair and beard mods
		private int m_HairModID = -1, m_HairModHue;
		private int m_BeardModID = -1, m_BeardModHue;

		public void SetHairMods(int hairID, int beardID)
		{
			if (hairID == -1)
			{
				InternalRestoreHair(true, ref m_HairModID, ref m_HairModHue);
			}
			else if (hairID != -2)
			{
				InternalChangeHair(true, hairID, ref m_HairModID, ref m_HairModHue);
			}

			if (beardID == -1)
			{
				InternalRestoreHair(false, ref m_BeardModID, ref m_BeardModHue);
			}
			else if (beardID != -2)
			{
				InternalChangeHair(false, beardID, ref m_BeardModID, ref m_BeardModHue);
			}
		}

		private void CreateHair(bool hair, int id, int hue)
		{
			if (hair)
			{
				//TODO Verification?
				HairItemID = id;
				HairHue = hue;
			}
			else
			{
				FacialHairItemID = id;
				FacialHairHue = hue;
			}
		}

		private void InternalRestoreHair(bool hair, ref int id, ref int hue)
		{
			if (id == -1)
			{
				return;
			}

			if (hair)
			{
				HairItemID = 0;
			}
			else
			{
				FacialHairItemID = 0;
			}

			//if( id != 0 )
			CreateHair(hair, id, hue);

			id = -1;
			hue = 0;
		}

		private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
		{
			if (storeID == -1)
			{
				storeID = hair ? HairItemID : FacialHairItemID;
				storeHue = hair ? HairHue : FacialHairHue;
			}
			CreateHair(hair, id, 0);
		}
		#endregion

		#region Virtues
		private DateTime m_LastSacrificeGain;
		private DateTime m_LastSacrificeLoss;
		private int m_AvailableResurrects;

		public DateTime LastSacrificeGain { get { return m_LastSacrificeGain; } set { m_LastSacrificeGain = value; } }
		public DateTime LastSacrificeLoss { get { return m_LastSacrificeLoss; } set { m_LastSacrificeLoss = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AvailableResurrects { get { return m_AvailableResurrects; } set { m_AvailableResurrects = value; } }

		private DateTime m_NextJustAward;
		private DateTime m_LastJusticeLoss;
		private List<Mobile> m_JusticeProtectors;

		public DateTime LastJusticeLoss { get { return m_LastJusticeLoss; } set { m_LastJusticeLoss = value; } }
		public List<Mobile> JusticeProtectors { get { return m_JusticeProtectors; } set { m_JusticeProtectors = value; } }

		private DateTime m_LastCompassionLoss;
		private DateTime m_NextCompassionDay;
		private int m_CompassionGains;

		public DateTime LastCompassionLoss { get { return m_LastCompassionLoss; } set { m_LastCompassionLoss = value; } }
		public DateTime NextCompassionDay { get { return m_NextCompassionDay; } set { m_NextCompassionDay = value; } }
		public int CompassionGains { get { return m_CompassionGains; } set { m_CompassionGains = value; } }

		private DateTime m_LastValorLoss;

		public DateTime LastValorLoss { get { return m_LastValorLoss; } set { m_LastValorLoss = value; } }

		private DateTime m_LastHonorLoss;
		private HonorContext m_ReceivedHonorContext;
		private HonorContext m_SentHonorContext;
		public DateTime m_hontime;

		public DateTime LastHonorLoss { get { return m_LastHonorLoss; } set { m_LastHonorLoss = value; } }

		public DateTime LastHonorUse { get; set; }

		public bool HonorActive { get; set; }

		public HonorContext ReceivedHonorContext { get { return m_ReceivedHonorContext; } set { m_ReceivedHonorContext = value; } }
		public HonorContext SentHonorContext { get { return m_SentHonorContext; } set { m_SentHonorContext = value; } }
		#endregion

		#region Young system
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Young
		{
			get { return GetFlag(PlayerFlag.Young); }
			set
			{
				SetFlag(PlayerFlag.Young, value);
				InvalidateProperties();
			}
		}
		private DateTime m_YoungTime;
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime YoungTime {	get { return m_YoungTime; } set { m_YoungTime = value; InvalidateProperties(); } } 

		public override string ApplyNameSuffix(string suffix)
		{
			if (Young)
			{
				if (suffix.Length == 0)
				{
					suffix = "(Young)";
				}
				else
				{
					suffix = String.Concat(suffix, " (Young)");
				}
			}

			#region Ethics
			if (m_EthicPlayer != null)
			{
				if (suffix.Length == 0)
				{
					suffix = m_EthicPlayer.Ethic.Definition.Adjunct.String;
				}
				else
				{
					suffix = String.Concat(suffix, " ", m_EthicPlayer.Ethic.Definition.Adjunct.String);
				}
			}
			#endregion

			if (Core.ML && Map == Faction.Facet)
			{
				Faction faction = Faction.Find(this);

				if (faction != null)
				{
					string adjunct = String.Format("[{0}]", faction.Definition.Abbreviation);
					if (suffix.Length == 0)
					{
						suffix = adjunct;
					}
					else
					{
						suffix = String.Concat(suffix, " ", adjunct);
					}
				}
			}

			return base.ApplyNameSuffix(suffix);
		}

		public override TimeSpan GetLogoutDelay()
		{
			/*
			if (Young || BedrollLogout || BlanketOfDarknessLogout || TestCenter.Enabled)
			{
				return TimeSpan.Zero;
			}
			*/
			if ( IsStaff() )
				return TimeSpan.Zero;
			return TimeSpan.FromDays( 10000 ); 
			//base.GetLogoutDelay();
		}

		private DateTime m_LastYoungMessage = DateTime.MinValue;

		public bool CheckYoungProtection(Mobile from)
		{
			return false;
			/*
			if (!Young)
			{
				return false;
			}

			if (Region is BaseRegion && !((BaseRegion)Region).YoungProtected)
			{
				return false;
			}

			if (from is BaseCreature && ((BaseCreature)from).IgnoreYoungProtection)
			{
				return false;
			}

			if (Quest != null && Quest.IgnoreYoungProtection(from))
			{
				return false;
			}

			if (DateTime.UtcNow - m_LastYoungMessage > TimeSpan.FromMinutes(1.0))
			{
				m_LastYoungMessage = DateTime.UtcNow;
				SendLocalizedMessage(1019067);
				// A monster looks at you menacingly but does not attack.  You would be under attack now if not for your status as a new citizen of Britannia.
			}

			return true;
			*/
		}

		private DateTime m_LastYoungHeal = DateTime.MinValue;

		public bool CheckYoungHealTime()
		{
			if (DateTime.Now - m_LastYoungHeal > TimeSpan.FromMinutes(5.0))
			{
				m_LastYoungHeal = DateTime.Now;
				return true;
			}

			return false;
		}
		private static readonly Point3D[] m_TrammelDeathDestinations = new[]
		{
			new Point3D(1481, 1612, 20), new Point3D(2708, 2153, 0), new Point3D(2249, 1230, 0), new Point3D(5197, 3994, 37),
			new Point3D(1412, 3793, 0), new Point3D(3688, 2232, 20), new Point3D(2578, 604, 0), new Point3D(4397, 1089, 0),
			new Point3D(5741, 3218, -2), new Point3D(2996, 3441, 15), new Point3D(624, 2225, 0), new Point3D(1916, 2814, 0),
			new Point3D(2929, 854, 0), new Point3D(545, 967, 0), new Point3D(3469, 2559, 36)
		};

		private static readonly Point3D[] m_IlshenarDeathDestinations = new[]
		{
			new Point3D(1216, 468, -13), new Point3D(723, 1367, -60), new Point3D(745, 725, -28), new Point3D(281, 1017, 0),
			new Point3D(986, 1011, -32), new Point3D(1175, 1287, -30), new Point3D(1533, 1341, -3), new Point3D(529, 217, -44),
			new Point3D(1722, 219, 96)
		};

		private static readonly Point3D[] m_MalasDeathDestinations = new[]
		{new Point3D(2079, 1376, -70), new Point3D(944, 519, -71)};

		private static readonly Point3D[] m_TokunoDeathDestinations = new[]
		{new Point3D(1166, 801, 27), new Point3D(782, 1228, 25), new Point3D(268, 624, 15)};

		public bool YoungDeathTeleport()
		{
			if (Region.IsPartOf<Jail>() || Region.IsPartOf("Samurai start location") ||
				Region.IsPartOf("Ninja start location") || Region.IsPartOf("Ninja cave"))
			{
				return false;
			}

			Point3D loc;
			Map map;

			DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));
			if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
			{
				loc = dungeon.EntranceLocation;
				map = dungeon.EntranceMap;
			}
			else
			{
				loc = Location;
				map = Map;
			}

			Point3D[] list;

			if (map == Map.Trammel)
			{
				list = m_TrammelDeathDestinations;
			}
			else if (map == Map.Ilshenar)
			{
				list = m_IlshenarDeathDestinations;
			}
			else if (map == Map.Malas)
			{
				list = m_MalasDeathDestinations;
			}
			else if (map == Map.Tokuno)
			{
				list = m_TokunoDeathDestinations;
			}
			else
			{
				return false;
			}

			Point3D dest = Point3D.Zero;
			int sqDistance = int.MaxValue;

			for (int i = 0; i < list.Length; i++)
			{
				Point3D curDest = list[i];

				int width = loc.X - curDest.X;
				int height = loc.Y - curDest.Y;
				int curSqDistance = width * width + height * height;

				if (curSqDistance < sqDistance)
				{
					dest = curDest;
					sqDistance = curSqDistance;
				}
			}

			MoveToWorld(dest, map);
			return true;
		}

		private void SendYoungDeathNotice()
		{
			SendGump(new YoungDeathNotice());
		}
		#endregion

		#region Speech
		private SpeechLog m_SpeechLog;
        private bool m_TempSquelched;

		public SpeechLog SpeechLog { get { return m_SpeechLog; } }

        [CommandProperty(AccessLevel.Administrator)]
        public bool TempSquelched { get { return m_TempSquelched; } set { m_TempSquelched = value; } }

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (SpeechLog.Enabled && NetState != null)
			{
				if (m_SpeechLog == null)
				{
					m_SpeechLog = new SpeechLog();
				}

				m_SpeechLog.Add(e.Mobile, e.Speech);
			}
		}

        public override void OnSaid(SpeechEventArgs e)
        {
            if (m_TempSquelched)
            {
                if (Core.ML)
                {
                    SendLocalizedMessage(500168); // You can not say anything, you have been muted.
                }
                else
                {
                    SendMessage("You can not say anything, you have been squelched."); //Cliloc ITSELF changed during ML.
                }

                e.Blocked = true;
            }
            else
            {
                base.OnSaid(e);
            }
        }
		#endregion

		#region Champion Titles
		[CommandProperty(AccessLevel.GameMaster)]
		public bool DisplayChampionTitle { get { return GetFlag(PlayerFlag.DisplayChampionTitle); } set { SetFlag(PlayerFlag.DisplayChampionTitle, value); } }

		private ChampionTitleInfo m_ChampionTitles;

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionTitleInfo ChampionTitles { get { return m_ChampionTitles; } set { } }

		private void ToggleChampionTitleDisplay()
		{
			if (!CheckAlive())
			{
				return;
			}

			if (DisplayChampionTitle)
			{
				SendLocalizedMessage(1062419, "", 0x23); // You have chosen to hide your monster kill title.
			}
			else
			{
				SendLocalizedMessage(1062418, "", 0x23); // You have chosen to display your monster kill title.
			}

			DisplayChampionTitle = !DisplayChampionTitle;
		}

		[PropertyObject]
		public class ChampionTitleInfo
		{
			public static TimeSpan LossDelay = TimeSpan.FromDays(1.0);
			public const int LossAmount = 90;

			private class TitleInfo
			{
				private int m_Value;
				private DateTime m_LastDecay;

				public int Value { get { return m_Value; } set { m_Value = value; } }
				public DateTime LastDecay { get { return m_LastDecay; } set { m_LastDecay = value; } }

				public TitleInfo()
				{ }

				public TitleInfo(GenericReader reader)
				{
					int version = reader.ReadEncodedInt();

					switch (version)
					{
						case 0:
							{
								m_Value = reader.ReadEncodedInt();
								m_LastDecay = reader.ReadDateTime();
								break;
							}
					}
				}

				public static void Serialize(GenericWriter writer, TitleInfo info)
				{
					writer.WriteEncodedInt(0); // version

					writer.WriteEncodedInt(info.m_Value);
					writer.Write(info.m_LastDecay);
				}
			}

			private TitleInfo[] m_Values;

			private int m_Harrower; //Harrower titles do NOT decay

			public int GetValue(ChampionSpawnType type)
			{
				return GetValue((int)type);
			}

			public void SetValue(ChampionSpawnType type, int value)
			{
				SetValue((int)type, value);
			}

			public void Award(ChampionSpawnType type, int value)
			{
				Award((int)type, value);
			}

			public int GetValue(int index)
			{
				if (m_Values == null || index < 0 || index >= m_Values.Length)
				{
					return 0;
				}

				if (m_Values[index] == null)
				{
					m_Values[index] = new TitleInfo();
				}

				return m_Values[index].Value;
			}

			public DateTime GetLastDecay(int index)
			{
				if (m_Values == null || index < 0 || index >= m_Values.Length)
				{
					return DateTime.MinValue;
				}

				if (m_Values[index] == null)
				{
					m_Values[index] = new TitleInfo();
				}

				return m_Values[index].LastDecay;
			}

			public void SetValue(int index, int value)
			{
				if (m_Values == null)
				{
					m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
				}

				if (value < 0)
				{
					value = 0;
				}

				if (index < 0 || index >= m_Values.Length)
				{
					return;
				}

				if (m_Values[index] == null)
				{
					m_Values[index] = new TitleInfo();
				}

				m_Values[index].Value = value;
			}

			public void Award(int index, int value)
			{
				if (m_Values == null)
				{
					m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
				}

				if (index < 0 || index >= m_Values.Length || value <= 0)
				{
					return;
				}

				if (m_Values[index] == null)
				{
					m_Values[index] = new TitleInfo();
				}

				m_Values[index].Value += value;
			}

			public void Atrophy(int index, int value)
			{
				if (m_Values == null)
				{
					m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
				}

				if (index < 0 || index >= m_Values.Length || value <= 0)
				{
					return;
				}

				if (m_Values[index] == null)
				{
					m_Values[index] = new TitleInfo();
				}

				int before = m_Values[index].Value;

				if ((m_Values[index].Value - value) < 0)
				{
					m_Values[index].Value = 0;
				}
				else
				{
					m_Values[index].Value -= value;
				}

				if (before != m_Values[index].Value)
				{
					m_Values[index].LastDecay = DateTime.UtcNow;
				}
			}

			public override string ToString()
			{
				return "...";
			}

			[CommandProperty(AccessLevel.GameMaster)]
			public int Abyss { get { return GetValue(ChampionSpawnType.Abyss); } set { SetValue(ChampionSpawnType.Abyss, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int Arachnid { get { return GetValue(ChampionSpawnType.Arachnid); } set { SetValue(ChampionSpawnType.Arachnid, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int ColdBlood { get { return GetValue(ChampionSpawnType.ColdBlood); } set { SetValue(ChampionSpawnType.ColdBlood, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int ForestLord { get { return GetValue(ChampionSpawnType.ForestLord); } set { SetValue(ChampionSpawnType.ForestLord, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int SleepingDragon { get { return GetValue(ChampionSpawnType.SleepingDragon); } set { SetValue(ChampionSpawnType.SleepingDragon, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int UnholyTerror { get { return GetValue(ChampionSpawnType.UnholyTerror); } set { SetValue(ChampionSpawnType.UnholyTerror, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int VerminHorde { get { return GetValue(ChampionSpawnType.VerminHorde); } set { SetValue(ChampionSpawnType.VerminHorde, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int Harrower { get { return m_Harrower; } set { m_Harrower = value; } }

			#region Mondain's Legacy Peerless Champion
			[CommandProperty(AccessLevel.GameMaster)]
			public int Glade { get { return GetValue(ChampionSpawnType.Glade); } set { SetValue(ChampionSpawnType.Glade, value); } }

			[CommandProperty(AccessLevel.GameMaster)]
			public int Corrupt { get { return GetValue(ChampionSpawnType.Corrupt); } set { SetValue(ChampionSpawnType.Corrupt, value); } }
			#endregion

			public ChampionTitleInfo()
			{ }

			public ChampionTitleInfo(GenericReader reader)
			{
				int version = reader.ReadEncodedInt();

				switch (version)
				{
					case 0:
						{
							m_Harrower = reader.ReadEncodedInt();

							int length = reader.ReadEncodedInt();
							m_Values = new TitleInfo[length];

							for (int i = 0; i < length; i++)
							{
								m_Values[i] = new TitleInfo(reader);
							}

							if (m_Values.Length != ChampionSpawnInfo.Table.Length)
							{
								var oldValues = m_Values;
								m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

								for (int i = 0; i < m_Values.Length && i < oldValues.Length; i++)
								{
									m_Values[i] = oldValues[i];
								}
							}
							break;
						}
				}
			}

			public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
			{
				writer.WriteEncodedInt(0); // version

				writer.WriteEncodedInt(titles.m_Harrower);

				int length = titles.m_Values.Length;
				writer.WriteEncodedInt(length);

				for (int i = 0; i < length; i++)
				{
					if (titles.m_Values[i] == null)
					{
						titles.m_Values[i] = new TitleInfo();
					}

					TitleInfo.Serialize(writer, titles.m_Values[i]);
				}
			}

			public static void CheckAtrophy(PlayerMobile pm)
			{
				ChampionTitleInfo t = pm.m_ChampionTitles;
				if (t == null)
				{
					return;
				}

				if (t.m_Values == null)
				{
					t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
				}

				for (int i = 0; i < t.m_Values.Length; i++)
				{
					if ((t.GetLastDecay(i) + LossDelay) < DateTime.UtcNow)
					{
						t.Atrophy(i, LossAmount);
					}
				}
			}

			public static void AwardHarrowerTitle(PlayerMobile pm)
				//Called when killing a harrower.  Will give a minimum of 1 point.
			{
				ChampionTitleInfo t = pm.m_ChampionTitles;
				if (t == null)
				{
					return;
				}

				if (t.m_Values == null)
				{
					t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
				}

				int count = 1;

				for (int i = 0; i < t.m_Values.Length; i++)
				{
					if (t.m_Values[i].Value > 900)
					{
						count++;
					}
				}

				t.m_Harrower = Math.Max(count, t.m_Harrower); //Harrower titles never decay.
			}

            public bool HasChampionTitle(PlayerMobile pm)
            {
                if (m_Harrower > 0)
                    return true;

                if (m_Values == null)
                    return false;

                foreach (TitleInfo info in m_Values)
                {
                    if (info.Value > 300)
                        return true;
                }

                return false;
            }
		}
		#endregion

		#region Recipes
		private Dictionary<int, bool> m_AcquiredRecipes;

		public virtual bool HasRecipe(Recipe r)
		{
			if (r == null)
			{
				return false;
			}

			return HasRecipe(r.ID);
		}

		public virtual bool HasRecipe(int recipeID)
		{
			if (m_AcquiredRecipes != null && m_AcquiredRecipes.ContainsKey(recipeID))
			{
				return m_AcquiredRecipes[recipeID];
			}

			return false;
		}

		public virtual void AcquireRecipe(Recipe r)
		{
			if (r != null)
			{
				AcquireRecipe(r.ID);
			}
		}

		public virtual void AcquireRecipe(int recipeID)
		{
			if (m_AcquiredRecipes == null)
			{
				m_AcquiredRecipes = new Dictionary<int, bool>();
			}

			m_AcquiredRecipes[recipeID] = true;
		}

		public virtual void ResetRecipes()
		{
			m_AcquiredRecipes = null;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int KnownRecipes
		{
			get
			{
				if (m_AcquiredRecipes == null)
				{
					return 0;
				}

				return m_AcquiredRecipes.Count;
			}
		}
		#endregion

		#region Buff Icons
		public void ResendBuffs()
		{
			if (!BuffInfo.Enabled || m_BuffTable == null)
			{
				return;
			}

			NetState state = NetState;

			if (state != null && state.BuffIcon)
			{
				foreach (BuffInfo info in m_BuffTable.Values)
				{
					state.Send(new AddBuffPacket(this, info));
				}
			}
		}

		private Dictionary<BuffIcon, BuffInfo> m_BuffTable;

		public void AddBuff(BuffInfo b)
		{
			if (!BuffInfo.Enabled || b == null)
			{
				return;
			}

			RemoveBuff(b); //Check & subsequently remove the old one.

			if (m_BuffTable == null)
			{
				m_BuffTable = new Dictionary<BuffIcon, BuffInfo>();
			}

			m_BuffTable.Add(b.ID, b);

			NetState state = NetState;

			if (state != null && state.BuffIcon)
			{
				state.Send(new AddBuffPacket(this, b));
			}
		}

		public void RemoveBuff(BuffInfo b)
		{
			if (b == null)
			{
				return;
			}

			RemoveBuff(b.ID);
		}

		public void RemoveBuff(BuffIcon b)
		{
			if (m_BuffTable == null || !m_BuffTable.ContainsKey(b))
			{
				return;
			}

			BuffInfo info = m_BuffTable[b];

			if (info.Timer != null && info.Timer.Running)
			{
				info.Timer.Stop();
			}

			m_BuffTable.Remove(b);

			NetState state = NetState;

			if (state != null && state.BuffIcon)
			{
				state.Send(new RemoveBuffPacket(this, b));
			}

			if (m_BuffTable.Count <= 0)
			{
				m_BuffTable = null;
			}
		}
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public ExploringTheDeepQuestChain ExploringTheDeepQuest { get; set; }

        public static bool PetAutoStable { get { return Core.SE; } }

        public void AutoStablePets()
		{
			if (PetAutoStable && AllFollowers.Count > 0)
			{
				for (int i = m_AllFollowers.Count - 1; i >= 0; --i)
				{
					BaseCreature pet = AllFollowers[i] as BaseCreature;

                    if (pet == null)
                    {
                        continue;
                    }

                    if (pet.Summoned && pet.Map != Map)
                    {
                        pet.PlaySound(pet.GetAngerSound());

                        Timer.DelayCall(pet.Delete);

						continue;
                    }

					if (!pet.CanAutoStable)
					{
						continue;
					}

					pet.ControlTarget = null;
					pet.ControlOrder = OrderType.Stay;
					pet.Internalize();

					pet.SetControlMaster(null);
					pet.SummonMaster = null;

					pet.IsStabled = true;
					pet.StabledBy = this;

					//pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy

					Stabled.Add(pet);
					m_AutoStabled.Add(pet);
					//펫 사라짐
					PetFollows -= pet.ControlSlots;
				}
			}
		}

		public void ClaimAutoStabledPets()
		{
			if (!PetAutoStable || !Region.AllowAutoClaim(this) || m_AutoStabled.Count <= 0)
			{
				return;
			}

			if (!Alive)
			{
                SendGump(new ReLoginClaimGump());
				return;
			}

			for (int i = m_AutoStabled.Count - 1; i >= 0; --i)
			{
				BaseCreature pet = m_AutoStabled[i] as BaseCreature;

				if (pet == null || pet.Deleted)
				{
					pet.IsStabled = false;
					pet.StabledBy = null;

					if (Stabled.Contains(pet))
					{
						Stabled.Remove(pet);
					}

					continue;
				}

				if ((Followers + pet.ControlSlots) <= FollowersMax)
				{
					pet.SetControlMaster(this);

					if (pet.Summoned)
					{
						pet.SummonMaster = this;
					}
					//PetFollows += pet.ControlSlots;
					pet.ControlTarget = this;
					pet.ControlOrder = OrderType.Follow;

					pet.MoveToWorld(Location, Map);

					pet.IsStabled = false;
					pet.StabledBy = null;

					//pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy

					if (Stabled.Contains(pet))
					{
						Stabled.Remove(pet);
					}
				}
				else
				{
					SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
				}
			}

			m_AutoStabled.Clear();
		}
	}
}
