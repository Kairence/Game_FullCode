using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a drake corpse")]
    public class ColdDrake : BaseCreature, IAuraCreature
    {
        [Constructable]
        public ColdDrake() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a cold crimson drake";
			if( 0.000001 > Utility.RandomDouble() )
				Body = 1417;
			else
				Body = 1418;

            BaseSoundID = 362;

            //Hue = Utility.RandomMinMax(1319, 1327);

            /* Cold Drake - Fame 10,000 / Karma -10,000 */
			/* [HP Calculation]
			   - Target HP: ~35,000
			   - Fame Bonus (10,000): ~24,150
			   - SetHits Required: 10,850 (Target - Bonus)
			*/
			this.SetStr(500, 650);       
			this.SetDex(180, 280);       
			this.SetInt(350, 500);       

			// [Hits] 최종 약 32,000 ~ 38,000 타겟
			this.SetHits(7850, 13850); 
			this.SetStam(180, 280);      
			this.SetMana(350, 500);      

			SetAttackSpeed(2.5);
			SetDamage(50, 75);    

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			// [Resistance] 냉기 면역 수준, 화염에 매우 취약
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 10, 25);     // 화염 약점
			this.SetResistance(ResistanceType.Cold, 75, 75);     // 냉기 저항 상한 (Max 75%)
			this.SetResistance(ResistanceType.Poison, 35, 45);
			this.SetResistance(ResistanceType.Energy, 35, 45);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Magery, 100.0, 110.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 12;      // 가죽 외피 (낮은 방어력으로 타격감 유지)

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 3;       // 컨트롤 슬롯: 3
			this.MinTameSkill = 145.0;   // 최소 테이밍 스킬 요구치

			this.Fame = 10000;           
			this.Karma = -10000;

            SetSpecialAbility(SpecialAbility.DragonBreath);
            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
        }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool ReacquireOnMovement { get { return !Controlled; } }
		public override int TreasureMapLevel { get { return 3; } }
        public override int Meat { get { return 10; } }
        public override int Hides { get { return 22; } }
        public override HideType HideType { get { return HideType.Horned; } }
        public override int DragonBlood { get { return 8; } }
        public override FoodType FavoriteFood { get { return FoodType.Fish; } }

        public virtual void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }

        public ColdDrake(Serial serial) : base(serial)
        {
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
