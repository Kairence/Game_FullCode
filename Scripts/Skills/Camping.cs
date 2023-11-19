using System;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System.Collections.Generic;
using Server.Items;
using Server.Targets;

namespace Server.SkillHandlers
{
    public class Camping
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Camping].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( pm.Fury < 100 )
					m.SendMessage("분노가 {0} 부족합니다.", 100 - pm.Fury );
				else
				{
					pm.Fury = 0;
					BaseWeapon two = pm.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
					BaseWeapon one = pm.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;
					BaseWeapon atkWeapon = pm.Weapon as BaseWeapon;
					atkWeapon.PlaySwingAnimation(pm);
					pm.CheckSkill(SkillName.Anatomy, 1000);
					if( atkWeapon.Skill is SkillName.Swords )
					{
						if ( one != null )
						{
							Server.Spells.Bushido.Evasion spell = new Server.Spells.Bushido.Evasion( pm, null );
							spell.Cast();
						}
						else if ( two != null )
						{
							if( atkWeapon is BaseAxe )
							{
								Server.Spells.Chivalry.DivineFurySpell spell = new Server.Spells.Chivalry.DivineFurySpell( pm, null );
								spell.Cast();
							}
							else
							{
								Server.Spells.Chivalry.ConsecrateWeaponSpell spell = new Server.Spells.Chivalry.ConsecrateWeaponSpell( pm, null );
								spell.Cast();
							}
						}
					}
					else if( atkWeapon.Skill is SkillName.Macing )
					{
						if ( one != null)
						{
							Server.Spells.Bushido.Confidence spell = new Server.Spells.Bushido.Confidence( pm, null );
							spell.Cast();
						}
						else
						{
							Server.Spells.Chivalry.HolyLightSpell spell = new Server.Spells.Chivalry.HolyLightSpell( pm, null );
							spell.Cast();
							//SpecialAttack = 5;
						}
					}
					else if( atkWeapon.Skill is SkillName.Fencing )
					{
						if ( one != null)
						{
							Server.Spells.Bushido.CounterAttack spell = new Server.Spells.Bushido.CounterAttack( pm, null );
							spell.Cast();
						}
						else
						{
							pm.FuryActive = true;
							/*
							Server.Spells.Chivalry.ConsecrateWeaponSpell spell = new Server.Spells.Chivalry.ConsecrateWeaponSpell( pm, null );
							spell.Cast();
							*/
						}
					}
					else if( atkWeapon.Skill is SkillName.Archery )
					{
						if ( two != null && atkWeapon is BaseRanged )
						{
							BaseRanged br = atkWeapon as BaseRanged;
							if( br.AmmoType == typeof( Bolt ) )
								pm.FuryActive = true;
							else
							{
								Server.Spells.Chivalry.EnemyOfOneSpell spell = new Server.Spells.Chivalry.EnemyOfOneSpell( pm, null );
								spell.Cast();
							}
						}
					}					
				}
			}
            //m.Target = new Anatomy.InternalTarget();
			/*
			Container pack = m.Backpack;
			bool skinning = false;
			if( pack != null )
			{
				List<Item> knife = pack.FindItemsByType<Item>();
				for ( int i = knife.Count -1; i >= 0; i--)
				{
					Item item = knife[i];
					if( item is SkinningKnife )
					{
						m.SendMessage("가방에서 피복칼을 찾아냅니다.");
						SkinningKnife sk = item as SkinningKnife;
						skinning = true;
						m.Target = new BladedItemTarget(sk);
						break;
					}
				}
           // m.SendLocalizedMessage(500321); // Whom shall I examine?
		   
			}
			if( !skinning )
				m.SendMessage("해부학 스킬을 사용하기 위해서는 가방 안에 피복칼이 있어야합니다.");
			*/
            return TimeSpan.FromSeconds(1.0);
        }

        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(8, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
				/*
				//아나토미 가죽 채취
				if( from.Backpack != null && SkinningKnife.IsChildOf( from.Backpack) )
				{
					from.SendMessage("시체를 더블클릭 하세요");
					Item item = targeted as Item;
					from.Target = new BladedItemTarget( SkinningKnife );
				}


                if (from == targeted)
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500324); // You know yourself quite well enough already.
                }
                else if (targeted is TownCrier)
                {
                    ((TownCrier)targeted).PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500322, from.NetState); // This person looks fine to me, though he may have some news...
                }
                else if (targeted is BaseVendor && ((BaseVendor)targeted).IsInvulnerable)
                {
                    ((BaseVendor)targeted).PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500326, from.NetState); // That can not be inspected.
                }
                else if (targeted is Mobile)
                {
                    Mobile targ = (Mobile)targeted;

                    int marginOfError = Math.Max(0, 25 - (int)(from.Skills[SkillName.Anatomy].Value / 4));

                    int str = targ.Str + Utility.RandomMinMax(-marginOfError, +marginOfError);
                    int dex = targ.Dex + Utility.RandomMinMax(-marginOfError, +marginOfError);
                    int stm = ((targ.Stam * 100) / Math.Max(targ.StamMax, 1)) + Utility.RandomMinMax(-marginOfError, +marginOfError);

                    int strMod = str / 10;
                    int dexMod = dex / 10;
                    int stmMod = stm / 10;

                    if (strMod < 0)
                        strMod = 0;
                    else if (strMod > 10)
                        strMod = 10;

                    if (dexMod < 0)
                        dexMod = 0;
                    else if (dexMod > 10)
                        dexMod = 10;

                    if (stmMod > 10)
                        stmMod = 10;
                    else if (stmMod < 0)
                        stmMod = 0;

                }
                else if (targeted is Item)
                {
                    ((Item)targeted).SendLocalizedMessageTo(from, 500323, ""); // Only living things have anatomies!
                }
				*/
            }
        }
    }
}