using System;
using Server;
using Server.Mobiles;
using Server.Multis;

namespace Server.Items
{
	public class BananaAddon : BaseAddon
	{
		//public override BaseAddonDeed Deed{ get{ return new SmallBedEastDeed(); } }

		private DateTime m_NextRecoveryTime = DateTime.Now + TimeSpan.FromHours( 400.0 );
		private int m_Life;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime NextRecoveryTime
		{
			get{ return m_NextRecoveryTime; }
			set{ m_NextRecoveryTime = value;}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Life
		{
			get{ return m_Life; }
			set{ m_Life = value;}
		}

		public void OnChop( Mobile from )
		{
			BaseHouse house = BaseHouse.FindHouseAt( from );

			if ( house != null && house.IsCoOwner( from ) )
			{
				Effects.PlaySound( Location, Map, 0x3B3 );
				from.SendLocalizedMessage( 500461 ); // You destroy the item.
				Delete();
			}
		}

		public override void OnComponentUsed( AddonComponent c, Mobile from )
		{
			if ( m_Life == 0 )
			{
				m_Life = 100;
			}
			PlayerMobile pm = from as PlayerMobile;
			int cookValue = (int)from.Skills[SkillName.Cooking].Value / 5;
			int IDPlus = (int)( from.Skills[SkillName.TasteID].Value + from.Skills[SkillName.ItemID].Value ) / 10;
			int roll = (int)( Utility.Random( 1000 ) + from.Skills[SkillName.Mysticism].Value );
			if ( from.InRange( c.Location, 2 ) )
			{
				if ( DateTime.Now < m_NextRecoveryTime )
				{
					from.SendMessage("This tree is not fruits.");
					return;
				}
				else
				{
					if( from.Stam < 15 )
					{
						from.SendMessage( String.Format("You need 15 stamina" ) ); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
					}
					else
					{
						int pick = cookValue + IDPlus;

						if( from.Skills.Musicianship.Value > Utility.RandomDouble() )
							pick = (int)( pick * Utility.RandomMinMax( 110, 150 ) * 0.01 );

						if ( roll < 300 )
							pick = (int)( pick * Utility.RandomMinMax( 0, 25 ) * 0.01 );
						else if( roll > 940 )
							pick = (int)( pick * Utility.RandomMinMax( 110, 125 ) * 0.01 );

					double Event = 1.0;
					Event ev = new Event();
					if ( ev.ServerEvent == 1 )
						Event = 1.25;
					//event
					pick = (int)( pick * Event);
					from.SendMessage( "You harvest {0} crop{1}!", pick, ( pick == 1 ? "" : "s" ) ); 

					Banana crop = new Banana( pick ); 
					from.AddToBackpack( crop );
					if ( from.Skills[SkillName.Lumberjacking].Value < Utility.Random( 240 ) )
						Life -= 1;
					if ( m_Life <= 0 )
						this.Delete();

					m_NextRecoveryTime = DateTime.Now + TimeSpan.FromHours( 120.0 ); // TODO: Proper time delay
					from.Stam -= 15;
					}
				}
			}
		}
		[Constructable]
		public BananaAddon()
		{
			AddComponent( new AddonComponent( 0xCAA ), 0, 0, 0 );
		}

		public BananaAddon( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 );

			writer.Write( m_Life );
			writer.WriteDeltaTime( m_NextRecoveryTime );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			switch ( version )
			{
				case 1:
				{
					Life = reader.ReadInt();
					NextRecoveryTime = reader.ReadDateTime();
					break;
				}
			}
		}
	}

	public class BananaDeed : BaseAddonDeed
	{
		public override BaseAddon Addon{ get{ return new BananaAddon(); } }
		//public override int LabelNumber{ get{ return 1044322; } } // small bed (east)

		[Constructable]
		public BananaDeed()
		{
			Name = "Banana Tree Deed";
		}

		public BananaDeed( Serial serial ) : base( serial )
		{
		}
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

	}
}