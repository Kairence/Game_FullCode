using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// Strike your opponent with great force, partially bypassing their armor and inflicting greater damage. Requires either Bushido or Ninjitsu skill
    /// </summary>
    public class ArmorPierce : WeaponAbility
    {
        public static Dictionary<Mobile, Timer> _Table = new Dictionary<Mobile, Timer>();

        public ArmorPierce()
        {
        }

        public override SkillName GetSecondarySkill(Mobile from)
        {
            return from.Skills[SkillName.Ninjitsu].Base > from.Skills[SkillName.Bushido].Base ? SkillName.Ninjitsu : SkillName.Bushido;
        }

        public override int BaseMana
        {
            get
            {
                return 30;
            }
        }

        public override double DamageScalar
        {
            get
            {
                return Core.HS ? 1.0 : 1.5;
            }
        }

        public override bool RequiresSE
        {
            get
            {
                return true;
            }
        }
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			int ignoreDamageBonus = (int)tactics * 10;//;( level >= 5 ? 500 : 0 ) + (int)tactics;
			//double levelDamageBonus = level >= 5 ? 0.25 : 0;
			//double damageBonus = level >= 5 ? 1.0 : 0.0;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[0,0], Misc.Util.SPMStam[0,1], level, bonus ) )
				return;
			
			double bonusDamage = level * 0.025;// + damageBonus;
			
			attacker.SendLocalizedMessage(1060076); // Your attack penetrates their armor!
			defender.SendLocalizedMessage(1060077); // The blow penetrated your armor!

			defender.PlaySound(0x56);
			defender.FixedParticles(0x3728, 200, 25, 9942, EffectLayer.Waist);

			Effects.PlaySound(defender.Location, defender.Map, 0x56);
			Effects.SendTargetParticles(defender, 0x3728, 200, 25, 0, 0, 9942, EffectLayer.Waist, 0);

			//계산
			damage = (int)( damage * ( 1 + bonusDamage ) ) + ignoreDamageBonus;

			AOS.Damage(defender, attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);

            ClearCurrentAbility(attacker);

		}
		/*
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
			return;
            if (!this.Validate(attacker) || !this.CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063350); // You pierce your opponent's armor!

            defender.SendLocalizedMessage(1153764); // Your armor has been pierced!
            defender.SendLocalizedMessage(1063351); // Your attacker pierced your armor!            

            if (Core.HS)
            {
                if (_Table.ContainsKey(defender))
                {
                    if (attacker.Weapon is BaseRanged)
                        return;

                    _Table[defender].Stop();
                }

                BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ArmorPierce, 1028860, 1154367, TimeSpan.FromSeconds(3), defender, "10"));
                _Table[defender] = Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(3), RemoveEffects, defender);
            }

            defender.PlaySound(0x28E);
            defender.FixedParticles(0x3728, 1, 26, 0x26D6, 0, 0, EffectLayer.Waist);
        }

        public static void RemoveEffects(Mobile m)
        {
            if (IsUnderEffects(m))
            {
                m.SendLocalizedMessage(1153904); // Your armor has returned to normal.
                _Table.Remove(m);
            }
        }

        public static bool IsUnderEffects(Mobile m)
        {
            if(m == null)
                return false;
                
            return _Table.ContainsKey(m);
        }
		*/
    }
}
