using Server.Mobiles;

namespace Server.Items
{
	/// <summary>
	///     The highly skilled warrior can use this special attack to make two quick swings in succession.
	///     Landing both blows would be devastating!
	/// </summary>
	public class DoubleStrike : WeaponAbility
	{
		public override int BaseMana { get { return 5; } }
		public override double DamageScalar { get { return 0.75; } }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            BaseWeapon wep = attacker.Weapon as BaseWeapon;

            if (wep != null)
                wep.InDoubleStrike = true;

            return true;
        }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) )
			{
				return;
			}

			ClearCurrentAbility(attacker);

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;

			if (weapon == null)
			{
				return;
			}

            // If no combatant, wrong map, one of us is a ghost, or cannot see, or deleted, then stop combat
            if (defender.Deleted || attacker.Deleted || defender.Map != attacker.Map || !defender.Alive ||
                !attacker.Alive || !attacker.CanSee(defender))
            {
                weapon.InDoubleStrike = false;
                attacker.Combatant = null;
                return;
            }

			if (!attacker.InRange(defender, weapon.MaxRange))
			{
                weapon.InDoubleStrike = false;
				return;
			}

            attacker.SendLocalizedMessage(1060084); // You attack with lightning speed!
            defender.SendLocalizedMessage(1060085); // Your attacker strikes with lightning speed!

            defender.PlaySound(0x3BB);
            defender.FixedEffect(0x37B9, 244, 25);

			int count = 1;
			
			/*
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				if( pm.SilverPoint[7] >= 5 )
					count += pm.SilverPoint[7] / 5;
			}
			*/
			for( int i = 0; i < count; i++ )
			{
				if (attacker.InLOS(defender))
				{
					attacker.RevealingAction();
					attacker.NextCombatTime = Core.TickCount + (int)weapon.OnSwing(attacker, defender).TotalMilliseconds;
				}
			}
            weapon.InDoubleStrike = false;
		}
	}
}