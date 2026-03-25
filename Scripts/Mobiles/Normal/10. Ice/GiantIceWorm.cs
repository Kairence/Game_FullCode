using System;

namespace Server.Mobiles
{
    [CorpseName("a giant ice worm corpse")]
    public class GiantIceWorm : BaseCreature
    {
        [Constructable]
        public GiantIceWorm()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 89;
            Name = "a giant ice worm";
            BaseSoundID = 0xDC;

			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;
			else if(Utility.RandomBool() )
				this.Hue = 1153;
			else
				this.Hue = 1154;
			
            /* Giant Ice Worm - Fame 5,500 / Karma -5,500 */
			/* [HP Calculation]
			   - Target HP: ~12,500
			   - Fame Bonus (5,500): ~12,500
			   - SetHits Required: 100~500 (보너스만으로도 목표 체력 도달 가능)
			*/
			this.SetStr(450, 600);       
			this.SetDex(100, 150);       
			this.SetInt(50, 100);        

			// [Hits] 최종 약 12,000 ~ 13,000 타겟
			this.SetHits(100, 500); 
			this.SetStam(100, 150);      
			this.SetMana(50, 100);       

			SetAttackSpeed(4.5);
			SetDamage(45, 65);   

			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Cold, 40);

			// [Resistance] 냉기 저항 상한 준수, 화염에 취약
			this.SetResistance(ResistanceType.Physical, 40, 55);
			this.SetResistance(ResistanceType.Fire, 10, 20);      
			this.SetResistance(ResistanceType.Cold, 75, 75);    // 냉기 저항 상한 (Max 75%)
			this.SetResistance(ResistanceType.Poison, 40, 55);
			this.SetResistance(ResistanceType.Energy, 30, 45);

			this.SetSkill(SkillName.Wrestling, 90.0, 105.0);
			this.SetSkill(SkillName.Tactics, 90.0, 105.0);
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);

			this.VirtualArmor = 10;      // 두꺼운 외피를 가졌으나 타격감을 위해 낮춤

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       // 컨트롤 슬롯: 2
			this.MinTameSkill = 125.0;   // 상한 200 대비 중급 숙련도 요구

			this.Fame = 5500;           
			this.Karma = -5500;
        }

        public GiantIceWorm(Serial serial)
            : base(serial)
        {
        }

        public override bool SubdueBeforeTame
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
                return Poison.Greater;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }

        public override bool StatLossAfterTame { get { return true; } }

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
