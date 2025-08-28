using System;

namespace Server.Mobiles
{
    [CorpseName("a sea horse corpse")]
    public class SeaHorse : BaseMount
    {
        [Constructable]
        public SeaHorse()
            : this("a sea horse")
        {
			this.CanSwim = true;
        }

        [Constructable]
        public SeaHorse(string name)
            : base(name, 0xD2, 0x3EB3, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
            //: base(name, 0x90, 0x3EB3, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.SetStr(200, 270);
            this.SetDex(156, 175);
            this.SetInt(16, 20);

            this.SetHits(271, 288);
			this.SetStam(100, 120);
            this.SetMana(1, 5);

            this.SetDamage(1, 4);
			SetAttackSpeed(20.0);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.Fame = 450;
            this.Karma = 0;
        }

        public SeaHorse(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					this.CanSwim = true;
					break;
			}
        }
    }
}