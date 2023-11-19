using System;
using System.Collections.Generic;
using Server.Regions;
using Server.Mobiles;
using Server.Items;
using Server.Network;
namespace Server.Misc
{
    public class WeightOverloading
    {
        public const int OverloadAllowance = 4;// We can be four stones overweight without getting fatigued

        public static void Initialize()
        {
            EventSink.Movement += new MovementEventHandler(EventSink_Movement);
            Mobile.FatigueHandler = FatigueOnDamage;
        }

        public static void FatigueOnDamage(Mobile m, int damage, DFAlgorithm df)
        {
            double fatigue = 0.0;

            switch (m.DFA)
            {
                case DFAlgorithm.Standard:
                    {
                        fatigue = (damage * (m.HitsMax / m.Hits) * ((double)m.Stam / m.StamMax)) - 5;
                    }
                    break;
                case DFAlgorithm.PainSpike:
                    {
                        fatigue = (damage * ((m.HitsMax / m.Hits) + ((50.0 + m.Stam) / m.StamMax) - 1.0)) - 5;
                    }
                    break;
            }

            var reduction = BaseArmor.GetInherentStaminaLossReduction(m) + 1;

            if (reduction > 1)
            {
                fatigue = fatigue / reduction;
            }

            if (fatigue > 0)
            {
                // On EA, if follows this special rule to reduce the chances of your stamina being dropped to 0
                if (m.Stam - fatigue <= 10)
                {
                    m.Stam -= (int)(fatigue * ((double)m.Hits / (double)m.HitsMax));
                }
                else
                {
                    m.Stam -= (int)fatigue;
                }
            }
        }

		private static bool DungeonCheck( Mobile from )
		{
			if( ( from.Region is DungeonRegion || from.Region.Name == "Ancient Lair" ))
			{
				return true;
			}
			return false;
		}
		
        public static void EventSink_Movement(MovementEventArgs e)
        {
            Mobile from = e.Mobile;
            if (!from.Alive || from.IsStaff())
			{
				from.SendSpeedControl(SpeedControlType.Disable);				
                return;
			}
            if (!from.Player)
            {
                return;
            }

			Server.Engines.Craft.AutoCraftTimer.EndTimer(from);
			
            if (from is PlayerMobile)
            {
				PlayerMobile pm = from as PlayerMobile;
				if( pm.TimerList[71] > 0 )
				{
					pm.SendMessage("작업중에는 이동할 수 없습니다.");
					e.Blocked = true;
				}
				else
					pm.Loop = false;
				//if( pm.Hunger <= 0 && pm.Mounted )
				//	BaseMount.Dismount(pm);

				/*
				if( pm.DeathCheck == 0 && !( pm.Region is TownRegion ) )
				{
					e.Blocked = true;
					pm.SendMessage("사망 보호 횟수가 0이 되어 외부로 나갈 수 없습니다!");
					pm.PlayerMove(false);
				}
				*/
				if( pm.X >= 5120 && pm.Y >= 2304 && pm.X < 6144 && pm.Y < 4096)
				{
					e.Blocked = true;
					pm.SendMessage("당신은 아직 여기를 탐험할 수 없습니다!");
					pm.PlayerMove(false);
					//e.Blocked = false;
				}
                int amt = 16;
				BaseClothing shoes = from.FindItemOnLayer(Layer.Shoes) as BaseClothing;
				
				if( shoes != null )
				{
					amt *= 2;
					if (0 >= Utility.Random(100)) // 25% chance to lower durability
					{
						if (shoes.MaxHitPoints > 0)
						{
							if (shoes.HitPoints >= 1)
								shoes.HitPoints--;
							else if ( shoes.MaxHitPoints > 0)
							{
								shoes.MaxHitPoints--;
								
								if (shoes.Parent is Mobile)
									((Mobile)shoes.Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
								if (shoes.MaxHitPoints <= 0 )
									shoes.Delete();
							}
						}
					}					
				}
				if (from.Mounted )
					amt = 32;

				if( (e.Direction & Direction.Running) != 0 )
					amt /= 2;

				/*
				if( DungeonCheck(from) )
				{
					if( !from.Mounted && ( e.Direction & Direction.Running) != 0 )
						amt = 2;
					else
						amt = 1;
				}
				*/
                if ((++pm.StepsTaken % amt) == 0)
				{
                    --from.Hunger;
					Server.Misc.Util.TiredCheck( pm, pm.Hunger, 1 );
				}
				
				if( pm.Region.Name == "Britain" )
					pm.SaveTown = 1;
				else if( pm.Region.Name == "Buccaneer's Den" )
					pm.SaveTown = 2;
				else if( pm.Region.Name == "Cove" )
					pm.SaveTown = 3;
				else if( pm.Region.Name == "Jhelom" )
					pm.SaveTown = 4;
				else if( pm.Region.Name == "Magincia" )
					pm.SaveTown = 5;
				else if( pm.Region.Name == "Minoc" )
					pm.SaveTown = 6;
				else if( pm.Region.Name == "Moonglow" )
					pm.SaveTown = 7;
				else if( pm.Region.Name == "Nujel'm" )
					pm.SaveTown = 8;
				else if( pm.Region.Name == "Serpent's Hold" )
					pm.SaveTown = 9;
				else if( pm.Region.Name == "Skara Brae" )
					pm.SaveTown = 10;
				else if( pm.Region.Name == "Trinsic" )
					pm.SaveTown = 11;
				else if( pm.Region.Name == "Vesper" )
					pm.SaveTown = 12;
				else if( pm.Region.Name == "Yew" )
					pm.SaveTown = 13;
				else if( pm.Region.Name == "New Haven" )
					pm.SaveTown = 14;
				
			}			
			
			BandageContext c = BandageContext.GetContext( from );
			if ( c != null )
				c.Slip();
			
            int maxWeight = from.MaxWeight + OverloadAllowance;
            int overWeight = (Mobile.BodyWeight + from.TotalWeight) - maxWeight;

            if (overWeight > 0)
            {
                from.Hunger -= GetStamLoss(from, overWeight, (e.Direction & Direction.Running) != 0);
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					Server.Misc.Util.TiredCheck( pm, pm.Hunger, GetStamLoss(from, overWeight, (e.Direction & Direction.Running) != 0), 0 );
				}
			}
			/*
			if (from.Hunger <= 0 ) //|| (( from.Mounted || from.Flying )) )&& DungeonCheck(from)) )
			{
				//from.SendLocalizedMessage(500109); // You are too fatigued to move, because you are carrying too much weight!
				//e.Blocked = true;
				from.SendSpeedControl(SpeedControlType.WalkSpeed);
				return;
			}
			else
				from.SendSpeedControl(SpeedControlType.Disable);					
			*/
			/*
            if (!Core.SA && ((from.Stam * 100) / Math.Max(from.StamMax, 1)) < 10)
            {
                --from.Stam;
            }
            if (from.Stam == 0)
            {
                from.SendLocalizedMessage(from.Mounted ? 500108 : 500110); // Your mount is too fatigued to move. : You are too fatigued to move.
                e.Blocked = true;
                return;
            }
			*/

        }

        public static int GetStamLoss(Mobile from, int overWeight, bool running)
        {
            int loss = 5 + (overWeight / 25);

            if (running)
                loss *= 2;

			if( DungeonCheck(from))
				loss *= 2;
			
            return loss;
        }

        public static bool IsOverloaded(Mobile m)
        {
            if (!m.Player || !m.Alive || m.IsStaff())
                return false;

            return ((Mobile.BodyWeight + m.TotalWeight) > (m.MaxWeight + OverloadAllowance));
        }
    }
}
