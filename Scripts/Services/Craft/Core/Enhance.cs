using System;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;
using Server.Accounting;

namespace Server.Engines.Craft
{
    public enum EnhanceResult
    {
        None,
        NotInBackpack,
        BadItem,
        BadResource,
        AlreadyEnhanced,
        Success,
        Failure,
        Broken,
        NoResources,
        NoSkill,
        Enchanted,
		NotItem,
		NotChance
    }

    public class Enhance
    {
        private static Dictionary<Type, CraftSystem> _SpecialTable;

        public static void Initialize()
        {
            _SpecialTable = new Dictionary<Type, CraftSystem>();

            _SpecialTable[typeof(ClockworkLeggings)] = DefBlacksmithy.CraftSystem;
            _SpecialTable[typeof(GargishClockworkLeggings)] = DefBlacksmithy.CraftSystem;
        }

        private static bool IsSpecial(Item item, CraftSystem system)
        {
            foreach (KeyValuePair<Type, CraftSystem> kvp in _SpecialTable)
            {
                if (kvp.Key == item.GetType() && kvp.Value == system)
                    return true;
            }

            return false;
        }

		
		//강화 옵션 선택
        public static readonly int[,,] EnhancedOption = new int[,,]
		{
			// 1. 금속 - 철 옵션 (무기, 방어구, 악세사리)
			{{ 	1081002,	7,	37500	},	//무기: 무기 피해 3.75% (3.75 x 10000)
			{	1081023,	12,	10000	},	//방어구: 물리 저항 1% (1 x 10000)
			{	1081044,	4,	400000	}},	//악세사리: 체력 증가 40 (40 x 10000)
			// 2. 금속 - 구리 옵션
			{{	1081003,	40,	75000	},	//무기: 공격 속도 7.5% (7.5 x 10000)
			{	1081024,	41,	25000	},	//방어구: 시전 속도 증가 2.5% (2.5 x 10000)
			{	1081045,	5,	400000	}},	//악세사리: 기력 증가 40 (40 x 10000)
			// 3. 금속 - 청동 옵션
			{{	1081004,	26,	37500	},	//무기: 에너지 피해 증가 3.75% (3.75 x 10000)
			{	1081025,	16,	10000	},	//방어구: 에너지 저항 1% (1 x 10000)
			{	1081046,	6,	400000	}},	//악세사리: 마나 증가 40 (40 x 10000)
			// 4. 금속 - 금 옵션
			{{	1081005,	3,	300000	},	//무기: 운 증가 30 (30 x 10000)
			{	1081026,	3,	100000	},	//방어구: 운 증가 10 (10 x 10000)
			{	1081047,	3,	200000	}},	//악세사리: 운 증가 20 (20 x 10000)
			// 5. 금속 - 아가파이트 옵션
			{{	1081006,	23,	37500	},	//무기: 화염 피해 증가 3.75% (3.75 x 10000)
			{	1081027,	13,	10000	},	//방어구: 화염 저항 1% (1 x 10000)
			{	1081048,	0,	200000	}},	//악세사리: 힘 증가 20 (20 x 10000)
			// 6. 금속 - 베라이트 옵션
			{{	1081007,	25,	37500	},	//무기: 독 피해 증가 3.75% (3.75 x 10000)
			{	1081028,	15,	10000	},	//방어구: 독 저항 1% (1 x 10000)
			{	1081049,	1,	200000	}},	//악세사리: 민첩 증가 20 (20 x 10000)
			// 7. 금속 - 벨러라이트 옵션
			{{	1081008,	24,	37500	},	//무기: 냉기 피해 증가 3.75% (3.75 x 10000)
			{	1081029,	14,	10000	},	//방어구: 냉기 저항 1% (1 x 10000)
			{	1081050,	2,	200000	}},	//악세사리: 지능 증가 20 (20 x 10000)
			// 8. 나무 - 나무 옵션
			{{	1081009,	7,	37500	},	//무기: 무기 피해 3.75% (3.75 x 10000)
			{	1081030,	4,	200000	},	//방어구: 체력 증가 20 (20 x 10000)
			{	1081051,	-1,	-1	}},	//악세사리: 옵션 없음
			// 9. 나무 - 떡갈 나무 옵션
			{{	1081010,	5,	600000	},	//무기: 기력 증가 60 (60 x 10000)
			{	1081031,	8,	12500	},	//방어구: 주문 피해 1.25% (1.25 x 10000)
			{	1081052,	-1,	-1	}},	//악세사리: 옵션 없음
			// 10. 나무 - 물푸레 나무 옵션
			{{	1081011,	40,	75000	},	//무기: 공격 속도 7.5% (7.5 x 10000)
			{	1081032,	7,	12500	},	//방어구: 무기 피해 1.25% (1.25 x 10000)
			{	1081053,	-1,	-1	}},	//악세사리: 옵션 없음
			// 11. 나무 - 주목 나무 옵션
			{{	1081012,	3,	300000	},	//무기: 운 증가 30 (30 x 10000)
			{	1081033,	3,	100000	},	//방어구: 운 증가 10 (10 x 10000)
			{	1081054,	-1,	-1	}},	//악세사리: 옵션 없음
			// 12. 나무 - 심재 나무 옵션
			{{	1081013,	44,	30000	},	//무기: 물리 치명타 피해 증가 3% (3 x 10000)
			{	1081034,	20,	50000	},	//방어구: 기력 회복 0.5 (0.5 x 100000)
			{	1081055,	-1,	-1	}},	//악세사리: 옵션 없음
			// 13. 나무 - 피 나무 옵션
			{{	1081014,	37,	15000	},	//무기: 체력 흡수 1.5% (1.5 x 10000)
			{	1081035,	19,	50000	},	//방어구: 체력 회복 0.5 (0.5 x 100000)
			{	1081056,	-1,	-1	}},	//악세사리: 옵션 없음
			// 14. 나무 - 서리 나무 옵션
			{{	1081015,	42,	15000	},	//무기: 물리 치명타 확률 증가 1.5% (1.5 x 10000)
			{	1081036,	21,	50000	},	//방어구: 마나 회복 0.5 (0.5 x 100000)
			{	1081057,	-1,	-1	}},	//악세사리: 옵션 없음
			// 15. 가죽 - 가죽 옵션
			{{	1081016,	8,	37500	},	//무기: 주문 피해 3.75% (3.75 x 10000)
			{	1081037,	40,	25000	},	//방어구: 공격 속도 2.5% (2.5 x 10000)
			{	1081058,	-1,	-1	}},	//악세사리: 옵션 없음
			// 16. 가죽 - 질긴 가죽 옵션
			{{	1081017,	6,	600000	},	//무기: 마나 증가 60 (60 x 10000)
			{	1081038,	6,	200000	},	//방어구: 마나 증가 20 (20 x 10000)
			{	1081059,	-1,	-1	}},	//악세사리: 옵션 없음
			// 17. 가죽 - 거친 가죽 옵션
			{{	1081018,	45,	30000	},	//무기: 마법 치명타 피해 증가 3% (3 x 10000)
			{	1081039,	45,	10000	},	//방어구: 마법 치명타 피해 증가 1% (1 x 10000)
			{	1081060,	-1,	-1	}},	//악세사리: 옵션 없음
			// 18. 가죽 - 경화 가죽 옵션
			{{	1081019,	3,	300000	},	//무기: 운 증가 30 (30 x 10000)
			{	1081040,	3,	100000	},	//방어구: 운 증가 10 (10 x 10000)
			{	1081061,	-1,	-1	}},	//악세사리: 옵션 없음
			// 19. 가죽 - 가시 가죽 옵션
			{{	1081020,	24,	37500	},	//무기: 냉기 피해 증가 3.75% (3.75 x 10000)
			{	1081041,	8,	12500	},	//방어구: 주문 피해 1.25% (1.25 x 10000)
			{	1081062,	-1,	-1	}},	//악세사리: 옵션 없음
			// 20. 가죽 - 뿔 가죽 옵션
			{{	1081021,	23,	37500	},	//무기: 화염 피해 증가 3.75% (3.75 x 10000)
			{	1081042,	43,	5000	},	//방어구: 마법 치명타 확률 증가 0.5% (0.5 x 10000)
			{	1081063,	-1,	-1	}},	//악세사리: 옵션 없음
			// 21. 가죽 - 미늘 가죽 옵션
			{{	1081022,	26,	37500	},	//무기: 에너지 피해 증가 3.75% (3.75 x 10000)
			{	1081043,	41,	25000	},	//방어구: 시전 속도 증가 2.5% (2.5 x 10000)
			{	1081064,	-1,	-1	}}	//악세사리: 옵션 없음
		};		
		

