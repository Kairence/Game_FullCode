using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ice hound corpse")]
    public class IceHound : BaseCreature
    {
        [Constructable]
        public IceHound()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an ice hound";
            Body = 98;
            BaseSoundID = 229;
			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;
			else if(Utility.RandomBool() )
				this.Hue = 1153;
			else
				this.Hue = 1154;
			
            /* Ice Hound - Fame 3,000 / Karma -3,000 */
			/* [HP Calculation]
			   - Target HP: ~6,500
			   - Fame Bonus (3,000): ~5,625
			   - SetHits Required: 875 (Target - Bonus)
			*/
			this.SetStr(150, 200);       
			this.SetDex(180, 250);       // 개답게 빠른 공속
			this.SetInt(50, 100);        

			// [Hits] 최종 약 6,000 ~ 7,000 타겟
			this.SetHits(375, 1375); 
			this.SetStam(180, 250);      

			SetAttackSpeed(2.0);
			SetDamage(25, 35);     

			this.SetDamageType(ResistanceType.Cold, 100);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 5, 15);      
			this.SetResistance(ResistanceType.Cold, 70, 75);    // Max 75%
			this.SetResistance(ResistanceType.Poison, 30, 45);

			this.SetSkill(SkillName.Wrestling, 85.0, 100.0);
			this.SetSkill(SkillName.Tactics, 85.0, 100.0);

			this.VirtualArmor = 6;       
			this.Tamable = true;         
			this.ControlSlots = 1;       // 컨트롤 슬롯: 1
			this.MinTameSkill = 85.0;    

			this.Fame = 3000;           
			this.Karma = -3000;

        }

        public IceHound(Serial serial)
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
                return PackInstinct.Canine;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
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