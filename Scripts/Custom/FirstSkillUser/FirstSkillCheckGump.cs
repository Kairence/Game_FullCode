using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
//using Server.Engines.CityLoyalty;

namespace Server.Gumps
{
    public class FirstSkillCheckGump : Gump
    {
		//private const int LabelColor = 0x7FFF;
        //private const int LabelHue = 0x481;

		private string[] Name = { "연금술", "해부학", "동물지식", "아이템 감정", "장비학", "방패술", "구걸", "대장장이", "활제작술", "평화유지", "야영술", "목공술", "지도제작술", "요리", "은신감지", "불협화음", "지능평가", "치료술", "낚시", "법의학", "허딩", "은신하기", "도발연주", "기록술", "자물쇠 따기", "마법학", "마법 저항", "전술", "훔쳐보기", "음악연주", "중독술", "궁술", "영혼대화", "훔치기", "재봉술", "길들이기", "맛 감정", "기계공 기술", "추적술", "수의학", "검술", "둔기술", "펜싱", "레슬링", "벌목술", "채광", "명상", "은신이동", "함정제거", "강령술", "집중", "기사도", "무사도", "닌자술", "주문조합", "신비술", "임뷰잉", "쓰로잉" };

		private FirstSkillCheck m_firstskill = null;
		private Mobile m_From = null;
        public FirstSkillCheckGump(Mobile from, FirstSkillCheck firstskill) : base(50, 50)
        {
			m_From = from;
			m_firstskill = firstskill;
			from.CloseGump(typeof(FirstSkillCheckGump));

            AddBackground(0, 0, 500, 480, 5054);

            AddHtml(10, 10, 250, 20, "스킬 체크 시스템", false, false); // <CENTER>HOUSE 			
			//AddHtml(130, 10, 200, 16, String.Format("{0:#,###}", dungeon.Death[0]), false, false);

            AddHtml(50, 40, 225, 20, "스킬", false, false); // House Description
            AddHtml(275, 40, 75, 20, "유저", false, false); // Storage
            //AddHtml(350, 40, 150, 20, "사망", false, false); // Lockdowns			
			int y = 60;
			for ( int i = 0; i < Name.Length; i++ )
			{
				//이름
				AddHtml( 50, y + i * 20, 225, 20, Name[i], false, false);
				//유저
				string name = "없음";
				if( firstskill.Skill[i] )
				{
					name = firstskill.User[i];
					AddButton(10, y + i * 20, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				}
				AddHtml(350, y + i * 20, 200, 16, name, false, false);
				//사망
				//AddHtml(350, y + i * 20, 200, 16, String.Format("{0:#,###}", dungeon.Death[i + 1]), false, false);
			}
		}
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_From.CheckAlive() || info.ButtonID == 0)
                return;
			
			m_firstskill.Skill[info.ButtonID - 1] = false;
		}
	}
}
