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

			this.SetAttackSpeed(3.0);  // 1.5초 -> 3.0초 (유저 평균 공속과 일치시켜 대응 가능하게 수정)
			this.SetDamage(12, 18);    // 방어 10인 유저에게 확정적으로 2~8의 데미지 전달.

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

            SetHits(20, 40);
            SetStam(40, 50);
            SetMana(10, 11);
			
			this.SetAttackSpeed(5.0);  // 일반 새보다 확연히 느린 속도 (여유로운 회피/포션 타이밍 제공)
			this.SetDamage(20, 30);    // 방어 10인 유저에게 10~20의 데미지 전달. (체력 1000 유저 기준 약 1~2% 타격)
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