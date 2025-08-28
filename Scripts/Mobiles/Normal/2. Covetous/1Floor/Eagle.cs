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

            this.SetStr(300, 407);
            this.SetDex(106, 120);
            this.SetInt(58, 70);

            this.SetHits(520, 725);
            this.SetMana(40, 60);
			this.SetStam(100, 120);

			SetAttackSpeed( 1.0 );

            this.SetDamage(3, 10);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.Fame = 2000;
            this.Karma = -2000;

            this.Tamable = true;
            this.ControlSlots = 3;
            this.MinTameSkill = 17.1;
            VirtualArmor = 6;
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