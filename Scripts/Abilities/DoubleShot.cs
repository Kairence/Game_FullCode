using System;

namespace Server.Items
{
    /// <summary>
    /// Send two arrows flying at your opponent if you're mounted. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public class DoubleShot : WeaponAbility
    {
        public DoubleShot()
        {
        }

        public override int BaseMana
        {
            get
            {
                return Core.TOL ? 30 : 35;
            }
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
        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            Use(attacker, defender);
        }

        public override bool Validate(Mobile from)
        {
            if (base.Validate(from))
            {
                if (from.Mounted)
                    return true;
                else
                {
                    from.SendLocalizedMessage(1070770); // You can only execute this attack while mounted!
                    ClearCurrentAbility(from);
                }
            }

            return false;
        }

        public void Use(Mobile attacker, Mobile defender)
        {
            if (!Validate(attacker))	//sanity
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063348); // You launch two shots at once!
            defender.SendLocalizedMessage(1063349); // You're attacked with a barrage of shots!

            defender.FixedParticles(0x37B9, 1, 19, 0x251D, EffectLayer.Waist);

            attacker.Weapon.OnSwing(attacker, defender);

            if (attacker.Weapon is BaseWeapon)
                ((BaseWeapon)attacker.Weapon).ProcessingMultipleHits = false;
        }
    }
}