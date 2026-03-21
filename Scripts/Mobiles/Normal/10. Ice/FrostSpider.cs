using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a frost spider corpse")]
    public class FrostSpider : BaseCreature
    {
        [Constructable]
        public FrostSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a frost spider";
            Body = 20;
            BaseSoundID = 0x388;

			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;
            else if (Utility.RandomBool())
                Hue = 1154;

            /* Frost Spider - Fame 2,500 / Karma -2,500 */
			/* [HP Calculation]
			   - Target HP: ~5,000
			   - Fame Bonus (2,500): ~4,375
			   - SetHits Required: 625 (Target - Bonus)
			*/
			this.SetStr(100, 150);       
			this.SetDex(150, 200);       // 거미 특유의 빠른 속도
			this.SetInt(50, 80);        

			// [Hits] 최종 약 4,500 ~ 5,500 타겟
			this.SetHits(125, 1125); 
			this.SetStam(150, 200);      

			SetAttackSpeed(2.0);
			SetDamage(22, 32);      

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			this.SetResistance(ResistanceType.Physical, 20, 30);
			this.SetResistance(ResistanceType.Fire, 5, 15);      
			this.SetResistance(ResistanceType.Cold, 65, 75);    // Max 75%
			this.SetResistance(ResistanceType.Poison, 40, 50);

			this.SetSkill(SkillName.Wrestling, 75.0, 90.0);
			this.SetSkill(SkillName.Tactics, 75.0, 90.0);
			this.SetSkill(SkillName.Poisoning, 60.0, 80.0);    // 독 공격 가능

			this.VirtualArmor = 5;       // 얇은 외골격
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 75.0;    

			this.Fame = 2500;           
			this.Karma = -2500;
        }

        public FrostSpider(Serial serial)
            : base(serial)
        {
        }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Arachnid;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.Poor);
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