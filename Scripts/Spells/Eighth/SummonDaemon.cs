using System;
using Server.Mobiles;

namespace Server.Spells.Eighth
{
    public class SummonDaemonSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Summon Daemon", "Kal Vas Xen Corp",
            269,
            9050,
            false,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);
        public SummonDaemonSpell(Mobile caster, Item scroll)
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

                if (Core.AOS)  /* Why two diff daemons? TODO: solve this */
                {
                    BaseCreature m_Daemon = new SummonedDaemon();
                    SpellHelper.Summon(m_Daemon, this.Caster, 0x216, duration, false, false);
                    m_Daemon.FixedParticles(0x3728, 8, 20, 5042, EffectLayer.Head);
                }
                else
                    SpellHelper.Summon(new Daemon(), this.Caster, 0x216, duration, false, false);
            }

            this.FinishSequence();
        }
    }
}