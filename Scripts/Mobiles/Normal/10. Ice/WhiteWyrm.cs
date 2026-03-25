using System;

namespace Server.Mobiles
{
    [CorpseName("a white wyrm corpse")]
    public class WhiteWyrm : BaseCreature, IAuraCreature
    {
        public override double AverageThreshold { get { return 0.25; } }

        [Constructable]
        public WhiteWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = Utility.RandomBool() ? 180 : 49;
			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;			
            Name = "a white wyrm";
            BaseSoundID = 362;

            /* White Wyrm - Fame 22,000 / Karma -22,000 */
			/* [HP Calculation]
			   - Target HP: ~130,000
			   - Fame Bonus (22,000): ~64,250
			   - SetHits Required: 65,750 (Target - Bonus)
			*/
			this.SetStr(900, 1100);      
			this.SetDex(180, 280);       
			this.SetInt(700, 900);       

			// [Hits] 최종 약 125,000 ~ 135,000 타겟
			this.SetHits(60750, 70750); 
			this.SetStam(180, 280);      
			this.SetMana(700, 900);      

			SetAttackSpeed(2.5);
			SetDamage(80, 115);   

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			// [Resistance] 냉기 면역 수준, 화염 취약
			this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 15, 30);     
			this.SetResistance(ResistanceType.Cold, 75, 75);    // Max 75%
			this.SetResistance(ResistanceType.Poison, 55, 70);
			this.SetResistance(ResistanceType.Energy, 55, 70);

			this.SetSkill(SkillName.Magery, 115.0, 130.0);
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);

			this.VirtualArmor = 15;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 4;       // 4슬롯 (강력한 주력 펫)
			this.MinTameSkill = 180.0;   // 상한 200 서버의 핵심 타겟 (매우 높음)

			this.Fame = 22000;           
			this.Karma = -22000;
            SetAreaEffect(AreaEffect.AuraDamage);
		}

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }


        public WhiteWyrm(Serial serial)
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
