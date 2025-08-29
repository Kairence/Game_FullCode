using System;
using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
    [CorpseName("a reapers corpse")]
    public class Reaper : BaseCreature
    {
        [Constructable]
        public Reaper()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "Boss a reaper";
            this.Body = 47;
            this.BaseSoundID = 442;

            this.SetStr(1406, 2025);
            this.SetDex(2060, 2250);
            this.SetInt(5010, 5500);

            this.SetHits(25000, 30000);
            this.SetMana(10000, 12500);
			this.SetStam(12000, 15000);

			SetAttackSpeed( 60.0 );
			
            this.SetDamage(690, 2080);

            this.SetDamageType(ResistanceType.Physical, 80);
            this.SetDamageType(ResistanceType.Poison, 20);

            this.SetResistance(ResistanceType.Physical, 35, 45);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 30, 40);

            this.SetSkill(SkillName.EvalInt, 116.1, 117.0);
            this.SetSkill(SkillName.Magery, 116.1, 117.0);
            this.SetSkill(SkillName.MagicResist, 116.1, 117.0);
            this.SetSkill(SkillName.Tactics, 116.1, 117.0);
            this.SetSkill(SkillName.Wrestling, 116.1, 117.0);

            this.Fame = 11000;
            this.Karma = -11000;

            this.VirtualArmor = 40;
        }

        public Reaper(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override bool DisallowAllMoves
        {
            get
            {
                return true;
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