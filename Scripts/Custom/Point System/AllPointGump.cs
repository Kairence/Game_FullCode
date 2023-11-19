using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Multis;
using Server.Accounting;
using System.Collections.Generic;
//using Server.Engines.CityLoyalty;

namespace Server.Gumps
{
	public class CityPointGump : Gump
	{
        //private const int LabelColor = 0x7FFF;
        //private const int LabelColorDisabled = 0x4210;
		private readonly PlayerMobile m_pm;
		
        public CityPointGump(PlayerMobile pm) : base(50, 50)
        {
            AddHtml(50, 7, 345, 20, "경험 시스템", false, false); // City Loyalty
			
			m_pm = pm;

			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			//pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));
			
            AddPage(0);

			Account acc = pm.Account as Account;
			
			
            AddBackground(0, 0, 280, 220, 5054);

            AddHtml(10, 10, 250, 20, "경험 시스템", false, false); 

            AddButton(10, 190, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(45, 190, 150, 20, 3000363, false, false); // Close

            AddPage(1);

            AddButton(10, 40, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtml(45, 40, 105, 20, "예전 생산 포인트:", false, false); 
			AddHtml(150, 40, 110, 16, String.Format("{0:#,###}", pm.GoldPoint[49]), false, false);

            AddButton(10, 60, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtml(45, 60, 105, 20,  "전투 포인트:", false, false);  
			AddHtml(150, 60, 110, 16, String.Format("{0:#,###}", pm.SilverPoint[0]), false, false);

            AddButton(10, 80, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddHtml(45, 80, 105, 20,  "가문 포인트", false, false); 
			AddHtml(150, 80, 110, 16, String.Format("{0:#,###}", acc.Point[0]), false, false);
			//AddHtml(150, 40, 110, 16, "), false, false);

			AddButton(10, 100, 4005, 4007, 4, GumpButtonType.Reply, 0);
			AddHtml(45, 100, 105, 20,  "유물 포인트:", false, false);
			AddHtml(150, 40, 110, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);
			
			AddButton(10, 120, 4005, 4007, 5, GumpButtonType.Reply, 0);
			AddHtml(45, 120, 105, 20,  "스킬 포인트", false, false);
			//AddHtml(150, 40, 110, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

            AddHtml(10, 150, 150, 20, "명성: ", false, false); // Fame: ~1_AMT~
            AddHtml(10, 170, 150, 20, "카르마: ", false, false); // Karma: ~1_AMT~}

            AddHtml(65, 150, 150, 20, pm.Fame.ToString(), false, false); // Fame: ~1_AMT~
            AddHtml(65, 170, 150, 20, pm.Karma.ToString(), false, false); // Fame: ~1_AMT~
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive())
                return;

            switch ( info.ButtonID )
            {
                case 1:
                    {
                        m_pm.SendGump(new GoldPointGump(m_pm));
                        break;
                    }
                case 2:
                    {
                        m_pm.SendGump(new SilverPointGump(m_pm));
                        break;
                    }
                case 3:
                    {
                        m_pm.SendGump(new EquipPointGump(m_pm));
                        break;
                    }
                case 4:
                    {
                        m_pm.SendGump(new ArtifactPointGump(m_pm));
						break;
                    }
                case 5:
                    {
                        m_pm.SendGump(new SkillPointGump(m_pm));
						break;
                    }
			}
        }		
	}

	#region 채집/생산
    public class GoldPointGump : Gump
    {
		private string[] GrawName = { "채집 경험 증가", "고급 채집 증가", "채집 수량 증가", "채집 변환 증가", "제작 경험 증가", "제작 성공 증가", "고급 제작 증가", "제작 수량 증가" };
		
		//private int[] MaxLevel = { 40, 75, 25, 50, 50, 70, 100, 100 };
		//private int[] BuffOption = { 100, 300, 500, 1000, 2000, 5000, 100, 250, 500, 1000, 2000 };

		//private int[] SkillCount = { 3, 36, 19, 10, 38, 56};
		
		/*
		private int pointCal( int number, int level )
		{
			int point = 1000000000;
			if( number == 1 )
				point = ( level + 1 ) * 50;
			else if( number == 2 )
				point = ( level + 1 ) * 250;
			else if( number == 3 || number == 4 )
				point = ( level + 1 ) * ( level + 1 ) * 10000;
			else
				point = BuffOption[level];
			return point;
		}
		*/
		readonly int Max_Level = 100;

		private PlayerMobile m_pm;
		private int m_Harvestpoint = 0; 
		private int m_Craftpoint = 0; 
        public GoldPointGump(PlayerMobile pm) : base(50, 50)
        {
			/*
			49번 : 기존 생산 경험치
			0번 : 채집 경험치
			1번 : 채집 포인트
			2번 : 채집 경험 증가
			3번 : 고급 채집 증가
			4번 : 채집 수량 증가
			5번 : 채집 변환 증가
			10번 : 제작 경험치
			11번 : 제작 포인트
			12번 : 제작 경험 증가
			13번 : 제작 성공 증가
			14번 : 고급 제작 증가
			15번 : 제작 수량 증가
			*/
			pm.CloseGump(typeof(CityPointGump));
			pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;

			int HarvestQuest = 0;
			for( int i = 0; i < 9; i++ )
			{
				if ( pm.QuestCheck[i] )
				{
					HarvestQuest += 5;
					if( i == 8 )
						HarvestQuest += 5;
				}
			}
			
			m_Harvestpoint = Misc.Util.Level( pm.GoldPoint[0] ) - pm.GoldPoint[1] + HarvestQuest;
			m_Craftpoint = Misc.Util.Level( pm.GoldPoint[10] ) - pm.GoldPoint[11];
			//pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

            AddPage(0);

            AddBackground(0, 0, 425, 600, 5054);
	
            //AddImageTiled(10, 10, 500, 150, 2624);
            //AddAlphaRegion(10, 10, 500, 150);

            AddHtml(10, 10, 250, 20, "채집 경험치", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 10, 200, 16, pm.GoldPoint[0] > 0 ? (String.Format("{0:#,###}", pm.GoldPoint[0])) : "0", false, false);

			
            AddHtml(10, 60, 250, 20, "남은 포인트", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 60, 200, 16, m_Harvestpoint > 0 ? String.Format("{0:#,###}", m_Harvestpoint ) : "0", false, false);
			
            AddHtml(10, 80, 100, 20, "현재 레벨", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 80, 40, 16, Misc.Util.Level( pm.GoldPoint[0] ) >= Misc.Util.MaxLevel ? Misc.Util.MaxLevel.ToString() : Misc.Util.Level( pm.GoldPoint[0] ).ToString(), false, false);

            AddHtml(10, 110, 130, 20, "다음 경험치", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 110, 250, 16, Misc.Util.Level( pm.GoldPoint[0] ) >= Misc.Util.MaxLevel ? "없음" : String.Format("{0:#,###}", Misc.Util.NextLevel( pm.GoldPoint[0] ) ), false, false);
			
            AddHtml(210, 10, 250, 20, "제작 경험치", false, false); // <CENTER>HOUSE 			
			AddHtml(330, 10, 200, 16, pm.GoldPoint[10] > 0 ? (String.Format("{0:#,###}", pm.GoldPoint[10])) : "0", false, false);

            AddHtml(210, 60, 250, 20, "남은 포인트", false, false); // <CENTER>HOUSE 			
			AddHtml(330, 60, 200, 16, m_Craftpoint > 0 ? String.Format("{0:#,###}", m_Craftpoint ) : "0", false, false);
			
            AddHtml(210, 80, 100, 20, "현재 레벨", false, false); // <CENTER>HOUSE 			
			AddHtml(330, 80, 40, 16, Misc.Util.Level( pm.GoldPoint[10] ) >= Misc.Util.MaxLevel ? "최대 레벨" : Misc.Util.Level( pm.GoldPoint[10] ).ToString(), false, false);

            AddHtml(210, 110, 130, 20, "다음 경험치", false, false); // <CENTER>HOUSE 			
			AddHtml(330, 110, 250, 16, Misc.Util.Level( pm.GoldPoint[10] ) >= Misc.Util.MaxLevel ? "없음" : String.Format("{0:#,###}", Misc.Util.NextLevel( pm.GoldPoint[10] ) ), false, false);
			


			
            AddHtml(50, 140, 225, 20, "옵션", false, false); // House Description
            AddHtml(260, 140, 225, 20, "옵션", false, false); // House Description
            //AddHtml(275, 90, 225, 20, "이름(현재 레벨)", false, false); // House Description
			
			if( pm.DeathCheck == 0 )
			{
				AddHtml(50, 40, 195, 20, "포인트 리셋(1주 1회)", false, false);
				AddButton( 10, 40, 4005, 4007, 100, GumpButtonType.Reply, 0);
				AddHtml(260, 40, 195, 20, "포인트 리셋(1주 1회)", false, false);
				AddButton( 210, 40, 4005, 4007, 200, GumpButtonType.Reply, 0);
			}

			int x = 50;
			int y = 160;
			int index = 2;
			for ( int i = 0; i < GrawName.Length; i++)
			{
				if( i == 4 )
				{
					x = 260;
					y = 160;
					index = 8;
				}
				int point = 0;
				//이름
				string allname = GrawName[i];

				allname += "(" + pm.GoldPoint[i + index].ToString() + ")";
				AddHtml(x, y, 250, 20, allname, false, false);
				if( i < 4 )
				{
					if( m_Harvestpoint > 0 && pm.GoldPoint[i + index] < Max_Level )
						AddButton(x - 40, y, 4005, 4007, i + index, GumpButtonType.Reply, 0);
				}
				else

				{
					if( m_Craftpoint > 0 && pm.GoldPoint[i + index] < Max_Level )
						AddButton(x - 40, y, 4005, 4007, i + index, GumpButtonType.Reply, 0);
				}
				y += 20;
				//index++;
			}
			
			y += 20;
			
			string HarvestQuestList = "채집 퀘스트 달성 없음";
			for( int i = 8; i < 0; --i )
			{
				if ( pm.QuestCheck[i] )
				{
					HarvestQuestList = "채집 퀘스트 " + (i + 1).ToString() + "단계 달성";
					break;
				}
			}
			AddHtml(10, y, 250, 20, HarvestQuestList, false, false);

			y += 20;

			AddHtml(50, y, 250, 20, "채집 업적 확인 버튼", false, false);
			AddButton( 10, y, 4005, 4007, 150, GumpButtonType.Reply, 0);
			
			AddHtml(260, y, 250, 20, "제작 업적 확인 버튼", false, false);
			AddButton( 210, y, 4005, 4007, 250, GumpButtonType.Reply, 0);
			
			if( pm.GoldPoint[49] > 0 )
			{
				y += 30;
				AddHtml(50, y, 205, 20, "예전 생산 포인트:", false, false); 
				AddHtml(215, y, 110, 16, String.Format("{0:#,###}", pm.GoldPoint[49]), false, false);
				
				y += 20;
				AddHtml(50, y, 300, 20, "예전 생산 포인트를 채집 포인트로 전환", false, false); 
				AddButton( 10, y, 4005, 4007, 300, GumpButtonType.Reply, 0);

				y += 20;
				AddHtml(50, y, 300, 20, "예전 생산 포인트를 제작 포인트로 전환", false, false); 
				AddButton( 10, y, 4005, 4007, 400, GumpButtonType.Reply, 0);
			}
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			if( info.ButtonID == 100 )
			{
				m_pm.DeathCheck = 1;
				for( int i = 1; i <= 5; i++ )
				{
					m_pm.GoldPoint[i] = 0;
				}
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else if( info.ButtonID == 150 )
			{
				m_pm.CloseGump(typeof(HarvestGump));
				m_pm.SendGump(new HarvestGump(m_pm));
				
			}
			else if( info.ButtonID == 200 )
			{
				m_pm.DeathCheck = 1;
				for( int i = 11; i <= 15; i++ )
				{
					m_pm.GoldPoint[i] = 0;
				}
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else if( info.ButtonID == 250 )
			{
				m_pm.CloseGump(typeof(CraftingGump));
				m_pm.SendGump(new CraftingGump(m_pm));
				
			}
			else if( info.ButtonID == 300 )
			{
				m_pm.SendMessage("1부터 " + m_pm.GoldPoint[49].ToString() + "사이의 숫자를 적으세요!");
				m_pm.BeginPrompt(
				(from, text) =>
				{
					int amount = Utility.ToInt32(text);
					if( amount <= m_pm.GoldPoint[49] )
					{
						from.SendMessage("채집 포인트를 " + amount.ToString() + "만큼 획득합니다!" );
						m_pm.GoldPoint[0] += amount;
						m_pm.GoldPoint[49] -= amount;
					}
					else
						from.SendMessage("잘못된 숫자나 문자를 넣으셨네요...");
				});				
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else if( info.ButtonID == 400 )
			{
				m_pm.SendMessage("1부터 " + m_pm.GoldPoint[49].ToString() + "사이의 숫자를 적으세요!");
				m_pm.BeginPrompt(
				(from, text) =>
				{
					int amount = Utility.ToInt32(text);
					if( amount <= m_pm.GoldPoint[49] )
					{
						from.SendMessage("제작 포인트를 " + amount.ToString() + "만큼 획득합니다!" );
						m_pm.GoldPoint[10] += amount;
						m_pm.GoldPoint[49] -= amount;
					}
					else
						from.SendMessage("잘못된 숫자나 문자를 넣으셨네요...");
				});				
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			else
			{
				int index = 1;
				if( info.ButtonID >= 12 && info.ButtonID <= 15 )
					index = 11;
				m_pm.GoldPoint[info.ButtonID]++;
				m_pm.GoldPoint[index]++;
				m_pm.Delta(MobileDelta.Stat);
				m_pm.ProcessDelta();
				m_pm.SendGump(new GoldPointGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
        }
        private void SendGump()
        {
            m_pm.SendGump(new GoldPointGump(m_pm));
        }
	}
	#endregion

	#region 전투
    public class SilverPointGump : Gump
    {
		readonly int Max_Level = 50;
		private PlayerMobile m_pm;
		
		//30번까지 증가
		//35번까지 포인트
		//70번 전체 던전 포인트
		//71번부터 던전 포인트
		
		private string[] Name = { "전투 경험 증가", "어그로 증가", "어그로 감소", "특수기 확률 증가", "무기 데미지 증가", "마법 데미지 증가", "관통 데미지 증가", "충격 데미지 증가", "출혈 데미지 증가", "화염 데미지 증가", "냉기 데미지 증가", "독 데미지 증가", "에너지 데미지 증가", "치유량 증가", "회복량 증가", "무기 치명타 확률 증가", "마법 치명타 확률 증가", "무기 치명타 데미지 증가", "마법 치명타 데미지 증가", "체력 증가", "기력 증가", "마나 증가", "체력 재생 증가", "기력 재생 증가", "마나 재생 증가", "모든 저항 증가", "힘 증가", "민첩 증가", "지능 증가", "운 증가" };

		private string[] Point_Name = {"희귀 포인트", "영웅 포인트", "서사 포인트", "전설 포인트", "신화 포인트" };
		
		private string[] Basic_Dungeon_Name = {"코베투스", "데스파이즈", "디싯", "쉐임", "오크 던전", "롱 던전", "아이스 던전", "파이어 던전", "히스로스 던전", "데스타드 던전" };
		
		private int[] Space = { 0, 2, 3, 5, 8, 12, 14, 16, 18, 21, 24 };
		
		private int m_point = 0; 
        public SilverPointGump(PlayerMobile pm) : base(50, 50)
        {
			int DungeonQuest = 0;
			for( int i = 0; i < 30; i++ )
			{
				if ( pm.QuestCheck[10000 + i] )
				{
					DungeonQuest += ( i % 3 ) + 1;
				}
			}

			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;
			m_point = Misc.Util.Level( pm.SilverPoint[0] ) - pm.SilverPoint[1] + DungeonQuest;
			pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));
            AddPage(0);

			
            AddBackground(0, 0, 1300, 880, 5054);
			
            AddHtml(10, 10, 250, 20, "전투 포인트", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", pm.SilverPoint[0]), false, false);

            AddBackground(0, 0, 1300, 880, 5054);
			
            AddHtml(10, 40, 250, 20, "남은 포인트", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 40, 200, 16, m_point > 0 ? String.Format("{0:#,###}", m_point ) : "0", false, false);
			
            AddHtml(10, 60, 100, 20, "현재 레벨", false, false); // <CENTER>HOUSE 			
			AddHtml(120, 60, 40, 16, Misc.Util.Level( pm.SilverPoint[0] ) >= Misc.Util.MaxLevel ? Misc.Util.MaxLevel.ToString() : Misc.Util.Level( pm.SilverPoint[0] ).ToString(), false, false);

            AddHtml(10, 90, 130, 20, "다음 경험치", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 90, 250, 16, Misc.Util.Level( pm.SilverPoint[0] ) >= Misc.Util.MaxLevel ? "없음" : String.Format("{0:#,###}", Misc.Util.NextLevel( pm.SilverPoint[0] ) ), false, false);



            //AddHtml(500, 10, 250, 20, "전투 스킬 포인트", false, false); // <CENTER>HOUSE 			
			//AddHtml(380, 10, 200, 16, String.Format("{0:#,###}", m_point), false, false);
			
			
            AddHtml(50, 120, 225, 20, "옵션", false, false); // House Description
            //AddHtml(275, 40, 225, 20, "이름(현재 레벨)", false, false); // House Description
            //AddHtml(275, 40, 75, 20, "레벨", false, false); // Storage
            //AddHtml(350, 40, 150, 20, "비용", false, false); // Lockdowns
			if( pm.DeathCheck == 0 )
			{
				AddHtml(655, 40, 195, 20, "포인트 리셋(1주 1회)", false, false);
				AddButton( 850, 40, 4005, 4007, 100, GumpButtonType.Reply, 0);
			}
			int y = 140;
			int size = 0;
			
			for ( int i = 0; i < Name.Length; i++)
			{
				int point = 0;
				//이름
				string allname = Name[i];

				allname += "(" + pm.SilverPoint[i + 2].ToString() + ")";
				AddHtml(50 + size, y, 250, 20, allname, false, false);
				if( m_point > 0 && pm.SilverPoint[i + 2] < Max_Level )
					AddButton(10 + size, y, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				size += 250;
				for( int j = 0; j < Space.Length; j++ )
				{
					if( i == Space[j] ) //띄어쓰기
					{
						y += 20;
						size = 0;
					}
				}
			}
			//포인트 적립
			/*
			y += 30;
            AddHtml(50, y, 405, 20, "등급 포인트", false, false); // House Description
			y += 20;
            AddHtml(50, y, 405, 20, "포인트 변환 시 티어 당 1, 2, 4, 7, 10, 15, 23개 획득. 유물 1단계 당 5개 추가 증가", false, false); // 			y += 20;
			y += 20;
            AddHtml(50, y, 405, 20, "아이템 제작 시 1 + 유물 등급 * 유물 등급 소모", false, false); // House Description
			y += 20;
            AddHtml(50, y, 405, 20, "아이템 변환은 등급 옆의 화살표 클릭. 제작은 제작 스킬의 아이템 강화 클릭.", false, false); // House Description
			y += 20;
			size = 0;
			for ( int i = 0; i < Point_Name.Length; i++ )
			{
				string allname = Point_Name[i];
				Account acc = pm.Account as Account;
				allname += " : " + String.Format("{0:#,###}", acc.Point[861 + i] > 0 ? acc.Point[861 + i].ToString() : "0" ) + "점";
				AddHtml(50 + size, y, 250, 20, allname, false, false);
				AddButton(10 + size, y, 4005, 4007, i + 31, GumpButtonType.Reply, 0);
				size += 250;
			}
			*/
			//업적 확인
			y += 30;
			size = 0;
            AddHtml(50, y, 405, 20, "업적 확인 버튼", false, false); // House Description
			AddButton(10 + size, y, 4005, 4007, 150, GumpButtonType.Reply, 0);
			
			//퀘스트 완료 확인
			y += 30;
            AddHtml(50, y, 405, 20, "퀘스트 완료 확인", false, false); // House Description

			y += 30;
			
			string BasicDungeonList = "";
			int index = 0;
			for( int i = 0; i < 10; ++i)
			{
				BasicDungeonList = " 퀘스트 달성 없음";
				for( int j = 0; j < 3; ++j )
				{
					if( pm.QuestCheck[10000 + index * 3 + j] )
						BasicDungeonList = " 퀘스트 " + (index + 1).ToString() + "단계 달성";
				}
				if( i == 5 )
				{
					index = 0;
					size = 0;
					y += 20;
				}
				AddHtml(50 + size, y, 250, 20, Basic_Dungeon_Name[i] + BasicDungeonList, false, false); 

				index++;
				size += 250;
			}
		}

        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;
			
			if( info.ButtonID == 100 )
			{
				m_pm.DeathCheck = 1;
				int returnPoint = 0;
				m_pm.DeathCheck = 1;
				for( int i = 1; i < m_pm.SilverPoint.Length; i++ )
				{
					m_pm.SilverPoint[i] = 0;
				}
				m_pm.SendGump(new SilverPointGump(m_pm));
				//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
			}
			else if( info.ButtonID == 150 )
			{
				m_pm.CloseGump(typeof(MonsterFeatGump));
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			else if( info.ButtonID <= 30 )
			{
				m_pm.SilverPoint[info.ButtonID + 1]++;
				m_pm.SilverPoint[1]++;
				m_pm.Delta(MobileDelta.Stat);
				m_pm.ComputeResistances();
				m_pm.ProcessDelta();
				m_pm.SendGump(new SilverPointGump(m_pm));
				//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
			}
			else
			{
				//m_pm.Target = new InternalTarget(info.ButtonID - 30);
				m_pm.SendGump(new SilverPointGump(m_pm));
			}
        }
		
		//장비 변환 클릭
		/*
		private class InternalTarget : Target
		{
			int m_Rank = 0;
			public InternalTarget(int rank) :  base ( 8, false, TargetFlags.None )
			{
				m_Rank = rank;
			}
			protected override void OnTarget( Mobile from, object targeted )
			{
				if( m_Rank == 0 )
				{
				}
				else if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					if( targeted is Item )
					{
						Item item = targeted as Item;
						BaseHouse house = BaseHouse.FindHouseAt(from);
						if( item.RootParent == from || ( house != null && house.IsOwner(from)) )
							Misc.Util.EquipPointReturn(pm, item, m_Rank);
					}
				}
			}
		}
		*/
        private void SendGump()
        {
            m_pm.SendGump(new SilverPointGump(m_pm));
        }
	}
	#endregion
	
	#region 업적
    public class EquipPointGump : Gump //1포인트 기대값 171.6666666 지피
    {
		private PlayerMobile m_pm;
		
		private string[] NameA = { "로그인 보너스" }; //, "생산 포인트 (10 포인트)", "전투 포인트 (10 포인트)" }; 
		private string[] Name = { "광물 채집", "목재 채집", "가죽 채집", "생산품 제작", "1만 명성", "2만 명성", "3만 명성" }; 
		private string[] accProduct = { "케릭터 구매", "집 구매", "스킬 경험치 1% 증가 구매" };
		private string[] goldProduct = { "즉석 복권 구매(0.25% 100만, 1% 10만, 10% 1만)", "추첨 복권 구매", "시약 8종 1000세트 6천", "시약 8종 10000세트 6만", "집 텔레포트 10만" };
		
		private int[] GivePoint = { 100, 900, 8500, 80000 };
		
		private int[] CharacterBuyPrice = { 10, 80, 500, 2500, 10000, 25000 }; 
		
		private int[] Price = { 15, 150, 17, 165, 1600, 10000 };
		private int[] goldPrice = { 1000, 1000, 6000, 60000, 100000 };
		
        public EquipPointGump(PlayerMobile pm) : base(50, 50)
        {
			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;
			//pm.CloseGump(typeof(SilverPointGump));
			pm.CloseGump(typeof(EquipPointGump));
			//pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

			Account acc = pm.Account as Account;
			
            AddPage(0);

            AddBackground(0, 0, 525, 590, 5054);
			
            //AddImageTiled(10, 10, 500, 150, 2624);
            //AddAlphaRegion(10, 10, 500, 150);

            AddHtml(10, 10, 250, 20, "가문 포인트: ", false, false); // <CENTER>HOUSE 			
			AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", acc.Point[0]), false, false);

            AddHtml(50, 40, 225, 20, "목록", false, false); // House Description
            AddHtml(275, 40, 75, 20, "상태", false, false); // Storage
            AddHtml(350, 40, 150, 20, "획득", false, false); // Lockdowns

			string donation = String.Format("{0:#,###}", acc.DonationPoint);
			
			if( acc.DonationPoint == 0 )
				AddHtml(50, 90, 350, 20, "이번 주 기부 없음", false, false); // House 				
			else
				AddHtml(50, 90,3500, 20, "이번 주 기부 포인트 " + donation + "점", false, false); // House Description
			AddButton(10, 90, 4005, 4007, 100, GumpButtonType.Reply, 0);

			donation = "";

			DonationCheck check = null;
			if( Server.Event.dc == null )
			{
				foreach ( Item item in World.Items.Values )
				{
					if ( item is DonationCheck )
					{
						DonationCheck dc = item as DonationCheck;
						Server.Event.dc = dc;
						check = dc;
						break;
					}
				}
			}
			else
				check = Server.Event.dc;
			
			if( check != null )
			{
				for( int i = 0; i < 1000; i++ )
				{
					if( check.DonationList[i] == acc.Username )
					{
						donation = ( i + 1 ).ToString();
						break;
					}
				}
			}
			
			
			if( donation == "")
				AddHtml(250, 90, 150, 20, "현재 순위 없음", false, false); // Storage
			else
				AddHtml(250, 90, 150, 20, "현재 순위 " + donation + "등", false, false); // Storage

            AddHtml(50, 110, 500, 20, "1만 골드 기부하기 : 기부 & 가문 포인트 100점 획득", false, false); // Lockdowns
            AddHtml(50, 130, 500, 20, "10만 골드 기부하기 : 기부 & 가문 포인트 900점 획득", false, false); // Lockdowns
            AddHtml(50, 150, 500, 20, "100만 골드 기부하기 : 기부 & 가문 포인트 8500점 획득", false, false); // Lockdowns
            AddHtml(50, 170, 500, 20, "1000만 골드 기부하기 : 기부 & 가문 포인트 80000점 획득", false, false); // Lockdowns
			if( Banker.GetBalance(pm) >= 10000 )
				AddButton(10, 110, 4005, 4007, 1, GumpButtonType.Reply, 0);
			if( Banker.GetBalance(pm) >= 100000 )
				AddButton(10, 130, 4005, 4007, 2, GumpButtonType.Reply, 0);
			if( Banker.GetBalance(pm) >= 1000000 )
				AddButton(10, 150, 4005, 4007, 3, GumpButtonType.Reply, 0);
			if( Banker.GetBalance(pm) >= 10000000 )
				AddButton(10, 170, 4005, 4007, 4, GumpButtonType.Reply, 0);

			int y = 60;
			
			for ( int i = 0; i < 1; i++)
			{
				//이름
				AddHtml( 50, y + i * 20, 225, 20, NameA[i], false, false);
				switch ( i )
				{
					case 0: //로그인
					{
						//달성량
						AddHtml( 275, y + i * 20, 75, 20, acc.LoginBonus.ToString() + "일", false, false);
						//보너스
						if( acc.LoginBonus <= 1 )
							AddHtml( 350, y + i * 20, 150, 20, Misc.Util.Equip_Login[0].ToString() + " 포인트", false, false);
						else
							AddHtml( 350, y + i * 20, 150, 20, Misc.Util.Equip_Login[acc.LoginBonus -1].ToString() + " 포인트", false, false);
						break;
					}
				}
			}

			AddHtml(50, 200, 500, 20, "업적 시스템 확인", false, false); // Storage
			int x = 0;
			y = 0;
			for ( int i = 0; i < Name.Length; i++)
			{
				if( i == 4 )
				{
					x = 0;
					y += 20;
				}
				AddHtml( 50 + x * 125, 220 + y, 200, 20, Name[i], false, false);
				AddButton( 10 + x * 125, 220 + y, 4005, 4007, i + 5, GumpButtonType.Reply, 0 );
				x++;
			}

			//가문 포인트 사용
			AddHtml(50, 270, 500, 20, "가문 포인트 구매", false, false); // Storage
			
			if( acc.CharacterSlotsBonus < 6 )
			{
				AddHtml( 50, 290, 500, 20, accProduct[0] + "(" + CharacterBuyPrice[acc.CharacterSlotsBonus].ToString() + ")", false, false);
				if( CharacterBuyPrice[acc.CharacterSlotsBonus] <= acc.Point[0] )
					AddButton( 10, 290, 4005, 4007, 15, GumpButtonType.Reply, 0 );
			}
			else
				AddHtml( 50, 290, 500, 20, "모든 케릭터 구매", false, false);
			int usePrice = ( acc.HouseSlotsBonus + 1 ) * 2000;
			
			AddHtml( 50, 310, 500, 20, accProduct[1] + "(" + usePrice.ToString() + ")", false, false);
			if( acc.HouseSlotsBonus < 5 + acc.CharacterSlotsBonus && usePrice <= acc.Point[0] )
				AddButton( 10, 310, 4005, 4007, 16, GumpButtonType.Reply, 0 );
			
			usePrice = ( acc.TeachingBonus + 1 ) * 500;

			AddHtml( 50, 330, 500, 20, accProduct[2] + "(" + usePrice.ToString() + ")", false, false);
			if( usePrice <= acc.Point[0] )
				AddButton( 10, 330, 4005, 4007, 17, GumpButtonType.Reply, 0 );
			
			for( int i = 0; i < goldProduct.Length; i++ )
			{
				if( i == 1 && acc.Lotto != 0 )
				{
					AddHtml(50, 360 + i * 20, 300, 20, "당신이 선택한 번호는" + acc.Lotto.ToString() + "입니다.", false, false); // Storage
				}
				else
				{
					AddHtml(50, 360 + i * 20, 500, 20, goldProduct[i], false, false); // Storage
					if( acc.Point[0] >= goldPrice[i] )
						AddButton( 10, 360 + i * 20, 4005, 4007, 18 + i, GumpButtonType.Reply, 0 );
				}					
			}
			LottoCheck lottocheck = null;
			if( Server.Event.lc == null )
			{
				foreach ( Item item in World.Items.Values )
				{
					if ( item is LottoCheck )
					{
						LottoCheck lc = item as LottoCheck;
						Server.Event.lc = lc;
						lottocheck = lc;
						break;
					}
				}				
				Console.WriteLine("Donation Respawn success");
			}
			else
				lottocheck = Server.Event.lc;
			if( lottocheck != null )
			{
				if( lottocheck.LottoNumber == 0 || lottocheck.LottoNumber == null )
					AddHtml(350, 380, 200, 20, "추첨 진행중입니다", false, false); // Storage
				else
					AddHtml(350, 400, 200, 20, "추첨 번호 : " + lottocheck.LottoNumber.ToString() , false, false); // Storage
				
			}
		}

		private void RandomCheck_Reward( PlayerMobile pm )
		{
			int Dice = Utility.RandomMinMax(1, 10000);
			int price = 0;
			if( Dice <= 25 )
				price = 1000000; //기대값 1000원
			else if( Dice <= 125 )
				price = 100000; //기대값 900원
			else if( Dice <= 1125 )
				price = 10000; //기대값 400원

			if( price > 0 )
			{
				Banker.Deposit( pm, price );

				if( price == 1000000 )
					World.Broadcast(0x0, false, "{0}님이 즉석 복권에서 100만 골드에 당첨되셨습니다!", m_pm.Name);
				else
					pm.SendMessage("즉석 복권에서 {0}골드를 획득합니다!", price.ToString());
			}
			else
				pm.SendMessage("운이 없네요...");
		}
		
		private void DonationSave( Account acc, int donation )
		{
			acc.DonationPoint += donation;
			acc.Point[0] += donation;
			
			if( acc.DonationPoint >= 100 )
			{
				DonationCheck check = null;
				if( Server.Event.dc == null )
				{
					foreach ( Item item in World.Items.Values )
					{
						if ( item is DonationCheck )
						{
							DonationCheck dc = item as DonationCheck;
							Server.Event.dc = dc;
							check = dc;
							break;
						}
					}
				}
				else
					check = Server.Event.dc;
				
				if( check != null )
				{
					for( int i = 0; i < check.DonationList.Length; i++ )
					{
						if( check.DonationList[i] == acc.Username )
							break;
						if( check.DonationList[i] == "" || check.DonationList[i] == null )
						{
							check.DonationList[i] = acc.Username;
							break;
						}
						if( check.DonationList[i] != "" )
						{
							foreach (Account a in Accounts.GetAccounts())
							{
								if( check.DonationList[i] == a.Username )
								{
									if( a.DonationPoint < acc.DonationPoint )
									{
										check.DonationList[i] = acc.Username;
										for( int j = i; j < check.DonationList.Length - 1; j++ ) 
										{
											if( check.DonationList[j] == "" || check.DonationList[i] == null )
												break;
											else
												check.DonationList[j + 1] = check.DonationList[j];
										}
										break;
									}
								}
							}
						}					
					}
				}
			}
		}
		
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;
			
			//m_pm.EquipPoint[0] -= Price[info.ButtonID - 1];
			
			if( info.ButtonID >= 1 && info.ButtonID < 5 )
			{
				Banker.Withdraw(m_pm, (int)Math.Pow(10, info.ButtonID + 3), true);
				Account acc = m_pm.Account as Account;
				DonationSave( acc, GivePoint[info.ButtonID - 1] );
				m_pm.SendGump(new EquipPointGump(m_pm));
			}		
			else if( info.ButtonID >= 5 && info.ButtonID < 12 )
			{
				//m_pm.CloseGump(typeof(SavingAccountGump));
				m_pm.SendGump(new SavingAccountGump(m_pm, info.ButtonID - 5));
			}
			else if( info.ButtonID == 15 )
			{
				Account acc = m_pm.Account as Account;
				acc.Point[0] -= CharacterBuyPrice[acc.CharacterSlotsBonus];
				acc.CharacterSlotsBonus++;
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if( info.ButtonID == 16 )
			{
				Account acc = m_pm.Account as Account;
				acc.Point[0] -= ( acc.HouseSlotsBonus + 1 ) * 2000;
				acc.HouseSlotsBonus++;
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if( info.ButtonID == 17 )
			{
				Account acc = m_pm.Account as Account;
				acc.Point[0] -= ( acc.TeachingBonus + 1 ) * 500;
				acc.TeachingBonus++;
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if( info.ButtonID >= 18 && info.ButtonID <= 22 )
			{
				Account acc = m_pm.Account as Account;
				if( info.ButtonID != 19 )
					acc.Point[0] -= goldPrice[info.ButtonID - 18];
					//Banker.Withdraw(m_pm, goldPrice[info.ButtonID - 18], true);
				switch ( info.ButtonID )
				{
					case 18:
					{
						RandomCheck_Reward( m_pm );
						break;
					}
					case 19:
					{
						if( acc.Lotto == 0 )
						{
							m_pm.SendMessage("1부터 9999 사이의 숫자를 적으세요!");
							m_pm.BeginPrompt(
							(from, text) =>
							{
								int amount = Utility.ToInt32(text);
								if( amount >= 1 && amount <= 10000 )
								{
									from.SendMessage(amount.ToString() + "번을 선택하셨습니다. 행운을 빕니다!" );
									acc.Lotto = amount;
									Banker.Withdraw(m_pm, 10000, true);
								}
								else
									from.SendMessage("잘못된 숫자나 문자를 넣으셨네요...");
							});
						}
						break;
					}
					case 20:
					{
						BagOfReagents br = new BagOfReagents ( 1000 );
						m_pm.AddToBackpack( br );
						m_pm.SendMessage("모든 시약을 1000개 획득합니다.");
						break;
					}
					case 21:
					{
						BagOfReagents br = new BagOfReagents ( 10000 );
						m_pm.AddToBackpack( br );
						m_pm.SendMessage("모든 시약을 10000개 획득합니다.");
						break;
					}
					case 22:
					{
						HouseTeleporterTileBag br = new HouseTeleporterTileBag(false);
						m_pm.AddToBackpack( br );
						m_pm.SendMessage("집 텔레포트를 획득합니다.");
						break;
					}
				}
				m_pm.SendGump(new EquipPointGump(m_pm));
			}
			else if( info.ButtonID == 100 )
			{
				m_pm.CloseGump(typeof(DonationRankingGump));
				m_pm.SendGump(new DonationRankingGump(m_pm));
			}
        }		
        private void SendGump()
        {
            m_pm.SendGump(new EquipPointGump(m_pm));
        }
	}
	#endregion
	
	#region 유물
    public class ArtifactPointGump : Gump
    {
        //private const int LabelColor = 0x7FFF;
        //private const int LabelHue = 0x481;

		private PlayerMobile m_pm;
		
		private string[] Name = { "힘 증가", "민첩성 증가", "지능 증가", "행운 증가", "체력 증가", "기력 증가", "마나 증가", "물리 데미지 증가", "마법 데미지 증가", "물리 데미지 감소", "마법 데미지 감소", "추종자 증가"};
		
        public ArtifactPointGump(PlayerMobile pm) : base(50, 50)
        {
			pm.CloseGump(typeof(CityPointGump));
			//pm.CloseGump(typeof(GoldPointGump));
			m_pm = pm;
			//pm.CloseGump(typeof(SilverPointGump));
			//pm.CloseGump(typeof(EquipPointGump));
			pm.CloseGump(typeof(ArtifactPointGump));
			//pm.CloseGump(typeof(SkillPointGump));

            AddPage(0);

            AddBackground(0, 0, 500, 350, 5054);
			
			//11 : 몬스터 유물 획득 체크
			
            //AddImageTiled(10, 10, 500, 150, 2624);
            //AddAlphaRegion(10, 10, 500, 150);

            AddHtml(10, 10, 250, 20, "유물 포인트 수정 중", false, false); // <CENTER>HOUSE 			
			/*
			AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

            AddHtml(50, 40, 225, 20, "이름", false, false); // House Description
            AddHtml(275, 40, 75, 20, "레벨", false, false); // Storage
            AddHtml(350, 40, 150, 20, "비용", false, false); // Lockdowns
			
            //AddImageTiled(10, 70, 500, 280, 2624);
            //AddAlphaRegion(10, 70, 500, 280);			

           // AddImageTiled(10, 970, 500, 20, 2624);
           // AddAlphaRegion(10, 970, 500, 20);

			int y = 60;
			for ( int i = 0; i < 12; i++)
			{
				int mul = 1;
				if( i == 11 )
					mul = 10;
				if( ( pm.ArtifactPoint[i + 1] + 1 ) * ( pm.ArtifactPoint[i + 1] + 1 ) * mul <= pm.ArtifactPoint[0] && pm.ArtifactPoint[i + 1] < 100 )
					AddButton(10, y + i * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				
				//이름
				AddHtml( 50, y + i * 20, 225, 20, Name[i], false, false);

				//레벨
				AddHtml( 275, y + i * 20, 75, 20, pm.ArtifactPoint[i + 1].ToString(), false, false);

				//비용
				if( pm.ArtifactPoint[ i + 1 ] < 100 )
					AddHtml( 350, y + i * 20, 150, 20, ( ( pm.ArtifactPoint[i + 1] + 1 ) * ( pm.ArtifactPoint[i + 1] + 1 ) * mul ).ToString(), false, false);
			}
			*/
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;
			
			int mul = 1;
			
			if( info.ButtonID == 12 )
				mul = 10;
			m_pm.ArtifactPoint[0] -= ( m_pm.ArtifactPoint[info.ButtonID] + 1 ) * ( m_pm.ArtifactPoint[info.ButtonID] + 1 ) * mul;
			m_pm.ArtifactPoint[info.ButtonID]++;
			m_pm.Delta(MobileDelta.Stat);
			m_pm.ProcessDelta();
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);

			//m_pm.CloseGump(typeof(SilverPointGump));
			//m_pm.SendGump(new SilverPointGump(m_pm));
        }		
        private void SendGump()
        {
            m_pm.SendGump(new ArtifactPointGump(m_pm));
        }
	}
	#endregion

	#region 스킬
    public class SkillPointGump : Gump
    {

		private PlayerMobile m_pm;
		
		private FirstSkillCheck fsc = null;
		private int price = 0;

		//private bool pagecheck = false;
	
        public SkillPointGump(PlayerMobile pm) : base(50, 50)
        {
			pm.CloseGump(typeof(CityPointGump));
			m_pm = pm;

			pm.CloseGump(typeof(SkillPointGump));
			
			foreach ( Item item in World.Items.Values )
			{
				if ( item is FirstSkillCheck )
				{
					fsc = item as FirstSkillCheck;
					break;
				}
			}			
			AddPage(0);
            

            AddBackground(0, 0, 810, 650, 5054);
			
            //AddImageTiled(10, 10, 500, 150, 2624);
            //AddAlphaRegion(10, 10, 500, 150);

            AddHtml(10, 10, 250, 20, "스킬 경험치", false, false); // <CENTER>HOUSE 			
			//AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", pm.ArtifactPoint[0]), false, false);

            AddHtml(300, 10, 75, 20, "스킬 총합: ", false, false); // <CENTER>HOUSE 
			AddHtml(380, 10, 100, 16, String.Format("{0:N1}", (double)pm.SkillsTotal * 0.1), false, false);

            AddHtml(50, 40, 100, 20, "이름", false, false); // House Description
            AddHtml(150, 40, 75, 20, "저장 상태", false, false); // House Description
            AddHtml(225, 40, 75, 20, "저장 스킬", false, false); // House Description
            AddHtml(300, 40, 175, 20, "상태", false, false); // Storage
			AddHtml(475, 40, 75, 20, "내 스킬", false, false);
            AddHtml(550, 40, 100, 20, "스킬 퍼센트", false, false); // Storage
            AddHtml(650, 40, 100, 20, "최대 스킬", false, false); // Storage
            //AddHtml(750, 40, 50, 20, "구매", false, false); // Storage

			//AddButton(100, 10, 4005, 4007, 100, GumpButtonType.Reply, 0);

			int y = 60;
			int page = pm.SkillGumpPage;
			int index = 0;
			//pm.skillpage = false;
			//AddButton( 130, 10, 4005, 4007, 100, GumpButtonType.Reply, 0);
			Account acc = pm.Account as Account;
	
			for ( int i = 0 + page * 29; i < 29 + page * 29; i++)
			{
				if( page == 0 )
				{
					AddButton(750, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				else
				{
					AddButton(750, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}
				//double AccountSkillSum = 0.0;
				double AccountSkillBest = 0.0;

				//가문 스킬 체크
				if( acc.Count > 1 )
				{
					for (int j = 0; j < acc.Length; ++j)
					{
						Mobile check = acc[j];
						if (check != null && check != pm )
						{
							if( AccountSkillBest < check.Skills[i].Base )
								AccountSkillBest = check.Skills[i].Base;

							//AccountSkillSum += check.Skills[i].Base * 0.1;
						}
					}
				}

				//스킬 저장 || 로드 버튼
				if( ( acc.Point[i + 801] == 0 && pm.Skills[i].Base != 0 ) || ( acc.Point[i + 801] != 0 && acc.Point[i + 801] > pm.Skills[i].Base && pm.SkillsTotal + acc.Point[i + 801] <= 15000 ) )
					AddButton(10, y + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				
				//이름
				AddHtml( 50, y + index * 20, 100, 20, fsc.SkillName[i], false, false);

				//저장 상태 & 저장 스킬
				string save = "없음";
				string skill = "없음";
				if( acc.Point[i + 801] != 0 )
				{
					save = "저장 됨";
					skill = ( acc.Point[i + 801] * 0.1 ).ToString();
				}
				AddHtml( 150, y + index * 20, 75, 20, save, false, false);
				AddHtml( 225, y + index * 20, 75, 20, skill, false, false);
				
				//상태
				string skillnow = ((int)(pm.SkillList[i])).ToString();
				if( pm.SkillList[i] >= 1000 )
					skillnow = string.Format("{0:#,###}", pm.SkillList[i]);
				skillnow += " / " + string.Format("{0:#,###}", Misc.Util.SkillExp_Calc(pm, i));
				AddHtml( 300, y + index * 20, 175, 20, skillnow, false, false );
				
				//내 스킬
				AddHtml( 475, y + index * 20, 75, 20, pm.Skills[i].Base.ToString(), false, false);
				
				//스킬 퍼센트
				AddHtml( 550, y + index * 20, 100, 20, string.Format("{0:N2}%", SkillPercent(pm, i)), false, false);

				//가문 최고 스킬
				AddHtml( 650, y + index * 20, 150, 20, AccountSkillBest.ToString(), false, false);

				index++;
			}			

		}

		
		private double SkillPercent(PlayerMobile pm, int skill )
		{
			double skillcalc = pm.SkillList[skill] * 100 / Misc.Util.SkillExp_Calc(pm, skill);
			return skillcalc > 100 ? 100 : skillcalc;
		}
		
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			if ( info.ButtonID == 100 )
			{
				
			}
			else if( info.ButtonID == 99 )
			{
				m_pm.SkillGumpPage = 0;
				m_pm.SendGump(new SkillPointGump(m_pm));
			}
			else if( info.ButtonID == 98 )
			{
				m_pm.SkillGumpPage = 1;
				m_pm.SendGump(new SkillPointGump(m_pm));
			}
			else
			{
				Account acc = m_pm.Account as Account;
				if( acc.Point[info.ButtonID + 800] == 0 && m_pm.Skills[info.ButtonID - 1].Base != 0 )
				{
					acc.Point[info.ButtonID + 800] = (int)( m_pm.Skills[info.ButtonID - 1].Base * 10 );
					m_pm.Skills[info.ButtonID - 1].Base = 0;
				}
				else if( acc.Point[info.ButtonID + 800] != 0 && acc.Point[info.ButtonID + 800] > m_pm.Skills[info.ButtonID - 1].Base )
				{
					m_pm.Skills[info.ButtonID - 1].Base = acc.Point[info.ButtonID + 800] * 0.1;
					acc.Point[info.ButtonID + 800] = 0;
				}
				m_pm.SendGump(new SkillPointGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
        }
        private void SendGump()
        {
            m_pm.SendGump(new SkillPointGump(m_pm));
        }
	}
	#endregion
    public class HarvestGump : Gump
    {
		PlayerMobile m_pm;
		int m_maxpage = 1;
		private string[] HarvestName =
		{
			"철 채집", "구리 채집", "청동 채집", "금 채집", "아가파이트 채집", "베라이트 채집", "벨러라이트 채집", "미스릴 채집", "옵시디언 채집", "모래 채집", "일반 나무 채집", "떡갈 나무 채집", "물푸레 나무 채집", "주목 나무 채집", "심재 나무 채집", "피 나무 채집", "서리 나무 채집", "칠흑 나무 채집", "영목 나무 채집", "", "일반 가죽 채집", "질긴 가죽 채집", "거친 가죽 채집", "경화 가죽 채집", "가시 가죽 채집", "뿔 가죽 채집", "미늘 가죽 채집", "극지 가죽 채집", "흑단 가죽 채집", "", "송어 채집", "배스 채집", "은어 채집", "붕어 채집", "메기 채집", "대구 채집", "농어 채집", "청어 채집", "참치 채집", ""
		};
	
		int[] AllCount = { 1, 5, 10, 20, 30, 50, 75, 100, 140, 200 };
		int[] HarvestStartNumber = {2, 12, 22, 82};

        public HarvestGump(PlayerMobile pm) : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 620, 270, 5054);
            AddImageTiled(10, 10, 600, 250, 2624);
			
			AddHtml(0, 20, 620, 20, CenterGray("채집 채취 수"), false, false); // <CENTER>HOUSE
			
			m_pm = pm;
			pm.CloseGump(typeof(HarvestGump));
			
			Account acc = pm.Account as Account;
			int page = pm.HarvestGumpPage;
			int index = 0;
			int maxlist = HarvestName.Length;
			int step = 10;
			m_maxpage = maxlist / step;
			int maxpage = Misc.Util.MaxpageCreate(maxlist, page, step);
			
			for ( int i = page * step; i < maxpage + page * step; i++)
			{
				if( page < m_maxpage -1 )
				{
					AddButton(480, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				if( page > 0 )
				{
					AddButton(450, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}
				
				AddHtml(50, 50 + index * 20, 200, 20, LeftGreen(HarvestName[i]), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				if( i == 9 )
					AddHtml(250, 50 + index * 20, 200, 20, LeftGreen("누적 : " + acc.Point[90].ToString()), false, false); // <DIV
				else if( i == 19 || i == 29 || i == 39 )
				{
					continue;
				}//AddHtml(250, 50 + index * 20, 200, 20, LeftGreen("누적 : 0", false, false)); // <DIV
				else
					AddHtml(250, 50 + index * 20, 200, 20, LeftGreen("누적 : " + acc.Point[HarvestStartNumber[page] + index].ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				//10일 때 400 11일 때 600 13일 때 1000, 이후 5단위로 1000. 
				int nextCount = 200;
				if( pm.HarvestPoint[i] < 10 )
					nextCount = AllCount[pm.HarvestPoint[i]];
				else if( pm.HarvestPoint[i] < 310 )
					nextCount = ( pm.HarvestPoint[i] - 9 ) * 200;
				else if( pm.HarvestPoint[i] < 400 )
					nextCount = ( pm.HarvestPoint[i] - 250 ) * 1000;
				else if( pm.HarvestPoint[i] < 485 )
					nextCount = ( pm.HarvestPoint[i] - 483 ) * 10000;
				else if( pm.HarvestPoint[i] < 494 )
					nextCount = ( pm.HarvestPoint[i] - 492 ) * 1000000;
				else
					nextCount = ( pm.HarvestPoint[i] + 1 ) * 10000000;
					
				AddHtml(450, 50 + index * 20, 200, 20, LeftGreen("다음 : " + nextCount.ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				if( index == 9 )
				{
					if( i == 9 && acc.Point[90] >= nextCount )
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
					else if( i == 19 && acc.Point[91] >= nextCount )
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
					else if( i == 29 && acc.Point[92] >= nextCount )
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
					else if( i == 39 && acc.Point[93] >= nextCount )
						AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				}
				else if( acc.Point[HarvestStartNumber[page] + index] >= nextCount )
					AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				index++;
			}
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			if( info.ButtonID == 99 )
			{
				if( m_pm.HarvestGumpPage > 0 )
					m_pm.HarvestGumpPage--;
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			else if( info.ButtonID == 98 )
			{
				if( m_pm.HarvestGumpPage < m_maxpage )
					m_pm.HarvestGumpPage++;
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			else
			{
				//
				m_pm.HarvestPoint[info.ButtonID - 1]++;
				Misc.Util.HarvestReward(m_pm, info.ButtonID - 1);
				m_pm.SendGump(new HarvestGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
        }
		
        private void SendGump()
        {
            m_pm.SendGump(new HarvestGump(m_pm));
        }
		
        private string CenterGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
        }
        private string LeftGreen(string format)
        {
            return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
        }
    }
	#region Crafting
    public class CraftingGump : Gump
    {
		PlayerMobile m_pm;
		int m_maxpage = 1;
		private string[] CraftName = 
			{ "포션 제작", "대장장이 제작", "활&석궁 제작", "목수 제작", "지도 제작", "요리 제작", "스크롤 제작", "재봉 제작", "기계공 제작", "임뷰잉 제작" };
		
		int[] AllCount = { 1, 5, 10, 20, 30, 50, 75, 100, 140, 200 };
        public CraftingGump(PlayerMobile pm) : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 620, 270, 5054);
            AddImageTiled(10, 10, 600, 250, 2624);
			
			AddHtml(0, 20, 620, 20, CenterGray("제작 생산 수"), false, false); // <CENTER>HOUSE

			m_pm = pm;
			
			pm.CloseGump(typeof(CraftingGump));
			Account acc = pm.Account as Account;
			int page = pm.CraftGumpPage;
			int index = 0;
			int maxlist = CraftName.Length;
			int step = 10;
			//m_maxpage = 1 + maxlist / step;
			int maxpage = Misc.Util.MaxpageCreate(maxlist, page, step);
			
			for ( int i = 0; i < 10; i++)
			{
				/*
				if( page < m_maxpage -1 )
				{
					AddButton(480, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				if( page > 0 )
				{
					AddButton(450, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}
				*/
				AddHtml(50, 50 + i * 20, 200, 20, LeftGreen(CraftName[i]), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(250, 50 + i * 20, 200, 20, LeftGreen("누적 : " + acc.Point[31 + i].ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				/*
				int nextCount = 200;
				if( pm.CraftPoint[i] < 10 )
					nextCount = AllCount[pm.CraftPoint[i]];
				else
					nextCount = ( pm.CraftPoint[i] - 9 ) * 200;
				*/

				int nextCount = 200;
				if( pm.CraftPoint[i] < 10 )
					nextCount = AllCount[pm.CraftPoint[i]];
				else if( pm.CraftPoint[i] < 310 )
					nextCount = ( pm.CraftPoint[i] - 9 ) * 200;
				else if( pm.CraftPoint[i] < 400 )
					nextCount = ( pm.CraftPoint[i] - 250 ) * 1000;
				else if( pm.CraftPoint[i] < 485 )
					nextCount = ( pm.CraftPoint[i] - 483 ) * 10000;
				else if( pm.CraftPoint[i] < 494 )
					nextCount = ( pm.CraftPoint[i] - 492 ) * 1000000;
				else
					nextCount = ( pm.CraftPoint[i] + 1 ) * 10000000;
			
				AddHtml(450, 50 + index * 20, 200, 20, LeftGreen("다음 : " + nextCount.ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>
				
				if( acc.Point[31 + i] >= nextCount )
					AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				index++;
			}
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			/*
			if( info.ButtonID == 99 )
			{
				if( m_pm.CraftGumpPage > 0 )
					m_pm.CraftGumpPage--;
			}
			else if( info.ButtonID == 98 )
			{
				if( m_pm.CraftGumpPage < m_maxpage )
					m_pm.CraftGumpPage++;
			}
			*/
			m_pm.CraftPoint[info.ButtonID - 1]++;
			Misc.Util.CraftReward(m_pm, info.ButtonID - 1);
            m_pm.SendGump(new CraftingGump(m_pm));
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
        }
        private void SendGump()
        {
            m_pm.SendGump(new CraftingGump(m_pm));
        }
		
        private string CenterGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
        }
        private string LeftGreen(string format)
        {
            return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
        }
    }		
	
	#endregion
	
	public class MonsterFeatGump : Gump
	{
        private string CenterGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
        }

        private string RightGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=RIGHT>{0}</DIV>", format);
        }

        private string LeftGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=LEFT>{0}</DIV>", format);
        }

        private string LeftGreen(string format)
        {
            return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
        }
		
		int[] KillCount = { 1, 3, 5, 10, 15, 20, 30, 50, 75, 100 };
		
		PlayerMobile m_pm;
		int m_maxpage = 1;
        public MonsterFeatGump(PlayerMobile pm) : base(50, 50)
        {
			m_pm = pm;
            AddPage(0);
            AddBackground(0, 0, 620, 270, 5054);
            AddImageTiled(10, 10, 600, 250, 2624);
			
			AddHtml(0, 20, 620, 20, CenterGray("몬스터 사망 수"), false, false); // <CENTER>HOUSE

			pm.CloseGump(typeof(MonsterFeatGump));
			Account acc = pm.Account as Account;
			int page = pm.MonsterFeatGumpPage;
			int index = 0;
			int maxlist = Misc.Util.m_MonsterItemDrop.GetLength(0);
			m_maxpage = 1 + maxlist / 10;
			int step = 10;
			int maxpage = Misc.Util.MaxpageCreate(maxlist, page, step);
			
			for ( int i = page * step; i < maxpage + page * step; i++)
			{
				if( page < m_maxpage -1 )
				{
					AddButton(480, 10, 4005, 4007, 98, GumpButtonType.Reply, 0);
				}
				if( page > 0 )
				{
					AddButton(450, 10, 4014, 4016, 99, GumpButtonType.Reply, 0);
				}
		
				AddHtmlLocalized(50, 50 + index * 20, 200, 20, 1052085 + i, "", 0x00FA9A, false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(250, 50 + index * 20, 200, 20, LeftGreen("누적 : " + acc.Point[i + 201].ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				int nextCount = 100;
				if( pm.MonsterPoint[i] < 10 )
					nextCount = KillCount[pm.MonsterPoint[i]];
				else if( pm.MonsterPoint[i] < 310 )
					nextCount = ( pm.MonsterPoint[i] - 10 ) * 100;
				else if( pm.MonsterPoint[i] < 400 )
					nextCount = ( pm.MonsterPoint[i] - 250 ) * 500;
				else if( pm.MonsterPoint[i] < 485 )
					nextCount = ( pm.MonsterPoint[i] - 483 ) * 5000;
				else if( pm.MonsterPoint[i] < 494 )
					nextCount = ( pm.MonsterPoint[i] - 492 ) * 500000;
				else
					nextCount = ( pm.MonsterPoint[i] + 1 ) * 5000000;

					
				AddHtml(450, 50 + index * 20, 200, 20, LeftGreen("다음 : " + nextCount.ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>
				
				if( acc.Point[i + 201] >= nextCount )
					AddButton(10, 50 + index * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);

				index++;
			}
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive() || info.ButtonID == 0)
                return;

			if( info.ButtonID == 99 )
			{
				if( m_pm.MonsterFeatGumpPage > 0 )
					m_pm.MonsterFeatGumpPage--;
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			else if( info.ButtonID == 98 )
			{
				if( m_pm.MonsterFeatGumpPage < m_maxpage )
					m_pm.MonsterFeatGumpPage++;
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			else
			{
				m_pm.MonsterPoint[info.ButtonID - 1]++;
				Misc.Util.MonsterFeatReward(m_pm, info.ButtonID - 1);
				m_pm.SendGump(new MonsterFeatGump(m_pm));
			}
			//Timer.DelayCall(TimeSpan.FromSeconds(0.1), SendGump);
        }
        private void SendGump()
        {
            m_pm.SendGump(new MonsterFeatGump(m_pm));
        }
	}
	
    public class SavingAccountGump : Gump
    {
		private string[,] Name =
		{
			{ "광물 채집", "철 채집", "구리 채집", "청동 채집", "금 채집", "아가파이트 채집", "베라이트 채집", "벨러라이트 채집", "미스릴 채집", "옵시디언 채집" },
			{ "나무 채집", "일반 나무 채집", "떡갈 나무 채집", "물푸레 나무 채집", "주목 나무 채집", "심재 나무 채집", "피 나무 채집", "서리 나무 채집", "칠흑 나무 채집", "영목 나무 채집" },
			{ "가죽 채집", "일반 가죽 채집", "질긴 가죽 채집", "거친 가죽 채집", "경화 가죽 채집", "가시 가죽 채집", "뿔 가죽 채집", "미늘 가죽 채집", "극지 가죽 채집", "흑단 가죽 채집" },
			{ "포션 제작", "대장장이 제작", "활&석궁 제작", "목수 제작", "지도 제작", "요리 제작", "스크롤 제작", "재봉 제작", "기계공 제작", "임뷰잉 제작" },
			{ "1,000", "2,000", "3,000", "4,000", "5,000", "6,000", "7,000", "8,000", "9,000", "10,000" },
			{ "11,000", "12,000", "13,000", "14,000", "15,000", "16,000", "17,000", "18,000", "19,000", "20,000" },
			{ "21,000", "22,000", "23,000", "24,000", "25,000", "26,000", "27,000", "28,000", "29,000", "30,000" }
		};
        public SavingAccountGump(PlayerMobile pm, int list) : base(50, 50)
        {
            AddPage(0);
            AddBackground(0, 0, 620, 270, 5054);
            AddImageTiled(10, 10, 600, 250, 2624);
			
			AddHtml(0, 20, 620, 20, CenterGray("업적 시스템"), false, false); // <CENTER>HOUSE
			pm.CloseGump(typeof(SavingAccountGump));

			Account acc = pm.Account as Account;
			
			for( int i = 0; i < Name.GetLength(1); i++ )
			{
				AddHtml(20, 50 + i * 20, 200, 20, LeftGreen(Name[list,i]), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(220, 50 + i * 20, 200, 20, LeftGreen("누적 : " + acc.Point[list * 10 + i + 1].ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>

				AddHtml(420, 50 + i * 20, 200, 20, LeftGreen("다음 : " + Math.Pow(acc.Point[500 + list * 10 + i + 1] + 1, 2).ToString()), false, false); // <DIV ALIGN=LEFT>Name:</DIV>
			}
			
		}

		private string PointText( int ranking, string point ) 
		{
			if( point == "0" )
				point = "없음";
			
			else
				point += "점";
			
			if( ranking == 0 )
				return String.Format("<basefont color=#FF0090><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if( ranking == 1 )
				return String.Format("<basefont color=#FFB400><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if( ranking == 2 )
				return String.Format("<basefont color=#B36BFF><DIV ALIGN=CENTER>{0}</DIV>", point);
			else if( ranking == 3 || ranking == 4 )
				return String.Format("<basefont color=#68D5ED><DIV ALIGN=CENTER>{0}</DIV>", point);
			else
				return String.Format("<basefont color=#00A000><DIV ALIGN=CENTER>{0}</DIV>", point);
		}

		
        private string CenterGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=CENTER>{0}</DIV>", format);
        }

        private string RightGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=RIGHT>{0}</DIV>", format);
        }

        private string LeftGray(string format)
        {
            return String.Format("<basefont color=#A9A9A9><DIV ALIGN=LEFT>{0}</DIV>", format);
        }

        private string LeftGreen(string format)
        {
            return String.Format("<basefont color=#00FA9A><DIV ALIGN=LEFT>{0:#,###}</DIV>", format);
        }
    }	
}
