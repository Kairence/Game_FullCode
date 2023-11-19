using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a mummy corpse")]
    public class Mummy : BaseCreature
    {
        [Constructable]
        public Mummy()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.4, 0.8)
        {
            Name = "a mummy";
            Body = 154;
            BaseSoundID = 471;

            SetStr(446, 470);
            SetDex(171, 190);
            SetInt(126, 140);

            SetHits(2118, 2120);
            SetStam(230, 250);
			SetMana(10, 20);

			SetAttackSpeed( 10.0 );

            SetDamage(20, 41);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Cold, 60);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 50, 60);
            SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.MagicResist, 105.1, 140.0);
            SetSkill(SkillName.Tactics, 105.1, 120.0);
            SetSkill(SkillName.Wrestling, 105.1, 120.0);

            Fame = 12000;
            Karma = -12000;

            VirtualArmor = 50;
        }

        public Mummy(Serial serial)
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
                return Poison.Lesser;
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
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Gems);
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
