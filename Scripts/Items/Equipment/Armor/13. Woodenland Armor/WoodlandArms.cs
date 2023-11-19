using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B6C, 0x3163)]
    public class WoodlandArms : BaseArmor
    {
        [Constructable]
        public WoodlandArms()
            : base(0x2B6C)
        {
  			AbsorptionAttributes.SoulChargeKinetic += 120;
            this.Weight = 5.0;
        }

        public WoodlandArms(Serial serial)
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
                return 85;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 80;
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