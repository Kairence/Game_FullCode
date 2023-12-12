using System;
using Server.Targeting;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public class Emerald : Item, IGem
    {
        [Constructable]
        public Emerald()
            : this(1)
        {
        }

        [Constructable]
        public Emerald(int amount)
            : base(0xF10)
        {
            this.Stackable = true;
            this.Amount = amount;
        }
		/*
		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			list.Add( 1060658, "무기\t{0}, 방패: 민첩 증가, 천옷: 독 저항력", "민첩 증가" );
			list.Add( 1060659, "가죽\t{0}, 가죽: 독 저항력, 뼈: 독 저항력", "독 저항력" );
			list.Add( 1060660, "링 갑옷\t{0}, 체인 갑옷: 민첩 증가, 플렛 갑옷: 민첩 증가", "민첩 증가" );
			list.Add( 1060661, "반지&팔찌\t{0}, 귀걸이&목걸이: 민첩 증가, 마법책: 민첩 증가", "민첩 증가" );	// F +5, E +10, D +15, C +20, B +30, A +40, S +50
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
				from.SendMessage("에메랄드가 부족합니다.");
				return;
			}
			else
			{
				from.SendMessage( "에메랄드를 어느 아이템에 사용하시겠습니까?" );
				from.Target = new GemTarget(this);
			}
			
		}
		public class GemTarget : Target
		{
			Emerald m_Gem;
			int m_Amount;
			public GemTarget(Emerald diamond) : base(1, false, TargetFlags.None )
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
								Misc.Util.NewUseGem(check, 1);
								from.SendMessage("제련이 완료되었습니다!");
								if( m_Gem.Amount == 100 )
									m_Gem.Delete();
								else
									m_Gem.Amount -= 100;
							}
						}
					}
				}
			}
		}

        public Emerald(Serial serial)
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

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}