using System;

namespace Server.Mobiles
{
    [CorpseName("a shadow wyrm corpse")]
    public class ShadowWyrm : BaseCreature
    {
        [Constructable]
        public ShadowWyrm()
            : base(AIType.AI_NecroMage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a shadow wyrm";
            Body = 106;
            BaseSoundID = 362;

            SetStr(6898, 7030);
            SetDex(6800, 7200);
            SetInt(7488, 7620);

            SetHits(17558, 18599);
			SetStam( 10000, 12000 );
			SetMana( 10000, 11500 );

			SetAttackSpeed( 1.0 );   
            SetDamage(40, 120);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Cold, 25);

            SetResistance(ResistanceType.Physical, 65, 75);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 45, 55);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 50, 60);

            SetSkill(SkillName.EvalInt, 280.1, 300.0);
            SetSkill(SkillName.Magery, 280.1, 300.0);
            SetSkill(SkillName.Meditation, 252.5, 275.0);
            SetSkill(SkillName.MagicResist, 300.3, 330.0);
            SetSkill(SkillName.Tactics, 297.6, 300.0);
            SetSkill(SkillName.Wrestling, 297.6, 300.0);
            SetSkill(SkillName.DetectHidden, 290.0, 300.0);
            SetSkill(SkillName.Necromancy, 280.0, 290.0);
            SetSkill(SkillName.SpiritSpeak, 200.0, 305.0);

            Fame = 25000;
            Karma = -25000;

            VirtualArmor = 35;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 176.0;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public ShadowWyrm(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool ReacquireOnMovement { get { return !Controlled; } }
        public override bool AutoDispel { get { return !Controlled; } }
        public override Poison PoisonImmune { get { return Poison.Deadly; } }
        public override Poison HitPoison { get { return Poison.Deadly; } }
        public override int TreasureMapLevel { get { return 5; } }
        public override int Meat { get { return 19; } }
        public override int Hides { get { return 20; } }
        public override int Scales { get { return 10; } }
        public override ScaleType ScaleType { get { return ScaleType.Black; } }
        public override HideType HideType { get { return HideType.Barbed; } }
        public override bool CanFly { get { return true; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 5);
        }

        public override int GetIdleSound()
        {
            return 0x2D5;
        }

        public override int GetHurtSound()
        {
            return 0x2D1;
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
