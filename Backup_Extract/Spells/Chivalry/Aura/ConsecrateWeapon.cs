using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Chivalry
{
    public class ConsecrateWeaponSpell : AuraSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Consecrate Weapon", "Consecrus Arma",
            202, 9002);

        // [오라 설정]
        public override int AuraHue => 1162; // 보라색(사용자 확인값)
        public override BuffIcon AuraIcon => BuffIcon.ConsecrateWeapon; 
        public override int TitleCliloc => 1060720;
        public override int SecondaryCliloc => 1060721;

        public override double RequiredSkill => 100;
        public override int MantraNumber => 1060720;

        public ConsecrateWeaponSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        // [중요] AOS.cs에서 호출할 메서드 - 부모 클래스의 전역 체크 활용
        public static bool UnderAura(Mobile m)
        {
            return AuraSpell.IsUnderAura<ConsecrateWeaponSpell>(m);
        }

        // 베이스 클래스의 로직만 사용 (로컬 테이블 기록 삭제)
        protected override void ApplyEffect(Mobile target)
        {
            base.ApplyEffect(target);
            //target.PlaySound(0x20C);
        }

        protected override string GetBuffArgs()
        {
            return "10\t1";
        }

        protected override void OnVisualEffect(Mobile caster)
        {
            // 이펙트 핑크색/보라색 미세 파티클
            caster.FixedParticles(0x377A, 1, 15, 5012, AuraHue, 2, EffectLayer.Waist);
        }
    }
}
