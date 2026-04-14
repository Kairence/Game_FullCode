using System;
using Server.Items;
using Server.SkillHandlers;

namespace Server.Engines.Craft
{
    public class DefImbuing : CraftSystem
    {
        private static CraftSystem m_CraftSystem;

        public override SkillName MainSkill => SkillName.Imbuing;
        public override int GumpTitleNumber => 1111867; // <CENTER>IMBUING MENU</CENTER>

        public static CraftSystem CraftSystem
        {
            get
            {
                if (m_CraftSystem == null)
                    m_CraftSystem = new DefImbuing();

                return m_CraftSystem;
            }
        }

        private DefImbuing() : base(1, 1, 1.25)
        {
        }

        public override double GetChanceAtMin(CraftItem item) => 0.0;

        private (int Message, bool CanCraft) CheckAccessibility(Mobile from, ITool tool)
        {
            if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
                return (1044038, false);

            if (tool is Item toolItem && !from.InRange(toolItem.GetWorldLocation(), 2))
                return (500446, false);

            if (!Imbuing.CheckSoulForge(from, 2))
                return (1079787, false);

            return (0, true);
        }

        public override int CanCraft(Mobile from, ITool tool, Type itemType)
        {
            var (message, canCraft) = CheckAccessibility(from, tool);
            return canCraft ? 0 : message;
        }

        public override void PlayCraftEffect(Mobile from)
        {
            from.PlaySound(0x5C9);
        }

        public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
        {
            if (toolBroken) from.SendLocalizedMessage(1044038);
            CraftSkillCheck(from, item.ItemType, MainSkill);
            return failed ? (lostMaterial ? 1044043 : 1044157) : 1044154;
        }

        public override void InitCraftList()
        {
            AddGemTiers(typeof(StarSapphire), "별무늬 사파이어", 1044231);
            AddGemTiers(typeof(Emerald), "에메랄드", 1044232);
            AddGemTiers(typeof(Sapphire), "사파이어", 1044233);
            AddGemTiers(typeof(Ruby), "루비", 1044234);
            AddGemTiers(typeof(Citrine), "황수정", 1044235);
            AddGemTiers(typeof(Amethyst), "자수정", 1044236);
            AddGemTiers(typeof(Tourmaline), "전기석", 1044237);
            AddGemTiers(typeof(Amber), "호박", 1044238);
            AddGemTiers(typeof(Diamond), "다이아몬드", 1044239);
			
			MarkOption = true;
        }

        // 헬퍼 메서드: 전달받은 재료 클리록 번호(resourceCliloc)를 AddCraft에 적용
        private void AddGemTiers(Type resourceType, string gemName, int resourceCliloc)
        {
            // [기획 반영 완벽 수정] 
            // 1. 이름 동기화: 희귀 -> 영웅 -> 서사 -> 전설 -> 신화
            // 2. 스킬 요구치 동기화: RefineGem.cs의 OnCraft와 맞추어 직관적인 스킬 구간 배정
            // 3. 재료 소모량 20, 40, 60, 80, 100 유지
            AddCraft(typeof(RefineGem), 1111867, $"희귀 {gemName}",   0.0,  50.0, resourceType, resourceCliloc,  20, "해당 보석이 부족합니다.");
            AddCraft(typeof(RefineGem), 1111867, $"영웅 {gemName}",    40.0,  90.0, resourceType, resourceCliloc,  40, "해당 보석이 부족합니다.");
            AddCraft(typeof(RefineGem), 1111867, $"서사 {gemName}",    80.0, 130.0, resourceType, resourceCliloc,  60, "해당 보석이 부족합니다.");
            AddCraft(typeof(RefineGem), 1111867, $"전설 {gemName}",   120.0, 170.0, resourceType, resourceCliloc,  80, "해당 보석이 부족합니다.");
            AddCraft(typeof(RefineGem), 1111867, $"신화 {gemName}", 160.0, 210.0, resourceType, resourceCliloc, 100, "해당 보석이 부족합니다.");
        }
    }
}