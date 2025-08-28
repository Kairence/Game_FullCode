using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a balron corpse")]
    public class Balron : BaseCreature
    {
        [Constructable]
        public Balron()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Boss " + NameList.RandomName("balron");
			
			Body = Utility.RandomBool() ? 38 : 41;
            BaseSoundID = 357;

            SetStr(15986, 21185);
            SetDex(17700, 18255);
            SetInt(15100, 17250);

            SetHits(359200, 371100);
			SetStam(310000, 312000);
			SetMana(310000, 312000);

			SetAttackSpeed( 5.0 );
            SetDamage(1220, 2429);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Energy, 25);

            SetResistance(ResistanceType.Physical, 65, 80);
            SetResistance(ResistanceType.Fire, 60, 80);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.Anatomy, 25.1, 50.0);
            SetSkill(SkillName.EvalInt, 90.1, 100.0);
            SetSkill(SkillName.Magery, 95.5, 100.0);
            SetSkill(SkillName.Meditation, 25.1, 50.0);
            SetSkill(SkillName.MagicResist, 100.5, 150.0);
            SetSkill(SkillName.Tactics, 90.1, 100.0);
            SetSkill(SkillName.Wrestling, 90.1, 100.0);

            Fame = 30000;
            Karma = -30000;

            VirtualArmor = 90;

            PackItem(new Longsword());
        }

        public Balron(Serial serial)
            : base(serial)
        {
        }

        public override bool CanFly
        {
            get { return true; }
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
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
            this.AddLoot(LootPack.FilthyRich, 2);
            this.AddLoot(LootPack.Rich);
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
