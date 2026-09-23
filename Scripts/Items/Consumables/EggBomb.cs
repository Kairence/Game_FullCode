#region References
using System;
using Server.SkillHandlers;
#endregion

namespace Server.Items
{
	public class EggBomb : Item, ICommodity
    {
		[Constructable]
		public EggBomb()
			: base(0x2808)
		{
			// Item ID should be 0x2809 - Temporary solution for clients 7.0.0.0 and up
			Stackable = Core.ML;
			Weight = 1.0;
		}

		public EggBomb(Serial serial)
			: base(serial)
		{ }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public override int LabelNumber { get { return 1030249; } }

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				// The item must be in your backpack to use it.
				from.SendLocalizedMessage(1060640);
			}
			else if (from.Skills.Hiding.Value < 150.0)
			{
				from.SendLocalizedMessage(1063013, "150\tHiding");
			}
			else if (from.NextSkillTime > Core.TickCount)
			{
				// You must wait a few seconds before you can use that item.
				from.SendLocalizedMessage(1070772);
			}
			else if (from.Mana < 10)
			{
				// You don't have enough mana to do that.
				from.SendLocalizedMessage(1049456);
			}
			else
			{
				Hiding.CombatOverride = true;

				// [커스텀: 에그밤 100% 은신 및 5초 이동 불가]
				from.Hidden = true;
				if (from is Server.Mobiles.PlayerMobile pm) pm.realHidden = true;
				from.Warmode = false;
				Server.Spells.Sixth.InvisibilitySpell.RemoveTimer(from);
                Server.Items.InvisibilityPotion.RemoveTimer(from);
				from.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x1F4, 501240); // You have hidden yourself well.

				from.Mana -= 10;
				from.FixedParticles(0x3709, 1, 30, 9904, 1108, 6, Server.EffectLayer.RightFoot);
				from.PlaySound(0x22F);

				from.Paralyze(TimeSpan.FromSeconds(5.0)); // 5초 이동 불가
				from.SendMessage("연막에 휩싸여 5초간 이동할 수 없습니다!");

				Consume();

				Hiding.CombatOverride = false;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (ItemID == 0x2809) // Temporary solution for clients 7.0.0.0 and up
			{
				ItemID = 0x2808;
			}
		}
	}
}
