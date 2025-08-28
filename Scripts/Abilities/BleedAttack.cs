using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;

namespace Server.Items
{
    /// <summary>
    /// Make your opponent bleed profusely with this wicked use of your weapon.
    /// When successful, the target will bleed for several seconds, taking damage as time passes for up to ten seconds.
    /// The rate of damage slows down as time passes, and the blood loss can be completely staunched with the use of bandages. 
    /// </summary>
    public class BleedAttack : WeaponAbility
    {
        private static readonly Dictionary<Mobile, BleedTimer> m_BleedTable = new Dictionary<Mobile, BleedTimer>();

        public BleedAttack()
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

			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[1,0], Misc.Util.SPMStam[1,1], level, bonus ) )
				return;
			
			if(defender is BaseCreature && ((BaseCreature)defender).BleedImmune)
			{
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
			}
			else
			{
				attacker.SendLocalizedMessage(1060159); // Your target is bleeding!
				defender.SendLocalizedMessage(1060160); // You are bleeding!
				if (defender is PlayerMobile)
				{
					defender.LocalOverheadMessage(MessageType.Regular, 0x21, 1060757); // You are bleeding profusely
					defender.NonlocalOverheadMessage(MessageType.Regular, 0x21, 1060758, defender.Name); // ~1_NAME~ is bleeding profusely
				}
			}
			int hitsPercent = 0;
			if( defender.HitsMax != 0 )
				hitsPercent = ( defender.Hits * 100 ) / defender.HitsMax;
			
			int bloodDamage = (int)( damage * ( 1 + hitsPercent * 0.02 + level * 0.025) );
			
			
			if( bloodDamage < 0 )
			{
				return;
			}
			
            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if ((context != null && (context.Type == typeof(LichFormSpell) || context.Type == typeof(WraithFormSpell))) ||
                (defender is BaseCreature && ((BaseCreature)defender).BleedImmune) || Server.Spells.Mysticism.StoneFormSpell.CheckImmunity(defender))
            {
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
                return;
            }
            defender.PlaySound(0x133);
            defender.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);
			Blood blood = new Blood();
			blood.ItemID = Utility.Random(0x122A, 5);
			blood.MoveToWorld(defender.Location, defender.Map);

			//피 흡수
			int bloodDrink = (int)tactics * 10;
			if( defender.Hits < bloodDrink )
				bloodDrink = defender.Hits;
			attacker.Hits += bloodDrink;

			AOS.Damage(defender, attacker, bloodDamage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
			BeginBleed(defender, attacker, damage, level, tactics);
		}			
		
		public static bool IsBleeding(Mobile m)
        {
            return m_BleedTable.ContainsKey(m);
        }
		
		public static void BeginBleed(Mobile m, Mobile from, bool splintering = false )
		{
			BeginBleed(m, from, 0, 0, 0, false );
		}
		
		public static void BeginBleed(Mobile m, Mobile from, int damage, int level, double tactics, bool splintering = false)
        {
            BleedTimer timer = null;

            if (m_BleedTable.ContainsKey(m))
            {
                if (splintering)
                {
                    timer = m_BleedTable[m];
                    timer.Stop();
                }
                else
                {
                    return;
                }
            }

            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Bleed, 1075829, 1075830, TimeSpan.FromSeconds(2), m, String.Format("{0}\t{1}\t{2}", "1", "10", "2")));

            //timer = new BleedTimer(from, m, CheckBloodDrink(from), damage, level, tactics);
			timer = new BleedTimer(from, m, false, damage, level, tactics);
			
            m_BleedTable[m] = timer;
            timer.Start();

            from.SendLocalizedMessage(1060159); // Your target is bleeding!
            m.SendLocalizedMessage(1060160); // You are bleeding!

            if (m is PlayerMobile)
            {
                m.LocalOverheadMessage(MessageType.Regular, 0x21, 1060757); // You are bleeding profusely
                m.NonlocalOverheadMessage(MessageType.Regular, 0x21, 1060758, m.Name); // ~1_NAME~ is bleeding profusely
            }

            m.PlaySound(0x133);
            m.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);
			
			
        }

        public static void DoBleed(Mobile m, Mobile from, int damage, bool blooddrinker)
        {
            if (m.Alive && !m.IsDeadBondedPet)
            {
                //if (!m.Player)
                //    damage *= 2;

                m.PlaySound(0x133);
				//m.Hits -= damage;
                AOS.Damage(m, from, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);

				/*
                if (blooddrinker && from.Hits < from.HitsMax)
                {
                    from.SendLocalizedMessage(1113606); //The blood drinker effect heals you.
                    from.Heal(damage);
                }
				*/
                Blood blood = new Blood();
                blood.ItemID = Utility.Random(0x122A, 5);
                blood.MoveToWorld(m.Location, m.Map);
            }
            else
            {
                EndBleed(m, false);
            }
        }

        public static void EndBleed(Mobile m, bool message)
        {
            Timer t = null;

            if (m_BleedTable.ContainsKey(m))
            {
                t = m_BleedTable[m];
                m_BleedTable.Remove(m);
            }

            if (t == null)
                return;

            t.Stop();
            BuffInfo.RemoveBuff(m, BuffIcon.Bleed);

            if (message)
                m.SendLocalizedMessage(1060167); // The bleeding wounds have healed, you are no longer bleeding!
        }
		
		public static bool CheckBloodDrink(Mobile attacker)
		{
            return attacker.Weapon is BaseWeapon && ((BaseWeapon)attacker.Weapon).WeaponAttributes.BloodDrinker > 0;
		}

		/*
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (!Validate(attacker) || (!attacker.InRange(defender, weapon.MaxRange)))
                return;

			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 10 )
					return;
				attacker.Stam -= 10;
			}
            ClearCurrentAbility(attacker);

			if(defender is BaseCreature && ((BaseCreature)defender).BleedImmune)
			{
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
			}
			else
			{
				attacker.SendLocalizedMessage(1060159); // Your target is bleeding!
				defender.SendLocalizedMessage(1060160); // You are bleeding!
				if (defender is PlayerMobile)
				{
					defender.LocalOverheadMessage(MessageType.Regular, 0x21, 1060757); // You are bleeding profusely
					defender.NonlocalOverheadMessage(MessageType.Regular, 0x21, 1060758, defender.Name); // ~1_NAME~ is bleeding profusely
				}
			}
            defender.PlaySound(0x133);
            defender.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);
			double hitsBonus = 0.0;
			if( attacker is PlayerMobile )
			{
				//레벨 체크 흡수 증가
				PlayerMobile pm = attacker as PlayerMobile;
				//hitsBonus += pm.SilverPoint[2] * 0.02;
			}
			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				//hitsBonus *= MonsterTier(bc);
			}
			//attacker.Hits += (int)( damage * 2 ); //hitsBonus );
			Blood blood = new Blood();
			blood.ItemID = Utility.Random(0x122A, 5);
			blood.MoveToWorld(defender.Location, defender.Map);
			AOS.Damage(defender, attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
			/*
            // Necromancers under Lich or Wraith Form are immune to Bleed Attacks.
            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if ((context != null && (context.Type == typeof(LichFormSpell) || context.Type == typeof(WraithFormSpell))) ||
                (defender is BaseCreature && ((BaseCreature)defender).BleedImmune) || Server.Spells.Mysticism.StoneFormSpell.CheckImmunity(defender))
            {
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
                return;
            }

			BeginBleed(defender, attacker);
        }
		*/
        private class BleedTimer : Timer
        {
            private readonly Mobile m_From;
            private readonly Mobile m_Mobile;
            private int m_Count;
            private int m_MaxCount;
			private int m_Damage;
			private double m_Tactics;
            private readonly bool m_BloodDrinker;

            public BleedTimer(Mobile from, Mobile m, bool blooddrinker, int damage, int count, double tactics )
                : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                m_From = from;
                m_Mobile = m;
				m_Damage = damage;
                Priority = TimerPriority.TwoFiftyMS;
				m_BloodDrinker = blooddrinker;

                m_MaxCount = count;
				m_Tactics = tactics;
			}

            protected override void OnTick()
            {
                if (!m_Mobile.Alive || m_Mobile.Deleted)
                {
                    EndBleed(m_Mobile, true);
                }
                else
                {
					int hitsPercent = 0;
					if( m_Mobile.HitsMax != 0 )
						hitsPercent = ( m_Mobile.Hits * 100 ) / m_Mobile.HitsMax;
					
					int bloodDamage = (int)( m_Damage * ( 1 + hitsPercent * 0.02 + m_MaxCount * 0.025) );
					
					
					if( bloodDamage < 0 )
					{
						return;
					}
									
					
                    //int bloodDamage = HitsPercentDamage(m_Mobile, m_From, m_MaxCount, m_Tactics);

                    //if (!Server.Spells.SkillMasteries.WhiteTigerFormSpell.HasBleedMod(m_From, out damage))
                    //    damage = Math.Max(1, Utility.RandomMinMax(5 - m_Count, (5 - m_Count) * 2));

                    DoBleed(m_Mobile, m_From, bloodDamage, m_BloodDrinker);

                    if (++m_Count == 5)
                        EndBleed(m_Mobile, true);
                }
            }
        }
    }
}
