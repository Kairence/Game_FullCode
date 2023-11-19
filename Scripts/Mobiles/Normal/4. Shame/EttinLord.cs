using System;

namespace Server.Mobiles
{
    [CorpseName("an ettins corpse")]
    public class EttinLord : BaseCreature
    {
        [Constructable]
        public EttinLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "Boss an ettin lord";
            this.Body = 261;
            this.BaseSoundID = 367;

            this.SetStr(606, 715);
            this.SetDex(446, 555);
            this.SetInt(111, 115);

            //this.SetHits(132, 139);
			SetHits(14932, 14979);
			SetMana(110, 115);
			SetStam(1360, 1500);

            //this.SetDamage(7, 13);
			SetDamage(66, 77);
			SetAttackSpeed( 4.5 );

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 45, 60);
            this.SetResistance(ResistanceType.Fire, 65, 75);
            this.SetResistance(ResistanceType.Cold, 80, 90);
            this.SetResistance(ResistanceType.Poison, 45, 55);
            this.SetResistance(ResistanceType.Energy, 45, 55);			
			
            this.SetSkill(SkillName.MagicResist, 210.1, 212.5);
            this.SetSkill(SkillName.Tactics, 210.1, 212.5);
            this.SetSkill(SkillName.Wrestling, 210.1, 212.5);

            this.Fame = 19000;
            this.Karma = -19000;

			Boss = true;
			
            this.VirtualArmor = 144;
        }

        public EttinLord(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override void GenerateLoot()
        {

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