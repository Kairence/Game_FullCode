using System;

namespace Server.Mobiles
{
    [CorpseName("a dragon wolf corpse")]
    public class DragonWolf : BaseCreature
    {
        [Constructable]
        public DragonWolf()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a dragon wolf";
            Body = 719;
            BaseSoundID = 0x5ED;

            /* Dragon Wolf - Fame 9,000 / Karma -9,000 */
			/* [HP Calculation]
			   - Target HP: ~25,000
			   - Fame Bonus (9,000): ~21,850
			   - SetHits Required: 3,150 (Target - Bonus)
			*/
			this.SetStr(450, 600);       
			this.SetDex(200, 300);       
			this.SetInt(100, 200);       

			// [Hits] 최종 약 23,000 ~ 27,000 타겟
			this.SetHits(2150, 4150); 
			this.SetStam(200, 300);      

			this.SetAttackSpeed(1.8);    // 늑대 특유의 매우 빠른 공속
			this.SetDamage(18, 30);      

			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Fire, 30);

			this.SetResistance(ResistanceType.Physical, 55, 70); 
			this.SetResistance(ResistanceType.Fire, 45, 60);     
			this.SetResistance(ResistanceType.Cold, 30, 45);     
			this.SetResistance(ResistanceType.Poison, 40, 55);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);

			this.VirtualArmor = 12;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       
			this.MinTameSkill = 145.0;   // 200 상한 대비 중상급

			this.Fame = 9000;           
			this.Karma = -9000;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public DragonWolf(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAngerOnTame { get { return true; } }

        public override int Meat { get { return 4; } }
        public override int Hides { get { return 25; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Rich);
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
