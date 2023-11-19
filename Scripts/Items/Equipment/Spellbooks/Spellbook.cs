#region References
using System;
using System.Collections.Generic;
 using Server.ContextMenus;
using Server.Commands;
using Server.Engines.Craft;
using Server.Ethics;
using Server.Multis;
using Server.Network;
using Server.Spells;
using Server.Targeting;
using Server.Mobiles;
using Server.Spells.Mysticism;
using Server.Factions;
using Server.Misc;
#endregion

namespace Server.Items
{
	public enum SpellbookType
	{
		Invalid = -1,
		Regular,
		Necromancer,
		Paladin,
		Ninja,
		Samurai,
		Arcanist,
		Mystic,
        SkillMasteries,
		Magery
	}

	public enum BookQuality
	{
		Regular,
		Exceptional,
	}

	
	
    public class Spellbook : Item, ICraftable, ISlayer, IEngravable, IVvVItem, IOwnerRestricted, IWearableDurability, IFactionItem, IEquipOption
	{
		private static readonly Dictionary<Mobile, List<Spellbook>> m_Table = new Dictionary<Mobile, List<Spellbook>>();

		private static readonly int[] m_LegendPropertyCounts = new[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0 properties : 21/52 : 40%
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 1 property   : 15/52 : 29%
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 2 properties : 10/52 : 19%
			3, 3, 3, 3, 3, 3 // 3 properties :  6/52 : 12%
		};

		private static readonly int[] m_ElderPropertyCounts = new[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0 properties : 15/34 : 44%
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 1 property   : 10/34 : 29%
			2, 2, 2, 2, 2, 2, // 2 properties :  6/34 : 18%
			3, 3, 3 // 3 properties :  3/34 :  9%
		};

		private static readonly int[] m_GrandPropertyCounts = new[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0 properties : 10/20 : 50%
			1, 1, 1, 1, 1, 1, // 1 property   :  6/20 : 30%
			2, 2, 2, // 2 properties :  3/20 : 15%
			3 // 3 properties :  1/20 :  5%
		};

		private static readonly int[] m_MasterPropertyCounts = new[]
		{
			0, 0, 0, 0, 0, 0, // 0 properties : 6/10 : 60%
			1, 1, 1, // 1 property   : 3/10 : 30%
			2 // 2 properties : 1/10 : 10%
		};

