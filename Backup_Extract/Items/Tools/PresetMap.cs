using System;
using System.Collections.Generic; // 추가
using System.Linq; // 추가 (Enumerable.FirstOrDefault 사용을 위해)
using Server;
using Server.Mobiles;
using Server.Regions;

namespace Server.Items
{
    public class PresetMap : MapItem
    {
        private int m_LabelNumber;
        private Point3D m_RecallLoc;
        private int m_RequiredSkill;
        private bool m_IsScouted;

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D RecallLoc { get { return m_RecallLoc; } set { m_RecallLoc = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsScouted { get { return m_IsScouted; } set { m_IsScouted = value; } }

        [Constructable]
        public PresetMap(PresetMapType type)
        {
            int v = (int)type;
            if (v >= 0 && v < PresetMapEntry.Table.Length)
                InitEntry(PresetMapEntry.Table[v]);
        }

        public PresetMap(PresetMapEntry entry)
        {
            if (entry != null)
                InitEntry(entry);
        }

        public void InitEntry(PresetMapEntry entry)
        {
            m_LabelNumber = entry.Name;
            Width = entry.Bounds.Width;
            Height = entry.Bounds.Height;
            Bounds = entry.Bounds;
            m_RequiredSkill = entry.RequiredSkill;

            string regionName = GetRegionNameByCliloc(entry.Name);
            
            // [수정] Dictionary 구조에서도 작동하도록 검색 로직 변경
            Region reg = null;
            foreach (Region r in Map.Trammel.Regions.Values)
            {
                if (r.Name == regionName)
                {
                    reg = r;
                    break;
                }
            }

            if (reg != null && reg.GoLocation != Point3D.Zero)
            {
                m_RecallLoc = reg.GoLocation;
                m_IsScouted = true;
                //Hue = 1154;
            }
            else
            {
                m_RecallLoc = Point3D.Zero;
                m_IsScouted = false;
            }
            
            this.Map = Map.Trammel; 
        }

        public static string GetRegionNameByCliloc(int cliloc)
        {
            switch (cliloc)
            {
                case 1041189: return "Britain";
                case 1041188: return "Moonglow";
                case 1041177: return "Trinsic";
                case 1041182: return "Minoc";
                case 1041178: return "Vesper";
                case 1041179: return "Yew";
                case 1041181: return "Jhelom";
                case 1041180: return "Skara Brae";
                case 1041186: return "Magincia";
                case 1041187: return "Ocllo";
                case 1041183: return "Buccaneer's Den";
                case 1041185: return "Nujelm";
                case 1041184: return "Serpent's Hold";
                case 1129001: return "Cove";
                default: return "";
            }
        }

		public void DoScout(Mobile from)
		{
			if (m_IsScouted)
			{
				from.SendLocalizedMessage(503428); // Already scouted.
				return;
			}

			if (!this.Bounds.Contains(from.Location))
			{
				from.SendLocalizedMessage(503427); // Not in the area.
				return;
			}

			// [변경] 스킬 체크는 Sextant에서 이미 했으므로 여기선 바로 성공 처리
			SuccessScout(from.Region, from);
		}

        private void SuccessScout(Region reg, Mobile from)
        {
            m_IsScouted = true;
            m_RecallLoc = reg.GoLocation;
            Hue = 1154;
            from.SendMessage($"{reg.Name}의 지형 정보를 완벽히 기록했습니다. 이제 리콜이 가능합니다.");
        }

        public override int LabelNumber { get { return (m_LabelNumber == 0 ? base.LabelNumber : m_LabelNumber); } }

        public PresetMap(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); 
            writer.Write(m_RecallLoc);
            writer.Write(m_IsScouted);
            writer.Write(m_RequiredSkill);
            writer.Write(m_LabelNumber);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            switch (version)
            {
                case 1:
                    {
                        m_RecallLoc = reader.ReadPoint3D();
                        m_IsScouted = reader.ReadBool();
                        m_RequiredSkill = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        m_LabelNumber = reader.ReadInt();
                        if (version < 1)
                        {
                            m_RecallLoc = Point3D.Zero;
                            m_IsScouted = false;
                            m_RequiredSkill = 100;
                        }
                        break;
                    }
            }
        }
    }

    public class PresetMapEntry
    {
        private int m_Name, m_RequiredSkill;
        private Rectangle2D m_Bounds;

        public int Name { get { return m_Name; } }
        public Rectangle2D Bounds { get { return m_Bounds; } }
        public int RequiredSkill { get { return m_RequiredSkill; } }

        public PresetMapEntry(int name, int skill, int xLeft, int yTop, int xRight, int yBottom)
        {
            m_Name = name;
            m_RequiredSkill = skill;
            m_Bounds = new Rectangle2D(xLeft, yTop, xRight - xLeft, yBottom - yTop);
        }

        private static PresetMapEntry[] m_Table = new PresetMapEntry[]
        {
            new PresetMapEntry( 1041189, 100, 1092, 1396, 1736, 1924 ), // Britain
            new PresetMapEntry( 1041188, 100, 4156, 0808, 4732, 1528 ), // Moonglow
            new PresetMapEntry( 1041177, 100, 1792, 2630, 2118, 2952 ), // Trinsic
            new PresetMapEntry( 1041182, 100, 2360, 0356, 2706, 0702 ), // Minoc
            new PresetMapEntry( 1041178, 100, 2636, 0592, 3064, 1012 ), // Vesper
            new PresetMapEntry( 1041179, 100, 0236, 0741, 0766, 1269 ), // Yew
            new PresetMapEntry( 1041181, 100, 1088, 3572, 1528, 4056 ), // Jhelom
            new PresetMapEntry( 1041180, 100, 0524, 2064, 0960, 2452 ), // Skara Brae
            new PresetMapEntry( 1041186, 100, 3530, 2022, 3818, 2298 ), // Magincia
            new PresetMapEntry( 1041187, 100, 3582, 2456, 3770, 2742 ), // Ocllo
            new PresetMapEntry( 1041183, 100, 2500, 1900, 3000, 2400 ), // Buccaneer's Den
            new PresetMapEntry( 1041185, 100, 3446, 1030, 3832, 1424 ), // Nujelm
            new PresetMapEntry( 1041184, 100, 2714, 3329, 3100, 3639 ), // Serpent's Hold
            new PresetMapEntry( 1129001, 100, 2200, 1110, 2360, 1248 ), // Cove
            new PresetMapEntry( 1041204, 999, 0000, 0000, 5199, 4095 ), // The World
            new PresetMapEntry( 0, 100, 600, 3280, 950, 3650 ), // Royal City
            new PresetMapEntry( 0, 100, 890, 430, 1090, 600 ), // Luna
            new PresetMapEntry( 0, 100, 5630, 3080, 5860, 3330 ), // Papua
            new PresetMapEntry( 0, 100, 5140, 3900, 5330, 4094 ), // Delucia
        };

        public static PresetMapEntry[] Table { get { return m_Table; } }
    }

    public enum PresetMapType
    {
        Britain, Moonglow, Trinsic, Minoc, Vesper, Yew, Jhelom, SkaraBrae,
        Magincia, Ocllo, BucsDen, Nujelm, SerpentsHold, Cove, World,
        RoyalCity, Luna, Papua, Delucia
    }
}