        private static bool CanEnhance(Item item)
        {
            return item is BaseArmor || item is BaseWeapon || item is BaseClothing || item is BaseJewel || item is Spellbook;
        }

		public static int EnchanceChance(Mobile from, int rank, int enchance)
		{
			//int needItem = 
			//if( rank
			return 0;
		}

        public static EnhanceResult Invoke(Mobile from, CraftSystem craftSystem, ITool tool, Item item, CraftResource resource, Type resType, ref object resMessage)
        {
            if (item == null)
                return EnhanceResult.BadItem;
			
            if (!item.IsChildOf(from.Backpack))
                return EnhanceResult.NotInBackpack;

            if (item is IArcaneEquip)
            {
                IArcaneEquip eq = (IArcaneEquip)item;
                if (eq.IsArcane)
                    return EnhanceResult.BadItem;
            }

			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				Account acc = pm.Account as Account;
				/*
				if( item is IEquipOption )
				{
					//강화 확률
					IEquipOption equip = item as IEquipOption;
					
					int enhancePaper = equip.SuffixOption[10] + 1; //강화에 필요한 강화서 수량
					if( enchancePaper > pm.
					
					if(  ) //아이템 랭크, 인챈트 등급
					{
						if( Utility.RandomDouble() > 0.2 + equip.PrefixOption[1] * 0.05 )
						{
							return EnhanceResult.NotChance;
						}
					}
					IEquipOption equip = item as IEquipOption;
					if( equip.PrefixOption[0] != 100 )
					{
						return EnhanceResult.BadResource;
					}
					else if( acc.Point[860 + equip.SuffixOption[1]] < 10 )
					{
						return EnhanceResult.BadResource;
					}
					else
					{
						acc.Point[860 + equip.SuffixOption[1]] -= 10;
						Misc.Util.NewOptionCreate(item, from, true );
						return EnhanceResult.BadResource;
					}
				}
				*/
			}
			return EnhanceResult.BadResource;
        }

