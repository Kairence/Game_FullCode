using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Network;
using System.Linq;
using Server.Mobiles;
using Server.Gumps;

namespace Server.Items
{
    public class EquipBag : Bag
    {
        private bool m_Failure;

		private bool m_Active;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Active
		{
			get { return m_Active; }
			set { m_Active = value; InvalidateProperties(); }
		}		

		private bool m_Weapon;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Weapon
		{
			get { return m_Weapon; }
			set { m_Weapon = value; InvalidateProperties(); }
		}

		private bool m_Armor;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Armor
		{
			get { return m_Armor; }
			set { m_Armor = value; InvalidateProperties(); }
		}		

		private bool m_Jewelry;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Jewelry
		{
			get { return m_Jewelry; }
			set { m_Jewelry = value; InvalidateProperties(); }
		}		
		
		
        public override int LabelNumber
        {
            get
            {
                return 1079931;
            }
        }// Salvage Bag

        [Constructable]
        public EquipBag()
            : this(Utility.RandomRedHue())
        {
        }

        [Constructable]
        public EquipBag(int hue)
        {
            Weight = 2.0;
            Hue = hue;
            m_Failure = false;
			Name = "장비 저장 가방";
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive)
            {
                list.Add(new OnEntry(this, m_Active ) );
                list.Add(new AllEntry(this, true ) );
                list.Add(new WeaponEntry(this, m_Weapon ) );
                list.Add(new ArmorEntry(this, m_Armor ) );
                list.Add(new JewelryEntry(this, m_Jewelry ) );
            }
        }

		//int[] rankpoint = { 1, 20, 50, 100, 200, 400, 600, 800, 1000 };
		
		/*
        #region Salvaging
        private void SalvageAll(Mobile from)
        {
            Container eBag = this;
			
            List<Item> Smeltables = eBag.FindItemsByType<Item>();

			bool notsmelt = false;
			
            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];
				
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					if( item is BaseJewel )
					{
						if( ((BaseJewel)item).Owner != null && ((BaseJewel)item).Owner != from )
						{
							notsmelt = true;
							continue;
						}
						int point = rankpoint[(int)((BaseJewel)item).ItemPower];
						if( point > 0 )
						{
							BaseJewel equip = item as BaseJewel;
							if( equip.Layer == Layer.Ring )
								pm.EquipPoint[17] += point;
							else if( equip.Layer == Layer.Bracelet )
								pm.EquipPoint[22] += point;
							else if( equip.Layer == Layer.Earrings )
								pm.EquipPoint[23] += point;
							else if( equip.Layer == Layer.Neck )
								pm.EquipPoint[24] += point;
							item.Delete();
						}
					}
					else if( item is BaseClothing )
					{
						if( ((BaseClothing)item).Owner != null && ((BaseClothing)item).Owner != from )
						{
							notsmelt = true;
							continue;
						}
						int point = rankpoint[(int)((BaseClothing)item).ItemPower];
						if( point > 0 )
						{
							pm.EquipPoint[16] += point;
							item.Delete();
						}
					}
					else if( item is Spellbook )
					{
						if( ((Spellbook)item).Owner != null && ((Spellbook)item).Owner != from )
						{
							notsmelt = true;
							continue;
						}
						int point = rankpoint[(int)((Spellbook)item).ItemPower];
						if( point > 0 )
						{
							if( item is Magerybook )
								pm.EquipPoint[9] += point;
							else if( item is NecromancerSpellbook )
								pm.EquipPoint[19] += point;
							else if( item is SpellweavingBook )
								pm.EquipPoint[20] += point;
							else if( item is MysticBook )
								pm.EquipPoint[21] += point;
							item.Delete();
						}
					}
					else if( item is BaseArmor )
					{
						if( ((BaseArmor)item).Owner != null && ((BaseArmor)item).Owner != from )
						{
							notsmelt = true;
							continue;
						}
						int point = rankpoint[(int)((BaseArmor)item).ItemPower];
						if( point > 0 )
						{
							BaseArmor equip = item as BaseArmor;
							if( equip.Layer == Layer.TwoHanded )
								pm.EquipPoint[25] += point;
							else if( equip.Layer == Layer.Neck )
								pm.EquipPoint[10] += point;
							else if( equip.Layer == Layer.Gloves )
								pm.EquipPoint[11] += point;
							else if( equip.Layer == Layer.Arms )
								pm.EquipPoint[12] += point;
							else if( equip.Layer == Layer.Helm )
								pm.EquipPoint[13] += point;
							else if( equip.Layer == Layer.Pants )
								pm.EquipPoint[14] += point;
							else if( equip.Layer == Layer.InnerTorso )
								pm.EquipPoint[15] += point;
							item.Delete();
						}
					}
					else if( item is BaseWeapon )
					{
						if( ((BaseWeapon)item).Owner != null && ((BaseWeapon)item).Owner != from )
						{
							notsmelt = true;
							continue;
						}
						int point = rankpoint[(int)((BaseWeapon)item).ItemPower];
						if( point > 0 )
						{
							BaseWeapon equip = item as BaseWeapon;
							if( equip.Skill is SkillName.Swords )
							{
								if( equip is BaseAxe )
									pm.EquipPoint[2] += point;
								else if( equip.Layer == Layer.TwoHanded )
									pm.EquipPoint[1] += point;
								else if( equip.Layer == Layer.OneHanded )
									pm.EquipPoint[0] += point;
							}
							else if( equip.Skill is SkillName.Macing )
							{
								if( equip.Layer == Layer.TwoHanded )
									pm.EquipPoint[4] += point;
								else if( equip.Layer == Layer.OneHanded )
									pm.EquipPoint[3] += point;
							}
							else if( equip.Skill is SkillName.Fencing )
							{
								if( equip.Layer == Layer.TwoHanded )
									pm.EquipPoint[6] += point;
								else if( equip.Layer == Layer.OneHanded )
									pm.EquipPoint[5] += point;
							}
							else if( equip.Skill is SkillName.Throwing )
							{
								pm.EquipPoint[18] += point;
							}
							else if( equip is BaseRanged )
							{
								if( ((BaseRanged)equip).AmmoType == typeof(Bolt) )
									pm.EquipPoint[8] += point;
								else if( ((BaseRanged)equip).AmmoType == typeof(Arrow) )
									pm.EquipPoint[7] += point;
							}
							item.Delete();
						}
					}
					from.CloseGump(typeof(EquipPointGump));
					from.SendGump(new EquipPointGump(pm));
				}
            }
			if( notsmelt )
				from.SendMessage("소유자가 없거나 본인이 소유자가 아닌 경우 분해할 수 없습니다!");
        }

        #endregion
		*/
        #region ContextMenuEntries
        private class OnEntry : ContextMenuEntry
        {
            private readonly EquipBag m_Bag;

