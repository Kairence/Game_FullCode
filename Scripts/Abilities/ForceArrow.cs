using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;

namespace Server.Items
{
    public class ForceArrow : WeaponAbility
    {
        public ForceArrow()
        {
        }
        private static readonly Dictionary<Mobile, BleedTimer> m_BleedTable = new Dictionary<Mobile, BleedTimer>();

        public override int BaseMana
        {
            get
            {
                return 15;
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
		
		/*
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
			if (!Validate(attacker))
				return;
			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 10 )
					return;
				attacker.Stam -= 10;
			}
			attacker.PlaySound(0x64C);
            attacker.SendLocalizedMessage(1074381); // You fire an arrow of pure force.
			if( attacker is OgreLord )
			{
				int x = 0;
				int y = 0;
				if( attacker.Location.X < defender.Location.X && attacker.Location.Y < defender.Location.Y ) // x = 1
				{
					x = defender.Location.X - attacker.Location.X;
					y = defender.Location.Y - attacker.Location.Y;
				}
				else if( attacker.Location.X >= defender.Location.X && attacker.Location.Y < defender.Location.Y )
				{
					x = attacker.Location.X - defender.Location.X;
					y = defender.Location.Y - attacker.Location.Y;
				}
				else if( attacker.Location.X < defender.Location.X && attacker.Location.Y >= defender.Location.Y )
				{
					x = defender.Location.X - attacker.Location.X;
					y = attacker.Location.Y - defender.Location.Y;
				}
				else
				{
					x = attacker.Location.X - defender.Location.X;
					y = attacker.Location.Y - defender.Location.Y;
				}
				
				if( attacker.Location.X == defender.Location.X && attacker.Location.Y == defender.Location.Y )
				{
					
				}
				else
				{
					double xstep = Math.Abs( attacker.Location.X - defender.Location.X );
					double ystep = Math.Abs( attacker.Location.Y - defender.Location.Y );
					int count = 1;
					if( xstep < ystep )
					{
						count = (int)ystep;
						xstep /= ystep;
						ystep /= count;
						if( attacker.Location.Y > defender.Location.Y )
							ystep *= -1;
						if( attacker.Location.X > defender.Location.X )
							xstep *= -1;
					}
					else
					{
						count = (int)xstep;
						ystep /= xstep;
						xstep /= count;
						if( attacker.Location.Y > defender.Location.Y )
							ystep *= -1;
						if( attacker.Location.X > defender.Location.X )
							xstep *= -1;
					}
					for( int i = 1; i <= count; i++ )
					{
						Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(attacker.X + (int)( xstep * i ), attacker.Y + (int)( ystep * i ), attacker.Z - 10), attacker.Map), new Entity(Serial.Zero, new Point3D(attacker.X + (int)(xstep * i ), attacker.Y + (int)(ystep * i ), attacker.Z + 50), attacker.Map), 0x2255, 1, 0, false, false, 13, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
						
						Point3D pnt = new Point3D(attacker.X + (int)(xstep * i ), attacker.Y + (int)(ystep * i ), attacker.Z);
						IPooledEnumerable mobiles = attacker.Map.GetMobilesInRange( pnt, 0 );

						foreach ( Mobile m in mobiles )
						{
							if ( m != attacker )
							{
								m.SendLocalizedMessage(1060091); // You feel disoriented!
								m.PlaySound(0x1E1);
								m.FixedParticles(0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head);
								m.Freeze(TimeSpan.FromSeconds( 5.0 ));
								AOS.Damage(m, attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
							}
						}
					}
				}
			}
			else
			{
				defender.SendLocalizedMessage(1074382); // You are struck by a force arrow!
				defender.PlaySound(0x1E1);
				defender.FixedParticles(0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head);
				AOS.Damage(defender, attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
			}
            ClearCurrentAbility(attacker);


            //defender.SendLocalizedMessage(1074382); // You are struck by a force arrow!
        }

        private static Dictionary<Mobile, List<ForceArrowInfo>> m_Table = new Dictionary<Mobile, List<ForceArrowInfo>>();

        public static void BeginForceArrow(Mobile attacker, Mobile defender)
        {
            ForceArrowInfo info = new ForceArrowInfo(attacker, defender);
            info.Timer = new ForceArrowTimer(info);

            if (!m_Table.ContainsKey(attacker))
                m_Table[attacker] = new List<ForceArrowInfo>();

            m_Table[attacker].Add(info);

            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ForceArrow, 1151285, 1151286, info.DefenseChanceMalus.ToString()));
        }

        public static void EndForceArrow(ForceArrowInfo info)
        {
            if (info == null)
                return;

            Mobile attacker = info.Attacker;

            if (m_Table.ContainsKey(attacker) && m_Table[attacker].Contains(info))
            {
                m_Table[attacker].Remove(info);

                if (m_Table[attacker].Count == 0)
                    m_Table.Remove(attacker);
            }

            BuffInfo.RemoveBuff(info.Defender, BuffIcon.ForceArrow);
        }

        public static bool HasForceArrow(Mobile attacker, Mobile defender)
        {
            if (!m_Table.ContainsKey(attacker))
                return false;

            foreach (ForceArrowInfo info in m_Table[attacker])
            {
                if (info.Defender == defender)
                    return true;
            }

            return false;
        }

        public static ForceArrowInfo GetInfo(Mobile attacker, Mobile defender)
        {
            if (!m_Table.ContainsKey(attacker))
                return null;

            foreach (ForceArrowInfo info in m_Table[attacker])
            {
                if (info.Defender == defender)
                    return info;
            }

            return null;
        }

        public class ForceArrowInfo
        {
            private Mobile m_Attacker;
            private Mobile m_Defender;
            private ForceArrowTimer m_Timer;
            private int m_DefenseChanceMalus;

            public Mobile Attacker { get { return m_Attacker; } }
            public Mobile Defender { get { return m_Defender; } }
            public ForceArrowTimer Timer { get { return m_Timer; } set { m_Timer = value; } }
            public int DefenseChanceMalus { get { return m_DefenseChanceMalus; } set { m_DefenseChanceMalus = value; } }

            public ForceArrowInfo(Mobile attacker, Mobile defender)
            {
                m_Attacker = attacker;
                m_Defender = defender;
                m_DefenseChanceMalus = 10;
            }
        }

        public class ForceArrowTimer : Timer
        {
            private ForceArrowInfo m_Info;
            private DateTime m_Expires;

            public ForceArrowTimer(ForceArrowInfo info)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1))
            {
                m_Info = info;
                Priority = TimerPriority.OneSecond;

                m_Expires = DateTime.UtcNow + TimeSpan.FromSeconds(10);

                Start();
            }

            protected override void OnTick()
            {
                if (m_Expires < DateTime.UtcNow)
                {
                    Stop();
                    EndForceArrow(m_Info);
                }
            }

            public void IncreaseExpiration()
            {
                m_Expires = m_Expires + TimeSpan.FromSeconds(2);

                m_Info.DefenseChanceMalus += 5;
            }
        }
		*/
    }
}