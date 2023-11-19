using System;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Regions;

namespace Server.Items
{
    public enum RepairSkillType
    {
        Smithing,
        Tailoring,
        Tinkering,
        Carpentry,
        Fletching,
        Masonry,
        Glassblowing,
		Inscribe
    }

    public class RepairDeed : Item
    {
        private RepairSkillType m_Skill;
        private double m_SkillLevel;
        private Mobile m_Crafter;
		private int m_Tier;

        [Constructable]
        public RepairDeed()
            : this(RepairSkillType.Smithing, 100.0, null, true)
        {
        }

        [Constructable]
        public RepairDeed(RepairSkillType skill, double level)
            : this(skill, level, null, true)
        {
        }

        [Constructable]
        public RepairDeed(RepairSkillType skill, double level, bool normalizeLevel)
            : this(skill, level, null, normalizeLevel)
        {
        }

        public RepairDeed(RepairSkillType skill, double level, Mobile crafter)
            : this(skill, level, crafter, true)
        {
        }

        public RepairDeed(RepairSkillType skill, double level, Mobile crafter, bool normalizeLevel)
            : base(7982)
        {
            Stackable = true;
			
			//1116792, 1116793, 1049639 ~ 1049642
			/*
            if (normalizeLevel)
                SkillLevel = (int)(level / 10) * 10;
            else
                SkillLevel = level;
			
			*/
			Tier = Misc.Util.RepairSkillCheck(level);
			ItemID = 7981 + Tier;
			
            m_Skill = skill;
            m_Crafter = crafter;
			if( skill == RepairSkillType.Smithing )
				Hue = 1102;
			else if( skill == RepairSkillType.Tailoring )
				Hue = 1155;
			else if( skill == RepairSkillType.Tinkering )
				Hue = 1109;
			else if( skill == RepairSkillType.Carpentry )
				Hue = 1512;
			else if( skill == RepairSkillType.Fletching )
				Hue = 1425;
			else if( skill == RepairSkillType.Inscribe )
				Hue = 2598;
			else
				Hue = 0x1BC;
            LootType = LootType.Regular;
        }

        public RepairDeed(Serial serial)
            : base(serial)
        {
        }
        
