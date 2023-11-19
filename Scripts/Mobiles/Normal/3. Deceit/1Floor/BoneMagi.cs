using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class BoneMagi : BaseCreature
    {
        [Constructable]
        public BoneMagi()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a bone mage";
            Body = 148;
            BaseSoundID = 451;

            SetStr(270, 285);
            SetDex(225, 235);
            SetInt(495, 500);

            SetHits(1150, 1160);
			SetStam(50, 60);
			SetMana(20, 30);

            SetDamage(5, 9);

			SetAttackSpeed( 60 );

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 35, 40);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 0, 10);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.EvalInt, 60.1, 65.0);
            SetSkill(SkillName.Magery, 60.1, 65.0);
            SetSkill(SkillName.MagicResist, 65.1, 67.5);
            SetSkill(SkillName.Tactics, 60.1, 65.0);
            SetSkill(SkillName.Wrestling, 50.1, 64.0);

            Fame = 9000;
            Karma = -9000;

            VirtualArmor = 1;
        }

        public BoneMagi(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune { get { return true; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Potions);
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
