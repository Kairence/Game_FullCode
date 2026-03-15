using System;
using Server.Regions;

namespace Server.Mobiles
{
    [CorpseName("a drake corpse")]
    public class Drake : BaseCreature
    {
        [Constructable]
        public Drake()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an brown drake";
            Body = 60; //Utility.RandomList(60, 61);
            BaseSoundID = 362;

            /* Drake - Fame 11,000 / Karma -11,000 */
			/* [HP Calculation]
			   - Target HP: ~35,000
			   - Fame Bonus (11,000): ~26,450
			   - SetHits Required: 8,550 (Target - Bonus)
			*/
			this.SetStr(400, 550);       
			this.SetDex(150, 250);       
			this.SetInt(150, 250);       

			// [Hits] 최종 약 32,000 ~ 38,000 타겟
			this.SetHits(5550, 11550); 
			this.SetStam(150, 250);      
			this.SetMana(150, 250);      

			SetAttackSpeed(2.5);
			SetDamage(50, 75);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 60, 75);     
			this.SetResistance(ResistanceType.Cold, 30, 45);     
			this.SetResistance(ResistanceType.Poison, 40, 55);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Magery, 90.0, 105.0);

			this.VirtualArmor = 12;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       
			this.MinTameSkill = 135.0;   // 200 상한 대비 중급용

			this.Fame = 11000;           
			this.Karma = -11000;

            PackReg(3);

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Drake(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override int Meat
        {
            get
            {
                return 10;
            }
        }
        public override int DragonBlood
        {
            get
            {
                return 8;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        public override int Scales
        {
            get
            {
                return 2;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.Yellow;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish;
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
            AddLoot(LootPack.Rich);
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
