using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B73, 0x316A)]
    public class WingedHelm : BaseArmor
    {
        [Constructable]
        public WingedHelm()
            : base(0x2B73)
        {
            this.Weight = 5.0;
 			Attributes.WeaponSpeed += 95;
       }

        public WingedHelm(Serial serial)
            : base(serial)
        {
        }

        public override Race RequiredRace
        {
            get
            {
                return Race.Elf;
            }
        }
        public override int InitMinHits
        {
            get
            {
                return 45;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 55;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 25;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 25;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 7;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Leather;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}