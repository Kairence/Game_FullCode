using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a sea serpents corpse")]
    [TypeAlias("Server.Mobiles.Seaserpant")]
    public class SeaSerpent : BaseCreature
    {
        [Constructable]
        public SeaSerpent()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a sea serpent";
            Body = 150;
            BaseSoundID = 447;

            Hue = Utility.Random(0x530, 9);

            SetStr(351, 825);
            SetDex(487, 535);
            SetInt(287, 355);

            SetHits(11600, 13000);
            SetStam(100, 150);
            SetMana(50, 100);

			SetAttackSpeed(17.5);
            SetDamage(66, 100);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 25, 35);
            SetResistance(ResistanceType.Fire, 40, 45);
            SetResistance(ResistanceType.Cold, 30, 35);
            SetResistance(ResistanceType.Poison, 25, 30);
            SetResistance(ResistanceType.Energy, 10, 15);

            SetSkill(SkillName.MagicResist, 30.1, 35.0);
            SetSkill(SkillName.Tactics, 30.1, 40.0);
            SetSkill(SkillName.Wrestling, 30.1, 40.0);

            Fame = 6000;
            Karma = -6000;

            VirtualArmor = 3;
            CanSwim = true;
            CantWalk = true;

            if (Utility.RandomBool())
                PackItem(new SulfurousAsh(10));
            else
                PackItem(new BlackPearl(10));

            PackItem(new RawFishSteak());

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public SeaSerpent(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return Utility.RandomList(1, 2); } }
        public override int Hides { get { return 10; } }
        public override HideType HideType { get { return HideType.Spined; } }

        public override void GenerateLoot()
        {
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
