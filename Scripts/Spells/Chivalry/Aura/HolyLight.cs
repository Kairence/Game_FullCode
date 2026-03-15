using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Chivalry
{
    public class HolyLightSpell : AuraSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Holy Light", "Augus Luminos", 206, 9002);

        public override int AuraHue => 0x481; 
        public override BuffIcon AuraIcon => BuffIcon.Resilience; 
        public override int TitleCliloc => 1060724;
        public override int SecondaryCliloc => 1153761;
        public override double RequiredSkill => 50;
        public override int MantraNumber => 1060724;

        public HolyLightSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        // [수정] EnemyOfOne과 동일하게 전역 체크 방식으로 변경
        public static bool UnderAura(Mobile m)
        {
            return AuraSpell.IsUnderAura<HolyLightSpell>(m);
        }

        // [삭제] 더 이상 m_InfluenceTable 기록이 필요 없으므로 ApplyEffect 오버라이드를 지우거나 베이스만 호출합니다.
        protected override void ApplyEffect(Mobile target)
        {
            base.ApplyEffect(target);
            // 추가적인 개별 효과가 없다면 이 메서드 자체를 삭제해도 베이스 로직이 작동합니다.
        }

        protected override string GetBuffArgs()
        {
            return "3\t10"; 
        }

        protected override void OnVisualEffect(Mobile caster)
        {
            caster.FixedParticles(0x377A, 1, 15, 5012, AuraHue, 2, EffectLayer.Waist);
        }
    }
}