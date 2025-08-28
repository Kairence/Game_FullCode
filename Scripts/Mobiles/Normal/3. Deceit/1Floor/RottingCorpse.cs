using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class RottingCorpse : BaseCreature
    {
        [Constructable]
        public RottingCorpse()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a rotting corpse";
            Body = 155;
            BaseSoundID = 471;

            SetStr(6010, 6500);
            SetDex(250, 300);
            SetInt(101, 200);

			SetAttackSpeed( 10.0 );

            SetHits(8059, 9122);
            SetStam(100, 150);
            SetMana(150, 175);

            SetDamage(220, 1156);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Cold, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetResistance(ResistanceType.Physical, 65, 85);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 50, 70);
            SetResistance(ResistanceType.Poison, 110, 125);
            SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.Poisoning, 320.0);
            SetSkill(SkillName.MagicResist, 350.0);
            SetSkill(SkillName.Tactics, 1700.0);
            SetSkill(SkillName.Wrestling, 190.1, 200.0);

            Fame = 18500;
            Karma = -18500;

            VirtualArmor = 100;
        }

        public RottingCorpse(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
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
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 2);
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
