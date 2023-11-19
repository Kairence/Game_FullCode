using System;
using Server.Mobiles;

namespace Server.Spells.Eighth
{
    public class WaterElementalSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Water Elemental", "Kal Vas Xen An Flam",
            269,
            9070,
            false,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public WaterElementalSpell(Mobile caster, Item scroll)
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

				SpellHelper.Summon(new SummonedWaterElemental(), this.Caster, 0x217, duration, false, false);
            }

            this.FinishSequence();
        }
    }
}