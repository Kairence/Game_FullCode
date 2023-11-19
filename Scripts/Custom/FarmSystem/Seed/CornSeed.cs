using System; 

namespace Server.Items
{ 
	public class CornSeed : BaseSeed 
	{ 
		[Constructable]
		public CornSeed() : this( 1 )
		{
		}

		[Constructable]
		public CornSeed( int amount ) : base( 0xDCF )
		{
			Hue = 0x160; 
			Name = "옥수수 씨앗"; 
		}
		
		public CornSeed( Serial serial ) : base( serial ) 
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
