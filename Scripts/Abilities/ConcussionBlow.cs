using System;
using System.Collections.Generic;
using System.Linq;
using Server.Spells;
using Server.Mobiles;
namespace Server.Items
{
    /// <summary>
    /// This devastating strike is most effective against those who are in good health and whose reserves of mana are low, or vice versa.
    /// </summary>
    public class ConcussionBlow : WeaponAbility
    {
        public ConcussionBlow()
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
			//int levelAreaBonus = level >= 5 ? 2 : 0;
			//double levelStunBonus = level >= 5 ? 5 : 0;
			
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[2,0], Misc.Util.SPMStam[2,1], level, bonus ) )
				return;
			
			//double bonusDamage = 5.0 + level * 2.0;

			attacker.SendLocalizedMessage(1060165); // You have delivered a concussion!

			//int x = 0;
			//int y = 0;
			int line = 1;//3 + (int)( tactics / 100 ) + levelAreaBonus;
			double duration = 10.0 + tactics * 0.1;
			
			/*
			if( attacker.Direction == Direction.East || (int)attacker.Direction == 130 ) // x = 1
			{
				x = 1;
			}
			else if( attacker.Direction == Direction.West || (int)attacker.Direction == 134 ) // x= -1
			{
				x = -1;
			}
			else if( attacker.Direction == Direction.South || (int)attacker.Direction == 132 ) // y = 1;
			{
				y = 1;
			}
			else if( attacker.Direction == Direction.North || (int)attacker.Direction == 128 ) // y= -1
			{
				y = -1;
			}
			else if( attacker.Direction == Direction.Mask || attacker.Direction == Direction.Up || (int)attacker.Direction == 134 ) //x = -1, y = -1
			{
				x = -1;
				y = -1;
			}
			else if( attacker.Direction == Direction.Down || (int)attacker.Direction == 131 ) // x = 1, y = 1
			{
				x = 1;
				y = 1;
			}
			else if( attacker.Direction == Direction.Left || (int)attacker.Direction == 133 ) // x = -1, y = 1
			{
				x = -1;
				y = 1;
			}
			else if( attacker.Direction == Direction.Right || (int)attacker.Direction == 129 ) // x = 1, y = -1
			{
				x = 1;
				y = -1;
			}
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				//line += pm.SilverPoint[4] / 4;
				//duration += pm.SilverPoint[4] * 0.4;
			}
			*/
			if( attacker is OgreLord )
			{
				line += 9;
				duration = 5;
			}
			//계산
			damage = (int)( damage * ( 4 + level * 0.1 ) );
			
			List<Mobile> targets = new List<Mobile>();
			IPooledEnumerable eable = attacker.GetMobilesInRange(line);//weapon.MaxRange);

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
					if ( m != attacker )
					{
						m.SendLocalizedMessage(1060091); // You feel disoriented!
						m.PlaySound(0x1E1);
						m.FixedParticles(0, 1, 0, 9946, EffectLayer.Head);
						if( Utility.RandomDouble() < 0.2 + level * 0.005 )
						{
							if( m is BaseCreature )
							{
								BaseCreature bc = m as BaseCreature;
								duration *= Misc.Util.MonsterTierCrowdControlRecovery(bc);
							}
							m.Freeze(TimeSpan.FromSeconds( duration ));
						}
						AOS.Damage(m, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
					}
				}
				ColUtility.Free(targets);
			}			
            ClearCurrentAbility(attacker);
        }
    }
}