        public static void CheckResult(ref EnhanceResult res, int chance)
        {
			
            //if (res != EnhanceResult.Success)
            //    return; // we've already failed..

            int random = Utility.Random(100);

            if (10 > random)
                res = EnhanceResult.Failure;
            else if (chance > random)
                res = EnhanceResult.Broken;
        }

        public static void BeginTarget(Mobile from, CraftSystem craftSystem, ITool tool)
        {
            CraftContext context = craftSystem.GetContext(from);
            PlayerMobile user = from as PlayerMobile;

            if (context == null)
                return;

            CraftSubResCol subRes = craftSystem.CraftSubRes;
			CraftSubRes res = subRes.GetAt(0);
			CraftResource resource = CraftResources.GetFromType(res.ItemType);
			from.Target = new InternalTarget(craftSystem, tool, res.ItemType, resource);

			/*
            if (lastRes >= 0 && lastRes < subRes.Count)
            {
                CraftSubRes res = subRes.GetAt(lastRes);
				from.Target = new InternalTarget(craftSystem, tool, res.ItemType, resource);
			}
                if (from.Skills[craftSystem.MainSkill].Value < res.RequiredSkill)
                {
                    from.SendGump(new CraftGump(from, craftSystem, tool, res.Message));
                }
                else
                {
                    CraftResource resource = CraftResources.GetFromType(res.ItemType);

                    if (resource != CraftResource.None)
                    {
                        from.Target = new InternalTarget(craftSystem, tool, res.ItemType, resource);

                        if (user.NextEnhanceSuccess)
                        {
                            from.SendLocalizedMessage(1149869, "100"); // Target an item to enhance with the properties of your selected material (Success Rate: ~1_VAL~%).
                        }
                        else
                        {
                            from.SendLocalizedMessage(1061004); // Target an item to enhance with the properties of your selected material.
                        }
                    }
                    else
                    {
                        from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
                    }
                }
            }
            else
            {
                from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
            }
			*/
        }

        private class InternalTarget : Target
        {
            private readonly CraftSystem m_CraftSystem;
            private readonly ITool m_Tool;
            private readonly Type m_ResourceType;
            private readonly CraftResource m_Resource;

            public InternalTarget(CraftSystem craftSystem, ITool tool, Type resourceType, CraftResource resource)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Tool = tool;
                m_ResourceType = resourceType;
                m_Resource = resource;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item)
                {
                    object message = null;
					EnhanceResult res = Enhance.Invoke(from, m_CraftSystem, m_Tool, (Item)targeted, m_Resource, m_ResourceType, ref message);

                    switch (res)
                    {
                        case EnhanceResult.NotInBackpack:
                            message = 1061005;
                            break; // The item must be in your backpack to enhance it.
                        case EnhanceResult.AlreadyEnhanced:
                            message = 1061012;
                            break; // This item is already enhanced with the properties of a special material.
                        case EnhanceResult.BadItem:
                            message = 1061011;
                            break; // You cannot enhance this type of item with the properties of the selected special material.
                        case EnhanceResult.BadResource: //재료 부족
                            message = 1061010;
                            break; // You must select a special material in order to enhance an item with its properties.
                        case EnhanceResult.Broken:
                            message = 1061080;
                            break; // You attempt to enhance the item, but fail catastrophically. The item is lost.
                        case EnhanceResult.Failure:
                            message = 1061082;
                            break; // You attempt to enhance the item, but fail. Some material is lost in the process.
                        case EnhanceResult.Success:
                            message = 1061008;
                            break; // You enhance the item with the properties of the special material.
                        case EnhanceResult.NoSkill:
                            message = 1044153;
                            break; // You don't have the required skills to attempt this item.
                        case EnhanceResult.Enchanted: 
                            message = 1080131; 
                            break; // You cannot enhance an item that is currently enchanted.
                    }
					
                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, message));
                }
            }
        }
    }
}
