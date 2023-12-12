// Treasure Chest Pack - Version 0.99I
// By Nerun

using Server;
using Server.Items;
using Server.Multis;
using Server.Network;
using System;
using Server.Regions;
using Server.Misc;

namespace Server.Items
{
// ---------- [Level 1] ----------
// Large, Medium and Small Crate
	[FlipableAttribute( 0xe3e, 0xe3f )] 
	public class TreasureLevel1 : BaseTreasureChestMod 
	{ 
		public override int DefaultGumpID{ get{ return 0x49; } }

		[Constructable] 
		public TreasureLevel1() : base( Utility.RandomList( 0xE3C, 0xE3E, 0x9a9 ) )
		{ 
			RequiredSkill = 20;
			LockLevel = this.RequiredSkill - Utility.Random( 0, 25 );
			if( LockLevel == 0 )
				LockLevel = 1;
			MaxLockLevel = this.RequiredSkill;
			TrapType = TrapType.None;
			//TrapPower = 1 * Utility.Random( 1, 25 );

			Item ReagentLoot = Loot.RandomReagent();
			ReagentLoot.Amount = Utility.Random( 3, 6 );
			DropItem( ReagentLoot );			

			if( Utility.RandomDouble() < 0.01 )
				DropItem( new TreasureMap( 1, Map.Trammel ) );

			if( Utility.RandomDouble() < 0.1 )
				DropItem( Loot.RandomGem() );

			
            //가난 드랍률
			for ( int i = 0; i < 3; i++ )
			{
				if( Utility.RandomDouble() < 0.1 )
				{
					Item item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

					if (item != null)
					{
						if( Utility.RandomDouble() < 0.1 )
							Util.NewItemCreate( item, 1 );
						
						DropItem( item );
					}
				}
			}
		}

		public TreasureLevel1( Serial serial ) : base( serial ) 
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

// ---------- [Level 1 Hybrid] ----------
// Large, Medium and Small Crate
	[FlipableAttribute( 0xe3e, 0xe3f )] 
	public class TreasureLevel1h : BaseTreasureChestMod 
	{ 
		public override int DefaultGumpID{ get{ return 0x49; } }

		[Constructable] 
		public TreasureLevel1h() : base( Utility.RandomList( 0xE3C, 0xE3E, 0x9a9 ) ) 
		{ 
			RequiredSkill = 30;
			LockLevel = this.RequiredSkill - Utility.Random( 0, 25 );
			MaxLockLevel = this.RequiredSkill;
			TrapType = TrapType.None;
			//TrapPower = 1 * Utility.Random( 1, 25 );

			switch ( Utility.Random( 3 )) 
			{ 
				case 0: DropItem( new BeverageBottle(BeverageType.Ale) ); break;
				case 1: DropItem( new BeverageBottle(BeverageType.Liquor) ); break;
				case 2: DropItem( new Jug(BeverageType.Cider) ); break;
			}
		} 

		public TreasureLevel1h( Serial serial ) : base( serial ) 
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

// ---------- [Level 2] ----------
// Large, Medium and Small Crate
// Wooden, Metal and Metal Golden Chest
// Keg and Barrel
	[FlipableAttribute( 0xe43, 0xe42 )] 
	public class TreasureLevel2 : BaseTreasureChestMod 
	{
		[Constructable] 
		public TreasureLevel2() : base( Utility.RandomList( 0xe3c, 0xE3E, 0x9a9, 0xe42, 0x9ab, 0xe40, 0xe7f, 0xe77 ) ) 
		{ 
			RequiredSkill = 40;
			LockLevel = this.RequiredSkill - Utility.Random( 0, 25 );
			MaxLockLevel = this.RequiredSkill;
			TrapType = TrapType.None;
			//TrapPower = 2 * Utility.Random( 1, 25 );

			Item ReagentLoot = Loot.RandomReagent();
			ReagentLoot.Amount = Utility.Random( 12, 15 );
			DropItem( ReagentLoot );			

			if( Utility.RandomDouble() < 0.01 )
				DropItem( new TreasureMap( 2, Map.Trammel ) );

			if( Utility.RandomDouble() < 0.2 )
				DropItem( Loot.RandomGem() );


            //취약 드랍률
			for ( int i = 0; i < 3; i++ )
			{
				if( Utility.RandomDouble() < 0.1 )
				{
					Item item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

					if (item != null)
					{
						double dice = Utility.RandomDouble();
						if( dice < 0.1 )
							Util.NewItemCreate( item, Utility.RandomMinMax(2, 3) );
						else
							Util.NewItemCreate( item, Utility.RandomMinMax(0, 1) );
						
					}
					DropItem( item );
				}
			}
		} 

		public TreasureLevel2( Serial serial ) : base( serial ) 
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

// ---------- [Level 3] ----------
// Wooden, Metal and Metal Golden Chest
	[FlipableAttribute( 0x9ab, 0xe7c )] 
	public class TreasureLevel3 : BaseTreasureChestMod 
	{ 
		public override int DefaultGumpID{ get{ return 0x4A; } }

