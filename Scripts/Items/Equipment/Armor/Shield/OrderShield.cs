using System;
using Server.Engines.Craft;
using Server.Guilds;

namespace Server.Items
{
	[Alterable(typeof(DefBlacksmithy), typeof(GargishOrderShield))]
	public class OrderShield : BaseShield
	{
		[Constructable]
		public OrderShield()
			: base(0x1BC4)
		{
			Weight = 50.0;
			ShieldMinDamage = 6;
			ShieldMaxDamage = 10;
			PrefixOption[61] = 103; //시전 속도
			SuffixOption[61] = 100000;
		}

		public OrderShield(Serial serial)
			: base(serial) { }

		public override int InitMinHits
		{
			get { return 100; }
		}
		public override int InitMaxHits
		{
			get { return 100; }
		}
		public override int AosStrReq
		{
			get { return 5000; }
		}
		public override int AosDexReq
		{
			get { return 1000; }
		}
		public override int AosIntReq
		{
			get { return 1000; }
		}
		public override int ArmorBase
		{
			get { return 5; }
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); //version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override bool OnEquip(Mobile from)
		{
			return this.Validate(from) && base.OnEquip(from);
		}

		public override void OnSingleClick(Mobile from)
		{
			if (this.Validate(this.Parent as Mobile))
				base.OnSingleClick(from);
		}

		public virtual bool Validate(Mobile m)
		{
			if (Core.AOS || m == null || !m.Player || m.IsStaff())
				return true;

			Guild g = m.Guild as Guild;

			if (g == null || g.Type != GuildType.Order)
			{
				m.FixedEffect(0x3728, 10, 13);
				this.Delete();

				return false;
			}

			return true;
		}
	}
}
