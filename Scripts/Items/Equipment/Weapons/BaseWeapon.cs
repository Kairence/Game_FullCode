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
        public ConsecratedWeaponContext ConsecratedContext { get; set; }

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

			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				pm.UnEquipCheck();
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
				//세트 아이템 해제 코드
				if( PrefixOption[50] > 0 )
				{
					if( m is PlayerMobile )
					{
						PlayerMobile pm = m as PlayerMobile;
						pm.ItemSetValue[PrefixOption[50]]--;
						Misc.Util.SetOption(pm, false);
					}					
				}
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
				if( m is PlayerMobile )
				{
					PlayerMobile pm = m as PlayerMobile;
					pm.UnEquipCheck();
				}

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
			double speed = Speed;

			if (speed == 0)
			{
				return TimeSpan.FromHours(1.0);
			}

			double delayInSeconds;

			if (Core.SE)
			{
				/*
                * This is likely true for Core.AOS as well... both guides report the same
                * formula, and both are wrong.
                * The old formula left in for AOS for legacy & because we aren't quite 100%
                * Sure that AOS has THIS formula
                */
				int bonus = Math.Min(AosAttributes.GetValue(m, AosAttribute.WeaponSpeed) / 100 
				+ AosWeaponAttributes.GetValue(m, AosWeaponAttribute.MageWeapon) / 100, 25000);
				if( m is BaseCreature )
				{
					BaseCreature bc = m as BaseCreature;
					speed = bc.AttackSpeed;
					if( bc.AttackSpeed == 0 )
						speed = 5.0;
					//bonus = m.Dex / 1000;
				}
				//레슬링 200 보너스
				if( this is Fists && m.Skills[SkillName.Wrestling].Value >= 200 )
					speed /= 2;
				
				/*
				if (bonus > 1000)
				{
					bonus = 1000;
				}
				*/
				double ticks = speed / 0.25;

				if (Core.ML)
				{
					//delayInSeconds = Math.Truncate( ( speed * 10000 / ( 1000 + bonus ) ) ) * 0.1;
					//if( delayInSeconds < 0.5 )
					//	delayInSeconds = 0.5;
					return TimeSpan.FromSeconds(Misc.Util.AttackSpeedTicks(speed, bonus)); 
				}
				else
				{
					speed = Math.Floor(speed * (bonus + 1000.0) / 1000.0);

					if (speed <= 0)
					{
						speed = 1;
					}

					ticks = Math.Floor((80000.0 / ((m.Stam + 100) * speed)) - 2);
				}

				// Swing speed currently capped at one swing every 1.25 seconds (5 ticks).
				if (ticks < 2)
				{
					ticks = 2;
				}


				delayInSeconds = ticks * 0.25;
				/*
				if( m.Region.IsPartOf("Covetous") && ( m is VampireBat || m is Harpy || m is StoneHarpy || m is Succubus ) )
					delayInSeconds = 0.5;
				*/
			}
			else if (Core.AOS)
			{
				int v = (m.Stam + 100) * (int)speed;

				int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

				v += AOS.Scale(v, bonus);

				if (v <= 0)
				{
					v = 1;
				}

				delayInSeconds = Math.Floor(40000.0 / v) * 0.5;

				// Maximum swing rate capped at one swing per second
				// OSI dev said that it has and is supposed to be 1.25
				if (delayInSeconds < 1.25)
				{
					delayInSeconds = 1.25;
				}
			}
			else
			{
				int v = (m.Stam + 100) * (int)speed;

				if (v <= 0)
				{
					v = 1;
				}

				delayInSeconds = 15000.0 / v;
			}

			return TimeSpan.FromSeconds(delayInSeconds);
		}

		//특수기 레벨
		private int WeaponAbilityLevel(Mobile from, bool first)
		{
			BaseWeapon usedWeapon = from.Weapon as BaseWeapon;
			int level = ( first ? ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMFirstBonus ) : ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMSecondBonus ) ) + ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMAllBonus );
			
			if( usedWeapon.Skill is SkillName.Swords )
				level += ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMSwordBonus ); //검		
			else if( usedWeapon.Skill is SkillName.Macing )	
				level += ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMMaceBonus ); //둔기		
			else if( usedWeapon.Skill is SkillName.Fencing )	
				level += ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMFancingBonus ); //펜싱		
			else if( usedWeapon.Skill is BaseRanged )	
				level += ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMBowBonus ); //활&보우		
			else
				level += ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.SPMWrestling ); //맨손	

			return level / 100;
		}

		public virtual void OnBeforeSwing(Mobile attacker, IDamageable damageable)
		{
            Mobile defender = damageable as Mobile;

			WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

			bool first = false;

			if( a != null && a == PrimaryAbility )
				first = true;

            if (a != null && (!a.OnBeforeSwing(attacker, defender, WeaponAbilityLevel(attacker, first))))
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
				if( canSwing )
				{
					/*
					if( attacker is PlayerMobile )
					{
						
						if( attacker.Stam < 1 )
						{
							attacker.SendMessage("당신은 기력이 없어서 무기를 휘두를 힘이 없습니다.");
							canSwing = false;
						}
						else
							attacker.Stam -= 1;
					}
					*/
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

		//데미지 감소
		public virtual int AbsorbDamage(Mobile attacker, Mobile defender, int damage, int target )
		{
			/*
			0 : 방패
			1 : 손
			2 : 머리
			3 : 목
			4 : 어깨
			5 : 하의
			6 : 상의
			*/
			double skillBonus = 0;
			int reducedamage = 0;

			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];


			//방어력 데미지 감소 계산
			if( defender is PlayerMobile )
			{
				Item armorItem = null;
				BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
				BaseWeapon two = defender.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
				BaseWeapon one = defender.FindItemOnLayer(Layer.FirstValid) as BaseWeapon;
				BaseWeapon one_one = defender.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;
				switch(target)
				{
					case 0:
					{
						if( shield != null )
						{
							//적중 시 이펙트
							defender.FixedEffect(0x37B9, 10, 16);
							defender.Animate(AnimationType.Parry, 0);

							damage = shield.OnHit(this, damage);						
							
							reducedamage = (int)( ( shield.ArmorBase + shield.ArmorAttributes.WeaponDefense / 100 ) * defender.Str * 0.0025 ); //+ defender.Skills.Parry.Value );
							
							if( attacker.Alive && defender.Alive )
								defender.CheckSkill( SkillName.Parry, skillUp( attacker, defender, atkSkill.Value ) );
						}						
						break;
					}
					case 1:
					{
						armorItem = defender.HandArmor;
						break;
					}
					case 2:
					{
						armorItem = defender.HeadArmor;
						break;
					}
					case 3:
					{
						armorItem = defender.NeckArmor;
						break;
					}
					case 4:
					{
						armorItem = defender.ArmsArmor;
						break;
					}
					case 5:
					{
						armorItem = defender.LegsArmor;
						break;
					}
					case 6:
					{
						armorItem = defender.ChestArmor;
						break;
					}
				}
				IWearableDurability armor = armorItem as IWearableDurability;

				if (armor != null )
				{
					if( target >= 1 )
					{
						if( armor is BaseArmor)
						{
							reducedamage += (int)( (((BaseArmor)armor).BaseArmorRating + ((BaseArmor)armor).ArmorAttributes.WeaponDefense / 100 ) * defender.Str * 0.0025 );
							damage = armor.OnHit(this, damage);
						}
						else if( armor is BaseClothing )
						{
							reducedamage += (int)( (((BaseClothing)armor).BaseArmorRating + ((BaseClothing)armor).ArmorAttributes.WeaponDefense / 100 ) * defender.Str * 0.0025 );
							damage = armor.OnHit(this, damage);
						}
					}
				}				
			}
			else if(defender is BaseCreature)
			{
				BaseCreature bc = defender as BaseCreature;
				reducedamage = (int)( Math.Min( bc.VirtualArmor * defender.Str * 0.00025, 2500 ));
			}
			damage -= reducedamage;
			if( damage < 0 )
				damage = 0;

			return damage;
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

		int armorignoredamage = 0;
		bool ignoreArmor = false;
		double tacticsBonus = 0.0;
		
		#region 특수기 데미지 설정
		/*
		
		private int PierceDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterPierce); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonancePierce) * 0.1 - AosArmorAttributes.GetValue(defender, AosArmorAttribute.PierceResist );

			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				percent_damage += pm.SilverPoint[8] * 10;
			}
			else if( attacker is BaseCreature )
			{
				percent_damage += attacker.Int * 0.1;
			}
			BaseShield defender_shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

			if( defender_shield != null && defender_shield is Buckler )
				percent_damage /= 2;

			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
		}
		private int ShockDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterKinetic); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonanceKinetic) * 0.1 - AosArmorAttributes.GetValue(defender, AosArmorAttribute.ShockResist );
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				percent_damage += pm.SilverPoint[9] * 10;
			}
			else if( attacker is BaseCreature )
			{
				percent_damage += attacker.Int * 0.1;
			}
			
			BaseShield defender_shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

			if( defender_shield != null && defender_shield is MetalShield )
				percent_damage /= 2;
			
			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
		}
		private int BleedDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterBleed); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonanceBleed) * 0.1 - AosArmorAttributes.GetValue(defender, AosArmorAttribute.BleedResist );
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				percent_damage += pm.SilverPoint[10] * 10;
			}
			else if( attacker is BaseCreature )
			{
				percent_damage += attacker.Int * 0.1;
			}
			
			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
		}
		
		private int FireDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterPoison); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonancePoison) * 0.1 + SAAbsorptionAttributes.GetValue(defender, AosArmorAttribute.InfectionBonus ) * 0.1;
			
			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
		}
		private int ColdDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterPoison); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonancePoison) * 0.1 + SAAbsorptionAttributes.GetValue(defender, AosArmorAttribute.InfectionBonus ) * 0.1;
			
			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
		}
		*/
		private int PoisonDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterPoison); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonancePoison) * 0.1 + ExtendedWeaponAttributes.GetValue(defender, ExtendedWeaponAttribute.InfectionBonus ) * 0.1;
			
			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
		}
		private int LightningDamage(Mobile attacker, Mobile defender, int damage )
		{
			damage += SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.EaterEnergy); 
			double percent_damage = 100 + SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ResonanceEnergy) * 0.1 + ExtendedWeaponAttributes.GetValue(defender, ExtendedWeaponAttribute.LightningBonus ) * 0.1;
			
			if( percent_damage > -100 )
				return (int)(( 1 + percent_damage * 0.01 ) * damage );
			else
				return 0;
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

			//무기술 증가
			if( defender is PlayerMobile )
			{
				BaseCreature bc = attacker as BaseCreature;
				if( attacker is BaseCreature && bc.ControlMaster == null )
				{
					double point = defSkill.Value * 5;
					if( point > 0 )
					{
						attacker.CheckSkill(atkSkill.SkillName, point);
						//attacker.CheckSkill(SkillName.Anatomy, point);
					}
				}
			}
			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				if( bc.ControlMaster == null )
				{
					double point = defSkill.Value + bc.BardingDifficulty;
					if( point > 0 )
						attacker.CheckSkill(atkSkill.SkillName, point);
				}
			}
			
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

			int bonus_damage = 0;
			//전투 체크
			if( attacker is PlayerMobile )
			{
				PlayerMobile apm = attacker as PlayerMobile;

				if( apm.twoMacingBonus >= 1 )
				{
					apm.twoMacingBonus = 0;
					bonus_damage = 1;
				}
				if( defender is PlayerMobile )
				{
					PlayerMobile dpm = defender as PlayerMobile;
					if( apm.TimerList[65] < 300 )
						apm.TimerList[65] = 300;
					if( dpm.TimerList[65] < 300 )
						dpm.TimerList[65] = 300;
				}
			}
			if( attacker is BaseCreature )
			{
				if( defender is PlayerMobile )
				{
					PlayerMobile dpm = defender as PlayerMobile;
					if( dpm.TimerList[64] < 60 )
						dpm.TimerList[64] = 60;
				}
			}
			
			int max = (int)( Math.Max((int)ScaleDamageAOS(attacker, MaxDamage, false), 1) ); //무기 최소뎀
			int min = (int)( Math.Max((int)ScaleDamageAOS(attacker, MinDamage, false), 1) ); //무기 최대뎀
			BaseWeapon one = null;
			BaseWeapon two = null;
			if( attacker is PlayerMobile )
			{
				one = attacker.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;
				two = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
			}
			if( bonus_damage == 0 )
			{
				//무기술, 명중률, 방어율
				int ac = Math.Min(AosAttributes.GetValue(attacker, AosAttribute.AttackChance), 500000 ); //명중률 증가 옵션
				int dc = Math.Min(AosAttributes.GetValue(defender, AosAttribute.DefendChance), 500000 ); //방어율 증가 옵션


				if( attacker is BaseCreature )
				{
					BaseCreature c = attacker as BaseCreature;
					max = c.DamageMax;
					min = c.DamageMin;
					ac = c.Str * 10;
					//chance_dice = atkSkill.Value + attacker.Str * 0.1 - defSkill.Value - dc * 0.1;
				}
				if( defender is BaseCreature )
				{
					BaseCreature d = defender as BaseCreature;
					dc = d.Dex * 10;
					
				}
				bonus_damage = max - min;

				double chance_dice = Utility.RandomDouble() * 200 + ac * 0.0001 - dc * 0.0001 - 100;

				//무기 데미지 결정 150, 200 보너스
				if( one != null )
				{
					if( atkWeapon.Skill is SkillName.Swords && attacker.Skills[SkillName.Swords].Value >= 200 && chance_dice < 50 )
						chance_dice = 50;
					else if( atkWeapon.Skill is SkillName.Macing && attacker.Skills[SkillName.Macing].Value >= 200 && chance_dice < 100 && Utility.RandomDouble() < 0.2 )
						chance_dice = 100;
				}
				else
				{
					if( chance_dice > 100 )
						chance_dice = 100;
					else if( chance_dice < -100 )
						chance_dice = -100;
				}
				if( chance_dice != 0 )
				{
					bonus_damage = (int)( chance_dice * bonus_damage );
					bonus_damage /= 100;
				}			
				damage += bonus_damage;				
				if( damage > max )
					damage = max;
				else if( damage < min )
					damage = min;
			}
			else
				damage = max;

            bool ranged = this is BaseRanged;
            int phys, fire, cold, pois, nrgy, chaos, direct;

            if ( SkillMasterySpell.HasSpell<ShieldBashSpell>(attacker))
            {
                phys = 100;
                fire = cold = pois = nrgy = chaos = direct = 0;
            }
            else
            {
                GetDamageTypes(attacker, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

                if (!OnslaughtSpell.HasOnslaught(attacker, defender) &&
                    ConsecratedContext != null &&
                    ConsecratedContext.Owner == attacker &&
                    ConsecratedContext.ConsecrateProcChance >= Utility.Random(100))
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

            bool acidicTarget = MaxRange <= 1 && !(this is Fists) && (defender is Slime );

			m_HiddenRank += 40;
            if (m_HiddenRank >= 1000)
            {
				m_HiddenRank -= 1000;
                if (MaxRange <= 1 && acidicTarget)
                {
                    attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500263); // *Acid blood scars your weapon!*
                }
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
			
			#region Critical Setting
			bool criticalUse = false;
			double criticalPercent = 0.0;
			double criticalDamage = 1.5;
			//스텟 치명 계산
			if( attacker is PlayerMobile )
				criticalPercent += attacker.Luck * 0.0001;
			else if( attacker is BaseCreature )
			{
				BaseCreature bc = attacker as BaseCreature;
				criticalPercent += bc.Dex * 0.00001;
				criticalDamage += bc.Stam * 0.00001 + Misc.Util.MonsterTierCriticalDamage(bc);
			}				

			//장비 치명 계산
			criticalDamage += Math.Min( AosAttributes.GetValue(attacker, AosAttribute.Brittle), 300000 ) * 0.000001;
			criticalPercent += Math.Min( AosAttributes.GetValue(attacker, AosAttribute.WeaponCritical), 500000 ) * 0.000001;
			
			//적 장비 치명 계산
			criticalDamage -= AosWeaponAttributes.GetValue(defender, AosWeaponAttribute.BattleLust ) * 0.000001;
			criticalPercent -= AosWeaponAttributes.GetValue(defender, AosWeaponAttribute.BloodDrinker ) * 0.000001;
			//방패 & 옷 치명 계산
			int armorNumber = 6;
			bool parry = false;
			if( defender is PlayerMobile )
			{
				BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
				if( shield != null )
				{
					double parryChance = defender.Skills[SkillName.Parry].Value * 0.001
					+ AosArmorAttributes.GetValue(defender, AosArmorAttribute.ShieldRecovery) * 0.000001;
					if( defender.Skills[SkillName.Parry].Value >= 100 )
						parryChance += 0.05;
					if( parryChance >= Utility.RandomDouble() )
					{
						armorNumber = 0;
						parry = true;
					}
				}
				if( !parry )
				{
					double randomDice = Utility.RandomDouble();
					if (randomDice < 0.1)
					{
						criticalDamage += 0.1;
						criticalPercent += 0.2;
						armorNumber = 1;
					}
					else if (randomDice < 0.2)
					{
						armorNumber = 2;
						criticalPercent += 0.5;
						criticalDamage += 1.0;
					}
					else if (randomDice < 0.25)
					{
						armorNumber = 3;
						criticalPercent += 0.9;
						criticalDamage += 0.85;
					}
					else if (randomDice < 0.40)
					{
						armorNumber = 4;
						criticalDamage += 0.1;
						criticalPercent += 0.25;
					}
					else if (randomDice < 0.65)
					{
						armorNumber = 5;
						criticalPercent += 0.05;
						criticalDamage += 0.1;
					}
					else
					{
						criticalPercent += 0.1;
						criticalDamage += 0.1;
					}					
				}
			}
			
			//무기(맨손) 치명 확률 계산
			if( attacker.Skills[atkWeapon.Skill].Value >= 150 )
				criticalPercent += 0.1;

			//도끼 치명 보너스
			if (atkWeapon.Type != WeaponType.Axe)
				criticalPercent = attacker.Skills[SkillName.Lumberjacking].Value * 0.0005;
			
			//스킬 치명 피해 보너스
			criticalDamage += attacker.Skills[atkWeapon.Skill].Value * 0.001;
		
			int twoWeaponBonusNumber = 0;
			// 0 : 없음
			//1 : 활 보너스
			//2 : 석궁 보너스
			//3 : 양손 검 보너스
			//4 : 양손 둔기 보너스
			//5 : 양손 펜싱 보너스
			
			
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				if( two != null && attacker.Skills[atkWeapon.Skill].Value >= 200 )
				{
					if( atkWeapon.Skill is SkillName.Swords )
						twoWeaponBonusNumber = 3;
					else if( atkWeapon.Skill is SkillName.Macing )
						twoWeaponBonusNumber = 4;
					else if( atkWeapon.Skill is SkillName.Fencing )
						twoWeaponBonusNumber = 5;
					else
					{
						BaseRanged br = atkWeapon as BaseRanged;
						if( br.AmmoType == typeof( Bolt ) )
							twoWeaponBonusNumber = 2;
						else 
							twoWeaponBonusNumber = 1;
					}
				}
				//활 치명 확률 보너스
				criticalPercent += pm.arrowCriticalBonus * 0.1;
				//석궁 치명 피해 보너스
				criticalDamage += pm.boltCriticalBonus * 0.05;
				if( criticalPercent < 0.25 && twoWeaponBonusNumber == 3 )
					criticalPercent = 0.25;
				//양손 펜싱 치명 확률 보너스
				if( pm.twoFencingBonus == 1 && twoWeaponBonusNumber == 5 )
				{
					pm.twoFencingBonus = 0;
					criticalPercent = 1;
				}
			}
			
			//치명타 반사신경 보너스
			if( defender.Skills.Tracking.Value >= 150 )
				criticalDamage -= 0.25;
			if( defender.Skills.Tracking.Value >= 100 )
				criticalPercent -= 0.1 + defender.Dex * 0.0001;
			if( defender.Skills.Tracking.Value > 0 )
				criticalDamage -= 0.005 * defender.Skills.Tracking.Value;			
			
			
			//전투 포인트 보너스
			//if( attacker is PlayerMobile )
			//{
			//	PlayerMobile pm = attacker as PlayerMobile;
			//	criticalDamage += pm.SilverPoint[19] * 0.025;
			//	criticalPercent += pm.SilverPoint[17] * 0.005;
			//	if( pm.Region.Name == "Wrong" )
			//		criticalPercent -= 1;
			//}
			
			
			//치명 데미지 및 전체 데미지 합산
			if( criticalPercent > Utility.RandomDouble() )
			{
				if( attacker is PlayerMobile )
				{
					PlayerMobile pm = attacker as PlayerMobile;
					pm.arrowCriticalBonus = 0;
					if( twoWeaponBonusNumber == 2 && pm.boltCriticalBonus < 10 )
						pm.boltCriticalBonus++;
					else if( twoWeaponBonusNumber == 4 )
						pm.twoMacingBonus = 1;
					else if( pm.twoFencingBonus == 0 && twoWeaponBonusNumber == 5 )
						pm.twoFencingBonus = 1;
					
				}
				//치명타 이펙트
				int itemID, soundID;

				switch ( atkWeapon.Skill )
				{
					case SkillName.Macing:
						itemID = 0xFB4;
						soundID = 0x232;
						break;
					case SkillName.Archery:
						itemID = 0x13B1;
						soundID = 0x145;
						break;
					default:
						itemID = 0xF5F;
						soundID = 0x56;
						break;
				}

				attacker.PlaySound(0x20C);
				attacker.PlaySound(soundID);
				attacker.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);

				IEntity from = new Entity(Serial.Zero, new Point3D(attacker.X, attacker.Y, attacker.Z), attacker.Map);
				IEntity to = new Entity(Serial.Zero, new Point3D(attacker.X, attacker.Y, attacker.Z + 50), attacker.Map);
				Effects.SendMovingParticles(from, to, itemID, 1, 0, false, false, 33, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
				if( criticalDamage < 0.5 )
					criticalDamage = 0.5;
				criticalDamage += 1;
				damage = (int)( damage * criticalDamage );
				
			}
			else
			{
				if( attacker is PlayerMobile )
				{
					PlayerMobile pm = attacker as PlayerMobile;
					if( twoWeaponBonusNumber == 1 && pm.arrowCriticalBonus < 10 )
						pm.arrowCriticalBonus++;
					if( twoWeaponBonusNumber == 2 )
						pm.boltCriticalBonus = 0;
				}
			}
				
			#endregion
			
			//슬레이어 데미지
			damage = (int)( damage * Misc.Util.GetSlayerDamageScalar(attacker, defender) );

			//데미지 감소 및 내구도 하락
            damage = AbsorbDamage(attacker, defender, damage, armorNumber);

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

			bool first = false;
            WeaponAbility primary = null;
            WeaponAbility secondard = null;
            SpecialMove move = SpecialMove.GetCurrentMove(attacker);
			WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
			if( a is ArmorIgnore || a is ArmorPierce )
				FuryCheck = true;
			int weaponLevel = 0;
			double weaponBonus = attacker.Skills[SkillName.Tactics].Value;
			//특수기 공격
			if( a != null && a == this.SecondaryAbility )
				weaponBonus -= 100;
			if( attacker is BaseCreature )
			{
				bool dice_SPM = (attacker.Stam * 0.001 > Utility.RandomDouble() );
				if( dice_SPM )
				{
					BaseCreature bc = attacker as BaseCreature;
					
					if( bc is Turkey || bc is Mongbat || bc is Eagle || bc is GiantTurkey || bc is Bird || bc is Chicken || bc is Ferret || bc is Lizardman || bc is LizardmanDefender) //방어구 무시 
					{
						//int pierce_damage = PierceDamage(attacker, defender, 100);
						//WeaponAbility.ArmorIgnore.OnHit( attacker, defender, pierce_damage);
					}
					
					else if( bc is Skree || bc is PolarBear || bc is Savage || bc is Llama) //뇌진탕 일격
					{
						//int shock_damage = ShockDamage(attacker, defender, Utility.RandomMinMax( 40, 160 ));
						//WeaponAbility.ConcussionBlow.OnHit( attacker, defender, shock_damage);
					}
					else if( bc is WolfSpider || bc is GiantSpider || bc is TrapdoorSpider || bc is GiantBlackWidow || bc is DreadSpider || bc is GiantDreadSpider || bc is BullFrog || bc is GiantSerpent || bc is Rat || bc is Slime || bc is Snake || bc is PestilentBandage || bc is RottingCorpse || bc is ClockworkScorpion || bc is Scorpion ) //독 바르기
					{
						//int poison_damage = PoisonDamage(attacker, defender, Utility.RandomMinMax( 80, 120 ));
						//WeaponAbility.InfectiousStrike.OnHit( attacker, defender, poison_damage);
					}
					else if( bc is VampireBat || bc is Alligator || bc is Cat || bc is Cougar || bc is DireWolf || bc is Dog || bc is GreyWolf || bc is SnowLeopard || bc is TimberWolf || bc is WhiteWolf || bc is PatchworkSkeleton || bc is SkeletalLich ) //흡혈 공격
					{
						//int bleed_damage = BleedDamage(attacker, defender, Utility.RandomMinMax( 20, 60 ));
						//WeaponAbility.BleedAttack.BeforeAttack( attacker, defender, bleed_damage);
					}
					else if( bc is Harpy || bc is StoneHarpy || bc is Succubus || bc is Skeleton || bc is Zombie || bc is Spectre || bc is Boar || bc is Bull || bc is Cow || bc is Goat || bc is GreatHart || bc is Hind || bc is MountainGoat || bc is Panther || bc is Pig || bc is Rabbit || bc is Sheep || bc is Bogling || bc is Crane || bc is Treefellow || bc is HeadlessOne || bc is Troll || bc is Ogre || bc is Mummy || bc is Lich || bc is LichLord || bc is Beholder || bc is Orc || bc is OrcCaptain || bc is OrcishLord || bc is Titan ) //마비 공격
					{
						//int shock_damage = ShockDamage(attacker, defender, Utility.RandomMinMax( 120, 360 ));
						//WeaponAbility.ParalyzingBlow.BeforeAttack( attacker, defender, shock_damage);
					}
					else if( bc is PlagueBeast || bc is SavageRider || bc is Walrus || bc is EttinLord || bc is Corpser || bc is Reaper || bc is Ettin || bc is Centaur ) //이중 일격
					{
						WeaponAbility.DoubleStrike.OnHit( attacker, defender, damage);
					}
					else if ( bc is SandVortex || bc is SwampTentacle || bc is SkeletalMount || bc is BoneKnight || bc is SkeletalKnight || bc is OrcChopper ) //소용돌이 일격
					{
						WeaponAbility.WhirlwindAttack.BeforeAttack( attacker, defender, damage);
					}
					else if( bc is Ghoul || bc is OrcScout ) //그림자 일격
					{
						//int pierce_damage = PierceDamage(attacker, defender, 240);
						//WeaponAbility.ShadowStrike.OnHit( attacker, defender, pierce_damage);
					}
				}
			}
			else if( attacker is PlayerMobile && a != null )
			{
				a.OnHit(attacker, defender, damage, weaponLevel, weaponBonus);
				WeaponAbility.ClearCurrentAbility(attacker);
			}
            if (defender == null)
            {
                AOS.Damage(damageable, attacker, damage, FuryCheck, phys, fire, cold, pois, nrgy, chaos, direct, false, ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

                // TODO: WeaponAbility/SpecialMove OnHit(...) convert target to IDamageable
                // Figure out which specials work on items. For now AI only.
				/*
				if (ignoreArmor)
                {
					attacker.SendLocalizedMessage(1060076); // Your attack penetrates their armor!
					defender.SendLocalizedMessage(1060077); // The blow penetrated your armor!

					defender.PlaySound(0x56);
					defender.FixedParticles(0x3728, 200, 25, 9942, EffectLayer.Waist);

					Effects.PlaySound(damageable.Location, damageable.Map, 0x56);
                    Effects.SendTargetParticles(damageable, 0x3728, 200, 25, 0, 0, 9942, EffectLayer.Waist, 0);
                }
				*/
                //WeaponAbility.ClearCurrentAbility(attacker);
                SpecialMove.ClearCurrentMove(attacker);
                if (AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) > 0)
                {
                    attacker.SendLocalizedMessage(1152566); // You fail to leech life from your target!
                }

                return;
            }
			
			/*
			//패시브 특수기 설정
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				if( pm.FuryActive )
				{
					BaseWeapon two = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
					if( atkWeapon.Skill is SkillName.Archery )
					{
						//spell.OnBeforeDamage(attacker, defender);
						double anatomy = defender.Skills.Anatomy.Value * 1.2;
						if( anatomy >= 100 )
							anatomy += 16;
						
						damage *= 140 + (int)anatomy;
						damage /= 100;
						Server.Spells.Bushido.MomentumStrike spell = new Server.Spells.Bushido.MomentumStrike();
						spell.OnHit(attacker, defender, damage);
						pm.FuryActive = false;
					}
					else
					{
						defender.Freeze(TimeSpan.FromSeconds( 6.0 ) );
						pm.FuryActive = false;
					}
				}
			}
			*/

			
			SpecialAttack = 0;

			/*
			if( defender is PlayerMobile )
			{
				PlayerMobile pm = defender as PlayerMobile;
				if( pm.Hunger > 0 )
				{
					damage -= pm.ArtifactPoint[10] * 2;
				}
			}
			*/
			
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
				if (ignoreArmor)
                {
					attacker.SendLocalizedMessage(1060076); // Your attack penetrates their armor!
					defender.SendLocalizedMessage(1060077); // The blow penetrated your armor!

					defender.PlaySound(0x56);
					defender.FixedParticles(0x3728, 200, 25, 9942, EffectLayer.Waist);

					Effects.PlaySound(damageable.Location, damageable.Map, 0x56);
                    Effects.SendTargetParticles(damageable, 0x3728, 200, 25, 0, 0, 9942, EffectLayer.Waist, 0);
                }

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

				//DualWield.DoHit(attacker, defender, damage);

				/*
				if (sparks)
				{
					int mana = attacker.Mana + damageGiven;
					if (!defender.Player) mana *= 2;
					attacker.Mana = Math.Min(attacker.ManaMax, attacker.Mana + mana);
				}
				*/
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

				/*
				if (a != null)
				{
					a.OnHit(attacker, defender, damage);
				}

				if (move != null)
				{
					move.OnHit(attacker, defender, damage);
				}
				if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
				{
					((IHonorTarget)defender).ReceivedHonorContext.OnTargetHit(attacker);
				}
				*/

				/*
				if (!ranged)
				{
					if (AnimalForm.UnderTransformation(attacker, typeof(GiantSerpent)))
					{
						defender.ApplyPoison(attacker, Poison.Lesser);
					}

					if (AnimalForm.UnderTransformation(defender, typeof(BullFrog)))
					{
						attacker.ApplyPoison(defender, Poison.Regular);
					}
				}

				BaseFamiliar.OnHit(attacker, damageable);
				WhiteTigerFormSpell.OnHit(attacker, defender);
				*/			
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

            Server.Spells.Fourth.CurseSpell.AddEffect(defender, duration, 10, 10, 10);
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

		/*
		1 = 한손 검(Built_In)
		2 = 양손 검(Built_In)
		3 = 도끼(Built_In)
		4 = 한손 둔기(Built_In)
		5 = 양손 둔기(Addon)
		6 = 한손 펜싱(Built_In)
		7 = 양손 펜싱(Addon)
		8 = 활(Addon)
		9 = 석궁(Addon)
		
		
		*/
		private int SpecialAttack = 0;
		
		public virtual void OnStun(Mobile attacker, IDamageable damageable)
		{
			//스턴 스킬 정의
            Mobile defender = damageable as Mobile;
		
            if(defender != null)
			    defender.PlaySound(GetMissDefendSound(attacker, defender));
			

			double stunPlus = attacker.Str * 0.001 - defender.Dex * 0.001;
			/*
			if( defender.Combatant == null || defender.Combatant != attacker )
				stunPlus += 0.5;
			double stunMul = 1.00;
			*/

			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			/*
			//곤봉술 스턴 증가 보너스
			if( atkWeapon.Skill is SkillName.Macing )
			{
				double skillBonus = attacker.Skills.Macing.Value * 0.01;
				if( skillBonus >= 100 )
					skillBonus += 0.3;
				stunMul += skillBonus;
			}
			
			//검술 스턴 감소 보너스
			if( defWeapon.Skill is SkillName.Swords )
			{
				stunPlus -= defender.Skills.Swords.Value * 0.01;
				if( defender.Skills.Swords.Value >= 100 )
					stunPlus -= 0.3;
			}

			//레슬링 스턴 증가 보너스
			if( atkWeapon.Skill is SkillName.Wrestling )
			{
				stunPlus += defender.Skills.Wrestling.Value * 0.01;
				if( defender.Skills.Wrestling.Value >= 100 )
					stunPlus += 0.3;
			}
			//레슬링 스턴 방어 보너스
			if( defWeapon.Skill is SkillName.Wrestling )
			{
				double skillBonus = attacker.Skills.Macing.Value * 0.01;
				if( skillBonus >= 100 )
					skillBonus += 0.3;
				stunMul -= skillBonus;
			}
			//방어자 방패 확인
			BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
			if( shield != null )
			{
				double skillBonus = defender.Skills.Parry.Value * 0.001;
				if( defender.Skills.Parry.Value >= 100 )
					skillBonus += 0.03;
				stunPlus -= skillBonus;
			}
			//스턴 결정
			if( stunPlus > 0 && stunMul > 0 )
			{
				BaseWeapon two = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
				BaseWeapon one = attacker.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;
				//개별 무기 설정 특수 설정
				if( atkWeapon.Skill is SkillName.Swords )
				{
					if ( one != null )
					{
						SpecialAttack = 1;
					}
					else if ( two != null )
					{
						if( atkWeapon is BaseAxe )
						{
							SpecialAttack = 3;
						}
						else
						{
							SpecialAttack = 2;
						}
					}
				}
				else if( atkWeapon.Skill is SkillName.Macing )
				{
					if ( one != null)
					{
						SpecialAttack = 4;
					}
					else
						SpecialAttack = 5;
				}
				else if( atkWeapon.Skill is SkillName.Fencing )
				{
					if ( one != null)
					{
						SpecialAttack = 6;
					}
					else
						SpecialAttack = 7;
				}
				else if( atkWeapon.Skill is SkillName.Archery )
				{
					if ( two != null && atkWeapon is BaseRanged )
					{
						BaseRanged br = atkWeapon as BaseRanged;
						if( br.AmmoType == typeof( Bolt ) )
							SpecialAttack = 9;
						else
							SpecialAttack = 8;
					}
				}
				if( attacker is PlayerMobile )
				{
					stunPlus *= (double)atkWeapon.Speed * 0.2;
				}
				defender.Freeze( TimeSpan.FromSeconds( stunPlus * stunMul ) );
			}
			*/
		}		
		
		public virtual void OnMiss(Mobile attacker, IDamageable damageable)
		{
            Mobile defender = damageable as Mobile;

			PlaySwingAnimation(attacker);
			attacker.PlaySound(GetMissAttackSound(attacker, defender));

            if(defender != null)
			    defender.PlaySound(GetMissDefendSound(attacker, defender));

			/*
			WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

			if (ability != null)
			{
				ability.OnMiss(attacker, defender);
			}

			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

			if (move != null)
			{
				move.OnMiss(attacker, defender);
			}
			*/
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

			int oreType;

			switch (m_Resource)
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
				case CraftResource.SernedLeather: oreType = 1051903; break; // 경화 가죽				case CraftResource.SpinedLeather:
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

					#region Mondain's Legacy
				case CraftResource.OakWood:
					oreType = 1072533;
					break; // oak
				case CraftResource.AshWood:
					oreType = 1072534;
					break; // ash
				case CraftResource.YewWood:
					oreType = 1072535;
					break; // yew
				case CraftResource.Heartwood:
					oreType = 1072536;
					break; // heartwood
				case CraftResource.Bloodwood:
					oreType = 1072538;
					break; // bloodwood
				case CraftResource.Frostwood:
					oreType = 1072539;
					break; // frostwood
					#endregion

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
				
				//list.Add(1053099, Util.ItemRank( (int)ItemPower), "", GetNameString());
				
				/*
				if( (int)ItemPower == 4 )
					list.Add(1053099, "<basefont color=#B36BFF>{0}\t{1}<basefont color=#FFFFFF>", "", GetNameString());
				else if( (int)ItemPower == 5 )
					list.Add(1053099, "<basefont color=#FF0090>{0}\t{1}<basefont color=#FFFFFF>", "", GetNameString());
				else if( (int)ItemPower == 6 )
					list.Add(1053099, "<basefont color=#FF7800>{0}\t{1}<basefont color=#FFFFFF>", "", GetNameString());
				else if( (int)ItemPower == 7 )
					list.Add(1053099, "<basefont color=#FFB400>{0}\t{1}<basefont color=#FFFFFF>", "", GetNameString());
				else if( (int)ItemPower == 8 )
					list.Add(1053099, "<basefont color=#DC143C>{0}\t{1}<basefont color=#FFFFFF>", "", GetNameString());
				else
					list.Add(LabelNumber);
				*/
            }
            else
            {
				list.Add(Name);
            }
			
			
			/*
			else if (oreType != 0)
			{
				list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
            }
            #region High Seas
            else if (SearingWeapon)
            {
                list.Add(1151318, String.Format("#{0}", LabelNumber));
            }
            #endregion
			*/

			/*
            * Want to move this to the engraving tool, let the non-harmful
            * formatting show, and remove CLILOCs embedded: more like OSI
            * did with the books that had markup, etc.
            *
            * This will have a negative effect on a few event things imgame
            * as is.
            *
            * If we cant find a more OSI-ish way to clean it up, we can
            * easily put this back, and use it in the deserialize
            * method and engraving tool, to make it perm cleaned up.
            */

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
            int prop = PrefixOption[99] + 1;
            double fprop;

            if (m_Poison != null && m_PoisonCharges > 0 && CanShowPoisonCharges())
			{
				#region Mondain's Legacy mod
				list.Add(m_Poison.LabelNumber, m_PoisonCharges.ToString());
				#endregion
			}

			//1060659 ~ 1060664 자율 코드 사용 가능
			
			//if( !Identified )
				//list.Add( 1060659, "<basefont color=#FF0000>아이템 감정\t안됨<basefont color=#FFFFFF>" );
			
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
			int phys, fire, cold, pois, nrgy, chaos, direct;

			GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
			//기본 옵션(충격 등)
			
			#region Mondain's Legacy
			if (chaos != 0)
			{
				list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%
			}

			if (direct != 0)
			{
				list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%
			}
			#endregion

			if (phys != 0)
			{
				list.Add(1061048, phys.ToString()); // physical damage ~1_val~%
			}

			if (fire != 0)
			{
				list.Add(1061049, fire.ToString()); // fire damage ~1_val~%
			}

			if (cold != 0)
			{
				list.Add(1061050, cold.ToString()); // cold damage ~1_val~%
			}

			if (pois != 0)
			{
				list.Add(1061051, pois.ToString()); // poison damage ~1_val~%
			}

			if (nrgy != 0)
			{
				list.Add(1061052, nrgy.ToString()); // energy damage ~1_val
			}

			if (Core.ML && chaos != 0)
			{
				list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%
			}

			if (Core.ML && direct != 0)
			{
				list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%
			}			
			
			if( this is Broadsword || this is PaladinSword || this is Bardiche || this is Axe || this is OrnateAxe || this is Scepter || this is Lance || this is Crossbow )
			{
				list.Add( 1063598, "50" ); //충격 데미지 50%
			}
			else if( this is Mace || this is BaseWand || this is WarHammer )
			{
				list.Add( 1063598, "100" ); //충격 데미지 100%
			}		
			else if( this is Cutlass || this is ThinLongsword || this is CrescentBlade || this is ExecutionersAxe || this is TwoHandedAxe || this is WarAxe || this is Dagger || this is Spear || this is CompositeBow)
			{
				list.Add( 1063597, "50" ); //관통 데미지 50%
			}
			else if( this is Kryss || this is ShortSpear )
			{
				list.Add( 1063597, "100" ); //관통 데미지 100%
			}
			else if( this is GnarledStaff )
			{
				list.Add( 1063600, "50" ); //주문 피해 50%
			}
			else if( this is ElvenSpellblade )
			{
				list.Add( 1063635, "0.8" ); //마나 회복 0.8
			}
			else if ( this is MagicalShortbow )
			{
				list.Add( 1063634, "50" ); //에너지 데미지 50%
			}

			list.Add(1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~
			
			/*
			if ((prop = GetLowerStatReq()) != 0)
			{
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%
			}
			*/
			if (Core.ML)
			{
				list.Add(1061167, String.Format("{0}s", Speed)); // weapon speed ~1_val~
			}
			else
			{
				list.Add(1061167, Speed.ToString());
			}

			if (MaxRange > 1)
			{
				list.Add(1061169, MaxRange.ToString()); // range ~1_val~
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
			if (Layer == Layer.TwoHanded)
			{
				list.Add(1061171); // two-handed weapon
			}
			else
			{
				list.Add(1061824); // one-handed weapon
			}

			XmlAttach.AddAttachmentProperties(this, list);

			if (m_Hits >= 0 && m_MaxHits > 0)
			{
				list.Add(1060639, "{0}\t{1}", m_Hits, m_MaxHits); // durability ~1_val~ / ~2_val~
			}

			if (Core.SE)
			{
				switch (Skill)
				{
					case SkillName.Swords:
						list.Add(1061172);
						break; // skill required: swordsmanship
					case SkillName.Macing:
						list.Add(1061173);
						break; // skill required: mace fighting
					case SkillName.Fencing:
						list.Add(1061174);
						break; // skill required: fencing
					case SkillName.Archery:
						list.Add(1061175);
						break; // skill required: archery
					case SkillName.Throwing:
						list.Add(1112075); // skill required: throwing
						break;
				}
			}			
			
			if( PrefixOption[0] >= 100 )
			{
				//신규 옵션 정리
				bool skillcheck = false;
				int skilluse = 5;
				int skillname = 0;
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
				if (ArtifactRarity > 0)
				{
					list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
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
				
				//강화 옵션
				if( PrefixOption[3] + PrefixOption[4] + PrefixOption[5] + PrefixOption[6] + PrefixOption[7] != 0 )
				{
					list.Add(1083001);
					
					for(int i = 0; i < 7; ++i)
					{
						if( PrefixOption[3 + i] > 0 )
						{
							list.Add( 1083002 + i, "{0}\t{1}", PrefixOption[i + 3], (((double)SuffixOption[i + 3])*Misc.Util.PercentCalc(PrefixOption[3 + i])).ToString() );
						}
					}
				}
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
						case CraftResource.Iron:
						{
							list.Add(1063530, "{0}\t{1}\t{2}", "10", (5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.Copper:
						{
							list.Add(1063531, "{0}\t{1}\t{2}", "20", (5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;					
						}
						case CraftResource.Bronze:
						{
							list.Add(1063532, "{0}\t{1}\t{2}", "50", (0.5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;					
						}
						case CraftResource.Gold:
						{
							list.Add(1063533, "{0}\t{1}\t{2}", "40", (50 * ( PrefixOption[99] + 1 )).ToString(), (2 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;					
						}
						case CraftResource.Agapite:
						{
							list.Add(1063534, "{0}\t{1}", (2.5 * ( PrefixOption[99] + 1 )).ToString(), (20 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~
							break;					
						}
						case CraftResource.Verite:
						{
							list.Add(1063535, "{0}\t{1}", (12.5 * ( PrefixOption[99] + 1 )).ToString(), (10 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;					
						}
						case CraftResource.Valorite:
						{
							list.Add(1063536, (0.5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;					
						}
						case CraftResource.RegularWood:
						{
							list.Add(1063563, "{0}\t{1}\t{2}", "10", (5 * ( PrefixOption[99] + 1 )).ToString(), (10 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 민첩 증가 ~2_val~ 기력 증가 ~3_val~		
							break;
						}
						case CraftResource.OakWood:
						{
							list.Add(1063564, "{0}\t{1}\t{2}", (0.5 * ( PrefixOption[99] + 1 )).ToString(), "40", (2 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.AshWood:
						{
							list.Add(1063565, "{0}\t{1}\t{2}", "20", (5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.YewWood:
						{
							list.Add(1063566, "{0}\t{1}\t{2}", (5 * ( PrefixOption[99] + 1 )).ToString(), (5 * ( PrefixOption[99] + 1 )).ToString(), (10 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.Heartwood:
						{
							list.Add(1063567, "{0}\t{1}", (0.5 * ( PrefixOption[99] + 1 )).ToString(), (12.5 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~						
							break;
						}
						case CraftResource.Bloodwood:
						{
							list.Add(1063568, "{0}\t{1}\t{2}", "50", (0.5 * ( PrefixOption[99] + 1 )).ToString(), (50 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~			
							break;
						}
						case CraftResource.Frostwood:
						{
							list.Add(1063569, "{0}\t{1}", (2.5 * ( PrefixOption[99] + 1 )).ToString(), (10 * ( PrefixOption[99] + 1 )).ToString()); // 전투 경험치 증가 ~1_val~% 힘 증가 ~2_val~ 민첩 증가 ~3_val~			
							break;
						}
					}
				}
			}
			


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
					double bonus = 1;
					if (Quality == ItemQuality.Exceptional)
					{
						maxValue = 1.0;
						this.MaxHitPoints += 20;
						this.HitPoints += 20;
					}
					
					/*
					if( from.Skills.ArmsLore.Value >= 150 )
					{
						maxValue += 0.1;
						bonus += 1;
					}
					if( from.Skills.ArmsLore.Value >= 200 )
					{
						bonus += 2;
						this.MaxHitPoints += 20;
						this.HitPoints += 20;
					}
					*/
					//int rank = Util.ItemRankMaker( from.Skills[craftSystem.MainSkill].Value );
					int rank = Util.ItemRankMaker( from.Skills.ArmsLore.Value, maxValue, bonus );
					
					//int tier = Util.ItemTierMaker( arms, rank, Misc.Util.ResourceNumberToNumber((int)Resource ), from );
					PlayerMobile pm = from as PlayerMobile;
					//암즈로어 스킬 상승 보너스
					if (Quality == ItemQuality.Exceptional)
						pm.CheckSkill(SkillName.ArmsLore, 1500 + rank * 250);
					else
						pm.CheckSkill(SkillName.ArmsLore, 500 + rank * 250);

					//Util.ItemCreate( this, rank, true, pm, tier );

					bool artifact = false;
					
					if( Resource == CraftResource.Verite || Resource == CraftResource.Bloodwood || Resource == CraftResource.SpinedLeather )
					{
						if (tool is Item && ((Item)tool).Parent is Container)
						{
							Container cntnr = (Container)((Item)tool).Parent;

							if (!cntnr.TryDropItem(from, this, false))
							{
								if(cntnr != from.Backpack)
									from.AddToBackpack(this);
								else
									this.MoveToWorld(from.Location, from.Map);
							}
						}
						artifact = true;
					}
					Util.NewItemCreate(this, rank, pm, artifact );
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
