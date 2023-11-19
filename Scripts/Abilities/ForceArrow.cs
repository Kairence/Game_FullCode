using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Spells;
using Server.Mobiles;

namespace Server.Items
{
    public class ForceArrow : WeaponAbility
    {
        public ForceArrow()
        {
        }
        public override int BaseMana
        {
            get
            {
                return 15;
            }
        }
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
    }
}