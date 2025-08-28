using System;

namespace Server.Mobiles
{
    [CorpseName("a slimey corpse")]
    public class Slime : BaseCreature
    {
        [Constructable]
        public Slime()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a slime";
            Body = 51;
            BaseSoundID = 456;

            Hue = Utility.RandomSlimeHue();

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

        public Slime(Serial serial)
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
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish | FoodType.FruitsAndVegies | FoodType.GrainsAndHay | FoodType.Eggs;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Poor);
            AddLoot(LootPack.Gems);
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            if (!base.CheckMovement(d, out newZ))
                return false;

            if (Region.IsPartOf("Underworld") && newZ > Location.Z)
                return false;

            return true;
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
