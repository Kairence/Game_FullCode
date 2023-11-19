using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B74, 0x316B)]
    public class HideChest : BaseArmor
    {
        [Constructable]
        public HideChest()
            : base(0x2B74)
        {
            this.Weight = 6.0;
  			Attributes.CastSpeed += 100;
        }

        public HideChest(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 35;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 45;
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
                return 25;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 5;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Cloth;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override ArmorMeditationAllowance DefMedAllowance
        {
            get
            {
                return ArmorMeditationAllowance.Half;
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