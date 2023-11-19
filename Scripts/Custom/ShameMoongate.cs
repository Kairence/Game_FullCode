using System;
using Server.Items;
using Server.Gumps;

namespace Server.Items
{
	public class ShameMoongate : Item
	{
		public override string DefaultName
		{
			get { return "쉐임 문게이트"; }
		}

		[Constructable]
		public ShameMoongate() : base( 0x1FD4 )
		{
			Movable = false;
         	Hue = 0;
		}

		public override bool OnMoveOver( Mobile m )
		{
			if ( m.Player )
				CheckGate( m );

			return true;
		}
		public virtual void CheckGate( Mobile m )
		{
			#region Mondain's Legacy
			if ( m.Hidden && m.AccessLevel == AccessLevel.Player && Core.ML )
			{
				m.RevealingAction();
			}
			m.CloseGump( typeof( ShameMoongateGump ) );
			m.SendGump( new ShameMoongateGump( m, this ) );
			#endregion
		}
		
		public ShameMoongate( Serial serial ) : base( serial )
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