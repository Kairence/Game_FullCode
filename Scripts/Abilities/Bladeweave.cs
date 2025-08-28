using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public class Bladeweave : WeaponAbility
    {
        private class BladeWeaveRedirect
        {
            public WeaponAbility NewAbility;
            public int ClilocEntry;

            public BladeWeaveRedirect(WeaponAbility ability, int cliloc)
            {
                NewAbility = ability;
                ClilocEntry = cliloc;
            }
        }

        private static Dictionary<Mobile, BladeWeaveRedirect> m_NewAttack = new Dictionary<Mobile, BladeWeaveRedirect>();

        public static bool BladeWeaving(Mobile attacker, out WeaponAbility a)
        {
            BladeWeaveRedirect bwr;
            bool success = m_NewAttack.TryGetValue(attacker, out bwr);
            if (success)
                a = bwr.NewAbility;
            else
                a = null;

            return success;
        }

        public Bladeweave()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			//bool levelAreaBonus = level >= 5 ? true : false;
			//double levelDamageBonus = level >= 5 ? 1.0 : 0;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[9,0], Misc.Util.SPMStam[9,1], level, bonus ) )
				return;

			double bonusDamage = 1.0 + level * 0.025;	
			double aggroRemover = 0.5 + tactics * 0.005;
			
			//계산
			damage = (int)( damage * ( 1 + bonusDamage ) );

			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
			attacker.SendLocalizedMessage(1063168); // You attack with lightning precision!
			defender.SendLocalizedMessage(1063169); // Your opponent's quick strike causes extra damage!
			defender.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);
			defender.PlaySound(0x51D);
			
			//어그로 삭제 코드
			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				Misc.Util.AggroTargetCheck(attacker, defender, (int)( damage * aggroRemover ) );
			}

			/*
			if( levelAreaBonus )
			{
				List<Mobile> targets = new List<Mobile>();
				IPooledEnumerable eable = attacker.GetMobilesInRange(6);//weapon.MaxRange);

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
							AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
							attacker.SendLocalizedMessage(1063168); // You attack with lightning precision!
							defender.SendLocalizedMessage(1063169); // Your opponent's quick strike causes extra damage!
							defender.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);
							defender.PlaySound(0x51D);
							
							//어그로 삭제 코드
							if( defender is BaseCreature )
							{
								BaseCreature bc = defender as BaseCreature;
								Misc.Util.AggroTargetCheck(attacker, defender, (int)( damage * aggroRemover ) );
							}
						}
					}
					ColUtility.Free(targets);
				}
			}
			*/
            ClearCurrentAbility(attacker);
		}

		/*
        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            if (!Validate(attacker) || !CheckMana(attacker, false))
                return false;

            int ran = -1;

            if (attacker is BaseCreature && PetTrainingHelper.CheckSecondarySkill((BaseCreature)attacker, SkillName.Bushido))
            {
                ran = Utility.Random(9);
            }
            else
            {
                bool canfeint = attacker.Skills[WeaponAbility.Feint.GetSecondarySkill(attacker)].Value >= WeaponAbility.Feint.GetRequiredSecondarySkill(attacker);
                bool canblock = attacker.Skills[WeaponAbility.Block.GetSecondarySkill(attacker)].Value >= WeaponAbility.Block.GetRequiredSecondarySkill(attacker);

                if (canfeint && canblock)
                {
                    ran = Utility.Random(9);
                }
                else if (canblock)
                {
                    ran = Utility.Random(8);
                }
                else
                {
                    ran = Utility.RandomList(0, 1, 2, 3, 4, 5, 6, 8);
                }
            }

            switch (ran)
            {
                case 0:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.ArmorIgnore, 1028838);
                    break;
                case 1:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.BleedAttack, 1028839);
                    break;
                case 2:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.ConcussionBlow, 1028840);
                    break;
                case 3:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.CrushingBlow, 1028841);
                    break;
                case 4:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.DoubleStrike, 1028844);
                    break;
                case 5:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.MortalStrike, 1028846);
                    break;
                case 6:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.ParalyzingBlow, 1028848);
                    break;
                case 7:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.Block, 1028853);
                    break;
                case 8:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(WeaponAbility.Feint, 1028857);
                    break;
                default:
                    // should never happen
                    return false;
            }


            return ((BladeWeaveRedirect)m_NewAttack[attacker]).NewAbility.OnBeforeSwing(attacker, defender);
        }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            BladeWeaveRedirect bwr;
            if (m_NewAttack.TryGetValue(attacker, out bwr))
                return bwr.NewAbility.OnBeforeDamage(attacker, defender);
            else
                return base.OnBeforeDamage(attacker, defender);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (CheckMana(attacker, false))
            {
                BladeWeaveRedirect bwr;
                if (m_NewAttack.TryGetValue(attacker, out bwr))
                {
                    attacker.SendLocalizedMessage(1072841, "#" + bwr.ClilocEntry.ToString());
                    bwr.NewAbility.OnHit(attacker, defender, damage);
                }
                else
                    base.OnHit(attacker, defender, damage);

                m_NewAttack.Remove(attacker);
                ClearCurrentAbility(attacker);
            }
        }
		*/
        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            BladeWeaveRedirect bwr;
            if (m_NewAttack.TryGetValue(attacker, out bwr))
                bwr.NewAbility.OnMiss(attacker, defender);
            else
                base.OnMiss(attacker, defender);

            m_NewAttack.Remove(attacker);
        }
    }
}