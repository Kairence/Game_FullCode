using System;
using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
	public class HitLocationManager
	{
		/// <summary>
		/// 플레이어 피격 시 각 부위별 확률에 따라 피격 지점을 결정합니다.
		/// (손: 10%, 머리: 10%, 목: 5%, 어깨: 15%, 하의: 25%, 상의: 35%)
		/// </summary>
		public static int GetRandomLocation()
		{
			double roll = Utility.RandomDouble();

			if (roll < 0.10) return 1;      // 손 (10%)
			else if (roll < 0.20) return 2; // 머리 (10%)
			else if (roll < 0.25) return 3; // 목 (5%)
			else if (roll < 0.40) return 4; // 어깨 (15%)
			else if (roll < 0.65) return 5; // 하의 (25%)
			else return 6;                 // 상의 (35%)
		}

		/// <summary>
		/// 부위별 치명타 확률 보너스를 반환합니다.
		/// </summary>
		public static double GetCritChanceBonus(int location)
		{
			switch (location)
			{
				case 1: return 0.10; // 손 10%
				case 2: return 0.25; // 머리 25%
				case 3: return 0.50; // 목 50%
				case 4: return 0.15; // 어깨 15%
				case 5: return 0.20; // 하의 20%
				case 6: return 0.20; // 상의 20%
				default: return 0.0;
			}
		}

		/// <summary>
		/// 부위별 치명타 추가 데미지 보너스를 반환합니다.
		/// </summary>
		public static double GetCritDamageBonus(int location)
		{
			switch (location)
			{
				case 2: return 0.50; // 머리 +50%
				case 3: return 1.50; // 목 +150%
				default: return 0.0; // 나머지 0%
			}
		}
		
		/// <summary>
		/// 부위 번호를 문자열로 변환합니다. (디버깅/메시지용)
		/// </summary>
		public static string GetLocationName(int location)
		{
			switch (location)
			{
				case 0: return "방패";
				case 1: return "손";
				case 2: return "머리";
				case 3: return "목";
				case 4: return "어깨";
				case 5: return "하의";
				case 6: return "상의";
				default: return "알 수 없음";
			}
		}
	}
}