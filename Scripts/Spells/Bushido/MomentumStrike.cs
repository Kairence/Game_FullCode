using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Spells.Bushido
{
    public class MomentumStrike : SamuraiMove
    {
        public MomentumStrike()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 0;
            }
        }
        public override double RequiredSkill
        {
            get
            {
                return 0.0;
            }
        }
        public override TextDefinition AbilityMessage
        {
            get
            {
                return new TextDefinition(1070757);
            }
        }// You prepare to strike two enemies with one blow.
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            //if (!this.Validate(attacker) || !this.CheckMana(attacker, false))
            //    return;

            //ClearCurrentMove(attacker);

            //BaseWeapon weapon = attacker.Weapon as BaseWeapon;

			double anatomy = attacker.Skills.Anatomy.Value * 0.03;
			if( anatomy >= 100 )
				anatomy += 0.4;

			int range = 1 + (int)anatomy;
			
            List<Mobile> targets = new List<Mobile>();
            IPooledEnumerable eable = attacker.GetMobilesInRange(range);//weapon.MaxRange);

			//if( WeaponAbility.Context(attacker) != null )
			WeaponAbility.RemoveContext(attacker);
			//if( WeaponAbility.Context2(attacker) != null )
			WeaponAbility.RemoveContext2(attacker);
			
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
                attacker.SendLocalizedMessage(1063171); // You transfer the momentum of your weapon into another enemy!
				for( int i = 0; i < targets.Count; i++ )
				{
					Mobile target = targets[i];

					target.SendLocalizedMessage(1063172); // You were hit by the momentum of a Samurai's weapon!

					target.FixedParticles(0x37B9, 1, 4, 0x251D, 0, 0, EffectLayer.Waist);

					attacker.PlaySound(0x510);

					attacker.DoHarmful(target);
                    AOS.Damage(target, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, true);

					//weapon.OnSwing(attacker, target, 1);

					if (target.Alive)
						attacker.Combatant = target;
				}
            }
            else
            {
                attacker.SendLocalizedMessage(1063123); // There are no valid targets to attack!
            }

            ColUtility.Free(targets);
        }

        public override void OnUse(Mobile m)
        {
            base.OnUse(m);

            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.MomentumStrike, 1060600, 1063268));
        }

        public override void OnClearMove(Mobile from)
        {
            base.OnClearMove(from);

            BuffInfo.RemoveBuff(from, BuffIcon.MomentumStrike);
        }

        public override void CheckGain(Mobile m)
        {
            m.CheckSkill(this.MoveSkill, this.RequiredSkill, 120.0);
        }
    }
}