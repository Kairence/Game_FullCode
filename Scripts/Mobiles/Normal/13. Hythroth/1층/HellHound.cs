using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a hell hound corpse")]
    public class HellHound : BaseCreature
    {
        [Constructable]
        public HellHound()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a hell hound";
            Body = 98;
            BaseSoundID = 229;

            /* Hell Hound - Fame 3,000 / Karma -3,000 */
			/* [HP Calculation]
			   - Target HP: ~6,500
			   - Fame Bonus (3,000): ~5,625
			   - SetHits Required: 875 (Target - Bonus)
			*/
			this.SetStr(180, 240);       
			this.SetDex(150, 200);       

			// [Hits] 최종 약 6,000 ~ 7,000 타겟
			this.SetHits(375, 1375); 
			this.SetStam(150, 200);      

			this.SetAttackSpeed(2.2);    
			this.SetDamage(14, 24);      

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 65, 75);     
			this.SetResistance(ResistanceType.Cold, 5, 20);      

			this.SetSkill(SkillName.Wrestling, 85.0, 100.0);
			this.SetSkill(SkillName.Tactics, 85.0, 100.0);

			this.VirtualArmor = 6;       

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 90.0;    

			this.Fame = 3000;           
			this.Karma = -3000;

            PackItem(new SulfurousAsh(5));
            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public HellHound(Serial serial)
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
