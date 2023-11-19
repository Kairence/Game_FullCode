using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a plant corpse")]
    public class Bogling : BaseCreature
    {
        [Constructable]
        public Bogling()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a bogling";
            Body = 779;
            BaseSoundID = 422;

            SetStr(106, 150);
            SetDex(10, 15);
            SetInt(20, 25);

            //SetHits(45, 47);
			SetHits(200, 210);
			SetStam(105, 125);
			SetMana(11, 15);
            //SetDamage(2, 4);
			SetDamage(3, 16);

			SetAttackSpeed( 20.0 );
			
            SetResistance(ResistanceType.Physical, 20, 25);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 15, 25);
            SetResistance(ResistanceType.Poison, 15, 25);
            SetResistance(ResistanceType.Energy, 15, 25);			
			
            SetDamageType(ResistanceType.Physical, 100);

            SetSkill(SkillName.MagicResist, 1.1, 2.0);
            SetSkill(SkillName.Tactics, 1.1, 2.0);
            SetSkill(SkillName.Wrestling, 1.1, 2.0);

            Fame = 2000;
            Karma = -2000;

            VirtualArmor = 1;

            PackItem(new BarkFragment(4));
            //PackItem(new Engines.Plants.Seed());
        }

        public Bogling(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 6;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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