using System;

namespace Server.Mobiles
{
    [CorpseName("an undead horse corpse")]
    public class SkeletalMount : BaseMount
    {
        [Constructable] 
        public SkeletalMount()
            : this("Boss a skeletal steed")
        {
        }

        [Constructable]
        public SkeletalMount(string name)
            : base(name, 793, 0x3EBB, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.SetStr(10491, 15000);
            this.SetDex(6460, 8550);
            this.SetInt(1460, 1600);

			SetHits(149302, 194979);
			SetMana(11000, 11500);
			SetStam(136000, 150000);

            this.SetDamage(128, 1200);
			SetAttackSpeed( 2.5 );

			Boss = true;
			
            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Cold, 50);

            this.SetResistance(ResistanceType.Physical, 60, 80);
            this.SetResistance(ResistanceType.Cold, 90, 95);
            this.SetResistance(ResistanceType.Poison, 100);
            this.SetResistance(ResistanceType.Energy, 10, 15);

            this.SetSkill(SkillName.MagicResist, 235.1, 240.0);
            this.SetSkill(SkillName.Tactics, 250.0);
            this.SetSkill(SkillName.Wrestling, 220.1, 230.0);

            VirtualArmor = 77;

            this.Fame = 22000;
            this.Karma = -22000;
        }

        public SkeletalMount(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
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

            switch( version )
            {
                case 0:
                    {
                        this.Name = "Boss a skeletal steed";
                        this.Tamable = false;
                        this.MinTameSkill = 0.0;
                        this.ControlSlots = 0;
                        break;
                    }
            }
        }
    }
}