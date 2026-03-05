using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a charred corpse")]
    public class FireGargoyle : BaseCreature
    {
        [Constructable]
        public FireGargoyle()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("fire gargoyle");
            Body = 130;
            BaseSoundID = 0x174;

            /* Fire Gargoyle - Fame 5,000 / Karma -5,000 */
			/* [HP Calculation]
			   - Target HP: ~12,000
			   - Fame Bonus (5,000): ~11,250
			   - SetHits Required: 750 (Target - Bonus)
			*/
			this.SetStr(300, 400);       
			this.SetDex(150, 200);       
			this.SetInt(250, 350);       

			// [Hits] 최종 약 11,000 ~ 13,000 타겟
			this.SetHits(250, 1250); 
			this.SetStam(150, 200);      
			this.SetMana(250, 350);      

			this.SetAttackSpeed(2.4);    
			this.SetDamage(15, 25);      

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 65, 75);     // 화염 저항 상한 (Max 75%)
			this.SetResistance(ResistanceType.Cold, 5, 15);      // 냉기 취약
			this.SetResistance(ResistanceType.Poison, 40, 50);

			this.SetSkill(SkillName.Magery, 85.0, 100.0);
			this.SetSkill(SkillName.EvalInt, 80.0, 95.0);
			this.SetSkill(SkillName.Wrestling, 85.0, 100.0);
			this.SetSkill(SkillName.Tactics, 85.0, 100.0);

			this.VirtualArmor = 8;       
			this.Tamable = false;

			this.Fame = 5000;           
			this.Karma = -5000;
        }

        public FireGargoyle(Serial serial)
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
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Gems);
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
