using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Factions;
using Server.Misc;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public enum GemType
    {
        None,
        StarSapphire,
        Emerald,
        Sapphire,
        Ruby,
        Citrine,
        Amethyst,
        Tourmaline,
        Amber,
        Diamond
    }

    public abstract class BaseJewel : Item, ICraftable, ISetItem, IResource, IVvVItem, IOwnerRestricted, ITalismanProtection, IFactionItem, IArtifact, ICombatEquipment, IQuality, IDurability, IEquipOption
    {
        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get { return m_FactionState; }
            set
            {
                m_FactionState = value;
            }
        }
        #endregion

        private int m_MaxHitPoints;
        private int m_HitPoints;

        private AosAttributes m_AosAttributes;
        private AosArmorAttributes m_AosArmorAttributes;
        private AosElementAttributes m_AosResistances;
        private AosSkillBonuses m_AosSkillBonuses;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private SAAbsorptionAttributes m_SAAbsorptionAttributes;
        private ExtendedWeaponAttributes m_ExtendedWeaponAttributes;
        private NegativeAttributes m_NegativeAttributes;
        private CraftResource m_Resource;
        private GemType m_GemType;

        #region Stygian Abyss
        private int m_TimesImbued;
        private bool m_IsImbued;
        private int m_GorgonLenseCharges;
        private LenseType m_GorgonLenseType;
        #endregion

        #region Runic Reforging
        private ItemPower m_ItemPower;
        private ReforgedPrefix m_ReforgedPrefix;
        private ReforgedSuffix m_ReforgedSuffix;
        #endregion

        private TalismanAttribute m_TalismanProtection;

        private bool _VvVItem;
        private Mobile _Owner;
        private string _OwnerName;
		private int m_BaseJewelRating;
        [CommandProperty(AccessLevel.GameMaster)]
        public int BaseJewelRating
        {
            get { return m_BaseJewelRating; }
            set { m_BaseJewelRating = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TalismanAttribute Protection
        {
            get { return m_TalismanProtection; }
            set { m_TalismanProtection = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsVvVItem
        {
            get { return _VvVItem; }
            set { _VvVItem = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner
        {
            get { return _Owner; }
            set { _Owner = value; if (_Owner != null) _OwnerName = _Owner.Name; InvalidateProperties(); }
        }

        public virtual string OwnerName
        {
            get { return _OwnerName; }
            set { _OwnerName = value; InvalidateProperties(); }
        }

        private Mobile m_BlessedBy;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile BlessedBy
        {
            get
            {
                return m_BlessedBy;
            }
            set
            {
                m_BlessedBy = value;
                InvalidateProperties();
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

			if (from.Alive)
			{
				if( LootType == LootType.Blessed )
					list.Add(new UnBlassCheck(this));
				else if( LootType == LootType.Regular )
					list.Add(new BlassCheck(this));
			}
        }
		
        #region ContextMenuEntries
        private class BlassCheck : ContextMenuEntry
        {
            private readonly BaseJewel m_Equip;

            public BlassCheck(BaseJewel equip)
                : base(6310)
            {
                m_Equip = equip;
            }

            public override void OnClick()
            {
                if (m_Equip.Deleted)
                    return;

				m_Equip.LootType = LootType.Blessed;
            }
        }
        private class UnBlassCheck : ContextMenuEntry
        {
            private readonly BaseJewel m_Equip;

            public UnBlassCheck(BaseJewel equip)
                : base(6311)
            {
                m_Equip = equip;
            }

            public override void OnClick()
            {
                if (m_Equip.Deleted)
                    return;

				m_Equip.LootType = LootType.Blessed;
            }
        }
		
		#endregion		
		
		private bool m_Identified;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Identified
		{
			get { return m_Identified; }
			set
			{
				m_Identified = value;
				InvalidateProperties();
			}
		}
        private class UnBlessEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly BaseJewel m_Item;

            public UnBlessEntry(Mobile from, BaseJewel item)
                : base(6208, -1)
            {
                m_From = from;
                m_Item = item;
            }

            public override void OnClick()
            {
                m_Item.BlessedFor = null;
                m_Item.BlessedBy = null;

                Container pack = m_From.Backpack;

                if (pack != null)
                {
                    pack.DropItem(new PersonalBlessDeed(m_From));
                    m_From.SendLocalizedMessage(1062200); // A personal bless deed has been placed in your backpack.
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get
            {
                return m_MaxHitPoints;
            }
            set
            {
                m_MaxHitPoints = value;

                //if (m_MaxHitPoints > 255)
                //   m_MaxHitPoints = 255;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get 
            {
                return m_HitPoints;
            }
            set 
            {
                if (value != m_HitPoints && MaxHitPoints > 0)
                {
                    m_HitPoints = value;

                    if (m_HitPoints < 0)
                        Delete();
                    else if (m_HitPoints > MaxHitPoints)
                        m_HitPoints = MaxHitPoints;

                    InvalidateProperties();
                }
            }
        }

        [CommandProperty(AccessLevel.Player)]
        public AosAttributes Attributes
        {
            get
            {
                return m_AosAttributes;
            }
            set
            {
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public AosArmorAttributes ArmorAttributes { get { return m_AosArmorAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosElementAttributes Resistances
        {
            get
            {
                return m_AosResistances;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses
        {
            get
            {
                return m_AosSkillBonuses;
            }
            set
            {
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes { get { return m_AosWeaponAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SAAbsorptionAttributes AbsorptionAttributes
        {
            get
            {
                return m_SAAbsorptionAttributes;
            }
            set
            {
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public ExtendedWeaponAttributes ExtendedWeaponAttributes { get { return m_ExtendedWeaponAttributes; } set { } }

        [CommandProperty(AccessLevel.Player)]
        public NegativeAttributes NegativeAttributes
        {
            get
            { 
                return m_NegativeAttributes;
            }
            set 
            { 
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return m_Resource;
            }
            set
            {
                if (m_Resource != value )
                {
                    UnscaleDurability();
                    CraftResource old = m_Resource;

                    m_Resource = value;

                    if (CraftItem.RetainsColor(GetType()))
                    {
                        Hue = CraftResources.GetHue(m_Resource);
                    }

                    InvalidateProperties();

                    if (Parent is Mobile)
                        ((Mobile)Parent).UpdateResistances();

                    ScaleDurability();
                }
			}
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public GemType GemType
        {
            get
            {
                return m_GemType;
            }
            set
            {
                m_GemType = value;
                InvalidateProperties();
            }
        }

        #region SA
        [CommandProperty(AccessLevel.GameMaster)]
        public int TimesImbued
        {
            get { return m_TimesImbued; }
            set { m_TimesImbued = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsImbued
        {
            get
            {
                if (TimesImbued >= 1 && !m_IsImbued)
                    m_IsImbued = true;

                return m_IsImbued;
            }
            set
            {
                if (TimesImbued >= 1)
                    m_IsImbued = true;
                else
                    m_IsImbued = value; InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GorgonLenseCharges
        {
            get { return m_GorgonLenseCharges; }
            set { m_GorgonLenseCharges = value; if (value == 0) m_GorgonLenseType = LenseType.None; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public LenseType GorgonLenseType
        {
            get { return m_GorgonLenseType; }
            set { m_GorgonLenseType = value; InvalidateProperties(); }
        }

        public virtual int[] BaseResists
        {
            get
            {
                return new int[] { 0, 0, 0, 0, 0 };
            }
        }

        public virtual void OnAfterImbued(Mobile m, int mod, int value)
        {
        }
        #endregion

        #region Runic Reforging
        [CommandProperty(AccessLevel.GameMaster)]
        public ReforgedPrefix ReforgedPrefix 
        {
            get { return m_ReforgedPrefix; } 
            set { m_ReforgedPrefix = value; InvalidateProperties(); } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ReforgedSuffix ReforgedSuffix 
        { 
            get { return m_ReforgedSuffix; } 
            set { m_ReforgedSuffix = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemPower ItemPower 
        {
            get { return m_ItemPower; }
            set { m_ItemPower = value; InvalidateProperties(); } 
        }
		private int[] m_PrefixOption = new int[100];
		public int[] PrefixOption
		{
			get { return m_PrefixOption; }
			set { m_PrefixOption = value;}
		}
		private int[] m_SuffixOption = new int[100];
		public int[] SuffixOption
		{
			get { return m_SuffixOption; }
			set { m_SuffixOption = value;}
		}
        #endregion

        public override int PhysicalResistance
        {
            get
            {
                return m_AosResistances.Physical + m_AosWeaponAttributes.ResistPhysicalBonus / 10000 + m_AosArmorAttributes.AllResist / 10000;
            }
        }
        public override int FireResistance
        {
            get
            {
                return m_AosResistances.Fire + m_AosWeaponAttributes.ResistFireBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000;
            }
        }
        public override int ColdResistance
        {
            get
            {
                return m_AosResistances.Cold + m_AosWeaponAttributes.ResistColdBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000;
            }
        }
        public override int PoisonResistance
        {
            get
            {
                return m_AosResistances.Poison + m_AosWeaponAttributes.ResistPoisonBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000;
            }
        }
        public override int EnergyResistance
        {
            get
            {
                return m_AosResistances.Energy + m_AosWeaponAttributes.ResistEnergyBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000;
            }
        }
        public virtual int BaseGemTypeNumber
        {
            get
            {
                return 0;
            }
        }

        public virtual int InitMinHits
        {
            get
            {
                return 25;
            }
        }
        public virtual int InitMaxHits
        {
            get
            {
                return 30;
            }
        }

        public virtual Race RequiredRace
        {
            get
            {
                return null;
            }
        }
        public virtual bool CanBeWornByGargoyles
        {
            get
            {
                return true;
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_GemType == GemType.None)
                    return base.LabelNumber;

                return BaseGemTypeNumber + (int)m_GemType - 1;
            }
        }

        public override double DefaultWeight
        {
            get
            {
                if (NegativeAttributes == null || NegativeAttributes.Unwieldly == 0)
                    return base.DefaultWeight;

                return 50;
            }
        }
		private int m_HiddenRank;
		[CommandProperty( AccessLevel.GameMaster )]
		public int HiddenRank
		{
			get{ return m_HiddenRank; }
			set{ m_HiddenRank = value; }
		}

        public override void OnAfterDuped(Item newItem)
        {
            BaseJewel jewel = newItem as BaseJewel;

            if (jewel == null)
                return;

            jewel.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
			jewel.m_AosArmorAttributes = new AosArmorAttributes(newItem, m_AosArmorAttributes);
            jewel.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
            jewel.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
            jewel.m_NegativeAttributes = new NegativeAttributes(newItem, m_NegativeAttributes);
            jewel.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
			jewel.m_SAAbsorptionAttributes = new SAAbsorptionAttributes(newItem, m_SAAbsorptionAttributes);
            jewel.m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(newItem, m_ExtendedWeaponAttributes);
            jewel.m_TalismanProtection = new TalismanAttribute(m_TalismanProtection);

            #region Mondain's Legacy
            jewel.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
            jewel.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
            #endregion

            //jewel.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);

            base.OnAfterDuped(newItem);
        }

        public virtual int ArtifactRarity
        {
            get
            {
                return 0;
            }
        }

        public override bool DisplayWeight
        {
            get
            {
                if (IsVvVItem)
                    return true;

                return base.DisplayWeight;
            }
        }

        private Mobile m_Crafter;
        private ItemQuality m_Quality;
        private bool m_PlayerConstructed;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get
            {
                return m_Crafter;
            }
            set
            {
                m_Crafter = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemQuality Quality
        {
            get
            {
                return m_Quality;
            }
            set
            {
                m_Quality = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed
        {
            get
            {
                return m_PlayerConstructed;
            }
            set
            {
                m_PlayerConstructed = value;
                InvalidateProperties();
            }
        }

        public BaseJewel(int itemID, Layer layer)
            : base(itemID)
        {
            m_AosAttributes = new AosAttributes(this);
            m_AosResistances = new AosElementAttributes(this);
            m_AosSkillBonuses = new AosSkillBonuses(this);
            m_AosArmorAttributes = new AosArmorAttributes(this);
            m_AosWeaponAttributes = new AosWeaponAttributes(this);
            m_Resource = CraftResource.Iron;
            m_GemType = GemType.None;
			m_Identified = true;

            Layer = layer;
			m_StrReq = -1;
			m_DexReq = -1;
			m_IntReq = -1;

            m_HitPoints = m_MaxHitPoints = InitMinHits; //Utility.RandomMinMax(InitMinHits, InitMaxHits);

            m_SetAttributes = new AosAttributes(this);
            m_SetSkillBonuses = new AosSkillBonuses(this);
            m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);
			m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);
            m_NegativeAttributes = new NegativeAttributes(this);
            m_TalismanProtection = new TalismanAttribute();
        }
		private int m_StrReq;
		private int m_DexReq;
		private int m_IntReq;
		public virtual int AosStrengthReq { get { return 0; } }
		public virtual int AosDexterityReq { get { return 0; } }
		public virtual int AosIntelligenceReq { get { return 0; } }
		
		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get{ return AosStrengthReq == 0 ? 1000 : AosStrengthReq; }
			set{ m_StrReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get{ return AosDexterityReq == 0 ? 1000 : AosDexterityReq; }//100 * ( SuffixOption[1] + 1 ) * ( SuffixOption[1] + 1 ) ); }
			set{ m_DexReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get{ return AosIntelligenceReq == 0 ? 1000 : AosIntelligenceReq; }//100 * ( SuffixOption[1] + 1 ) * ( SuffixOption[1] + 1 ) ); }
			set{ m_IntReq = value; InvalidateProperties(); }
		}


        #region Stygian Abyss
        public override bool CanEquip(Mobile from)
        {
            if (BlessedBy != null && BlessedBy != from)
            {
                from.SendLocalizedMessage(1075277); // That item is blessed by another player.
                return false;
            }
			/*
			else if( from is PlayerMobile && !Util.EquipCheck( ((PlayerMobile)from), this ) )
			{
				from.SendLocalizedMessage(1071936); // You cannot equip that.
				return false;
			}
			*/
            if (from.IsPlayer())
            {
                if (_Owner != null && _Owner != from)
                {
                    from.SendLocalizedMessage(501023); // You must be the owner to use this item.
                    return false;
                }

                if (this is IAccountRestricted && ((IAccountRestricted)this).Account != null)
                {
                    Accounting.Account acct = from.Account as Accounting.Account;

                    if (acct == null || acct.Username != ((IAccountRestricted)this).Account)
                    {
                        from.SendLocalizedMessage(1071296); // This item is Account Bound and your character is not bound to it. You cannot use this item.
                        return false;
                    }
                }

                if (IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(from))
                {
                    from.SendLocalizedMessage(1155496); // This item can only be used by VvV participants!
                    return false;
                }
				/*
				if( SpellHelper.CheckCombat(from) )
				{
					from.SendMessage("전투 중에는 장비를 착용할 수 없습니다!");
					return false;
				}
				*/
            }

            if (from.AccessLevel < AccessLevel.GameMaster)
            {
                bool morph = from.FindItemOnLayer(Layer.Earrings) is MorphEarrings;

                if (from.Race == Race.Gargoyle && !CanBeWornByGargoyles)
                {
                    from.SendLocalizedMessage(1111708); // Gargoyles can't wear 
                    return false;
                }
				/*
                else if (RequiredRace != null && from.Race != RequiredRace && !morph)
                {
                    if (RequiredRace == Race.Elf)
                        from.SendLocalizedMessage(1072203); // Only Elves may use 
                    else if (RequiredRace == Race.Gargoyle)
                        from.SendLocalizedMessage(1111707); // Only gargoyles can wear 
                    else
                        from.SendMessage("Only {0} may use ", RequiredRace.PluralName);

                    return false;
                }
				*/
				else if (from.Dex < AOS.Scale2(DexRequirement, 1000 - GetLowerStatReq()))
				{
					from.SendLocalizedMessage(502077); // You cannot equip that.
					return false;
				}
				else if (from.Str < AOS.Scale2(StrRequirement, 1000 - GetLowerStatReq()))
				{
					from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
					return false;
				}
				else if (from.Int < AOS.Scale2(IntRequirement, 1000 - GetLowerStatReq()))
				{
					from.SendLocalizedMessage(1071936); // You cannot equip that.
					return false;
				}
			}
			int levelcheck = 40;
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				int equippercent = 1000 - WeaponAttributes.LowerStatReq;
				levelcheck *= equippercent;
				levelcheck /= 1000;
				if( Misc.Util.Level(pm.SilverPoint[0]) < PrefixOption[99] * levelcheck )
				{
					from.SendLocalizedMessage(1071936); // You cannot equip that.
					return false;
				}
			}
		
            return base.CanEquip(from);
        }
        public int GetLowerStatReq()
        {
            if (!Core.AOS)
                return 0;

			int v = m_AosWeaponAttributes.LowerStatReq;
			
			if (v > 1000)
			{
				v = 1000;
			}

			return v;
        }
        public virtual void OnHit(int damage)
        {
			m_HiddenRank += damage;
			bool destroy = false;
			int breaken = 1;
			if( m_HiddenRank >= 750 )
			{
				destroy = true;
				breaken = m_HiddenRank / 750;
				m_HiddenRank -= 750 * breaken;
			}
            if ( destroy ) // 25% chance to lower durability
            {
				if (m_MaxHitPoints > 0 + breaken)
				{
					if (m_HitPoints >= 1 + breaken)
						HitPoints -= 1 + breaken;
					else if ( m_MaxHitPoints > 0 + breaken)
					{
						MaxHitPoints -= 1 + breaken;
						
						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
						if (m_MaxHitPoints <= 0 + breaken )
							Delete();
					}
				}
				if( Parent is PlayerMobile )
				{
					PlayerMobile pm = Parent as PlayerMobile;
					//Misc.Util.EquipPoint( pm, this );
				}
            }
        }

        public void UnscaleDurability()
        {
             int scale = 100;

            m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
            m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;

            InvalidateProperties();
       }

        public void ScaleDurability()
        {
             int scale = 100;

            m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
            m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;

            if (m_MaxHitPoints > 255)
                m_MaxHitPoints = 255;

            if (m_HitPoints > 255)
                m_HitPoints = 255;

            InvalidateProperties();
       }

        public virtual bool CanFortify { get { return IsImbued == false && NegativeAttributes.Antique < 4; } }
        public virtual bool CanRepair { get { return m_NegativeAttributes.NoRepair == 0; } }
        #endregion

        public override void OnAdded(object parent)
        {
            if (Core.AOS && parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                m_AosSkillBonuses.AddTo(from);

                int strBonus = m_AosAttributes.BonusStr;
                int dexBonus = m_AosAttributes.BonusDex;
                int intBonus = m_AosAttributes.BonusInt;

                if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
                {
                    string modName = Serial.ToString();

                    if (strBonus != 0)
                        from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                    if (dexBonus != 0)
                        from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                    if (intBonus != 0)
                        from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
                }

				if( !Identified )
					Identified = true;
				if( Owner == null && ( PrefixOption[0] == 200 || PrefixOption[0] == 300 ) )
					Owner = from;

                from.CheckStatTimers();

                #region Mondain's Legacy Sets
                if (IsSetItem)
                {
                    m_SetEquipped = SetHelper.FullSetEquipped(from, SetID, Pieces);

                    if (m_SetEquipped)
                    {
                        m_LastEquipped = true;
                        SetHelper.AddSetBonus(from, SetID);
                    }
                }
                #endregion
				//세트 아이템 체크 코드
				if( PrefixOption[50] > 0 )
				{
					if( from is PlayerMobile )
					{
						PlayerMobile pm = from as PlayerMobile;
						pm.ItemSetValue[PrefixOption[50]]++;
						Misc.Util.SetOption(pm, false);
					}					
				}			
            }

            if (parent is Mobile)
            {
                if (Server.Engines.XmlSpawner2.XmlAttach.CheckCanEquip(this, (Mobile)parent))
                    Server.Engines.XmlSpawner2.XmlAttach.CheckOnEquip(this, (Mobile)parent);
                else
                    ((Mobile)parent).AddToBackpack(this);
            }
        }

        public override void OnRemoved(object parent)
        {
            if (Core.AOS && parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                m_AosSkillBonuses.Remove();

                string modName = Serial.ToString();

                from.RemoveStatMod(modName + "Str");
                from.RemoveStatMod(modName + "Dex");
                from.RemoveStatMod(modName + "Int");

                from.CheckStatTimers();

                #region Mondain's Legacy Sets
                if (IsSetItem && m_SetEquipped)
                    SetHelper.RemoveSetBonus(from, SetID, this);
                #endregion
				//세트 아이템 해제 코드
				if( PrefixOption[50] > 0 )
				{
					if( from is PlayerMobile )
					{
						PlayerMobile pm = from as PlayerMobile;
						pm.ItemSetValue[PrefixOption[50]]--;
						Misc.Util.SetOption(pm, false);
					}					
				}				
            }

            Server.Engines.XmlSpawner2.XmlAttach.CheckOnRemoved(this, parent);
        }

        public virtual void SetProtection(Type type, TextDefinition name, int amount)
        {
            m_TalismanProtection = new TalismanAttribute(type, name, amount);
        }

        public BaseJewel(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            int oreType;

            switch ( m_Resource )
            {
                case CraftResource.DullCopper: oreType = 1053108; break; // dull copper
                case CraftResource.ShadowIron: oreType = 1053107; break; // shadow iron
                case CraftResource.Copper: oreType = 1053106; break; // copper
                case CraftResource.Bronze: oreType = 1053105; break; // bronze
                case CraftResource.Gold: oreType = 1053104; break; // golden
                case CraftResource.Agapite: oreType = 1053103; break; // agapite
                case CraftResource.Verite: oreType = 1053102; break; // verite
                case CraftResource.Valorite: oreType = 1053101; break; // valorite
				case CraftResource.DernedLeather: oreType = 1051901; break; // 거친 가죽
				case CraftResource.RatnedLeather: oreType = 1051902; break; // 질긴 가죽
				case CraftResource.SernedLeather: oreType = 1051903; break; // 경화 가죽
                case CraftResource.SpinedLeather: oreType = 1061118; break; // spined
                case CraftResource.HornedLeather: oreType = 1061117; break; // horned
                case CraftResource.BarbedLeather: oreType = 1061116; break; // barbed
                case CraftResource.RedScales: oreType = 1060814; break; // red
                case CraftResource.YellowScales: oreType = 1060818; break; // yellow
                case CraftResource.BlackScales: oreType = 1060820; break; // black
                case CraftResource.GreenScales: oreType = 1060819; break; // green
                case CraftResource.WhiteScales: oreType = 1060821; break; // white
                case CraftResource.BlueScales: oreType = 1060815; break; // blue
                case CraftResource.OakWood: oreType = 1072533;  break; // oak
                case CraftResource.AshWood: oreType = 1072534; break; // ash
                case CraftResource.YewWood: oreType = 1072535; break; // yew
                case CraftResource.Heartwood: oreType = 1072536; break; // heartwood
                case CraftResource.Bloodwood: oreType = 1072538; break; // bloodwood
                case CraftResource.Frostwood: oreType = 1072539; break; // frostwood
                default: oreType = 0; break;
            }
			
			//아이템 이름 설정
            if (Name == null)
            {
				if (oreType != 0)
				{
					if( !Identified )
						list.Add(1028266, "<basefont color=#AAAAAA>{0}\t#{1}\t{2}<basefont color=#FFFFFF>", "", oreType, GetNameString());
					else if( (int)ItemPower == 0 || (int)ItemPower >= 4 )
					{
						if (m_ReforgedPrefix != ReforgedPrefix.None && m_ReforgedSuffix != ReforgedSuffix.None )
						{
							list.Add(1028261, String.Format(Util.OreAllItemRank( (int)ItemPower), "",  RunicReforging.GetPrefixName(m_ReforgedPrefix), RunicReforging.GetSuffixName(m_ReforgedSuffix), oreType,GetNameString()));
						}
						else if ( m_ReforgedPrefix != ReforgedPrefix.None )
						{
							list.Add(1028262, String.Format(Util.OreOneItemRank( (int)ItemPower), "",  RunicReforging.GetPrefixName(m_ReforgedPrefix), oreType, GetNameString()));
						}
						else if ( m_ReforgedSuffix != ReforgedSuffix.None )
						{
							list.Add(1028263, String.Format(Util.OreOneItemRank( (int)ItemPower), "",  RunicReforging.GetSuffixName(m_ReforgedSuffix), oreType, GetNameString()));
							
						}
						else
						{
							list.Add(1028264, String.Format(Util.OreItemRank( (int)ItemPower), "", oreType, GetNameString()));
						}
					}
					else
						list.Add(1053099, "#{0}\t{1}", oreType, GetNameString());
				}
				else if( SuffixOption[99] > 0 )
				{
					if( !Identified )
						list.Add(1028266, "<basefont color=#AAAAAA>{0}\t#{1}\t{2}<basefont color=#FFFFFF>", "", 1052084 + SuffixOption[99], GetNameString());
					else
						list.Add(1028264, String.Format(Util.OreItemRank( (int)ItemPower), "", 1052084 + SuffixOption[99], GetNameString()));
				}
				else
				{
					if( !Identified )
						list.Add(1028265, "<basefont color=#AAAAAA>{0}\t{1}<basefont color=#FFFFFF>", "", GetNameString());
					else if( (int)ItemPower == 0 || (int)ItemPower >= 4 )
					{
						if (m_ReforgedPrefix != ReforgedPrefix.None && m_ReforgedSuffix != ReforgedSuffix.None )
						{
							list.Add(1028258, String.Format(Util.AllItemRank( (int)ItemPower), "", RunicReforging.GetPrefixName(m_ReforgedPrefix), RunicReforging.GetSuffixName(m_ReforgedSuffix), GetNameString()));
						}
						else if ( m_ReforgedPrefix != ReforgedPrefix.None )
						{
							list.Add(1028259, String.Format(Util.OneItemRank( (int)ItemPower), "", RunicReforging.GetPrefixName(m_ReforgedPrefix), GetNameString()));
						}
						else if ( m_ReforgedSuffix != ReforgedSuffix.None )
						{
							list.Add(1028260, String.Format(Util.OneItemRank( (int)ItemPower), "", RunicReforging.GetSuffixName(m_ReforgedSuffix), GetNameString()));
							
						}
						else
						{
							list.Add(1053099, Util.ItemRank( (int)ItemPower), "", GetNameString());
						}
					}
					else
						list.Add(1053099, "{0}\t{1}", "", GetNameString());						
				}			
            }
            else
            {
				base.AddNameProperty(list);
            }
        }

        private string GetNameString()
        {
            string name = Name;

            if (name == null)
                name = String.Format("#{0}", LabelNumber);

            return name;
        }

        public override void AddCraftedProperties(ObjectPropertyList list)
        {
			//구 아이템 체크
			/*
			if( PlayerConstructed && ( PrefixOption[98] == null || PrefixOption[98] != 1 ) )
			{
				list.Add( 1063524 );
			}
			*/
			//기본 옵션
			list.Add(1063523);

            if (OwnerName != null)
                list.Add(1153213, OwnerName);

            if (m_Crafter != null)
                list.Add(1050043, m_Crafter.TitleName); // crafted by ~1_NAME~

            if (m_Quality == ItemQuality.Exceptional)
                list.Add(1063341); // exceptional

            if (IsImbued)
                list.Add(1080418); // (Imbued)
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            if (IsVvVItem)
                list.Add(1154937); // VvV Item
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);           

            #region Factions
            FactionEquipment.AddFactionProperties(this, list);
            #endregion
			if (ArtifactRarity > 0)
			{
				list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
			}
			if( PrefixOption[99] > 0 )
			{
				int levelcheck = 40;
				if( RootParent != null && RootParent is PlayerMobile )
				{
					PlayerMobile pm = RootParent as PlayerMobile;
					int equippercent = 1000 - WeaponAttributes.LowerStatReq;
					
					levelcheck *= equippercent;
					levelcheck /= 1000;
					
					if( Misc.Util.Level(pm.SilverPoint[0]) < PrefixOption[99] * levelcheck )
						list.Add( 1063525, ( PrefixOption[99] * levelcheck ).ToString() );
					else
						list.Add( 1063520, ( PrefixOption[99] * levelcheck ).ToString() );
				}
				else
					list.Add( 1063520, ( PrefixOption[99] * levelcheck ).ToString() );
			}
			//장비요구치
			int strReq = AOS.Scale2(StrRequirement, 1000 - GetLowerStatReq());

			if (strReq > 0)
			{
				if( GetLowerStatReq() > 0 )
				{
					if( RootParent != null && RootParent is PlayerMobile )
					{
						PlayerMobile pm = RootParent as PlayerMobile;
						if( pm.Str < strReq )
							list.Add(1063558, "{0}\t{1}\t{2}", strReq.ToString(), StrRequirement.ToString(), (StrRequirement - strReq).ToString()); // strength requirement ~1_val~
						else
							list.Add(1063557, "{0}\t{1}\t{2}", strReq.ToString(), StrRequirement.ToString(), (StrRequirement - strReq).ToString()); // strength requirement ~1_val~
					}
					else
						list.Add(1063557, "{0}\t{1}\t{2}", strReq.ToString(), StrRequirement.ToString(), (StrRequirement - strReq).ToString()); // strength requirement ~1_val~
				}
				else
					list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~
			}
			strReq = AOS.Scale2(DexRequirement, 1000 - GetLowerStatReq());

			if (strReq > 0)
			{
				if( GetLowerStatReq() > 0 )
				{
					if( RootParent != null && RootParent is PlayerMobile )
					{
						PlayerMobile pm = RootParent as PlayerMobile;
						if( pm.Dex < strReq )
							list.Add(1063560, "{0}\t{1}\t{2}", strReq.ToString(), DexRequirement.ToString(), (DexRequirement - strReq).ToString()); // strength requirement ~1_val~
						else
							list.Add(1063559, "{0}\t{1}\t{2}", strReq.ToString(), DexRequirement.ToString(), (DexRequirement - strReq).ToString()); // strength requirement ~1_val~
					}
					else
						list.Add(1063559, "{0}\t{1}\t{2}", strReq.ToString(), DexRequirement.ToString(), (DexRequirement - strReq).ToString()); // strength requirement ~1_val~
				}
				else
					list.Add(1005008, strReq.ToString()); // strength requirement ~1_val~
			}
			strReq = AOS.Scale2(IntRequirement, 1000 - GetLowerStatReq());
			if (strReq > 0)
			{
				if( GetLowerStatReq() > 0 )
				{
					if( RootParent != null && RootParent is PlayerMobile )
					{
						PlayerMobile pm = RootParent as PlayerMobile;
						if( pm.Int < strReq )
							list.Add(1063562, "{0}\t{1}\t{2}", strReq.ToString(), IntRequirement.ToString(), (IntRequirement - strReq).ToString()); // strength requirement ~1_val~
						else
							list.Add(1063561, "{0}\t{1}\t{2}", strReq.ToString(), IntRequirement.ToString(), (IntRequirement - strReq).ToString()); // strength requirement ~1_val~
					}
					else
						list.Add(1063561, "{0}\t{1}\t{2}", strReq.ToString(), IntRequirement.ToString(), (IntRequirement - strReq).ToString()); // strength requirement ~1_val~
				}
				else
					list.Add(1005009, strReq.ToString()); // strength requirement ~1_val~
			}

			if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
				list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~			
			//아이템 등급 색
			//list.Add(Util.ItemRank((int)ItemPower ));
			
            if (m_GorgonLenseCharges > 0)
                list.Add(1112590, m_GorgonLenseCharges.ToString()); //Gorgon Lens Charges: ~1_val~
            
			//if( !Identified )
			//	list.Add( 1060659, "<basefont color=#FF0000>아이템 감정\t안됨<basefont color=#FFFFFF>" );

			if( PrefixOption[0] >= 100 )
			{
				bool skillcheck = false;
				int skilluse = 0;
				int skillname = 0;
				//신규 옵션 정리
				if( PrefixOption[61] + SuffixOption[61] != 0 )
				{
					for( int i = 0; i < 10; ++i)
					{
						if( PrefixOption[i + 61] == 0 && SuffixOption[i + 61] == 0 )
							break;
						
						if( Misc.Util.NewEquipOption[PrefixOption[i + 61], 0, 0] < 60 ) //스킬
						{
							SkillName skill = (SkillName)Enum.ToObject(typeof(SkillName), Misc.Util.NewEquipOption[PrefixOption[i + 61], 0, 0]);
							skillname = m_AosSkillBonuses.GetSkillName(skill);
							if ( skillname > 0 )
							{
								list.Add(1080641 + skilluse, "#{0}\t{1}", skillname, ((double)SuffixOption[i + 61] * 0.01).ToString());
								skillcheck = true;
							}
							skilluse++;
						}
						else
						{
							int optionpercentcheck = 1081997 + Misc.Util.OPLPercentCheck(Misc.Util.NewEquipOption[PrefixOption[i + 61], 0, 0]);
							list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[PrefixOption[i + 61], 0, 0], (((double)SuffixOption[i + 61])*Misc.Util.PercentCalc(PrefixOption[i + 61])).ToString());
						}
					}
				}

				list.Add(1063512); // [마법 옵션]
				for( int i = 0; i < SuffixOption[0]; ++i)
				{
					if( Misc.Util.NewEquipOption[PrefixOption[i + 11], 0, 0] < 60 ) //스킬
					{
						SkillName skill = (SkillName)Enum.ToObject(typeof(SkillName), Misc.Util.NewEquipOption[PrefixOption[i + 11], 0, 0]);
						skillname = m_AosSkillBonuses.GetSkillName(skill);
						if ( skillname > 0 )
						{
							list.Add(1080641 + skilluse, "#{0}\t{1}", skillname, ((double)SuffixOption[i + 11] * 0.01).ToString());
							skillcheck = true;
						}
						skilluse++;
					}
					else
					{
						int optionpercentcheck = 1081999 + Misc.Util.OPLPercentCheck(Misc.Util.NewEquipOption[PrefixOption[i + 11], 0, 0]);
						list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[PrefixOption[i + 11], 0, 0], (((double)SuffixOption[i + 11])*Misc.Util.PercentCalc(PrefixOption[i + 11])).ToString());
					}
				}
				//재료 옵션
				if( PrefixOption[41] != 0 )
				{
					list.Add(1081001);
					list.Add( PrefixOption[41] );
				}
				//재련 옵션
				if( PrefixOption[0] == 100 )
				{
					list.Add(1082001);
					if( SuffixOption[2] > 0 )
					{
						list.Add(1082002, SuffixOption[2].ToString() );
					}
					for(int i = 0; i < 5; ++i )
					{
						if( PrefixOption[31 + i] == -1 )
							break;

						int optionpercentcheck = 1082003 + i + Misc.Util.OPLPercentCheck(Misc.Util.NewEquipOption[PrefixOption[i + 31], 0, 0], 5);
						
						list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[PrefixOption[i + 31], 0, 0], (((double)SuffixOption[i + 31])*Misc.Util.PercentCalc(PrefixOption[i + 31])).ToString() );
					}
				}				
			}
			
			if( Identified )
			{
				/*
				#region Mondain's Legacy Sets
				if (IsSetItem)
				{
					list.Add(1080240, Pieces.ToString()); // Part of a Jewelry Set (~1_val~ pieces)

					if (SetID == SetItem.Bestial)
						list.Add(1151541, BestialSetHelper.GetTotalBerserk(this).ToString()); // Berserk ~1_VAL~

					if (BardMasteryBonus)
						list.Add(1151553); // Activate: Bard Mastery Bonus x2<br>(Effect: 1 min. Cooldown: 30 min.)

					if (m_SetEquipped)
					{
						list.Add(1080241); // Full Jewelry Set Present					
						SetHelper.GetSetProperties(list, this);
					}
				}
				#endregion
				m_NegativeAttributes.GetProperties(list, this);
				*/
			}
			//세트 옵션
			if( PrefixOption[50] != 0 )
			{
				int setcount = 0;
				if( RootParent != null && RootParent is Mobile )
				{
					Mobile from = RootParent as Mobile;
					if( from is PlayerMobile )
					{
						PlayerMobile pm = from as PlayerMobile;
						setcount = pm.ItemSetValue[PrefixOption[50]];
					}
				}

				//list.Add(1084001);
				list.Add(1084100 + PrefixOption[50]);
				int totalset = Misc.Util.SetItemList[PrefixOption[50]].GetLength(0) / 2;
				int maxset = 8;
				for( int i = 0; i < totalset; ++i)
				{
					int equipoption = Misc.Util.SetItemList[PrefixOption[50]][i * 2];
					int equipvalue = Misc.Util.SetItemList[PrefixOption[50]][i * 2 + 1];
					int optionpercentcheck = 1084011 + i + Misc.Util.OPLPercentCheck(Misc.Util.NewEquipOption[equipoption, 0, 0], maxset);

					//Console.WriteLine("first optionpercentcheck : {0}", optionpercentcheck );
					
					if( i < setcount -1 )
						optionpercentcheck += maxset * 2;

					//Console.WriteLine("second optionpercentcheck : {0}", optionpercentcheck );
					list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[equipoption, 0, 0], (((double)equipvalue )* Misc.Util.PercentCalc(equipoption)).ToString() );
				}
			}			
		}

        public override void AddItemPowerProperties(ObjectPropertyList list)
        {
			/*
            if (m_ItemPower != ItemPower.None)
            {
                if (m_ItemPower <= ItemPower.LegendaryArtifact)
                    list.Add(1151488 + ((int)m_ItemPower - 1));
                else
                    list.Add(1152281 + ((int)m_ItemPower - 9));
            }
			*/
        }

        public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (m_Crafter != null)
			{
				LabelTo(from, 1050043, m_Crafter.TitleName); // crafted by ~1_NAME~
			}
			var attrs = new List<EquipInfoAttribute>();

			if (DisplayLootType)
			{
				if (LootType == LootType.Blessed)
				{
					attrs.Add(new EquipInfoAttribute(1038021)); // blessed
				}
				else if (LootType == LootType.Cursed)
				{
					attrs.Add(new EquipInfoAttribute(1049643)); // cursed
				}
				else if (LootType == LootType.Newbied)
				{
					attrs.Add(new EquipInfoAttribute(1032969)); // cursed
				}
			}
		}

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            bool drop = base.DropToWorld(from, p);

            EnchantedHotItemSocket.CheckDrop(from, this);

            return drop;
        }
        
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(19); // version

 			m_AosArmorAttributes.Serialize(writer);
 			m_ExtendedWeaponAttributes.Serialize(writer);
 			m_AosWeaponAttributes.Serialize(writer);
			
			writer.Write(m_IntReq);
 			//웨폰 어빌 & 접두 접미 별도 저장 코드
			for (int i = 0; i < m_PrefixOption.Length; i++)
			{
				writer.Write( (int) m_PrefixOption[i] );
			}
			for (int i = 0; i < m_SuffixOption.Length; i++)
			{
				writer.Write( (int) m_SuffixOption[i] );
			}
			//서브 내구도
			writer.Write(m_HiddenRank);
			
			writer.Write(m_BaseJewelRating);
			
			// 아이템 감정
			writer.Write(m_Identified);
			
            // Version 12 - removed VvV Item (handled in VvV System) and BlockRepair (Handled as negative attribute)

            writer.Write(m_SetPhysicalBonus);
            writer.Write(m_SetFireBonus);
            writer.Write(m_SetColdBonus);
            writer.Write(m_SetPoisonBonus);
            writer.Write(m_SetEnergyBonus);

            writer.Write(m_PlayerConstructed);

            m_TalismanProtection.Serialize(writer);

            writer.Write(_Owner);
            writer.Write(_OwnerName);

            //Version 7
            writer.Write((bool)m_IsImbued);
            
            // Version 6
            m_NegativeAttributes.Serialize(writer);

            // Version 5
            #region Region Reforging
            writer.Write((int)m_ReforgedPrefix);
            writer.Write((int)m_ReforgedSuffix);
            writer.Write((int)m_ItemPower);
            #endregion

            #region Stygian Abyss
            writer.Write(m_GorgonLenseCharges);
            writer.Write((int)m_GorgonLenseType);

            // Version 4
            writer.WriteEncodedInt((int)m_TimesImbued);
			//m_AosWeaponAttributes.Serialize(writer);
            m_SAAbsorptionAttributes.Serialize(writer);
            #endregion

            writer.Write((Mobile)m_BlessedBy);
            writer.Write((bool)m_LastEquipped);
            writer.Write((bool)m_SetEquipped);
            writer.WriteEncodedInt((int)m_SetHue);

            m_SetAttributes.Serialize(writer);
            m_SetSkillBonuses.Serialize(writer);

            writer.Write(m_Crafter);
            writer.Write((int)m_Quality);

            // Version 3
            writer.WriteEncodedInt((int)m_MaxHitPoints);
            writer.WriteEncodedInt((int)m_HitPoints);

            writer.WriteEncodedInt((int)m_Resource);
            writer.WriteEncodedInt((int)m_GemType);

            m_AosAttributes.Serialize(writer);
            m_AosResistances.Serialize(writer);
            m_AosSkillBonuses.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
				case 19: m_AosArmorAttributes = new AosArmorAttributes(this, reader);
						goto case 18;
				case 18: m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this, reader);
						goto case 17;
				case 17: m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						goto case 16;
				case 16: 
				{
					m_IntReq = reader.ReadInt();
					//m_AosWeaponAttributes = new AosWeaponAttributes(this);
					for (int i = 0; i < m_PrefixOption.Length; i++)
					{
						m_PrefixOption[i] = reader.ReadInt();
					}
					for (int i = 0; i < m_SuffixOption.Length; i++)
					{
						m_SuffixOption[i] = reader.ReadInt();
					}
					goto case 15;
				}
				case 15: m_HiddenRank = reader.ReadInt();
						goto case 14;
				case 14: m_BaseJewelRating = reader.ReadInt();
						goto case 13;
				case 13: m_Identified = reader.ReadBool();
						goto case 12;
                case 12:
                case 11:
                    {
                        m_SetPhysicalBonus = reader.ReadInt();
                        m_SetFireBonus = reader.ReadInt();
                        m_SetColdBonus = reader.ReadInt();
                        m_SetPoisonBonus = reader.ReadInt();
                        m_SetEnergyBonus = reader.ReadInt();
                        goto case 10;
                    }
                case 10:
                    {
                        m_PlayerConstructed = reader.ReadBool();
                        goto case 9;
                    }
                case 9:
                    {
                        m_TalismanProtection = new TalismanAttribute(reader);
                        goto case 8;
                    }
                case 8:
                    {
                        if (version == 11)
                            reader.ReadBool();
						
                        _Owner = reader.ReadMobile();
                        _OwnerName = reader.ReadString();
                        goto case 7;
                    }
                case 7:
                    {
                        m_IsImbued = reader.ReadBool();
                        goto case 6;
                    }
                case 6:
                    {
                        m_NegativeAttributes = new NegativeAttributes(this, reader);
                        goto case 5;
                    }
                case 5:
                    {
                        #region Runic Reforging
                        m_ReforgedPrefix = (ReforgedPrefix)reader.ReadInt();
                        m_ReforgedSuffix = (ReforgedSuffix)reader.ReadInt();
                        m_ItemPower = (ItemPower)reader.ReadInt();
						
                        if(version < 12 && reader.ReadBool())
                            m_NegativeAttributes.NoRepair = 1;
                        #endregion

                        #region Stygian Abyss
                        m_GorgonLenseCharges = reader.ReadInt();
                        m_GorgonLenseType = (LenseType)reader.ReadInt();
                        #endregion

                        goto case 4;
                    }
                case 4:
                    {
                        #region Stygian Abyss
                        m_TimesImbued = reader.ReadEncodedInt();
						//m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
                        m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this, reader);
                        #endregion

                        m_BlessedBy = reader.ReadMobile();
                        m_LastEquipped = reader.ReadBool();
                        m_SetEquipped = reader.ReadBool();
                        m_SetHue = reader.ReadEncodedInt();

                        m_SetAttributes = new AosAttributes(this, reader);
                        m_SetSkillBonuses = new AosSkillBonuses(this, reader);

                        m_Crafter = reader.ReadMobile();
                        m_Quality = (ItemQuality)reader.ReadInt();
                        goto case 3;
                    }
                case 3:
                    {
                        m_MaxHitPoints = reader.ReadEncodedInt();
                        m_HitPoints = reader.ReadEncodedInt();
                        goto case 2;
                    }
                case 2:
                    {
                        m_Resource = (CraftResource)reader.ReadEncodedInt();
                        m_GemType = (GemType)reader.ReadEncodedInt();

                        goto case 1;
                    }
                case 1:
                    {
                        m_AosAttributes = new AosAttributes(this, reader);
                        m_AosResistances = new AosElementAttributes(this, reader);
                        m_AosSkillBonuses = new AosSkillBonuses(this, reader);
						//m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						
                        if (Core.AOS && Parent is Mobile)
                            m_AosSkillBonuses.AddTo((Mobile)Parent);

                        int strBonus = m_AosAttributes.BonusStr;
                        int dexBonus = m_AosAttributes.BonusDex;
                        int intBonus = m_AosAttributes.BonusInt;

                        if (Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
                        {
                            Mobile m = (Mobile)Parent;

                            string modName = Serial.ToString();

                            if (strBonus != 0)
                                m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                            if (dexBonus != 0)
                                m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                            if (intBonus != 0)
                                m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
                        }

                        if (Parent is Mobile)
                            ((Mobile)Parent).CheckStatTimers();

                        break;
                    }
                case 0:
                    {
                        m_AosAttributes = new AosAttributes(this);
                        m_AosResistances = new AosElementAttributes(this);
                        m_AosSkillBonuses = new AosSkillBonuses(this);
                        break;
                    }
            }
			if( m_AosArmorAttributes == null )
				m_AosArmorAttributes = new AosArmorAttributes(this);

			if( m_ExtendedWeaponAttributes == null )
				m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);
			if( m_AosWeaponAttributes == null )
				m_AosWeaponAttributes = new AosWeaponAttributes(this);
            if (m_NegativeAttributes == null)
                m_NegativeAttributes = new NegativeAttributes(this);

            if (m_TalismanProtection == null)
                m_TalismanProtection = new TalismanAttribute();

            #region Mondain's Legacy Sets
            if (m_SetAttributes == null)
                m_SetAttributes = new AosAttributes(this);

            if (m_SetSkillBonuses == null)
                m_SetSkillBonuses = new AosSkillBonuses(this);
            #endregion

            if (version < 2)
            {
                m_Resource = CraftResource.Iron;
                m_GemType = GemType.None;
            }
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            PlayerConstructed = true;

            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            if (!craftItem.ForceNonExceptional)
                Resource = CraftResources.GetFromType(resourceType);

            if (1 < craftItem.Resources.Count)
            {
                resourceType = craftItem.Resources.GetAt(1).ItemType;

                if (resourceType == typeof(StarSapphire))
                    GemType = GemType.StarSapphire;
                else if (resourceType == typeof(Emerald))
                    GemType = GemType.Emerald;
                else if (resourceType == typeof(Sapphire))
                    GemType = GemType.Sapphire;
                else if (resourceType == typeof(Ruby))
                    GemType = GemType.Ruby;
                else if (resourceType == typeof(Citrine))
                    GemType = GemType.Citrine;
                else if (resourceType == typeof(Amethyst))
                    GemType = GemType.Amethyst;
                else if (resourceType == typeof(Tourmaline))
                    GemType = GemType.Tourmaline;
                else if (resourceType == typeof(Amber))
                    GemType = GemType.Amber;
                else if (resourceType == typeof(Diamond))
                    GemType = GemType.Diamond;
            }

            #region Mondain's Legacy
            m_Quality = (ItemQuality)quality;

            if (makersMark)
                m_Crafter = from;
            #endregion

			if( from is PlayerMobile )
			{
				double maxValue = 0.8;
				double bonus = 1;
				if (m_Quality == ItemQuality.Exceptional)
				{
					maxValue = 1.0;
					this.MaxHitPoints += 20;
					this.HitPoints += 20;
				}

				/*				
				if( from.Skills.ArmsLore.Value >= 150 )
				{
					maxValue = 1;
					bonus += 1;
				}
				if( from.Skills.ArmsLore.Value >= 200 )
				{
					bonus += 2;
					this.MaxHitPoints = 120;
					this.HitPoints = 120;
					if(Quality == ItemQuality.Exceptional)
					{
						this.MaxHitPoints = 140;
						this.HitPoints = 140;
					}
				}
				*/
				//int rank = Util.ItemRankMaker( from.Skills[craftSystem.MainSkill].Value );
				int rank = Util.ItemRankMaker( from.Skills.ArmsLore.Value, maxValue, bonus );				
				
				//int tier = Util.ItemTierMaker( arms, rank, Misc.Util.ResourceNumberToNumber((int)Resource ), from );
				PlayerMobile pm = from as PlayerMobile;
				Util.NewItemCreate(this, rank, pm );
				//암즈로어 스킬 상승 보너스
				if (m_Quality == ItemQuality.Exceptional)
					pm.CheckSkill(SkillName.ArmsLore, 1500 + rank * 250);
				else
					pm.CheckSkill(SkillName.ArmsLore, 500 + rank * 250);
			}			
			
            return 1;
        }

        #endregion

        #region Mondain's Legacy Sets
        public override bool OnDragLift(Mobile from)
        {
            if (Parent is Mobile && from == Parent)
            {
                if (IsSetItem && m_SetEquipped)
                    SetHelper.RemoveSetBonus(from, SetID, this);
            }

            return base.OnDragLift(from);
        }

        public virtual SetItem SetID
        {
            get
            {
                return SetItem.None;
            }
        }
        public virtual int Pieces
        {
            get
            {
                return 0;
            }
        }

        public virtual bool BardMasteryBonus
        {
            get
            {
                return (SetID == SetItem.Virtuoso);
            }
        }

        public virtual bool MixedSet
        {
            get
            {
                return false;
            }
        }

        public bool IsSetItem
        {
            get
            {
                return SetID == SetItem.None ? false : true;
            }
        }

        private int m_SetHue;
        private bool m_SetEquipped;
        private bool m_LastEquipped;

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetHue
        {
            get
            {
                return m_SetHue;
            }
            set
            {
                m_SetHue = value;
                InvalidateProperties();
            }
        }

        public bool SetEquipped
        {
            get
            {
                return m_SetEquipped;
            }
            set
            {
                m_SetEquipped = value;
            }
        }

        public bool LastEquipped
        {
            get
            {
                return m_LastEquipped;
            }
            set
            {
                m_LastEquipped = value;
            }
        }

        private int m_SetPhysicalBonus, m_SetFireBonus, m_SetColdBonus, m_SetPoisonBonus, m_SetEnergyBonus;

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetPhysicalBonus
        {
            get
            {
                return m_SetPhysicalBonus;
            }
            set
            {
                m_SetPhysicalBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetFireBonus
        {
            get
            {
                return m_SetFireBonus;
            }
            set
            {
                m_SetFireBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetColdBonus
        {
            get
            {
                return m_SetColdBonus;
            }
            set
            {
                m_SetColdBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetPoisonBonus
        {
            get
            {
                return m_SetPoisonBonus;
            }
            set
            {
                m_SetPoisonBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetEnergyBonus
        {
            get
            {
                return m_SetEnergyBonus;
            }
            set
            {
                m_SetEnergyBonus = value;
                InvalidateProperties();
            }
        }

        private AosAttributes m_SetAttributes;
        private AosSkillBonuses m_SetSkillBonuses;

        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes SetAttributes
        {
            get
            {
                return m_SetAttributes;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SetSkillBonuses
        {
            get
            {
                return m_SetSkillBonuses;
            }
            set
            {
            }
        }

        public int SetResistBonus(ResistanceType resist)
        {
            switch (resist)
            {
                case ResistanceType.Physical: return PhysicalResistance;
                case ResistanceType.Fire: return FireResistance;
                case ResistanceType.Cold: return ColdResistance;
                case ResistanceType.Poison: return PoisonResistance;
                case ResistanceType.Energy: return EnergyResistance;
            }

            return 0;
        }
        #endregion
    }
}
