using System;
using Server.Mobiles;

namespace Server.Items
{
    public class SerpentArrow : WeaponAbility
    {
        public SerpentArrow()
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
				if( attacker.Stam < 5 )
					return;
				attacker.Stam -= 5;
			}
			defender.SendLocalizedMessage(1112369); // 	You have been poisoned by a lethal arrow!

            defender.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
            defender.PlaySound(0x474);

			AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 100, 0, 0, 0, false, false, false);
            ClearCurrentAbility(attacker);
        }
    }
}