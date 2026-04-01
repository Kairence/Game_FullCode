#region References
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;

using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.XmlSpawner2;
using Server.Ethics;
using Server.Factions;
using Server.Mobiles;
using Server.Network;
using Server.Services.Virtues;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Sixth;
using Server.Spells.Spellweaving;
using Server.Spells.SkillMasteries;
using Server.Misc;
using Server.Regions;


#endregion

namespace Server.Items
{
	public interface ISlayer
	{
		SlayerName Slayer { get; set; }
		SlayerName Slayer2 { get; set; }
	}

    public abstract class BaseWeapon : Item, IWeapon, IFactionItem, IUsesRemaining, ICraftable, ISlayer, IDurability, ISetItem, IVvVItem, IOwnerRestricted, IResource, IArtifact, ICombatEquipment, IEngravable, IQuality, IEquipOption
    {
		#region Damage Helpers
		public static BaseWeapon GetDamageOutput(Mobile wielder, out int min, out int max)
		{
			return GetDamageOutput(wielder, null, out min, out max);
		}

		public static BaseWeapon GetDamageOutput(Mobile wielder, BaseWeapon weapon, out int min, out int max)
		{
			int minRaw, maxRaw;

			return GetDamageOutput(wielder, weapon, out minRaw, out maxRaw, out min, out max);
		}

		public static BaseWeapon GetDamageOutput(
			Mobile wielder,
			out int minRaw,
			out int maxRaw,
			out int minVal,
			out int maxVal)
		{
			return GetDamageOutput(wielder, null, out minRaw, out maxRaw, out minVal, out maxVal);
		}

		public static BaseWeapon GetDamageOutput(
			Mobile wielder,
			BaseWeapon weapon,
			out int minRaw,
			out int maxRaw,
			out int minVal,
			out int maxVal)
		{
			minRaw = maxRaw = 0;
			minVal = maxVal = 0;

			if (wielder == null)
			{
				return null;
			}

			if (weapon == null)
			{
				weapon = wielder.Weapon as BaseWeapon ?? Fists;
			}

			if (weapon == null)
			{
				return null;
			}

			weapon.GetBaseDamageRange(wielder, out minVal, out maxVal);

			if (wielder is BaseCreature)
			{
				if (((BaseCreature)wielder).DamageMin >= 0 || (weapon is Fists && !wielder.Body.IsHuman))
				{
					minRaw = minVal;
					maxRaw = maxVal;
					return weapon;
				}
			}

			minRaw = weapon.MinDamage;
			maxRaw = weapon.MaxDamage;

			//마법책 체크(데미지 1 ~ 3, 공격 속도 3.0)
			
			if( weapon is Fists )
			{
				Console.WriteLine("bookcheck");
				Spellbook book = wielder.FindItemOnLayer(Layer.OneHanded) as Spellbook;				
				if( book != null )
				{
					Console.WriteLine("bookequip");
					minRaw = 1;
					maxRaw = 3;
				}
			}

			if (Core.AOS)
			{
				minVal = (int)weapon.ScaleDamageAOS(wielder, minVal, false);
				maxVal = (int)weapon.ScaleDamageAOS(wielder, maxVal, false);
			}
			else
			{
				minVal = (int)weapon.ScaleDamageOld(wielder, minVal, false);
				maxVal = (int)weapon.ScaleDamageOld(wielder, maxVal, false);
			}

			return weapon;
		}
		#endregion

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
			get { return m_FactionState; }
			set
			{
				m_FactionState = value;

				if (m_FactionState == null)
				{
					Hue = CraftResources.GetHue(Resource);
				}

				LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
			}
		}
		#endregion

        #region IUsesRemaining members
        private int m_UsesRemaining;
        private bool m_ShowUsesRemaining;
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining { get { return m_UsesRemaining; } set { m_UsesRemaining = value; InvalidateProperties(); } }

        public bool ShowUsesRemaining { get { return m_ShowUsesRemaining; } set { m_ShowUsesRemaining = value; InvalidateProperties(); } }
        
        public void ScaleUses()
        {
            m_UsesRemaining = (m_UsesRemaining * GetUsesScalar()) / 100;
            InvalidateProperties();
        }

        public void UnscaleUses()
        {
            m_UsesRemaining = (m_UsesRemaining * 100) / GetUsesScalar();
        }

        public int GetUsesScalar()
        {
            if (m_Quality == ItemQuality.Exceptional)
                return 200;

            return 100;
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

		/* Weapon internals work differently now (Mar 13 2003)
        *
        * The attributes defined below default to -1.
        * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
        * If not, the attribute value itself is used. Here's the list:
        *  - MinDamage
        *  - MaxDamage
        *  - Speed
        *  - HitSound
        *  - MissSound
        *  - StrRequirement, DexRequirement, IntRequirement
        *  - WeaponType
        *  - WeaponAnimation
        *  - MaxRange
        */

		#region Var declarations
		// Instance values. These values are unique to each weapon.
		private WeaponDamageLevel m_DamageLevel;
		private WeaponAccuracyLevel m_AccuracyLevel;
		private WeaponDurabilityLevel m_DurabilityLevel;
		private ItemQuality m_Quality;
		private Mobile m_Crafter;
		private Poison m_Poison;
		private int m_PoisonCharges;
		private bool m_Identified;
		private int m_Hits;
		private int m_MaxHits;
		private SlayerName m_Slayer;
		private SlayerName m_Slayer2;

		#region Mondain's Legacy
		private TalismanSlayerName m_Slayer3;
		#endregion

		private SkillMod m_SkillMod, m_MageMod, m_MysticMod;
		private CraftResource m_Resource;
		private bool m_PlayerConstructed;

        private bool m_Altered;

        private AosAttributes m_AosAttributes;
        private AosArmorAttributes m_AosArmorAttributes;
		private AosWeaponAttributes m_AosWeaponAttributes;
		private AosSkillBonuses m_AosSkillBonuses;
		private AosElementAttributes m_AosElementDamages;
		private SAAbsorptionAttributes m_SAAbsorptionAttributes;
        private NegativeAttributes m_NegativeAttributes;
        private ExtendedWeaponAttributes m_ExtendedWeaponAttributes;

		// Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
		private int m_StrReq, m_DexReq, m_IntReq;
		private int m_MinDamage, m_MaxDamage;
		private int m_HitSound, m_MissSound;
		private float m_Speed;
		private int m_MaxRange;
		private SkillName m_Skill;
		private WeaponType m_Type;
		private WeaponAnimation m_Animation;

        #region Stygian Abyss
        private int m_TimesImbued;
        private bool m_IsImbued;
        private bool m_DImodded;
        #endregion

        #region Runic Reforging
        private ItemPower m_ItemPower;
        private ReforgedPrefix m_ReforgedPrefix;
        private ReforgedSuffix m_ReforgedSuffix;
        #endregion
        #endregion

		private double m_CanPoison;
		private double m_CanExplosion;
		private bool m_NotUseUniqueOption;
		
        #region Virtual Properties
        public virtual WeaponAbility PrimaryAbility { get { return null; } }
		public virtual WeaponAbility SecondaryAbility { get { return null; } }

		public virtual int DefMaxRange { get { return 1; } }
		public virtual int DefHitSound { get { return 0; } }
		public virtual int DefMissSound { get { return 0; } }
		public virtual SkillName DefSkill { get { return SkillName.Swords; } }
		public virtual WeaponType DefType { get { return WeaponType.Slashing; } }
		public virtual WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

		public virtual int AosStrengthReq { get { return 0; } }
		public virtual int AosDexterityReq { get { return 0; } }
		public virtual int AosIntelligenceReq { get { return 0; } }
		public virtual int AosMinDamage { get { return 0; } }
		public virtual int AosMaxDamage { get { return 0; } }
		public virtual int AosSpeed { get { return 0; } }
		public virtual float MlSpeed { get { return 0.0f; } }
		public virtual int AosMaxRange { get { return DefMaxRange; } }
		public virtual int AosHitSound { get { return DefHitSound; } }
		public virtual int AosMissSound { get { return DefMissSound; } }
		public virtual SkillName AosSkill { get { return DefSkill; } }
		public virtual WeaponType AosType { get { return DefType; } }
		public virtual WeaponAnimation AosAnimation { get { return DefAnimation; } }

		public virtual int OldStrengthReq { get { return 0; } }
		public virtual int OldDexterityReq { get { return 0; } }
		public virtual int OldIntelligenceReq { get { return 0; } }
		public virtual int OldMinDamage { get { return 0; } }
		public virtual int OldMaxDamage { get { return 0; } }
		public virtual int OldSpeed { get { return 0; } }
		public virtual int OldMaxRange { get { return DefMaxRange; } }
		public virtual int OldHitSound { get { return DefHitSound; } }
		public virtual int OldMissSound { get { return DefMissSound; } }
		public virtual SkillName OldSkill { get { return DefSkill; } }
		public virtual WeaponType OldType { get { return DefType; } }
		public virtual WeaponAnimation OldAnimation { get { return DefAnimation; } }

		public virtual int InitMinHits { get { return 0; } }
		public virtual int InitMaxHits { get { return 0; } }

        public virtual bool CanFortify { get { return !IsImbued && NegativeAttributes.Antique < 4; } }
        public virtual bool CanRepair { get { return m_NegativeAttributes.NoRepair == 0; } }
		public virtual bool CanAlter { get { return true; } }

		public override int PhysicalResistance { get { return m_AosWeaponAttributes.ResistPhysicalBonus / 10000 + m_AosArmorAttributes.AllResist / 10000; } }
		public override int FireResistance { get { return m_AosWeaponAttributes.ResistFireBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000; } }
		public override int ColdResistance { get { return m_AosWeaponAttributes.ResistColdBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000; } }
		public override int PoisonResistance { get { return m_AosWeaponAttributes.ResistPoisonBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000; } }
		public override int EnergyResistance { get { return m_AosWeaponAttributes.ResistEnergyBonus / 10000 + m_AosArmorAttributes.ElementalResist / 10000 + m_AosArmorAttributes.AllResist / 10000; } }

		public virtual SkillName AccuracySkill { get { return SkillName.Tactics; } }

        public override double DefaultWeight
        {
            get
            {
                if (NegativeAttributes == null || NegativeAttributes.Unwieldly == 0)
                    return base.DefaultWeight;

                return 50;
            }
        }

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

		private class UnBlessEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly BaseWeapon m_Weapon; // BaseArmor, BaseWeapon or BaseClothing

			public UnBlessEntry(Mobile from, BaseWeapon weapon)
				: base(6208, -1)
			{
				m_From = from;
				m_Weapon = weapon;
			}

			public override void OnClick()
			{
				m_Weapon.BlessedFor = null;
				m_Weapon.BlessedBy = null;

				Container pack = m_From.Backpack;

				if (pack != null)
				{
					pack.DropItem(new PersonalBlessDeed(m_From));
					m_From.SendLocalizedMessage(1062200); // A personal bless deed has been placed in your backpack.
				}
			}
		}
		#endregion

		#endregion

