using System; 

namespace Server.Items
{ 
	public class WheatSeed : BaseSeed 
	{ 
		[Constructable]
		public WheatSeed() : this( 1 )
		{
		}

		[Constructable]
		public WheatSeed( int amount ) : base( 0xDCF )
		{
			Hue = 45; 
			Name = "밀 씨앗"; 
		}
		
		public WheatSeed( Serial serial ) : base( serial ) 
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
