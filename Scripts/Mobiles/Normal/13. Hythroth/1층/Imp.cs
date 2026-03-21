using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an imp corpse")]
    public class Imp : BaseCreature
    {
        [Constructable]
        public Imp()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an imp";
            Body = 74;
            BaseSoundID = 422;

            /* Imp - Fame 1,000 / Karma -1,000 */
			/* [HP Calculation]
			   - Target HP: ~2,200
			   - Fame Bonus (1,000): ~1,563
			   - SetHits Required: 637 (Target - Bonus)
			*/
			this.SetStr(80, 110);       
			this.SetDex(100, 150);       
			this.SetInt(150, 200);       

			// [Hits] 최종 약 2,000 ~ 2,500 타겟
			this.SetHits(437, 937); 
			this.SetMana(150, 200);      

			SetAttackSpeed(10.0);
			SetDamage(10, 15);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 20, 30);
			this.SetResistance(ResistanceType.Fire, 50, 65);     
			this.SetResistance(ResistanceType.Cold, 10, 25);     
			this.SetResistance(ResistanceType.Poison, 30, 45);

			this.SetSkill(SkillName.Magery, 80.0, 95.0);
			this.SetSkill(SkillName.EvalInt, 80.0, 95.0);
			this.SetSkill(SkillName.Wrestling, 70.0, 85.0);

			this.VirtualArmor = 4;       
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 75.0;    // 200 상한 대비 초보용

			this.Fame = 1000;           
			this.Karma = -1000;
        }

        public Imp(Serial serial)
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
        public override int Hides
        {
            get
            {
                return 6;
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
                return PackInstinct.Daemon;
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
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.MedScrolls, 2);
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