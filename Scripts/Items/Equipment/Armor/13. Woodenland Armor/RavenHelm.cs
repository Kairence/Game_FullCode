using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B71, 0x3168)]
    public class RavenHelm : BaseArmor
    {
        [Constructable]
        public RavenHelm()
            : base(0x2B71)
        {
			Attributes.RegenHits += 5;
            this.Weight = 5.0;
        }

        public RavenHelm(Serial serial)
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
                return 75;
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
                return 13;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Plate;
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