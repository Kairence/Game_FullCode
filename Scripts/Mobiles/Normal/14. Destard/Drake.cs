using System;
using Server.Regions;

namespace Server.Mobiles
{
    [CorpseName("a drake corpse")]
    public class Drake : BaseCreature
    {
        [Constructable]
        public Drake()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an brown drake";
            Body = 60; //Utility.RandomList(60, 61);
            BaseSoundID = 362;

            SetStr(4010, 4030);
            SetDex(3133, 3152);
            SetInt(3101, 3140);

            SetHits(4410, 4580);
			SetStam( 1000, 2000 );
			SetMana( 1000, 1500 );

			SetAttackSpeed( 5.0 );
            SetDamage(115, 270);

            SetDamageType(ResistanceType.Physical, 80);
            SetDamageType(ResistanceType.Fire, 20);

            SetResistance(ResistanceType.Physical, 45, 50);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 40, 50);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 120.1, 135.0);
            SetSkill(SkillName.Tactics, 120.1, 135.0);
            SetSkill(SkillName.Wrestling, 120.1, 135.0);

            Fame = 13000;
            Karma = -13000;

            VirtualArmor = 26;
            ControlSlots = 2;
            MinTameSkill = 104.3;

            PackReg(3);

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Drake(Serial serial)
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
        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override int Meat
        {
            get
            {
                return 10;
            }
        }
        public override int DragonBlood
        {
            get
            {
                return 8;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        public override int Scales
        {
            get
            {
                return 2;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.Yellow;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish;
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
            AddLoot(LootPack.Rich);
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