        public override bool DisplayLootType
        {
            get
            {
                return Core.ML;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public RepairSkillType RepairSkill
        {
            get
            {
                return m_Skill;
            }
            set
            {
                m_Skill = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double SkillLevel
        {
            get
            {
                return m_SkillLevel;
            }
            set
            {
                m_SkillLevel = Math.Max(Math.Min(value, 120.0), 0) ;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Tier
        {
            get
            {
                return m_Tier;
            }
            set
            {
                m_Tier = value;
                InvalidateProperties();
            }
        }
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
        public static RepairSkillType GetTypeFor(CraftSystem s)
        {
            for (int i = 0; i < RepairSkillInfo.Table.Length; i++)
            {
                if (RepairSkillInfo.Table[i].System == s)
                    return (RepairSkillType)i;
            }

            return RepairSkillType.Smithing;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            //list.Add(1061133, String.Format("{0}\t{1}", GetSkillTitle(m_SkillLevel).ToString(), RepairSkillInfo.GetInfo(m_Skill).Name)); // A repair service contract from ~1_SKILL_TITLE~ ~2_SKILL_NAME~.
			//1116792, 1116793, 1049639 ~ 1049642
			if( Amount <= 1 )
				list.Add(1061133, String.Format("{0}\t{1}", GetSkillTitle(m_Tier).ToString(), RepairSkillInfo.GetInfo(m_Skill).Name)); // A repair service contract from ~1_SKILL_TITLE~ ~2_SKILL_NAME~.
			else
				list.Add(1116800, String.Format("{0}\t{1}\t{2}", GetSkillTitle(m_Tier).ToString(), RepairSkillInfo.GetInfo(m_Skill).Name, Amount.ToString())); // A repair service contract from ~1_SKILL_TITLE~ ~2_SKILL_NAME~.
			/*
			if(Tier != 0 )
			{
				if( Tier <= 2 )
				{
					list.Add( 1116791 + Tier, RepairSkillInfo.GetInfo(m_Skill).Name );
				}
				else
					list.Add( 1049636 + Tier, RepairSkillInfo.GetInfo(m_Skill).Name );
			}
			*/
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            if (m_Crafter != null)
                list.Add(1050043, m_Crafter.TitleName); // crafted by ~1_NAME~

            list.Add(1060636); // exceptional

            base.AddWeightProperty(list);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
			if(Tier != 0 )
			{
				if( Tier <= 2 )
				{
					list.Add( 1116791 + Tier );
				}
				else
					list.Add( 1049636 + Tier );
			}            
			/*
            list.Add(1071345, String.Format("{0:F1}", m_SkillLevel)); // Skill: ~1_val~
            var desc = RepairSkillInfo.GetInfo(m_Skill).Description;

            if (desc != null)
            {
                list.Add(desc.ToString());
            }
			*/
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Check(from))
				Repair.Do(from, RepairSkillInfo.GetInfo(m_Skill).System, this);
        }

        public bool Check(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
                from.SendLocalizedMessage(1047012); // The contract must be in your backpack to use it.
            //else if (!VerifyRegion(from))
            //    TextDefinition.SendMessageTo(from, RepairSkillInfo.GetInfo(m_Skill).NotNearbyMessage);
            else
                return true;

            return false;
        }

        public bool VerifyRegion(Mobile m)
        {
			return true;
            //TODO: When the entire region system data is in, convert to that instead of a proximity thing.
            if (!m.Region.IsPartOf<TownRegion>())
                return false;

            return Server.Factions.Faction.IsNearType(m, RepairSkillInfo.GetInfo(m_Skill).NearbyTypes, 6);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

			writer.Write((int)m_Tier);
            writer.Write((int)m_Skill);
            writer.Write(m_SkillLevel);
            writer.Write(m_Crafter);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch( version )
            {
				case 1:
				{
					m_Tier = reader.ReadInt();
					goto case 0;
				}
                case 0:
                    {
                        m_Skill = (RepairSkillType)reader.ReadInt();
                        m_SkillLevel = reader.ReadDouble();
                        m_Crafter = reader.ReadMobile();

                        break;
                    }
            }
        }

        private static TextDefinition GetSkillTitle(int Tier)
        {
			//견습 장인 명장 거장 전설 신화 1116794
			return (1116793 + Tier );
			/*
				int skill = (int)(skillLevel / 10);

            if (skill >= 11)
                return (1062008 + skill - 11);
            else if (skill >= 5)
                return (1061123 + skill - 5);

            switch( skill )
            {
                case 4:
                    return "a Novice";
                case 3:
                    return "a Neophyte";
                default:
                    return "a Newbie";		//On OSI, it shouldn't go below 50, but, this is for 'custom' support.
            }
			*/
        }        
    }

    public class RepairSkillInfo
    {
        private static readonly RepairSkillInfo[] m_Table = new RepairSkillInfo[]
        {
                new RepairSkillInfo(DefBlacksmithy.CraftSystem,     typeof(Blacksmith), 1047013, 1023015),
                new RepairSkillInfo(DefTailoring.CraftSystem,       typeof(Tailor),     1061132, 1022981),
                new RepairSkillInfo(DefTinkering.CraftSystem,       typeof(Tinker),     1061166, 1022983),
                new RepairSkillInfo(DefCarpentry.CraftSystem,       typeof(Carpenter),  1061135, 1060774),
                new RepairSkillInfo(DefBowFletching.CraftSystem,    typeof(Bowyer),     1061134, 1023005),
                new RepairSkillInfo(DefMasonry.CraftSystem,         typeof(Carpenter),  1061135, 1060774, 1044635),
                new RepairSkillInfo(DefGlassblowing.CraftSystem,    typeof(Alchemist),  1111838, 1115634, 1044636),
                new RepairSkillInfo(DefInscription.CraftSystem,    	typeof(Mage),  		1111838, 1011468, 1044636),
        };

        private readonly CraftSystem m_System;
        private readonly Type[] m_NearbyTypes;
        private readonly TextDefinition m_NotNearbyMessage;
        private readonly TextDefinition m_Name;
        private readonly TextDefinition m_Description;

        public RepairSkillInfo(CraftSystem system, Type[] nearbyTypes, TextDefinition notNearbyMessage, TextDefinition name, TextDefinition description = null)
        {
            m_System = system;
            m_NearbyTypes = nearbyTypes;
            m_NotNearbyMessage = notNearbyMessage;
            m_Name = name;
            m_Description = description;
        }

        public RepairSkillInfo(CraftSystem system, Type nearbyType, TextDefinition notNearbyMessage, TextDefinition name, TextDefinition description = null)
            : this(system, new Type[] { nearbyType }, notNearbyMessage, name, description)
        {
        }

        public static RepairSkillInfo[] Table
        {
            get
            {
                return m_Table;
            }
        }
        public TextDefinition NotNearbyMessage
        {
            get
            {
                return m_NotNearbyMessage;
            }
        }
        public TextDefinition Name
        {
            get
            {
                return m_Name;
            }
        }
        public TextDefinition Description
        {
            get
            {
                return m_Description;
            }
        }
        public CraftSystem System
        {
            get
            {
                return m_System;
            }
        }
        public Type[] NearbyTypes
        {
            get
            {
                return m_NearbyTypes;
            }
        }
        public static RepairSkillInfo GetInfo(RepairSkillType type)
        {
            int v = (int)type;

            if (v < 0 || v >= m_Table.Length)
                v = 0;

            return m_Table[v];
        }
    }
}
