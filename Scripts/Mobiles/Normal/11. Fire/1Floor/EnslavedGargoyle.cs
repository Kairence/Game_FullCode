using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an enslaved gargoyle corpse")]
    public class EnslavedGargoyle : BaseCreature
    {
        [Constructable]
        public EnslavedGargoyle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an enslaved gargoyle";
            Body = 0x2F1;
            BaseSoundID = 0x174;

            /* Enslaved Gargoyle - Fame 2,500 / Karma -2,500 */
			/* [HP Calculation]
			   - Target HP: ~5,500
			   - Fame Bonus (2,500): ~4,375
			   - SetHits Required: 1,125 (Target - Bonus)
			*/
			this.SetStr(200, 300);       
			this.SetDex(100, 150);       

			// [Hits] 최종 약 5,000 ~ 6,000 타겟
			this.SetHits(625, 1625); 
			this.SetStam(100, 150);      

			SetAttackSpeed(2.6);
			SetDamage(12, 18);     

			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Fire, 30);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 50, 60);     
			this.SetResistance(ResistanceType.Cold, 0, 10);      // 냉기 속성에 매우 취약

			this.SetSkill(SkillName.Wrestling, 70.0, 85.0);
			this.SetSkill(SkillName.Tactics, 70.0, 85.0);

			this.VirtualArmor = 5;       
			this.Tamable = false;

			this.Fame = 2500;           
			this.Karma = -2500;
            //SetSpecialAbility(SpecialAbility.AngryFire);
        }

        public EnslavedGargoyle(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
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