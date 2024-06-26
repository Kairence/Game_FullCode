using System;

namespace Server.Mobiles
{
    [CorpseName("a bird corpse")]
    public class Bird : BaseCreature
    {
        [Constructable]
        public Bird()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            if (Utility.RandomBool())
            {
                this.Hue = 0x901;

                switch ( Utility.Random(3) )
                {
                    case 0:
                        this.Name = "a crow";
                        break;
                    case 2:
                        this.Name = "a raven";
                        break;
                    case 1:
                        this.Name = "a magpie";
                        break;
                }
            }
            else
            {
                this.Hue = Utility.RandomBirdHue();
                this.Name = NameList.RandomName("bird");
            }

            this.Body = 6;
            this.BaseSoundID = 0x1B;

            this.SetStr(10, 15);
            this.SetDex(25, 35);
            this.SetInt(10, 15);

            SetHits(150, 180);
            SetStam(40, 50);
            SetMana(10, 11);
			
			SetAttackSpeed(10.0);

            SetDamage(5, 9);

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

        public Bird(Serial serial)
            : base(serial)
        {
        }

        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override int Feathers
        {
            get
            {
                return 25;
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

    [CorpseName("a bird corpse")]
    public class TropicalBird : BaseCreature
    {
        [Constructable]
        public TropicalBird()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Hue = Utility.RandomBirdHue();
            this.Name = "a tropical bird";

            this.Body = 6;
            this.BaseSoundID = 0xBF;

            this.SetStr(10, 15);
            this.SetDex(25, 35);
            this.SetInt(10, 15);

            SetHits(150, 180);
            SetStam(40, 50);
            SetMana(10, 11);
			
			SetAttackSpeed(10.0);

            SetDamage(5, 9);

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

        public TropicalBird(Serial serial)
            : base(serial)
        {
        }

        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Feathers
        {
            get
            {
                return 25;
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