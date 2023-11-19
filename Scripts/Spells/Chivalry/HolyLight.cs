using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Spells.Chivalry
{
    public class HolyLightSpell : PaladinSpell
    {
        public override DamageType SpellDamageType { get { return DamageType.SpellAOE; } }

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Holy Light", "Augus Luminos",
            -1,
            9002);

        public HolyLightSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override TimeSpan CastDelayBase
        {
            get
            {
                return TimeSpan.FromSeconds(0.0);
            }
        }
        public override double RequiredSkill
        {
            get
            {
                return 0.0;
            }
        }
        public override int RequiredMana
        {
            get
            {
                return 0;
            }
        }
        public override int RequiredTithing
        {
            get
            {
                return 0;
            }
        }
        public override int MantraNumber
        {
            get
            {
                return 1060724;
            }
        }// Augus Luminos
        public override bool BlocksMovement
        {
            get
            {
                return false;
            }
        }
        public override bool DelayedDamage
        {
            get
            {
                return false;
            }
        }
        public override void OnCast()
        {
            if (CheckSequence())
            {
                foreach (var id in AcquireIndirectTargets(Caster.Location, 3))
                {
                    Mobile m = id as Mobile;
					double anatomy = Caster.Skills.Anatomy.Value * 1.5;
					if( anatomy >= 100 )
						anatomy += 20;

                    int damage = 50 + (int)anatomy;//ComputePowerValue(10) + Utility.RandomMinMax(0, 2);

					damage = Utility.RandomMinMax(damage, damage * 2 );
					
                    // TODO: Should caps be applied?

                    Caster.DoHarmful(m);
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
					m.Freeze(TimeSpan.FromSeconds( 3.0 ));
                }

                Caster.PlaySound(0x212);
                Caster.PlaySound(0x206);

                Effects.SendLocationParticles(EffectItem.Create(Caster.Location, Caster.Map, EffectItem.DefaultDuration), 0x376A, 1, 29, 0x47D, 2, 9962, 0);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(Caster.X, Caster.Y, Caster.Z - 7), Caster.Map, EffectItem.DefaultDuration), 0x37C4, 1, 29, 0x47D, 2, 9502, 0);
            }

            FinishSequence();
        }
    }
}