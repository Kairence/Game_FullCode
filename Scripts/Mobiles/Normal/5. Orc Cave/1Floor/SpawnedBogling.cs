using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a bogling corpse")]
    public class SpawnedBogling : Bogling
    {
        [Constructable]
        public SpawnedBogling()
        {
            Container pack = this.Backpack;

            if (pack != null)
                pack.Delete();

            this.NoKillAwards = true;
			this.Fame = 0;
			this.Karma = 0;
        }

        public SpawnedBogling(Serial serial)
            : base(serial)
        {
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            c.Delete();
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
            this.NoKillAwards = true;
        }
    }
}