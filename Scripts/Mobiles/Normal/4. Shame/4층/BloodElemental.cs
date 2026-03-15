using System;

namespace Server.Mobiles
{
    [CorpseName("a blood elemental corpse")]
    public class BloodElemental : BaseCreature, IBloodCreature
    {
        [Constructable]
        public BloodElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a blood elemental";
            this.Body = 159;
            this.BaseSoundID = 278;

            /* Blood Elemental - Fame 22,000 / Karma -22,000 */
			/* [HP Calculation]
			   - Target HP: ~150,000
			   - Fame Bonus (22,000): ~64,300
			   - SetHits Required: 85,700 (Target - Bonus)
			*/
			this.SetStr(1000, 1200);     
			this.SetDex(450, 600);       
			this.SetInt(1200, 1500);     

			// [Hits] 최종 약 145,000 ~ 155,000 타겟
			this.SetHits(80700, 90700); 
			this.SetStam(450, 600);      
			this.SetMana(1200, 1500);    

			SetAttackSpeed(6.5);
			SetDamage(65, 95);    

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 65, 75); // Max 75%
			this.SetResistance(ResistanceType.Poison, 65, 75); // Max 75%
			this.SetResistance(ResistanceType.Energy, 40, 55);

			this.SetSkill(SkillName.Magery, 120.0, 135.0);
			this.SetSkill(SkillName.EvalInt, 120.0, 135.0);
			this.SetSkill(SkillName.Meditation, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);
			this.SetSkill(SkillName.Wrestling, 120.0, 135.0);

			this.VirtualArmor = 30;      
			this.Tamable = false;

			this.Fame = 22000;           
			this.Karma = -22000;
			
			this.SpecialType2 = 9;
			this.SpecialChance2 = 0.40;	
			
        }

        public BloodElemental(Serial serial)
            : base(serial)
        {
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