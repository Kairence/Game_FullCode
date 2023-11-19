using System;
using System.Collections.Generic;
using Server.Targeting;

namespace Server.Items
{
    public class ArcaneGem : Item, ICommodity
    {
        public const int DefaultArcaneHue = 2117;
        public override int LabelNumber {get {return 1114115;} } // Arcane Gem

        [Constructable]
        public ArcaneGem()
            : this(1)
        {
        }

        [Constructable]
        public ArcaneGem(int amount)
            : base(0x1EA7)
        {
            Stackable = true;
            Amount = amount;
            Weight = 1.0;
        }

        public ArcaneGem(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }
       
        public static bool ConsumeCharges(Mobile from, int amount)
        {
            List<Item> items = from.Items;
            int avail = 0;

            for (int i = 0; i < items.Count; ++i)
            {
                Item obj = items[i];

                if (obj is IArcaneEquip)
                {
                    IArcaneEquip eq = (IArcaneEquip)obj;

                    if (eq.IsArcane)
                        avail += eq.CurArcaneCharges;
                }
            }

            if (avail < amount)
                return false;

            for (int i = 0; i < items.Count; ++i)
            {
                Item obj = items[i];

                if (obj is IArcaneEquip)
                {
                    IArcaneEquip eq = (IArcaneEquip)obj;

                    if (eq.IsArcane)
                    {
                        if (eq.CurArcaneCharges > amount)
                        {
                            eq.CurArcaneCharges -= amount;
                            break;
                        }
                        else
                        {
                            amount -= eq.CurArcaneCharges;
                            eq.CurArcaneCharges = 0;
                        }
                    }
                }
            }

            return true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042010); // You must have the object in your backpack to use it.
            }
            else
            {
                from.BeginTarget(2, false, TargetFlags.None, new TargetCallback(OnTarget));
            }
        }

		/*
        public int GetChargesFor(Mobile m)
        {
            int v = (int)(m.Skills[SkillName.Tailoring].Value / 5);

            if (v < 16)
                return 16;
            else if (v > 24)
                return 24;

            return v;
        }
		*/
        public void OnTarget(Mobile from, object obj)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042010); // You must have the object in your backpack to use it.
                return;
            }
			
			/*
			if( obj is Item )
			{
				from.SendMessage("어느 장비의 단계를 향상시키겠습니까?");
				Item item = obj as Item;
				Misc.Util.TierUpgrade(from, 0, item, true );		
			}
			
			if ( obj is IDurability)
			{
                Item item = (Item)obj;
                IDurability wearable = (IDurability)obj;
				if (!wearable.CanFortify || wearable.MaxHitPoints > 254)
				{
					from.SendMessage("이 아이템에는 사용할 수 없습니다.");
					return;
				}
				int charge = 0;
				if( item is BaseWeapon )
				{
					BaseWeapon chargeitem = item as BaseWeapon;
					if( (int)chargeitem.ItemPower >= 4 && (int)chargeitem.ItemPower <= 8 )
						charge = (int)chargeitem.ItemPower - 3;
				}
				else if( item is BaseArmor )
				{
					BaseArmor chargeitem = item as BaseArmor;
					if( (int)chargeitem.ItemPower >= 4 && (int)chargeitem.ItemPower <= 8 )
						charge = (int)chargeitem.ItemPower - 3;
				}
				else if( item is BaseClothing )
				{
					BaseClothing chargeitem = item as BaseClothing;
					if( (int)chargeitem.ItemPower >= 4 && (int)chargeitem.ItemPower <= 8 )
						charge = (int)chargeitem.ItemPower - 3;
				}
				else if( item is BaseJewel )
				{
					BaseJewel chargeitem = item as BaseJewel;
					if( (int)chargeitem.ItemPower >= 4 && (int)chargeitem.ItemPower <= 8 )
						charge = (int)chargeitem.ItemPower - 3;
				}
				else if( item is Spellbook )
				{
					Spellbook chargeitem = item as Spellbook;
					if( (int)chargeitem.ItemPower >= 4 && (int)chargeitem.ItemPower <= 8 )
						charge = (int)chargeitem.ItemPower - 3;
				}
				else
				{
					from.SendMessage("사용할 수 없는 장비입니다.");
					return;
				}
				if( charge > 0 && Amount >= charge )
				{
					from.PlaySound(0x247);
					wearable.MaxHitPoints++;
					if( Amount == charge )
						Delete();
					else
						Amount -= charge;
				}
			}
			*/
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
