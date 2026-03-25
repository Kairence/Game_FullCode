using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Chivalry
{
    public class CleanseByFireSpell : AuraSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Cleanse By Fire", "Expor Flamus",
            201, 9002);

        // [오라 설정 구현]
        public override int AuraHue => 1258; // 강렬한 주황/빨강
        public override BuffIcon AuraIcon => BuffIcon.ImmolatingWeapon; 
        public override int TitleCliloc => 1060718;
        public override int SecondaryCliloc => 1153762;

        public override double RequiredSkill => 50;
        public override int MantraNumber => 1060718;

        public CleanseByFireSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        // [수정] 부모 클래스의 전역 체크 메서드를 사용하도록 변경 (핵심!)
        public static bool UnderAura(Mobile m)
        {
            return AuraSpell.IsUnderAura<CleanseByFireSpell>(m);
        }

        // [수정] 개별 테이블 기록 로직 삭제 및 사운드만 추가
        protected override void ApplyEffect(Mobile target)
        {
            base.ApplyEffect(target);
            target.PlaySound(0x208);
        }

        protected override string GetBuffArgs()
        {
            return "10\t20";
        }

        protected override void OnVisualEffect(Mobile caster)
        {
            // 시전자 주변 미세 파티클 (주황색)
            caster.FixedParticles(0x377A, 1, 15, 5012, AuraHue, 2, EffectLayer.Waist);
        }
    }
}
