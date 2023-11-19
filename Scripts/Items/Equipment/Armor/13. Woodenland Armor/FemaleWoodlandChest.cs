using System;

namespace Server.Items
{
    //Is this a filler-type item? the clilocs don't match up and at a glacnce I can't find direct reference of it
    [FlipableAttribute(0x2B6D, 0x3164)]
    public class FemaleElvenPlateChest : BaseArmor
    {
        [Constructable]
        public FemaleElvenPlateChest()
            : base(0x2B6D)
        {
  			AbsorptionAttributes.SoulChargeKinetic += 140;
            this.Weight = 8.0;
        }

        public FemaleElvenPlateChest(Serial serial)
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
                return 105;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 95;
            }
        }
        public override bool AllowMaleWearer
        {
            get
            {
                return false;
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