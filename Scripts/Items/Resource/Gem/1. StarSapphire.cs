using System;
using Server.Targeting;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public class StarSapphire : Item, IGem
    {
        [Constructable]
        public StarSapphire()
            : this(1)
        {
        }

        [Constructable]
        public StarSapphire(int amount)
            : base(0x0F0F)
        {
            this.Stackable = true;
            this.Amount = amount;
        }
		/*
		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			list.Add( 1060658, "무기\t{0}, 방패: 지력 증가, 천옷: 빠른 주문 시전", "지력 증가");
			list.Add( 1060659, "가죽\t{0}, 스텃: 공격 속도 증가, 뼈: 피해 증가","빠른 주문 시전" );
			list.Add( 1060660, "링 갑옷\t{0}, 체인 갑옷: 물리 데미지 반사, 플렛 갑옷: 물리 데미지 반사", "공격 확률 증가" );
			list.Add( 1060661, "반지&팔찌\t{0}, 귀걸이&목걸이: 지력 증가, 마법책: 지력 증가", "지력 증가" );	// F +5, E +10, D +15, C +20, B +30, A +40, S +50
		}
		
		*/
		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) ) // Make sure its in their pack
			{
				 from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if( Amount < 100 )
			{
				from.SendMessage("별무늬 사파이어가 부족합니다.");
				return;
			}
			else
			{
				from.SendMessage( "별무늬 사파이어를 어느 아이템에 사용하시겠습니까?" );
				from.Target = new GemTarget(this);
			}
			
		}
		public class GemTarget : Target
		{
			StarSapphire m_Gem;
			int m_Amount;
			public GemTarget(StarSapphire diamond) : base(1, false, TargetFlags.None )
			{
				m_Gem = diamond;
			}
			protected override void OnTarget( Mobile from, object targeted )
			{
				if (targeted is Item)
				{
					Item check = targeted as Item;

					if (!check.IsChildOf(from.Backpack))
					{
						from.SendMessage("장비를 백팩 안에 넣어서 사용하십시오.");
						return;
					}
					else
					{
						if( check is IEquipOption )
						{
							IEquipOption equip = check as IEquipOption;
							if( equip.PrefixOption[0] != 100 )
							{
								from.SendMessage("아티펙트 혹은 구 아이템은 제련이 불가능합니다!!");
							}
							else if( equip.SuffixOption[2] <= 0 )
							{
								from.SendMessage("이 아이템은 더 이상 제련이 불가능합니다!");
							}
							else
							{
								Misc.Util.NewUseGem(check, 0);
								from.SendMessage("제련이 완료되었습니다!");
								if( m_Gem.Amount == 100 )
									m_Gem.Delete();
								else
									m_Gem.Amount -= 100;
							}
						}
					}
					/*
					if( targeted is BaseWeapon )
					{
						BaseWeapon item = targeted as BaseWeapon;
						if( !item.Identified )
						{
							from.SendMessage("아이템 감정이 되지 않았습니다!");
							return;
						}
						else if( 100 > m_Gem.Amount )
						{
						}
						else if( (int)item.ItemPower < 1 )
						{
							from.SendMessage("일반 아이템은 사용할 수 없습니다.");
							return;
						}
						else
						{
							if( from is PlayerMobile )
							{
								PlayerMobile pm = from as PlayerMobile;
								Misc.Util.ItemCreate( item, 0, false, pm, 0 );
								if( ((int)item.ItemPower - 3 ) * 4 == m_Gem.Amount )
									m_Gem.Delete();
								else
									m_Gem.Amount -= ((int)item.ItemPower - 3 ) * 4;
								
							}
						}
					}
					else if( targeted is BaseArmor )
					{
						BaseArmor item = targeted as BaseArmor;
						if( !item.Identified )
						{
							from.SendMessage("아이템 감정이 되지 않았습니다!");
							return;
						}
						else if( ((int)item.ItemPower - 3 ) * 4 > m_Gem.Amount )
						{
							from.SendMessage("별무늬 사파이어가 부족합니다.");
							return;
						}
						else if( (int)item.ItemPower < 1 )
						{
							from.SendMessage("일반 아이템은 사용할 수 없습니다.");
							return;
						}
						else
						{
							if( from is PlayerMobile )
							{
								PlayerMobile pm = from as PlayerMobile;
								Misc.Util.ItemCreate( item, 0, false, pm, 0 );
								if( ((int)item.ItemPower - 3 ) * 4 == m_Gem.Amount )
									m_Gem.Delete();
								else
									m_Gem.Amount -= ((int)item.ItemPower - 3 ) * 4;
								
							}
						}
					}
					else if( targeted is BaseClothing )
					{
						BaseClothing item = targeted as BaseClothing;
						if( !item.Identified )
						{
							from.SendMessage("아이템 감정이 되지 않았습니다!");
							return;
						}
						else if( ((int)item.ItemPower - 3 ) * 4 > m_Gem.Amount )
						{
							from.SendMessage("별무늬 사파이어가 부족합니다.");
							return;
						}
						else if( (int)item.ItemPower < 1 )
						{
							from.SendMessage("일반 아이템은 사용할 수 없습니다.");
							return;
						}
						else
						{
							if( from is PlayerMobile )
							{
								PlayerMobile pm = from as PlayerMobile;
								Misc.Util.ItemCreate( item, 0, false, pm, 0 );
								if( ((int)item.ItemPower - 3 ) * 4 == m_Gem.Amount )
									m_Gem.Delete();
								else
									m_Gem.Amount -= ((int)item.ItemPower - 3 ) * 4;
								
							}
						}
					}
					else if( targeted is BaseJewel )
					{
						BaseJewel item = targeted as BaseJewel;
						if( !item.Identified )
						{
							from.SendMessage("아이템 감정이 되지 않았습니다!");
							return;
						}
						else if( ((int)item.ItemPower - 3 ) * 4 > m_Gem.Amount )
						{
							from.SendMessage("별무늬 사파이어가 부족합니다.");
							return;
						}
						else if( (int)item.ItemPower < 1 )
						{
							from.SendMessage("일반 아이템은 사용할 수 없습니다.");
							return;
						}
						else
						{
							if( from is PlayerMobile )
							{
								PlayerMobile pm = from as PlayerMobile;
								Misc.Util.ItemCreate( item, 0, false, pm, 0 );
								if( ((int)item.ItemPower - 3 ) * 4 == m_Gem.Amount )
									m_Gem.Delete();
								else
									m_Gem.Amount -= ((int)item.ItemPower - 3 ) * 4;
								
							}
						}
					}
					else if( targeted is Spellbook )
					{
						Spellbook item = targeted as Spellbook;
						if( !item.Identified )
						{
							from.SendMessage("아이템 감정이 되지 않았습니다!");
							return;
						}
						else if( ((int)item.ItemPower - 3 ) * 4 > m_Gem.Amount )
						{
							from.SendMessage("별무늬 사파이어가 부족합니다.");
							return;
						}
						else if( (int)item.ItemPower < 1 )
						{
							from.SendMessage("일반 아이템은 사용할 수 없습니다.");
							return;
						}
						else
						{
							if( from is PlayerMobile )
							{
								PlayerMobile pm = from as PlayerMobile;
								Misc.Util.ItemCreate( item, 0, false, pm, 0 );
								if( ((int)item.ItemPower - 3 ) * 4 == m_Gem.Amount )
									m_Gem.Delete();
								else
									m_Gem.Amount -= ((int)item.ItemPower - 3 ) * 4;
								
							}
						}
					}
					*/
				}
			}
		}

        public StarSapphire(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight
        {
            get
            {
                return 0.1;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
                ItemID = 0x0F0F;
        }
    }
}