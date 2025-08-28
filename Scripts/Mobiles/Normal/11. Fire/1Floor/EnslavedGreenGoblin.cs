using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an goblin corpse")]
    public class EnslavedGreenGoblin : BaseCreature
    {
        [Constructable]
        public EnslavedGreenGoblin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Enslaved Green Goblin";
            Body = 334;
			Hue = 874;
            BaseSoundID = 0x600;

            SetStr(326, 326);
            SetDex(71, 71);
            SetInt(126, 126);

            SetHits(284, 384);
            SetStam(71, 71);
            SetMana(126, 126);

			SetAttackSpeed( 5.0 );

            SetDamage(15, 27);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 40);
            SetResistance(ResistanceType.Fire, 38, 39);
            SetResistance(ResistanceType.Cold, 31, 32);
            SetResistance(ResistanceType.Poison, 12, 12);
            SetResistance(ResistanceType.Energy, 10, 11);

            SetSkill(SkillName.MagicResist, 121.6, 122.9);
            SetSkill(SkillName.Tactics, 80.0, 81.2);
            SetSkill(SkillName.Anatomy, 82.0, 83.4);
            SetSkill(SkillName.Wrestling, 99.2, 99.4);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 28;
        }

        public EnslavedGreenGoblin(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound() { return 0x600; }
        public override int GetIdleSound() { return 0x600; }
        public override int GetAttackSound() { return 0x5FD; }
        public override int GetHurtSound() { return 0x5FF; }
        public override int GetDeathSound() { return 0x5FE; }

        public override bool CanRummageCorpses { get { return true; } }
        public override int TreasureMapLevel { get { return 1; } }
        public override int Meat { get { return 1; } }
        //public override OppositionGroup OppositionGroup { get { return OppositionGroup.SavagesAndOrcs; } }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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