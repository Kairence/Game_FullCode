using System; 

namespace Server.Items
{ 
	[FlipableAttribute( 0xc7f, 0xc81 )]
	public class Corn : Food
	{
		[Constructable]
		public Corn() : this( 1 )
		{
		}

		[Constructable]
		public Corn( int amount ) : base( amount, 0xc7f )
		{
			this.Weight = 0.01;
			this.FillFactor = 0;
		}

		public Corn( Serial serial ) : base( serial )
		{
		}

		//public override Item Dupe( int amount )
		//{
		//	return base.Dupe( new Corn(), amount );
		//}

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