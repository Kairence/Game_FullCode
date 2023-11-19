using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Spells.Eighth
{
    public class EarthquakeSpell : MagerySpell
    {
        public override DamageType SpellDamageType { get { return DamageType.SpellAOE; } }

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Earthquake", "In Vas Por",
            233,
            9012,
            false,
            Reagent.Bloodmoss,
            Reagent.Ginseng,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);

        public EarthquakeSpell(Mobile caster, Item scroll)
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
        public override bool DelayedDamage
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override void OnCast()
        {
            if (SpellHelper.CheckTown(Caster, Caster) && CheckSequence())
            {
                foreach (var id in AcquireIndirectTargets(Caster.Location, 8))
                {
                    Mobile m = id as Mobile;

                    int damage;

                    if (Core.AOS)
                    {
						damage = GetNewAosDamage(0, 10, 140, id is PlayerMobile, id);
						damage *= 1000 + AosWeaponAttributes.GetValue(Caster, AosWeaponAttribute.HitLightning );
						damage /= 1000;
                    }
                    else
                    {
                        damage = (id.Hits * 6) / 10;

                        if ((m == null || !m.Player) && damage < 10)
                            damage = 10;
                        else if (damage > 75)
                            damage = 75;
                    }

                    Caster.DoHarmful(id);
                    SpellHelper.Damage(this, id, damage, 0, 0, 0, 0, 100);
                }
            }

            FinishSequence();
        }
    }
}