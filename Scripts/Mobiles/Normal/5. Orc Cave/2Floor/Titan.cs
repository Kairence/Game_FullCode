using System;

namespace Server.Mobiles
{
    [CorpseName("a titans corpse")]
    public class Titan : BaseCreature
    {
        [Constructable]
        public Titan()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a titan";
            this.Body = 76;
            this.BaseSoundID = 609;

            this.SetStr(2360, 2850);
            this.SetDex(2600, 4500);
            this.SetInt(4810, 5050);

            this.SetHits(13232, 15351);
			this.SetStam(8000, 8500);
			this.SetMana(13200, 15000);
			
            this.SetDamage(282, 1020);

			SetAttackSpeed( 10.0 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 65, 75);
            this.SetResistance(ResistanceType.Fire, 40, 50);
            this.SetResistance(ResistanceType.Cold, 65, 75);
            this.SetResistance(ResistanceType.Poison, 50, 60);
            this.SetResistance(ResistanceType.Energy, 60, 70);

            this.SetSkill(SkillName.EvalInt, 185.1, 200.0);
            this.SetSkill(SkillName.Magery, 185.1, 200.0);
            this.SetSkill(SkillName.MagicResist, 180.2, 210.0);
            this.SetSkill(SkillName.Tactics, 160.1, 180.0);
            this.SetSkill(SkillName.Wrestling, 140.1, 150.0);

            this.Fame = 21000;
            this.Karma = -21000;

            this.VirtualArmor = 100;
		
        }

        public Titan(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override void GenerateLoot()
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