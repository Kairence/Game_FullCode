using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class ImbuingExcludeContext
    {
        public Dictionary<int, HashSet<int>> ExcludedOptions { get; set; } = new();
    }

    public class Imbuing
    {
		// Imbuing.cs의 Imbuing 클래스 내부에 추가
		public static readonly string[] GemNames = 
		[ 
			"별무늬 사파이어", "에메랄드", "사파이어", "루비", "황수정", "자수정", "전기석", "호박", "다이아몬드" 
		];
        // 1. 이미지의 클리록 번호와 인덱스 매핑 (1044231 ~ 1044239)
        public static int GetGemCliloc(int index) => index switch
        {
            0 => 1044231, // 별무늬 사파이어
            1 => 1044232, // 에메랄드
            2 => 1044233, // 사파이어
            3 => 1044234, // 루비
            4 => 1044235, // 황수정
            5 => 1044236, // 자수정
            6 => 1044237, // 전기석
            7 => 1044238, // 호박
            8 => 1044239, // 다이아몬드
            _ => 1044037
        };
        private static Dictionary<Mobile, ImbuingExcludeContext> m_ExcludeTable = new();

        public static ImbuingExcludeContext GetExcludeContext(Mobile m)
        {
            if (!m_ExcludeTable.ContainsKey(m))
                m_ExcludeTable[m] = new ImbuingExcludeContext();
            return m_ExcludeTable[m];
        }

        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Imbuing].Callback = from => {
                if (from is PlayerMobile pm && from.Alive) {
                    pm.CloseGump(typeof(Server.Gumps.ImbuingExcludeGump));
                    pm.SendGump(new Server.Gumps.ImbuingExcludeGump(pm, 0));
                }
                return TimeSpan.FromSeconds(1.0);
            };
        }

        // --- [필수 호환성 메서드] ---
        public static bool IsInNonImbueList(Type t) => false;
        public static int GetMaxWeight(Item item) => 550;
        public static bool CheckSoulForge(Mobile from, int range, bool message, bool checkqueen, out double bonus)
        {
            bonus = 0.0;
            bool isForge = false;
            var eable = from.Map.GetItemsInRange(from.Location, range);
            foreach (var item in eable) {
                if (item is SoulforgeStation || (item.ItemID >= 0x4277 && item.ItemID <= 0x4286) || (item.ItemID >= 0x4263 && item.ItemID <= 0x4272))
                { isForge = true; break; }
            }
            eable.Free();
            if (!isForge && message) from.SendLocalizedMessage(1079787);
            return isForge;
        }

        public static bool CheckSoulForge(Mobile from, int range) => CheckSoulForge(from, range, true, false, out _);
        public static bool CheckSoulForge(Mobile from, int range, out double bonus) => CheckSoulForge(from, range, true, false, out bonus);
        public static bool CheckSoulForge(Mobile from, int range, bool message) => CheckSoulForge(from, range, message, false, out _);
        public static bool CheckSoulForge(Mobile from, int range, bool message, bool checkqueen) => CheckSoulForge(from, range, message, checkqueen, out _);

        public static int GetValueForID(Item item, int id) => 0;
        public static int GetTotalWeight(Item item, int id, bool trueWeight, bool imbuing) => 0;
        public static int GetTotalMods(Item item, int id = -1) => 0;
        public static void SetProperty(Item item, int id, int value) { }
        public static Type[] IngredTypes = [ typeof(MagicalResidue) ];
    }
}