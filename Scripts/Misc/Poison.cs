#region References
using System;
using System.Globalization;

using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Services.Virtues;
using Server.Spells;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
#endregion

namespace Server
{
	public class PoisonImpl : Poison
	{
		[CallPriority(10)]
		public static void Configure()
		{
			if (Core.AOS)
			{
										//이름, 	 레벨,최소,추뎀,배수,딜레이,인터벌,카운트,메시지 카운트
				Register(new PoisonImpl("Lesser", 0, 0, 0, 20.0, 5.0, 5.00, 10000, 5));
				Register(new PoisonImpl("Regular", 1, 1001, 100, 15.0, 5.0, 5.00, 10000, 5));
				Register(new PoisonImpl("Greater", 2, 5001, 500, 10.0, 5.0, 5.00, 10000, 5));
				Register(new PoisonImpl("Deadly", 3, 20001, 1000, 5.0, 5.0, 5.00, 10000, 5));
				Register(new PoisonImpl("Lethal", 4, 100001, 2000, 2.5, 5.0, 5.00, 10000, 5));
			}
			else
			{
				Register(new PoisonImpl("Lesser", 0, 4, 26, 2.500, 3.5, 3.0, 10, 2));
				Register(new PoisonImpl("Regular", 1, 5, 26, 3.125, 3.5, 3.0, 10, 2));
				Register(new PoisonImpl("Greater", 2, 6, 26, 6.250, 3.5, 3.0, 10, 2));
				Register(new PoisonImpl("Deadly", 3, 7, 26, 12.500, 3.5, 4.0, 10, 2));
				Register(new PoisonImpl("Lethal", 4, 9, 26, 25.000, 3.5, 5.0, 10, 2));
			}

			#region Mondain's Legacy
			if (Core.ML)
			{
				Register(new PoisonImpl("LesserDarkglow", 10, 4, 16, 7.5, 3.0, 2.25, 10, 4));
				Register(new PoisonImpl("RegularDarkglow", 11, 8, 18, 10.0, 3.0, 3.25, 10, 3));
				Register(new PoisonImpl("GreaterDarkglow", 12, 12, 20, 15.0, 3.0, 4.25, 10, 2));
				Register(new PoisonImpl("DeadlyDarkglow", 13, 16, 30, 30.0, 3.0, 5.25, 15, 2));

				Register(new PoisonImpl("LesserParasitic", 14, 4, 16, 7.5, 3.0, 2.25, 10, 4));
				Register(new PoisonImpl("RegularParasitic", 15, 8, 18, 10.0, 3.0, 3.25, 10, 3));
				Register(new PoisonImpl("GreaterParasitic", 16, 12, 20, 15.0, 3.0, 4.25, 10, 2));
				Register(new PoisonImpl("DeadlyParasitic", 17, 16, 30, 30.0, 3.0, 5.25, 15, 2));
				Register(new PoisonImpl("LethalParasitic", 18, 20, 50, 35.0, 3.0, 5.25, 20, 2));
			}
			#endregion
		}

		public static Poison IncreaseLevel(Poison oldPoison)
		{
			Poison newPoison = oldPoison == null ? null : GetPoison(oldPoison.Level + 1);

			return newPoison ?? oldPoison;
		}

        public static Poison DecreaseLevel(Poison oldPoison)
        {
            Poison newPoison = (oldPoison == null ? null : GetPoison(oldPoison.Level - 1));

            return (newPoison == null ? oldPoison : newPoison);
        }

		// Info
		private readonly string m_Name;
		private readonly int m_Level;

		// Damage
		private readonly int m_Minimum;
		private readonly int m_Maximum;
		private readonly double m_Scalar;

		// Timers
		private readonly TimeSpan m_Delay;
		private readonly TimeSpan m_Interval;
		private readonly int m_Count;

		private readonly int m_MessageInterval;

		public override string Name { get { return m_Name; } }
		public override int Level { get { return m_Level; } }

		#region Mondain's Legacy
		public override int RealLevel
		{
			get
			{
				if (m_Level >= 14)
				{
					return m_Level - 14;
				}
				
				if (m_Level >= 10)
				{
					return m_Level - 10;
				}

				return m_Level;
			}
		}

		public override int LabelNumber
		{
			get
			{
				if (m_Level >= 14)
				{
					return 1072852; // parasitic poison charges: ~1_val~
				}
				
				if (m_Level >= 10)
				{
					return 1072853; // darkglow poison charges: ~1_val~
				}

				return 1062412 + m_Level; // ~poison~ poison charges: ~1_val~
			}
		}
		#endregion

