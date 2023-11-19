using System;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// Available on some crossbows, this special move allows archers to fire while on the move.
    /// This shot is somewhat less accurate than normal, but the ability to fire while running is a clear advantage.
    /// </summary>
    public class MovingShot : WeaponAbility
    {
        public MovingShot()
        {
        }

        public override bool ValidatesDuringHit
        {
            get
            {
                return false;
            }
        }
        public override int BaseMana
        {
            get
            {
                return 10;
            }
        }
        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            //Validates in OnSwing for accuracy scalar
            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060089); // You fail to execute your special move
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

            attacker.SendLocalizedMessage(1060216); // Your shot was successful

            //defender.PlaySound(0x3BB);
            //defender.FixedEffect(0x37B9, 244, 25);

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
		/*
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
			BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            if (!this.Validate(attacker)|| !this.CheckMana(attacker, 3, 5.0)  || (!attacker.InRange(defender, weapon.MaxRange)))
                return;

		//Validates in OnSwing for accuracy scalar
            ClearCurrentAbility(attacker);
			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
            attacker.SendLocalizedMessage(1060216); // Your shot was successful
        }
		*/
    }
}