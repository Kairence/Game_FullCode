using System;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Fourth
{
    public class LightningSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Lightning", "Por Ort Grav",
            239,
            9021,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public LightningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fourth;
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
			int range = 10;
			if ( Caster is AirElemental )
				range = 20;
			if ( Caster is Titan )
				range = 15;

            this.Caster.Target = new InternalTarget(this, range);
		}

        public void Target(IDamageable m)
        {
            Mobile mob = m as Mobile;

            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile source = Caster;
                SpellHelper.Turn(Caster, m.Location);

                SpellHelper.CheckReflect((int)Circle, ref source, ref m);

				double damage = 0;
				int min = 20;
				int max = 120;
				if( Caster is SummonedAirElemental )
				{
					min = 40;
					max = 60;
				}
				if( Caster is Titan )
				{
					min = 15;
					max = 40;
				}
				damage = GetNewAosDamage(0, min, max, m);

                if (m is Mobile)
                {
                    Effects.SendBoltEffect(m, true, 0, false);
                }
                else
                {
                    Effects.SendBoltEffect(EffectMobile.Create(m.Location, m.Map, EffectMobile.DefaultDuration), true, 0, false);
                }

                if (damage > 0)
                {
					if( Caster is Titan && Utility.RandomDouble() < 0.1 )
					{
						//타이탄 광역기
						foreach (var id in AcquireIndirectTargets(m.Location, 10))
						{
							Mobile tar = id as Mobile;

							this.Caster.DoHarmful(id);

							if (tar != null)
							{
								Effects.SendBoltEffect(tar, true, 0, false);
							}
							else
							{
								Effects.SendBoltEffect(EffectMobile.Create(tar.Location, tar.Map, EffectMobile.DefaultDuration), true, 0, false);
							}
							SpellHelper.Damage(this, id, damage * 2, 0, 0, 100, 0, 0);
						}						
					}
					else
						SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
				}
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly LightningSpell m_Owner;
            public InternalTarget(LightningSpell owner, int range)
                : base(range, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IDamageable)
                    m_Owner.Target((IDamageable)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
