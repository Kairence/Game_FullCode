using System;

namespace Server.Mobiles
{
    [CorpseName("an eagle corpse")]
    public class Eagle : BaseCreature
    {
        [Constructable]
        public Eagle()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "an eagle";
            this.Body = 5;
            this.BaseSoundID = 0x2EE;

            this.SetStr(1, 10);      // 최종 Str 547~556
			this.SetDex(35, 55);     
			this.SetInt(1, 10);      // 최종 Int 56~65

			this.SetHits(80, 180);   // 최종 Hits 1,300~1,400
			this.SetStam(35, 55);

			SetAttackSpeed(2.2);
			SetDamage(12, 18);

			this.SetDamageType(ResistanceType.Physical, 100);
			this.SetResistance(ResistanceType.Physical, 10, 15);

			this.SetSkill(SkillName.Wrestling, 18.5, 28.5);
			this.SetSkill(SkillName.Tactics, 18.5, 28.5);

			// 테이밍 설정
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 15.1;

			this.Fame = 600;
			this.Karma = 0;
		}

        public Eagle(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override int Feathers
        {
            get
            {
                return 36;
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