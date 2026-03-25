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
            this.SetStr(1, 10);
			this.SetDex(45, 75); 

			this.SetHits(89, 139); // 최종 Hits 800~850
			this.SetStam(45, 75); 
			this.SetMana(0);

			SetAttackSpeed(3.0);
			SetDamage(12, 18); // 말(14-22)보다 약간 약한 해상 가축 

			this.SetDamageType(ResistanceType.Physical, 100);
			this.SetResistance(ResistanceType.Physical, 10, 20);
			this.SetResistance(ResistanceType.Cold, 25, 35);

			this.Fame = 300;
			this.Tamable = true;
			this.MinTameSkill = 15.1;
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
