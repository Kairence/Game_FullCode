using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Gumps
{
	public class PointQuestGump : Gump
	{
		string[] QuestName = { "수송", "호위", "자원", "제작", "사냥" };
		public PointQuestGump( PlayerMobile pm, int list ) : base( 0, 0 )
		{
			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			this.AddPage(0);
			this.AddBackground(0, 0, 300, 170, 5170);
			//this.AddLabel(40, 40, 0, @"Contract For: " + m_InscriptionBodParent.AmountToItem + " " + ProductBodType.Inscription[m_InscriptionBodParent.Item].Name);
			//this.AddLabel(40, 60, 0, @"Quantity Itemed: " + m_InscriptionBodParent.AmountItemed);
			//this.AddLabel(40, 80, 0, @"Reward: " + m_InscriptionBodParent.Reward);
			//if (m_InscriptionBodParent.AmountItemed != m_InscriptionBodParent.AmountToItem)
			//{
			//	this.AddButton(90, 110, 2061, 2062, 1, GumpButtonType.Reply, 0);
			//	this.AddLabel(104, 108, 0, @"Item");
			//}
			//else
			//{
			//	this.AddButton(90, 110, 2061, 2062, 2, GumpButtonType.Reply, 0);
			//	this.AddLabel(104, 108, 0, @"Reward");
			//}
		}		
		/*
		
		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile m_from = state.Mobile;
			PlayerMobile pm = m_from as PlayerMobile;
			int roll = Utility.Random( 250 );
			int Event = 100;
			Event ev = new Event();
			if(ev.ServerEvent == 2 )
				Event = 110;
       			if (m_from != null && m_InscriptionBodParent != null)
			{
				if ( info.ButtonID == 1 )
				{
					m_from.SendMessage("추가할 아이템을 클릭하세요.");
       	        			m_from.Target = new InscriptionItemedTarget(m_InscriptionBodParent);
				}
				if ( info.ButtonID == 2 )
				{
					if ( (int)m_from.Skills.ItemID.Value >= roll && m_InscriptionBodParent.Reward > 1000 )
					{
						if ( pm.InscriptionPoint + (int)( m_InscriptionBodParent.Reward * ( 1 + m_from.Skills.ItemID.Value / 200 * Event ) / 100 ) <= 4000000000 )
						{
							m_from.SendMessage( 0x40, "훌륭하군요! 인스크립션 코인을 추가로 {0} 더 드리겠어요!", (int)( m_InscriptionBodParent.Reward * ( 1 + m_from.Skills.ItemID.Value / 200 * Event ) / 100 ) );
	                    				pm.InscriptionPoint += (int)( m_InscriptionBodParent.Reward * ( 1 + m_from.Skills.ItemID.Value / 20 * Event ) / 100 );
						}
						else
						{
							m_from.SendMessage("인스크립션 코인 저장량이 너무 많아 은행으로 송금됩니다.");
							m_from.BankBox.DropItem(new Inscriptioncoins((int)( m_InscriptionBodParent.Reward * ( 1 + m_from.Skills.ItemID.Value / 200 * Event ) / 100 )));
						}
					}
					else
					{
						if ( pm.InscriptionPoint + m_InscriptionBodParent.Reward <= 4000000000 )
						{
							m_from.SendMessage("인스크립션 코인이 {0} 적립됩니다.", (int)( ( m_InscriptionBodParent.Reward * Event ) / 100 ) );
        	            				pm.InscriptionPoint += ( m_InscriptionBodParent.Reward * Event ) / 100;
						}
						else
						{
							m_from.SendMessage("인스크립션 코인 저장량이 너무 많아 은행으로 송금됩니다.");
							m_from.BankBox.DropItem(new Inscriptioncoins((int)( ( m_InscriptionBodParent.Reward * Event ) / 100 )));
						}
					}
				m_from.AddToBackpack(new Gold(m_InscriptionBodParent.Reward));
       	        		m_InscriptionBodParent.Delete();
				}
			}
		}
		*/
	}
	
	/*
	public class InscriptionItemedTarget : Target
	{
		private InscriptionBod m_InscriptionBodParent;
		
		public InscriptionItemedTarget( InscriptionBod InscriptionBodParent ) : base( -1, true, TargetFlags.None )
		{
			m_InscriptionBodParent = InscriptionBodParent;
		}
		
		protected override void OnTarget( Mobile from, object target )
		{
            		if (m_InscriptionBodParent == null || from == null || target == null || m_InscriptionBodParent.Item == null)
           		{
                		Console.WriteLine( "InscriptionBod: Sa bug !! Mais o? on sait pas :p" );
                		return;
            		}

			if ( target == from && m_InscriptionBodParent.AmountItemed == 0 )
			{
				from.SendMessage("퀘스트를 포기했습니다.");
				m_InscriptionBodParent.AmountItemed = 0;
				m_InscriptionBodParent.AmountToItem = 0;
				m_InscriptionBodParent.Reward = 1000;
			}

            		if (target is Item)
			{
                		Item Itemed = (Item)target;

                      		if (Itemed.GetType() == ProductBodType.Inscription[m_InscriptionBodParent.Item].Type)
				{
               				 m_InscriptionBodParent.AmountItemed += 1;
                			Itemed.Delete();
					if ( m_InscriptionBodParent.AmountItemed < m_InscriptionBodParent.AmountToItem )
       	        			from.Target = new InscriptionItemedTarget(m_InscriptionBodParent);
				}
				else
				{
				from.SendMessage("This item isn't acceptable.");
				}
			}
			else
			{
			from.SendMessage("This is not a item.");
			}
		}
		
		/*private bool IsValidFor(Mobile from, Mobile corpse)
		{
			if(corpse != null)
			{
				//Mobile killer = corpse.Killer;
				
				//if(killer!=null)
				//{
				//	if(killer is BaseCreature)
				//	{
				//		BaseCreature bc = (BaseCreature)killer;
				//		if((bc.Summoned && bc.SummonMaster==from)||(bc.Controlled && bc.ControlMaster==from))
				//			return true;
				//	}
				//}
			}
			return false;
		}
	}*/
}
