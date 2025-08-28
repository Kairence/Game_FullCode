using System;

namespace Server.Mobiles
{
    [CorpseName("a vampire bat corpse")]
    public class VampireBat : BaseCreature
    {
        [Constructable]
        public VampireBat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a vampire bat";
            this.Body = 317;
            this.BaseSoundID = 0x270;

            this.SetStr(270, 290);
            this.SetDex(150, 200);
            this.SetInt(35, 80);

            this.SetHits(450, 550);
			SetStam(350, 500);
			SetMana(10, 20);
			
            this.SetDamage(15, 19);

			SetAttackSpeed( 2.0 );
			
            this.SetDamageType(ResistanceType.Physical, 80);
            this.SetDamageType(ResistanceType.Poison, 20);

            this.SetResistance(ResistanceType.Physical, 35, 45);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 60, 85);
            this.SetResistance(ResistanceType.Energy, 20, 25);

            this.SetSkill(SkillName.MagicResist, 20.1, 25.0);
            this.SetSkill(SkillName.Tactics, 25.1, 30.0);
            this.SetSkill(SkillName.Wrestling, 20.1, 25.0);

            this.Fame = 1750;
            this.Karma = -1750;

            this.VirtualArmor = 6;
        }

        public VampireBat(Serial serial)
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
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
        }

        public override int GetIdleSound()
        {
            return 0x29B;
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