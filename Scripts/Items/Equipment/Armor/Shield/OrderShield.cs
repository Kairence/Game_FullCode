using System;
using Server.Guilds;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishOrderShield))]
    public class OrderShield : BaseShield
    {
        [Constructable]
        public OrderShield()
            : base(0x1BC4)
        {
            Weight = 10.0;
			PrefixOption[61] = 109;
			SuffixOption[61] = 50;
			PrefixOption[62] = 110;
			SuffixOption[62] = 6500;
			PrefixOption[63] = 110;
			SuffixOption[63] = 400;				
			PrefixOption[64] = 94;
			SuffixOption[64] = 500;				
        }

        public OrderShield(Serial serial)
            : base(serial)
        {
        }
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 8000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 2500;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 100;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 0;
            }
        }
		
		public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);//version
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