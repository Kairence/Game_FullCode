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

            SetStr(3820, 4350);
            SetDex(2220, 3250);
            SetInt(3200, 3500);

            SetHits(4820, 5888);
			SetStam(3000, 5000);
            this.SetMana(666,999);

 			SetAttackSpeed( 5.0 );

			SetDamage(127, 270);

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

            Fame = 13500;
            Karma = -13500;

            VirtualArmor = 66;

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
