using System;
using Server;
using Server.Mobiles;
using Server.Commands.Generic;
using System.Collections.Generic;
using Server.Items;

namespace Server.Commands
{
	public class OldMagicItemDeleteInfoCommand
	{
		public static void Initialize()
		{
	      	CommandSystem.Register( "OMID", AccessLevel.GameMaster, new CommandEventHandler( OldMagicItemDeleteInfo_OnCommand ) );
		}

		[Usage( "OldMagicItemDelete" )]
		[Description( "예전 아이템 삭제 코드." )]
		public static void OldMagicItemDeleteInfo_OnCommand( CommandEventArgs e )
		{
			e.Mobile.SendMessage("예전 아이템 삭제를 시작합니다!");
			int count = 0;
			var list = new List<Item>();
			foreach ( Item i in World.Items.Values )
			{
				if (( i is BaseWeapon || i is BaseArmor || i is BaseClothing || i is BaseJewel || i is Spellbook ))
				{
					list.Add( i );
				}
			}
			for ( int i = 0; i < list.Count; ++i )
			{
				if( list is BaseWeapon )
				{
					BaseWeapon tar = list[i] as BaseWeapon;
					if( tar.ReforgedPrefix == ReforgedPrefix.None && tar.ReforgedSuffix == ReforgedSuffix.None )
						continue;
					else
					{
						tar.Delete();
						count++;
					}
				}
				if( list is BaseArmor )
				{
					BaseArmor tar = list[i] as BaseArmor;
					if( tar.ReforgedPrefix == ReforgedPrefix.None && tar.ReforgedSuffix == ReforgedSuffix.None )
						continue;
					else
					{
						tar.Delete();
						count++;
					}
				}
				if( list is BaseClothing )
				{
					BaseClothing tar = list[i] as BaseClothing;
					if( tar.ReforgedPrefix == ReforgedPrefix.None && tar.ReforgedSuffix == ReforgedSuffix.None )
						continue;
					else
					{
						tar.Delete();
						count++;
					}
				}
				if( list is BaseJewel )
				{
					BaseJewel tar = list[i] as BaseJewel;
					if( tar.ReforgedPrefix == ReforgedPrefix.None && tar.ReforgedSuffix == ReforgedSuffix.None )
						continue;
					else
					{
						tar.Delete();
						count++;
					}
				}
				if( list is Spellbook )
				{
					Spellbook tar = list[i] as Spellbook;
					if( tar.ReforgedPrefix == ReforgedPrefix.None && tar.ReforgedSuffix == ReforgedSuffix.None )
						continue;
					else
					{
						tar.Delete();
						count++;
					}
				}
			}
			e.Mobile.SendMessage("총 {0}개의 아이템을 삭제했습니다.", count);
		}
	}
}