		[Constructable] 
		public TreasureLevel3() : base( Utility.RandomList( 0x9ab, 0xe40, 0xe42 ) ) 
		{ 
			RequiredSkill = 60;
			LockLevel = this.RequiredSkill - Utility.Random( 0, 25 );
			MaxLockLevel = this.RequiredSkill;
			TrapType = TrapType.None;
			//TrapPower = 3 * Utility.Random( 1, 25 );

			Item ReagentLoot = Loot.RandomReagent();
			ReagentLoot.Amount = Utility.Random( 30, 36 );
			DropItem( ReagentLoot );			

			if( Utility.RandomDouble() < 0.01 )
				DropItem( new TreasureMap( 3, Map.Trammel ) );

			if( Utility.RandomDouble() < 0.3 )
				DropItem( Loot.RandomGem() );

			if( Utility.RandomDouble() < 0.1 )
				DropItem( Loot.RandomWand() );

            //평균 드랍률
			for ( int i = 0; i < 3; i++ )
			{
				if( Utility.RandomDouble() < 0.1 )
				{
					Item item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

					if (item != null)
					{
						double dice = Utility.RandomDouble();
						if( dice < 0.1 )
							Util.NewItemCreate( item, Utility.RandomMinMax(3, 4) );
						else
							Util.NewItemCreate( item, Utility.RandomMinMax(0, 2) );
						
					}
					DropItem( item );
				}
			}
		} 

		public TreasureLevel3( Serial serial ) : base( serial ) 
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

// ---------- [Level 4] ----------
// Wooden, Metal
	[FlipableAttribute( 0xe41, 0xe40 )] 
	public class TreasureLevel4 : BaseTreasureChestMod 
	{ 
		[Constructable] 
		public TreasureLevel4() : base( Utility.RandomList( 0xe40, 0xe42 ) )
		{ 
			RequiredSkill = 80;
			LockLevel = this.RequiredSkill - Utility.Random( 0, 25 );
			MaxLockLevel = this.RequiredSkill;
			TrapType = TrapType.None;
			//TrapPower = 4 * Utility.Random( 1, 25 );

			DropItem( new BlankScroll( Utility.Random( 4, 8 ) ) );
			
			Item ReagentLoot = Loot.RandomReagent();
			ReagentLoot.Amount = Utility.Random( 75, 90 );
			DropItem( ReagentLoot );			

			if( Utility.RandomDouble() < 0.01 )
				DropItem( new TreasureMap( 4, Map.Trammel ) );

			if( Utility.RandomDouble() < 0.4 )
				DropItem( Loot.RandomGem() );

			if( Utility.RandomDouble() < 0.15 )
				DropItem( Loot.RandomWand() );

            //부자 드랍률
			for ( int i = 0; i < 3; i++ )
			{
				double dice = Utility.RandomDouble();
				if( dice < 0.36 )
				{
					Item item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

					if (item != null)
					{
						if( dice < 0.01 )
							Util.NewItemCreate( item, Utility.RandomMinMax(4, 5) );
						else if( dice < 0.11 )
							Util.NewItemCreate( item, Utility.RandomMinMax(1, 3) );
						else
							Util.NewItemCreate( item, Utility.RandomMinMax(0, 2) );
					}
					DropItem( item );
				}
			} 
		}
		public TreasureLevel4( Serial serial ) : base( serial ) 
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
// ---------- [Level 5] ----------
//Metal Golden Chest
	[FlipableAttribute( 0x9ab )] 
	public class TreasureLevel5 : BaseTreasureChestMod 
	{ 
		[Constructable] 
		public TreasureLevel5() : base( 0x9ab )
		{ 
			RequiredSkill = 100;
			LockLevel = this.RequiredSkill - Utility.Random( 0, 25 );
			MaxLockLevel = this.RequiredSkill;
			TrapType = TrapType.None;
			//TrapPower = 4 * Utility.Random( 1, 25 );

			DropItem( new BlankScroll( Utility.Random( 1, 4 ) ) );
			
			Item ReagentLoot = Loot.RandomReagent();
			ReagentLoot.Amount = Utility.Random( 390, 420 );
			DropItem( ReagentLoot );			

			if( Utility.RandomDouble() < 0.01 )
				DropItem( new TreasureMap( 5, Map.Trammel ) );

			if( Utility.RandomDouble() < 0.5 )
				DropItem( Loot.RandomGem() );

			if( Utility.RandomDouble() < 0.2 )
				DropItem( Loot.RandomWand() );

            //꽤부자 드랍률
			for ( int i = 0; i < 3; i++ )
			{
				double dice = Utility.RandomDouble();
				if( dice < 0.36 )
				{
					Item item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();

					if (item != null)
					{
						if( dice < 0.01 )
							Util.NewItemCreate( item, Utility.RandomMinMax(5, 6) );
						else if( dice < 0.11 )
							Util.NewItemCreate( item, Utility.RandomMinMax(2, 4) );
						else
							Util.NewItemCreate( item, Utility.RandomMinMax(0, 3) );
					}
					DropItem( item );
				}
			}
		} 

		public TreasureLevel5( Serial serial ) : base( serial ) 
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