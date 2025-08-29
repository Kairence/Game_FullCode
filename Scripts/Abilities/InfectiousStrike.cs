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
        
		private readonly int[] PoisonLevelDamage =
		{
			100, 175, 300, 500, 750
		};
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			int levelPoisonBonus = 0; //level >= 5 ? 1 : 0;
			//double levelAttackBonus = level >= 5 ? 0.3 : 0;
			
			double poisonChance = 0.3 + tactics * 0.002 + level * 0.001;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[7,0], Misc.Util.SPMStam[7,1], level, bonus ) )
				return;

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

			int poisonLevel = 0;
			if( weapon.PoisonCharges > 0 )
			{
                --weapon.PoisonCharges;
				if( weapon.Poison == Poison.Lethal )
					poisonLevel = 4;
				else if( weapon.Poison == Poison.Deadly )
					poisonLevel = 3;
				else if( weapon.Poison == Poison.Greater )
					poisonLevel = 2;
				else if( weapon.Poison == Poison.Regular )
					poisonLevel = 1;
			}
			else
				poisonChance /= 2;
			
			for( int i = 0; i < 1 + levelPoisonBonus; ++i )
			{
				if( Utility.RandomDouble() <= poisonChance )
				{
					damage = (int)( PoisonLevelDamage[poisonLevel] * ( 1 + level * 0.07 ) );
					//독 스킬 계산
					//double attackerPoisoningValue = 1 + attacker.Skills.Poisoning.Value * 0.002;
					damage += (int)( attacker.Skills.Poisoning.Value * 10 );
					//if( attackerPoisoningValue >= 100 )
					//{
					//	damage += 500;
						//attackerPoisoningValue += 0.1;
					//}
					//damage = (int)( damage * attackerPoisoningValue );

					if( damage < 100 )
						damage = 100;
				
					Misc.Util.PoisonSavingDamage(defender, damage);
					defender.ApplyPoison(attacker, Poison.Lesser);
				}
			}

            defender.PlaySound(0xDD);
            defender.FixedParticles(0x3728, 244, 25, 9941, 1266, 0, EffectLayer.Waist);

			//AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 100, 0, 0, 0, false, false, false);
            ClearCurrentAbility(attacker);
        }
    }
}
