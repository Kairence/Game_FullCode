using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an elder dragon corpse")]
    public class GreaterDragon : BaseCreature
    {
        [Constructable]
        public GreaterDragon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.3, 0.5)
        {
            Name = "an elder dragon";
            Body = 172; //구 Rikktor
            BaseSoundID = 362;

            SetStr(8025, 8425);
            SetDex(8100, 8948);
            SetInt(8475, 9075);

            SetHits(35000, 36000);
			SetStam( 10000, 12000 );
			SetMana( 10000, 11500 );

			SetAttackSpeed( 5.0 );            
            SetDamage(240, 633);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 60, 85);
            SetResistance(ResistanceType.Fire, 65, 90);
            SetResistance(ResistanceType.Cold, 40, 55);
            SetResistance(ResistanceType.Poison, 40, 60);
            SetResistance(ResistanceType.Energy, 50, 75);

            SetSkill(SkillName.Meditation, 0);
            SetSkill(SkillName.EvalInt, 210.0, 340.0);
            SetSkill(SkillName.Magery, 210.0, 340.0);
            SetSkill(SkillName.Poisoning, 0);
            SetSkill(SkillName.Anatomy, 0);
            SetSkill(SkillName.MagicResist, 210.0, 340.0);
            SetSkill(SkillName.Tactics, 210.0, 340.0);
            SetSkill(SkillName.Wrestling, 215.0, 345.0);

            Fame = 29000;
            Karma = -29000;

            VirtualArmor = 26;

            Tamable = true;
            ControlSlots = 4;
            MinTameSkill = 194.7;

            //SetWeaponAbility(WeaponAbility.BleedAttack);
            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public GreaterDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
        }
        public override bool ReacquireOnMovement
        {
            get
            {
                return !Controlled;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return !Controlled;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int Hides
        {
            get
            {
                return 30;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override int Scales
        {
            get
            {
                return 7;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return (Body == 12 ? ScaleType.Yellow : ScaleType.Red);
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
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
            AddLoot(LootPack.FilthyRich, 4);
            AddLoot(LootPack.Gems, 8);
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
