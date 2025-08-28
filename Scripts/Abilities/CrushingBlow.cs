using System;
using Server.Mobiles;
using System.Collections.Generic;
namespace Server.Items
{
    /// <summary>
    /// Also known as the Haymaker, this attack dramatically increases the damage done by a weapon reaching its mark.
    /// </summary>

    public class CrushingBlow : WeaponAbility
    {
        public CrushingBlow()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 15;
            }
        }
        public override double DamageScalar
        {
            get
            {
                return 0;
            }
        }
		
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			bool levelAreaBonus = false;//level >= 5 ? true : false;
			//double levelCrushBonus = level >= 5 ? 0.06 : 0;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[3,0], Misc.Util.SPMStam[3,1], level, bonus ) )
				return;
			
			//double bonusDamage = 2.5 + level * 1.00;		
			
			//계산
			damage = (int)( damage * ( 4 + level * 0.1 ) );

			//강타 계산
			double crushChance = tactics * 0.001 + level * 0.001;
			int specialDamage = Misc.Util.SmashCalc(attacker, defender, crushChance );
			damage += specialDamage;

			attacker.SendLocalizedMessage(1060090); // You have delivered a crushing blow!
			defender.SendLocalizedMessage(1060166); // You feel disoriented!

			defender.PlaySound(0x213);
			defender.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);

			Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 10), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0x36FE, 1, 0, false, false, 1133, 3, 9501, 1, 0, EffectLayer.Waist, 0x100);			
			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);


			if( levelAreaBonus )
			{
				List<Mobile> targets = new List<Mobile>();
				IPooledEnumerable eable = attacker.GetMobilesInRange(1);//weapon.MaxRange);

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
							m.SendLocalizedMessage(1060166); // You feel disoriented!

							m.PlaySound(0x213);
							m.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);

							Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z + 10), m.Map), new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z + 20), m.Map), 0x36FE, 1, 0, false, false, 1133, 3, 9501, 1, 0, EffectLayer.Waist, 0x100);			
							AOS.Damage(m, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
						}
					}
					ColUtility.Free(targets);
				}
			}
            ClearCurrentAbility(attacker);
        }
	}
}