using System;

namespace Server.Items
{
    public enum PigmentType
    {
        None,
        ParagonGold,
        VioletCouragePurple,
        InvulnerabilityBlue,
        LunaWhite,
        DryadGreen,
        ShadowDancerBlack,
        BerserkerRed,
        NoxGreen,
        RumRed,
        FireOrange,
        FadedCoal,
        Coal,
        FadedGold,
        StormBronze,
        Rose,
        MidnightCoal,
        FadedBronze,
        FadedRose,
        DeepRose
    }

    public class DarkenedSky : CloseHelm
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public DarkenedSky()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 4;
			PrefixOption[82] = 40;
        }

        public DarkenedSky(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070966;
            }
        }// Darkened Sky

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class KasaOfTheRajin : WizardsHat
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public KasaOfTheRajin()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 8;
			PrefixOption[82] = 45;
        }

        public KasaOfTheRajin(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070969;
            }
        }// Kasa of the Raj-in

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class RuneBeetleCarapace : StuddedChest
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public RuneBeetleCarapace()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 14;
			PrefixOption[82] = 16;

        }

        public RuneBeetleCarapace(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070968;
            }
        }// Rune Beetle Carapace

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class Stormgrip : HideGloves
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public Stormgrip()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 26;
			PrefixOption[82] = 31;
        }

        public Stormgrip(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070970;
            }
        }// Stormgrip

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SwordOfTheStampede : Spear
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public SwordOfTheStampede()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 9;
			PrefixOption[82] = 48;
        }

        public SwordOfTheStampede(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070964;
            }
        }// Sword of the Stampede

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SwordsOfProsperity : Robe
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public SwordsOfProsperity()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 3;
			PrefixOption[82] = 51;
        }

        public SwordsOfProsperity(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070963;
            }
        }// Swords of Prosperity

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class TheHorselord : CompositeBow
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public TheHorselord()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 1;
			PrefixOption[82] = 60;
        }

        public TheHorselord(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070967;
            }
        }// The Horselord
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class TomeOfLostKnowledge : Magerybook
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public TomeOfLostKnowledge()
            : base()
        {
            LootType = LootType.Regular;
            Hue = 0x530;

			PrefixOption[80] = 2;
			PrefixOption[81] = 21;
			PrefixOption[82] = 75;
        }

        public TomeOfLostKnowledge(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070971;
            }
        }// Tome of Lost Knowledge
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class WindsEdge : GoldEarrings
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }
        [Constructable]
        public WindsEdge()
            : base()
        {
			PrefixOption[80] = 2;
			PrefixOption[81] = 26;
			PrefixOption[82] = 40;
        }

        public WindsEdge(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070965;
            }
        }// Wind's Edge

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PigmentsOfTokuno : BasePigmentsOfTokuno
    {
		public override bool IsArtifact { get { return true; } }

        public static int[][] Table { get { return m_Table; } }
        private static readonly int[][] m_Table = new int[][]
        {
            // Hue, Label
            new int[] { /*PigmentType.None,*/ 0, -1 },
            new int[] { /*PigmentType.ParagonGold,*/ 0x501, 1070987 },
            new int[] { /*PigmentType.VioletCouragePurple,*/ 0x486, 1070988 },
            new int[] { /*PigmentType.InvulnerabilityBlue,*/ 0x4F2, 1070989 },
            new int[] { /*PigmentType.LunaWhite,*/ 0x47E, 1070990 },
            new int[] { /*PigmentType.DryadGreen,*/ 0x48F, 1070991 },
            new int[] { /*PigmentType.ShadowDancerBlack,*/ 0x455, 1070992 },
            new int[] { /*PigmentType.BerserkerRed,*/ 0x21, 1070993 },
            new int[] { /*PigmentType.NoxGreen,*/ 0x58C, 1070994 },
            new int[] { /*PigmentType.RumRed,*/ 0x66C, 1070995 },
            new int[] { /*PigmentType.FireOrange,*/ 0x54F, 1070996 },
            new int[] { /*PigmentType.Fadedcoal,*/ 0x96A, 1079579 },
            new int[] { /*PigmentType.Coal,*/ 0x96B, 1079580 },
            new int[] { /*PigmentType.FadedGold,*/ 0x972, 1079581 },
            new int[] { /*PigmentType.StormBronze,*/ 0x977, 1079582 },
            new int[] { /*PigmentType.Rose,*/ 0x97C, 1079583 },
            new int[] { /*PigmentType.MidnightCoal,*/ 0x96C, 1079584 },
            new int[] { /*PigmentType.FadedBronze,*/ 0x975, 1079585 },
            new int[] { /*PigmentType.FadedRose,*/ 0x97B, 1079586 },
            new int[] { /*PigmentType.DeepRose,*/ 0x97E, 1079587 }
        };
        private PigmentType m_Type;
        [Constructable]
        public PigmentsOfTokuno()
            : this(PigmentType.None, 10)
        {
        }

        [Constructable]
        public PigmentsOfTokuno(PigmentType type)
            : this(type, (type == PigmentType.None || type >= PigmentType.FadedCoal) ? 10 : 50)
        {
        }

        [Constructable]
        public PigmentsOfTokuno(PigmentType type, int uses)
            : base(uses)
        {
            Weight = 1.0;
            Type = type;
        }

        public PigmentsOfTokuno(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PigmentType Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
				
                int v = (int)m_Type;

                if (v >= 0 && v < m_Table.Length)
                {
                    Hue = m_Table[v][0];
                    Label = m_Table[v][1];
                }
                else
                {
                    Hue = 0;
                    Label = -1;
                }
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1070933;
            }
        }// Pigments of Tokuno
        public static int[] GetInfo(PigmentType type)
        {
            int v = (int)type;

            if (v < 0 || v >= m_Table.Length)
                v = 0;
			
            return m_Table[v];
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.WriteEncodedInt((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion
			
            switch ( version )
            {
                case 1:
                    Type = (PigmentType)reader.ReadEncodedInt();
                    break;
                case 0:
                    break;
            }
        }
    }
}
