using System;

namespace Server.Mobiles
{
    [CorpseName("a bull corpse")]
    public class Bull : BaseCreature
    {
        [Constructable]
        public Bull()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a bull";
            this.Body = Utility.RandomList(0xE8, 0xE9);
            this.BaseSoundID = 0x64;

            if (0.5 >= Utility.RandomDouble())
                this.Hue = 0x901;

            this.SetStr(277, 444);
            this.SetDex(256, 275);
            this.SetInt(147, 175);

            SetHits(1333, 1888);
            SetStam(240, 260);
            SetMana(40, 50);
			
			SetAttackSpeed(5.0);
			
            this.SetDamage(13, 25);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Cold, 10, 15);

            this.SetSkill(SkillName.Wrestling, 66.2, 86.4);
            this.SetSkill(SkillName.Tactics, 64.0, 66.0);
            this.SetSkill(SkillName.MagicResist, 44.0, 45.0);			
			
            this.Fame = 1000;
            this.Karma = 0;

            this.VirtualArmor = 2;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 71.1;
        }

        public Bull(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 10;
            }
        }
        public override int Hides
        {
            get
            {
                return 15;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Bull;
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