using System;

namespace Server.Mobiles
{
    [CorpseName("a snake corpse")]
    public class Snake : BaseCreature
    {
        [Constructable]
        public Snake()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a snake";
            this.Body = 52;
            this.Hue = Utility.RandomSnakeHue();
            this.BaseSoundID = 0xDB;

            SetStr(222, 434);
            SetDex(116, 121);
            SetInt(16, 20);

            SetHits(650, 690);
            SetStam(10, 15);
            SetMana(10, 15);

			SetAttackSpeed(20.0);
            SetDamage(5, 12);

            SetDamageType(ResistanceType.Poison, 100);

            Fame = 1000;
            Karma = -1000;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 23.1;
        }

        public Snake(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override bool DeathAdderCharmable
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Eggs;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }
    }
}