using System;
using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;

using Server.Spells;

namespace Server.Items
{
    /// <summary>
    /// A godsend to a warrior surrounded, the Whirlwind Attack allows the fighter to strike at all nearby targets in one mighty spinning swing.
    /// </summary>
    public class WhirlwindAttack : WeaponAbility
    {
        public WhirlwindAttack()
        {
        }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            BaseWeapon wep = attacker.Weapon as BaseWeapon;

            if (wep != null)
                wep.ProcessingMultipleHits = true;

            return true;
        }
        public override int BaseMana
        {
            get
            {
                return 25;
            }
        }
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker) )
                return;

            ClearCurrentAbility(attacker);

            Map map = attacker.Map;

            if (map == null)
                return;

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            weapon.ProcessingMultipleHits = true;

            if (weapon == null)
               return;

            //if (!this.CheckMana(attacker, true))
            //    return;

            attacker.FixedEffect(0x3728, 10, 15);
            attacker.PlaySound(0x2A1);

			int range = 3;
			
			/*
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				range += pm.SilverPoint[13] / 5;
			}
			*/
            var list = SpellHelper.AcquireIndirectTargets(attacker, attacker, attacker.Map, range)
                .OfType<Mobile>()
                .Where(m => attacker.InRange(m, range) && m != defender).ToList();

            int count = list.Count;

            if (count > 0)
            {
                attacker.SendLocalizedMessage(1060161); // The whirling attack strikes a target!
                attacker.RevealingAction();

                foreach(var m in list)
                {
                    m.SendLocalizedMessage(1060162); // You are struck by the whirling attack and take damage!
					AOS.Damage(m, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
                    //weapon.OnHit(attacker, m, damageBonus);
                }
            }

            ColUtility.Free(list);

            weapon.ProcessingMultipleHits = false;
        }
    }
}