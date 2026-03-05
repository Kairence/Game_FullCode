using System;

namespace Server.Mobiles
{
    [CorpseName("a brown dragon corpse")]
    public class Dragon : BaseCreature
    {
        [Constructable]
        public Dragon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a brown dragon";
            Body = 12;//Utility.RandomList(12, 59);
            BaseSoundID = 362;

            /* Dragon - Fame 18,000 / Karma -18,000 */
			/* [HP Calculation]
			   - Target HP: ~90,000
			   - Fame Bonus (18,000): ~46,725
			   - SetHits Required: 43,275 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(150, 250);       
			this.SetInt(500, 700);       

			// [Hits] 최종 약 85,000 ~ 95,000 타겟
			this.SetHits(38275, 48275); 
			this.SetStam(150, 250);      
			this.SetMana(500, 700);      

			this.SetAttackSpeed(2.2);    
			this.SetDamage(35, 60);      

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 60, 75); 
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 35, 50);     
			this.SetResistance(ResistanceType.Poison, 50, 65);

			this.SetSkill(SkillName.Magery, 110.0, 125.0);
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);

			this.VirtualArmor = 15;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 3;       
			this.MinTameSkill = 175.0;   // 200 상한 서버의 강력한 전력

			this.Fame = 18000;           
			this.Karma = -18000;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Dragon(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return !Controlled;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return !Controlled;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
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
                return HideType.Barbed;
            }
        }
        public override int Scales
        {
            get
            {
                return 7;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return (Body == 12 ? ScaleType.Yellow : ScaleType.Red);
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
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
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.Gems, 8);
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