		public PoisonImpl(
			string name,
			int level,
			int min,
			int max,
			double percent,
			double delay,
			double interval,
			int count,
			int messageInterval)
		{
			m_Name = name;								//독 이름
			m_Level = level;							//독 레벨
			m_Minimum = min;							//독 최소 단계(0, 1001, 5001, 20001, 100001)
			m_Maximum = max;							//독 추뎀(0, 100, 500, 1000, 2000)
			m_Scalar = percent;							//총 독량의 피해(20%, 15%, 10%, 5%, 2.5%)
			m_Delay = TimeSpan.FromSeconds(delay);		//5초 마다 독 발생
			m_Interval = TimeSpan.FromSeconds(interval);//5초 마다 독 발생
			m_Count = count;							//총 1만번(무한)
			m_MessageInterval = messageInterval;		//메시지는 독 5회당 발생
		}

		public override Timer ConstructTimer(Mobile m)
		{
			return new PoisonTimer(m, this);
		}

		public class PoisonTimer : Timer
		{
			private readonly PoisonImpl m_Poison;
			private readonly Mobile m_Mobile;
			private Mobile m_From;
			private int m_LastDamage;
			private int m_Index;

			public Mobile From { get { return m_From; } set { m_From = value; } }

			public PoisonTimer(Mobile m, PoisonImpl p)
				: base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
			{
				m_From = m;
				m_Mobile = m;
				m_Poison = p;

				int damage = damage = Utility.RandomMinMax( p.m_Minimum, p.m_Maximum );
				/*
				if( m_Mobile is PlayerMobile )
				{
					damage = (int)( m_Mobile.Stam * p.m_Scalar * 0.01 );
				}
				else
				{
					
					if( m_Mobile is BaseCreature )
					{
						BaseCreature bc = m_Mobile as BaseCreature;
						if( bc.Controlled )
						{
							if( damage < m_Mobile.Stam * p.m_Scalar * 0.01 )
								damage = (int)( m_Mobile.Stam * p.m_Scalar * 0.01 );
						}
					}
				}
				*/
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Poison, 1017383, 1075633, TimeSpan.FromSeconds((int)((p.m_Count + 1) * p.m_Interval.TotalSeconds)), m, String.Format("{0}\t{1}", damage, (int)p.m_Interval.TotalSeconds)));
            }

			private readonly int[] PoisonLevel =
			{
				0, 1000, 5000, 20000, 100000
			};

			private readonly int[] PoisonPlus =
			{
				0, 100, 500, 1000, 2000
			};
			private readonly double[] PoisonPercent =
			{
				0.2, 0.15, 0.1, 0.5, 0.25
			};

