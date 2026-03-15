using System;

namespace Server.Mobiles
{
    [CorpseName("a red dragon corpse")]
    public class RidableDragon : BaseMount
    {
        [Constructable]
        public RidableDragon()
            : this("a red dragon")
        {
        }
 		
        [Constructable]
        public RidableDragon(string name)
            : base(name, 0x31A, 0x3EBD, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 59;//Utility.RandomList(12, 59);
            BaseSoundID = 362;

            /* Ridable Dragon - Fame 22,000 / Karma -22,000 */
			/* [HP Calculation]
			   - Target HP: ~135,000
			   - Fame Bonus (22,000): ~64,250
			   - SetHits Required: 70,750 (Target - Bonus)
			*/
			this.SetStr(950, 1150);      
			this.SetDex(180, 280);       
			this.SetInt(600, 850);       

			// [Hits] 최종 약 130,000 ~ 140,000 타겟
			this.SetHits(65750, 75750); 
			this.SetStam(180, 280);      
			this.SetMana(600, 850);      

			SetAttackSpeed(2.5);
			SetDamage(85, 125);      

			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 40, 55);     
			this.SetResistance(ResistanceType.Poison, 60, 75);
			this.SetResistance(ResistanceType.Energy, 60, 75);

			this.SetSkill(SkillName.Magery, 115.0, 130.0);
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 140.0);
			this.SetSkill(SkillName.Wrestling, 115.0, 130.0);

			this.VirtualArmor = 18;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 4;       // 탑승의 메리트를 고려해 4슬롯 할당
			this.MinTameSkill = 192.0;   // 200 상한 서버의 정점에 가까운 요구치

			this.Fame = 22000;           
			this.Karma = -22000;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public RidableDragon(Serial serial)
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
