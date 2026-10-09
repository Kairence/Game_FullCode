using System;

namespace Server.Mobiles
{
    [CorpseName("an elder gazer corpse")]
    public class ElderGazer : BaseCreature
    {
        [Constructable]
        public ElderGazer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an elder gazer";
            this.Body = 778;
            this.BaseSoundID = 377;

            /* Elder Gazer - Fame 15,000 / Karma -15,000 */
			/* [HP Calculation]
			   - Target HP: ~60,000
			   - Fame Bonus (15,000): ~37,200
			   - SetHits Required: 22,800 (Target - Bonus)
			*/
			this.SetStr(300, 450);       
			this.SetDex(200, 300);       
			this.SetInt(800, 1000);      

			// [Hits] 최종 약 55,000 ~ 65,000 타겟
			this.SetHits(17800, 27800); 
			this.SetStam(200, 300);      
			this.SetMana(800, 1000);     

			SetAttackSpeed(10.0);
			SetDamage(20, 30);      

			this.SetDamageType(ResistanceType.Energy, 100);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 45, 55);
			this.SetResistance(ResistanceType.Cold, 45, 55);
			this.SetResistance(ResistanceType.Poison, 45, 55);
			this.SetResistance(ResistanceType.Energy, 70, 75); // Max 75%

			this.SetSkill(SkillName.Magery, 110.0, 125.0);
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);
			this.SetSkill(SkillName.Meditation, 100.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 110.0);

			this.VirtualArmor = 20;      
			this.Tamable = false;

			this.Fame = 15000;           
			this.Karma = -15000;
        }

        public ElderGazer(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich);
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
