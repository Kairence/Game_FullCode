using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a wyvern corpse")]
    public class Wyvern : BaseCreature
    {
        [Constructable]
        public Wyvern()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a wyvern";
            this.Body = 62;
            this.BaseSoundID = 362;

            /* Wyvern - Fame 12,000 / Karma -12,000 */
			/* [HP Calculation]
			   - Target HP: ~40,000
			   - Fame Bonus (12,000): ~28,650
			   - SetHits Required: 11,350 (Target - Bonus)
			*/
			this.SetStr(500, 650);       
			this.SetDex(180, 280);       
			this.SetInt(100, 150);       

			// [Hits] 최종 약 38,000 ~ 42,000 타겟
			this.SetHits(9350, 13350); 
			this.SetStam(180, 280);      

			SetAttackSpeed(2.5);
			SetDamage(55, 85);    

			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistance] 비늘 덕분에 물리 저항이 우수함
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 45, 60);     
			this.SetResistance(ResistanceType.Cold, 30, 45);     
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 치명독 사용자답게 독 면역 (Max 75%)
			this.SetResistance(ResistanceType.Energy, 35, 50);

			this.SetSkill(SkillName.Wrestling, 105.0, 120.0);
			this.SetSkill(SkillName.Tactics, 105.0, 120.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 140.0);    // 최상급 독(Level 5+) 숙련도

			this.VirtualArmor = 10;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       
			this.MinTameSkill = 135.0;   // 200 상한 대비 중급 이상의 핵심 타겟

			this.Fame = 12000;           
			this.Karma = -12000;
			
            this.PackItem(new LesserPoisonPotion());
        }

        public Wyvern(Serial serial)
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
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Deadly;
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
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Meager);
            this.AddLoot(LootPack.MedScrolls);
        }

        public override int GetAttackSound()
        {
            return 713;
        }

        public override int GetAngerSound()
        {
            return 718;
        }

        public override int GetDeathSound()
        {
            return 716;
        }

        public override int GetHurtSound()
        {
            return 721;
        }

        public override int GetIdleSound()
        {
            return 725;
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