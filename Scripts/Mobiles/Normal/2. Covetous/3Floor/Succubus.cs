using System;
using System.Linq;
using System.Collections;

using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
    [CorpseName("a succubus corpse")]
    public class Succubus : BaseCreature
    {
        [Constructable]
        public Succubus()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a succubus";
            Body = 149;
            BaseSoundID = 0x4B0;

            SetStr(320, 350);
            SetDex(220, 250);
            SetInt(320, 350);

            SetHits(4820, 4888);
			SetStam(300, 500);
            this.SetMana(666,999);

 			SetAttackSpeed( 5.0 );

			SetDamage(27, 70);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Poison, 25);

            SetResistance(ResistanceType.Physical, 40, 55);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.MagicResist, 150.5, 155.0);
            SetSkill(SkillName.Tactics, 130.1, 135.0);
            SetSkill(SkillName.Wrestling, 120.1, 125.0);

            Fame = 13000;
            Karma = -13000;

            VirtualArmor = 18;

            SetSpecialAbility(SpecialAbility.LifeDrain);
        }

        public Succubus(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.MedScrolls, 2);
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
