using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Spells.SkillMasteries;

namespace Server.Spells.Necromancy
{
    public class PoisonStrikeSpell : NecromancerSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Poison Strike", "In Vas Nox",
            203,
            9031,
            Reagent.NoxCrystal);

        public PoisonStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override DamageType SpellDamageType { get { return DamageType.SpellAOE; } }

        public override TimeSpan CastDelayBase
        {
            get
            {
                return TimeSpan.FromSeconds((Core.ML ? 2.0 : 1.5));
            }
        }
        public override double RequiredSkill
        {
            get
            {
                return 50.0;
            }
        }
        public override int RequiredMana
        {
            get
            {
                return 17;
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
			if ( Caster is PoisonElemental )
				range = 20;

            this.Caster.Target = new InternalTarget(this, range);
		}

        public void Target(IDamageable m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile mob = m as Mobile;
                SpellHelper.Turn(Caster, m);
                if (Core.SA && HasDelayContext(m))
                {
                    DoHurtFizzle();
                    return;
                }
				if( Caster is PoisonElemental && mob.Poison == null )
				{
					BaseWeapon atkWeapon = Caster.Weapon as BaseWeapon;
					BaseWeapon defWeapon = mob.Weapon as BaseWeapon;

					Skill atkSkill = Caster.Skills[atkWeapon.Skill];
					Skill defSkill = mob.Skills[defWeapon.Skill];

					double chance = atkSkill.Value - defSkill.Value + 50;
					chance /= 2;
					if( chance < Utility.Random(100) )					
						mob.Poison = Poison.Lethal;
				}
				ApplyEffects(m);
                //ConduitSpell.CheckAffected(Caster, m, ApplyEffects);
            }

            FinishSequence();
        }

        public void ApplyEffects(IDamageable m, double strength = 1.0)
        {
            /* Creates a blast of poisonous energy centered on the target.
                * The main target is inflicted with a large amount of Poison damage, and all valid targets in a radius of 2 tiles around the main target are inflicted with a lesser effect.
                * One tile from main target receives 50% damage, two tiles from target receives 33% damage.
                */

            Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x36B0, 1, 14, 63, 7, 9915, 0);
            Effects.PlaySound(m.Location, m.Map, 0x229);

			int min = 75;
			int max = 165;
			
			if( Caster is SummonedDaemon )
			{
				min = 60;
				max = 90;
			}

            double damage = GetNewAosDamage(0, min, max, m);
			
			Caster.DoHarmful(m);
			SpellHelper.Damage(this, m, damage, 0, 0, 0, 100, 0);	

			/*

            double sdiBonus;

            if (Core.SE)
            {
                if (Core.SA)
                {
                    sdiBonus = (double)SpellHelper.GetSpellDamageBonus(Caster, m, CastSkill, m is PlayerMobile) / 100;
                }
                else
                {
                    sdiBonus = (double)AosAttributes.GetValue(Caster, AosAttribute.SpellDamage) / 100;

                    // PvP spell damage increase cap of 15% from an item’s magic property in Publish 33(SE)
                    if (m is PlayerMobile && Caster.Player && sdiBonus > 15)
                        sdiBonus = 15;
                }
            }
            else
            {
                sdiBonus = (double)AosAttributes.GetValue(Caster, AosAttribute.SpellDamage) / 100;
            }

            double pvmDamage = (damage * (1 + sdiBonus)) * strength;
            double pvpDamage = damage * (1 + sdiBonus);

            Map map = m.Map;

            if (map != null)
            {
                foreach (var id in AcquireIndirectTargets(m.Location, 2))
                {
                    int num;

                    if (Utility.InRange(id.Location, m.Location, 0))
                        num = 1;
                    else if (Utility.InRange(id.Location, m.Location, 1))
                        num = 2;
                    else
                        num = 3;

                    Caster.DoHarmful(id);
                    SpellHelper.Damage(this, id, ((id is PlayerMobile && Caster.Player) ? pvpDamage : pvmDamage) / num, 0, 0, 0, 100, 0);
                }
            }
			*/
        }

        private class InternalTarget : Target
        {
            private readonly PoisonStrikeSpell m_Owner;
            public InternalTarget(PoisonStrikeSpell owner, int range)
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
