using System;

namespace Server.Mobiles
{
    [CorpseName("a frost ooze corpse")]
    public class FrostOoze : BaseCreature
    {
        [Constructable]
        public FrostOoze()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a frost ooze";
            this.Body = 94;
            this.BaseSoundID = 456;

            /* Frost Ooze - Fame 1,500 / Karma -1,500 */
			/* [HP Calculation]
			   - Target HP: ~3,500
			   - Fame Bonus (1,500): ~2,625
			   - SetHits Required: 875 (Target - Bonus)
			*/
			this.SetStr(80, 100);       
			this.SetDex(50, 70);        
			this.SetInt(20, 40);         

			// [Hits] 최종 약 3,000 ~ 4,000 타겟
			this.SetHits(375, 1375); 
			this.SetStam(50, 70);      

			SetAttackSpeed(4.0);
			SetDamage(18, 28);    

			this.SetDamageType(ResistanceType.Cold, 100);

			this.SetResistance(ResistanceType.Physical, 10, 20);
			this.SetResistance(ResistanceType.Fire, -20, 0);    // 불에 매우 약함
			this.SetResistance(ResistanceType.Cold, 75, 75);    // 냉기 면역 (Max 75%)
			this.SetResistance(ResistanceType.Poison, 75, 75);  // 독 면역 (Max 75%)

			this.SetSkill(SkillName.Wrestling, 50.0, 65.0);
			this.SetSkill(SkillName.Tactics, 50.0, 65.0);

			this.VirtualArmor = 2;       // 점액질이라 방어력 거의 없음
			this.Tamable = false;

			this.Fame = 1500;           
			this.Karma = -1500;
        }

        public FrostOoze(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 2));
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