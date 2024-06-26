using System;
using Server.Mobiles;

namespace Server.Spells.Eighth
{
    public class EarthElementalSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Earth Elemental", "Kal Vas Xen Ylem",
            269,
            9020,
            false,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public EarthElementalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Eighth;
            }
        }
        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            return true;
        }

        public override void OnCast()
        {
            if (this.CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds(1000000);

                if (Core.AOS)
                    SpellHelper.Summon(new SummonedEarthElemental(), this.Caster, 0x217, duration, false, false);
                else
                    SpellHelper.Summon(new EarthElemental(), this.Caster, 0x217, duration, false, false);
            }

            this.FinishSequence();
        }
    }
}