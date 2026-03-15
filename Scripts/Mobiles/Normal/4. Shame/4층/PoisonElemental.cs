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

            /* Poison Elemental - Fame 18,000 / Karma -18,000 */
			/* [HP Calculation]
			   - Target HP: ~90,000
			   - Fame Bonus (18,000): ~47,400
			   - SetHits Required: 42,600 (Target - Bonus)
			*/
			this.SetStr(600, 750);       
			this.SetDex(300, 450);       
			this.SetInt(1000, 1200);     

			// [Hits] 최종 약 85,000 ~ 95,000 타겟
			this.SetHits(37600, 47600); 
			this.SetStam(300, 450);      
			this.SetMana(1000, 1200);    

			SetAttackSpeed(10.0);
			SetDamage(25, 38);      

			this.SetDamageType(ResistanceType.Poison, 100);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 30, 40);
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 70, 75); // Max 75%
			this.SetResistance(ResistanceType.Energy, 50, 60);

			this.SetSkill(SkillName.Magery, 115.0, 125.0);
			this.SetSkill(SkillName.EvalInt, 115.0, 125.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Wrestling, 110.0, 120.0);

			this.VirtualArmor = 20;      
			this.Tamable = false;

			this.Fame = 18000;           
			this.Karma = -18000;

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