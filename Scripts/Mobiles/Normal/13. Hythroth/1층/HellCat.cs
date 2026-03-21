using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a hell cat corpse")]
    [TypeAlias("Server.Mobiles.Hellcat")]
    public class HellCat : BaseCreature
    {
        [Constructable]
        public HellCat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a hell cat";
            Body = 0xC9;
            Hue = Utility.RandomList(0x647, 0x650, 0x659, 0x662, 0x66B, 0x674);
            BaseSoundID = 0x69;

            /* Hell Cat - Fame 1,500 / Karma -1,500 */
			/* [HP Calculation]
			   - Target HP: ~4,000
			   - Fame Bonus (1,500): ~2,625
			   - SetHits Required: 1,375 (Target - Bonus)
			*/
			this.SetStr(120, 160);       
			this.SetDex(150, 200);       

			// [Hits] 최종 약 3,500 ~ 4,500 타겟
			this.SetHits(875, 1875); 
			this.SetStam(150, 200);      

			SetAttackSpeed(2.0);
			SetDamage(15, 25);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 55, 70);     

			this.SetSkill(SkillName.Wrestling, 75.0, 90.0);
			this.SetSkill(SkillName.Tactics, 75.0, 90.0);

			this.VirtualArmor = 4;       

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 70.0;    

			this.Fame = 1500;           
			this.Karma = -1500;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public HellCat(Serial serial)
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
