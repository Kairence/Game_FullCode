using System;
using Server.Guilds;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishChaosShield))]
    public class ChaosShield : BaseShield
    {
        [Constructable]
        public ChaosShield()
            : base(0x1BC3)
        {
            Weight = 40.0;
			PrefixOption[61] = 104; //시전 속도
			SuffixOption[61] = 100000;
			PrefixOption[62] = 109; //방패 방어 확률
			SuffixOption[62] = 200000;
			PrefixOption[63] = 110; //모든 피격 데미지 감소
			SuffixOption[63] = 50000;
		
        }

        public ChaosShield(Serial serial)
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
                return 3500;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 1000;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 20;
            }
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);//version
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
            if (m == null || !m.Player || m.IsStaff() || Core.AOS)
                return true;

            Guild g = m.Guild as Guild;

            if (g == null || g.Type != GuildType.Chaos)
            {
                m.FixedEffect(0x3728, 10, 13);
                this.Delete();

                return false;
            }

            return true;
        }
    }
}