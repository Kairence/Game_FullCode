using System;
using Server;
using Server.Items;

namespace Server.Items
{
    public enum FamilyCompType { Wealth, Resource, Hunting, Economy }

    public class DonationCheck : Item 
    {
        private DateTime m_RespawnTime = DateTime.Now;
        public DateTime RespawnTime { get { return m_RespawnTime; } set { m_RespawnTime = value; InvalidateProperties(); } }

        // 100위까지 기록하기 위해 배열 크기 확장
        private string[][] m_RankingNames = new string[4][];
        private int[][] m_RankingScores = new int[4][];
        private bool[][] m_IsNpc = new bool[4][];

        public string[][] RankingNames => m_RankingNames;
        public int[][] RankingScores => m_RankingScores;
        public bool[][] IsNpc => m_IsNpc;

        private FamilyCompType m_ActiveTheme;
        public FamilyCompType ActiveTheme { get { return m_ActiveTheme; } set { m_ActiveTheme = value; InvalidateProperties(); } }

        [Constructable]
        public DonationCheck() : base(0xED4)
        {
            Movable = false;
            Hue = 1121;
            Name = "가문 시스템 데이터 센터";
            m_ActiveTheme = (FamilyCompType)Utility.Random(4);
            
            for (int i = 0; i < 4; i++)
            {
                m_RankingNames[i] = new string[100];
                m_RankingScores[i] = new int[100];
                m_IsNpc[i] = new bool[100];
            }
        }

        public DonationCheck(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            
            writer.Write(2); // Version 2 (배열 100칸으로 확장)

            writer.Write(m_RespawnTime);
            writer.Write((int)m_ActiveTheme);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    writer.Write(m_RankingNames[i][j]);
                    writer.Write(m_RankingScores[i][j]);
                    writer.Write(m_IsNpc[i][j]);
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_RespawnTime = reader.ReadDateTime();
            m_ActiveTheme = (FamilyCompType)reader.ReadInt();

            for (int i = 0; i < 4; i++)
            {
                m_RankingNames[i] = new string[100];
                m_RankingScores[i] = new int[100];
                m_IsNpc[i] = new bool[100];
            }

            if (version >= 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        m_RankingNames[i][j] = reader.ReadString();
                        m_RankingScores[i][j] = reader.ReadInt();
                        m_IsNpc[i][j] = reader.ReadBool();
                    }
                }
            }
            else
            {
                // 구버전(10칸) 데이터 로드 시의 하위 호환성 처리
                int limit = 10;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < limit; j++)
                    {
                        string tempName = reader.ReadString();
                        int tempScore = reader.ReadInt();
                        bool tempNpc = reader.ReadBool();
                        
                        m_RankingNames[i][j] = tempName;
                        m_RankingScores[i][j] = tempScore;
                        m_IsNpc[i][j] = tempNpc;
                    }
                }
                
                // version 1에서 사용하던 WeeklyVP는 Event.cs의 중앙 관리로 이관되었으므로 무시 처리
                if (version == 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int count = reader.ReadInt();
                        for (int j = 0; j < count; j++)
                        {
                            reader.ReadString();
                            reader.ReadInt();
                        }
                    }
                }
            }
        }
    }
}