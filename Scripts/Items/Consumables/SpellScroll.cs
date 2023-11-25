using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Spells;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class SpellScroll : Item, ICommodity
    {
        private int m_SpellID;
        public SpellScroll(Serial serial)
            : base(serial)
        {
        }

        [Constructable]
        public SpellScroll(int spellID, int itemID)
            : this(spellID, itemID, 1)
        {
        }

        [Constructable]
        public SpellScroll(int spellID, int itemID, int amount)
            : base(itemID)
        {
            this.Stackable = true;
            this.Weight = 0.1;
            this.Amount = amount;

            this.m_SpellID = spellID;
        }

        public int SpellID
        {
            get
            {
                return this.m_SpellID;
            }
        }
        TextDefinition ICommodity.Description
        {
            get
            {
                return this.LabelNumber;
            }
        }
        bool ICommodity.IsDeedable
        {
            get
            {
                return (Core.ML);
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)this.m_SpellID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 0:
                    {
                        this.m_SpellID = reader.ReadInt();

                        break;
                    }
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive && this.Movable)
                list.Add(new ContextMenus.AddToSpellbookEntry());
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Multis.DesignContext.Check(from))
                return; // They are customizing

            if (!this.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }
            #region SA
            else if (from.Flying && from is PlayerMobile && BaseMount.OnFlightPath(from))
            {
                from.SendLocalizedMessage(1113749); // You may not use that while flying over such precarious terrain.
                return;
            }
            #endregion
			
			if( m_SpellID >= 47 && m_SpellID <= 63 )
			{
				from.Target = new ScrollTarget(this, m_SpellID);
			}
                //from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
			else
			{
				Spell spell = SpellRegistry.NewSpell(this.m_SpellID, from, this);

				if (spell != null)
					spell.Cast();
				else
					from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
			}
        }
		public class ScrollTarget : Target
		{
			SpellScroll m_SpellScroll;
			private int m_SpellID;
			public ScrollTarget(SpellScroll diamond, int spellid) : base(1, false, TargetFlags.None )
			{
				m_SpellScroll = diamond;
				m_SpellID = spellid;
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
							int upgrade_code = 0;
							int upgrade_line = 0;
							if( equip is BaseWeapon && m_SpellID >= 57 && m_SpellID <= 63 )
							{
								if( m_SpellID == 59 )
									upgrade_code = 4;
								else if( m_SpellID == 57 )
									upgrade_code = 5;
								else if( m_SpellID == 60 )
									upgrade_code = 3;
								else if( m_SpellID == 62 )
									upgrade_code = 1;
								else if( m_SpellID == 63 )
									upgrade_code = 2;
								else if( m_SpellID == 58 )
									upgrade_code = 6;
								
							}
							else if( equip is BaseArmor && ( m_SpellID == 49 || m_SpellID == 52 || m_SpellID == 53 ) )
							{
								if( m_SpellID == 49 )
									upgrade_code = 1;
								else if( m_SpellID == 53 )
									upgrade_code = 2;
								upgrade_line = 1;
							}
							else if(equip.PrefixOption[3 + upgrade_code] >= 20)
							{
								from.SendMessage("최대 강화입니다!!!" );
								return;
							}
							else if( equip.SuffixOption[1] >= 6 || equip.PrefixOption[3 + upgrade_code] == -1)
							{
								from.SendMessage("버그 아이템입니다!!!" );
								return;
							}
							else
							{
								from.SendMessage("아직 이 아이템은 강화할 수 없습니다!");
								return;

							}
							int amount = Misc.Util.NewItemPowerUpgrade[ equip.PrefixOption[3 + upgrade_code], upgrade_line, ( equip.SuffixOption[1] + 1 ) * 2 -1 ];
							
							if( amount > m_SpellScroll.Amount )
							{
								from.SendMessage("강화를 하기 위해서는 {0}장의 스크롤이 필요합니다!", amount );
								return;
							}
							else
							{
								bool success = Misc.Util.NewItemPowerChance(equip.PrefixOption[3 + upgrade_line]);
								if( success )
								{
									Misc.Util.NewItemPowerMake( check, upgrade_code );
								}
								else
								{
									if( 1 + equip.PrefixOption[3 + upgrade_code] > 9 )
										check.Delete();
									else
									{
										int lostHP = ( 1 + equip.PrefixOption[3 + upgrade_code] ) * ( 1 + equip.PrefixOption[3 + upgrade_code] );
										if( ( 1 + equip.PrefixOption[3 + upgrade_code] ) * ( 1 + equip.PrefixOption[3 + upgrade_code] ) > amount )
											lostHP = Utility.RandomMinMax(amount, ( 1 + equip.PrefixOption[3 + upgrade_code] ) * ( 1 + equip.PrefixOption[3 + upgrade_code] ));
										else
											lostHP = Utility.RandomMinMax(( 1 + equip.PrefixOption[3 + upgrade_code] ) * ( 1 + equip.PrefixOption[3 + upgrade_code] ), amount);

										if( equip.MaxHitPoints <= lostHP )
											check.Delete();
										else
											equip.MaxHitPoints -= lostHP;
										if( equip.MaxHitPoints < equip.HitPoints )
											equip.HitPoints = equip.MaxHitPoints;

									}
									
								}
								if( amount == m_SpellScroll.Amount )
									m_SpellScroll.Delete();
								else
									m_SpellScroll.Amount -= amount;
							}
						}
					}
				}
			}
		}
    }
}