            protected override void OnTick()
            {
				int poisonDamage = 0;
				int absorbDamage = Misc.Util.PoisonAbsorbDamage(m_Mobile);
				if( m_Mobile is PlayerMobile )
				{
					PlayerMobile pm = m_Mobile as PlayerMobile;
					poisonDamage = pm.PoisonSaving;

				}
				else if( m_Mobile is BaseCreature )
				{
					BaseCreature bc = m_Mobile as BaseCreature;
					poisonDamage = bc.PoisonSaving;
				}
				
				int poisonCheck = 0;
				for( int i = 4; i < 0; --i)
				{
					if( poisonDamage > PoisonLevel[i] )
					{
						poisonCheck = i;
						break;
					}
				}
				poisonDamage = (int)( poisonDamage * PoisonPercent[m_Poison.RealLevel] ) + PoisonPlus[m_Poison.RealLevel];

				switch(poisonCheck)
				{
					case 4:
					{
						m_Mobile.Poison = Poison.Lethal;
						break;
					}
					case 3:
					{
						m_Mobile.Poison = Poison.Deadly;
						break;
					}
					case 2:
					{
						m_Mobile.Poison = Poison.Greater;
						break;
					}
					case 1:
					{
						m_Mobile.Poison = Poison.Regular;
						break;
					}
					default:
					{
						m_Mobile.Poison = Poison.Lesser;
						break;
					}
				}

				int damage = poisonDamage - absorbDamage;
				//독 피해
				if( damage > 0 )
					AOS.Damage(m_Mobile, m_From, damage, 0, 0, 0, 100, 0);
				else
					m_Mobile.LocalOverheadMessage(MessageType.Emote, 0x3F, 1053092); // * You feel yourself resisting the effects of the poison *

				//독이 모두 제거되었는지 체크
                if (m_Index++ == m_Poison.m_Count || poisonDamage < 100 )
                {
					if( m_Mobile is PlayerMobile )
					{
						PlayerMobile pm = m_Mobile as PlayerMobile;
						pm.PoisonSaving = 0;

						//스텟 독 저항성
						absorbDamage += pm.Str * 2;
					}
					else if( m_Mobile is BaseCreature )
					{
						BaseCreature bc = m_Mobile as BaseCreature;
						bc.PoisonSaving = 0;
					}

                    m_Mobile.SendLocalizedMessage(502136); // The poison seems to have worn off.
                    m_Mobile.Poison = null;
					
                    if (m_Mobile is PlayerMobile)
                        BuffInfo.RemoveBuff((PlayerMobile)m_Mobile, BuffIcon.Poison);

                    Stop();
                    return;
                }
				
				/*
                int damage;

                if (!Core.AOS && m_LastDamage != 0 && Utility.RandomBool())
                {
                    damage = m_LastDamage;
                }
                else
                {
					if( m_Mobile is PlayerMobile )
					{
						damage = (int)( m_Mobile.Stam * m_Poison.m_Scalar * 0.01 );
						if( poisonvalue > 0 )
						{
							if( poisonvalue >= 100 )
								poisonvalue += 30;
							double[] bonusPoison = {0.03, 0.06, 0.09, 0.12, 0.15 };
							damage = (int)( m_Mobile.Stam * ( m_Poison.m_Scalar * 0.01 + poisonvalue * bonusPoison[m_Poison.RealLevel] * 0.01 ) );
						}
						
					}
					else
					{
						damage = Utility.RandomMinMax( m_Poison.m_Minimum, m_Poison.m_Maximum );
						if( m_Mobile is BaseCreature )
						{
							BaseCreature bc = m_Mobile as BaseCreature;
							if( bc.Controlled )
							{
								if( damage < m_Mobile.Stam * m_Poison.m_Scalar * 0.01 )
									damage = (int)( m_Mobile.Stam * m_Poison.m_Scalar * 0.01 );
							}
						}
					}

					/*
                    damage = 1 + (int)(m_Mobile.Hits * m_Poison.m_Scalar);

                    if (damage < m_Poison.m_Minimum)
                        damage = m_Poison.m_Minimum;
                    else if (damage > m_Poison.m_Maximum)
                        damage = m_Poison.m_Maximum;
                    m_LastDamage = damage;
                }

                if (m_From != null)
                {
                    if (m_From is BaseCreature && ((BaseCreature)m_From).RecentSetControl && ((BaseCreature)m_From).GetMaster() == m_Mobile)
                    {
                        m_From = null;
                    }
                    else
                    {
                        m_From.DoHarmful(m_Mobile, true);
                    }
                }

                IHonorTarget honorTarget = m_Mobile as IHonorTarget;

                if (honorTarget != null && honorTarget.ReceivedHonorContext != null)
                    honorTarget.ReceivedHonorContext.OnTargetPoisoned();

                #region Mondain's Legacy
                if (Core.ML)
                {
                    if (m_From != null && m_Mobile != m_From && !m_From.InRange(m_Mobile.Location, 1) && m_Poison.m_Level >= 10 && m_Poison.m_Level <= 13) // darkglow
                    {
                        m_From.SendLocalizedMessage(1072850); // Darkglow poison increases your damage!
                        damage = (int)Math.Floor(damage * 1.1);
                    }

                    if (m_From != null && m_Mobile != m_From && m_From.InRange(m_Mobile.Location, 1) && m_Poison.m_Level >= 14 && m_Poison.m_Level <= 18) // parasitic
                    {
                        int toHeal = Math.Min(m_From.HitsMax - m_From.Hits, damage);

                        if (toHeal > 0)
                        {
                            m_From.SendLocalizedMessage(1060203, toHeal.ToString()); // You have had ~1_HEALED_AMOUNT~ hit points of damage healed.
                            m_From.Heal(toHeal, m_Mobile, false);
                        }
                    }
                }
                #endregion

                AOS.Damage(m_Mobile, m_From, damage, 0, 0, 0, 100, 0);

                if (damage > 0)
                {
                    m_Mobile.RevealingAction();
                }

                if ((m_Index % m_Poison.m_MessageInterval) == 0)
                    m_Mobile.OnPoisoned(m_From, m_Poison, m_Poison);
				*/
            }
        }
	}
}
