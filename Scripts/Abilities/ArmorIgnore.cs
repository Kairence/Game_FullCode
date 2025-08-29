using System;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// This special move allows the skilled warrior to bypass his target's physical resistance, for one shot only.
    /// The Armor Ignore shot does slightly less damage than normal.
    /// Against a heavily armored opponent, this ability is a big win, but when used against a very lightly armored foe, it might be better to use a standard strike!
    /// </summary>

	public class ArmorIgnore : WeaponAbility
    {
        public ArmorIgnore()
        {
        }

		public override int BaseMana
		{
			get
			{
				return 5;
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
    }
}