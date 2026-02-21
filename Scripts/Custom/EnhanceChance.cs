using System;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class EnhancedChance
    {
		// [부위][인덱스][0:옵션ID, 1:가산수치]
        // 제공해주신 switch-case 번호를 기반으로 기획서와 1:1 매칭 완료
        private static readonly int[][][] m_EnhanceTable = new int[][][]
        {
            new int[][] // 0: 무기
            { 
                // 금속: 철(7:무피), 구리(40:공속), 청동(26:에너지피해), 금(3:운), 아가(23:화염피해), 베라(25:독피해), 벨러(24:냉기피해)
                new int[] { 7, 37500 }, new int[] { 40, 75000 }, new int[] { 26, 37500 }, new int[] { 3, 300000 }, new int[] { 23, 37500 }, new int[] { 25, 37500 }, new int[] { 24, 37500 },
                // 나무: 나무(7:무피), 떡갈(5:기력), 물푸레(40:공속), 주목(3:운), 심재(44:물리치명피해), 피나무(37:체력흡수), 서리(42:물리치명확률)
                new int[] { 7, 37500 }, new int[] { 5, 600000 }, new int[] { 40, 75000 }, new int[] { 3, 300000 }, new int[] { 44, 30000 }, new int[] { 37, 15000 }, new int[] { 42, 15000 },
                // 가죽: 가죽(8:주문피해), 질긴(6:마나), 거친(45:마법치명피해), 경화(3:운), 가시(24:냉기피해), 뿔(23:화염피해), 미늘(26:에너지피해)
                new int[] { 8, 37500 }, new int[] { 6, 600000 }, new int[] { 45, 30000 }, new int[] { 3, 300000 }, new int[] { 24, 37500 }, new int[] { 23, 37500 }, new int[] { 26, 37500 }
            },
            new int[][] // 1: 방어구
            { 
                // 금속: 철(12:물저), 구리(41:시전속도), 청동(16:에저), 금(3:운), 아가(13:화저), 베라(15:독저), 벨러(14:냉저)
                new int[] { 12, 10000 }, new int[] { 41, 25000 }, new int[] { 16, 10000 }, new int[] { 3, 100000 }, new int[] { 13, 10000 }, new int[] { 15, 10000 }, new int[] { 14, 10000 },
                // 나무: 나무(4:체력), 떡갈(8:주문피해), 물푸레(7:무기피해), 주목(3:운), 심재(20:기력회복), 피나무(19:체력회복), 서리(21:마나회복)
                new int[] { 4, 200000 }, new int[] { 8, 12500 }, new int[] { 7, 12500 }, new int[] { 3, 100000 }, new int[] { 20, 5000 }, new int[] { 19, 5000 }, new int[] { 21, 5000 },
                // 가죽: 가죽(40:공속), 질긴(6:마나), 거친(45:마법치명피해), 경화(3:운), 가시(8:주문피해), 뿔(43:마법치명확률), 미늘(41:시전속도)
                new int[] { 40, 25000 }, new int[] { 6, 200000 }, new int[] { 45, 10000 }, new int[] { 3, 100000 }, new int[] { 8, 12500 }, new int[] { 43, 5000 }, new int[] { 41, 25000 }
            },
            new int[][] // 2: 악세사리
            { 
                // 금속: 철(4:체력), 구리(5:기력), 청동(6:마나), 금(3:운), 아가(0:힘), 베라(1:민첩), 벨러(2:지능)
                new int[] { 4, 400000 }, new int[] { 5, 400000 }, new int[] { 6, 400000 }, new int[] { 3, 200000 }, new int[] { 0, 2000 }, new int[] { 1, 2000 }, new int[] { 2, 2000 }
            }
        };
		public static readonly double[][] EnhanceScales = new double[][]
		{
			// { 단계별 상승률, 0강부터의 누적 합계 }
			new double[] { 0.0, 0.0 },   // 0강
			new double[] { 1.0, 1.0 },   // 1강 (3.75%)
			new double[] { 1.1, 2.1 },   // 2강
			new double[] { 1.2, 3.3 },   // 3강
			new double[] { 1.4, 4.7 },   // 4강
			new double[] { 1.6, 6.3 },   // 5강
			new double[] { 1.9, 8.2 },   // 6강
			new double[] { 2.3, 10.5 },  // 7강
			new double[] { 2.8, 13.3 },  // 8강
			new double[] { 3.2, 16.5 },  // 9강
			new double[] { 3.5, 20.0 }   // 10강 (최종 75%)
		};
		public static readonly int[] MaterialCosts = new int[]
		{
			10, 20, 40, 80, 160, 320, 640, 1280, 2560, 5120, 10240
		};
		public static readonly Type[] ResourceTypes = new Type[]
        {
            typeof(IronIngot), typeof(CopperIngot), typeof(BronzeIngot), typeof(GoldIngot), typeof(AgapiteIngot), typeof(VeriteIngot), typeof(ValoriteIngot),
            typeof(Leather), typeof(DernedLeather), typeof(RatnedLeather), typeof(SernedLeather), typeof(SpinedLeather), typeof(HornedLeather), typeof(BarbedLeather),
            typeof(Board), typeof(OakBoard), typeof(AshBoard), typeof(YewBoard), typeof(HeartwoodBoard), typeof(BloodwoodBoard), typeof(FrostwoodBoard)
        };	
		public static int GetOptionID(int part, int index)
		{
			if (part >= 0 && part < m_EnhanceTable.Length)
			{
				if (index >= 0 && index < m_EnhanceTable[part].Length)
				{
					// [0]번 인덱스가 옵션 ID (7, 40 등)
					return m_EnhanceTable[part][index][0];
				}
			}
			return 0;
		}		
		public static int GetTableValue(int part, int index)
		{
			// 배열의 범위를 벗어나지 않는지 체크 (방어적 코딩)
			if (part >= 0 && part < m_EnhanceTable.Length)
			{
				if (index >= 0 && index < m_EnhanceTable[part].Length)
				{
					// [0]은 옵션 ID이고, [1]이 우리가 필요한 '수치'입니다.
					return m_EnhanceTable[part][index][1];
				}
			}
			return 0; // 해당 항목이 없으면 0 반환
		}		

		// 0: 확률 실패, 1: 성공, 2: 재료 부족/시도 불가
		public static int TryEnhance(Mobile from, Item item)
		{
			IEquipOption eq = item as IEquipOption;
			if (eq == null || from == null) return 2;

			int partIdx = GetPartIndex(item);
			if (partIdx == -1) return 2;

			if (eq.SuffixOption[10] >= 10 || eq.PrefixOption[10] < 0 || eq.PrefixOption[10] >= ResourceTypes.Length)
				return 2;

			Type typeToConsume = ResourceTypes[eq.PrefixOption[10]];
			int cost = MaterialCosts[eq.SuffixOption[10]];
			int currentAmount = from.Backpack.GetAmount(typeToConsume);

			if (currentAmount < cost)
			{
				int missing = cost - currentAmount;
				from.SendMessage(0x22, "강화 재료가 {0}개 부족합니다.", missing);
				return 2; // 재료 부족
			}

			from.Backpack.ConsumeTotal(typeToConsume, cost);

			// 확률 판정 (유저님이 수정하신 높은 확률식 적용)
			if (Utility.RandomDouble() <= (0.8 - eq.SuffixOption[10] * 0.08 + eq.SuffixOption[1] * 0.04))
			{
				eq.SuffixOption[10]++;
				ItemOptionCreator.NewEquipOptionList(item, 
					m_EnhanceTable[partIdx][eq.PrefixOption[10]][0], 
					(int)(m_EnhanceTable[partIdx][eq.PrefixOption[10]][1] * EnhanceScales[eq.SuffixOption[10]][0]), 0);

				item.InvalidateProperties();
				from.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
				return 1; // 성공
			}

			return 0; // 확률 실패 (재료 소모됨)
		}

        public static int GetPartIndex(Item item)
        {
            if (item is BaseWeapon) return 0;
            if (item is BaseArmor) return 1;
            if (item is BaseJewel) return 2;
            return -1;
        }
    }
}