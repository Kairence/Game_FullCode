/* Based on Gargoyle, still no infos on Undead Gargoyle... Have to get also the correct body ID */
using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an undead gargoyle corpse")]
    public class UndeadGargoyle : BaseCreature
    {
        [Constructable]
        public UndeadGargoyle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an Undead Gargoyle";
            Body = 722;
            BaseSoundID = 372;

            /* Undead Gargoyle - Fame 12,000 / Karma -12,000 */
			/* [HP Calculation]
			   - Target HP: ~35,000
			   - Fame Bonus (12,000): ~28,650
			   - SetHits Required: 6,350 (Target - Bonus)
			*/
			this.SetStr(450, 600);       
			this.SetDex(150, 200);       
			this.SetInt(150, 250);       

			// [Hits] 최종 약 32,000 ~ 38,000 타겟
			this.SetHits(3350, 9350); 
			this.SetStam(150, 200);      
			this.SetMana(150, 250);      

			SetAttackSpeed(2.8);
			SetDamage(55, 85);   

			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Fire, 30);
			this.SetDamageType(ResistanceType.Poison, 30);    // 언데드 부패 독

			this.SetResistance(ResistanceType.Physical, 50, 65);
			this.SetResistance(ResistanceType.Fire, 60, 75);     
			this.SetResistance(ResistanceType.Cold, 20, 35);     // 언데드라 일반 불생물보다는 냉기에 강함
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 독 면역 (Max 75%)

			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 12;      
			this.Tamable = false;

			this.Fame = 12000;           
			this.Karma = -12000;
        }

        public UndeadGargoyle(Serial serial)
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
            AddLoot(LootPack.Average);
            AddLoot(LootPack.MedScrolls);
            AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 4));
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