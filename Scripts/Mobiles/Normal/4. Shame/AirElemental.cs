using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an air elemental corpse")]
    public class AirElemental : BaseCreature
    {
        [Constructable]
        public AirElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an air elemental";
            Body = 13;
            Hue = 0x4001;
            BaseSoundID = 655;

            this.SetStr(136, 145);
            this.SetDex(136, 145);
            this.SetInt(321, 345);

            this.SetHits(1307, 1413);
			SetStam(300, 400);
			SetMana(100);

			SetAttackSpeed( 2.5 );
            SetDamage(1, 11);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Cold, 40);
            SetDamageType(ResistanceType.Energy, 40);

            SetResistance(ResistanceType.Physical, 65, 75);
            SetResistance(ResistanceType.Fire, 15, 25);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 80, 90);
            SetResistance(ResistanceType.Energy, 105, 125);

            SetSkill(SkillName.EvalInt, 90.1, 95.0);
            SetSkill(SkillName.Magery, 90.1, 95.0);
            SetSkill(SkillName.MagicResist, 190.1, 195.0);

            Fame = 10000;
            Karma = -10000;

            this.VirtualArmor = 100;

            //this.PackItem(new SulfurousAsh(4));

			/*
			switch (Utility.Random(24))
            {
                case 0: PackItem(new PainSpikeScroll()); break;
                case 1: PackItem(new PoisonStrikeScroll()); break;
                case 2: PackItem(new StrangleScroll()); break;
                case 3: PackItem(new VengefulSpiritScroll()); break;
			}
			*/

            ControlSlots = 2;
        }

        public AirElemental(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 117.5;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.MedScrolls);
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