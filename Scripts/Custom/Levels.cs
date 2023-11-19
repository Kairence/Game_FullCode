using System;
using System.Text;
using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
	public class Levels
	{

		public static int SkillExp( PlayerMobile pm, double skillvalue, int skill )
		{
			double maxvalue = 1000;
			if( skillvalue < 10.0 )
				maxvalue = 1000 + skillvalue * 100; //00.1 ~ 10.0 스킬 포인트. 100포인트 씩 증가
			else if( skillvalue < 20.0 )
				maxvalue = 2000 + ( skillvalue - 10.0 ) * 200; //10.1 ~ 10.0 스킬 포인트. 20포인트 씩 증가
			else if( skillvalue < 30.0 )
				maxvalue = 4000 + ( skillvalue - 20.0 ) * 300; //20.1 ~ 30.0 스킬 포인트. 20포인트 씩 증가
			else if( skillvalue < 40.0 )
				maxvalue = 7000 + ( skillvalue - 30.0 ) * 500; //30.1 ~ 40.0 스킬 포인트. 30포인트 씩 증가
			else if( skillvalue < 50.0 )
				maxvalue = 12000 + ( skillvalue - 40.0 ) * 800; //40.1 ~ 50.0 스킬 포인트. 40포인트 씩 증가
			else if( skillvalue < 60.0 )
				maxvalue = 20000 + ( skillvalue - 50.0 ) * 1000; //50.1 ~ 60.0 스킬 포인트. 50포인트 씩 증가
			else if( skillvalue < 70.0 )
				maxvalue = 30000 + ( skillvalue - 60.0 ) * 1500; //60.1 ~ 70.0 스킬 포인트. 75포인트 씩 증가
			else if( skillvalue < 80.0 )
				maxvalue = 45000 + ( skillvalue - 70.0 ) * 2250; //70.1 ~ 80.0 스킬 포인트. 100포인트 씩 증가
			else if( skillvalue < 90.0 )
				maxvalue = 67500 + ( skillvalue - 80.0 ) * 3250; //80.1 ~ 90.0 스킬 포인트. 150포인트 씩 증가
			else if( skillvalue < 100.0 )
				maxvalue = 100000 + ( skillvalue - 90.0 ) * 5000; //90.1 ~ 100.0 스킬 포인트. 250포인트 씩 증가
			else if( skillvalue < 105.0 )
				maxvalue = 150000 + ( skillvalue - 100.0 ) * 30000; //100.1 ~ 105.0 스킬 포인트. 500포인트 씩 증가
			else if( skillvalue < 110.0 )
				maxvalue = 300000 + ( skillvalue - 104.0 ) * 70000; //105.1 ~ 110.0 스킬 포인트. 1000포인트 씩 증가
			else if( skillvalue < 115.0 )
				maxvalue = 650000 + ( skillvalue - 110.0 ) * 150000; //110.1 ~ 115.0 스킬 포인트. 5000포인트 씩 증가
			else if( skillvalue < 120.0 )
				maxvalue = 14000000 + ( skillvalue - 115.0 ) * 500000; //115.1 ~ 120.0 스킬 포인트. 10000포인트 씩 증가
			else if( skillvalue >= 120.0 )
				maxvalue = 4000000 + ( skillvalue - 120.0 ) * 1000000; //120.0 ~ 스킬 포인트. 20000포인트 씩 증가
				maxvalue -= pm.SkillList[skill];
				if( (int)maxvalue < maxvalue )
					maxvalue++;
			return (int)maxvalue / 10;
		}

		public const int MaxGoldLevel = 20;
		private static int[] m_GoldExps = new int[]
		{
			1000,
			2000,
			3000,
			4000,
			7000,
			10000,
			15000,
			25000,
			40000,
			60000,
			100000,
			150000,
			250000,
			400000,
			600000,
			1000000,
			1500000,
			2500000,
			5000000,
			10000000
		};

		public static int GoldExp( int level )
		{
			if( level < MaxGoldLevel )
				return m_GoldExps[ level ];
			else
				return 2000000000;
		}


		public const int MaxLevel = 100;
		
		public static int ExpCal( PlayerMobile pm )
		{
			if( pm.Level < MaxLevel )
				return m_Exps[ pm.Level + 1 ] - pm.Exp;
			else
				return 0;
		}
		public static int ExpCal( BaseCreature bc )
		{
			if( bc.Level < MaxLevel )
				return m_Exps[ bc.Level + 1 ] - bc.Exp;
			else
				return 0;
		}
		public static int HitsCal( PlayerMobile pm )
		{
				return m_Hits[ pm.Level];
		}
		public static int StamCal( PlayerMobile pm )
		{
				return m_Stam[ pm.Level];
		}

		private static void LevelUp( Mobile m )
		{
			Effects.SendLocationParticles( EffectItem.Create( m.Location, m.Map, EffectItem.DefaultDuration ), 0, 0, 0, 0, 0, 5060, 0 );
			Effects.PlaySound( m.Location, m.Map, 0x243 );

			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( m.X - 6, m.Y - 6, m.Z + 15 ), m.Map ), m, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( m.X - 4, m.Y - 6, m.Z + 15 ), m.Map ), m, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( m.X - 6, m.Y - 4, m.Z + 15 ), m.Map ), m, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			m.Hits = m.HitsMax;
			m.Mana = m.ManaMax;
			m.Stam = m.StamMax;
		}
		
		public static void AwardLevel( Mobile m, int offset )
		{
			if ( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if ( offset < 1 || pm.Level >= MaxLevel )
						return;

				if ( pm.Young )
				{
					offset *= 80;
					offset /= 100;
				}
					
				if( pm.Level < MaxLevel && pm.Exp + offset >= m_Exps[ pm.Level + 1 ] )
				{
					pm.Level++;
					if( pm.Level > 90 )
						pm.StatCap = 90 + pm.Level * 2;
					else
						pm.StatCap = pm.Level + 90;
					pm.Exp = m_Exps[ pm.Level ];
					LevelUp( m );
				}
				else
					pm.Exp += offset;

				SkillCheck.LevelStatGain(m);
				if( pm.Level > 90 )
					SkillCheck.LevelStatGain(m);
			}
			else if ( m is BaseCreature )
			{
				BaseCreature bc = m as BaseCreature;
				if ( offset < 1 || bc.Level >= MaxLevel )
					return;
				if ( bc.Controlled && !bc.Summoned )
				{
					if( bc.Level < MaxLevel && bc.Exp + offset >= m_Exps[ bc.Level + 1 ] )
					{
						bc.Level++;
						bc.Exp = m_Exps[ bc.Level ];
						bc.Upgrade();
						LevelUp( m );
						SkillCap( bc );
					}
					else
						bc.Exp += offset;
				}
			}
		}

		public static void SkillCap( BaseCreature bc )
		{
			for (int i = 0; i < bc.Skills.Length; ++i)
			{
                bc.Skills[i].Cap = bc.Level;
			}
		}
		
		private static int[] m_Exps = new int[]
		{
			0,
			100,
			220,
			356,
			499,
			659,
			888,
			1265,
			1758,
			2404,
			3205,
			4525,
			6077,
			7885,
			9974,
			12371,
			15108,
			18219,
			21741,
			25715,
			30187,
			37205,
			45126,
			54038,
			64042,
			75246,
			87771,
			101748,
			117323,
			134655,
			153921,
			181313,
			211844,
			245828,
			283611,
			325572,
			372129,
			423742,
			480917,
			544208,
			614229,
			691652,
			777217,
			871739,
			976113,
			1091324,
			1218457,
			1358702,
			1513373,
			1683910,
			1871901,
			2099091,
			2349800,
			2626380,
			2931418,
			3267760,
			3638536,
			4047189,
			4497508,
			4993659,
			5540225,
			6142247,
			6805272,
			7535399,
			8339339,
			9224473,
			10198921,
			11271613,
			12452374,
			13752011,
			15182412,
			16812654,
			18607519,
			20583471,
			22758618,
			25152880,
			27788168,
			30688585,
			33880643,
			37393507,
			41259258,
			45513184,
			50194102,
			55344712,
			61011984,
			67247582,
			74108340,
			81656774,
			89961652,
			99098617,
			109150878,
			120515966,
			133477563,
			148665319,
			167251851,
			191497036,
			225846740,
			279151414,
			369146555,
			553061211,
			1000000000
		};
		private static int[] m_Hits = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15,
			16,
			17,
			18,
			19,
			20,
			22,
			24,
			26,
			28,
			30,
			32,
			34,
			36,
			38,
			40,
			42,
			44,
			46,
			48,
			50,
			52,
			54,
			56,
			58,
			60,
			63,
			66,
			69,
			72,
			75,
			78,
			81,
			84,
			87,
			90,
			93,
			96,
			99,
			102,
			105,
			108,
			111,
			114,
			117,
			120,
			124,
			128,
			132,
			136,
			140,
			144,
			148,
			152,
			156,
			160,
			164,
			168,
			172,
			176,
			180,
			184,
			188,
			192,
			196,
			200,
			205,
			210,
			215,
			220,
			225,
			230,
			235,
			240,
			245,
			250,
			255,
			260,
			265,
			270,
			275,
			280,
			285,
			290,
			295,
			300
		};
		private static int[] m_Stam = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			2,
			2,
			3,
			3,
			4,
			4,
			5,
			5,
			6,
			6,
			7,
			7,
			8,
			8,
			9,
			9,
			10,
			10,
			11,
			11,
			12,
			12,
			13,
			13,
			14,
			14,
			15,
			15,
			16,
			16,
			17,
			17,
			18,
			18,
			19,
			19,
			20,
			20,
			21,
			21,
			22,
			22,
			23,
			23,
			24,
			24,
			25,
			25,
			26,
			26,
			27,
			27,
			28,
			28,
			29,
			29,
			30,
			30,
			31,
			31,
			32,
			32,
			33,
			33,
			34,
			34,
			35,
			35,
			36,
			36,
			37,
			37,
			38,
			38,
			39,
			39,
			40,
			41,
			42,
			43,
			44,
			45,
			46,
			47,
			48,
			49,
			50
		};
	}
}