using System;
using System.Collections.Generic;
using System.Linq;

using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// This attack allows you to disarm your foe.
    /// Now in Age of Shadows, a successful Disarm leaves the victim unable to re-arm another weapon for several seconds.
    /// </summary>
    public class Disarm : WeaponAbility
    {
        public static readonly TimeSpan BlockEquipDuration = TimeSpan.FromSeconds(5.0);
        public Disarm()
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
			int levelWeakBonus = 10 + (int)( tactics * 0.2 );
			bool levelDisarmBonus = false;//level >= 5 ? true : false;
			double skillTime = 4.0 + tactics * 0.04 + level * 0.2;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[4,0], Misc.Util.SPMStam[4,1], level, bonus ) )
				return;
			
			double bonusDamage = 1.5 + level * 0.05;
		
            if (IsImmune(defender))
            {
                attacker.SendLocalizedMessage(1111827); // Your opponent is gripping their weapon too tightly to be disarmed.
                defender.SendLocalizedMessage(1111828); // You will not be caught off guard by another disarm attack for some time.
                return;
            }
			if( defender is PlayerMobile )
			{
				PlayerMobile pm = defender as PlayerMobile;
				pm.disarmcheck = levelDisarmBonus;
				pm.disarmtime = DateTime.Now + TimeSpan.FromSeconds(skillTime);
				pm.disarmweak = levelWeakBonus;
			}
			else if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				bc.disarmcheck = levelDisarmBonus;
				bc.disarmtime = DateTime.Now + TimeSpan.FromSeconds(skillTime);
				bc.disarmweak = levelWeakBonus;
			}
			
			//계산
			damage = (int)( damage * ( 1 + bonusDamage ) );

            defender.PlaySound(0x3B9);
            defender.FixedParticles(0x37BE, 232, 25, 9948, EffectLayer.LeftHand);
			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
		}

        private Type[] _AutoRearms =
        {
            typeof(BritannianInfantry)
        };

        public static List<Mobile> _Immunity;

        public static bool IsImmune(Mobile m)
        {
            return _Immunity != null && _Immunity.Contains(m);
        }

        public static void AddImmunity(Mobile m, TimeSpan duration)
        {
            if (_Immunity == null)
                _Immunity = new List<Mobile>();

            _Immunity.Add(m);

            Timer.DelayCall<Mobile>(duration, mob =>
                {
                    if (_Immunity != null && _Immunity.Contains(mob))
                        _Immunity.Remove(mob);
                }, m);
        }
    }
}