            public OnEntry(EquipBag bag, bool enabled)
                : base(6302)
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
                    m_Bag.Active = !m_Bag.Active;
            }
        }

        private class AllEntry : ContextMenuEntry
        {
            private readonly EquipBag m_Bag;

            public AllEntry(EquipBag bag, bool enabled)
                : base(6303)
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
				{
                    m_Bag.Weapon = true;
                    m_Bag.Armor = true;
                    m_Bag.Jewelry = true;
				}
            }
        }

        private class WeaponEntry : ContextMenuEntry
        {
            private readonly EquipBag m_Bag;

            public WeaponEntry(EquipBag bag, bool enabled)
                : base(6304)
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
				{
                    m_Bag.Weapon = !m_Bag.Weapon;
				}
            }
        }
		
        private class ArmorEntry : ContextMenuEntry
        {
            private readonly EquipBag m_Bag;

            public ArmorEntry(EquipBag bag, bool enabled)
                : base(6305)
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
				{
                    m_Bag.Armor = !m_Bag.Armor;
				}
            }
        }
        private class JewelryEntry : ContextMenuEntry
        {
            private readonly EquipBag m_Bag;

            public JewelryEntry(EquipBag bag, bool enabled)
                : base(6306)
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
				{
                    m_Bag.Jewelry = !m_Bag.Jewelry;
				}
            }
        }

        #endregion

        #region Serialization
        public EquipBag(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)1); // version
			
			writer.Write((bool)m_Active);
			writer.Write((bool)m_Weapon);
			writer.Write((bool)m_Armor);
			writer.Write((bool)m_Jewelry);
			
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
			
			if( version == 1 )
			{
				m_Active = reader.ReadBool();
				m_Weapon = reader.ReadBool();
				m_Armor = reader.ReadBool();
				m_Jewelry = reader.ReadBool();
			}
        }
        #endregion
    }
}