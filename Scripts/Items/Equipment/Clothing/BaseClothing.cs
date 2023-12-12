using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Factions;
using Server.Network;
using Server.Mobiles;
using Server.Misc;
using Server.Spells;

namespace Server.Items
{
    public interface IArcaneEquip
    {
        bool IsArcane { get; }
        int CurArcaneCharges { get; set; }
        int MaxArcaneCharges { get; set; }
        int TempHue { get; set; }
    }

    public abstract class BaseClothing : Item, IDyable, IScissorable, IFactionItem, ICraftable, IWearableDurability, IResource, ISetItem, IVvVItem, IOwnerRestricted, IArtifact, ICombatEquipment, IEngravable, IQuality, IEquipOption
    {
        private string m_EngravedText;

        [CommandProperty(AccessLevel.GameMaster)]
        public string EngravedText
        {
            get { return m_EngravedText; }
            set
            {
                m_EngravedText = value;
                InvalidateProperties();
            }
        }

        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get
            {
                return m_FactionState;
            }
            set
            {
                m_FactionState = value;

                if (m_FactionState == null)
                    Hue = 0;

                LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion

        private bool _VvVItem;
        private Mobile _Owner;
        private string _OwnerName;

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

        public virtual bool CanFortify { get { return !IsImbued && NegativeAttributes.Antique < 4; } }
        public virtual bool CanRepair { get { return m_NegativeAttributes.NoRepair == 0; } }
		public virtual bool CanAlter { get { return true; } }

        private int m_MaxHitPoints;
        private int m_HitPoints;
        private Mobile m_Crafter;
        private ItemQuality m_Quality;
        protected CraftResource m_Resource;
        private int m_StrReq = -1;
        private int m_DexReq = -1;
        private int m_IntReq = -1;

        private bool m_Altered;

        private AosAttributes m_AosAttributes;
        private AosArmorAttributes m_AosClothingAttributes;
        private AosSkillBonuses m_AosSkillBonuses;
        private AosElementAttributes m_AosResistances;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private SAAbsorptionAttributes m_SAAbsorptionAttributes;
        private ExtendedWeaponAttributes m_ExtendedWeaponAttributes;

        private NegativeAttributes m_NegativeAttributes;

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

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get { return m_MaxHitPoints; }
            set
            {
                m_MaxHitPoints = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get { return m_HitPoints; }
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

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get { return m_Crafter; }
            set
            {
                m_Crafter = value;
                InvalidateProperties();
            }
        }
		private int m_BaseArmorRating;
        [CommandProperty(AccessLevel.GameMaster)]
        public int BaseArmorRating
        {
			get { return m_BaseArmorRating; }
			set { m_BaseArmorRating = value;}
        }

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get{ return ( m_StrReq == -1 ? AosStrReq * ( PrefixOption[99] + 1 ) : m_StrReq ); }
			set{ m_StrReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get{ return ( m_DexReq == -1 ? AosDexReq * ( PrefixOption[99] + 1 ) : m_DexReq ); }
			set{ m_DexReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get{ return ( m_IntReq == -1 ? AosIntReq * ( PrefixOption[99] + 1 ) : m_IntReq ); }
			set{ m_IntReq = value; InvalidateProperties(); }
		}		

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemQuality Quality
        {
            get { return m_Quality; }
            set
            {
                m_Quality = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed { get; set; }

        #region Stygian Abyss
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

        [CommandProperty(AccessLevel.GameMaster)]
        public int PhysNonImbuing { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int FireNonImbuing { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ColdNonImbuing { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonNonImbuing { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EnergyNonImbuing { get; set; }

        public virtual int[] BaseResists
        {
            get
            {
                var list = new int[5];

                list[0] = BasePhysicalResistance;
                list[1] = BaseFireResistance;
                list[2] = BaseColdResistance;
                list[3] = BasePoisonResistance;
                list[4] = BaseEnergyResistance;

                return list;
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

        #region Personal Bless Deed
        private Mobile m_BlessedBy;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile BlessedBy
        {
            get { return m_BlessedBy; }
            set
            {
                m_BlessedBy = value;
                InvalidateProperties();
            }
        }
		private int m_HiddenRank;
		[CommandProperty( AccessLevel.GameMaster )]
		public int HiddenRank
		{
			get{ return m_HiddenRank; }
			set{ m_HiddenRank = value; }
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
            private readonly BaseClothing m_Equip;

            public BlassCheck(BaseClothing equip)
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
            private readonly BaseClothing m_Equip;

            public UnBlassCheck(BaseClothing equip)
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
        private class UnBlessEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly BaseClothing m_Item;

            public UnBlessEntry(Mobile from, BaseClothing item)
                : base(6208, -1)
            {
                m_From = from;
                m_Item = item; // BaseArmor, BaseWeapon or BaseClothing
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
        #endregion

        public virtual CraftResource DefaultResource { get { return CraftResource.None; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return m_Resource;
            }
            set
            {
                m_Resource = value;
                Hue = CraftResources.GetHue(m_Resource);
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes { get { return m_AosAttributes; } set { } }

        //[CommandProperty(AccessLevel.GameMaster)]
        //public AosArmorAttributes ClothingAttributes { get { return m_AosClothingAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosArmorAttributes ArmorAttributes { get { return m_AosClothingAttributes; } set { } }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses { get { return m_AosSkillBonuses; } set {  } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosElementAttributes Resistances { get { return m_AosResistances; } set  { } }

        //[CommandProperty(AccessLevel.GameMaster)]
        //public SAAbsorptionAttributes SAAbsorptionAttributes { get { return m_SAAbsorptionAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SAAbsorptionAttributes AbsorptionAttributes { get { return m_SAAbsorptionAttributes; } set { } }

		
        [CommandProperty(AccessLevel.GameMaster)]
        public ExtendedWeaponAttributes ExtendedWeaponAttributes { get { return m_ExtendedWeaponAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public NegativeAttributes NegativeAttributes { get { return m_NegativeAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes { get { return m_AosWeaponAttributes; } set { } }

        public virtual int BasePhysicalResistance { get { return 0; } }
        public virtual int BaseFireResistance { get { return 0; } }
        public virtual int BaseColdResistance { get { return 0; } }
        public virtual int BasePoisonResistance { get { return 0; } }
        public virtual int BaseEnergyResistance { get { return 0; } }
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
       
        #region Mondain's Legacy Sets
        public override int PhysicalResistance
        {
            get
            {
                return BasePhysicalResistance + m_AosResistances.Physical; // + m_AosWeaponAttributes.ResistPhysicalBonus / 100 + m_AosClothingAttributes.AllResist / 100;
            }
        }
        public override int FireResistance
        {
            get
            {
                return BaseFireResistance + m_AosResistances.Fire; // + m_AosWeaponAttributes.ResistFireBonus / 100 + m_AosClothingAttributes.ElementalResist / 100 + m_AosClothingAttributes.AllResist / 100;
            }
        }
        public override int ColdResistance
        {
            get
            {
                return BaseColdResistance + m_AosResistances.Cold; // + m_AosWeaponAttributes.ResistColdBonus / 100 + m_AosClothingAttributes.ElementalResist / 100 + m_AosClothingAttributes.AllResist / 100;
            }
        }
        public override int PoisonResistance
        {
            get
            {
                return BasePoisonResistance + m_AosResistances.Poison; // + m_AosWeaponAttributes.ResistPoisonBonus / 100 + m_AosClothingAttributes.ElementalResist / 100 + m_AosClothingAttributes.AllResist / 100;
            }
        }
        public override int EnergyResistance
        {
            get
            {
                return BaseEnergyResistance + m_AosResistances.Energy; // + m_AosWeaponAttributes.ResistEnergyBonus / 100 + m_AosClothingAttributes.ElementalResist / 100 + m_AosClothingAttributes.AllResist / 100;
            }
        }
        #endregion

        public virtual int ArtifactRarity { get { return 0; } }

        public override bool DisplayWeight
        {
            get
            {
                if (IsVvVItem)
                    return true;

                return base.DisplayWeight;
            }
        }

        public virtual int BaseStrBonus { get { return 0; } }
        public virtual int BaseDexBonus { get { return 0; } }
        public virtual int BaseIntBonus { get { return 0; } }

        public override double DefaultWeight
        {
            get
            {
                if (NegativeAttributes == null || NegativeAttributes.Unwieldly == 0)
                    return base.DefaultWeight;

                return 50;
            }
        }

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
                return false;

            return base.AllowSecureTrade(from, to, newOwner, accepted);
        }

        public virtual Race RequiredRace { get { return null; } }

        #region Stygian Abyss
        public virtual bool CanBeWornByGargoyles
        {
            get
            {
                return false;
            }
        }
        #endregion

        public override bool CanEquip(Mobile from)
        {
            if (!Ethics.Ethic.CheckEquip(from, this))
                return false;

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
                bool morph = from.FindItemOnLayer(Layer.Earrings) is MorphEarrings;

                if (from.Race == Race.Gargoyle && !CanBeWornByGargoyles)
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1111708); // Gargoyles can't wear this.
                    return false;
                }
				/*
				else if( from is PlayerMobile && !Util.EquipCheck( ((PlayerMobile)from), this ) )
				{
					from.SendLocalizedMessage(1071936); // You cannot equip that.
					return false;
				}
                else if (RequiredRace != null && from.Race != RequiredRace && !morph)
                {
                    if (RequiredRace == Race.Elf)
                        from.SendLocalizedMessage(1072203); // Only Elves may use this.
                    else if (RequiredRace == Race.Gargoyle)
                        from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1111707); // Only gargoyles can wear this.
                    else
                        from.SendMessage("Only {0} may use this.", RequiredRace.PluralName);

                    return false;
                }
				*/
                else if (!AllowMaleWearer && !from.Female)
                {
                    if (AllowFemaleWearer)
                        from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1010388); // Only females can wear this.
                    else
                        from.SendLocalizedMessage(1071936); // You cannot equip that.

                    return false;
                }
                else if (!AllowFemaleWearer && from.Female)
                {
                    if (AllowMaleWearer)
                        from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1063343); // Only males can wear this.
                    else
                        from.SendLocalizedMessage(1071936); // You cannot equip that.

                    return false;
                }
                #region Personal Bless Deed
                else if (BlessedBy != null && BlessedBy != from)
                {
                    from.SendLocalizedMessage(1075277); // That item is blessed by another player.

                    return false;
                }
                #endregion
				
                else
                {
                    int strBonus = ComputeStatBonus(StatType.Str), strReq = ComputeStatReq(StatType.Str);
                    int dexBonus = ComputeStatBonus(StatType.Dex), dexReq = ComputeStatReq(StatType.Dex);
                    int intBonus = ComputeStatBonus(StatType.Int), intReq = ComputeStatReq(StatType.Int);
					
					if (from.Dex < AOS.Scale2(dexReq, 1000 - GetLowerStatReq()))
					{
						from.SendLocalizedMessage(502077); // You cannot equip that.
						return false;
					}
					else if (from.Str < AOS.Scale2(strReq, 1000 - GetLowerStatReq()))
					{
						from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
						return false;
					}
					else if (from.Int < AOS.Scale2(intReq, 1000 - GetLowerStatReq()))
					{
						from.SendLocalizedMessage(1071936); // You cannot equip that.
						return false;
					}
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

        public virtual int AosStrReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldStrReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosDexReq
        {
            get
            {
                return 10;
            }
        }
        public virtual int OldDexReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosIntReq
        {
            get
            {
                return 10;
            }
        }
        public virtual int OldIntReq
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
                return 0;
            }
        }
        public virtual int InitMaxHits
        {
            get
            {
                return 0;
            }
        }

        public virtual bool AllowMaleWearer
        {
            get
            {
                return true;
            }
        }
        public virtual bool AllowFemaleWearer
        {
            get
            {
                return true;
            }
        }
        public virtual bool CanBeBlessed
        {
            get
            {
                return true;
            }
        }

        public int ComputeStatReq(StatType type)
        {
            return AOS.Scale(StrRequirement, 100 - GetLowerStatReq());
        }

        public int ComputeStatBonus(StatType type)
        {
            if (type == StatType.Str)
                return BaseStrBonus + Attributes.BonusStr;
            else if (type == StatType.Dex)
                return BaseDexBonus + Attributes.BonusDex;
            else
                return BaseIntBonus + Attributes.BonusInt;
        }
		
        public override bool OnEquip(Mobile from)
        {
            from.CheckStatTimers();

            int strBonus = ComputeStatBonus(StatType.Str);
            int dexBonus = ComputeStatBonus(StatType.Dex);
            int intBonus = ComputeStatBonus(StatType.Int);

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

			if( !Identified && Owner == null )
			{
				Owner = from;
				Identified = true;
			}
			
            Server.Engines.XmlSpawner2.XmlAttach.CheckOnEquip(this, from);

            return base.OnEquip(from);
        }		

        public virtual void AddStatBonuses(Mobile parent)
        {
            if (parent == null)
                return;

            int strBonus = ComputeStatBonus(StatType.Str);
            int dexBonus = ComputeStatBonus(StatType.Dex);
            int intBonus = ComputeStatBonus(StatType.Int);

            if (strBonus == 0 && dexBonus == 0 && intBonus == 0)
                return;

			if( !Identified )
				return;
			
            string modName = Serial.ToString();

            if (strBonus != 0)
                parent.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

            if (dexBonus != 0)
                parent.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

            if (intBonus != 0)
                parent.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			
			
        }

        public static void ValidateMobile(Mobile m)
        {
            for (int i = m.Items.Count - 1; i >= 0; --i)
            {
                if (i >= m.Items.Count)
                    continue;

                Item item = m.Items[i];

                if (item is BaseClothing)
                {
                    BaseClothing clothing = (BaseClothing)item;

                    #region Stygian Abyss
                    if (m.Race == Race.Gargoyle && !clothing.CanBeWornByGargoyles)
                    {
                        m.SendLocalizedMessage(1111708); // Gargoyles can't wear this.
                        m.AddToBackpack(clothing);
                    }
                    #endregion

					/*
                    if (clothing.RequiredRace != null && m.Race != clothing.RequiredRace)
                    {
                        if (clothing.RequiredRace == Race.Elf)
                            m.SendLocalizedMessage(1072203); // Only Elves may use this.
                        #region Stygian Abyss
                        else if (clothing.RequiredRace == Race.Gargoyle)
                            m.SendLocalizedMessage(1111707); // Only gargoyles can wear this.
                        #endregion
                        else
                            m.SendMessage("Only {0} may use this.", clothing.RequiredRace.PluralName);

                        m.AddToBackpack(clothing);
                    }
                    else*/ if (!clothing.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
                    {
                        if (clothing.AllowFemaleWearer)
                            m.SendLocalizedMessage(1010388); // Only females can wear this.
                        else
                            m.SendLocalizedMessage(1071936); // You cannot equip that.

                        m.AddToBackpack(clothing);
                    }
                    else if (!clothing.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
                    {
                        if (clothing.AllowMaleWearer)
                            m.SendLocalizedMessage(1063343); // Only males can wear this.
                        else
                            m.SendLocalizedMessage(1071936); // You cannot equip that.

                        m.AddToBackpack(clothing);
                    }
                }
            }
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

        public override void OnAdded(object parent)
        {
            Mobile mob = parent as Mobile;

            if (mob != null)
            {
                if (Core.AOS)
                    m_AosSkillBonuses.AddTo(mob);

                #region Mondain's Legacy Sets
                if (IsSetItem)
                {
                    m_SetEquipped = SetHelper.FullSetEquipped(mob, SetID, Pieces);

                    if (m_SetEquipped)
                    {
                        m_LastEquipped = true;
                        SetHelper.AddSetBonus(mob, SetID);
                    }
                }
                #endregion

                AddStatBonuses(mob);
                mob.CheckStatTimers();
            }

            base.OnAdded(parent);
        }

        public override void OnRemoved(object parent)
        {
            Mobile mob = parent as Mobile;

            if (mob != null)
            {
                if (Core.AOS)
                    m_AosSkillBonuses.Remove();

                #region Mondain's Legacy Sets
                if (IsSetItem && m_SetEquipped)
                    SetHelper.RemoveSetBonus(mob, SetID, this);
                #endregion

                string modName = Serial.ToString();

                mob.RemoveStatMod(modName + "Str");
                mob.RemoveStatMod(modName + "Dex");
                mob.RemoveStatMod(modName + "Int");

                mob.CheckStatTimers();
            }
            base.OnRemoved(parent);
        }

        public DateTime NextSelfRepair { get; set; }

        public virtual int OnHit(BaseWeapon weapon, int damageTaken)
        {
			m_HiddenRank += damageTaken;
			bool destroy = false;
			int breaken = 1;
			if( m_HiddenRank >= 1000 )
			{
				destroy = true;
				breaken = m_HiddenRank / 1000;
				m_HiddenRank -= 1000 * breaken;
			}
            if ( destroy ) // 25% chance to lower durability
            {
				if (m_MaxHitPoints > 0 + breaken)
				{
					if (m_HitPoints >= 1+ breaken)
						HitPoints -= 1 + breaken;
					else if ( m_MaxHitPoints > 0+ breaken)
					{
						MaxHitPoints -= 1 + breaken;
						
						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
						if (m_MaxHitPoints <= 0+ breaken )
							Delete();
					}
				}
				if( Parent is PlayerMobile )
				{
					PlayerMobile pm = Parent as PlayerMobile;
					//Misc.Util.EquipPoint( pm, this );
				}
            }
            return damageTaken;
        }
        public virtual void OnHit(int damageTaken)
        {
			m_HiddenRank += damageTaken;
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
					if (m_HitPoints >= 1+ breaken)
						HitPoints -= 1 + breaken;
					else if ( m_MaxHitPoints > 0+ breaken)
					{
						MaxHitPoints -= 1 + breaken;
						
						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
						if (m_MaxHitPoints <= 0+ breaken )
							Delete();
					}
				}
            }
        }		
        public BaseClothing(int itemID, Layer layer)
            : this(itemID, layer, 0)
        {
        }

        public BaseClothing(int itemID, Layer layer, int hue)
            : base(itemID)
        {
            Layer = layer;
            Hue = hue;

            m_Resource = DefaultResource;
            m_Quality = ItemQuality.Normal;
			m_Identified = true;

            m_HitPoints = m_MaxHitPoints = InitMinHits; //Utility.RandomMinMax(InitMinHits, InitMaxHits);

            m_AosAttributes = new AosAttributes(this);
            m_AosClothingAttributes = new AosArmorAttributes(this);
            m_AosSkillBonuses = new AosSkillBonuses(this);
            m_AosResistances = new AosElementAttributes(this);
            m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);
			m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);            m_NegativeAttributes = new NegativeAttributes(this);
            m_AosWeaponAttributes = new AosWeaponAttributes(this);

            #region Mondain's Legacy Sets
            m_SetAttributes = new AosAttributes(this);
            m_SetSkillBonuses = new AosSkillBonuses(this);
            #endregion
			/*
			if( this is WizardsHat )
				m_Identified = false;
			else
				m_Identified = true;
			*/
        }

        public override void OnAfterDuped(Item newItem)
        {
            BaseClothing clothing = newItem as BaseClothing;

            if (clothing == null)
                return;

			if(!Identified )
				return;
			
            clothing.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
            clothing.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
            clothing.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
            clothing.m_AosClothingAttributes = new AosArmorAttributes(newItem, m_AosClothingAttributes);
            clothing.m_SAAbsorptionAttributes = new SAAbsorptionAttributes(newItem, m_SAAbsorptionAttributes);
            clothing.m_NegativeAttributes = new NegativeAttributes(newItem, m_NegativeAttributes);
            clothing.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
            clothing.m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(newItem, m_ExtendedWeaponAttributes);

            #region Mondain's Legacy
            clothing.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
            clothing.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
            #endregion

            base.OnAfterDuped(newItem);
        }

        public BaseClothing(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            if (base.AllowEquipedCast(from))
                return true;

            return (m_AosAttributes.SpellChanneling != 0);
        }

        public void UnscaleDurability()
        {
            int scale = 100 + m_AosClothingAttributes.DurabilityBonus;

            m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
            m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;

            InvalidateProperties();
        }

        public void ScaleDurability()
        {
            int scale = 100 + m_AosClothingAttributes.DurabilityBonus;

            m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
            m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;

            if (m_MaxHitPoints > 255)
                m_MaxHitPoints = 255;

            if (m_HitPoints > 255)
                m_HitPoints = 255;

            InvalidateProperties();
        }

        public override bool CheckPropertyConfliction(Mobile m)
        {
            if (base.CheckPropertyConfliction(m))
                return true;

            if (Layer == Layer.Pants)
                return (m.FindItemOnLayer(Layer.InnerLegs) != null);

            if (Layer == Layer.Shirt)
                return (m.FindItemOnLayer(Layer.InnerTorso) != null);

            return false;
        }

        private string GetNameString()
        {
            string name = Name;

            if (name == null)
                name = String.Format("#{0}", LabelNumber);

            return name;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            int oreType;

            switch ( m_Resource )
            {
                case CraftResource.DullCopper:
                    oreType = 1053108;
                    break; // dull copper
                case CraftResource.ShadowIron:
                    oreType = 1053107;
                    break; // shadow iron
                case CraftResource.Copper:
                    oreType = 1053106;
                    break; // copper
                case CraftResource.Bronze:
                    oreType = 1053105;
                    break; // bronze
                case CraftResource.Gold:
                    oreType = 1053104;
                    break; // golden
                case CraftResource.Agapite:
                    oreType = 1053103;
                    break; // agapite
                case CraftResource.Verite:
                    oreType = 1053102;
                    break; // verite
                case CraftResource.Valorite:
                    oreType = 1053101;
                    break; // valorite
				case CraftResource.DernedLeather: oreType = 1051901; break; // 거친 가죽
				case CraftResource.RatnedLeather: oreType = 1051902; break; // 질긴 가죽
				case CraftResource.SernedLeather: oreType = 1051903; break; // 경화 가죽                case CraftResource.SpinedLeather:
                    oreType = 1061118;
                    break; // spined
                case CraftResource.HornedLeather:
                    oreType = 1061117;
                    break; // horned
                case CraftResource.BarbedLeather:
                    oreType = 1061116;
                    break; // barbed
                case CraftResource.RedScales:
                    oreType = 1060814;
                    break; // red
                case CraftResource.YellowScales:
                    oreType = 1060818;
                    break; // yellow
                case CraftResource.BlackScales:
                    oreType = 1060820;
                    break; // black
                case CraftResource.GreenScales:
                    oreType = 1060819;
                    break; // green
                case CraftResource.WhiteScales:
                    oreType = 1060821;
                    break; // white
                case CraftResource.BlueScales:
                    oreType = 1060815;
                    break; // blue
                default:
                    oreType = 0;
                    break;
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
				list.Add(Name);
            }
			/*
            if (m_ReforgedPrefix != ReforgedPrefix.None || m_ReforgedSuffix != ReforgedSuffix.None)
            {
                if (m_ReforgedPrefix != ReforgedPrefix.None)
                {
                    int prefix = RunicReforging.GetPrefixName(m_ReforgedPrefix);

                    if (m_ReforgedSuffix == ReforgedSuffix.None)
                        list.Add(1151757, String.Format("#{0}\t{1}", prefix, GetNameString())); // ~1_PREFIX~ ~2_ITEM~
                    else
                        list.Add(1151756, String.Format("#{0}\t{1}\t#{2}", prefix, GetNameString(), RunicReforging.GetSuffixName(m_ReforgedSuffix))); // ~1_PREFIX~ ~2_ITEM~ of ~3_SUFFIX~
                }
                else if (m_ReforgedSuffix != ReforgedSuffix.None)
                {
                    RunicReforging.AddSuffixName(list, m_ReforgedSuffix, GetNameString());
                }
            }
            else if (oreType != 0)
                list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
            else if (Name == null)
                list.Add(LabelNumber);
            else
                list.Add(Name);
			*/
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
                list.Add(1060636); // Exceptional

            if (IsImbued)
                list.Add(1080418); // (Imbued)

            if (m_Altered)
                list.Add(1111880); // Altered
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            if (IsVvVItem)
                list.Add(1154937); // VvV Item

            if (!string.IsNullOrEmpty(m_EngravedText))
            {
                list.Add(1158847, Utility.FixHtml(m_EngravedText)); // Embroidered: ~1_MESSAGE~	
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            #region Factions
            FactionEquipment.AddFactionProperties(this, list);
            #endregion
			int prop;
			int armorratingcheck = 0;
			
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
			/*
			if( Layer == Layer.Arms || Layer == Layer.Pants || Layer == Layer.Gloves || Layer == Layer.Neck || Layer == Layer.Helm || Layer == Layer.InnerTorso )
				list.Add(1060659, "방어력\t+{0}", 1); // faster casting ~1_val~
			*/
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
				//신규 옵션 정리
				if( PrefixOption[61] + SuffixOption[61] != 0 )
				{
					bool skillcheck = false;
					int skilluse = 5;
					int skillname = 0;
					
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
							list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[PrefixOption[i + 61], 0, 0], (((double)SuffixOption[i + 61])*0.01).ToString());
						}
					}
				}
			}
		
			if( Identified )
			{
				#region Mondain's Legacy Sets
				if (IsSetItem)
				{
					if (MixedSet)
						list.Add(1073491, Pieces.ToString()); // Part of a Weapon/Armor Set (~1_val~ pieces)
					else
						list.Add(1072376, Pieces.ToString()); // Part of an Armor Set (~1_val~ pieces)

					if (SetID == SetItem.Bestial)
						list.Add(1151541, BestialSetHelper.GetTotalBerserk(this).ToString()); // Berserk ~1_VAL~

					if (BardMasteryBonus)
						list.Add(1151553); // Activate: Bard Mastery Bonus x2<br>(Effect: 1 min. Cooldown: 30 min.)

					if (m_SetEquipped)
					{
						if (MixedSet)
							list.Add(1073492); // Full Weapon/Armor Set Present
						else
							list.Add(1072377); // Full Armor Set Present

						GetSetProperties(list);
					}
				}
				#endregion
				//신규 옵션 정리
				if( PrefixOption[0] >= 100 )
				{
					bool skillcheck = false;
					int skilluse = 0;
					int skillname = 0;
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
							list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[PrefixOption[i + 11], 0, 0], (((double)SuffixOption[i + 11])*0.01).ToString());
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
							
							list.Add( optionpercentcheck, "#{0}\t{1}", Misc.Util.NewEquipOption[PrefixOption[i + 31], 0, 0], (((double)SuffixOption[i + 31])*0.01).ToString() );
						}
					}
					
					//강화 옵션
					if( PrefixOption[3] + PrefixOption[4] + PrefixOption[5] + PrefixOption[6] + PrefixOption[7] != 0 )
					{
						list.Add(1083001);
						
						for(int i = 0; i < 7; ++i)
						{
							if( PrefixOption[3 + i] > 0 )
							{
								list.Add( 1083002 + i, "{0}\t{1}", PrefixOption[i + 3], (((double)SuffixOption[i + 3])*0.01).ToString() );
							}
						}
					}
					else
					{

					}
				}

				/*
				if (RequiredRace == Race.Elf)
					list.Add(1075086); // Elves Only
				#region Stygian Abyss
				else if (RequiredRace == Race.Gargoyle)
					list.Add(1111709); // Gargoyles Only
				#endregion
				*/
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
            List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

            AddEquipInfoAttributes(from, attrs);

            int number;

            if (Name == null)
            {
                number = LabelNumber;
            }
            else
            {
                LabelTo(from, Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && Crafter == null && Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, attrs.ToArray());

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            bool drop = base.DropToWorld(from, p);

            EnchantedHotItemSocket.CheckDrop(from, this);

            return drop;
        }

        public virtual void AddEquipInfoAttributes(Mobile from, List<EquipInfoAttribute> attrs)
        {
            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
				else if (LootType == LootType.Newbied)
				{
					attrs.Add(new EquipInfoAttribute(1032969)); // cursed
				}
            }

            #region Factions
            if (m_FactionState != null)
                attrs.Add(new EquipInfoAttribute(1041350)); // faction item
            #endregion

            if (m_Quality == ItemQuality.Exceptional)
                attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));
        }

        #region Serialization
        private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        [Flags]
        private enum SaveFlag
        {
            None = 0x00000000,
            Resource = 0x00000001,
            Attributes = 0x00000002,
            ClothingAttributes = 0x00000004,
            SkillBonuses = 0x00000008,
            Resistances = 0x00000010,
            MaxHitPoints = 0x00000020,
            HitPoints = 0x00000040,
            PlayerConstructed = 0x00000080,
            Crafter = 0x00000100,
            Quality = 0x00000200,
            StrReq = 0x00000400,
            NegativeAttributes  = 0x00000800,
            #region Imbuing
            //TimesImbued = 0x12000000,
            #endregion
            Altered = 0x00001000,
            xWeaponAttributes = 0x00002000,
            DexReq = 0x00000800,
            IntReq = 0x00001000
        }

        #region Mondain's Legacy Sets
        private static void SetSaveFlag(ref SetFlag flags, SetFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private static bool GetSaveFlag(SetFlag flags, SetFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        [Flags]
        private enum SetFlag
        {
            None = 0x00000000,
            Attributes = 0x00000001,
            ArmorAttributes = 0x00000002,
            SkillBonuses = 0x00000004,
            PhysicalBonus = 0x00000008,
            FireBonus = 0x00000010,
            ColdBonus = 0x00000020,
            PoisonBonus = 0x00000040,
            EnergyBonus = 0x00000080,
            SetHue = 0x00000100,
            LastEquipped = 0x00000200,
            SetEquipped = 0x00000400,
            SetSelfRepair = 0x00000800,
        }
        #endregion

        public void xWeaponAttributesDeserializeHelper(GenericReader reader, BaseClothing item)
        {
            SaveFlag flags = (SaveFlag)reader.ReadInt();

            if (flags != SaveFlag.None)
                flags = SaveFlag.xWeaponAttributes;

            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                m_AosWeaponAttributes = new AosWeaponAttributes(item, reader);
            else
                m_AosWeaponAttributes = new AosWeaponAttributes(item);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(16); // version
			
 			m_ExtendedWeaponAttributes.Serialize(writer);

			//접두 접미 별도 저장 코드
			for (int i = 0; i < m_PrefixOption.Length; i++)
			{
				writer.Write( (int) m_PrefixOption[i] );
			}
			for (int i = 0; i < m_SuffixOption.Length; i++)
			{
				writer.Write( (int) m_SuffixOption[i] );
			}
			
			//방어력
			writer.Write(m_BaseArmorRating);
			
			//내구도
			writer.Write(m_HiddenRank);

			// Identified
			writer.Write(m_Identified);
			
            // Embroidery Tool version 11
            writer.Write(m_EngravedText);

            // Version 10 - removed VvV Item (handled in VvV System) and BlockRepair (Handled as negative attribute)

            writer.Write(_Owner);
            writer.Write(_OwnerName);

            //Version 8
            writer.Write((bool)m_IsImbued);

            // Version 7
            m_SAAbsorptionAttributes.Serialize(writer);

            #region Runic Reforging
            writer.Write((int)m_ReforgedPrefix);
            writer.Write((int)m_ReforgedSuffix);
            writer.Write((int)m_ItemPower);
            #endregion

            #region Stygian Abyss
            writer.Write(m_GorgonLenseCharges);
            writer.Write((int)m_GorgonLenseType);

            writer.Write(PhysNonImbuing);
            writer.Write(FireNonImbuing);
            writer.Write(ColdNonImbuing);
            writer.Write(PoisonNonImbuing);
            writer.Write(EnergyNonImbuing);

            // Version 6
            writer.Write((int)m_TimesImbued);

            #endregion

            writer.Write(m_BlessedBy);

            #region Mondain's Legacy Sets
            SetFlag sflags = SetFlag.None;

            SetSaveFlag(ref sflags, SetFlag.Attributes, !m_SetAttributes.IsEmpty);
            SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !m_SetSkillBonuses.IsEmpty);
            SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, m_SetPhysicalBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.FireBonus, m_SetFireBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.ColdBonus, m_SetColdBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.PoisonBonus, m_SetPoisonBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.EnergyBonus, m_SetEnergyBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.SetHue, m_SetHue != 0);
            SetSaveFlag(ref sflags, SetFlag.LastEquipped, m_LastEquipped);
            SetSaveFlag(ref sflags, SetFlag.SetEquipped, m_SetEquipped);
            SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, m_SetSelfRepair != 0);

            writer.WriteEncodedInt((int)sflags);

            if (GetSaveFlag(sflags, SetFlag.Attributes))
                m_SetAttributes.Serialize(writer);

            if (GetSaveFlag(sflags, SetFlag.SkillBonuses))
                m_SetSkillBonuses.Serialize(writer);

            if (GetSaveFlag(sflags, SetFlag.PhysicalBonus))
                writer.WriteEncodedInt((int)m_SetPhysicalBonus);

            if (GetSaveFlag(sflags, SetFlag.FireBonus))
                writer.WriteEncodedInt((int)m_SetFireBonus);

            if (GetSaveFlag(sflags, SetFlag.ColdBonus))
                writer.WriteEncodedInt((int)m_SetColdBonus);

            if (GetSaveFlag(sflags, SetFlag.PoisonBonus))
                writer.WriteEncodedInt((int)m_SetPoisonBonus);

            if (GetSaveFlag(sflags, SetFlag.EnergyBonus))
                writer.WriteEncodedInt((int)m_SetEnergyBonus);

            if (GetSaveFlag(sflags, SetFlag.SetHue))
                writer.WriteEncodedInt((int)m_SetHue);

            if (GetSaveFlag(sflags, SetFlag.LastEquipped))
                writer.Write((bool)m_LastEquipped);

            if (GetSaveFlag(sflags, SetFlag.SetEquipped))
                writer.Write((bool)m_SetEquipped);

            if (GetSaveFlag(sflags, SetFlag.SetSelfRepair))
                writer.WriteEncodedInt((int)m_SetSelfRepair);
            #endregion

            // Version 5
            SaveFlag flags = SaveFlag.None;

            SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.NegativeAttributes, !m_NegativeAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != DefaultResource);
            SetSaveFlag(ref flags, SaveFlag.Attributes, !m_AosAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.ClothingAttributes, !m_AosClothingAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Resistances, !m_AosResistances.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, m_MaxHitPoints != 0);
            SetSaveFlag(ref flags, SaveFlag.HitPoints, m_HitPoints != 0);
            SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, PlayerConstructed != false);
            SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
            SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != ItemQuality.Normal);
            SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
            SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
            SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
            #region Imbuing
            //SetSaveFlag(ref flags, SaveFlag.TimesImbued, m_TimesImbued != 0);
            #endregion
            SetSaveFlag(ref flags, SaveFlag.Altered, m_Altered);

            writer.WriteEncodedInt((int)flags);

            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                m_AosWeaponAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.NegativeAttributes))
                m_NegativeAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.Resource))
                writer.WriteEncodedInt((int)m_Resource);

            if (GetSaveFlag(flags, SaveFlag.Attributes))
                m_AosAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.ClothingAttributes))
                m_AosClothingAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                m_AosSkillBonuses.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.Resistances))
                m_AosResistances.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.MaxHitPoints))
                writer.WriteEncodedInt((int)m_MaxHitPoints);

            if (GetSaveFlag(flags, SaveFlag.HitPoints))
                writer.WriteEncodedInt((int)m_HitPoints);

            if (GetSaveFlag(flags, SaveFlag.Crafter))
                writer.Write((Mobile)m_Crafter);

            if (GetSaveFlag(flags, SaveFlag.Quality))
                writer.WriteEncodedInt((int)m_Quality);

            if (GetSaveFlag(flags, SaveFlag.StrReq))
                writer.WriteEncodedInt((int)m_StrReq);

            if (GetSaveFlag(flags, SaveFlag.DexReq))
                writer.WriteEncodedInt((int)m_DexReq);

            if (GetSaveFlag(flags, SaveFlag.IntReq))
                writer.WriteEncodedInt((int)m_IntReq);

		}

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
				case 16: m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this, reader);
						goto case 15;
				case 15:
				{
					for (int i = 0; i < m_PrefixOption.Length; i++)
					{
						m_PrefixOption[i] = reader.ReadInt();
					}
					for (int i = 0; i < m_SuffixOption.Length; i++)
					{
						m_SuffixOption[i] = reader.ReadInt();
					}
					m_BaseArmorRating = reader.ReadInt();
					goto case 14;
				}
				case 14:
				{
					m_HiddenRank = reader.ReadInt();
					goto case 13;
				}
				case 13:
				{
					m_Identified = reader.ReadBool();
					goto case 12;
				}
                case 12:
                case 11:
                    {
                        m_EngravedText = reader.ReadString();
                        goto case 9;
                    }
                case 10:
                case 9:
                    {
                        if (version == 9)
                            reader.ReadBool();

                        _Owner = reader.ReadMobile();
                        _OwnerName = reader.ReadString();
                        goto case 8;
                    }
                case 8:
                        {
                            m_IsImbued = reader.ReadBool();
                            goto case 7;
                        }
                case 7:
                    {
                        m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this, reader);

                        #region Runic Reforging
                        m_ReforgedPrefix = (ReforgedPrefix)reader.ReadInt();
                        m_ReforgedSuffix = (ReforgedSuffix)reader.ReadInt();
                        m_ItemPower = (ItemPower)reader.ReadInt();

                        if (version == 9 && reader.ReadBool())
                        {
                            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                                {
                                    m_NegativeAttributes.NoRepair = 1;
                                });
                        }
                        #endregion

                        #region Stygian Abyss
                        m_GorgonLenseCharges = reader.ReadInt();
                        m_GorgonLenseType = (LenseType)reader.ReadInt();

                        PhysNonImbuing = reader.ReadInt();
                        FireNonImbuing = reader.ReadInt();
                        ColdNonImbuing = reader.ReadInt();
                        PoisonNonImbuing = reader.ReadInt();
                        EnergyNonImbuing = reader.ReadInt();
                        goto case 6;
                    }
                case 6:
                    {
                        if(version == 6)
                            m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);

                        m_TimesImbued = reader.ReadInt();
                       
                        #endregion

                        m_BlessedBy = reader.ReadMobile();

                        #region Mondain's Legacy Sets
                        SetFlag sflags = (SetFlag)reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.Attributes))
                            m_SetAttributes = new AosAttributes(this, reader);
                        else
                            m_SetAttributes = new AosAttributes(this);

                        if (GetSaveFlag(sflags, SetFlag.ArmorAttributes))
                            m_SetSelfRepair = (new AosArmorAttributes(this, reader)).SelfRepair;

                        if (GetSaveFlag(sflags, SetFlag.SkillBonuses))
                            m_SetSkillBonuses = new AosSkillBonuses(this, reader);
                        else
                            m_SetSkillBonuses = new AosSkillBonuses(this);

                        if (GetSaveFlag(sflags, SetFlag.PhysicalBonus))
                            m_SetPhysicalBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.FireBonus))
                            m_SetFireBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.ColdBonus))
                            m_SetColdBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.PoisonBonus))
                            m_SetPoisonBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.EnergyBonus))
                            m_SetEnergyBonus = reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.SetHue))
                            m_SetHue = reader.ReadEncodedInt();

                        if (GetSaveFlag(sflags, SetFlag.LastEquipped))
                            m_LastEquipped = reader.ReadBool();

                        if (GetSaveFlag(sflags, SetFlag.SetEquipped))
                            m_SetEquipped = reader.ReadBool();

                        if (GetSaveFlag(sflags, SetFlag.SetSelfRepair))
                            m_SetSelfRepair = reader.ReadEncodedInt();
                        #endregion

                        goto case 5;
                    }
                case 5:
                    {
                        SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

                        if (version > 11)
                        {
                            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                                m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
                            else
                                m_AosWeaponAttributes = new AosWeaponAttributes(this);
                        }

                        if (GetSaveFlag(flags, SaveFlag.NegativeAttributes))
                            m_NegativeAttributes = new NegativeAttributes(this, reader);
                        else
                            m_NegativeAttributes = new NegativeAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.Resource))
                            m_Resource = (CraftResource)reader.ReadEncodedInt();
                        else
                            m_Resource = DefaultResource;

                        if (GetSaveFlag(flags, SaveFlag.Attributes))
                            m_AosAttributes = new AosAttributes(this, reader);
                        else
                            m_AosAttributes = new AosAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.ClothingAttributes))
                            m_AosClothingAttributes = new AosArmorAttributes(this, reader);
                        else
                            m_AosClothingAttributes = new AosArmorAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                            m_AosSkillBonuses = new AosSkillBonuses(this, reader);
                        else
                            m_AosSkillBonuses = new AosSkillBonuses(this);

                        if (GetSaveFlag(flags, SaveFlag.Resistances))
                            m_AosResistances = new AosElementAttributes(this, reader);
                        else
                            m_AosResistances = new AosElementAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.MaxHitPoints))
                            m_MaxHitPoints = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.HitPoints))
                            m_HitPoints = reader.ReadEncodedInt();

                        if (GetSaveFlag(flags, SaveFlag.Crafter))
                            m_Crafter = reader.ReadMobile();

                        if (GetSaveFlag(flags, SaveFlag.Quality))
                            m_Quality = (ItemQuality)reader.ReadEncodedInt();
                        else
                            m_Quality = ItemQuality.Normal;

                        if (GetSaveFlag(flags, SaveFlag.StrReq))
                            m_StrReq = reader.ReadEncodedInt();
                        else
                            m_StrReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.DexReq))
                            m_DexReq = reader.ReadEncodedInt();
                        else
                            m_DexReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.IntReq))
                            m_IntReq = reader.ReadEncodedInt();
                        else
                            m_IntReq = -1;
						
						
                        if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
                            PlayerConstructed = true;

                        if (GetSaveFlag(flags, SaveFlag.Altered))
                            m_Altered = true;

                        break;
                    }
                case 4:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();

                        goto case 3;
                    }
                case 3:
                    {
                        m_AosAttributes = new AosAttributes(this, reader);
                        m_AosClothingAttributes = new AosArmorAttributes(this, reader);
                        m_AosSkillBonuses = new AosSkillBonuses(this, reader);
                        m_AosResistances = new AosElementAttributes(this, reader);

                        goto case 2;
                    }
                case 2:
                    {
                        PlayerConstructed = reader.ReadBool();
                        goto case 1;
                    }
                case 1:
                    {
                        m_Crafter = reader.ReadMobile();
                        m_Quality = (ItemQuality)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        m_Crafter = null;
                        m_Quality = ItemQuality.Normal;
                        break;
                    }
            }
			if( m_ExtendedWeaponAttributes == null )
				m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);

            #region Mondain's Legacy Sets
            if (m_SetAttributes == null)
                m_SetAttributes = new AosAttributes(this);

            if (m_SetSkillBonuses == null)
                m_SetSkillBonuses = new AosSkillBonuses(this);
            #endregion

            if (version < 2)
                PlayerConstructed = true; // we don't know, so, assume it's crafted

            if (version < 3)
            {
                m_AosAttributes = new AosAttributes(this);
                m_AosClothingAttributes = new AosArmorAttributes(this);
                m_AosSkillBonuses = new AosSkillBonuses(this);
                m_AosResistances = new AosElementAttributes(this);
            }

            if (m_AosWeaponAttributes == null)
                m_AosWeaponAttributes = new AosWeaponAttributes(this);

            if (version < 4)
                m_Resource = DefaultResource;

            if (m_MaxHitPoints == 0 && m_HitPoints == 0)
                m_HitPoints = m_MaxHitPoints = InitMinHits;//Utility.RandomMinMax(InitMinHits, InitMaxHits);

            Mobile parent = Parent as Mobile;

            if (parent != null)
            {
                if (Core.AOS)
                    m_AosSkillBonuses.AddTo(parent);

                AddStatBonuses(parent);
                parent.CheckStatTimers();
            }
        }

        #endregion

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;
            else if (RootParent is Mobile && from != RootParent)
                return false;

            Hue = sender.DyedHue;

            return true;
        }

        public virtual bool Scissor(Mobile from, Scissors scissors)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(502437); // Items you wish to cut must be in your backpack.
                return false;
            }

            if (Ethics.Ethic.IsImbued(this))
            {
                from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
                return false;
            }

            CraftSystem system = DefTailoring.CraftSystem;

            CraftItem item = system.CraftItems.SearchFor(GetType());

            if (item != null && item.Resources.Count == 1 && item.Resources.GetAt(0).Amount >= 2)
            {
                try
                {
                    Type resourceType = null;

                    CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

                    if (info != null && info.ResourceTypes.Length > 0)
                        resourceType = info.ResourceTypes[0];

                    if (resourceType == null)
                        resourceType = item.Resources.GetAt(0).ItemType;

                    Item res = (Item)Activator.CreateInstance(resourceType);

                    ScissorHelper(from, res, PlayerConstructed ? (item.Resources.GetAt(0).Amount / 2) : 1);

                    res.LootType = LootType.Regular;

                    return true;
                }
                catch
                {
                }
            }

            from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
            return false;
        }

        public void DistributeBonuses(Mobile from, int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                switch ( Utility.Random(5) )
                {
                    case 0: ++m_AosResistances.Physical; break;
                    case 1: ++m_AosResistances.Fire; break;
                    case 2: ++m_AosResistances.Cold; break;
                    case 3: ++m_AosResistances.Poison; break;
                    case 4: ++m_AosResistances.Energy; break;
                }
            }

            // Arms Lore Bonus - Verified on EA
            if (Core.ML && from != null)
            {
                double div = Siege.SiegeShard ? 12.5 : 20;
                int bonus = (int)Math.Min(4, (from.Skills.ArmsLore.Value / div));

                for (int i = 0; i < bonus; i++)
                {
                    switch (Utility.Random(5))
                    {
                        case 0: Resistances.Physical++; break;
                        case 1: Resistances.Fire++; break;
                        case 2: Resistances.Cold++; break;
                        case 3: Resistances.Poison++; break;
                        case 4: Resistances.Energy++; break;
                    }
                }

                from.CheckSkill(SkillName.ArmsLore, 0, 100);
            }

            #region Stygian Abyss
            PhysNonImbuing = PhysicalResistance;
            FireNonImbuing = FireResistance;
            ColdNonImbuing = ColdResistance;
            PoisonNonImbuing = PoisonResistance;
            EnergyNonImbuing = EnergyResistance;
            #endregion

            InvalidateProperties();
        }

        #region ICraftable Members

        public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            Quality = (ItemQuality)quality;

            if (makersMark)
                Crafter = from;

            PlayerConstructed = true;

			if (typeRes == null)
			{
				typeRes = craftItem.Resources.GetAt(0).ItemType;
			}

			Resource = CraftResources.GetFromType(typeRes);

			if( from is PlayerMobile )
			{
				int arms = (int)from.Skills.ArmsLore.Value * 100;
				if (Quality == ItemQuality.Exceptional)
					arms += 5000;
				
				int rank = Util.ItemRankMaker( from.Skills[craftSystem.MainSkill].Value * 4 );
				
				PlayerMobile pm = from as PlayerMobile;
				Util.NewItemCreate(this, rank, pm );
			}				
		

            return quality;
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

        private AosAttributes m_SetAttributes;
        private AosSkillBonuses m_SetSkillBonuses;
        private int m_SetSelfRepair;

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

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetSelfRepair
        {
            get
            {
                return m_SetSelfRepair;
            }
            set
            {
                m_SetSelfRepair = value;
                InvalidateProperties();
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

        public virtual void GetSetProperties(ObjectPropertyList list)
        {
            int prop;

            if (!m_SetEquipped)
            {
                if (SetID == SetItem.Virtuoso)
                    list.Add(1151571); // Mastery Bonus Cooldown: 15 min.

                if (m_SetPhysicalBonus != 0)
                    list.Add(1072382, m_SetPhysicalBonus.ToString()); // physical resist +~1_val~%

                if (m_SetFireBonus != 0)
                    list.Add(1072383, m_SetFireBonus.ToString()); // fire resist +~1_val~%

                if (m_SetColdBonus != 0)
                    list.Add(1072384, m_SetColdBonus.ToString()); // cold resist +~1_val~%

                if (m_SetPoisonBonus != 0)
                    list.Add(1072385, m_SetPoisonBonus.ToString()); // poison resist +~1_val~%

                if (m_SetEnergyBonus != 0)
                    list.Add(1072386, m_SetEnergyBonus.ToString()); // energy resist +~1_val~%			
            }
            else if (m_SetEquipped && SetHelper.ResistsBonusPerPiece(this) && RootParentEntity is Mobile)
            {
                Mobile m = (Mobile)RootParentEntity;

                if (m_SetPhysicalBonus != 0)
                    list.Add(1080361, SetHelper.GetSetTotalResist(m, ResistanceType.Physical).ToString()); // physical resist ~1_val~% (total)

                if (m_SetFireBonus != 0)
                    list.Add(1080362, SetHelper.GetSetTotalResist(m, ResistanceType.Fire).ToString()); // fire resist ~1_val~% (total)

                if (m_SetColdBonus != 0)
                    list.Add(1080363, SetHelper.GetSetTotalResist(m, ResistanceType.Cold).ToString()); // cold resist ~1_val~% (total)

                if (m_SetPoisonBonus != 0)
                    list.Add(1080364, SetHelper.GetSetTotalResist(m, ResistanceType.Poison).ToString()); // poison resist ~1_val~% (total)

                if (m_SetEnergyBonus != 0)
                    list.Add(1080365, SetHelper.GetSetTotalResist(m, ResistanceType.Energy).ToString()); // energy resist ~1_val~% (total)
            }

            if ((prop = m_SetSelfRepair) != 0)
                list.Add(1060450, prop.ToString()); // self repair ~1_val~		

            SetHelper.GetSetProperties(list, this);
        }

        public int SetResistBonus(ResistanceType resist)
        {
            if (SetHelper.ResistsBonusPerPiece(this))
            {
                switch (resist)
                {
                    case ResistanceType.Physical: return m_SetEquipped ? PhysicalResistance + m_SetPhysicalBonus : PhysicalResistance;
                    case ResistanceType.Fire: return m_SetEquipped ? FireResistance + m_SetFireBonus : FireResistance;
                    case ResistanceType.Cold: return m_SetEquipped ? ColdResistance + m_SetColdBonus : ColdResistance;
                    case ResistanceType.Poison: return m_SetEquipped ? PoisonResistance + m_SetPoisonBonus : PoisonResistance;
                    case ResistanceType.Energy: return m_SetEquipped ? EnergyResistance + m_SetEnergyBonus : EnergyResistance;
                }
            }
            else
            {
                switch (resist)
                {
                    case ResistanceType.Physical: return m_SetEquipped ? LastEquipped ? (PhysicalResistance * Pieces) + m_SetPhysicalBonus : 0 : PhysicalResistance;
                    case ResistanceType.Fire: return m_SetEquipped ? LastEquipped ? (FireResistance * Pieces) + m_SetFireBonus : 0 : FireResistance;
                    case ResistanceType.Cold: return m_SetEquipped ? LastEquipped ? (ColdResistance * Pieces) + m_SetColdBonus : 0 : ColdResistance;
                    case ResistanceType.Poison: return m_SetEquipped ? LastEquipped ? (PoisonResistance * Pieces) + m_SetPoisonBonus : 0 : PoisonResistance;
                    case ResistanceType.Energy: return m_SetEquipped ? LastEquipped ? (EnergyResistance * Pieces) + m_SetEnergyBonus : 0 : EnergyResistance;
                }
            }

            return 0;
        }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Altered
        {
            get { return m_Altered; }
            set
            {
                m_Altered = value;
                InvalidateProperties();
            }
        }
    }
}
