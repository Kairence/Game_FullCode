using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a deep sea serpents corpse")]
    public class DeepSeaSerpent : BaseCreature
    {
        [Constructable]
        public DeepSeaSerpent()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a deep sea serpent";
            Body = 150;
            BaseSoundID = 447;

            Hue = Utility.Random(0x8A0, 5);

            SetStr(4251, 5425);
            SetDex(3870, 4350);
            SetInt(7870, 8550);

            SetHits(4700, 6000);
            SetStam(1000, 1200);
            SetMana(1000, 1200);
			
			SetAttackSpeed(10.0);
			
            SetDamage(426, 490);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 30, 40);
            SetResistance(ResistanceType.Fire, 70, 80);
            SetResistance(ResistanceType.Cold, 40, 50);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 15, 20);

            SetSkill(SkillName.MagicResist, 160.1, 180.0);
            SetSkill(SkillName.Tactics, 160.1, 180.0);
            SetSkill(SkillName.Wrestling, 160.1, 180.0);

            Fame = 14000;
            Karma = -14000;

            VirtualArmor = 10;
            CanSwim = true;
            CantWalk = true;

            if (Utility.RandomBool())
                PackItem(new SulfurousAsh(20));
            else
                PackItem(new BlackPearl(20));

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public DeepSeaSerpent(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return 2; } }
        public override int Meat { get { return 10; } }
		public override int Hides { get { return 10; } }
        public override HideType HideType { get { return HideType.Horned; } }

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
