using System;

namespace Server.Mobiles
{
    [CorpseName("a harpy corpse")]
    public class Harpy : BaseCreature
    {
        [Constructable]
        public Harpy()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a harpy";
            this.Body = 30;
            this.BaseSoundID = 402;

            this.SetStr(650, 700);
            this.SetDex(1250, 1300);
            this.SetInt(100, 500);

            this.SetHits(1410, 1600);
			SetStam(1000, 1200);
			SetMana(40, 80);
			
            this.SetDamage(25, 47);

			SetAttackSpeed( 2.5 );
			
			
            this.SetDamageType(ResistanceType.Physical, 80);
            this.SetDamageType(ResistanceType.Poison, 20);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Fire, 10, 20);
            this.SetResistance(ResistanceType.Cold, 10, 30);
            this.SetResistance(ResistanceType.Poison, 20, 30);
            this.SetResistance(ResistanceType.Energy, 10, 20);

            this.SetSkill(SkillName.MagicResist, 55.1, 57.5);
			this.SetSkill(SkillName.Tactics, 56.1, 60.0);
            this.SetSkill(SkillName.Wrestling, 56.1, 60.0);

            this.Fame = 5000;
            this.Karma = -5000;

            this.VirtualArmor = 17;
        }

        public Harpy(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override int Feathers
        {
            get
            {
                return 50;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager, 2);
        }

        public override int GetAttackSound()
        {
            return 916;
        }

        public override int GetAngerSound()
        {
            return 916;
        }

        public override int GetDeathSound()
        {
            return 917;
        }

        public override int GetHurtSound()
        {
            return 919;
        }

        public override int GetIdleSound()
        {
            return 918;
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