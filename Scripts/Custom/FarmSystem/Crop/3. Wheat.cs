using System; 
using System.Collections;
using Server.Mobiles; 
using Server.Targeting;

namespace Server.Items 
{ 
	public class Wheat : Item
	{
		[Constructable]
		public Wheat() : this( 1 )
		{
		}

		[Constructable]
		public Wheat( int amount ) : base( 0x1EBD )
		{
			Stackable = true;
			Weight = 0.01;
			Amount = amount;
		}

		public Wheat( Serial serial ) : base( serial )
		{
		}
		private class PickMillerTarget : Target
		{
			private Wheat m_Wheat;

			public PickMillerTarget( Wheat wheat ) : base( 3, false, TargetFlags.None )
			{
				m_Wheat = wheat;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Wheat.Deleted )
					return;

				IFlourMill mill = targeted as IFlourMill;

				if (mill == null && targeted is AddonComponent )
					mill = ((AddonComponent)targeted).Addon as IFlourMill;

				if ( mill is Item )
				{
					Item item = (Item)mill;

					if ( !m_Wheat.IsChildOf( from.Backpack ) )
					{
						from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
					}
					//else if ( mill.AddonLife == 0 )
					//	from.SendMessage("사용할 수 없습니다!");

					else
					{
						m_Wheat.Consume(5);
					//	mill.AddonLife -= 1;
						from.AddToBackpack( new SackFlour() );
					}
				}
				else
				{
					from.SendLocalizedMessage( 1044491 ); // Use that on a spinning wheel.
				}
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // What spinning wheel do you wish to spin this on?
				from.Target = new PickMillerTarget( this );
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
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
