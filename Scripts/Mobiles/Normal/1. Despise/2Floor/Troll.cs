using System;

namespace Server.Mobiles
{
    [CorpseName("a troll corpse")]
    public class Troll : BaseCreature
    {
        [Constructable]
        public Troll()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a troll";
            this.Body = 54;//Utility.RandomList(53, 54);
            this.BaseSoundID = 461;

            this.SetStr(476, 485);
            this.SetDex(446, 455);
            this.SetInt(116, 125);

            this.SetHits(1540, 1566);
			SetMana(10, 15);
			SetStam(355, 360);

            this.SetDamage(9, 40);

			SetAttackSpeed( 7.5 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 35, 45);

            this.SetResistance(ResistanceType.Physical, 55, 65);
            this.SetResistance(ResistanceType.Fire, 25, 35);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 5, 15);
            this.SetResistance(ResistanceType.Energy, 55, 65);			
			
            this.SetSkill(SkillName.MagicResist, 101.1, 105.0);
            this.SetSkill(SkillName.Tactics, 105.1, 107.0);
            this.SetSkill(SkillName.Wrestling, 105.1, 107.0);

            this.Fame = 10000;
            this.Karma = -10000;

            this.VirtualArmor = 40;
        }

        public Troll(Serial serial)
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
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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