using System;
using Server.Mobiles;

namespace Server.Mobiles
{
    [CorpseName("a giant cow corpse")]
    public class DevelCow : Cow
    {
        [Constructable]
        public DevelCow()
            : base()
        {
            this.Name = "DevelCow";
            this.Body = 0xE7;
            this.BodySize = 2.0;

            this.SetStr(500);
            this.SetDex(100);
            this.SetInt(50);

            this.SetHits(2500);
            this.SetStam(100);
            this.SetMana(0);

            this.SetDamage(20, 35);

            this.Fame = 5000;
            this.Karma = 0;
            this.Tamable = true;
            this.MinTameSkill = 99.9;
        }

        public DevelCow(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