		#region Getters & Setters
		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes Attributes { get { return m_AosAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosArmorAttributes ArmorAttributes { get { return m_AosArmorAttributes; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public AosWeaponAttributes WeaponAttributes { get { return m_AosWeaponAttributes; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses { get { return m_AosSkillBonuses; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public AosElementAttributes AosElementDamages { get { return m_AosElementDamages; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public SAAbsorptionAttributes AbsorptionAttributes { get { return m_SAAbsorptionAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public NegativeAttributes NegativeAttributes { get { return m_NegativeAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ExtendedWeaponAttributes ExtendedWeaponAttributes { get { return m_ExtendedWeaponAttributes; } set { } }

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

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get { return m_Hits; }
			set
			{
				if (m_Hits == value)
				{
					return;
				}

				if (value > m_MaxHits)
				{
					value = m_MaxHits;
				}

				m_Hits = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get { return m_MaxHits; }
			set
			{
				m_MaxHits = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PoisonCharges
		{
			get { return m_PoisonCharges; }
			set
			{
				m_PoisonCharges = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Poison Poison
		{
			get { return m_Poison; }
			set
			{
				m_Poison = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ItemQuality Quality
		{
			get { return m_Quality; }
			set
			{
				UnscaleDurability();
                UnscaleUses();
				m_Quality = value;
				ScaleDurability();
                ScaleUses();
				InvalidateProperties();
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

		[CommandProperty(AccessLevel.GameMaster)]
		public TalismanSlayerName Slayer3
		{
			get { return m_Slayer3; }
			set
			{
				m_Slayer3 = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get { return m_Resource; }
			set
			{
				UnscaleDurability();
				m_Resource = value;
				Hue = CraftResources.GetHue(m_Resource);
				InvalidateProperties();
				ScaleDurability();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponDamageLevel DamageLevel
		{
			get { return m_DamageLevel; }
			set
			{
				m_DamageLevel = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponDurabilityLevel DurabilityLevel
		{
			get { return m_DurabilityLevel; }
			set
			{
				UnscaleDurability();
				m_DurabilityLevel = value;
				InvalidateProperties();
				ScaleDurability();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed { get { return m_PlayerConstructed; } set { m_PlayerConstructed = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxRange
		{
			get { return (m_MaxRange == -1 ? Core.AOS ? AosMaxRange : OldMaxRange : m_MaxRange); }
			set
			{
				m_MaxRange = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponAnimation Animation { get { return (m_Animation == (WeaponAnimation)(-1) ? Core.AOS ? AosAnimation : OldAnimation : m_Animation); } set { m_Animation = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponType Type { get { return (m_Type == (WeaponType)(-1) ? Core.AOS ? AosType : OldType : m_Type); } set { m_Type = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName Skill
		{
			get { return (m_Skill == (SkillName)(-1) ? Core.AOS ? AosSkill : OldSkill : m_Skill); }
			set
			{
				m_Skill = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitSound { get { return (m_HitSound == -1 ? Core.AOS ? AosHitSound : OldHitSound : m_HitSound); } set { m_HitSound = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MissSound { get { return (m_MissSound == -1 ? Core.AOS ? AosMissSound : OldMissSound : m_MissSound); } set { m_MissSound = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MinDamage
		{
			get { return (m_MinDamage == -1 ? Core.AOS ? AosMinDamage : OldMinDamage : m_MinDamage); }
			set
			{
				m_MinDamage = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxDamage
		{
			get { return (m_MaxDamage == -1 ? Core.AOS ? AosMaxDamage : OldMaxDamage : m_MaxDamage); }
			set
			{
				m_MaxDamage = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public float Speed
		{
			get
			{
				if (m_Speed != -1)
				{
					return m_Speed;
				}

				if (Core.ML)
				{
					return MlSpeed;
				}
				else if (Core.AOS)
				{
					return AosSpeed;
				}

				return OldSpeed;
			}
			set
			{
				m_Speed = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get{ return m_StrReq == -1 ? AosStrengthReq : 1000; }
			set{ m_StrReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get{ return m_DexReq == -1 ? AosDexterityReq : 1000; }
			set{ m_DexReq = value; InvalidateProperties(); }
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get{ return m_IntReq == -1 ? AosIntelligenceReq : 1000; }
			set{ m_IntReq = value; InvalidateProperties(); }
		}

		/*
		
		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement { get { return (m_DexReq == -1 ? Core.AOS ? AosDexterityReq : OldDexterityReq : m_DexReq); } set { m_DexReq = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement { get { return (m_IntReq == -1 ? Core.AOS ? AosIntelligenceReq : OldIntelligenceReq : m_IntReq); } set { m_IntReq = value; } }
		*/
		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponAccuracyLevel AccuracyLevel
		{
			get { return m_AccuracyLevel; }
			set
			{
				if (m_AccuracyLevel != value)
				{
					m_AccuracyLevel = value;

					if (UseSkillMod)
					{
						if (m_AccuracyLevel == WeaponAccuracyLevel.Regular)
						{
							if (m_SkillMod != null)
							{
								m_SkillMod.Remove();
							}

							m_SkillMod = null;
						}
						else if (m_SkillMod == null && Parent is Mobile)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							((Mobile)Parent).AddSkillMod(m_SkillMod);
						}
						else if (m_SkillMod != null)
						{
							m_SkillMod.Value = (int)m_AccuracyLevel * 5;
						}
					}

					InvalidateProperties();
				}
			}
		}

        public Mobile FocusWeilder { get; set; }
        public Mobile EnchantedWeilder { get; set; }

        public int LastParryChance { get; set; }

        #region Stygian Abyss
        [CommandProperty(AccessLevel.GameMaster)]
        public int TimesImbued
        {
            get { return m_TimesImbued; }
            set { m_TimesImbued = value; }
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
        public bool DImodded
        {
            get { return m_DImodded; }
            set { m_DImodded = value; }
        }

        public int[] BaseResists
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

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SearingWeapon
        {
            get { return HasSocket<SearingWeapon>(); }
            set
            {
                if (!value)
                {
                    RemoveSocket<SearingWeapon>();
                }
                else if (!SearingWeapon)
                {
                    AttachSocket(new SearingWeapon(this));
                }
            }
        }

        #region Runic Reforging

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemPower ItemPower
        {
            get { return m_ItemPower; }
            set { m_ItemPower = value; InvalidateProperties(); }
        }

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
		
        [CommandProperty(AccessLevel.GameMaster)]
        public double CanPoison
        {
            get { return m_CanPoison; }
            set { m_CanPoison = value; InvalidateProperties(); }
        }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public double CanExplosion
        {
            get { return m_CanExplosion; }
            set { m_CanExplosion = value; InvalidateProperties(); }
        }
		
        [CommandProperty(AccessLevel.GameMaster)]
        public bool NotUseUniqueOption
        {
            get { return m_NotUseUniqueOption; }
            set { m_NotUseUniqueOption = value; InvalidateProperties(); }
        }
		
		
        #endregion
        #endregion

		
		
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
			
			/*
            if (SearingWeapon && Parent == from)
            {
                list.Add(new SearingWeapon.ToggleExtinguishEntry(from, this));
            }

			if (BlessedFor == from && BlessedBy == from && RootParent == from)
			{
				list.Add(new UnBlessEntry(from, this));
			}
			*/
		}

        #region ContextMenuEntries
        private class BlassCheck : ContextMenuEntry
        {
            private readonly BaseWeapon m_Equip;

            public BlassCheck(BaseWeapon equip)
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
            private readonly BaseWeapon m_Equip;

            public UnBlassCheck(BaseWeapon equip)
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
		public override void OnAfterDuped(Item newItem)
		{
            base.OnAfterDuped(newItem);

			BaseWeapon weap = newItem as BaseWeapon;

			if (weap == null)
			{
				return;
			}

			if( !this.Identified )
				return;
			
			weap.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
			weap.m_AosArmorAttributes = new AosArmorAttributes(newItem, m_AosArmorAttributes);
 			weap.m_AosElementDamages = new AosElementAttributes(newItem, m_AosElementDamages);
			weap.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
			weap.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
            weap.m_NegativeAttributes = new NegativeAttributes(newItem, m_NegativeAttributes);
            weap.m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(newItem, m_ExtendedWeaponAttributes);

			#region Mondain's Legacy
			weap.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
			weap.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
			#endregion

			#region SA
			weap.m_SAAbsorptionAttributes = new SAAbsorptionAttributes(newItem, m_SAAbsorptionAttributes);
			#endregion
		}

		public virtual void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
            m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;

            InvalidateProperties();
		}

		public virtual void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * scale) + 99) / 100;
            m_MaxHits = ((m_MaxHits * scale) + 99) / 100;

            if (m_MaxHits > 255)
                m_MaxHits = 255;

            if (m_Hits > 255)
                m_Hits = 255;

            InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if (m_Quality == ItemQuality.Exceptional)
			{
				bonus += 20;
			}

			switch (m_DurabilityLevel)
			{
				case WeaponDurabilityLevel.Durable:
					bonus += 20;
					break;
				case WeaponDurabilityLevel.Substantial:
					bonus += 50;
					break;
				case WeaponDurabilityLevel.Massive:
					bonus += 70;
					break;
				case WeaponDurabilityLevel.Fortified:
					bonus += 100;
					break;
				case WeaponDurabilityLevel.Indestructible:
					bonus += 120;
					break;
			}

			if (Core.AOS)
			{
				if( this.Identified )
					bonus += m_AosWeaponAttributes.DurabilityBonus;

				#region Mondain's Legacy
				if (m_Resource == CraftResource.Heartwood)
				{
					return bonus;
				}
				#endregion

				CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
				CraftAttributeInfo attrInfo = null;

				if (resInfo != null)
				{
					attrInfo = resInfo.AttributeInfo;
				}

				if (attrInfo != null)
				{
					bonus += attrInfo.WeaponDurability;
				}
			}

			return bonus;
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

		public static void BlockEquip(Mobile m, TimeSpan duration)
		{
			if (m.BeginAction(typeof(BaseWeapon)))
			{
				new ResetEquipTimer(m, duration).Start();
			}
		}

		private class ResetEquipTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public ResetEquipTimer(Mobile m, TimeSpan duration)
				: base(duration)
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction(typeof(BaseWeapon));
                m_Mobile.SendLocalizedMessage(1060168); // Your confusion has passed, you may now arm a weapon!
            }
		}

		public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
		{
			if (base.CheckConflictingLayer(m, item, layer))
			{
				return true;
			}

			if (Layer == Layer.TwoHanded && layer == Layer.OneHanded)
			{
                m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500214); // You already have something in both hands.
                return true;
			}
			else if (Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight))
			{
                m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500215); // // You can only wield one weapon at a time.
				return true;
			}

			return false;
		}

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethic.CheckTrade(from, to, newOwner, this))
			{
				return false;
			}

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public virtual Race RequiredRace { get { return null; } }
		//On OSI, there are no weapons with race requirements, this is for custom stuff

		#region SA
		public virtual bool CanBeWornByGargoyles { get { return false; } }
		#endregion

		public override bool CanEquip(Mobile from)
		{
			if (!Ethic.CheckEquip(from, this))
			{
				return false;
			}

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
            }

            bool morph = from.FindItemOnLayer(Layer.Earrings) is MorphEarrings;

			if (from.Race == Race.Gargoyle && !CanBeWornByGargoyles && from.IsPlayer())
			{
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1111708); // Gargoyles can't wear this.
                return false;
			}

			/*
			if (RequiredRace != null && from.Race != RequiredRace && !morph)
			{
				if (RequiredRace == Race.Elf)
				{
					from.SendLocalizedMessage(1072203); // Only Elves may use this.
                }
				else if (RequiredRace == Race.Gargoyle)
				{
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1111707); // Only gargoyles can wear this.
                }
				else
				{
					from.SendMessage("Only {0} may use ", RequiredRace.PluralName);
				}

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
			else if (!from.CanBeginAction(typeof(BaseWeapon)))
			{
                from.SendLocalizedMessage(3000201); // You must wait to perform another action.
                return false;
			}
				#region Personal Bless Deed
			else if (BlessedBy != null && BlessedBy != from)
			{
				from.SendLocalizedMessage(1075277); // That item is blessed by another player.

				return false;
			}
			else if (!XmlAttach.CheckCanEquip(this, from))
			{
				return false;
			}
				#endregion

			else
			{
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
		}

		public virtual bool UseSkillMod { get { return !Core.AOS; } }

		public override bool OnEquip(Mobile from)
		{
			int strBonus = m_AosAttributes.BonusStr;
			int dexBonus = m_AosAttributes.BonusDex;
			int intBonus = m_AosAttributes.BonusInt;

			WeaponAbility a = WeaponAbility.GetCurrentAbility(from);
			if( a != null )
				WeaponAbility.ClearCurrentAbility(from);
			
			if( !Identified )
				Identified = true;
			if( Owner == null && ( PrefixOption[0] == 200 || PrefixOption[0] == 300 ) )
				Owner = from;
			
			
			if ( this.Identified && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
			{
				Mobile m = from;

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

			from.NextCombatTime = Core.TickCount + (int)GetDelay(from).TotalMilliseconds;

			if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
			{
				if (m_SkillMod != null)
				{
					m_SkillMod.Remove();
				}

				m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
				from.AddSkillMod(m_SkillMod);
			}

			XmlAttach.CheckOnEquip(this, from);

            InDoubleStrike = false;

			return true;
		}

		public override void OnAdded(object parent)
		{
			base.OnAdded(parent);

			if (parent is Mobile)
			{
				Mobile from = (Mobile)parent;

				if (Core.AOS)
				{
					m_AosSkillBonuses.AddTo(from);
				}

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
				/*
				//세트 아이템 체크 코드
				if( PrefixOption[50] > 0 )
				{
					if( from is PlayerMobile )
					{
						PlayerMobile pm = from as PlayerMobile;
						pm.ItemSetValue[PrefixOption[50]]++;
						Misc.SetItem.SetOption(pm, false);
					}					
				}
				*/
                if (HasSocket<Caddellite>())
                {
                    Caddellite.UpdateBuff(from);
                }

                if (ExtendedWeaponAttributes.Focus > 0)
                {
                    Focus.UpdateBuff(from);
                }

                from.CheckStatTimers();
				from.Delta(MobileDelta.WeaponDamage);
			}
		}

		public override void OnRemoved(object parent)
		{
			if (parent is Mobile)
			{
				Mobile m = (Mobile)parent;
				BaseWeapon weapon = m.Weapon as BaseWeapon;

				string modName = Serial.ToString();

				m.RemoveStatMod(modName + "Str");
				m.RemoveStatMod(modName + "Dex");
				m.RemoveStatMod(modName + "Int");

				if (weapon != null)
				{
					m.NextCombatTime = Core.TickCount + (int)weapon.GetDelay(m).TotalMilliseconds;
				}

				if (UseSkillMod && m_SkillMod != null)
				{
					m_SkillMod.Remove();
					m_SkillMod = null;
				}

				if (m_MageMod != null)
				{
					m_MageMod.Remove();
					m_MageMod = null;
				}

				if (Core.AOS)
				{
					m_AosSkillBonuses.Remove();
				}

				ImmolatingWeaponSpell.StopImmolating(this, (Mobile)parent);
                Spells.Mysticism.EnchantSpell.OnWeaponRemoved(this, m);

                if (FocusWeilder != null)
                    FocusWeilder = null;

                SkillMasterySpell.OnWeaponRemoved(m, this);

				#region Mondain's Legacy Sets
				if (IsSetItem && m_SetEquipped)
				{
					SetHelper.RemoveSetBonus(m, SetID, this);
				}
				#endregion
				/*
				//세트 아이템 해제 코드
				if( PrefixOption[50] > 0 )
				{
					if( m is PlayerMobile )
					{
						PlayerMobile pm = m as PlayerMobile;
						pm.ItemSetValue[PrefixOption[50]]--;
						Misc.SetItem.SetOption(pm, false);
					}					
				}
				*/
                if (HasSocket<Caddellite>())
                {
                    Caddellite.UpdateBuff(m);
                }

                if (SearingWeapon)
                {
                    Server.Items.SearingWeapon.OnWeaponRemoved(this);
                }

                if (ExtendedWeaponAttributes.Focus > 0)
                {
                    Focus.UpdateBuff(m);
                }
				WeaponAbility a = WeaponAbility.GetCurrentAbility(m);
				if( a != null )
					WeaponAbility.ClearCurrentAbility(m);

                m.CheckStatTimers();

                m.Delta(MobileDelta.WeaponDamage);

                XmlAttach.CheckOnRemoved(this, parent);
			}

            LastParryChance = 0;
        }

        public void AddMysticMod(Mobile from)
        {
            if (m_MysticMod != null)
                m_MysticMod.Remove();

            int value = m_ExtendedWeaponAttributes.MysticWeapon;

            if (Enhancement.GetValue(from, ExtendedWeaponAttribute.MysticWeapon) > value)
                value = Enhancement.GetValue(from, ExtendedWeaponAttribute.MysticWeapon);

            m_MysticMod = new DefaultSkillMod(SkillName.Mysticism, true, -30 + value);
            from.AddSkillMod(m_MysticMod);
        }

        public void RemoveMysticMod()
        {
            if (m_MysticMod != null)
            {
                m_MysticMod.Remove();
                m_MysticMod = null;
            }
        }

		public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
		{
			SkillName sk;

			if (checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0)
			{
				double swrd = m.Skills[SkillName.Swords].Value;
				double fenc = m.Skills[SkillName.Fencing].Value;
				double mcng = m.Skills[SkillName.Macing].Value;
				double val;

				sk = SkillName.Swords;
				val = swrd;

				if (fenc > val)
				{
					sk = SkillName.Fencing;
					val = fenc;
				}
				if (mcng > val)
				{
					sk = SkillName.Macing;
					val = mcng;
				}
			}
			/*
			else if (m_AosWeaponAttributes.MageWeapon != 0)
			{
				if (m.Skills[SkillName.Magery].Value > m.Skills[Skill].Value)
				{
					sk = SkillName.Magery;
				}
				else
				{
					sk = Skill;
				}
			}
            else if (m_ExtendedWeaponAttributes.MysticWeapon != 0 || Enhancement.GetValue(m, ExtendedWeaponAttribute.MysticWeapon) > 0)
            {
                if (m.Skills[SkillName.Mysticism].Value > m.Skills[Skill].Value)
                {
                    sk = SkillName.Mysticism;
                }
                else
                {
                    sk = Skill;
                }
            }
			*/
            else
            {
                sk = Skill;

                if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman &&
                    m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
                {
                    sk = SkillName.Wrestling;
                }
            }

			return sk;
		}

		public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender)
		{
			return attacker.Skills[GetUsedSkill(attacker, true)].Value;
		}

		public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender)
		{
			return defender.Skills[GetUsedSkill(defender, true)].Value;
		}

		public static bool CheckAnimal(Mobile m, Type type)
		{
			return AnimalForm.UnderTransformation(m, type);
		}

		//명중과 회피
		public virtual bool CheckHit(Mobile attacker, IDamageable damageable)
		{
            Mobile defender = damageable as Mobile;

            if (defender == null)
            {
                if (damageable is IDamageableItem)
                    return ((IDamageableItem)damageable).CheckHit(attacker);

                return true;
            }

			//명중 확률
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];

			//int ac = AosAttributes.GetValue(attacker, AosAttribute.AttackChance);
            //int dc = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

			double stunPercent = attacker.Str * 0.1 - defender.Dex * 0.1; // + ac - dc;

			/*
			if( atkSkill.Value >= 100 )
				stunPercent += 30;
			
			stunPercent -= defSkill.Value;
			if( defSkill.Value >= 100 )
				stunPercent -= 15;
			//카운터 펜싱 보너스
			if( CounterAttack.IsCountering(defender) )
			{
				double anatomy = defender.Skills.Anatomy.Value * 0.3;
				if( anatomy >= 100 )
					anatomy += 4;
					stunPercent -= anatomy + 10;
			}

			//라이트닝 스트라이크 석궁 보너스
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				if( pm.FuryActive && atkWeapon.Skill is SkillName.Archery )
				{
					double anatomy = attacker.Skills.Anatomy.Value * 0.3;
					if( anatomy >= 100 )
						anatomy += 4;
					stunPercent += anatomy + 10;
				}
			}
			*/
			
			/*
			Spellbook book = defender.FindItemOnLayer(Layer.OneHanded) as Spellbook;
			if( book != null )
			{
				if( book is NecromancerSpellbook )
				{
					stunPercent -= defender.Skills.Necromancy.Value * 0.5;
					if( defender.Skills.Necromancy.Value >= 100 )
						stunPercent -= 15;
				}
			}
			*/
			/*
			//펜싱 명중 보너스
			if( atkWeapon.Skill is SkillName.Fencing )
			{
				stunPercent += attacker.Skills.Fencing.Value * 0.2;
				if( attacker.Skills.Fencing.Value >= 100 )
					stunPercent += 6;
			}
			*/
			if( defender is PlayerMobile )
			{
				BaseCreature bc = attacker as BaseCreature;
				if( attacker is BaseCreature && bc.ControlMaster == null && bc.SummonMaster == null )
				{
					attacker.CheckSkill(atkSkill.SkillName, defSkill.Value);
					attacker.CheckSkill(SkillName.Anatomy, defender.Skills.Anatomy.Value);
				}
			}
			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				if( bc.ControlMaster == null && bc.SummonMaster == null )
				{
					double point = defSkill.Value + bc.BardingDifficulty;
					if( point > 0 )
					{
						attacker.CheckSkill(atkSkill.SkillName, point);
						attacker.CheckSkill(SkillName.Anatomy, point);
					}
				}
			}
			
			if( stunPercent > Utility.Random(100) )
				return true;
			else
				return false;
		}

		public virtual TimeSpan GetDelay(Mobile m)
		{
			double speed = Speed; // 무기 기본 속도 초 단위

			if (speed == 0)
				return TimeSpan.FromHours(1.0);

			// 1. 보너스 추출 (1% = 10,000 스케일)
			int bonus = Server.Misc.ItemOptionCreator.GetAttributeValue(m, Server.Misc.CustomOption.SwingSpeed);

			// [바드 불협화음 50 보너스] 적용된 디버프만큼 공속 보너스를 깎습니다.
			bonus -= Server.SkillHandlers.Discordance.GetSpeedPenalty(m);

			// 몬스터 예외 처리
			if (m is BaseCreature bc)
			{
				if (bc.AttackSpeed != 0) speed = bc.AttackSpeed;
			}

			// 레슬링 200 보너스 (기존 로직 유지)
			if (this is Fists && m.Skills[SkillName.Wrestling].Value >= 200)
				speed /= 2;

			// 2. 정확한 스케일 연산 (기본 100% = 1,000,000)
			double effectiveBonus = 1000000.0 + bonus;
			
			// 극단적인 디버프 한계치 방어
			if (effectiveBonus < 100000.0) effectiveBonus = 100000.0;

			double rawDelay = speed * (1000000.0 / effectiveBonus);

			// 3. 0.1초 정밀 틱 정규화
			double tickUnit = 0.1;
			double delayInSeconds = Math.Ceiling(rawDelay / tickUnit) * tickUnit;

			// 4. 최소 공속 방어 (서버 한계점)
			if (delayInSeconds < 0.1)
				delayInSeconds = 0.1;

			return TimeSpan.FromSeconds(delayInSeconds);
		}
		public virtual void OnBeforeSwing(Mobile attacker, IDamageable damageable)
		{
            Mobile defender = damageable as Mobile;

			WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

			bool first = false;

			if( a != null && a == PrimaryAbility )
				first = true;

            if (a != null ) //&& (!a.OnBeforeSwing(attacker, defender, WeaponAbilityLevel(attacker, first))))
            {
                WeaponAbility.ClearCurrentAbility(attacker);
            }

			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null && (!move.OnBeforeSwing(attacker, defender) || SkillMasterySpell.CancelSpecialMove(attacker)))
            {
                SpecialMove.ClearCurrentMove(attacker);
            }
		}

        public virtual TimeSpan OnSwing(Mobile attacker, IDamageable damageable)
		{
            return OnSwing(attacker, damageable, 1.0);
		}

        public virtual TimeSpan OnSwing(Mobile attacker, IDamageable damageable, double damageBonus)
		{
			bool canSwing = true;

			//공속
			if (Core.AOS)
			{
				canSwing = ( /* !attacker.Paralyzed && */ !attacker.Frozen);
				int useStam = 0;
				if( attacker.Skills[this.Skill].Value < 150.0 || attacker is PlayerMobile)
				{
					useStam = 5;
				}
				if( attacker.Stam < useStam )
				{
					attacker.SendMessage("당신은 기력이 없어서 무기를 휘두를 힘이 없습니다.");
					canSwing = false;
				}
				
				if( canSwing )
				{
					/*
					if( attacker is PlayerMobile )
					{
						
						if( attacker.Stam < 1 )
						{
							canSwing = false;
						}
						else
							attacker.Stam -= 1;
					}
					*/
					if( useStam > 0 )
					{
						attacker.Stam -= useStam;
					}
					if( attacker is BaseCreature )
					{
						BaseCreature bc = attacker as BaseCreature;
						if( bc.AI == AIType.AI_Mage && !bc.Controlled )
							canSwing = false;
					}
				}
				if (canSwing)
				{
					Spell sp = attacker.Spell as Spell;

					canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
				}

				if (canSwing)
				{
					PlayerMobile p = attacker as PlayerMobile;

					canSwing = (p == null || p.PeacedUntil <= DateTime.UtcNow);
				}
			}

            if (canSwing && attacker.HarmfulCheck(damageable))
			{
				//attacker.DisruptiveAction();

				if (attacker.NetState != null)
				{
                    attacker.Send(new Swing(0, attacker, damageable));
				}

                //if (!CheckHit(attacker, damageable))
				//	damageBonus *= 0.5;
                OnHit(attacker, damageable, damageBonus);
			}

			return GetDelay(attacker);
		}

		#region Sounds
		public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
		{
			int sound = attacker.GetAttackSound();

			if (sound == -1)
			{
				sound = HitSound;
			}

			return sound;
		}

		public virtual int GetHitDefendSound(Mobile attacker, Mobile defender)
		{
			return defender.GetHurtSound();
		}

		public virtual int GetMissAttackSound(Mobile attacker, Mobile defender)
		{
			if (attacker.GetAttackSound() == -1)
			{
				return MissSound;
			}
			else
			{
				return -1;
			}
		}

		public virtual int GetMissDefendSound(Mobile attacker, Mobile defender)
		{
			return -1;
		}
		#endregion

        private Item GetRandomValidItem(Mobile m)
        {
            Item[] items = m.Items.Where(item => _DamageLayers.Contains(item.Layer) && item is IWearableDurability).ToArray();

            if (items.Length == 0)
                return null;

            return items[Utility.Random(items.Length)];
        }

        private List<Layer> _DamageLayers = new List<Layer>()
        {
            Layer.FirstValid,
            Layer.OneHanded,
            Layer.TwoHanded,
            Layer.Shoes,
            Layer.Pants,
            Layer.Shirt,
            Layer.Helm,
            Layer.Arms,
            Layer.Gloves,
            Layer.Ring,
            Layer.Talisman,
            Layer.Neck,
            Layer.Waist,
            Layer.InnerTorso,
            Layer.Bracelet,
            Layer.MiddleTorso,
            Layer.Earrings,
            Layer.Cloak,
            Layer.OuterTorso,
            Layer.OuterLegs,
            Layer.InnerLegs,
        };

		private bool mortalBonus = false;
		private int DamagePosition = -1;
		private bool FuryCheck = false;

		private double skillUp( Mobile attacker, Mobile defender, double point )
		{
			if( attacker == null || defender == null )
				return 0;
			if( attacker == defender )
				return 0;
			if( attacker is PlayerMobile && defender is PlayerMobile )
				return 0;
			
			if( attacker is BaseCreature )
			{
				BaseCreature bc = attacker as BaseCreature;
				if( bc.ControlMaster == defender || bc.SummonMaster == defender )
					return 0;
				if( bc.ControlMaster == null && bc.SummonMaster == null )
					return point * 10;
				if( bc.ControlMaster != null )
				{
					BaseCreature target = defender as BaseCreature;
					if( target.ControlMaster == null && target.SummonMaster == null )
						return point;
					else
						return 0;
				}
				return 0;
			}

			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				if( bc.ControlMaster == attacker || bc.SummonMaster == attacker )
					return 0;
				if( bc.ControlMaster == null && bc.SummonMaster == null )
					return point * 10;
				if( bc.ControlMaster != null )
				{
					BaseCreature target = attacker as BaseCreature;
					if( target.ControlMaster == null && target.SummonMaster == null )
						return point;
					else
						return 0;
				}
				return 0;
			}

			if( attacker is PlayerMobile )
			{
				BaseCreature bc = defender as BaseCreature;
				if( bc.ControlMaster == null && bc.SummonMaster == null )
					return point;
				else 
					return 0;
			}
			
			if( defender is PlayerMobile )
			{
				BaseCreature bc = attacker as BaseCreature;
				if( bc.ControlMaster == null && bc.SummonMaster == null )
					return point;
				else 
					return 0;
			}
			return point;
		}

		//동일 장비 체크 및 스킬 비교 체크
		private bool CrossWeaponValueCheck(Mobile attacker, Mobile defender )
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];

			if( atkSkill == defSkill && atkSkill.Value < defSkill.Value )
				return true;

			return false;
		}

		public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
		{
			if (attacker.Player || defender.Player)
			{
				return 0;
			}

			BaseCreature bc = attacker as BaseCreature;

			if (bc == null || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned))
			{
				return 0;
			}

			Mobile master = bc.ControlMaster;

			if (master == null)
			{
				master = bc.SummonMaster;
			}

			if (master == null)
			{
				return 0;
			}

			int inPack = 1;

            IPooledEnumerable eable = defender.GetMobilesInRange(1);

			foreach (Mobile m in eable)
			{
				if (m != attacker && m is BaseCreature)
				{
					BaseCreature tc = (BaseCreature)m;

					if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned))
					{
						continue;
					}

					Mobile theirMaster = tc.ControlMaster;

					if (theirMaster == null)
					{
						theirMaster = tc.SummonMaster;
					}

					if (master == theirMaster && tc.Combatant == defender)
					{
						++inPack;
					}
				}
			}

            eable.Free();

			if (inPack >= 5)
			{
				return 100;
			}
			else if (inPack >= 4)
			{
				return 75;
			}
			else if (inPack >= 3)
			{
				return 50;
			}
			else if (inPack >= 2)
			{
				return 25;
			}

			return 0;
		}

		private bool m_InDoubleStrike;
        private bool m_ProcessingMultipleHits;

		public bool InDoubleStrike 
        {
            get { return m_InDoubleStrike; }
            set
            { 
                m_InDoubleStrike = value;

                if (m_InDoubleStrike)
                    ProcessingMultipleHits = true;
                else
                    ProcessingMultipleHits = false;
            } 
        }

        public bool ProcessingMultipleHits
        {
            get { return m_ProcessingMultipleHits; }
            set
            {
                m_ProcessingMultipleHits = value;

                if (!m_ProcessingMultipleHits)
                    BlockHitEffects = false;
            }
        }

        public bool EndDualWield { get; set; }
        public bool BlockHitEffects { get; set; }
        public DateTime NextSelfRepair { get; set; }

		public void OnHit(Mobile attacker, IDamageable damageable)
		{
            OnHit(attacker, damageable, 1.0);
		}

		
		// 패링 체크 로직 (기본 구현)
		public bool CheckParry(Mobile m)
		{
			return false;//(m.ParryChance > Utility.RandomDouble());
		}
		// 부위별 방어구 가져오기 로직
		public BaseArmor GetArmorByLocation(Mobile m, int location)
		{
			Layer layer = Layer.Invalid;
			switch (location)
			{
				case 0: layer = Layer.TwoHanded; break; // 방패
				case 1: layer = Layer.Helm; break;
				case 2: layer = Layer.Neck; break;
				case 3: layer = Layer.InnerTorso; break;
				case 4: layer = Layer.Arms; break;
				case 5: layer = Layer.Gloves; break;
				case 6: layer = Layer.Pants; break;
			}
			return m.FindItemOnLayer(layer) as BaseArmor;
		}
		//스킬 포인트 획득 함수
		private void CheckWeaponSkillGain(Mobile attacker, Mobile defender, Skill atkSkill, Skill defSkill)
		{
			if (attacker == null || defender == null || atkSkill == null || defSkill == null)
				return;

			double checkPoint = 0;

			// 1. 방어자가 플레이어인 경우 (공격자는 야생 몬스터여야 함)
			if (defender is PlayerMobile)
			{
				BaseCreature bc = attacker as BaseCreature;
				if (bc != null && bc.ControlMaster == null)
				{
					// 플레이어의 방어 기술에 비례하여 대폭 상승 (5배 보너스)
					checkPoint = defSkill.Value * 5;
				}
			}
			// 2. 방어자가 야생 몬스터인 경우
			else if (defender is BaseCreature)
			{
				BaseCreature bc = defender as BaseCreature;
				if (bc != null && bc.ControlMaster == null)
				{
					// 몬스터의 방어 기술 + 바딩 난이도를 합산하여 체크 포인트 결정
					checkPoint = defSkill.Value + bc.BardingDifficulty;
				}
			}

			// 최종 스킬 체크 실행
			if (checkPoint > 0)
			{
				attacker.CheckSkill(atkSkill.SkillName, checkPoint);
			}
		}
		
		private void UpdateCombatTimers(Mobile attacker, Mobile defender)
		{
			if (attacker == null || defender == null)
				return;

			// 1. 플레이어 vs 플레이어 (PvP 타이머)
			if (attacker is PlayerMobile && defender is PlayerMobile)
			{
				PlayerMobile apm = (PlayerMobile)attacker;
				PlayerMobile dpm = (PlayerMobile)defender;

				// TimerList[65]: PvP 전투 상태 유지 (예: 300초)
				if (apm.TimerList[65] < 300) apm.TimerList[65] = 300;
				if (dpm.TimerList[65] < 300) dpm.TimerList[65] = 300;
			}
			// 2. 몬스터 vs 플레이어 (PvM 타이머)
			else if (attacker is BaseCreature && defender is PlayerMobile)
			{
				PlayerMobile dpm = (PlayerMobile)defender;

				// TimerList[64]: 몬스터와의 전투 상태 유지 (예: 60초)
				if (dpm.TimerList[64] < 60) dpm.TimerList[64] = 60;
			}
		}
		
		#region 특수기 데미지 설정
		public static int GetWeaponCategoryID(BaseWeapon weapon, Mobile attacker)
		{
			if (attacker is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)attacker;
				
				// 두 슬롯 중 하나라도 설정되어 있는지 확인
				if (bc.SpecialType1 >= 0 || bc.SpecialType2 >= 0)
				{
					double roll = Utility.RandomDouble(); // 0.0 ~ 1.0 사이의 주사위
					
					// 1. 첫 번째 구간 체크 (0 ~ SpecialChance1)
					if (bc.SpecialType1 >= 0 && roll < bc.SpecialChance1)
					{
						return bc.SpecialType1;
					}
					
					// 2. 두 번째 구간 체크 (SpecialChance1 ~ SpecialChance1 + SpecialChance2)
					// 예: 1번 20%(0.2), 2번 10%(0.1) 일 때, roll이 0.2 ~ 0.3 사이면 2번 당첨
					if (bc.SpecialType2 >= 0 && roll < (bc.SpecialChance1 + bc.SpecialChance2))
					{
						return bc.SpecialType2;
					}
				}
				return -1; // 합산 확률 구간에 들지 못함 (당첨 실패)
			}		
		
			// 0: 한손 검 (Swords + OneHanded)
			if (weapon.Skill == SkillName.Swords && weapon.Layer == Layer.OneHanded && !(weapon is BaseAxe))
				return 0;

			// 1: 양손 검 (Swords + TwoHanded)
			if (weapon.Skill == SkillName.Swords && weapon.Layer == Layer.TwoHanded && !(weapon is BaseAxe))
				return 1;

			// 2: 도끼 (BaseAxe 클래스 판정)
			if (weapon is BaseAxe)
				return 2;

			// 3: 한손 둔기 (Macing + OneHanded)
			if (weapon.Skill == SkillName.Macing && weapon.Layer == Layer.OneHanded)
				return 3;

			// 4: 양손 둔기 (Macing + TwoHanded)
			if (weapon.Skill == SkillName.Macing && weapon.Layer == Layer.TwoHanded)
				return 4;

			// 5: 한손 펜싱 (Fencing + OneHanded)
			if (weapon.Skill == SkillName.Fencing && weapon.Layer == Layer.OneHanded)
				return 5;

			// 6: 양손 펜싱 (Fencing + TwoHanded)
			if (weapon.Skill == SkillName.Fencing && weapon.Layer == Layer.TwoHanded)
				return 6;

			// 7: 활 (BaseRanged 중 석궁류가 아닌 것)
			if (weapon is BaseRanged && !(weapon is Crossbow || weapon is HeavyCrossbow || weapon is RepeatingCrossbow))
				return 7;

			// 8: 석궁 (Crossbow 계열)
			if (weapon is Crossbow || weapon is HeavyCrossbow || weapon is RepeatingCrossbow)
				return 8;

			// 9: 맨손 (Fists)
			if (weapon is Fists || weapon == null)
				return 9;

			return 9; // 기본값은 맨손으로 처리
		}	
		#endregion		
        public virtual void OnHit(Mobile attacker, IDamageable damageable, double damageBonus)
		{
			if( damageable == null )
				return;
			
            if (EndDualWield)
            {
                ProcessingMultipleHits = false;
                EndDualWield = false;
            }

            Mobile defender = damageable as Mobile;
            Clone clone = null;
				
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];
			

			BaseWeapon one = null;
			BaseWeapon two = null;
			if( attacker is PlayerMobile )
			{
				one = attacker.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;
				two = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
			}			

			//무기술 증가
			CheckWeaponSkillGain(attacker, defender, atkSkill, defSkill);
			
            if (defender != null)
            {
                clone = MirrorImage.GetDeflect(attacker, defender);
            }

            if (clone != null)
            {
                defender = clone;
            }

			PlaySwingAnimation(attacker);

            if(defender != null)
			    PlayHurtAnimation(defender);

			attacker.PlaySound(GetHitAttackSound(attacker, defender));

            if(defender != null)
			    defender.PlaySound(GetHitDefendSound(attacker, defender));

			int damage = ComputeDamage(attacker, defender); //데미지 결정

			//전투 체크
			UpdateCombatTimers(attacker, defender);
			
			//전투 로직 시작
			// --- [1단계: 무기 민 ~ 맥 랜덤치 결정 및 숙련도 가중치] ---
			// ScaleDamageAOS를 통해 스탯/스킬이 반영된 기초 민~맥뎀을 가져옵니다.
			bool isSpecialProc = false; 
			int phys = 100, fire = 0, cold = 0, pois = 0, nrgy = 0, chaos = 0, direct = 0; // 데미지 타입 초기화
			bool ranged = this is BaseRanged;
			if (DefenseMastery.IsPerfectDefense(defender))
			{
				damage = 1;
				defender.SendLocalizedMessage(1063345);
				attacker.SendLocalizedMessage(1063346);
				defender.FixedEffect(0x376A, 9, 32); // 방어 성공 시 스파크 이펙트 추가 (선택)
			}
			else
			{
				int min = Math.Max((int)ScaleDamageAOS(attacker, MinDamage, false), 1);
				int max = Math.Max((int)ScaleDamageAOS(attacker, MaxDamage, false), 1);
				
				// 1. 초기 부위 설정 (이미 결정된 부위가 있다면 그 값을, 없다면 -1)
				int hitLocation = -1; 

				BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
				bool isParried = false;

				if (Server.Spells.Chivalry.ConsecrateWeaponSpell.UnderAura(attacker))
				{
					min += 1;
					max += 1;
				}				

				if (shield != null)
				{
					double parryChance = 0.0;
					
					if (shield.ItemID == 0x1BC4 && defender.Skills[SkillName.Chivalry].Value >= 150.0)
						parryChance = defender.Skills[SkillName.Chivalry].Value * 0.0005;
					else if (shield.ItemID == 0x1BC3 && defender.Skills[SkillName.Necromancy].Value >= 150.0)
						parryChance = defender.Skills[SkillName.Necromancy].Value * 0.0005;
					else
						parryChance = defender.Skills[SkillName.Parry].Value * 0.0005;

					if (parryChance > Utility.RandomDouble())
					{
						// 시각적 효과만 여기서 처리
						defender.FixedEffect(0x37B9, 10, 16);
						defender.Animate(AnimationType.Parry, 0);
						defender.PlaySound(0x1F7); 

						// [핵심] 패링 성공 시 타겟을 0(방패)으로 강제 고정
						hitLocation = 0; 
						
						attacker.SendLocalizedMessage(1061128);
						defender.SendLocalizedMessage(1061127);
					}
				}			

				//특수기 발동 체크
				// 1. 특수기 발동 여부 먼저 판단 (여기서 걸러지면 끝)
				if (attacker is PlayerMobile)
				{
					isSpecialProc = (atkSkill.Value >= 50.0 && 0.05 > Utility.RandomDouble());
				}
				else if (attacker is BaseCreature)
				{
					BaseCreature bc = (BaseCreature)attacker;
					// 몬스터는 설정된 두 확률의 합만큼 발동 확률을 가짐
					isSpecialProc = ((bc.SpecialChance1 + bc.SpecialChance2) > Utility.RandomDouble());
				}
				bool forceArrow = isSpecialProc && (attacker.Skills[SkillName.Tactics].Value >= 200.0) && (atkWeapon.Skill == SkillName.Archery);
				// 2. 엔진을 통해 데미지 재계산 및 피격 부위 확정
				// (damage는 이미 위에서 계산된 기초 데미지값이므로 이를 인자로 활용 가능)
				var (calculatedDamage, finalLoc) = CombatEngine.CalculateFinalDamage(attacker, defender, min, max, hitLocation, false, forceArrow);
				// 3. 내구도 처리 및 최종 데미지 확정 (기존 damage 변수에 덮어쓰기)
				damage = CombatEngine.OnCombatAction(attacker, defender, calculatedDamage, finalLoc, false);
				
				//int phys, fire, cold, pois, nrgy, chaos, direct;

				if ( SkillMasterySpell.HasSpell<ShieldBashSpell>(attacker))
				{
					phys = 100;
					fire = cold = pois = nrgy = chaos = direct = 0;
				}
				else
				{
					GetDamageTypes(attacker, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

					if (!OnslaughtSpell.HasOnslaught(attacker, defender) )
					{
						phys = damageable.PhysicalResistance;
						fire = damageable.FireResistance;
						cold = damageable.ColdResistance;
						pois = damageable.PoisonResistance;
						nrgy = damageable.EnergyResistance;

						int low = phys, type = 0;

						if (fire < low) { low = fire; type = 1; }
						if (cold < low) { low = cold; type = 2; }
						if (pois < low) { low = pois; type = 3; }
						if (nrgy < low) { low = nrgy; type = 4; }

						phys = fire = cold = pois = nrgy = chaos = direct = 0;

						if (type == 0) phys = 100;
						else if (type == 1) fire = 100;
						else if (type == 2) cold = 100;
						else if (type == 3) pois = 100;
						else if (type == 4) nrgy = 100;
					}
					else if (Core.ML && ranged)
					{
						IRangeDamage rangeDamage = attacker.FindItemOnLayer(Layer.Cloak) as IRangeDamage;

						if (rangeDamage != null)
						{
							rangeDamage.AlterRangedDamage(ref phys, ref fire, ref cold, ref pois, ref nrgy, ref chaos, ref direct);
						}
					}
				}

				bool splintering = false;


				//택틱, 해부학 스킬 증가
				if( attacker.Alive && defender.Alive )
				{
					attacker.CheckSkill( SkillName.Tactics, skillUp( attacker, defender, defender.Skills.Tactics.Value ) );
					attacker.CheckSkill( SkillName.Anatomy, skillUp( attacker, defender, defender.Skills.Anatomy.Value ) );
					if( two != null )
					{
						attacker.CheckSkill( SkillName.Bushido, skillUp( attacker, defender, damage ) );
						defender.CheckSkill( SkillName.Bushido, skillUp( attacker, defender, damage ) );
					}
					if( one != null )
					{
						attacker.CheckSkill( SkillName.Ninjitsu, skillUp( attacker, defender, damage ) );
						defender.CheckSkill( SkillName.Ninjitsu, skillUp( attacker, defender, damage ) );
					}				
				}
				
				//전체 데미지 증가 감소(SPM 포함)
				if( attacker is PlayerMobile )
				{
					PlayerMobile pm = attacker as PlayerMobile;
					if( pm.disarmtime > DateTime.Now )
					{
						damage *= 100 - pm.disarmweak;
						damage /= 100;
					}
					
				}
				else if( attacker is BaseCreature )
				{
					BaseCreature bc = attacker as BaseCreature;
					if( bc.disarmtime > DateTime.Now )
					{
						damage *= 100 - bc.disarmweak;
						damage /= 100;
					}
					
				}
				if( defender is PlayerMobile && defender.Skills[SkillName.Tactics].Value > 0 )
				{
					if( defender is PlayerMobile )
					{
						PlayerMobile pm = defender as PlayerMobile;
						if( pm.WeaponDefenseTime > DateTime.Now )
						{
							damage /= 2;
							if( defender.Skills[SkillName.Tactics].Value >= 150 )
								damage /= 2;
						}
						pm.WeaponDefenseTime = DateTime.Now + TimeSpan.FromSeconds(defender.Skills[SkillName.Tactics].Value * 0.025);
						if( pm.dismounttime > DateTime.Now )
						{
							damage *= 100 + pm.dismountweak;
							damage /= 100;
						}
					}
					if( defender is BaseCreature )
					{
						BaseCreature bc = defender as BaseCreature;
						if( bc.WeaponDefenseTime > DateTime.Now )
						{
							damage /= 2;
							if( defender.Skills[SkillName.Tactics].Value >= 150 )
								damage /= 2;
						}
						bc.WeaponDefenseTime = DateTime.Now + TimeSpan.FromSeconds(defender.Skills[SkillName.Tactics].Value * 0.01);
						if( bc.dismounttime > DateTime.Now )
						{
							damage *= 100 + bc.dismountweak;
							damage /= 100;
						}
					}				
				}
			}

			if( isSpecialProc )
			{
				// 무기 타입 판별 (앞서 만든 GetSpecialWeaponType 함수 사용)
				int typeID = GetWeaponCategoryID(this, attacker);

				// 전술 단계에 따른 누적 특수기 연쇄 시전!
				// 예: 전술 150이면 50점 기술, 100점 기술, 150점 기술이 차례대로 터짐
				if (typeID >= 0)
				{
					SpecialAbilityManager.ExecuteChainAbilities(typeID, attacker, defender, damage);
				}
			}
            if (defender == null)
            {
                AOS.Damage(damageable, attacker, damage, FuryCheck, phys, fire, cold, pois, nrgy, chaos, direct, false, ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

                SpecialMove.ClearCurrentMove(attacker);
                if (AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) > 0)
                {
                    attacker.SendLocalizedMessage(1152566); // You fail to leech life from your target!
                }

                return;
            }
		
			if( defender != null )
			{
				int specialDamage = 0;
				if( two != null && attacker.Skills[SkillName.Bushido].Value >= 100 )
					specialDamage = Misc.Util.SmashCalc(attacker, defender);
				else if( one != null && defender.Combatant != attacker && attacker.Skills[SkillName.Ninjitsu].Value >= 100 )
					specialDamage = (int)( damage * 1 + Misc.Util.SneakCalc(attacker, defender, damage) );

				damage += specialDamage;
			}

            Timer.DelayCall(d => AddBlood(d, damage), defender);

				
			int damageGiven = damage;

            if (defender == null)
            {
                AOS.Damage(damageable, attacker, damage, FuryCheck, phys, fire, cold, pois, nrgy, chaos, direct, false, ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

                // TODO: WeaponAbility/SpecialMove OnHit(...) convert target to IDamageable
                // Figure out which specials work on items. For now AI only.
                //WeaponAbility.ClearCurrentAbility(attacker);
                SpecialMove.ClearCurrentMove(attacker);
                if (AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) > 0)
                {
                    attacker.SendLocalizedMessage(1152566); // You fail to leech life from your target!
                }

                return;
            }
			else
			{
				damageGiven = AOS.Damage(
				defender,
				attacker,
				damage,
				FuryCheck,
				phys,
				fire,
				cold,
				pois,
				nrgy,
				chaos,
				direct,
				false,
				ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

				if (Core.AOS)
				{
					int maChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitMagicArrow));
					int harmChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitHarm));
					int fireballChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireball));
					int lightningChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLightning));

					int witherChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitDispel));
					
					if (maChance != 0 && maChance > Utility.Random(1000))
					{
						DoMagicArrow(attacker, defender, maChance);
					}

					if (harmChance != 0 && harmChance > Utility.Random(1000))
					{
						DoHarm(attacker, defender, harmChance);
					}

					if (fireballChance != 0 && fireballChance > Utility.Random(1000))
					{
						DoFireball(attacker, defender, fireballChance);
					}

					if (lightningChance != 0 && lightningChance > Utility.Random(1000))
					{
						DoLightning(attacker, defender, lightningChance);
					}
					if (witherChance != 0 && witherChance > Utility.Random(1000))
					{
						DoWither(attacker, defender, witherChance);
					}
				}

				if (attacker is BaseCreature)
				{
					((BaseCreature)attacker).OnGaveMeleeAttack(defender);
				}

				if (defender is BaseCreature)
				{
					((BaseCreature)defender).OnGotMeleeAttack(attacker);
				}
	
			}

			if( FuryCheck )
				FuryCheck = false;
			XmlAttach.OnWeaponHit(this, attacker, defender, damageGiven);
		}
        public virtual int OnHit(BaseWeapon weapon, int damage)
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
				if (MaxHitPoints == 0 && m_Hits == 0 )
				{
					if (Parent is Mobile)
						((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.		
					Delete();
				}
				else if (m_MaxHits > 0)
				{
					if (m_Hits >= 1)
						HitPoints--;
					else if (m_MaxHits > 0)
					{
						MaxHitPoints--;

						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.

						if (m_MaxHits <= 0)
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
        public Direction GetOppositeDir(Direction d)
        {
            Direction direction = Direction.Down;

            if (d == Direction.West)
                direction = Direction.East;

            if (d == Direction.East)
                direction = Direction.West;

            if (d == Direction.North)
                direction = Direction.South;

            if (d == Direction.South)
                direction = Direction.North;

            if (d == Direction.Right)
                direction = Direction.Left;

            if (d == Direction.Left)
                direction = Direction.Right;

            if (d == Direction.Up)
                direction = Direction.Down;

            if (d == Direction.Down)
                direction = Direction.Up;

            return direction;
        }

		public virtual int GetNewAosDamage(int bonus, int min, int max, Mobile Caster, IDamageable damageable, int spell = 0)
		{
            Mobile target = damageable as Mobile;

            int damage = bonus + Utility.RandomMinMax( min, max ); //Utility.Dice(dice, sides, bonus) * 100;
			int	bonus_damage = max - min;
			double chance_dice = Caster.Skills.Magery.Value - target.Skills.MagicResist.Value;

			if( Caster is BaseCreature )
				chance_dice += Caster.Skills.Meditation.Value;
			
			if( chance_dice > 100 )
				chance_dice = 100;
			else if( chance_dice < -100 )
				chance_dice = -100;

			bonus_damage = (int)( chance_dice * bonus_damage );
			bonus_damage /= 100;
			
			damage += bonus_damage;
			
			if( damage > max )
				damage = max;
			else if( damage < min )
				damage = min;
			
			//기본 데미지
			double statBonus = Caster.Skills.EvalInt.Value * 0.4;
			double skillBonus = Caster.Skills.Spellweaving.Value * 0.2;
			
			int damageBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);
			
			switch ( spell )
			{
				case 5:
				{
					damageBonus += AosWeaponAttributes.GetValue(Caster, AosWeaponAttribute.HitColdArea );
					break;
				}
			}
			
			double totalBonus = ( 1 + damageBonus * 0.001 ) * ( 1 + statBonus * 0.001 ) * ( 1 + skillBonus * 0.01 );
			
			if( totalBonus < 0 )
				totalBonus = 0;

			damage = (int) ( damage * totalBonus );
			return damage;
		}		
		
		#region Do<AoSEffect>
		public virtual void DoMagicArrow(Mobile attacker, Mobile defender, int magicvalue)
		{
			if (!attacker.CanBeHarmful(defender, false))
			{
				return;
			}

			if( attacker.Mana < 3 )
				return;
			
			attacker.Mana -= 3;
			
			attacker.DoHarmful(defender);

			attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
			attacker.PlaySound(0x1E5);

			int damage = GetNewAosDamage(0, 10, 25, attacker, defender, 1);
			if( magicvalue > 1000 )
			{
				damage *= 10000 + magicvalue;
				damage /= 10000;
				
			}
			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
			
            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		public virtual void DoHarm(Mobile attacker, Mobile defender, int magicvalue)
		{
			if (!attacker.CanBeHarmful(defender, false))
			{
				return;
			}

			if( attacker.Mana < 5 )
				return;
			
			attacker.Mana -= 5;
			attacker.DoHarmful(defender);

			defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
			defender.PlaySound(0x0FC);

			int damage = GetNewAosDamage(0, 28, 35, attacker, defender, 2); 
			if( magicvalue > 1000 )
			{
				damage *= 10000 + magicvalue;
				damage /= 10000;
				
			}
			
			SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		public virtual void DoFireball(Mobile attacker, Mobile defender, int magicvalue)
		{
			if (!attacker.CanBeHarmful(defender, false))
			{
				return;
			}

			if( attacker.Mana < 7 )
				return;
			
			attacker.Mana -= 7;
			attacker.DoHarmful(defender);

			int damage = GetNewAosDamage(0, 21, 49, attacker, defender, 3); 
			if( magicvalue > 1000 )
			{
				damage *= 10000 + magicvalue;
				damage /= 10000;
				
			}

			attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
			attacker.PlaySound(0x15E);

			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		public virtual void DoLightning(Mobile attacker, Mobile defender, int magicvalue)
		{
			if (!attacker.CanBeHarmful(defender, false))
			{
				return;
			}
			if( attacker.Mana < 10 )
				return;
			
			attacker.Mana -= 10;

			attacker.DoHarmful(defender);

			int damage = GetNewAosDamage(0, 14, 84, attacker, defender, 4); 
			if( magicvalue > 1000 )
			{
				damage *= 10000 + magicvalue;
				damage /= 10000;
				
			}

			defender.BoltEffect(0);

			SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		public virtual void DoWither(Mobile attacker, Mobile defender, int magicvalue)
		{
			if (!attacker.CanBeHarmful(defender, false))
			{
				return;
			}
			if( attacker.Mana < 15 )
				return;
			
			attacker.Mana -= 15;

			attacker.DoHarmful(defender);
			int damage = GetNewAosDamage(0, 35, 38, attacker, defender, 5); 

			if( magicvalue > 1000 )
			{
				damage *= 10000 + magicvalue;
				damage /= 10000;
				
			}

			Map map = attacker.Map;

			if (map != null)
			{
				Effects.PlaySound(attacker.Location, map, 0x1FB);
				Effects.PlaySound(attacker.Location, map, 0x10B);
				Effects.SendLocationParticles(EffectItem.Create(attacker.Location, map, EffectItem.DefaultDuration), 0x37CC, 1, 40, 97, 3, 9917, 0);

				foreach (var id in SpellHelper.AcquireIndirectTargets(attacker, attacker.Location, attacker.Map, 6 ))
				{
					Mobile m = id as Mobile;

					attacker.DoHarmful(id);

					if (m != null)
					{
						m.FixedParticles(0x374A, 1, 15, 9502, 97, 3, (EffectLayer)255);
					}
					else
					{
						Effects.SendLocationParticles(id, 0x374A, 1, 30, 97, 3, 9502, 0);
					}
					SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);
				}
			}			

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		/*
        public virtual void DoExplosion(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
            {
                return;
            }

            attacker.DoHarmful(defender);

            double damage = GetAosSpellDamage(attacker, defender, 40, 1, 5);

            defender.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
            defender.PlaySound(0x307);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }
		*/
        public virtual void DoHitVelocity(Mobile attacker, IDamageable damageable)
        {
            int bonus = (int)attacker.GetDistanceToSqrt(damageable);

            if (bonus > 0)
            {
                AOS.Damage(damageable, attacker, bonus * 3, 100, 0, 0, 0, 0);

                if (attacker.Player)
                {
                    attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!
                }

                if (damageable is Mobile && ((Mobile)damageable).Player)
                {
                    ((Mobile)damageable).SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
                }
            }

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

		#region Stygian Abyss
		public virtual void DoCurse(Mobile attacker, Mobile defender)
		{
			attacker.SendLocalizedMessage(1113717); // You have hit your target with a curse effect.
			defender.SendLocalizedMessage(1113718); // You have been hit with a curse effect.

			defender.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
			defender.PlaySound(0x1EA);
            TimeSpan duration = TimeSpan.FromSeconds(30);

			defender.AddStatMod(
                new StatMod(StatType.Str, String.Format("[Magic] {0} Curse", StatType.Str), -10, duration));
			defender.AddStatMod(
                new StatMod(StatType.Dex, String.Format("[Magic] {0} Curse", StatType.Dex), -10, duration));
			defender.AddStatMod(
                new StatMod(StatType.Int, String.Format("[Magic] {0} Curse", StatType.Int), -10, duration));

			int percentage = -10; //(int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
			string args = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", percentage, percentage, percentage, 10, 10, 10, 10);

            Server.Spells.Fourth.CurseSpell.AddEffect(attacker, defender);
            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.Curse, 1075835, 1075836, duration, defender, args));

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		public virtual void DoFatigue(Mobile attacker, Mobile defender, int damagegiven)
		{
			// Message?
			// Effects?
			defender.Stam -= (damagegiven * (100 - m_AosWeaponAttributes.HitFatigue)) / 100;

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}

		public virtual void DoManaDrain(Mobile attacker, Mobile defender, int damagegiven)
		{
			// Message?
			defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
			defender.PlaySound(0x1F8);
			defender.Mana -= (damagegiven * (100 - m_AosWeaponAttributes.HitManaDrain)) / 100;

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}
		#endregion

		public virtual void DoLowerAttack(Mobile from, Mobile defender)
		{
			if (HitLower.ApplyAttack(defender))
			{
				defender.PlaySound(0x28E);
				Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
			}
		}

		public virtual void DoLowerDefense(Mobile from, Mobile defender)
		{
			if (HitLower.ApplyDefense(defender))
			{
				defender.PlaySound(0x28E);
				Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
			}
		}

		public virtual void DoAreaAttack(Mobile from, Mobile defender, int damageGiven, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
		{
			Map map = from.Map;

			if (map == null || defender == null )
			{
				return;
			}

            var list = SpellHelper.AcquireIndirectTargets(from, from, from.Map, 5);

			var count = 0;

            foreach(var m in list)
            {
				++count;

                from.DoHarmful(m, true);
                m.FixedEffect(0x3779, 1, 15, hue, 0);
                AOS.Damage(m, from, (int)(damageGiven), phys, fire, cold, pois, nrgy, Server.DamageType.SpellAOE);
            }

			if (count > 0)
			{
				Effects.PlaySound(from.Location, map, sound);
            }

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
		}
		#endregion

        public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender, SlayerName slayer)
        {
            if (slayer == SlayerName.None)
                return CheckSlayerResult.None;

            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(slayer);

            if (atkSlayer != null && atkSlayer.Slays(defender) && _SuperSlayers.Contains(atkSlayer.Name))
            {
                return CheckSlayerResult.SuperSlayer;
            }

            if (atkSlayer != null && atkSlayer.Slays(defender))
            {
                return CheckSlayerResult.Slayer;
            }

            return CheckSlayerResult.None;
        }

        public CheckSlayerResult CheckSlayerOpposition(Mobile attacker, Mobile defender)
        {
            ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

            if (defISlayer == null)
            {
                defISlayer = defender.Weapon as ISlayer;
            }

            if (defISlayer != null)
            {
                SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
                SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);
                SlayerEntry defSetSlayer = SlayerGroup.GetEntryByName(SetHelper.GetSetSlayer(defender));

                if (defISlayer is Item && defSlayer == null && defSlayer2 == null)
                {
                    defSlayer = SlayerGroup.GetEntryByName(SlayerSocket.GetSlayer((Item)defISlayer));
                }

                if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker) ||
                    defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(attacker) ||
                    defSetSlayer != null && defSetSlayer.Group.OppositionSuperSlays(attacker))
                {
                    return CheckSlayerResult.Opposition;
                }
            }

            return CheckSlayerResult.None;
        }

        public CheckSlayerResult CheckTalismanSlayer(Mobile attacker, Mobile defender)
        {
            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && TalismanSlayer.Slays(talisman.Slayer, defender))
            {
                return CheckSlayerResult.Slayer;
            }
            else if (Slayer3 != TalismanSlayerName.None && TalismanSlayer.Slays(Slayer3, defender))
            {
                return CheckSlayerResult.Slayer;
            }

            return CheckSlayerResult.None;
        }

        private List<SlayerName> _SuperSlayers = new List<SlayerName>()
        {
            SlayerName.Repond, SlayerName.Silver, SlayerName.Fey,
            SlayerName.ElementalBan, SlayerName.Exorcism, SlayerName.ArachnidDoom,
            SlayerName.ReptilianDeath, SlayerName.Dinosaur, SlayerName.Myrmidex,
            SlayerName.Eodon
        };

		#region Blood
		public void AddBlood(Mobile defender, int damage)
		{
			if (damage <= 5 || defender == null || defender.Map == null || !defender.HasBlood || !CanDrawBlood(defender))
			{
				return;
			}

			var m = defender.Map;
			var b = new Rectangle2D(defender.X - 2, defender.Y - 2, 5, 5);

			var count = Core.AOS ? Utility.RandomMinMax(2, 3) : Utility.RandomMinMax(1, 2);

			for (var i = 0; i < count; i++)
			{
				AddBlood(defender, m.GetRandomSpawnPoint(b), m);
			}
		}

		protected virtual void AddBlood(Mobile defender, Point3D target, Map map)
		{
			var blood = CreateBlood(defender);

			var id = blood.ItemID;

			blood.ItemID = 1; // No Draw

			blood.OnBeforeSpawn(target, map);
			blood.MoveToWorld(target, map);
			blood.OnAfterSpawn();

			Effects.SendMovingEffect(defender, blood, id, 7, 10, true, false, blood.Hue, 0);

			Timer.DelayCall(TimeSpan.FromMilliseconds(500), b => b.ItemID = id, blood);
		}

		protected virtual bool CanDrawBlood(Mobile defender)
		{
			return defender.HasBlood;
		}

		protected virtual Blood CreateBlood(Mobile defender)
		{
			return new Blood
			{
				Hue = defender.BloodHue
			};
		}
		#endregion

		#region Elemental Damage
		public static int[] GetElementDamages(Mobile m)
		{
			var o = new[] {100, 0, 0, 0, 0, 0, 0};

			var w = m.Weapon as BaseWeapon ?? Fists;

			if (w != null)
			{
				w.GetDamageTypes(m, out o[0], out o[1], out o[2], out o[3], out o[4], out o[5], out o[6]);
			}

			return o;
		}

		public virtual void GetDamageTypes(
			Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
		{
			if (wielder is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)wielder;

				phys = bc.PhysicalDamage;
				fire = bc.FireDamage;
				cold = bc.ColdDamage;
				pois = bc.PoisonDamage;
				nrgy = bc.EnergyDamage;
				chaos = bc.ChaosDamage;
				direct = bc.DirectDamage;
			}
			else
			{
				fire = m_AosElementDamages.Fire / 10000;
				cold = m_AosElementDamages.Cold / 10000;
				pois = m_AosElementDamages.Poison / 10000;
				nrgy = m_AosElementDamages.Energy / 10000;
				chaos = m_AosElementDamages.Chaos / 10000;
				direct = m_AosElementDamages.Direct / 10000;

				phys = 100 - fire - cold - pois - nrgy - chaos - direct;
				CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

				if (resInfo != null)
				{
					CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

					if (attrInfo != null)
					{
						int left = phys;

						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponColdDamage, ref cold, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponEnergyDamage, ref nrgy, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponFireDamage, ref fire, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponPoisonDamage, ref pois, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponChaosDamage, ref chaos, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponDirectDamage, ref direct, left);

						phys = left;
					}
				}
			}
		}

		private int ApplyCraftAttributeElementDamage(int attrDamage, ref int element, int totalRemaining)
		{
			if (totalRemaining <= 0)
			{
				return 0;
			}

			if (attrDamage <= 0)
			{
				return totalRemaining;
			}

			int appliedDamage = attrDamage;

			if ((appliedDamage + element) > 100)
			{
				appliedDamage = 100 - element;
			}

			if (appliedDamage > totalRemaining)
			{
				appliedDamage = totalRemaining;
			}

			element += appliedDamage;

			return totalRemaining - appliedDamage;
		}
		#endregion
		
		public virtual void OnMiss(Mobile attacker, IDamageable damageable)
		{
            Mobile defender = damageable as Mobile;

			PlaySwingAnimation(attacker);
			attacker.PlaySound(GetMissAttackSound(attacker, defender));

            if(defender != null)
			    defender.PlaySound(GetMissDefendSound(attacker, defender));

			if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
			{
				((IHonorTarget)defender).ReceivedHonorContext.OnTargetMissed(attacker);
			}

            SkillMasterySpell.OnMiss(attacker, defender);
		}
		
		public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
		{
			if (attacker is BaseCreature)
			{
				BaseCreature c = (BaseCreature)attacker;

				if (c.DamageMin >= 0)
				{
					min = c.DamageMin;
					max = c.DamageMax;
					return;
				}

				if (this is Fists && !attacker.Body.IsHuman)
				{
					min = attacker.Str / 28;
					max = attacker.Str / 28;
					return;
				}
			}

            if (this is Fists && TransformationSpellHelper.UnderTransformation(attacker, typeof(HorrificBeastSpell)))
            {
                min = 5;
                max = 15;
            }
            else
            {
                min = MinDamage;
                max = MaxDamage;
            }
		}

		//무기술 대미지 재보정
		public virtual double GetBaseDamage(Mobile attacker)
		{
			int min, max;

			GetBaseDamageRange(attacker, out min, out max);

			int damage = Utility.RandomMinMax(min, max);

			if (Core.AOS)
			{
				return damage;
			}

			/* Apply damage level offset
             * : Regular : 0
             * : Ruin    : 1
             * : Might   : 3
             * : Force   : 5
             * : Power   : 7
             * : Vanq    : 9
             */
			if (m_DamageLevel != WeaponDamageLevel.Regular)
			{
				damage += (2 * (int)m_DamageLevel) - 1;
			}

			return damage;
		}

		public virtual double GetBonus(double value, double scalar, double threshold, double offset)
		{
			double bonus = value * scalar;

			if (value >= threshold)
			{
				bonus += offset;
			}

			return bonus / 100;
		}

		public virtual int GetHitChanceBonus()
		{
			if (!Core.AOS)
			{
				return 0;
			}

			int bonus = 0;

			switch (m_AccuracyLevel)
			{
				case WeaponAccuracyLevel.Accurate:
					bonus += 02;
					break;
				case WeaponAccuracyLevel.Surpassingly:
					bonus += 04;
					break;
				case WeaponAccuracyLevel.Eminently:
					bonus += 06;
					break;
				case WeaponAccuracyLevel.Exceedingly:
					bonus += 08;
					break;
				case WeaponAccuracyLevel.Supremely:
					bonus += 10;
					break;
			}

			return bonus;
		}

		public virtual int GetDamageBonus()
		{
            #region Stygian Abyss
            if (m_DImodded)
                return 0;
            #endregion

			int bonus = VirtualDamageBonus;

			if (!Core.AOS)
			{
				switch (m_Quality)
				{
					case ItemQuality.Low:
						bonus -= 20;
						break;
					case ItemQuality.Exceptional:
						bonus += 20;
						break;
				}

				switch (m_DamageLevel)
				{
					case WeaponDamageLevel.Ruin:
						bonus += 15;
						break;
					case WeaponDamageLevel.Might:
						bonus += 20;
						break;
					case WeaponDamageLevel.Force:
						bonus += 25;
						break;
					case WeaponDamageLevel.Power:
						bonus += 30;
						break;
					case WeaponDamageLevel.Vanq:
						bonus += 35;
						break;
				}
			}

			return bonus;
		}

		public virtual void GetStatusDamage(Mobile from, out int min, out int max)
		{
			int baseMin, baseMax;

			GetBaseDamageRange(from, out baseMin, out baseMax);

			if (Core.AOS)
			{
				//데미지 스텟표기창
				min = (int)( Math.Max((int)ScaleDamageAOS(from, baseMin, false), 1) );
				max = (int)( Math.Max((int)ScaleDamageAOS(from, baseMax, false), 1) );
			}
			else
			{
				min = Math.Max((int)ScaleDamageOld(from, baseMin, false), 1);
				max = Math.Max((int)ScaleDamageOld(from, baseMax, false), 1);
			}
		}

		public virtual double ScaleDamageAOS(Mobile attacker, double damage, bool checkSkills)
		{
			#region Physical bonuses
			/*
            * These are the bonuses given by the physical characteristics of the mobile.
            * No caps apply.
            */

			//민첩성 스텟 데미지 보너스
			double bonus = 0;
			double skillBonus = 0;
			if( attacker is PlayerMobile )
			{
				bonus += attacker.Dex * 0.05;
				bonus += Math.Min( AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage), 15000) * 0.01;
				bonus += AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.UseBestSkill) * 0.01;
				PlayerMobile pm = attacker as PlayerMobile;
				//damageBonus += pm.SilverPoint[6] * 100;
				if( pm.TimerList[70] != 0 )
				{
					bonus += pm.PotionPower;
				}
				
				/* 무기 피해 증가 패시브 보너스 위치
				bonus += PassiveOption;
				*/
			}
			//공통 스킬 보너스 설계
			skillBonus += attacker.Skills[SkillName.Anatomy].Value * 0.00125;
			if( attacker.Skills[SkillName.Anatomy].Value >= 150 )
				skillBonus += 0.1;
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			skillBonus += attacker.Skills[atkWeapon.Skill].Value * 0.002;
			if( attacker.Skills[atkWeapon.Skill].Value >= 100 )
				skillBonus += 0.1;
			skillBonus += attacker.Skills[SkillName.Tactics].Value * 0.00125;
			if( attacker.Skills[SkillName.Tactics].Value >= 100 )
				skillBonus += 0.05;
			
			//double skillBonus = attacker.Skills[SkillName.Anatomy].Value * 0.0025 + attacker.Skills[SkillName.Tactics].Value * 0.0025 + attacker.Skills[SkillName.Focus].Value * 0.002;
			//BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			//skillBonus += attacker.Skills[atkWeapon.Skill].Value * 0.002;
			#endregion
			
			//펫 공격력 증가 및 동물학 보너스
			double etcBonus = 0.0;
			if( attacker is BaseCreature )
			{
				BaseCreature bc = attacker as BaseCreature;
				if( bc.ControlMaster != null )
				{
					if( bc.MinTameSkill <= bc.ControlMaster.Skills[SkillName.AnimalLore].Value )
					{
						double point = bc.ControlSlots * 5;
						if( bc.Combatant != null && bc.Combatant is BaseCreature )
						{
							BaseCreature defender = bc.Combatant as BaseCreature;
							point += defender.BardingDifficulty;
						}
						if( bc.ControlMaster == null && bc.SummonMaster == null && point > 0 )
							bc.ControlMaster.CheckSkill( SkillName.AnimalLore, point );
					}
					if( bc.ControlMaster is PlayerMobile )
					{
						skillBonus += bc.ControlMaster.Skills[SkillName.AnimalLore].Value * 0.0025;
						if( bc.ControlMaster.Skills[SkillName.AnimalLore].Value >= 100 )
							skillBonus += 0.05;
						skillBonus += bc.ControlMaster.Skills[SkillName.AnimalTaming].Value * 0.002;
						if( bc.ControlMaster.Skills[SkillName.AnimalTaming].Value >= 100 )
							skillBonus += 0.2;
						skillBonus += bc.ControlMaster.Skills[SkillName.Veterinary].Value * 0.00125;
						if( bc.ControlMaster.Skills[SkillName.Veterinary].Value >= 100 )
							skillBonus += 0.05;
					}
					skillBonus *= 0.5;
					//if( bc.AI == AIType.AI_Mage )
					//	damage *= 0.25;
				}
			}
			
			double totalBonus = ( 1 + bonus * 0.01 );
			if( totalBonus < 0 )
				totalBonus = 0;
			
			damage *= totalBonus;
			damage += ExtendedWeaponAttributes.GetValue(attacker, ExtendedWeaponAttribute.BaseWeaponDamage) * 0.01;
			damage += ExtendedWeaponAttributes.GetValue(attacker, ExtendedWeaponAttribute.BaseAllDamage) * 0.01;

			return damage * totalBonus;
		}

		public virtual int VirtualDamageBonus { get { return 0; } }

		public virtual int ComputeDamageAOS(Mobile attacker, Mobile defender)
		{
			return (int)ScaleDamageAOS(attacker, GetBaseDamage(attacker), true);
		}

		public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills)
		{
			if (checkSkills)
			{
				attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap);
					// Passively check tactics for gain
				attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap);
					// Passively check Anatomy for gain

				if (Type == WeaponType.Axe)
				{
					attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
				}
			}

			/* Compute tactics modifier
            * :   0.0 = 50% loss
            * :  50.0 = unchanged
            * : 100.0 = 50% bonus
            */
			damage += (damage * ((attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0));

			/* Compute strength modifier
            * : 1% bonus for every 5 strength
            */
			double modifiers = (attacker.Str / 5.0) / 100.0;

			/* Compute anatomy modifier
            * : 1% bonus for every 5 points of anatomy
            * : +10% bonus at Grandmaster or higher
            */
			double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
			modifiers += ((anatomyValue / 5.0) / 100.0);

			if (anatomyValue >= 100.0)
			{
				modifiers += 0.1;
			}

			/* Compute lumberjacking bonus
            * : 1% bonus for every 5 points of lumberjacking
            * : +10% bonus at Grandmaster or higher
            */

			if (Type == WeaponType.Axe)
			{
				double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;
			    lumberValue = (lumberValue/5.0)/100.0;
			    if (lumberValue > 0.2)
			        lumberValue = 0.2;

				modifiers += lumberValue;

				if (lumberValue >= 100.0)
				{
					modifiers += 0.1;
				}
			}

			// New quality bonus:
			if (m_Quality != ItemQuality.Normal)
			{
				modifiers += (((int)m_Quality - 1) * 0.2);
			}

			// Virtual damage bonus:
			if (VirtualDamageBonus != 0)
			{
				modifiers += (VirtualDamageBonus / 100.0);
			}

			// Apply bonuses
			damage += (damage * modifiers);

			return ScaleDamageByDurability((int)damage);
		}

		public virtual int ScaleDamageByDurability(int damage)
		{
			int scale = 100;

			if (m_MaxHits > 0 && m_Hits < m_MaxHits)
			{
				scale = 50 + ((50 * m_Hits) / m_MaxHits);
			}

			return AOS.Scale(damage, scale);
		}

		public virtual int ComputeDamage(Mobile attacker, Mobile defender)
		{
			if (Core.AOS)
			{
				return ComputeDamageAOS(attacker, defender);
			}

			int damage = (int)ScaleDamageOld(attacker, GetBaseDamage(attacker), true);

			// pre-AOS, halve damage if the defender is a player or the attacker is not a player
			if (defender is PlayerMobile || !(attacker is PlayerMobile))
			{
				damage = (int)(damage / 2.0);
			}

			return damage;
		}

		public virtual void PlayHurtAnimation(Mobile from)
		{
			if (from.Mounted)
			{
				return;
			}

            if (Core.SA)
            {
                from.Animate(AnimationType.Impact, 0);
            }
            else
            {
                int action;
                int frames;

                switch (from.Body.Type)
                {
                    case BodyType.Sea:
                    case BodyType.Animal:
                        {
                            action = 7;
                            frames = 5;
                            break;
                        }
                    case BodyType.Monster:
                        {
                            action = 10;
                            frames = 4;
                            break;
                        }
                    case BodyType.Human:
                        {
                            action = 20;
                            frames = 5;
                            break;
                        }
                    default:
                        return;
                }

                from.Animate(action, frames, 1, true, false, 0);
            }
        }

		public virtual void PlaySwingAnimation(Mobile from)
		{
			int action;

            if (Core.SA)
            {
                action = GetNewAnimationAction(from);

                from.Animate(AnimationType.Attack, action); 
            }
            else
            {
                switch (from.Body.Type)
                {
                    case BodyType.Sea:
                    case BodyType.Animal:
                        {
                            action = Utility.Random(5, 2);
                            break;
                        }
                    case BodyType.Monster:
                        {
                            switch (Animation)
                            {
                                default:
                                case WeaponAnimation.Wrestle:
                                case WeaponAnimation.Bash1H:
                                case WeaponAnimation.Pierce1H:
                                case WeaponAnimation.Slash1H:
                                case WeaponAnimation.Bash2H:
                                case WeaponAnimation.Pierce2H:
                                case WeaponAnimation.Slash2H:
                                    action = Utility.Random(4, 3);
                                    break;
                                case WeaponAnimation.ShootBow:
                                    return; // 7
                                case WeaponAnimation.ShootXBow:
                                    return; // 8
                            }

                            break;
                        }
                    case BodyType.Human:
                        {
                            if (!from.Mounted)
                            {
                                action = (int)Animation;
                            }
                            else
                            {
                                switch (Animation)
                                {
                                    default:
                                    case WeaponAnimation.Wrestle:
                                    case WeaponAnimation.Bash1H:
                                    case WeaponAnimation.Pierce1H:
                                    case WeaponAnimation.Slash1H:
                                        action = 26;
                                        break;
                                    case WeaponAnimation.Bash2H:
                                    case WeaponAnimation.Pierce2H:
                                    case WeaponAnimation.Slash2H:
                                        action = 29;
                                        break;
                                    case WeaponAnimation.ShootBow:
                                        action = 27;
                                        break;
                                    case WeaponAnimation.ShootXBow:
                                        action = 28;
                                        break;
                                }
                            }

                            break;
                        }
                    default:
                        return;
                }

                from.Animate(action, 7, 1, true, false, 0);
            }
		}

        public int GetNewAnimationAction(Mobile from)
        {
            switch (Animation)
            {
                default:
                case WeaponAnimation.Wrestle: return 0;
                case WeaponAnimation.Bash1H: return 3;
                case WeaponAnimation.Pierce1H: return 5;
                case WeaponAnimation.Slash1H: return 4;
                case WeaponAnimation.Bash2H: return 6;
                case WeaponAnimation.Pierce2H: return 8;
                case WeaponAnimation.Slash2H: return 7;
                case WeaponAnimation.ShootBow: return 1;
                case WeaponAnimation.ShootXBow: return 2;
                case WeaponAnimation.Throwing: return 9;
            }
        }

		#region Serialization/Deserialization
		private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
		{
			if (setIf)
			{
				flags |= toSet;
			}
		}

		private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
		{
			return ((flags & toGet) != 0);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(24); // version

 			m_AosArmorAttributes.Serialize(writer);
			writer.Write(m_NotUseUniqueOption);
			
			writer.Write(m_CanPoison);
			writer.Write(m_CanExplosion);

			//접두 접미 별도 저장 코드
			
			for (int i = 0; i < m_PrefixOption.Length; i++)
			{
				writer.Write( (int) m_PrefixOption[i] );
			}
			for (int i = 0; i < m_SuffixOption.Length; i++)
			{
				writer.Write( (int) m_SuffixOption[i] );
			}
			
            // Version 19 - Removes m_SearingWeapon as its handled as a socket now
            // Version 18 - removed VvV Item (handled in VvV System) and BlockRepair (Handled as negative attribute)

			writer.Write(m_HiddenRank);
			
            writer.Write(m_UsesRemaining);
            writer.Write(m_ShowUsesRemaining);

            writer.Write(_Owner);
            writer.Write(_OwnerName);

            // Version 15 converts old leech to new leech

            //Version 14
            writer.Write(m_IsImbued);

            //version 13, converted SaveFlags to long, added negative attributes

            //version 12
            #region Runic Reforging
            writer.Write((int)m_ReforgedPrefix);
            writer.Write((int)m_ReforgedSuffix);
            writer.Write((int)m_ItemPower);
            #endregion

            writer.Write(m_DImodded);

			// Version 11
			writer.Write(m_TimesImbued);
            // Version 10
			writer.Write(m_BlessedBy); // Bless Deed

			#region Veteran Rewards
			writer.Write(m_EngravedText);
			#endregion

			#region Mondain's Legacy
			writer.Write((int)m_Slayer3);
			#endregion

			#region Mondain's Legacy Sets
			SetFlag sflags = SetFlag.None;

			SetSaveFlag(ref sflags, SetFlag.Attributes, !m_SetAttributes.IsEmpty);
			SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !m_SetSkillBonuses.IsEmpty);
			SetSaveFlag(ref sflags, SetFlag.Hue, m_SetHue != 0);
			SetSaveFlag(ref sflags, SetFlag.LastEquipped, m_LastEquipped);
			SetSaveFlag(ref sflags, SetFlag.SetEquipped, m_SetEquipped);
			SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, m_SetSelfRepair != 0);
            SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, m_SetPhysicalBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.FireBonus, m_SetFireBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.ColdBonus, m_SetColdBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.PoisonBonus, m_SetPoisonBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.EnergyBonus, m_SetEnergyBonus != 0);

			writer.WriteEncodedInt((int)sflags);

            if (GetSaveFlag(sflags, SetFlag.PhysicalBonus))
            {
                writer.WriteEncodedInt((int)m_SetPhysicalBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.FireBonus))
            {
                writer.WriteEncodedInt((int)m_SetFireBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.ColdBonus))
            {
                writer.WriteEncodedInt((int)m_SetColdBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.PoisonBonus))
            {
                writer.WriteEncodedInt((int)m_SetPoisonBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.EnergyBonus))
            {
                writer.WriteEncodedInt((int)m_SetEnergyBonus);
            }

			if (GetSaveFlag(sflags, SetFlag.Attributes))
			{
				m_SetAttributes.Serialize(writer);
			}

			if (GetSaveFlag(sflags, SetFlag.SkillBonuses))
			{
				m_SetSkillBonuses.Serialize(writer);
			}

			if (GetSaveFlag(sflags, SetFlag.Hue))
			{
				writer.Write(m_SetHue);
			}

			if (GetSaveFlag(sflags, SetFlag.LastEquipped))
			{
				writer.Write(m_LastEquipped);
			}

			if (GetSaveFlag(sflags, SetFlag.SetEquipped))
			{
				writer.Write(m_SetEquipped);
			}

			if (GetSaveFlag(sflags, SetFlag.SetSelfRepair))
			{
				writer.WriteEncodedInt(m_SetSelfRepair);
			}
			#endregion

			// Version 9
			SaveFlag flags = SaveFlag.None;

			SetSaveFlag(ref flags, SaveFlag.DamageLevel, m_DamageLevel != WeaponDamageLevel.Regular);
			SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, m_AccuracyLevel != WeaponAccuracyLevel.Regular);
			SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, m_DurabilityLevel != WeaponDurabilityLevel.Regular);
			SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != ItemQuality.Normal);
			SetSaveFlag(ref flags, SaveFlag.Hits, m_Hits != 0);
			SetSaveFlag(ref flags, SaveFlag.MaxHits, m_MaxHits != 0);
			SetSaveFlag(ref flags, SaveFlag.Slayer, m_Slayer != SlayerName.None);
			SetSaveFlag(ref flags, SaveFlag.Poison, m_Poison != null);
			SetSaveFlag(ref flags, SaveFlag.PoisonCharges, m_PoisonCharges != 0);
			SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
			SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified);
			SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
			SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
			SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
			SetSaveFlag(ref flags, SaveFlag.MinDamage, m_MinDamage != -1);
			SetSaveFlag(ref flags, SaveFlag.MaxDamage, m_MaxDamage != -1);
			SetSaveFlag(ref flags, SaveFlag.HitSound, m_HitSound != -1);
			SetSaveFlag(ref flags, SaveFlag.MissSound, m_MissSound != -1);
			SetSaveFlag(ref flags, SaveFlag.Speed, m_Speed != -1);
			SetSaveFlag(ref flags, SaveFlag.MaxRange, m_MaxRange != -1);
			SetSaveFlag(ref flags, SaveFlag.Skill, m_Skill != (SkillName)(-1));
			SetSaveFlag(ref flags, SaveFlag.Type, m_Type != (WeaponType)(-1));
			SetSaveFlag(ref flags, SaveFlag.Animation, m_Animation != (WeaponAnimation)(-1));
			SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != CraftResource.Iron);
			SetSaveFlag(ref flags, SaveFlag.xAttributes, !m_AosAttributes.IsEmpty);
			SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
			SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, m_PlayerConstructed);
			SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
			SetSaveFlag(ref flags, SaveFlag.Slayer2, m_Slayer2 != SlayerName.None);
			SetSaveFlag(ref flags, SaveFlag.ElementalDamages, !m_AosElementDamages.IsEmpty);
			SetSaveFlag(ref flags, SaveFlag.EngravedText, !String.IsNullOrEmpty(m_EngravedText));
			SetSaveFlag(ref flags, SaveFlag.xAbsorptionAttributes, !m_SAAbsorptionAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.xNegativeAttributes, !m_NegativeAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Altered, m_Altered);
            SetSaveFlag(ref flags, SaveFlag.xExtendedWeaponAttributes, !m_ExtendedWeaponAttributes.IsEmpty);

            writer.Write((long)flags);

			if (GetSaveFlag(flags, SaveFlag.DamageLevel))
			{
				writer.Write((int)m_DamageLevel);
			}

			if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
			{
				writer.Write((int)m_AccuracyLevel);
			}

			if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
			{
				writer.Write((int)m_DurabilityLevel);
			}

			if (GetSaveFlag(flags, SaveFlag.Quality))
			{
				writer.Write((int)m_Quality);
			}

			if (GetSaveFlag(flags, SaveFlag.Hits))
			{
				writer.Write(m_Hits);
			}

			if (GetSaveFlag(flags, SaveFlag.MaxHits))
			{
				writer.Write(m_MaxHits);
			}

			if (GetSaveFlag(flags, SaveFlag.Slayer))
			{
				writer.Write((int)m_Slayer);
			}

			if (GetSaveFlag(flags, SaveFlag.Poison))
			{
				Poison.Serialize(m_Poison, writer);
			}

			if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
			{
				writer.Write(m_PoisonCharges);
			}

			if (GetSaveFlag(flags, SaveFlag.Crafter))
			{
				writer.Write(m_Crafter);
			}

			if (GetSaveFlag(flags, SaveFlag.StrReq))
			{
				writer.Write(m_StrReq);
			}

			if (GetSaveFlag(flags, SaveFlag.DexReq))
			{
				writer.Write(m_DexReq);
			}

			if (GetSaveFlag(flags, SaveFlag.IntReq))
			{
				writer.Write(m_IntReq);
			}

			if (GetSaveFlag(flags, SaveFlag.MinDamage))
			{
				writer.Write(m_MinDamage);
			}

			if (GetSaveFlag(flags, SaveFlag.MaxDamage))
			{
				writer.Write(m_MaxDamage);
			}

			if (GetSaveFlag(flags, SaveFlag.HitSound))
			{
				writer.Write(m_HitSound);
			}

			if (GetSaveFlag(flags, SaveFlag.MissSound))
			{
				writer.Write(m_MissSound);
			}

			if (GetSaveFlag(flags, SaveFlag.Speed))
			{
				writer.Write(m_Speed);
			}

			if (GetSaveFlag(flags, SaveFlag.MaxRange))
			{
				writer.Write(m_MaxRange);
			}

			if (GetSaveFlag(flags, SaveFlag.Skill))
			{
				writer.Write((int)m_Skill);
			}

			if (GetSaveFlag(flags, SaveFlag.Type))
			{
				writer.Write((int)m_Type);
			}

			if (GetSaveFlag(flags, SaveFlag.Animation))
			{
				writer.Write((int)m_Animation);
			}

			if (GetSaveFlag(flags, SaveFlag.Resource))
			{
				writer.Write((int)m_Resource);
			}

			if (GetSaveFlag(flags, SaveFlag.xAttributes))
			{
				m_AosAttributes.Serialize(writer);
			}

			if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
			{
				m_AosWeaponAttributes.Serialize(writer);
			}

			if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
			{
				m_AosSkillBonuses.Serialize(writer);
			}

			if (GetSaveFlag(flags, SaveFlag.Slayer2))
			{
				writer.Write((int)m_Slayer2);
			}

			if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
			{
				m_AosElementDamages.Serialize(writer);
			}

			if (GetSaveFlag(flags, SaveFlag.EngravedText))
			{
				writer.Write(m_EngravedText);
			}

			#region SA
			if (GetSaveFlag(flags, SaveFlag.xAbsorptionAttributes))
			{
				m_SAAbsorptionAttributes.Serialize(writer);
			}

            if (GetSaveFlag(flags, SaveFlag.xNegativeAttributes))
            {
                m_NegativeAttributes.Serialize(writer);
            }
			#endregion

            if (GetSaveFlag(flags, SaveFlag.xExtendedWeaponAttributes))
            {
                m_ExtendedWeaponAttributes.Serialize(writer);
            }
		}

		[Flags]
		private enum SaveFlag : long
		{
			None = 0x00000000,
			DamageLevel = 0x00000001,
			AccuracyLevel = 0x00000002,
			DurabilityLevel = 0x00000004,
			Quality = 0x00000008,
			Hits = 0x00000010,
			MaxHits = 0x00000020,
			Slayer = 0x00000040,
			Poison = 0x00000080,
			PoisonCharges = 0x00000100,
			Crafter = 0x00000200,
			Identified = 0x00000400,
			StrReq = 0x00000800,
			DexReq = 0x00001000,
			IntReq = 0x00002000,
			MinDamage = 0x00004000,
			MaxDamage = 0x00008000,
			HitSound = 0x00010000,
			MissSound = 0x00020000,
			Speed = 0x00040000,
			MaxRange = 0x00080000,
			Skill = 0x00100000,
			Type = 0x00200000,
			Animation = 0x00400000,
			Resource = 0x00800000,
			xAttributes = 0x01000000,
			xWeaponAttributes = 0x02000000,
			PlayerConstructed = 0x04000000,
			SkillBonuses = 0x08000000,
			Slayer2 = 0x10000000,
			ElementalDamages = 0x20000000,
			EngravedText = 0x40000000,
			xAbsorptionAttributes = 0x80000000,
            xNegativeAttributes = 0x100000000,
            Altered = 0x200000000,
            xExtendedWeaponAttributes = 0x400000000
        }

		#region Mondain's Legacy Sets
		private static void SetSaveFlag(ref SetFlag flags, SetFlag toSet, bool setIf)
		{
			if (setIf)
			{
				flags |= toSet;
			}
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
			WeaponAttributes = 0x00000002,
			SkillBonuses = 0x00000004,
			Hue = 0x00000008,
			LastEquipped = 0x00000010,
			SetEquipped = 0x00000020,
			SetSelfRepair = 0x00000040,
            PhysicalBonus = 0x00000080,
            FireBonus = 0x00000100,
            ColdBonus = 0x00000200,
            PoisonBonus = 0x00000400,
            EnergyBonus = 0x00000800,
		}
		#endregion

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 24:
				{
					m_AosArmorAttributes = new AosArmorAttributes(this, reader);
					goto case 23;
				}
				case 23:
				{
					m_NotUseUniqueOption = reader.ReadBool();
					goto case 22;
				}
				case 22:
				{
					m_CanPoison = reader.ReadDouble();
					m_CanExplosion = reader.ReadDouble();
					goto case 21;
				}
				case 21:
				{
					for (int i = 0; i < m_PrefixOption.Length; i++)
					{
						m_PrefixOption[i] = reader.ReadInt();
					}
					for (int i = 0; i < m_SuffixOption.Length; i++)
					{
						m_SuffixOption[i] = reader.ReadInt();
					}
					goto case 20;
				}
				case 20:
				{
					m_HiddenRank = reader.ReadInt();
					goto case 19;
				}
                case 19: // Removed SearingWeapon
                case 18:
                case 17:
                    {
                        m_UsesRemaining = reader.ReadInt();
                        m_ShowUsesRemaining = reader.ReadBool();
                        goto case 16;
                    }
                case 16:
                    {
                        if(version == 17)
                            reader.ReadBool();

                        _Owner = reader.ReadMobile();
                        _OwnerName = reader.ReadString();
                        goto case 15;
                    }
                case 15:
                case 14:
                    {
                        m_IsImbued = reader.ReadBool();
                        goto case 13;
                    }
                case 13:
                case 12:
                    {
                        #region Runic Reforging
                        m_ReforgedPrefix = (ReforgedPrefix)reader.ReadInt();
                        m_ReforgedSuffix = (ReforgedSuffix)reader.ReadInt();
                        m_ItemPower = (ItemPower)reader.ReadInt();

                        if (version < 18 && reader.ReadBool())
                        {
                            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                            {
                                m_NegativeAttributes.NoRepair = 1;
                            });
                        }
                        #endregion

                        #region Stygian Abyss
                        m_DImodded = reader.ReadBool();

                        if (version == 18)
                        {
                            if (reader.ReadBool())
                            {
                                Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                                {
                                    AttachSocket(new SearingWeapon(this));
                                });
                            }
                        }
                        goto case 11;
                    }
				case 11:
					{
						m_TimesImbued = reader.ReadInt();

                        #endregion

                        goto case 10;
					}
				case 10:
					{
						m_BlessedBy = reader.ReadMobile();
						m_EngravedText = reader.ReadString();
						m_Slayer3 = (TalismanSlayerName)reader.ReadInt();

						SetFlag flags = (SetFlag)reader.ReadEncodedInt();
                        if (GetSaveFlag(flags, SetFlag.PhysicalBonus))
                        {
                            m_SetPhysicalBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.FireBonus))
                        {
                            m_SetFireBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.ColdBonus))
                        {
                            m_SetColdBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.PoisonBonus))
                        {
                            m_SetPoisonBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.EnergyBonus))
                        {
                            m_SetEnergyBonus = reader.ReadEncodedInt();
                        }

