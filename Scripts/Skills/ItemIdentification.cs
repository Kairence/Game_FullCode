using System;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;
using Server.Network;
using Server.SkillHandlers;
using Server.Multis;

namespace Server.Items
{
    public class ItemIdentification
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.ItemID].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile from)
        {
			/*
			if( from.Hunger < 100 )
				from.SendMessage("감정을 하기 위해서는 최소 만복도가 1% 이상이어야 합니다.");
			else
			{
				from.Hunger -= 100;
				from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
				from.Target = new InternalTarget();
			}
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				pm.Tired += 10;
			}
			*/
			from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
			from.Target = new InternalTarget();
            return TimeSpan.FromSeconds(1.0);
        }

		static double[] canid =
		{
			0, 50, 80, 110, 140, 170
		};
		
        [PlayerVendorTarget]
        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(8, false, TargetFlags.None)
            {
                this.AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile from, object o)
            {
				if( o is Item )
				{
					Item item = o as Item;
					BaseHouse house = BaseHouse.FindHouseAt(from);
					if( item is IEquipOption )
					{
						IEquipOption equip = item as IEquipOption;

						if( !equip.Identified || item is Container )
						{
							if( item.RootParent == from || ( house != null && house.IsOwner(from)) )
							{
								if( from.Skills.ItemID.Value < 150 && ( equip.PrefixOption[0] == 200 || equip.PrefixOption[0] == 300 ) )
								{
									from.SendMessage("유물은 아이템 감정 150이상부터 가능합니다!");
									return;
								}							
								if( equip.PrefixOption[0] < 100 )
								{
									from.SendMessage("구 아이템은 감정할 수 없습니다.");
									return;
								}
								if( from.Skills.ItemID.Value < canid[equip.SuffixOption[1]] )
								{
									from.SendMessage("이 아이템을 감정하려면 {0}의 스킬을 더 올리세요.", ( canid[equip.SuffixOption[1]] - from.Skills.ItemID.Value ).ToString());
									return;
								}
								int pointBonus = (int)( from.Skills.ItemID.Value - canid[equip.SuffixOption[1]] ) / 10;
								if( from.Skills.ItemID.Value >= 200 )
								{
									pointBonus = (int)( from.Skills.ItemID.Value - canid[equip.SuffixOption[1]] ) / 5;
								}
								if( pointBonus < 1 )
									pointBonus = 1;
								if( from.Skills.ItemID.Value >= 150 )
									pointBonus += 1;

								int dice = Utility.RandomMinMax(1, pointBonus);
								equip.MaxHitPoints += dice;
								equip.HitPoints += dice;
								
								from.SendMessage("아이템 감정으로 내구도를 {0} 올렸습니다!", dice);
								equip.Identified = true;
							}
						}
						else
							from.SendMessage("이미 감정이 되었습니다");
					}
				}				
                Server.Engines.XmlSpawner2.XmlAttach.RevealAttachments(from, o);
            }
        }
    }
}
