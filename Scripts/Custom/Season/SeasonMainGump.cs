using System;
using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
	public class SeasonMainGump : Gump
	{
		public static void Initialize()
		{
			CommandSystem.Register("시즌", AccessLevel.Player, e => e.Mobile.SendGump(new SeasonMainGump(e.Mobile)));
		}

		public SeasonMainGump(Mobile from)
			: base(150, 150)
		{
			Closable = true;
			Disposable = true;
			Dragable = true;

			AddPage(0);

			// 1. 메인 배경
			AddBackground(0, 0, 420, 560, 9270);

			// 2. 상단 헤더 (Dashboard)
			AddImageTiled(20, 20, 380, 50, 2624);
			AddHtml(
				20,
				32,
				380,
				25,
				"<BASEFONT SIZE=6 COLOR=#FFD700><CENTER>SEASON DASHBOARD</CENTER></BASEFONT>",
				false,
				false
			);

			// 3. 메뉴 버튼들
			DrawLargeMenuButton(60, 120, 1, 0x15A4, "자원 채취 업적", "금속 / 나무 / 가죽");
			DrawLargeMenuButton(60, 250, 2, 0x15A1, "몬스터 토벌 기록", "필드 및 던전");
			DrawLargeMenuButton(60, 380, 3, 0x159E, "스킬 마스터리 업적", "스킬 상승 경쟁");

			// 4. 하단 시즌 정보 및 캐릭터 확인
			AddImageTiled(20, 500, 380, 45, 2624);

			// 시즌 기간 표시
			AddLabel(40, 512, 1153, "SEASON 1 : 1일 ~ 25일");

			// [추가] 시즌 캐릭터 정보 확인 로직
			string youngCharName = "없음";
			int labelColor = 0x22; // 기본 빨간색 (없을 때)

			IAccount acct = from.Account;
			if (acct != null)
			{
				for (int i = 0; i < acct.Length; ++i)
				{
					Mobile m = acct[i];
					if (m is PlayerMobile pm && pm.Young)
					{
						youngCharName = pm.Name;
						labelColor = 0x42; // 초록색 (있을 때)
						break;
					}
				}
			}

			AddLabel(230, 512, 1153, "시즌 캐릭터 :");
			AddLabel(325, 512, labelColor, youngCharName);
		}

		private void DrawLargeMenuButton(int x, int y, int buttonID, int iconID, string title, string subTitle)
		{
			AddButton(x, y, 0x0918, 0x0919, buttonID, GumpButtonType.Reply, 0);
			AddHtml(x + 100, y + 0, 220, 25, $"<BASEFONT SIZE=6 COLOR=#FFFFFF>{title}</BASEFONT>", false, false);
			AddHtml(x + 102, y + 30, 220, 20, $"<BASEFONT SIZE=4 COLOR=#FFD700>{subTitle}</BASEFONT>", false, false);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;

			switch (info.ButtonID)
			{
				case 1:
					from.SendGump(new ResourceAchievementGump(from));
					break;
				case 2:
					from.SendGump(new MonsterDropHandlerGump(from, 0));
					break;
				case 3:
					from.SendGump(new SkillAchievementGump(from));
					break;
			}
		}
	}
}
