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

            this.SetStr(5, 15);      // 최종 Str 551~561
			this.SetDex(60, 80);     
			this.SetInt(5, 15);      

			this.SetHits(30, 80);    // 최종 Hits 1,250~1,300
			this.SetStam(60, 80);

			SetAttackSpeed(2.5);
			SetDamage(4, 8); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Cold, 10, 15);

			this.SetSkill(SkillName.Wrestling, 18.5, 28.5);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 29.1;

			this.Fame = 600;
			this.Karma = 0;

            this.SetResistance(ResistanceType.Physical, 5, 5);
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