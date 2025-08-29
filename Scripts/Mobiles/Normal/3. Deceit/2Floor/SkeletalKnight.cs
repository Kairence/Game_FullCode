using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class SkeletalKnight : BaseCreature
    {
        [Constructable]
        public SkeletalKnight()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skeletal knight";
            Body = 147;
            BaseSoundID = 451;

            SetStr(3496, 3950);
            SetDex(3576, 3595);
            SetInt(1236, 1260);

            SetHits(4180, 5350);
            this.SetMana(100, 150);
			SetStam(2500, 3450);

			SetAttackSpeed( 2.5 );

            SetDamage(20, 181);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Cold, 60);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 105.1, 110.0);
            SetSkill(SkillName.Tactics, 185.1, 200.0);
            SetSkill(SkillName.Wrestling, 185.1, 195.0);

            Fame = 13000;
            Karma = -13000;

            VirtualArmor = 30;
        }

        public SkeletalKnight(Serial serial)
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
            AddLoot(LootPack.Average);
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
