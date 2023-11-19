using System;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// This special move represents a significant change to the use of poisons in Age of Shadows.
    /// Now, only certain weapon types — those that have Infectious Strike as an available special move — will be able to be poisoned.
    /// Targets will no longer be poisoned at random when hit by poisoned weapons.
    /// Instead, the wielder must use this ability to deliver the venom.
    /// While no skill in Poisoning is directly required to use this ability, being knowledgeable in the application and use of toxins
    /// will allow a character to use Infectious Strike at reduced mana cost and with a chance to inflict more deadly poison on his victim.
    /// With this change, weapons will no longer be corroded by poison.
    /// Level 5 poison will be possible when using this special move.
    /// </summary>
    public class InfectiousStrike : WeaponAbility
    {
        public InfectiousStrike()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 5;
            }
        }
        
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker) )
                return;

 			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 5 )
					return;
				attacker.Stam -= 5;
			}

            defender.PlaySound(0xDD);
            defender.FixedParticles(0x3728, 244, 25, 9941, 1266, 0, EffectLayer.Waist);

			AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 100, 0, 0, 0, false, false, false);
            ClearCurrentAbility(attacker);
        }
    }
}
