using System;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// This powerful ability requires secondary skills to activate.
    /// Successful use of Shadowstrike deals extra damage to the target — and renders the attacker invisible!
    /// Only those who are adept at the art of stealth will be able to use this ability.
    /// </summary>
    public class ShadowStrike : WeaponAbility
    {
        public ShadowStrike()
        {
        }

        public override double DamageScalar
        {
            get
            {
                return 1.25;
            }
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			int weaponBonus = 0;//level >= 5 ? 2 : 0;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[11,0], Misc.Util.SPMStam[11,1], level, bonus ) )
				return;

			double aggrotactics = tactics * 0.005;
			double bonusDamage = 2.22 + level * 0.0237;
			
			BaseWeapon one = attacker.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;
			BaseShield shield = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
			if( one != null )
				weaponBonus++;
			if( shield == null )
				weaponBonus++;
			
			//°è»ê
			damage = (int)( damage * ( 1 + bonusDamage + weaponBonus * 1.11 ) );

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060078); // You strike and hide in the shadows!
            defender.SendLocalizedMessage(1060079); // You are dazed by the attack and your attacker vanishes!

            Effects.SendLocationParticles(EffectItem.Create(attacker.Location, attacker.Map, EffectItem.DefaultDuration), 0x376A, 8, 12, 9943);
            attacker.PlaySound(0x482);

            defender.FixedEffect(0x37BE, 20, 25);
			
			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false, 0);
			int aggro = (int)( damage * aggrotactics );
			Misc.Util.AggroTargetCheck(attacker, defender, aggro );
        }
        public override int BaseMana
        {
            get
            {
                return 15;
            }
        }		
		/*
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
			int range = 1;
			if( attacker.Hidden )
				range = 6;
            if ( !this.Validate(attacker) || !this.CheckMana(attacker, 3, 5.0)  || (!attacker.InRange(defender, range)))
                return;

			int damageBonus = 30;
			
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				//damageBonus += pm.SilverPoint[12] * 2;
				
				if ( pm.realHidden )
					damageBonus *= 2;

				if( defender != null && attacker.CanSee(defender) )
				{
					attacker.RevealingAction();
					attacker.SendLocalizedMessage(1060078); // You strike and hide in the shadows!
					defender.SendLocalizedMessage(1060079); // You are dazed by the attack and your attacker vanishes!
					damage *= 100 + damageBonus;
					damage /= 100;
					attacker.MoveToWorld(defender.Location, defender.Map);
					Effects.SendLocationParticles(EffectItem.Create(attacker.Location, attacker.Map, EffectItem.DefaultDuration), 0x376A, 8, 12, 9943);
					attacker.PlaySound(0x482);

					defender.FixedEffect(0x37BE, 20, 25);
					AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);
				}
			}

		}
		*/
    }
}