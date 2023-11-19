using System; 

namespace Server.Items
{ 
	public class LettuceSeed : BaseSeed 
	{ 
		[Constructable]
		public LettuceSeed() : this( 1 )
		{
		}

		[Constructable]
		public LettuceSeed( int amount ) : base( 0xDCF )
		{
			Hue = 0x1D8; 
			Name = "상추 씨앗"; 
		}
		
		public LettuceSeed( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); 
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
		} 
	} 
} 
