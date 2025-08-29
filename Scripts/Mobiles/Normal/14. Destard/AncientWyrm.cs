using System;

namespace Server.Mobiles
{
    [CorpseName("a dragon corpse")]
    public class AncientWyrm : BaseCreature
    {
        [Constructable]
        public AncientWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Boss an ancient wyrm";
            Body = 46;
            BaseSoundID = 362;

			Boss = true;

            SetStr(16096, 17185);
            SetDex(16000, 17500);
            SetInt(16860, 17750);

            SetHits(365800, 371100);
			SetStam( 100000, 120000 );
			SetMana( 100000, 115000 );

			SetAttackSpeed( 10.0 );  
            SetDamage(2200, 5000);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Fire, 25);

            SetResistance(ResistanceType.Physical, 65, 75);
            SetResistance(ResistanceType.Fire, 80, 90);
            SetResistance(ResistanceType.Cold, 70, 80);
            SetResistance(ResistanceType.Poison, 60, 70);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.EvalInt, 380.1, 400.0);
            SetSkill(SkillName.Magery, 380.1, 400.0);
            SetSkill(SkillName.Meditation, 352.5, 375.0);
            SetSkill(SkillName.MagicResist, 300.5, 350.0);
            SetSkill(SkillName.Tactics, 397.6, 400.0);
            SetSkill(SkillName.Wrestling, 397.6, 400.0);

            Fame = 30000;
            Karma = -30000;

            VirtualArmor = 33;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public AncientWyrm(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override int Hides
        {
            get
            {
                return 40;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int Scales
        {
            get
            {
                return 12;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return (ScaleType)Utility.Random(4);
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Utility.RandomBool() ? Poison.Lesser : Poison.Regular;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
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
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 5);
        }

        public override int GetIdleSound()
        {
            return 0x2D3;
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
