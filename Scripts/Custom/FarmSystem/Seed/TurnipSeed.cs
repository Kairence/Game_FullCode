using System; 

namespace Server.Items 
{ 
	public class TurnipSeed : BaseSeed 
	{ 
		[Constructable]
		public TurnipSeed() : this( 1 )
		{
		}

		[Constructable]
		public TurnipSeed( int amount ) : base( 0xDCF )
		{
			Hue = 0x1F6; 
			Name = "순무 씨앗"; 
		}
		
		public TurnipSeed( Serial serial ) : base( serial ) 
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
