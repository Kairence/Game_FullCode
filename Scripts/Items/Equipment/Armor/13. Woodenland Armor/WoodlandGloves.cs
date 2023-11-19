using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B6A, 0x3161)]
    public class WoodlandGloves : BaseArmor
    {
        [Constructable]
        public WoodlandGloves()
            : base(0x2B6A)
        {
  			AbsorptionAttributes.SoulChargeKinetic += 100;
            this.Weight = 2.0;
        }

        public WoodlandGloves(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 50;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 65;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 65;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 70;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 13;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Wood;
            }
        }
        public override Race RequiredRace
        {
            get
            {
                return Race.Elf;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}