using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class Gargoyle : BaseCreature
    {
        [Constructable]
        public Gargoyle()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a gargoyle";
            this.Body = 4;
            this.BaseSoundID = 372;

            /* Gargoyle - Fame 2,800 / Karma -2,800 */
			/* [HP Calculation]
			   - Target HP: ~6,000
			   - Fame Bonus (2,800): ~4,900
			   - SetHits Required: 1,100 (Target - Bonus)
			*/
			this.SetStr(250, 350);       
			this.SetDex(120, 180);       

			// [Hits] 최종 약 5,500 ~ 6,500 타겟
			this.SetHits(600, 1600); 
			this.SetStam(120, 180);      

			SetAttackSpeed(10.0);
			SetDamage(12, 20);    

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 45, 55);     
			this.SetResistance(ResistanceType.Cold, 10, 20);     

			this.SetSkill(SkillName.Wrestling, 80.0, 95.0);
			this.SetSkill(SkillName.Tactics, 80.0, 95.0);

			this.VirtualArmor = 6;       
			this.Tamable = false;

			this.Fame = 2800;           
			this.Karma = -2800;
        }

        public Gargoyle(Serial serial)
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
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.MedScrolls);
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 4));
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