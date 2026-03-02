using System;
using Server.Accounting;

namespace Server.Misc
{
	public class SeasonCount
	{
		public const int MaxGoal = 100000000; // 개별 1억
		public const int TotalMaxGoal = 1000000000; // 통합 10억

		// 검프 연동 상수 (기획서 반영)
		public const int MetalTotal = 1;
		public const int MetalStart = 2;
		public const int WoodTotal = 11;
		public const int WoodStart = 12;
		public const int LeatherTotal = 21;
		public const int LeatherStart = 22;
		public const int FishTotal = 31;
		public const int FishStart = 32;

		public static void OnCount(Mobile from, int index, int amount)
		{
			Account acct = from.Account as Account;

			if (acct == null || acct.Point == null || index < 1 || index >= 1000)
				return;

			// 1. 시즌 포인트 업데이트 (1~400)
			Update(from, acct, index, amount);

			// 2. 계정 공동 포인트 업데이트 (시즌 번호 + 400 대칭 저장)
			// 몬스터 킬 수(400번)를 포함하여 400 이하의 모든 포인트는 +400하여 저장
			if (index <= 400 && (index + 400) < acct.Point.Length)
			{
				Update(from, acct, index + 400, amount);
			}
		}

		private static void Update(Mobile from, Account acct, int idx, int amount)
		{
			// 통합 인덱스 판별: 1, 11, 21, 31, 41, 400 (시즌) / 401, 411, 421, 431, 441, 800 (공동)
			bool isTotal =
				(idx % 10 == 1 && idx <= 51) || idx == 400 || (idx % 10 == 1 && idx >= 401 && idx <= 451) || idx == 800;

			int limit = isTotal ? TotalMaxGoal : MaxGoal;

			if (acct.Point[idx] >= limit)
				return;

			if ((acct.Point[idx] += amount) >= limit)
			{
				acct.Point[idx] = limit;

				// 시즌 포인트 영역 알림
				if (idx >= 1 && idx <= 400)
				{
					from.SendMessage(
						0x42,
						isTotal ? $"[시즌 대업적] {idx}번 카테고리 달성!" : $"[시즌 업적] {idx}번 항목 달성!"
					);
				}
			}
		}
	}
}
