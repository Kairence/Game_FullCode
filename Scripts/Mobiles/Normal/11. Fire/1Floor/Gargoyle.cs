using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class Gargoyle : BaseCreature
    {
        [Constructable]
        public Gargoyle()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a gargoyle";
            this.Body = 4;
            this.BaseSoundID = 372;

            this.SetStr(566, 775);
            this.SetDex(566, 775);
            this.SetInt(1031, 1145);

            this.SetHits(688, 805);
			SetStam(100, 200);
			SetMana(240, 380);

			SetAttackSpeed( 5.0 );

            this.SetDamage(47, 84);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 30, 35);
            this.SetResistance(ResistanceType.Fire, 25, 35);
            this.SetResistance(ResistanceType.Cold, 5, 10);
            this.SetResistance(ResistanceType.Poison, 15, 25);

            this.SetSkill(SkillName.EvalInt, 80.1, 108.0);
            this.SetSkill(SkillName.Magery, 80.1, 108.0);
            this.SetSkill(SkillName.MagicResist, 80.1, 108.0);
            this.SetSkill(SkillName.Tactics, 80.1, 107.0);
            this.SetSkill(SkillName.Wrestling, 84.1, 108.0);

            this.Fame = 4000;
            this.Karma = -4000;

            this.VirtualArmor = 22;
        }

        public Gargoyle(Serial serial)
            : base(serial)
        {
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
                return 1;
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
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.MedScrolls);
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 4));
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