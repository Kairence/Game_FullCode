using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class SkeletalMage : BaseCreature
    {
        [Constructable]
        public SkeletalMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skeletal mage";
            Body = 148;
            BaseSoundID = 451;

            SetStr(276, 400);
            SetDex(1056, 1075);
            SetInt(2586, 2610);

            SetHits(2460, 3000);
			SetStam(550, 600);
			SetMana(1000, 1500);

            SetDamage(33, 57);

			SetAttackSpeed( 60 );

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 65, 80);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.EvalInt, 130.1, 140.0);
            SetSkill(SkillName.Magery, 130.1, 140.0);
            SetSkill(SkillName.MagicResist, 145.1, 170.0);
            SetSkill(SkillName.Tactics, 135.1, 140.0);
            SetSkill(SkillName.Wrestling, 135.1, 145.0);

            Fame = 9500;
            Karma = -9500;

            VirtualArmor = 38;

        }

        public SkeletalMage(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune { get { return true; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override TribeType Tribe { get { return TribeType.Undead; } }

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
