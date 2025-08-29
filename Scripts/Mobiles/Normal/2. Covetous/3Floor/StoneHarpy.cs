using System;

namespace Server.Mobiles
{
    [CorpseName("a stone harpy corpse")]
    public class StoneHarpy : BaseCreature
    {
        [Constructable]
        public StoneHarpy()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a stone harpy";
            this.Body = 73;
            this.BaseSoundID = 402;

            this.SetStr(3400, 4500);
            this.SetDex(1050, 2100);
            this.SetInt(1050, 2100);

            this.SetHits(4350, 4500);
			SetStam(3450, 4100);
            this.SetMana(1000, 2000);

			SetAttackSpeed( 15.0 );

            this.SetDamage(410, 600);

            this.SetDamageType(ResistanceType.Physical, 75);
            this.SetDamageType(ResistanceType.Poison, 25);

            this.SetResistance(ResistanceType.Physical, 80, 100);
            this.SetResistance(ResistanceType.Fire, 40, 60);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 80, 100);
            this.SetResistance(ResistanceType.Energy, 20, 30);

            this.SetSkill(SkillName.MagicResist, 95.1, 99.5);
            this.SetSkill(SkillName.Tactics, 95.1, 100.0);
            this.SetSkill(SkillName.Wrestling, 95.1, 100.0);

            this.Fame = 11500;
            this.Karma = -11500;

            this.VirtualArmor = 35;
        }

        public StoneHarpy(Serial serial)
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
            this.AddLoot(LootPack.Average, 2);
            this.AddLoot(LootPack.Gems, 2);
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