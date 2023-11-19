using System; 

namespace Server.Items
{ 
	public class OnionSeed : BaseSeed 
	{ 
		[Constructable]
		public OnionSeed() : this( 1 )
		{
		}

		[Constructable]
		public OnionSeed( int amount ) : base( 0xDCF )
		{
			Hue = 0x1BF; 
			Name = "양파 씨앗"; 
		}
		
		public OnionSeed( Serial serial ) : base( serial ) 
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
