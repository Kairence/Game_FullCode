using System;

namespace Server.Mobiles
{
    [CorpseName("a hell cat corpse")]
    [TypeAlias("Server.Mobiles.Preditorhellcat")]
    public class PredatorHellCat : BaseCreature
    {
        [Constructable]
        public PredatorHellCat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a predator hellcat";
            Body = 127;
            BaseSoundID = 0xBA;

            /* Predator Hell Cat - Fame 7,000 / Karma -7,000 */
			/* [HP Calculation]
			   - Target HP: ~18,000
			   - Fame Bonus (7,000): ~16,400
			   - SetHits Required: 1,600 (Target - Bonus)
			*/
			this.SetStr(350, 450);       
			this.SetDex(200, 300);       // 표범 특유의 빠른 속도
			this.SetInt(100, 150);       

			// [Hits] 최종 약 17,000 ~ 19,000 타겟
			this.SetHits(600, 2600); 
			this.SetStam(200, 300);      

			SetAttackSpeed(1.8);
			SetDamage(30, 45);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 70, 75);     
			this.SetResistance(ResistanceType.Cold, 15, 30);     
			this.SetResistance(ResistanceType.Poison, 40, 55);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);

			this.VirtualArmor = 8;       

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       
			this.MinTameSkill = 140.0;   // 200 상한 대비 중급 숙련도

			this.Fame = 7000;           
			this.Karma = -7000;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public PredatorHellCat(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
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
                return PackInstinct.Feline;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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
