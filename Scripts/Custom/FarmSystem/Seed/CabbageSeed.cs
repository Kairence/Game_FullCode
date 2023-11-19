using System; 

namespace Server.Items
{ 
	public class CabbageSeed : BaseSeed
	{ 
		[Constructable]
		public CabbageSeed() : this( 1 )
		{
		}

		[Constructable]
		public CabbageSeed( int amount ) : base( 0xDCF )
		{
			Hue = 0x232; 
			Name = "양배추 씨앗"; 
		}
		
		public CabbageSeed( Serial serial ) : base( serial ) 
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
