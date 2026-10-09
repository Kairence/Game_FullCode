using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class StoneGargoyle : BaseCreature
    {
        [Constructable]
        public StoneGargoyle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a stone gargoyle";
            this.Body = 67;
            this.BaseSoundID = 0x174;

            /* Stone Gargoyle - Fame 11,000 / Karma -11,000 */
			/* [HP Calculation]
			   - Target HP: ~32,000
			   - Fame Bonus (11,000): ~26,450
			   - SetHits Required: 5,550 (Target - Bonus)
			*/
			this.SetStr(500, 650);       
			this.SetDex(120, 170);       
			this.SetInt(100, 150);       

			// [Hits] 최종 약 30,000 ~ 34,000 타겟
			this.SetHits(4550, 6550); 
			this.SetStam(120, 170);      

			SetAttackSpeed(4.0);
			SetDamage(35, 50);     // 일반 가고일보다 훨씬 강력한 한 방

			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistance] 물리와 독에 특화된 저항
			this.SetResistance(ResistanceType.Physical, 65, 75); // 돌 피부 (Max 75%)
			this.SetResistance(ResistanceType.Fire, 40, 55);     
			this.SetResistance(ResistanceType.Cold, 25, 40);     
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 독 면역 (Max 75%)
			this.SetResistance(ResistanceType.Energy, 35, 50);

			this.SetSkill(SkillName.Wrestling, 105.0, 120.0);
			this.SetSkill(SkillName.Tactics, 105.0, 120.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);

			this.VirtualArmor = 15;      // 단단한 석재 질감
			this.Tamable = false;

			this.Fame = 11000;           
			this.Karma = -11000;

            if (0.05 > Utility.RandomDouble())
                this.PackItem(new GargoylesPickaxe());
        }

        public StoneGargoyle(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average, 2);
            this.AddLoot(LootPack.Gems, 1);
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
