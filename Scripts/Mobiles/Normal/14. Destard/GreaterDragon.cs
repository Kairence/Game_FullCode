using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an elder dragon corpse")]
    public class GreaterDragon : BaseCreature
    {
        [Constructable]
        public GreaterDragon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.3, 0.5)
        {
            Name = "an elder dragon";
            Body = 172; //구 Rikktor
            BaseSoundID = 362;

			/* Elder Dragon - Fame 27,000 / Karma -27,000 */
			/* [HP Calculation]
			   - Target HP: ~175,000
			   - Fame Bonus (27,000): ~84,850
			   - SetHits Required: 90,150 (Target - Bonus)
			*/
			this.SetStr(1100, 1300);     
			this.SetDex(160, 260);       
			this.SetInt(600, 800);       

			// [Hits] 명성 보너스 포함 최종 약 170,000 ~ 180,000 타겟
			this.SetHits(85150, 95150); 
			this.SetStam(160, 260);      
			this.SetMana(600, 800);      

			SetAttackSpeed(2.5);
			SetDamage(100, 140);      

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 70, 75); 
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 50, 65);     
			this.SetResistance(ResistanceType.Poison, 60, 75);
			this.SetResistance(ResistanceType.Energy, 60, 75);

			this.SetSkill(SkillName.Wrestling, 125.0, 140.0);
			this.SetSkill(SkillName.Tactics, 125.0, 140.0);
			this.SetSkill(SkillName.Magery, 115.0, 130.0);

			this.VirtualArmor = 20;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 5;       
			this.MinTameSkill = 195.0;   // 최상위 요구치 유지

			this.Fame = 27000;           
			this.Karma = -27000;

            //SetWeaponAbility(WeaponAbility.BleedAttack);
            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public GreaterDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
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
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int Hides
        {
            get
            {
                return 30;
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
            AddLoot(LootPack.FilthyRich, 4);
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
