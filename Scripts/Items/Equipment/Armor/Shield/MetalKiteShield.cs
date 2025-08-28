using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishKiteShield))]
    public class MetalKiteShield : BaseShield, IDyable
    {
        [Constructable]
        public MetalKiteShield()
            : base(0x1B74)
        {
            this.Weight = 25.0;
			PrefixOption[61] = 41; //시전 속도
			SuffixOption[61] = -500000;
			PrefixOption[62] = 109; //방패 방어 확률
			SuffixOption[62] = 300000;
			PrefixOption[63] = 110; //모든 피격 데미지 감소
			SuffixOption[63] = 350000;
        }

        public MetalKiteShield(Serial serial)
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
                return 100;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 30;
            }
        }
        public bool Dye(Mobile from, DyeTub sender)
        {
            if (this.Deleted)
                return false;

            this.Hue = sender.DyedHue;

            return true;
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
    }
}