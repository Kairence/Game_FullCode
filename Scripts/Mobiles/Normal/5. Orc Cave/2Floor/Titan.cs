using System;

namespace Server.Mobiles
{
    [CorpseName("a titans corpse")]
    public class Titan : BaseCreature
    {
        [Constructable]
        public Titan()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a titan";
            this.Body = 76;
            this.BaseSoundID = 609;

            /* Titan - Fame 20,000 / Karma -20,000 */
			/* [HP Calculation]
			   - Target HP: ~120,000
			   - Fame Bonus (20,000): ~55,833
			   - SetHits Required: 64,167 (Target - Bonus)
			*/
			this.SetStr(1000, 1200);     
			this.SetDex(150, 250);       
			this.SetInt(600, 800);       

			// [Hits] 최종 약 115,000 ~ 125,000 타겟
			this.SetHits(59167, 69167); 
			this.SetStam(150, 250);      
			this.SetMana(600, 800);      

			this.SetAttackSpeed(3.0);    // 거대한 만큼 느리지만 치명적임
			this.SetDamage(45, 65);      

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 40, 55);
			this.SetResistance(ResistanceType.Cold, 40, 55);
			this.SetResistance(ResistanceType.Poison, 40, 55);
			this.SetResistance(ResistanceType.Energy, 65, 75); // Max 75%

			this.SetSkill(SkillName.Wrestling, 115.0, 130.0);
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.Magery, 100.0, 115.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);

			this.VirtualArmor = 20;      
			this.Tamable = false;

			this.Fame = 20000;           
			this.Karma = -20000;
		
        }

        public Titan(Serial serial)
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
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
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