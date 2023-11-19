using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Network;
using System.Linq;

namespace Server.Items
{
    public class SalvageBag : Bag
    {
        private bool m_Failure;
		
        public override int LabelNumber
        {
            get
            {
                return 1079931;
            }
        }// Salvage Bag

        [Constructable]
        public SalvageBag()
            : this(Utility.RandomBlueHue())
        {
        }

        [Constructable]
        public SalvageBag(int hue)
        {
            Weight = 2.0;
            Hue = hue;
            m_Failure = false;
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive)
            {
                //list.Add(new SalvageIngotsEntry(this, IsChildOf(from.Backpack) && Resmeltables()));
                //list.Add(new SalvageClothEntry(this, IsChildOf(from.Backpack) && Scissorables()));
                list.Add(new SalvageAllEntry(this, IsChildOf(from.Backpack)));
            }
        }
		
        private bool BlacksmithResmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                //if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                //    return false;

                CraftResourceInfo info = CraftResources.GetInfo(resource);

				if( item.ItemID == 5359 || item.ItemID == 5360 )
					return false;
				
                if (info == null ) //|| info.ResourceTypes.Length == 0)
                    return false;
				
                CraftItem craftItem = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                    return false;

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 1)
                    return false; // Not enough metal to resmelt
					
                double difficulty = 0.0;

                switch ( resource )
                {
                    case CraftResource.Copper:
                        difficulty = 20.0;
                        break;
                    case CraftResource.Bronze:
                        difficulty = 40.0;
                        break;
                    case CraftResource.Gold:
                        difficulty = 60.0;
                        break;
                    case CraftResource.Agapite:
                        difficulty = 80.0;
                        break;
                    case CraftResource.Verite:
                        difficulty = 100.0;
                        break;
                    case CraftResource.Valorite:
                        difficulty = 120.0;
                        break;
                }
			
                Type resourceType = info.ResourceTypes[0];
                Item ingot = (Item)Activator.CreateInstance(resourceType);

                double skill = from.Skills[SkillName.Blacksmith].Value;
			
                if (difficulty > skill)
                {
                    m_Failure = true; 
                }
				else
				{
					if (item is DragonBardingDeed || (item is BaseArmor && ((BaseArmor)item).PlayerConstructed) || (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed))					
					{
						double amount = craftResource.Amount;

						if (amount < 1)
							ingot.Amount = 1;
						else
							ingot.Amount = (int)amount;
						item.Delete();
					}
					else
						m_Failure = true; 
				}

				if( !m_Failure )
				{
					from.AddToBackpack(ingot);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return true;
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
        private void SalvageBlacksmith(Mobile from)
        {
            int salvaged = 0;
            int notSalvaged = 0;
			
            Container sBag = this;
			
            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
                if (item is BaseArmor && ((BaseArmor)item).PlayerConstructed)
                {
                    if (BlacksmithResmelt(from, item, ((BaseArmor)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
                else if (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed)
                {
                    if (BlacksmithResmelt(from, item, ((BaseWeapon)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
                else if (item is DragonBardingDeed)
                {
                    if (BlacksmithResmelt(from, item, ((DragonBardingDeed)item).Resource))
                        salvaged++;

                    else
                        notSalvaged++;
                }
            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
                from.SendLocalizedMessage(1079973, String.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
        }

        private bool CarpentryResmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                //if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                //    return false;

                CraftResourceInfo info = CraftResources.GetInfo(resource);

                if (info == null ) //|| info.ResourceTypes.Length == 0)
                    return false;
				
                CraftItem craftItem = DefCarpentry.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                    return false;

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 1)
                    return false; // Not enough metal to resmelt
					
                double difficulty = 0.0;

                switch ( resource )
                {
                    case CraftResource.RegularWood:
                        difficulty = 00.0;
                        break;
					case CraftResource.OakWood:
                        difficulty = 20.0;
                        break;
                    case CraftResource.AshWood:
                        difficulty = 40.0;
                        break;
                    case CraftResource.YewWood:
                        difficulty = 60.0;
                        break;
                    case CraftResource.Heartwood:
                        difficulty = 80.0;
                        break;
                    case CraftResource.Bloodwood:
                        difficulty = 100.0;
                        break;
                    case CraftResource.Frostwood:
                        difficulty = 120.0;
                        break;
                }
			
                Type resourceType = info.ResourceTypes[0];
                Item wood = (Item)Activator.CreateInstance(resourceType);

                double skill = from.Skills[SkillName.Carpentry].Value;
			
                if (difficulty > skill)
                {
                    m_Failure = true; 
                }
				else
				{
					if ((item is BaseArmor && ((BaseArmor)item).PlayerConstructed) || (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed))
					{
						double amount = craftResource.Amount * 0.5;
						amount *= 2;

						if (amount < 1)
							wood.Amount = 1;
						else
							wood.Amount = (int)amount;
						item.Delete();
					}
					else if (item is CraftableFurniture)
					{
						double amount = craftResource.Amount * 0.5;

						if (amount < 1)
							wood.Amount = 1;
						else
							wood.Amount = (int)amount;
						item.Delete();
					}
					else
						m_Failure = true; 
				}

				if( !m_Failure )
				{
					from.AddToBackpack(wood);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return true;
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
        private void SalvageCarpentry(Mobile from)
        {
            int salvaged = 0;
            int notSalvaged = 0;
			
            Container sBag = this;
			
            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
                if (item is BaseArmor && ((BaseArmor)item).PlayerConstructed)
                {
                    if (CarpentryResmelt(from, item, ((BaseArmor)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
                else if (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed)
                {
                    if (CarpentryResmelt(from, item, ((BaseWeapon)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
                else if (item is CraftableFurniture)
                {
                    if (CarpentryResmelt(from, item, ((CraftableFurniture)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
                from.SendLocalizedMessage(1079973, String.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
        }
		
        private bool BowCraftResmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                //if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                //    return false;

                CraftResourceInfo info = CraftResources.GetInfo(resource);

                if (info == null ) //|| info.ResourceTypes.Length == 0)
                    return false;
				
                CraftItem craftItem = DefBowFletching.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                    return false;

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 1)
                    return false; // Not enough metal to resmelt
					
                double difficulty = 0.0;

                switch ( resource )
                {
                    case CraftResource.RegularWood:
                        difficulty = 00.0;
                        break;
                    case CraftResource.OakWood:
                        difficulty = 20.0;
                        break;
                    case CraftResource.AshWood:
                        difficulty = 40.0;
                        break;
                    case CraftResource.YewWood:
                        difficulty = 60.0;
                        break;
                    case CraftResource.Heartwood:
                        difficulty = 80.0;
                        break;
                    case CraftResource.Bloodwood:
                        difficulty = 100.0;
                        break;
                    case CraftResource.Frostwood:
                        difficulty = 120.0;
                        break;
                }
			
                Type resourceType = info.ResourceTypes[0];
                Item wood = (Item)Activator.CreateInstance(resourceType);

                double skill = from.Skills[SkillName.Fletching].Value;
			
                if (difficulty > skill)
                {
                    m_Failure = true; 
                }
				else
				{
					if (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed)
					{
						double amount = craftResource.Amount * 0.5;
						amount *= 2;

						if (amount < 1)
							wood.Amount = 1;
						else
							wood.Amount = (int)amount;
						item.Delete();
					}
					else
						m_Failure = true; 
				}

				if( !m_Failure )
				{
					from.AddToBackpack(wood);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return true;
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
        private void SalvageBowCraft(Mobile from)
        {
            int salvaged = 0;
            int notSalvaged = 0;
			
            Container sBag = this;
			
            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
                if (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed)
                {
                    if (BowCraftResmelt(from, item, ((BaseWeapon)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
                from.SendLocalizedMessage(1079973, String.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
        }
		
        private bool TailorResmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                //if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                //    return false;

                CraftResourceInfo info = CraftResources.GetInfo(resource);

                if (info == null ) //|| info.ResourceTypes.Length == 0)
                    return false;
				
                CraftItem craftItem = DefTailoring.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                    return false;

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 1)
                    return false; // Not enough metal to resmelt
					
                double difficulty = 0.0;

                switch ( resource )
                {
                    case CraftResource.RegularLeather:
                        difficulty = 0.0;
                        break;
                    case CraftResource.DernedLeather:
                        difficulty = 20.0;
                        break;
                    case CraftResource.RatnedLeather:
                        difficulty = 40.0;
                        break;
                    case CraftResource.SernedLeather:
                        difficulty = 60.0;
                        break;
                    case CraftResource.SpinedLeather:
                        difficulty = 80.0;
                        break;
                    case CraftResource.HornedLeather:
                        difficulty = 100.0;
                        break;
                    case CraftResource.BarbedLeather:
                        difficulty = 120.0;
                        break;
                }
			
                Type resourceType = info.ResourceTypes[0];
                Item leather = (Item)Activator.CreateInstance(resourceType);

                double skill = from.Skills[SkillName.Tailoring].Value;
			
                if (difficulty > skill)
                {
                    m_Failure = true; 
                }
				else
				{
					if (item is BaseArmor && ((BaseArmor)item).PlayerConstructed)
					{
						double amount = craftResource.Amount * 0.5;
							amount *= 2;

						if (amount < 1)
							leather.Amount = 1;
						else
							leather.Amount = (int)amount;
						item.Delete();
					}
					else
					{
						m_Failure = true; 
					}
				}

				if( !m_Failure )
				{
					from.AddToBackpack(leather);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return true;
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
        private void SalvageTailor(Mobile from)
        {
            int salvaged = 0;
            int notSalvaged = 0;
			
            Container sBag = this;
			
            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
                if (item is BaseArmor && ((BaseArmor)item).PlayerConstructed)
                {
                    if (TailorResmelt(from, item, ((BaseArmor)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
                from.SendLocalizedMessage(1079973, String.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
        }
		
        private bool TinkeringResmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                //if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                //    return false;

                CraftResourceInfo info = CraftResources.GetInfo(resource);

                if (info == null ) //|| info.ResourceTypes.Length == 0)
                    return false;
				
                CraftItem craftItem = DefTinkering.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                    return false;

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 1)
                    return false; // Not enough metal to resmelt
					
                double difficulty = 0.0;

                switch ( resource )
                {
                    case CraftResource.Copper:
                        difficulty = 20.0;
                        break;
                    case CraftResource.Bronze:
                        difficulty = 40.0;
                        break;
                    case CraftResource.Gold:
                        difficulty = 60.0;
                        break;
                    case CraftResource.Agapite:
                        difficulty = 80.0;
                        break;
                    case CraftResource.Verite:
                        difficulty = 100.0;
                        break;
                    case CraftResource.Valorite:
                        difficulty = 120.0;
                        break;
                }
			
                Type resourceType = info.ResourceTypes[0];
                Item ingot = (Item)Activator.CreateInstance(resourceType);

                double skill = from.Skills[SkillName.Tinkering].Value;
			
                if (difficulty > skill)
                {
                    m_Failure = true; 
                }
				else
				{
					if (item is BaseJewel && ((BaseJewel)item).PlayerConstructed)
					{
						double amount = craftResource.Amount * 0.5;
							amount *= 2;

						if (amount < 1)
							ingot.Amount = 1;
						else
							ingot.Amount = (int)amount;
						item.Delete();
					}
					else
					{
						m_Failure = true; 
					}
				}

				if( !m_Failure )
				{
					from.AddToBackpack(ingot);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return true;
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
        private void SalvageTinkering(Mobile from)
        {
            int salvaged = 0;
            int notSalvaged = 0;
			
            Container sBag = this;
			
            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
                if (item is BaseJewel && ((BaseJewel)item).PlayerConstructed)
                {
                    if (TinkeringResmelt(from, item, ((BaseJewel)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }

            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
                from.SendLocalizedMessage(1079973, String.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
        }
        private bool ClothingResmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                //if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                //    return false;

                CraftResourceInfo info = CraftResources.GetInfo(resource);

                if (info == null ) //|| info.ResourceTypes.Length == 0)
                    return false;
				
                CraftItem craftItem = DefTailoring.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                    return false;

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 1)
                    return false; // Not enough metal to resmelt
					
                double difficulty = 0.0;
			
                Type resourceType = info.ResourceTypes[0];
                Item leather = (Item)Activator.CreateInstance(resourceType);

                double skill = from.Skills[SkillName.Tailoring].Value;
			
                if (difficulty > skill)
                {
                    m_Failure = true; 
                }
				else
				{
					if (item is BaseClothing && ((BaseClothing)item).PlayerConstructed)
					{
						double amount = craftResource.Amount * 0.5;
							amount *= 2;

						if (amount < 1)
							leather.Amount = 1;
						else
							leather.Amount = (int)amount;
						item.Delete();
					}
					else
					{
						m_Failure = true; 
					}
				}

				if( !m_Failure )
				{
					from.AddToBackpack(leather);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return true;
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }	
        private void SalvageCloth(Mobile from)
        {
            int salvaged = 0;
            int notSalvaged = 0;
			
            Container sBag = this;
			
            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
                if (item is BaseClothing && ((BaseClothing)item).PlayerConstructed)
                {
                    if (ClothingResmelt(from, item, ((BaseClothing)item).Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }

            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
                from.SendLocalizedMessage(1079973, String.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
        }		
 
        private void SalvageAll(Mobile from)
        {
            SalvageBlacksmith(from);
			SalvageBowCraft(from);
			SalvageCarpentry(from);
			SalvageTailor(from);
			SalvageTinkering(from);
            SalvageCloth(from);
        }

        #region ContextMenuEntries
        private class SalvageAllEntry : ContextMenuEntry
        {
            private readonly SalvageBag m_Bag;

            public SalvageAllEntry(SalvageBag bag, bool enabled)
                : base(6276)
            {
                m_Bag = bag;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;

			}

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                    return;

                Mobile from = Owner.From;

                if (from.CheckAlive())
                    m_Bag.SalvageAll(from);
            }
        }

		/*
        private class SalvageIngotsEntry : ContextMenuEntry
        {
            private readonly SalvageBag m_Bag;

            public SalvageIngotsEntry(SalvageBag bag, bool enabled)
                : base(6277)
            {
                m_Bag = bag;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                    return;

                Mobile from = Owner.From;

                if (from.CheckAlive())
                    m_Bag.SalvageIngots(from);
            }
        }

        private class SalvageClothEntry : ContextMenuEntry
        {
            private readonly SalvageBag m_Bag;

            public SalvageClothEntry(SalvageBag bag, bool enabled)
                : base(6278)
            {
                m_Bag = bag;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                    return;

                Mobile from = Owner.From;

                if (from.CheckAlive())
                    m_Bag.SalvageCloth(from);
            }
        }
		*/
        #endregion

        #region Serialization
        public SalvageBag(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
        #endregion
    }
}