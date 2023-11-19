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
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker))
                return;
			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 10 )
					return;
				attacker.Stam -= 10;
			}
            defender.BoltEffect(0);
			AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 0, 100, 0, 0, false, false, false);
            ClearCurrentAbility(attacker);
        }
    }
}