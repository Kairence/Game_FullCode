using System;

namespace Server.Mobiles
{
    [CorpseName("a chicken corpse")]
    public class Chicken : BaseCreature
    {
        [Constructable]
        public Chicken()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a chicken";
            Body = 0xD0;
            BaseSoundID = 0x6E;

            this.SetStr(200, 250);
            this.SetDex(25, 35);
            this.SetInt(10, 15);

            SetHits(150, 180);
            SetStam(40, 50);
            SetMana(10, 11);
			
			SetAttackSpeed(10.0);

            SetDamage(1, 4);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetSkill(SkillName.Wrestling, 4.2, 6.4);
            this.SetSkill(SkillName.Tactics, 4.0, 6.0);
            this.SetSkill(SkillName.MagicResist, 4.0, 5.0);

            this.Fame = 150;
            this.Karma = 0;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = -6.9;
        }

        public Chicken(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override int Feathers
        {
            get
            {
                return 25;
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