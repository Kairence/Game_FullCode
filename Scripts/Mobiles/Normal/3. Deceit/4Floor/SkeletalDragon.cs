using System;

namespace Server.Mobiles
{
    [CorpseName("a skeletal dragon corpse")]
    public class SkeletalDragon : BaseCreature
    {
        [Constructable]
        public SkeletalDragon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Boss a skeletal dragon";
            Body = 104;
            BaseSoundID = 0x488;

            SetStr(2898, 3030);
            SetDex(1068, 1200);
            SetInt(6488, 6620);

            SetHits(155800, 165990);
			SetStam(900, 999);
			SetMana(1000, 1500);

            SetDamage(322, 450);

			Boss = true;
			
			SetAttackSpeed( 5.0 );
			
            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Fire, 25);

            SetResistance(ResistanceType.Physical, 75, 80);
            SetResistance(ResistanceType.Fire, 40, 60);
            SetResistance(ResistanceType.Cold, 40, 60);
            SetResistance(ResistanceType.Poison, 70, 80);
            SetResistance(ResistanceType.Energy, 40, 60);

            SetSkill(SkillName.EvalInt, 340.1, 360.0);
            SetSkill(SkillName.Magery, 340.1, 360.0);
            SetSkill(SkillName.MagicResist, 360.3, 390.0);
            SetSkill(SkillName.Tactics, 357.6, 360.0);
            SetSkill(SkillName.Wrestling, 357.6, 360.0);
            SetSkill(SkillName.Necromancy, 340.1, 360.0);
            SetSkill(SkillName.SpiritSpeak, 340.1, 360.0);

            Fame = 31000;
            Karma = -31000;

            VirtualArmor = 130;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public SkeletalDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool AutoDispel { get { return !Controlled; } }
        public override bool BleedImmune { get { return true; } }
        public override bool ReacquireOnMovement { get { return !Controlled; } }
        public override double BonusPetDamageScalar { get { return (Core.SE) ? 3.0 : 1.0; } }
        public override int Hides { get { return 20; } }
        public override int Meat { get { return 19; } } // where's it hiding these? :)
        public override HideType HideType { get { return HideType.Barbed; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
        public override Poison PoisonImmune { get { return Poison.Lethal; } }
        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 4);
            AddLoot(LootPack.Gems, 5);
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
