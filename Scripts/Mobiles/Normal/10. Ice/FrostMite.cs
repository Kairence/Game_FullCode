using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a beetle corpse")]
    public class FrostMite : BaseCreature
    {
        [Constructable]
        public FrostMite() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a frost mite";
            Body = 0x590;
            Female = true;

			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;

            /* Frost Mite - Fame 5,000 / Karma -5,000 */
			/* [HP Calculation]
			   - Target HP: ~12,000
			   - Fame Bonus (5,000): ~11,250
			   - SetHits Required: 750 (Target - Bonus)
			*/
			this.SetStr(250, 350);       
			this.SetDex(200, 300);       // 매우 빠른 기동력
			this.SetInt(50, 100);        

			// [Hits] 최종 약 11,000 ~ 13,000 타겟
			this.SetHits(250, 1250); 
			this.SetStam(200, 300);      
			this.SetMana(50, 100);       

			this.SetAttackSpeed(2.0);    // 민첩한 공격 속도
			this.SetDamage(12, 22);      

			this.SetDamageType(ResistanceType.Cold, 100); // 100% 냉기 공격

			// [Resistance] 저항 캡 75% 준수 / 화염에 극도로 취약
			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 5, 15);      // 화염 약점
			this.SetResistance(ResistanceType.Cold, 70, 75);     // 냉기 저항 상한 (Max 75%)
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			this.SetSkill(SkillName.Wrestling, 85.0, 100.0);
			this.SetSkill(SkillName.Tactics, 85.0, 100.0);
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);

			this.VirtualArmor = 8;       // 결정 껍질이지만 타격감을 위해 낮춤

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       // 컨트롤 슬롯: 2 (낮은 슬롯 소모로 범용성 높음)
			this.MinTameSkill = 90.0;    // 최소 테이밍 스킬 요구치

			this.Fame = 5000;           
			this.Karma = -5000;
        }

        public override int GetAngerSound()
        {
            return 0x4E8;
        }

        public override int GetIdleSound()
        {
            return 0x4E7;
        }

        public override int GetAttackSound()
        {
            return 0x4E6;
        }

        public override int GetHurtSound()
        {
            return 0x4E9;
        }

        public override int GetDeathSound()
        {
            return 0x4E5;
        }

        public override int Meat { get { return 5; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool StatLossAfterTame { get { return true; } }

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }

        public FrostMite(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
