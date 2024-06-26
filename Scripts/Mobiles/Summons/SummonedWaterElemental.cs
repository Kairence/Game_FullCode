using System;

namespace Server.Mobiles
{
    [CorpseName("a water elemental corpse")]
    public class SummonedWaterElemental : BaseCreature
    {
        [Constructable]
        public SummonedWaterElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a water elemental";
            this.Body = 16;
            this.BaseSoundID = 278;

            this.SetStr(500);
            this.SetDex(100);
            this.SetInt(500);

            this.SetHits(100);
			this.SetStam(1000);
			this.SetMana(100);

            this.SetDamage(1);

            this.SetDamageType(ResistanceType.Physical, 0);
            this.SetDamageType(ResistanceType.Cold, 100);

            this.SetResistance(ResistanceType.Physical, 50, 60);
            this.SetResistance(ResistanceType.Fire, 20, 30);
            this.SetResistance(ResistanceType.Cold, 70, 80);
            this.SetResistance(ResistanceType.Poison, 45, 55);
            this.SetResistance(ResistanceType.Energy, 40, 50);

            this.SetSkill(SkillName.Meditation, 90.0);
            this.SetSkill(SkillName.EvalInt, 80.0);
            this.SetSkill(SkillName.Magery, 80.0);
            this.SetSkill(SkillName.MagicResist, 75.0);

            this.VirtualArmor = 5;
            this.CanSwim = true;
        }

        public SummonedWaterElemental(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 117.5;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
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