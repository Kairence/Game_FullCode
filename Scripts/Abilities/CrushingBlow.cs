using System;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// Also known as the Haymaker, this attack dramatically increases the damage done by a weapon reaching its mark.
    /// </summary>

    public class CrushingBlow : WeaponAbility
    {
        public CrushingBlow()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 15;
            }
        }
        public override double DamageScalar
        {
            get
            {
                return 0;
            }
        }
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker))
                return;
					
			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 15 )
					return;
				attacker.Stam -= 15;
				
			}

            ClearCurrentAbility(attacker);

			attacker.SendLocalizedMessage(1060090); // You have delivered a crushing blow!

			int x = 0;
			int y = 0;
			if( attacker.Direction == Direction.East || (int)attacker.Direction == 130 ) // x = 1
			{
				x = 1;
			}
			else if( attacker.Direction == Direction.West || (int)attacker.Direction == 134 ) // x= -1
			{
				x = -1;
			}
			else if( attacker.Direction == Direction.South || (int)attacker.Direction == 132 ) // y = 1;
			{
				y = 1;
			}
			else if( attacker.Direction == Direction.North || (int)attacker.Direction == 128 ) // y= -1
			{
				y = -1;
			}
			else if( attacker.Direction == Direction.Mask || attacker.Direction == Direction.Up || (int)attacker.Direction == 134 ) //x = -1, y = -1
			{
				x = -1;
				y = -1;
			}
			else if( attacker.Direction == Direction.Down || (int)attacker.Direction == 131 ) // x = 1, y = 1
			{
				x = 1;
				y = 1;
			}
			else if( attacker.Direction == Direction.Left || (int)attacker.Direction == 133 ) // x = -1, y = 1
			{
				x = -1;
				y = 1;
			}
			else if( attacker.Direction == Direction.Right || (int)attacker.Direction == 129 ) // x = 1, y = -1
			{
				x = 1;
				y = -1;
			}
			int line = 5;
			double duration = 0.0;
			/*
			if( attacker is PlayerMobile )
			{
				PlayerMobile pm = attacker as PlayerMobile;
				//line += pm.SilverPoint[4] / 4;
				//duration += pm.SilverPoint[4] * 0.4;
			}
			*/
			if( attacker is OgreLord )
			{
				line += 9;
				duration = 5;
			}
			for( int i = 1; i <= line; i++ )
			{
				//attacker.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);
				
				Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(attacker.X + x * i, attacker.Y + y * i, attacker.Z + 50), attacker.Map), new Entity(Serial.Zero, new Point3D(attacker.X + x * i, attacker.Y + y * i, attacker.Z + 20), attacker.Map), 0xFB4, 1, 0, false, false, 0, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
				
				Point3D pnt = new Point3D(attacker.X + x * i, attacker.Y + y * i, attacker.Z);
				IPooledEnumerable mobiles = attacker.Map.GetMobilesInRange( pnt, 0 );

				foreach ( Mobile m in mobiles )
				{
					if ( m != attacker )
					{
						m.SendLocalizedMessage(1060091); // You feel disoriented!
						m.PlaySound(0x1E1);
						m.FixedParticles(0, 1, 0, 9946, EffectLayer.Head);
						if( m is BaseCreature )
						{
							BaseCreature bc = m as BaseCreature;
							duration *= MonsterTier(bc);
						}
						m.Freeze(TimeSpan.FromSeconds( duration ));
						AOS.Damage(m, attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
					}
				}
			}			
        }
	}
}