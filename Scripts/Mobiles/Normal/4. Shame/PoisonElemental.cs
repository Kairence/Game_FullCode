using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a poison elementals corpse")]
    public class PoisonElemental : BaseCreature
    {
        [Constructable]
        public PoisonElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a poison elemental";
            this.Body = 162;
            this.BaseSoundID = 263;

            this.SetStr(500, 510);
            this.SetDex(710, 999);
            this.SetInt(661, 675);

            this.SetHits(25600, 26099);
			SetStam(240, 245);
			SetMana(100, 150);
			
			SetAttackSpeed( 7.5 );

            this.SetDamage(5, 35);

            this.SetDamageType(ResistanceType.Physical, 10);
            this.SetDamageType(ResistanceType.Poison, 90);

            this.SetResistance(ResistanceType.Physical, 60, 70);
            this.SetResistance(ResistanceType.Fire, 40, 60);
            this.SetResistance(ResistanceType.Cold, 20, 30);
            this.SetResistance(ResistanceType.Poison, 100);
            this.SetResistance(ResistanceType.Energy, 60, 80);

            SetSkill(SkillName.EvalInt, 260.1, 265.0);
            SetSkill(SkillName.Magery, 260.1, 265.0);
            SetSkill(SkillName.MagicResist, 260.1, 265.0);
            SetSkill(SkillName.Wrestling, 325.1, 330.0);
			SetSkill(SkillName.Poisoning, 360.1, 365.0);
			
            this.Fame = 22000;
            this.Karma = -22000;

            this.VirtualArmor = 300;

            //this.PackItem(new LesserPoisonPotion());
        }

		public int poisonline = 0;
		
        public PoisonElemental(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }

        public override double HitPoisonChance
        {
            get
            {
                return 0.75;
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