using System;

namespace Server.Mobiles
{
    [CorpseName("a grey wolf corpse")]
    [TypeAlias("Server.Mobiles.Greywolf")]
    public class GreyWolf : BaseCreature
    {
        [Constructable]
        public GreyWolf()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a grey wolf";
            this.Body = Utility.RandomList(25, 27);
            this.BaseSoundID = 0xE5;

            this.SetStr(256, 280);
            this.SetDex(366, 385);
            this.SetInt(26, 50);

            this.SetHits(434, 488);
            SetStam(212, 215);
            SetMana(10, 11);
			
			SetAttackSpeed(3.5);
            this.SetDamage(8, 14);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 20, 25);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Cold, 10, 15);
            this.SetResistance(ResistanceType.Poison, 5, 10);

            this.SetSkill(SkillName.MagicResist, 45.1, 50.0);
            this.SetSkill(SkillName.Tactics, 75.1, 90.0);
            this.SetSkill(SkillName.Wrestling, 75.1, 90.0);

            this.Fame = 1150;
            this.Karma = 0;

            this.VirtualArmor = 1;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 41.1;
        }

        public GreyWolf(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 6;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Canine;
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