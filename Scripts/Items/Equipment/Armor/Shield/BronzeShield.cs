using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
    public class BronzeShield : BaseShield
    {
        public override int InitMinHits { get { return 25; } }
        public override int InitMaxHits { get { return 30; } }
        public override int AosStrReq { get { return 20; } }
        public override int AosIntReq { get { return 40; } }
        public override int ArmorBase { get { return 16; } }
		
        [Constructable]
        public BronzeShield()
            : base(0x1B72)
        {
            Weight = 6.0;
			Attributes.SpellDamage += 100;
        }

        public BronzeShield(Serial serial)
            : base(serial)
        {
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