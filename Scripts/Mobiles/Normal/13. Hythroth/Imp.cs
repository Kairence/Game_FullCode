using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an imp corpse")]
    public class Imp : BaseCreature
    {
        [Constructable]
        public Imp()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an imp";
            Body = 74;
            BaseSoundID = 422;

            SetStr(91, 115);
            SetDex(61, 80);
            SetInt(86, 105);

            SetHits(855, 1070);
			SetStam(310, 312);
			SetMana(310, 312);

			SetAttackSpeed( 5.0 );
            SetDamage(50, 104);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Fire, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetResistance(ResistanceType.Physical, 25, 35);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.EvalInt, 20.1, 30.0);
            SetSkill(SkillName.Magery, 90.1, 100.0);
            SetSkill(SkillName.MagicResist, 30.1, 50.0);
            SetSkill(SkillName.Tactics, 42.1, 50.0);
            SetSkill(SkillName.Wrestling, 40.1, 44.0);
            SetSkill(SkillName.Necromancy, 20);
            SetSkill(SkillName.SpiritSpeak, 20);

            Fame = 5000;
            Karma = -5000;

            VirtualArmor = 30;

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 83.1;
        }

        public Imp(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 6;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Daemon;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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