						if (GetSaveFlag(flags, SetFlag.Attributes))
						{
							m_SetAttributes = new AosAttributes(this, reader);
						}
						else
						{
							m_SetAttributes = new AosAttributes(this);
						}

						if (GetSaveFlag(flags, SetFlag.WeaponAttributes))
						{
							m_SetSelfRepair = (new AosWeaponAttributes(this, reader)).SelfRepair;
						}

						if (GetSaveFlag(flags, SetFlag.SkillBonuses))
						{
							m_SetSkillBonuses = new AosSkillBonuses(this, reader);
						}
						else
						{
							m_SetSkillBonuses = new AosSkillBonuses(this);
						}

						if (GetSaveFlag(flags, SetFlag.Hue))
						{
							m_SetHue = reader.ReadInt();
						}

						if (GetSaveFlag(flags, SetFlag.LastEquipped))
						{
							m_LastEquipped = reader.ReadBool();
						}

						if (GetSaveFlag(flags, SetFlag.SetEquipped))
						{
							m_SetEquipped = reader.ReadBool();
						}

						if (GetSaveFlag(flags, SetFlag.SetSelfRepair))
						{
							m_SetSelfRepair = reader.ReadEncodedInt();
						}

						goto case 5;
					}
				case 9:
				case 8:
				case 7:
				case 6:
				case 5:
					{
						SaveFlag flags;

                        if(version < 13)
                            flags = (SaveFlag)reader.ReadInt();
                        else
                            flags = (SaveFlag)reader.ReadLong();

						if (GetSaveFlag(flags, SaveFlag.DamageLevel))
						{
							m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

							if (m_DamageLevel > WeaponDamageLevel.Vanq)
							{
								m_DamageLevel = WeaponDamageLevel.Ruin;
							}
						}

						if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
						{
							m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

							if (m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
							{
								m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
							}
						}

						if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
						{
							m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

							if (m_DurabilityLevel > WeaponDurabilityLevel.Indestructible)
							{
								m_DurabilityLevel = WeaponDurabilityLevel.Durable;
							}
						}

						if (GetSaveFlag(flags, SaveFlag.Quality))
						{
							m_Quality = (ItemQuality)reader.ReadInt();
						}
						else
						{
							m_Quality = ItemQuality.Normal;
						}

						if (GetSaveFlag(flags, SaveFlag.Hits))
						{
							m_Hits = reader.ReadInt();
						}

						if (GetSaveFlag(flags, SaveFlag.MaxHits))
						{
							m_MaxHits = reader.ReadInt();
						}

						if (GetSaveFlag(flags, SaveFlag.Slayer))
						{
							m_Slayer = (SlayerName)reader.ReadInt();
						}

						if (GetSaveFlag(flags, SaveFlag.Poison))
						{
							m_Poison = Poison.Deserialize(reader);
						}

						if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
						{
							m_PoisonCharges = reader.ReadInt();
						}

						if (GetSaveFlag(flags, SaveFlag.Crafter))
						{
							m_Crafter = reader.ReadMobile();
						}

						if (GetSaveFlag(flags, SaveFlag.Identified))
						{
							m_Identified = (version >= 6 || reader.ReadBool());
						}

						if (GetSaveFlag(flags, SaveFlag.StrReq))
						{
							m_StrReq = reader.ReadInt();
						}
						else
						{
							m_StrReq = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.DexReq))
						{
							m_DexReq = reader.ReadInt();
						}
						else
						{
							m_DexReq = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.IntReq))
						{
							m_IntReq = reader.ReadInt();
						}
						else
						{
							m_IntReq = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.MinDamage))
						{
							m_MinDamage = reader.ReadInt();
						}
						else
						{
							m_MinDamage = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.MaxDamage))
						{
							m_MaxDamage = reader.ReadInt();
						}
						else
						{
							m_MaxDamage = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.HitSound))
						{
							m_HitSound = reader.ReadInt();
						}
						else
						{
							m_HitSound = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.MissSound))
						{
							m_MissSound = reader.ReadInt();
						}
						else
						{
							m_MissSound = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.Speed))
						{
							if (version < 9)
							{
								m_Speed = reader.ReadInt();
							}
							else
							{
								m_Speed = reader.ReadFloat();
							}
						}
						else
						{
							m_Speed = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.MaxRange))
						{
							m_MaxRange = reader.ReadInt();
						}
						else
						{
							m_MaxRange = -1;
						}

						if (GetSaveFlag(flags, SaveFlag.Skill))
						{
							m_Skill = (SkillName)reader.ReadInt();
						}
						else
						{
							m_Skill = (SkillName)(-1);
						}

						if (GetSaveFlag(flags, SaveFlag.Type))
						{
							m_Type = (WeaponType)reader.ReadInt();
						}
						else
						{
							m_Type = (WeaponType)(-1);
						}

						if (GetSaveFlag(flags, SaveFlag.Animation))
						{
							m_Animation = (WeaponAnimation)reader.ReadInt();
						}
						else
						{
							m_Animation = (WeaponAnimation)(-1);
						}

						if (GetSaveFlag(flags, SaveFlag.Resource))
						{
							m_Resource = (CraftResource)reader.ReadInt();
						}
						else
						{
							m_Resource = CraftResource.Iron;
						}

						if (GetSaveFlag(flags, SaveFlag.xAttributes))
						{
							m_AosAttributes = new AosAttributes(this, reader);
						}
						else
						{
							m_AosAttributes = new AosAttributes(this);
						}

						if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
						{
							m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						}
						else
						{
							m_AosWeaponAttributes = new AosWeaponAttributes(this);
						}

						if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							((Mobile)Parent).AddSkillMod(m_SkillMod);
						}

						if (version < 7 && m_AosWeaponAttributes.MageWeapon != 0)
						{
							m_AosWeaponAttributes.MageWeapon = 30 - m_AosWeaponAttributes.MageWeapon;
						}

						if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30 &&
							Parent is Mobile)
						{
							m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
							((Mobile)Parent).AddSkillMod(m_MageMod);
						}

						if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
						{
							m_PlayerConstructed = true;
						}

						if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
						{
							m_AosSkillBonuses = new AosSkillBonuses(this, reader);
						}
						else
						{
							m_AosSkillBonuses = new AosSkillBonuses(this);
						}

						if (GetSaveFlag(flags, SaveFlag.Slayer2))
						{
							m_Slayer2 = (SlayerName)reader.ReadInt();
						}

						if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
						{
							m_AosElementDamages = new AosElementAttributes(this, reader);
						}
						else
						{
							m_AosElementDamages = new AosElementAttributes(this);
						}

						if (GetSaveFlag(flags, SaveFlag.EngravedText))
						{
							m_EngravedText = reader.ReadString();
						}

						#region Stygian Abyss
						if (version > 9 && GetSaveFlag(flags, SaveFlag.xAbsorptionAttributes))
						{
							m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this, reader);
						}
						else
						{
							m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);
						}

                        if (version >= 13 && GetSaveFlag(flags, SaveFlag.xNegativeAttributes))
                        {
                            m_NegativeAttributes = new NegativeAttributes(this, reader);
                        }
                        else
                        {
                            m_NegativeAttributes = new NegativeAttributes(this);
                        }
                        #endregion

                        if (GetSaveFlag(flags, SaveFlag.Altered))
                        {
                            m_Altered = true;
                        }

                        if (GetSaveFlag(flags, SaveFlag.xExtendedWeaponAttributes))
                        {
                            m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this, reader);
                        }
                        else
                        {
                            m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);
                        }

                        if (Core.TOL && m_ExtendedWeaponAttributes.MysticWeapon != 0 && m_ExtendedWeaponAttributes.MysticWeapon != 30 && Parent is Mobile)
                        {
                            m_MysticMod = new DefaultSkillMod(SkillName.Mysticism, true, -30 + m_ExtendedWeaponAttributes.MysticWeapon);
                            ((Mobile)Parent).AddSkillMod(m_MysticMod);
                        }

                        break;
					}
				case 4:
					{
						m_Slayer = (SlayerName)reader.ReadInt();

						goto case 3;
					}
				case 3:
					{
						m_StrReq = reader.ReadInt();
						m_DexReq = reader.ReadInt();
						m_IntReq = reader.ReadInt();

						goto case 2;
					}
				case 2:
					{
						m_Identified = reader.ReadBool();

						goto case 1;
					}
				case 1:
					{
						m_MaxRange = reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						if (version == 0)
						{
							m_MaxRange = 1; // default
						}

						if (version < 5)
						{
							m_Resource = CraftResource.Iron;
							m_AosAttributes = new AosAttributes(this);
							m_AosWeaponAttributes = new AosWeaponAttributes(this);
							m_AosElementDamages = new AosElementAttributes(this);
							m_AosSkillBonuses = new AosSkillBonuses(this);
						}

						m_MinDamage = reader.ReadInt();
						m_MaxDamage = reader.ReadInt();

						m_Speed = reader.ReadInt();

						m_HitSound = reader.ReadInt();
						m_MissSound = reader.ReadInt();

						m_Skill = (SkillName)reader.ReadInt();
						m_Type = (WeaponType)reader.ReadInt();
						m_Animation = (WeaponAnimation)reader.ReadInt();
						m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
						m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
						m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
						m_Quality = (ItemQuality)reader.ReadInt();

						m_Crafter = reader.ReadMobile();

						m_Poison = Poison.Deserialize(reader);
						m_PoisonCharges = reader.ReadInt();

						if (m_StrReq == OldStrengthReq)
						{
							m_StrReq = -1;
						}

						if (m_DexReq == OldDexterityReq)
						{
							m_DexReq = -1;
						}

						if (m_IntReq == OldIntelligenceReq)
						{
							m_IntReq = -1;
						}

						if (m_MinDamage == OldMinDamage)
						{
							m_MinDamage = -1;
						}

						if (m_MaxDamage == OldMaxDamage)
						{
							m_MaxDamage = -1;
						}

						if (m_HitSound == OldHitSound)
						{
							m_HitSound = -1;
						}

						if (m_MissSound == OldMissSound)
						{
							m_MissSound = -1;
						}

						if (m_Speed == OldSpeed)
						{
							m_Speed = -1;
						}

						if (m_MaxRange == OldMaxRange)
						{
							m_MaxRange = -1;
						}

						if (m_Skill == OldSkill)
						{
							m_Skill = (SkillName)(-1);
						}

						if (m_Type == OldType)
						{
							m_Type = (WeaponType)(-1);
						}

						if (m_Animation == OldAnimation)
						{
							m_Animation = (WeaponAnimation)(-1);
						}

						if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							((Mobile)Parent).AddSkillMod(m_SkillMod);
						}

						break;
					}
			}

            if (version < 15)
            {
                if (WeaponAttributes.HitLeechHits > 0 || WeaponAttributes.HitLeechMana > 0)
                {
                    WeaponAttributes.ScaleLeech(Attributes.WeaponSpeed);
                }
            }

			if( m_AosArmorAttributes == null )
				m_AosArmorAttributes = new AosArmorAttributes(this);
			
			
			#region Mondain's Legacy Sets
			if (m_SetAttributes == null)
			{
				m_SetAttributes = new AosAttributes(this);
			}

			if (m_SetSkillBonuses == null)
			{
				m_SetSkillBonuses = new AosSkillBonuses(this);
			}
			#endregion

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

			if (m_Hits <= 0 && m_MaxHits <= 0)
			{
				m_Hits = m_MaxHits = InitMinHits;//Utility.RandomMinMax(InitMinHits, InitMaxHits);
			}

			if (version < 6)
			{
				m_PlayerConstructed = true; // we don't know, so, assume it's crafted
			}

            if (m_Slayer == SlayerName.DaemonDismissal || m_Slayer == SlayerName.BalronDamnation)
                m_Slayer = SlayerName.Exorcism;

            if (m_Slayer2 == SlayerName.DaemonDismissal || m_Slayer2 == SlayerName.BalronDamnation)
                m_Slayer2 = SlayerName.Exorcism;
		}
		#endregion

		public BaseWeapon(int itemID)
			: base(itemID)
		{
			Layer = (Layer)ItemData.Quality;

			m_Quality = ItemQuality.Normal;
			m_StrReq = -1;
			m_DexReq = -1;
			m_IntReq = -1;
			m_MinDamage = -1;
			m_MaxDamage = -1;
			m_HitSound = -1;
			m_MissSound = -1;
			m_Speed = -1;
			m_MaxRange = -1;
			m_Skill = (SkillName)(-1);
			m_Type = (WeaponType)(-1);
			m_Animation = (WeaponAnimation)(-1);

			m_Hits = m_MaxHits = InitMinHits;//Utility.RandomMinMax(InitMinHits, InitMaxHits);

			m_Resource = CraftResource.Iron;
			m_Identified = true;

			m_AosAttributes = new AosAttributes(this);
			m_AosWeaponAttributes = new AosWeaponAttributes(this);
			m_AosArmorAttributes = new AosArmorAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
			m_AosElementDamages = new AosElementAttributes(this);
            m_NegativeAttributes = new NegativeAttributes(this);
            m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);

			#region Stygian Abyss
			m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);
			#endregion

			#region Mondain's Legacy Sets
			m_SetAttributes = new AosAttributes(this);
			m_SetSkillBonuses = new AosSkillBonuses(this);
			#endregion

			m_AosSkillBonuses = new AosSkillBonuses(this);

			m_UsesRemaining = 500;
			
			/*
            if (this is ITool)
            {
                m_UsesRemaining = Utility.RandomMinMax(25, 75);
            }
            else
            {
                m_UsesRemaining = 150;
            }
			*/
		}

		public BaseWeapon(Serial serial)
			: base(serial)
		{ }

		private string GetNameString()
		{
			string name = Name;

			if (name == null)
			{
				name = String.Format("#{0}", LabelNumber);
			}
			return name;
		}
		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get { return base.Hue; }
			set
			{
				base.Hue = value;
				InvalidateProperties();
			}
		}

		public int GetElementalDamageHue()
		{
			int phys, fire, cold, pois, nrgy, chaos, direct;
			GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
			//Order is Cold, Energy, Fire, Poison, Physical left

			int currentMax = 50;
			int hue = 0;

			if (pois >= currentMax)
			{
				hue = 1267 + (pois - 50) / 10;
				currentMax = pois;
			}

			if (fire >= currentMax)
			{
				hue = 1255 + (fire - 50) / 10;
				currentMax = fire;
			}

			if (nrgy >= currentMax)
			{
				hue = 1273 + (nrgy - 50) / 10;
				currentMax = nrgy;
			}

			if (cold >= currentMax)
			{
				hue = 1261 + (cold - 50) / 10;
				currentMax = cold;
			}

			return hue;
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
            if (m_ExtendedWeaponAttributes.AssassinHoned > 0)
            {
                list.Add(1152207); // Assassin's Edge
                return;
            }

			Misc.NewOptionOPL.AppendName(list, this);

			if (!String.IsNullOrEmpty(m_EngravedText))
			{
                list.Add(1062613, Utility.FixHtml(m_EngravedText));
			}
		}

		public override bool AllowEquipedCast(Mobile from)
		{
			if (base.AllowEquipedCast(from))
			{
				return true;
			}

            return true; //m_AosAttributes.SpellChanneling > 0 || Enhancement.GetValue(from, AosAttribute.SpellChanneling) > 0;
		}

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

		public virtual int GetLuckBonus()
		{
			#region Mondain's Legacy
			if (m_Resource == CraftResource.Heartwood)
			{
				return 0;
			}
			#endregion

			CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

			if (resInfo == null)
			{
				return 0;
			}

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if (attrInfo == null)
			{
				return 0;
			}

			return attrInfo.WeaponLuck;
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
            {
                list.Add(1153213, OwnerName);
            }
			
            if (m_Crafter != null)
            {
                list.Add(1050043, m_Crafter.TitleName); // crafted by ~1_NAME~
            }

            if (m_Quality == ItemQuality.Exceptional)
            {
                list.Add(1060636); // Exceptional
            }

            if (IsImbued)
            {
                list.Add(1080418); // (Imbued)
            }

            if (m_Altered)
            {
                list.Add(1111880); // Altered
            }
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            if (IsVvVItem)
                list.Add(1154937); // VvV Item
        }

        public override void AddUsesRemainingProperties(ObjectPropertyList list)
        {
            if (ShowUsesRemaining)
            {
                list.Add(1060584, UsesRemaining.ToString()); // uses remaining: ~1_val~
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            #region Factions
            //FactionEquipment.AddFactionProperties(this, list);
			#endregion

			//OPL 장비 통합으로 변경			
			Server.Misc.NewOptionOPL.Append(list, this);

			if (IsSetItem && !m_SetEquipped)
			{
				list.Add(1072378); // <br>Only when full set is present:
				GetSetProperties(list);
			}

            if (Core.EJ && LastParryChance > 0)
            {
                list.Add(1158861, LastParryChance.ToString()); // Last Parry Chance: ~1_val~%
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

        public bool CanShowPoisonCharges()
        {
			return true;
        }

        public override void OnSingleClick(Mobile from)
		{
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

			#region Factions
			if (m_FactionState != null)
			{
				attrs.Add(new EquipInfoAttribute(1041350)); // faction item
			}
			#endregion

			if (m_Quality == ItemQuality.Exceptional)
			{
				attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));
			}

			if (m_Identified || from.AccessLevel >= AccessLevel.GameMaster)
			{
				if (m_Slayer != SlayerName.None)
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
					if (entry != null)
					{
						attrs.Add(new EquipInfoAttribute(entry.Title));
					}
				}

				if (m_Slayer2 != SlayerName.None)
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
					if (entry != null)
					{
						attrs.Add(new EquipInfoAttribute(entry.Title));
					}
				}

				if (m_DurabilityLevel != WeaponDurabilityLevel.Regular)
				{
					attrs.Add(new EquipInfoAttribute(1038000 + (int)m_DurabilityLevel));
				}

				if (m_DamageLevel != WeaponDamageLevel.Regular)
				{
					attrs.Add(new EquipInfoAttribute(1038015 + (int)m_DamageLevel));
				}

				if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
				{
					attrs.Add(new EquipInfoAttribute(1038010 + (int)m_AccuracyLevel));
				}
			}
			else if (m_Slayer != SlayerName.None || m_Slayer2 != SlayerName.None ||
					 m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular ||
					 m_AccuracyLevel != WeaponAccuracyLevel.Regular)
			{
				attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
			}

			if (m_Poison != null && m_PoisonCharges > 0)
			{
				attrs.Add(new EquipInfoAttribute(1017383, m_PoisonCharges));
			}

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
			{
				return;
			}

			EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, attrs.ToArray());

			from.Send(new DisplayEquipmentInfo(this, eqInfo));
		}

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            bool drop = base.DropToWorld(from, p);

            EnchantedHotItemSocket.CheckDrop(from, this);

            return drop;
        }
		private int m_HiddenRank;
		[CommandProperty( AccessLevel.GameMaster )]
		public int HiddenRank
		{
			get{ return m_HiddenRank; }
			set{ m_HiddenRank = value; }
		}
		
		public static BaseWeapon Fists { get; set; }

		#region ICraftable Members
		public int OnCraft(
			int quality,
			bool makersMark,
			Mobile from,
			CraftSystem craftSystem,
			Type typeRes,
			ITool tool,
			CraftItem craftItem,
			int resHue)
		{
			Quality = (ItemQuality)quality;

			if (makersMark)
			{
				Crafter = from;
			}

			PlayerConstructed = true;

			if (typeRes == null)
			{
				typeRes = craftItem.Resources.GetAt(0).ItemType;
			}

			if( this is SkinningKnife || this is ButcherKnife || this is Cleaver )
				return quality;
			
			if (Core.AOS)
			{
				if (!craftItem.ForceNonExceptional)
				{
					Resource = CraftResources.GetFromType(typeRes);
				}

				CraftContext context = craftSystem.GetContext(from);
				
				if( from is PlayerMobile )
				{
					double maxValue = 0.8;
					if (Quality == ItemQuality.Exceptional)
					{
						maxValue = 1.0;
						this.MaxHitPoints += 20;
						this.HitPoints += 20;
					}
					
					PlayerMobile pm = from as PlayerMobile;

					/*
						제작술 스킬 1당 옵션 기대치 1로 계산
						장비학 스킬 1당 옵션 기대치 0.2로 계산
						고급일 시 옵션 기대치 값 50 증가				
					*/
					double bonus = from.Skills[craftSystem.MainSkill].Value + from.Skills.ArmsLore.Value * 0.2;
					if (Quality == ItemQuality.Exceptional)
						bonus += 50;
					
					int rank = ItemOptionCreator.ItemCreator(this, bonus, pm);
					if (Quality == ItemQuality.Exceptional)
						pm.CheckSkill(SkillName.ArmsLore, 1500 + rank * 250);
					else
						pm.CheckSkill(SkillName.ArmsLore, 500 + rank * 250);
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
			#endregion

			return quality;
		}

        public virtual void DistributeMaterialBonus(CraftAttributeInfo attrInfo)
        {
            if (m_Resource != CraftResource.Heartwood)
            {
                m_AosAttributes.WeaponDamage += attrInfo.WeaponDamage;
                m_AosAttributes.WeaponSpeed += attrInfo.WeaponSwingSpeed;
                m_AosAttributes.AttackChance += attrInfo.WeaponHitChance;
                m_AosAttributes.RegenHits += attrInfo.WeaponRegenHits;
                m_AosWeaponAttributes.HitLeechHits += attrInfo.WeaponHitLifeLeech;
            }
            else
            {
                switch (Utility.Random(6))
                {
                    case 0: m_AosAttributes.WeaponDamage += attrInfo.WeaponDamage; break;
                    case 1: m_AosAttributes.WeaponSpeed += attrInfo.WeaponSwingSpeed; break;
                    case 2: m_AosAttributes.AttackChance += attrInfo.WeaponHitChance; break;
                    case 3: m_AosAttributes.Luck += attrInfo.WeaponLuck; break;
                    case 4: m_AosWeaponAttributes.LowerStatReq += attrInfo.WeaponLowerRequirements; break;
                    case 5: m_AosWeaponAttributes.HitLeechHits += attrInfo.WeaponHitLifeLeech; break;
                }
            }
        }

		#region Mondain's Legacy Sets
		public override bool OnDragLift(Mobile from)
		{
			if (Parent is Mobile && from == Parent)
			{
				if (IsSetItem && m_SetEquipped)
				{
					SetHelper.RemoveSetBonus(from, SetID, this);
				}
			}

			return base.OnDragLift(from);
		}

		public virtual SetItem SetID { get { return SetItem.None; } }
		public virtual int Pieces { get { return 0; } }

        public virtual bool BardMasteryBonus
        {
            get
            {
                return (SetID == SetItem.Virtuoso);
            }
        }

        public bool IsSetItem { get { return SetID != SetItem.None; } }

		private int m_SetHue;
		private bool m_SetEquipped;
		private bool m_LastEquipped;

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetHue
		{
			get { return m_SetHue; }
			set
			{
				m_SetHue = value;
				InvalidateProperties();
			}
		}

		public bool SetEquipped { get { return m_SetEquipped; } set { m_SetEquipped = value; } }

		public bool LastEquipped { get { return m_LastEquipped; } set { m_LastEquipped = value; } }

		private AosAttributes m_SetAttributes;
		private AosSkillBonuses m_SetSkillBonuses;
		private int m_SetSelfRepair;
        private int m_SetPhysicalBonus, m_SetFireBonus, m_SetColdBonus, m_SetPoisonBonus, m_SetEnergyBonus;

		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes SetAttributes { get { return m_SetAttributes; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SetSkillBonuses { get { return m_SetSkillBonuses; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetSelfRepair
		{
			get { return m_SetSelfRepair; }
			set
			{
				m_SetSelfRepair = value;
				InvalidateProperties();
			}
		}

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

			if ((prop = m_SetSelfRepair) != 0 && WeaponAttributes.SelfRepair == 0)
			{
				list.Add(1060450, prop.ToString()); // self repair ~1_val~
			}

			SetHelper.GetSetProperties(list, this);
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

    public enum CheckSlayerResult
    {
        None,
        Slayer,
        SuperSlayer,
        Opposition
    }
}
