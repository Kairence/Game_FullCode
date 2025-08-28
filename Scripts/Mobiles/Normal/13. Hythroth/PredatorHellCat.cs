using System;

namespace Server.Mobiles
{
    [CorpseName("a hell cat corpse")]
    [TypeAlias("Server.Mobiles.Preditorhellcat")]
    public class PredatorHellCat : BaseCreature
    {
        [Constructable]
        public PredatorHellCat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a predator hellcat";
            Body = 127;
            BaseSoundID = 0xBA;

            SetStr(2161, 3185);
            SetDex(1096, 2115);
            SetInt(2076, 3100);

            SetHits(2097, 2131);
			SetStam(1310, 2312);
			SetMana(1310, 2312);

			SetAttackSpeed( 5.0 );
            SetDamage(25, 175);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Fire, 25);

            SetResistance(ResistanceType.Physical, 25, 35);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Energy, 5, 15);

            SetSkill(SkillName.MagicResist, 75.1, 90.0);
            SetSkill(SkillName.Tactics, 50.1, 65.0);
            SetSkill(SkillName.Wrestling, 50.1, 65.0);
            SetSkill(SkillName.Necromancy, 20.0);
            SetSkill(SkillName.SpiritSpeak, 20.0);
            SetSkill(SkillName.Wrestling, 50.1, 65.0);
            SetSkill(SkillName.DetectHidden, 41.2);

            Fame = 6000;
            Karma = -6000;

            VirtualArmor = 6;

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 90.0;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public PredatorHellCat(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 10;
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
                return PackInstinct.Feline;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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
