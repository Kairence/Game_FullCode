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
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			//bool levelAreaBonus = level >= 5 ? true : false;
			//double levelCrushBonus = level >= 5 ? 0.06 : 0;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[3,0], Misc.Util.SPMStam[3,1], level, bonus ) )
				return;
			
			//double bonusDamage = 2.5 + level * 1.00;		
			
			//계산
			damage = (int)( damage * ( 4 + level * 0.1 ) );
			
			//강타 계산
			double crushChance = 0.15 + tactics * 0.001 + level * 0.01;
			int specialDamage = Misc.Util.SmashCalc(attacker, defender, crushChance );
			damage += specialDamage;

			attacker.SendLocalizedMessage(1060090); // You have delivered a crushing blow!
			defender.SendLocalizedMessage(1060166); // You feel disoriented!

			defender.PlaySound(0x213);
			defender.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);

			Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 10), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0x36FE, 1, 0, false, false, 1133, 3, 9501, 1, 0, EffectLayer.Waist, 0x100);			

			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);

            ClearCurrentAbility(attacker);
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