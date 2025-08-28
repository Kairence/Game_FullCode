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

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			//int levelCountBonus = level >= 5 ? 1 : 0;
			//double levelDamageBonus = level >= 5 ? 0.3 : 0;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[6,0], Misc.Util.SPMStam[6,1], level, bonus ) )
				return;
			
			int count = 1;// + levelCountBonus + (int)(tactics / 100 );
			//double chance = tactics - (int)(tactics / 100 );
			//chance *= 0.01;
			//if( chance > Utility.RandomDouble() )
			//	count++;
			
			//계산
			damage = (int)( damage * ( 1 + level * 0.01 )  + tactics);

            attacker.SendLocalizedMessage(1060084); // You attack with lightning speed!
            defender.SendLocalizedMessage(1060085); // Your attacker strikes with lightning speed!

            defender.PlaySound(0x3BB);
            defender.FixedEffect(0x37B9, 244, 25);

			for( int i = 0; i < count; i++ )
			{
				AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
			}
		}
	}
}