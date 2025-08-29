using System;
using System.Collections.Generic;
using Server.Spells;
using Server.Mobiles;

namespace Server.Items
{
    public class LightningArrow : WeaponAbility
    {
        public LightningArrow()
        {
        }
        public override int BaseMana
        {
            get
            {
                return 10;
            }
        }
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			//int levelEnergyBonus = level >= 5 ? 15 : 0;
			bool levelAreaBonus = true;//level >= 5 ? true : false;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[14,0], Misc.Util.SPMStam[14,1], level, bonus ) )
				return;

			int energy = 100; //40 + levelEnergyBonus + 5 * level;
			
			if( energy > 100 )
				energy = 100;
			
			//계산
			damage = (int)( damage * ( 1 + level * 0.02 ) + tactics * 2 );
            //defender.BoltEffect(0);
			//AOS.Damage(defender, attacker, damage, false, 100 - energy, 0, 0, 0, energy, 0, 0, false, false, false);
			
			int area = 1;

			if( levelAreaBonus )
			{
				List<Mobile> targets = new List<Mobile>();
				IPooledEnumerable eable = attacker.GetMobilesInRange(area);//weapon.MaxRange);

				foreach (Mobile m in eable)
				{
					if (m != defender && m != attacker && m.CanBeHarmful(attacker, false) && attacker.InLOS(m) && 
						Server.Spells.SpellHelper.ValidIndirectTarget(attacker, m))
					{
						targets.Add(m);
					}
				}
				eable.Free();
				
				if (targets.Count > 0)
				{
					for (int i = 0; i < targets.Count; ++i)
					{
						Mobile m = targets[i];
						if ( m != attacker || m != defender )
						{
							m.BoltEffect(0);
							AOS.Damage(m, attacker, damage, false, 100 - energy, 0, 0, 0, energy, 0, 0, false, false, false);
						}
					}
					ColUtility.Free(targets);
				}							
			}
            ClearCurrentAbility(attacker);
        }
    }
}