using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gazer corpse")]
    public class Gazer : BaseCreature
    {
        [Constructable]
        public Gazer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a gazer";
            this.Body = 22;
            this.BaseSoundID = 377;

            /* Gazer - Fame 5,000 / Karma -5,000 */
			/* [HP Calculation]
			   - Target HP: ~13,000
			   - Fame Bonus (5,000): ~11,200
			   - SetHits Required: 1,800 (Target - Bonus)
			*/
			this.SetStr(100, 150);       
			this.SetDex(150, 250);       
			this.SetInt(400, 550);       

			// [Hits] 최종 약 12,000 ~ 14,000 타겟
			this.SetHits(800, 2800); 
			this.SetStam(150, 250);      
			this.SetMana(400, 550);      

			SetAttackSpeed(10.0);
			SetDamage(12, 18);      

			this.SetDamageType(ResistanceType.Energy, 100);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 30, 40);
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 65, 75); // Max 75%

			this.SetSkill(SkillName.Magery, 90.0, 100.0);
			this.SetSkill(SkillName.EvalInt, 90.0, 100.0);
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);
			this.SetSkill(SkillName.Wrestling, 80.0, 90.0);

			this.VirtualArmor = 10;      
			this.Tamable = false;

			this.Fame = 5000;           
			this.Karma = -5000;

        }

		public bool poisoncheck = false;
		
        public Gazer(Serial serial)
            : base(serial)
        {
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
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Potions);
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