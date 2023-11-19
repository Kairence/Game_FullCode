using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class BladeSpiritsSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Blade Spirits", "In Jux Hur Ylem",
            266,
            9040,
            false,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.Nightshade);
        public BladeSpiritsSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
            }
        }
		/*
        public override TimeSpan GetCastDelay()
        {
            if (Core.AOS)
                return TimeSpan.FromTicks(base.GetCastDelay().Ticks * ((Core.SE) ? 3 : 5));

            return base.GetCastDelay() + TimeSpan.FromSeconds(6.0);
        }
		*/

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            return true;
        }

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            Map map = this.Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                this.Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, this.Caster) && this.CheckSequence())
            {
                TimeSpan duration;

                if (Core.AOS)
                    duration = TimeSpan.FromSeconds(30);
                else
                    duration = TimeSpan.FromSeconds(Utility.Random(80, 40));

				int count = 1 + (int)( Caster.Skills.Magery.Value / 50 );

				SpellHelper.SummonCheck( Caster );
				
				if( count > 1 )
				{
					for( int i = 0; i < count; i++ )
					{
						BaseCreature.Summon(new BladeSpirits(true), false, this.Caster, new Point3D(p), 0x212, duration);						
					}
				}
				else
					BaseCreature.Summon(new BladeSpirits(true), false, this.Caster, new Point3D(p), 0x212, duration);
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private BladeSpiritsSpell m_Owner;
            public InternalTarget(BladeSpiritsSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                    this.m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetOutOfLOS(Mobile from, object o)
            {
                from.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
                from.Target = new InternalTarget(this.m_Owner);
                from.Target.BeginTimeout(from, this.TimeoutTime - DateTime.UtcNow);
                this.m_Owner = null;
            }

            protected override void OnTargetFinish(Mobile from)
            {
                if (this.m_Owner != null)
                    this.m_Owner.FinishSequence();
            }
        }
    }
}