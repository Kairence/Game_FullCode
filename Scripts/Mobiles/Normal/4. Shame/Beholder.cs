using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a beholder corpse")]
    public class Beholder : BaseCreature
    {
        [Constructable]
        public Beholder()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "Boss a beholder";
            this.Body = 53;
            this.BaseSoundID = 377;

			Boss = true;
				
            this.SetStr(17600, 20000);
            this.SetDex(15010, 17050);
            this.SetInt(18006, 21000);

            this.SetHits(700060, 781000);
			SetStam(750000, 780000);
			SetMana(750000, 780000);

            this.SetDamage(530, 1300);

			SetAttackSpeed( 2.5 );
			
            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Energy, 50);

            this.SetResistance(ResistanceType.Physical, 75, 85);
            this.SetResistance(ResistanceType.Fire, 70, 80);
            this.SetResistance(ResistanceType.Cold, 50, 60);
            this.SetResistance(ResistanceType.Poison, 50, 60);
            this.SetResistance(ResistanceType.Energy, 50, 60);

            SetSkill(SkillName.Magery, 370.1, 375.0);
            this.SetSkill(SkillName.MagicResist, 370.0, 375);
            this.SetSkill(SkillName.Tactics, 370.0, 375);
            this.SetSkill(SkillName.Wrestling, 370.0, 375);

            this.Fame = 31000;
            this.Karma = -31000;

            this.VirtualArmor = 35;

        }

        public Beholder(Serial serial)
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
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
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