		private static readonly int[] m_AdeptPropertyCounts = new[]
		{
			0, 0, 0, // 0 properties : 3/4 : 75%
			1 // 1 property   : 1/4 : 25%
		};

        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get { return m_FactionState; }
            set
            {
                m_FactionState = value;

                LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion

		private string m_EngravedText;
		private BookQuality m_Quality;
		private AosAttributes m_AosAttributes;
        private AosArmorAttributes m_AosArmorAttributes;
        private AosElementAttributes m_AosResistances;
		private AosSkillBonuses m_AosSkillBonuses;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private NegativeAttributes m_NegativeAttributes;
		private SAAbsorptionAttributes m_AbsorptionAttributes;
        private ExtendedWeaponAttributes m_ExtendedWeaponAttributes;
		private ulong m_Content;
		private int m_Count;
		private Mobile m_Crafter;
		private SlayerName m_Slayer;
		private SlayerName m_Slayer2;
		public virtual int ArtifactRarity { get { return 0; } }

		
		
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
		//Currently though there are no dual slayer spellbooks, OSI has a habit of putting dual slayer stuff in later
		[Constructable]
		public Spellbook()
			: this(ulong.MaxValue)
		{ }

		[Constructable]
		public Spellbook(ulong content)
			: this( ulong.MaxValue, 0xEFA)
		{ }


		private int m_StrReq, m_DexReq, m_IntReq;
		public virtual int AosStrengthReq { get { return 0; } }
		public virtual int AosDexterityReq { get { return 0; } }
		public virtual int AosIntelligenceReq { get { return 0; } }
		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get{ return ( m_StrReq == -1 ? AosStrengthReq * ( PrefixOption[99] + 1 ) : m_StrReq ); }
			set{ m_StrReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get{ return ( m_DexReq == -1 ? AosDexterityReq * ( PrefixOption[99] + 1 ) : m_DexReq ); }
			set{ m_DexReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get{ return ( m_IntReq == -1 ? AosIntelligenceReq * ( PrefixOption[99] + 1 ) : m_IntReq ); }
			set{ m_IntReq = value; InvalidateProperties(); }
		}


		public Spellbook(Serial serial)
			: base(serial)
		{ }

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

		[CommandProperty(AccessLevel.GameMaster)]
		public BookQuality Quality
		{
			get { return m_Quality; }
			set
			{
				m_Quality = value;
				InvalidateProperties();
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
		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes Attributes { get { return m_AosAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosArmorAttributes ArmorAttributes { get { return m_AosArmorAttributes; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public SAAbsorptionAttributes AbsorptionAttributes { get { return m_AbsorptionAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ExtendedWeaponAttributes ExtendedWeaponAttributes { get { return m_ExtendedWeaponAttributes; } set { } }

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
		public AosSkillBonuses SkillBonuses { get { return m_AosSkillBonuses; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes { get { return m_AosWeaponAttributes; } set { } }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public NegativeAttributes NegativeAttributes { get { return m_NegativeAttributes; } set { } }

		public virtual SpellbookType SpellbookType { get { return SpellbookType.Regular; } }
		public virtual int BookOffset { get { return 0; } }
		public virtual int BookCount { get { return 64; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public ulong Content
		{
			get { return m_Content; }
			set
			{
				if (m_Content != value)
				{
					m_Content = value;

					m_Count = 0;

					while (value > 0)
					{
						m_Count += (int)(value & 0x1);
						value >>= 1;
					}

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SpellCount { get { return m_Count; } }

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

		public override bool DisplayLootType { get { return Core.AOS; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer
		{
			get { return m_Slayer; }
			set
			{
				m_Slayer = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer2
		{
			get { return m_Slayer2; }
			set
			{
				m_Slayer2 = value;
				InvalidateProperties();
			}
		}

        #region IVvVItem / IOwnerRestricted
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
        #endregion

        #region IWearableDurability
        private int m_MaxHitPoints;
        private int m_HitPoints;

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get { return m_HitPoints; }
            set
            {
                if (m_HitPoints == value)
                {
                    return;
                }

                if (value > m_MaxHitPoints)
                {
                    value = m_MaxHitPoints;
                }

                m_HitPoints = value;

                InvalidateProperties();
            }
        }

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

        private ItemPower m_ItemPower;
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

        private ReforgedPrefix m_ReforgedPrefix;
        [CommandProperty(AccessLevel.GameMaster)]
        public ReforgedPrefix ReforgedPrefix
        {
            get { return m_ReforgedPrefix; }
            set { m_ReforgedPrefix = value; InvalidateProperties(); }
        }

        private ReforgedSuffix m_ReforgedSuffix;
        [CommandProperty(AccessLevel.GameMaster)]
        public ReforgedSuffix ReforgedSuffix
        {
            get { return m_ReforgedSuffix; }
            set { m_ReforgedSuffix = value; InvalidateProperties(); }
        }

		
        public override int PhysicalResistance
        {
            get
            {
                return m_AosResistances.Physical + m_AosWeaponAttributes.ResistPhysicalBonus;
            }
        }
        public override int FireResistance
        {
            get
            {
                return m_AosResistances.Fire + m_AosWeaponAttributes.ResistFireBonus;
            }
        }
        public override int ColdResistance
        {
            get
            {
                return m_AosResistances.Cold + m_AosWeaponAttributes.ResistColdBonus;
            }
        }
        public override int PoisonResistance
        {
            get
            {
                return m_AosResistances.Poison + m_AosWeaponAttributes.ResistPoisonBonus;
            }
        }
        public override int EnergyResistance
        {
            get
            {
                return m_AosResistances.Energy + m_AosWeaponAttributes.ResistEnergyBonus;
            }
        }		
        public virtual bool CanFortify { get { return false; } }
        public virtual bool CanRepair { get { return m_NegativeAttributes.NoRepair == 0; } }

        public virtual int InitMinHits { get { return 0; } }
        public virtual int InitMaxHits { get { return 0; } }
		private int m_HiddenRank;
		[CommandProperty( AccessLevel.GameMaster )]
		public int HiddenRank
		{
			get{ return m_HiddenRank; }
			set{ m_HiddenRank = value; }
		}

        public virtual void ScaleDurability()
        {
        }

        public virtual void UnscaleDurability()
        {
        }
		public Spellbook(ulong content, int itemID)
			: base(itemID)
		{
			m_AosAttributes = new AosAttributes(this);
            m_AosResistances = new AosElementAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
            m_AosWeaponAttributes = new AosWeaponAttributes(this);
            m_AosArmorAttributes = new AosArmorAttributes(this);
            m_NegativeAttributes = new NegativeAttributes(this);
			m_AbsorptionAttributes = new SAAbsorptionAttributes(this);
			m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);

			m_HitPoints = m_MaxHitPoints = InitMinHits; //Utility.RandomMinMax(InitMinHits, InitMaxHits);

			Weight = 3.0;
			Layer = Layer.OneHanded;
			//Name = "마법책";
			m_Identified = true;
			//LootType = LootType.Blessed;
			m_IntReq = -1;

			Content = content;
		}
        public virtual int OnHit(BaseWeapon weap, int damage)
        {
			m_HiddenRank += damage;
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
				if (MaxHitPoints > 0 + breaken)
				{
					if (HitPoints >= 1 + breaken)
						HitPoints -= 1 + breaken;
					else if ( MaxHitPoints > 0 + breaken)
					{
						MaxHitPoints-= 1 + breaken;
						
						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
						if (MaxHitPoints <= 0 + breaken )
							Delete();
					}
				}
				if( Parent is PlayerMobile )
				{
					PlayerMobile pm = Parent as PlayerMobile;
					//Misc.Util.EquipPoint( pm, this );
				}
            }
            return damage;
        }
        #endregion
        public virtual void OnAttack()
        {
			m_HiddenRank += 40;
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
				if (MaxHitPoints > 0 + breaken)
				{
					if (HitPoints >= 1 + breaken)
						HitPoints -= 1 + breaken;
					else if ( MaxHitPoints > 0 + breaken)
					{
						MaxHitPoints-= 1 + breaken;
						
						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
						if (MaxHitPoints <= 0 + breaken )
							Delete();
					}
				}
            }
        }
        public static void Initialize()
		{
			EventSink.OpenSpellbookRequest += EventSink_OpenSpellbookRequest;
			EventSink.CastSpellRequest += EventSink_CastSpellRequest;
            EventSink.TargetedSpell += Targeted_Spell;       

			CommandSystem.Register("AllSpells", AccessLevel.GameMaster, AllSpells_OnCommand);
		}

        #region Enhanced Client
        private static void Targeted_Spell(TargetedSpellEventArgs e)
        {
            try
            {
                Mobile from = e.Mobile;

                if (!DesignContext.Check(from))
                {
                    return; // They are customizing
                }

                Spellbook book = null;
                int spellID = e.SpellID;

                if (book == null || !book.HasSpell(spellID))
                {
                    book = Find(from, spellID);
                }

                if (book != null && book.HasSpell(spellID))
                {
                    SpecialMove move = SpellRegistry.GetSpecialMove(spellID);

                    if (move != null)
                    {
                        SpecialMove.SetCurrentMove(from, move);
                    }
                    else
                    {
                        Mobile to = World.FindMobile(e.Target.Serial);
                        Item toI = World.FindItem(e.Target.Serial);
                        Spell spell = SpellRegistry.NewSpell(spellID, from, null);

                        if (to != null)
                        {
                            spell.InstantTarget = to;
                        }
                        else if (toI != null)
                        {
                            spell.InstantTarget = toI as IDamageableItem;
                        }

                        if (spell != null)
                        {
                            spell.Cast();
                        }
                        else if (!Server.Spells.SkillMasteries.MasteryInfo.IsPassiveMastery(spellID))
                        {
                            from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
                        }
                    }
                }
                else
                {
                    from.SendLocalizedMessage(500015); // You do not have that spell!
                }
            }
            catch { }
        }
        #endregion

        public static SpellbookType GetTypeForSpell(int spellID)
		{
			if (spellID >= 0 && spellID < 64)
			{
				return SpellbookType.Regular;
			}
			else if (spellID >= 100 && spellID < 117)
			{
				return SpellbookType.Necromancer;
			}
			else if (spellID >= 200 && spellID < 210)
			{
				return SpellbookType.Paladin;
			}
			else if (spellID >= 400 && spellID < 406)
			{
				return SpellbookType.Samurai;
			}
			else if (spellID >= 500 && spellID < 508)
			{
				return SpellbookType.Ninja;
			}
			else if (spellID >= 600 && spellID < 617)
			{
				return SpellbookType.Arcanist;
			}
			else if (spellID >= 677 && spellID < 693)
			{
				return SpellbookType.Mystic;
			}
            else if (spellID >= 700 && spellID < 746)
            {
                return SpellbookType.SkillMasteries;
            }
			else if (spellID >= 0 && spellID < 64)
			{
				return SpellbookType.Magery;
			}

			return SpellbookType.Invalid;
		}

		public static Spellbook FindRegular(Mobile from)
		{
			return Find(from, -1, SpellbookType.Regular);
		}

		public static Spellbook FindNecromancer(Mobile from)
		{
			return Find(from, -1, SpellbookType.Necromancer);
		}

		public static Spellbook FindPaladin(Mobile from)
		{
			return Find(from, -1, SpellbookType.Paladin);
		}

		public static Spellbook FindSamurai(Mobile from)
		{
			return Find(from, -1, SpellbookType.Samurai);
		}

		public static Spellbook FindNinja(Mobile from)
		{
			return Find(from, -1, SpellbookType.Ninja);
		}

		public static Spellbook FindArcanist(Mobile from)
		{
			return Find(from, -1, SpellbookType.Arcanist);
		}

		public static Spellbook FindMystic(Mobile from)
		{
			return Find(from, -1, SpellbookType.Mystic);
		}

		public static Spellbook FindMagery(Mobile from, int spellID)
		{
			return Find(from, spellID, GetTypeForSpell(spellID));
		}

		public static Spellbook Find(Mobile from, int spellID)
		{
			return Find(from, spellID, GetTypeForSpell(spellID));
		}

		public static Spellbook Find(Mobile from, int spellID, SpellbookType type)
		{
			if (from == null)
			{
				return null;
			}

			if (from.Deleted)
			{
				m_Table.Remove(from);
				return null;
			}

			List<Spellbook> list = null;

			m_Table.TryGetValue(from, out list);

			bool searchAgain = false;

			if (list == null)
			{
				m_Table[from] = list = FindAllSpellbooks(from);
			}
			else
			{
				searchAgain = true;
			}

			Spellbook book = FindSpellbookInList(list, from, spellID, type);

			if (book == null && searchAgain)
			{
				m_Table[from] = list = FindAllSpellbooks(from);

				book = FindSpellbookInList(list, from, spellID, type);
			}

			return book;
		}

		public static Spellbook FindSpellbookInList(List<Spellbook> list, Mobile from, int spellID, SpellbookType type)
		{
			Container pack = from.Backpack;

			for (int i = list.Count - 1; i >= 0; --i)
			{
				if (i >= list.Count)
				{
					continue;
				}

				Spellbook book = list[i];

				if (!book.Deleted && (book.Parent == from || (pack != null && book.Parent == pack)) &&
					ValidateSpellbook(book, spellID, type))
				{
					return book;
				}

				list.RemoveAt(i);
			}

			return null;
		}

		public static List<Spellbook> FindAllSpellbooks(Mobile from)
		{
			var list = new List<Spellbook>();

			Item item = from.FindItemOnLayer(Layer.OneHanded);

			if (item is Spellbook)
			{
				list.Add((Spellbook)item);
			}

			Container pack = from.Backpack;

			if (pack == null)
			{
				return list;
			}

			for (int i = 0; i < pack.Items.Count; ++i)
			{
				item = pack.Items[i];

				if (item is Spellbook)
				{
					list.Add((Spellbook)item);
				}
			}

			return list;
		}

		public static Spellbook FindEquippedSpellbook(Mobile from)
		{
			return (from.FindItemOnLayer(Layer.OneHanded) as Spellbook);
		}

		public static bool ValidateSpellbook(Spellbook book, int spellID, SpellbookType type)
		{
			return (book.SpellbookType == type && (spellID == -1 || book.HasSpell(spellID)));
		}

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethic.CheckTrade(from, to, newOwner, this))
			{
				return false;
			}

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public override bool CanEquip(Mobile from)
		{
			if (!Ethic.CheckEquip(from, this))
			{
				return false;
			}
			else if (!from.CanBeginAction(typeof(BaseWeapon)))
			{
				return false;
			}
            else if (_Owner != null && _Owner != from)
            {
                from.SendLocalizedMessage(501023); // You must be the owner to use this item.
                return false;
            }
			/*
			else if( from is PlayerMobile && !Util.EquipCheck( ((PlayerMobile)from), this ) )
			{
				from.SendLocalizedMessage(1071936); // You cannot equip that.
				return false;
			}
			*/
            else if (IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(from))
            {
                from.SendLocalizedMessage(1155496); // This item can only be used by VvV participants!
                return false;
            }
			else if( SpellbookType == SpellbookType.Regular )
			{
				from.SendLocalizedMessage(1071936); // You cannot equip that.
				return false;
			}
			/*
			if( SpellHelper.CheckCombat(from) )
			{
				from.SendMessage("전투 중에는 장비를 착용할 수 없습니다!");
				return false;
			}
			*/
			if (from.Dex < AOS.Scale2(DexRequirement, 1000 - GetLowerStatReq()))
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
			//레벨 체크
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

		public override bool AllowEquipedCast(Mobile from)
		{
			return true;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			/*
            if (dropped is SpellScroll && !(dropped is SpellStone))
            {
				SpellScroll scroll = (SpellScroll)dropped;

				SpellbookType type = GetTypeForSpell(scroll.SpellID);

				if (type != SpellbookType)
				{
					return false;
				}
				else if (HasSpell(scroll.SpellID))
				{
					from.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
					return false;
				}
				else
				{
					int val = scroll.SpellID - BookOffset;

					if (val >= 0 && val < BookCount)
					{
						from.Send(new PlaySound(0x249, GetWorldLocation()));

						m_Content |= (ulong)1 << val;
						++m_Count;

                        if (dropped.Amount > 1)
                        {
                            dropped.Amount--;
                            return base.OnDragDrop(from, dropped);
                        }
                        else
                        {
                            InvalidateProperties();
                            scroll.Delete();
                            return true;
                        }
                    }
					return false;
				}
				*/
			//}
  		    return false;
		}

		public override void OnAfterDuped(Item newItem)
		{
			Spellbook book = newItem as Spellbook;

			if (book == null)
			{
				return;
			}

			book.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
			book.m_AosArmorAttributes = new AosArmorAttributes(newItem, m_AosArmorAttributes);
            book.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
			book.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
            book.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
            book.m_NegativeAttributes = new NegativeAttributes(newItem, m_NegativeAttributes);
			book.m_AbsorptionAttributes = new SAAbsorptionAttributes(newItem, m_AbsorptionAttributes);
            book.m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(newItem, m_ExtendedWeaponAttributes);

            base.OnAfterDuped(newItem);
		}

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
					{
						from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));
					}

					if (dexBonus != 0)
					{
						from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));
					}

					if (intBonus != 0)
					{
						from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
					}
				}
				if( !Identified && Owner == null )
				{
					Owner = from;
					Identified = true;
				}
                if (HasSocket<Caddellite>())
                {
                    Caddellite.UpdateBuff(from);
                }

				from.CheckStatTimers();
			}
		}

		public override void OnRemoved(object parent)
		{
			if (Core.AOS && parent is Mobile)
			{
				Mobile from = (Mobile)parent;

				m_AosSkillBonuses.Remove();

                if (HasSocket<Caddellite>())
                {
                    Caddellite.UpdateBuff(from);
                }

				string modName = Serial.ToString();

				from.RemoveStatMod(modName + "Str");
				from.RemoveStatMod(modName + "Dex");
				from.RemoveStatMod(modName + "Int");

				from.CheckStatTimers();
			}
		}
        private string GetNameString()
        {
            string name = Name;

            if (name == null)
                name = String.Format("#{0}", LabelNumber);

            return name;
        }


		public bool HasSpell(int spellID)
		{
			spellID -= BookOffset;

			return (spellID >= 0 && spellID < BookCount && (m_Content & ((ulong)1 << spellID)) != 0);
		}

		public void DisplayTo(Mobile to)
		{
			// The client must know about the spellbook or it will crash!
			NetState ns = to.NetState;

			if (ns == null)
			{
				return;
			}

			if (Parent == null)
			{
				to.Send(WorldPacket);
			}
			else if (Parent is Item)
			{
				// What will happen if the client doesn't know about our parent?
				if (ns.ContainerGridLines)
				{
					to.Send(new ContainerContentUpdate6017(this));
				}
				else
				{
					to.Send(new ContainerContentUpdate(this));
				}
			}
			else if (Parent is Mobile)
			{
				// What will happen if the client doesn't know about our parent?
				to.Send(new EquipUpdate(this));
			}

			if (ns.HighSeas)
			{
				to.Send(new DisplaySpellbookHS(this));
			}
			else
			{
				to.Send(new DisplaySpellbook(this));
			}

			if (ObjectPropertyList.Enabled)
			{
				if (ns.NewSpellbook)
				{
					to.Send(new NewSpellbookContent(this, ItemID, BookOffset + 1, m_Content));
				}
				else
				{
					if (ns.ContainerGridLines)
					{
						to.Send(new SpellbookContent6017(m_Count, BookOffset + 1, m_Content, this));
					}
					else
					{
						to.Send(new SpellbookContent(m_Count, BookOffset + 1, m_Content, this));
					}
				}
			}
			else
			{
				if (ns.ContainerGridLines)
				{
					to.Send(new SpellbookContent6017(m_Count, BookOffset + 1, m_Content, this));
				}
				else
				{
					to.Send(new SpellbookContent(m_Count, BookOffset + 1, m_Content, this));
				}
			}
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
				list.Add(Name);
            }

            //if (!String.IsNullOrEmpty(_EngravedText))
            //{
            //   list.Add(1062613, Utility.FixHtml(_EngravedText));
            //}
        }
		
		public int GetLowerStatReq()
		{
			if (!Core.AOS)
			{
				return 0;
			}

			int v = m_AosWeaponAttributes.LowerStatReq;

			if( !this.Identified )
				v = 0;
			/*
			#region Mondain's Legacy
			if (m_Resource == CraftResource.Heartwood)
			{
				return v;
			}
			#endregion
			*/
			CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

			if (info != null)
			{
				CraftAttributeInfo attrInfo = info.AttributeInfo;

				if (attrInfo != null)
				{
					v += attrInfo.WeaponLowerRequirements;
				}
			}

			if (v > 1000)
			{
				v = 1000;
			}

			return v;
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

			//구 아이템 체크
			/*
			if( PlayerConstructed && ( PrefixOption[98] == null || PrefixOption[98] != 1 ) )
			{
				list.Add( 1063524 );
			}
			*/
			//기본 옵션
			list.Add(1063523);

			if (m_Quality == BookQuality.Exceptional)
			{
				list.Add(1063341); // exceptional
			}

			if (m_EngravedText != null)
			{
                list.Add(1072305, Utility.FixHtml(m_EngravedText)); // Engraved: ~1_INSCRIPTION~
			}

			if (m_Crafter != null)
			{
				list.Add(1050043, m_Crafter.TitleName); // crafted by ~1_NAME~
			}

            #region Factions
            FactionEquipment.AddFactionProperties(this, list);
            #endregion
			if (ArtifactRarity > 0)
			{
				list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
			}
            int prop = PrefixOption[99] + 1;
            double fprop;
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
			int strReq = AOS.Scale2(IntRequirement, 1000 - GetLowerStatReq());
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
			if (Layer == Layer.TwoHanded)
			{
				list.Add(1061171); // two-handed weapon
			}
			else
			{
				list.Add(1061824); // one-handed weapon
			}
			
			//아이템 등급 색
			//list.Add(Util.ItemRank((int)ItemPower ));
			//if( !Identified )
			//	list.Add( 1060659, "<basefont color=#FF0000>아이템 감정\t안됨<basefont color=#FFFFFF>" );
			if( Identified )
			{
				if (IsVvVItem)
				{
					list.Add(1154937); // VvV Item
				}

				if (OwnerName != null)
				{
					list.Add(1153213, OwnerName);
				}

				if (m_NegativeAttributes != null)
				{
					m_NegativeAttributes.GetProperties(list, this);
				}

				//신규 옵션 정리
				if( PrefixOption[0] > 0 )
				{
					list.Add(1063512); // [마법 옵션]
					bool skillcheck = false;
					int skilluse = 0;
					int skillname = 0;
					if( ReforgedPrefix == ReforgedPrefix.None && ReforgedSuffix == ReforgedSuffix.None )
					{
						for( int i = 0; i < PrefixOption[0]; ++i)
						{
							if( PrefixOption[i * 4 + 1] < 60 ) //스킬
							{
								SkillName skill = (SkillName)Enum.ToObject(typeof(SkillName),PrefixOption[i * 4 + 1]);
								skillname = m_AosSkillBonuses.GetSkillName(skill);
								if ( skillname > 0 )
								{
									list.Add(1080641 + skilluse, "#{0}\t{1}\t{2}\t{3}", skillname, (((double)PrefixOption[i * 4 + 4])*0.1).ToString(), (((double)PrefixOption[i * 4 + 2])*0.1).ToString(), (((double)PrefixOption[i * 4 + 3])*0.1).ToString());
									skillcheck = true;
								}
								skilluse++;
							}
							else if( PrefixOption[i * 4 + 1] >= 1080578 && PrefixOption[i * 4 + 1] <= 1080650)
							{
								if( Misc.Util.ItemOption_ToIntCheck( PrefixOption[i * 4 + 1] ) )
									list.Add( PrefixOption[i * 4 + 1], "{0}\t{1}\t{2}", PrefixOption[i * 4 + 4], PrefixOption[i * 4 + 2], PrefixOption[i * 4 + 3]);
								else
									list.Add( PrefixOption[i * 4 + 1], "{0}\t{1}\t{2}", PrefixOption[i * 4 + 4]*0.1, PrefixOption[i * 4 + 2]*0.1, PrefixOption[i * 4 + 3]*0.1);
							}
							else
							{
								if( Misc.Util.ItemOption_ToIntCheck( PrefixOption[i * 4 + 1] ) )
									list.Add( PrefixOption[i * 4 + 1], PrefixOption[i * 4 + 4].ToString());
								else
									list.Add( PrefixOption[i * 4 + 1], (((double)PrefixOption[i * 4 + 4])*0.1).ToString());
							}
						}
						if (!skillcheck && m_AosSkillBonuses != null)
						{
							m_AosSkillBonuses.GetProperties(list);
						}
					}
				}
			}
			//고유 옵션 설정
			if( SuffixOption[98] == 1 )
			{
				list.Add( 1063513 );
			
				if( SuffixOption[99] != 0 )
				{
					list.Add(1063699 + SuffixOption[99]);
				}
				if( PlayerConstructed )
				{
					switch ( Resource )
					{
						case CraftResource.RegularLeather:
						{
							list.Add(1063570, "{0}\t{1}\t{2}", "10", (5 * ( PrefixOption[99] + 1 )).ToString(), (20 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.DernedLeather:
						{
							list.Add(1063571, "{0}\t{1}\t{2}", "20", (50 * ( PrefixOption[99] + 1 )).ToString(), (20 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.RatnedLeather:
						{
							list.Add(1063572, "{0}\t{1}\t{2}", (5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.SernedLeather:
						{
							list.Add(1063573, "{0}\t{1}\t{2}", "10", (5 * ( PrefixOption[99] + 1 )).ToString(), (0.5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.SpinedLeather:
						{
							list.Add(1063574, "{0}\t{1}\t{2}", "40", (2 * ( PrefixOption[99] + 1 )).ToString(), (20 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.HornedLeather:
						{
							list.Add(1063575, "{0}", (12.5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.BarbedLeather:
						{
							list.Add(1063576, "{0}\t{1}\t{2}", "20", (5 * ( PrefixOption[99] + 1 )).ToString(), (0.5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
					}
				}
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
            private readonly Spellbook m_Equip;

            public BlassCheck(Spellbook equip)
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
            private readonly Spellbook m_Equip;

            public UnBlassCheck(Spellbook equip)
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
        public virtual void AddProperty(ObjectPropertyList list)
        {
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

			LabelTo(from, 1042886, m_Count.ToString());
		}

		public override void OnDoubleClick(Mobile from)
		{
			Container pack = from.Backpack;

			if ( SpellbookType == SpellbookType.Regular && (Parent == from || (pack != null && Parent == pack)) )
			{
				DisplayTo(from);
			}
			else
			{
				from.SendLocalizedMessage(500207);
					// The spellbook must be in your backpack (and not in a container within) to open.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(17); // version

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
						
			//접두, 접미
            writer.Write((int)m_ReforgedPrefix);
            writer.Write((int)m_ReforgedSuffix);
			
			//버전 12 - 서브 내구도
			writer.Write(m_HiddenRank);
			
			//버전 11 - 유저 제작, 크래프트 자원
            writer.WriteEncodedInt((int)m_Resource);
            writer.Write(m_PlayerConstructed);
			
			//버전 10 - 내구도 체크
            writer.WriteEncodedInt((int)m_MaxHitPoints);
            writer.WriteEncodedInt((int)m_HitPoints);			
			
			// 아이템 감정
			
			m_AbsorptionAttributes.Serialize(writer);
			
			writer.Write(m_Identified);
			
            writer.Write((int)m_ItemPower);
            m_AosResistances.Serialize(writer);
			
            m_NegativeAttributes.Serialize(writer);

            writer.Write(m_HitPoints);
            writer.Write(m_MaxHitPoints);

            writer.Write(_VvVItem);
            writer.Write(_Owner);
            writer.Write(_OwnerName);

			writer.Write((byte)m_Quality);

			writer.Write(m_EngravedText);

			writer.Write(m_Crafter);

			writer.Write((int)m_Slayer);
			writer.Write((int)m_Slayer2);

			m_AosAttributes.Serialize(writer);
			m_AosSkillBonuses.Serialize(writer);

			writer.Write(m_Content);
			writer.Write(m_Count);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 17: m_AosArmorAttributes = new AosArmorAttributes(this, reader);
						goto case 16;
				case 16: m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this, reader);
						goto case 15;
				case 15: m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						goto case 14;
				case 14: 
				{
					m_IntReq = reader.ReadInt();
					for (int i = 0; i < m_PrefixOption.Length; i++)
					{
						m_PrefixOption[i] = reader.ReadInt();
					}
					for (int i = 0; i < m_SuffixOption.Length; i++)
					{
						m_SuffixOption[i] = reader.ReadInt();
					}
					goto case 13;
				}
				case 13:
				{
                    m_ReforgedPrefix = (ReforgedPrefix)reader.ReadInt();
                    m_ReforgedSuffix = (ReforgedSuffix)reader.ReadInt();
					goto case 12;
				}
				case 12:
				{
					m_HiddenRank = reader.ReadInt();
					goto case 11;
				}
				case 11:
				{
                    m_Resource = (CraftResource)reader.ReadEncodedInt();
                    m_PlayerConstructed = reader.ReadBool();
					goto case 10;
				}
				case 10:
				{
					m_MaxHitPoints = reader.ReadEncodedInt();
					m_HitPoints = reader.ReadEncodedInt();					
					goto case 9;
				}
				case 9:
				{
					m_AbsorptionAttributes = new SAAbsorptionAttributes(this, reader);
				
					goto case 8;
				}
				case 8:
				{
                    m_Identified = reader.ReadBool();
					goto case 7;
				}
				case 7:
				{
                    m_ItemPower = (ItemPower)reader.ReadInt();
                    m_AosResistances = new AosElementAttributes(this, reader);
					goto case 6;
				}
                case 6:
                    {
                        m_NegativeAttributes = new NegativeAttributes(this, reader);

                        
                        m_MaxHitPoints = reader.ReadInt();
                        m_HitPoints = reader.ReadInt();

                        _VvVItem = reader.ReadBool();
                        _Owner = reader.ReadMobile();
                        _OwnerName = reader.ReadString();

                        goto case 5;
                    }
				case 5:
					{
						m_Quality = (BookQuality)reader.ReadByte();

						goto case 4;
					}
				case 4:
					{
						m_EngravedText = reader.ReadString();

						goto case 3;
					}
				case 3:
					{
						m_Crafter = reader.ReadMobile();
						goto case 2;
					}
				case 2:
					{
						m_Slayer = (SlayerName)reader.ReadInt();
						m_Slayer2 = (SlayerName)reader.ReadInt();
						goto case 1;
					}
				case 1:
					{
						m_AosAttributes = new AosAttributes(this, reader);
						m_AosSkillBonuses = new AosSkillBonuses(this, reader);

						goto case 0;
					}
				case 0:
					{
						m_Content = reader.ReadULong();
						m_Count = reader.ReadInt();

						break;
					}
			}
			if( m_AosArmorAttributes == null )
				m_AosArmorAttributes = new AosArmorAttributes(this);

			if( m_ExtendedWeaponAttributes == null )
				m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);

			if( m_AosWeaponAttributes == null )
				m_AosWeaponAttributes = new AosWeaponAttributes(this);

			if (m_AbsorptionAttributes == null )
			{
				m_AbsorptionAttributes = new SAAbsorptionAttributes(this);
			}
			
			if (m_AosAttributes == null)
			{
				m_AosAttributes = new AosAttributes(this);
			}

			if (m_AosSkillBonuses == null)
			{
				m_AosSkillBonuses = new AosSkillBonuses(this);
			}
			
			if (m_AosResistances == null)
			{
                m_AosResistances = new AosElementAttributes(this);
			}

            if (m_NegativeAttributes == null)
            {
                m_NegativeAttributes = new NegativeAttributes(this);
            }

			if (Core.AOS && Parent is Mobile)
			{
				m_AosSkillBonuses.AddTo((Mobile)Parent);
			}

			int strBonus = m_AosAttributes.BonusStr;
			int dexBonus = m_AosAttributes.BonusDex;
			int intBonus = m_AosAttributes.BonusInt;

			if (Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
			{
				Mobile m = (Mobile)Parent;

				string modName = Serial.ToString();

				if (strBonus != 0)
				{
					m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));
				}

				if (dexBonus != 0)
				{
					m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));
				}

				if (intBonus != 0)
				{
					m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
				}
			}

			if (Parent is Mobile)
			{
				((Mobile)Parent).CheckStatTimers();
			}
		}

		public virtual int OnCraft(
			int quality,
			bool makersMark,
			Mobile from,
			CraftSystem craftSystem,
			Type typeRes,
            ITool tool,
			CraftItem craftItem,
			int resHue)
		{
			if (makersMark)
			{
				Crafter = from;
			}
			m_Quality = (BookQuality)(quality - 1);
			
			PlayerConstructed = true;

			if (typeRes == null)
			{
				typeRes = craftItem.Resources.GetAt(0).ItemType;
			}

			if (Core.AOS)
			{
				if( this is Magerybook || this is MysticBook || this is NecromancerSpellbook || this is SpellweavingBook )
				{
					if (!craftItem.ForceNonExceptional)
					{
						Resource = CraftResources.GetFromType(typeRes);
					}

					CraftContext context = craftSystem.GetContext(from);
					
					if( from is PlayerMobile )
					{
						int arms = (int)from.Skills.ArmsLore.Value * 100;
						if (m_Quality == BookQuality.Exceptional)
							arms += 5000;
						
						int rank = Util.ItemRankMaker( from.Skills[craftSystem.MainSkill].Value * 4 );
						int tier = Util.ItemTierMaker( arms, rank, Misc.Util.ResourceNumberToNumber((int)Resource ), from );
						
						PlayerMobile pm = from as PlayerMobile;
						Util.ItemCreate( this, rank, true, pm, tier );
					}
				}
				if (craftItem != null && !craftItem.ForceNonExceptional)
				{
					CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

					if (resInfo == null)
					{
						return quality;
					}
				}
			}
			return quality;
		}
		private CraftResource m_Resource;
        private bool m_PlayerConstructed;
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
		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get { return m_Resource; }
			set
			{
				m_Resource = value;
				Hue = CraftResources.GetHue(m_Resource);
				InvalidateProperties();
			}
		}

		[Usage("AllSpells")]
		[Description("Completely fills a targeted spellbook with scrolls.")]
		private static void AllSpells_OnCommand(CommandEventArgs e)
		{
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, AllSpells_OnTarget);
			e.Mobile.SendMessage("Target the spellbook to fill.");
		}

		private static void AllSpells_OnTarget(Mobile from, object obj)
		{
			if (obj is Spellbook)
			{
				Spellbook book = (Spellbook)obj;

				if (book.BookCount == 64)
				{
					book.Content = ulong.MaxValue;
				}
				else
				{
					book.Content = (1ul << book.BookCount) - 1;
				}

				from.SendMessage("The spellbook has been filled.");

				CommandLogging.WriteLine(
					from, "{0} {1} filling spellbook {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(book));
			}
			else
			{
				from.BeginTarget(-1, false, TargetFlags.None, AllSpells_OnTarget);
				from.SendMessage("That is not a spellbook. Try again.");
			}
		}

		private static void EventSink_OpenSpellbookRequest(OpenSpellbookRequestEventArgs e)
		{
			Mobile from = e.Mobile;

			if (!DesignContext.Check(from))
			{
				return; // They are customizing
			}

			SpellbookType type;

			switch (e.Type)
			{
				default:
				case 1:
					type = SpellbookType.Regular;
					break;
				case 2:
					type = SpellbookType.Necromancer;
					break;
				case 3:
					type = SpellbookType.Paladin;
					break;
				case 4:
					type = SpellbookType.Ninja;
					break;
				case 5:
					type = SpellbookType.Samurai;
					break;
				case 6:
					type = SpellbookType.Arcanist;
					break;
				case 7:
					type = SpellbookType.Mystic;
					break;
				case 8:
					type = SpellbookType.Magery;
					break;
			}

			Spellbook book = Find(from, -1, type);

			if (book != null)
			{
				book.DisplayTo(from);
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
		private static void EventSink_CastSpellRequest(CastSpellRequestEventArgs e)
		{
			Mobile from = e.Mobile;

			if (!DesignContext.Check(from))
			{
				return; // They are customizing
			}

			Spellbook book = e.Spellbook as Spellbook;
			int spellID = e.SpellID;

			if (book == null || !book.HasSpell(spellID))
			{
				book = Find(from, spellID);
			}

			if (book != null && book.HasSpell(spellID))
			{
				SpecialMove move = SpellRegistry.GetSpecialMove(spellID);

				if (move != null)
				{
					SpecialMove.SetCurrentMove(from, move);
				}
				else
				{
					Spell spell = SpellRegistry.NewSpell(spellID, from, null);

					if (spell != null)
					{
						spell.Cast();
					}
                    else if ( !Server.Spells.SkillMasteries.MasteryInfo.IsPassiveMastery( spellID ) )
                    {
						from.SendLocalizedMessage( 502345 ); // This spell has been temporarily disabled.
                    }
				}
			}
			else
			{
				from.SendLocalizedMessage(500015); // You do not have that spell!
			}
		}
	}
}
