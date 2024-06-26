using System;
using System.Collections.Generic;
using Server.Engines.PartySystem;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class ArchProtectionSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Arch Protection", "Vas Uus Sanct",
            Core.AOS ? 239 : 215,
            9011,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public ArchProtectionSpell(Mobile caster, Item scroll)
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
        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!this.Caster.CanSee(p))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = this.Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 1 + (int)Caster.Skills.Magery.Value / 50);

                    foreach (Mobile m in eable)
                    {
                        if (this.Caster.CanBeBeneficial(m, false))
                            targets.Add(m);
                    }

                    eable.Free();

                    if (targets.Count > 0)
                    {
                        for (int i = 0; i < targets.Count; ++i)
                        {
                            Mobile m = targets[i];

							if( m.MeleeDamageAbsorb == 0 || m.MeleeDamageAbsorb == 1 )
							{
								m.PlaySound(0x1E9);
								m.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
								m.MeleeDamageAbsorb += 2;
								string args = String.Format("{0}\t{1}", 0, 0);
								BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Protection, 1075814, 1075815, args.ToString()));
							}
							else
							{
								m.MeleeDamageAbsorb -= 2;
								m.PlaySound(0x1ED);
								m.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);

								BuffInfo.RemoveBuff(m, BuffIcon.Protection);
								BuffInfo.RemoveBuff(m, BuffIcon.ArchProtection);
							}
                        }
                    }
                }
            }

            this.FinishSequence();
        }

        private static Dictionary<Mobile, Int32> _Table = new Dictionary<Mobile, Int32>();

        private static void AddEntry(Mobile m, Int32 v)
        {
            _Table[m] = v;
        }

        public static void RemoveEntry(Mobile m)
        {
            if (_Table.ContainsKey(m))
            {
                int v = _Table[m];
                _Table.Remove(m);
                m.EndAction(typeof(ArchProtectionSpell));
                m.VirtualArmorMod -= v;
                if (m.VirtualArmorMod < 0)
                    m.VirtualArmorMod = 0;
            }
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Owner;

            public InternalTimer(Mobile target, Mobile caster)
                : base(TimeSpan.FromSeconds(0))
            {
                double time = caster.Skills[SkillName.Magery].Value * 1.2;
                if (time > 144)
                    time = 144;
                this.Delay = TimeSpan.FromSeconds(time);
                this.Priority = TimerPriority.OneSecond;

                this.m_Owner = target;
            }

            protected override void OnTick()
            {
                ArchProtectionSpell.RemoveEntry(this.m_Owner);
            }
        }

        private class InternalTarget : Target
        {
            private readonly ArchProtectionSpell m_Owner;
            public InternalTarget(ArchProtectionSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;

                if (p != null)
                    this.m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
