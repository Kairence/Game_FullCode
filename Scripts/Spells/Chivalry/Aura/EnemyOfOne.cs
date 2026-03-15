using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Chivalry
{
    public class EnemyOfOneSpell : AuraSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo("Enemy of One", "Forul Solum", 205, 9002);

        public override int AuraHue => 0x4E; 
        public override BuffIcon AuraIcon => BuffIcon.EnemyOfOne;
        public override int TitleCliloc => 1060723;
        public override int SecondaryCliloc => 1153760;
        public override double RequiredSkill => 50;
        public override int MantraNumber => 1060723;

        public EnemyOfOneSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        // 부모의 기능을 호출하여 CS1540 에러 완전 해결
        public static bool UnderAura(Mobile m)
        {
            return AuraSpell.IsUnderAura<EnemyOfOneSpell>(m);
        }

        protected override string GetBuffArgs()
        {
            return "10\t20";
        }

        protected override void OnVisualEffect(Mobile caster)
        {
            // 핑크색/보라색 미세 파티클
            caster.FixedParticles(0x377A, 1, 15, 5012, AuraHue, 2, EffectLayer.Waist);
        }
    }
}