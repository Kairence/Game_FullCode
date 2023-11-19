using System;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
	/*
		최초 100 유저
	*/
	
	public class FirstSkillCheck : Item 
	{
		private bool[] m_Skill = new bool[100];
		private string[] m_User = new string[100];
		private int[] m_SkillStack = new int[100];
		private Mobile[] m_Reader = new Mobile[100];
		private DateTime m_LeaderRewardTime = DateTime.Now;
		
		public DateTime LeaderRewardTime
		{
			get{ return m_LeaderRewardTime;}
			set{ m_LeaderRewardTime = value; InvalidateProperties();}
		}


		public bool[] Skill
		{
			get{ return m_Skill;}
			set{ m_Skill = value; InvalidateProperties();}
		}
		public int[] SkillStack
		{
			get{ return m_SkillStack;}
			set{ m_SkillStack = value; InvalidateProperties();}
		}
		
		public string[] User
		{
			get{ return m_User;}
			set{ m_User = value; InvalidateProperties();}
		}

		public Mobile[] Reader
		{
			get{ return m_Reader;}
			set{ m_Reader = value; InvalidateProperties();}
		}

		
		public override string DefaultName
		{
			get { return "유저 스킬 체크 시스템"; }
		}
		[Constructable]
		public FirstSkillCheck() : base( 0xED4 )
		{
			Movable = false;
			Hue = 1168;
			Name = "유저 스킬 체크 시스템";
		}
		
		public string[] SkillName = { "연금술", "해부학", "동물지식", "아이템 감정", "장비학", "방패술", "구걸", "대장장이", "활제작술", "평화유지", "야영술", "목수", "지도제작술", "요리", "은신감지", "불협화음", "지능평가", "치료술", "낚시", "신앙", "낙농업", "은신하기", "도발연주", "기록술", "자물쇠 따기", "마법학", "마법 저항", "전술", "훔쳐보기", "음악연주", "중독술", "궁술", "영혼대화", "훔치기", "재봉술", "길들이기", "무두질", "기계공 기술", "반사신경", "수의학", "검술", "둔기술", "펜싱", "레슬링", "벌목술", "채광", "명상", "은신이동", "함정제거", "강령술", "집중", "기사도", "스매쉬", "암술", "원소술", "신비술", "임뷰잉", "쓰로잉" };
		
		public override void OnDoubleClick( Mobile from )
		{
			from.CloseGump( typeof( FirstSkillCheckGump ) );
			from.SendGump( new FirstSkillCheckGump( from, this ) );
		}

		public FirstSkillCheck( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 3 ); // version

			writer.Write( (DateTime) m_LeaderRewardTime );

			for (int i = 0; i < 100; i++)
			{
				writer.Write( (Mobile) m_Reader[i] );
			}
			for (int i = 0; i < 100; i++)
			{
				if( m_Skill[i] && m_SkillStack[i] == 0 )
					m_SkillStack[i] = 1;
				writer.Write( (int) m_SkillStack[i] );
			}
			for (int i = 0; i < 100; i++)
			{
				writer.Write( (bool) m_Skill[i] );
				writer.Write( (string) m_User[i] );
			}
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 3:
				{
					m_LeaderRewardTime = reader.ReadDateTime();
					goto case 2;
				}
				case 2:
				{
					for (int i = 0; i < 100; i++)
					{
						m_Reader[i] = reader.ReadMobile();
					}
					
					goto case 1;
				}
				case 1:
				{
					for (int i = 0; i < 100; i++)
					{
						m_SkillStack[i] = reader.ReadInt();
					}
					
					goto case 0;
				}
				case 0:
				{
					for (int i = 0; i < 100; i++)
					{
						m_Skill[i] = reader.ReadBool();
						m_User[i] = reader.ReadString();
					}
					break;
				}
			}
		}
	}
}
