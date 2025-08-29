using System;

namespace Server.Mobiles
{
    [CorpseName("a crane corpse")]
    public class Crane : BaseCreature
    {
        [Constructable]
        public Crane()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a crane";
            this.Body = 254;
            this.BaseSoundID = 0x4D7;

            this.SetStr(260, 350);
            this.SetDex(16, 25);
            this.SetInt(11, 15);

            this.SetHits(26, 35);
			SetAttackSpeed( 3.0 );
			SetStam(10, 15);
            this.SetMana(10, 15);

            this.SetDamage(1, 2);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 5, 5);

            this.Fame = 0;
            this.Karma = 200;

            this.VirtualArmor = 1;
        }

        public Crane(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Feathers
        {
            get
            {
                return 25;
            }
        }
        public override int GetAngerSound()
        {
            return 0x4D9;
        }

        public override int GetIdleSound()
        {
            return 0x4D8;
        }

        public override int GetAttackSound()
        {
            return 0x4D7;
        }

        public override int GetHurtSound()
        {
            return 0x4DA;
        }

        public override int GetDeathSound()
        {
            return 0x4D6;
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