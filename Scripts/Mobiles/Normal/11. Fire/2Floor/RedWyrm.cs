using System;

namespace Server.Mobiles
{
    [CorpseName("a red wyrm corpse")]
    public class RedWyrm : BaseCreature, IAuraCreature
    {
        public override double AverageThreshold { get { return 0.25; } }

        [Constructable]
        public RedWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = Utility.RandomBool() ? 180 : 49;
            Name = "a red wyrm";
			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1174;
			else
				Hue = 1360;
            BaseSoundID = 362;

            /* Red Wyrm - Fame 24,000 / Karma -24,000 */
			/* [HP Calculation]
			   - Target HP: ~160,000
			   - Fame Bonus (24,000): ~72,550
			   - SetHits Required: 87,450 (Target - Bonus)
			*/
			this.SetStr(1100, 1300);     
			this.SetDex(200, 300);       
			this.SetInt(800, 1000);      

			// [Hits] 최종 약 155,000 ~ 165,000 타겟
			this.SetHits(82450, 92450); 
			this.SetStam(200, 300);      
			this.SetMana(800, 1000);      

			SetAttackSpeed(2.5);
			SetDamage(85, 125);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 65, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 75, 75);     // 화염 면역
			this.SetResistance(ResistanceType.Cold, 10, 25);     // 냉기 약점
			this.SetResistance(ResistanceType.Poison, 60, 75);
			this.SetResistance(ResistanceType.Energy, 60, 75);

			this.SetSkill(SkillName.Magery, 120.0, 140.0);
			this.SetSkill(SkillName.EvalInt, 120.0, 140.0);
			this.SetSkill(SkillName.Wrestling, 115.0, 135.0);
			this.SetSkill(SkillName.Tactics, 120.0, 140.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 150.0);

			this.VirtualArmor = 18;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 4;       // 4슬롯 (테이머의 모든 슬롯을 차지하는 최종 펫)
			this.MinTameSkill = 195.0;   // 상한 200 서버의 정점 (이 녀석을 얻기 위한 최종 도전)

			this.Fame = 24000;           
			this.Karma = -24000;
            SetAreaEffect(AreaEffect.AuraDamage);
		}

        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }

        public RedWyrm(Serial serial)
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
                return 9;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.White;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Gold;
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
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Gems, Utility.Random(1, 5));
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
