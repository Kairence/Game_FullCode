using System;

namespace Server.Mobiles
{
    [CorpseName("a pig corpse")]
    public class Pig : BaseCreature
    {
        [Constructable]
        public Pig()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a pig";
            this.Body = 0xCB;
            this.BaseSoundID = 0xC4;

            this.SetStr(20,25);
            this.SetDex(20,25);
            this.SetInt(5,10);

            this.SetHits(12,20);
            SetStam(1, 5);
            SetMana(1, 5);

			SetAttackSpeed(20.0);

            this.SetDamage(2, 4);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 10, 15);

            this.SetSkill(SkillName.MagicResist, 5.0);
            this.SetSkill(SkillName.Tactics, 5.0);
            this.SetSkill(SkillName.Wrestling, 5.0);

            this.Fame = 150;
            this.Karma = 0;

            this.VirtualArmor = 12;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 11.1;
        }

        public Pig(Serial serial)
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
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
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