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
        Enchanted
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

        private static bool CanEnhance(Item item)
        {
            return item is BaseArmor || item is BaseWeapon || item is BaseClothing || item is BaseJewel || item is Spellbook;
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
				if( item is IEquipOption )
				{
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
			}
			return EnhanceResult.BadResource;
        }

        public static void CheckResult(ref EnhanceResult res, int chance)
        {
            if (res != EnhanceResult.Success)
                return; // we've already failed..

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
