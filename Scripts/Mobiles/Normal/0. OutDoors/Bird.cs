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

			// [역산] 명성 150 보너스(Str+511, Hits+450, Stam+52, Skill+0.3) 반영
			// 최종 Str 520~530 목표
			this.SetStr(9, 19);
			this.SetDex(10, 20); // 최종 Dex 약 162~172 도달
			this.SetInt(10, 20);

			// 최종 Hits 460~480 목표
			this.SetHits(10, 30);
			this.SetStam(5, 10);  // 최종 Stam 약 57~62 도달
			this.SetMana(0);

			// [컨셉] 부리로 빠르게 쪼는 공격 (공속 2.5)
			SetAttackSpeed(2.5);
			SetDamage(1, 2);

			// 최종 Skill 1.0~2.0 목표
			this.SetSkill(SkillName.Wrestling, 0.7, 1.7);
			this.SetSkill(SkillName.Tactics, 0.7, 1.7);
			this.SetSkill(SkillName.MagicResist, 0.5, 1.0);

			this.Fame = 150;
			this.Karma = 0;
			this.VirtualArmor = 0; // 방어구 없음

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = -18.9;

            this.SetDamageType(ResistanceType.Physical, 100);
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