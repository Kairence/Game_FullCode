using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
    public class Buckler : BaseShield
    {
        public override int InitMinHits { get { return 40; } }
        public override int InitMaxHits { get { return 50; } }
        public override int AosStrReq { get { return 25; } }
        public override int AosDexReq { get { return 25; } }
        public override int ArmorBase { get { return 19; } }
		
        [Constructable]
        public Buckler()
            : base(0x1B73)
        {
            Weight = 5.0;
        }

        public Buckler(Serial serial)
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