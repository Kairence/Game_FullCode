using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a drake corpse")]
    public class FireDrake : BaseCreature, IAuraCreature
    {
        [Constructable]
        public FireDrake() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fire crimson drake";
			if( 0.000001 > Utility.RandomDouble() )
				Body = 1419;
			else
				Body = 1420;

            BaseSoundID = 362;

            //Hue = Utility.RandomMinMax(1319, 1327);

            /* Fire Drake - Fame 10,000 / Karma -10,000 */
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

			this.SetAttackSpeed(2.2);    
			this.SetDamage(20, 35);      

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 75, 75);     // 화염 면역 (Max 75%)
			this.SetResistance(ResistanceType.Cold, 0, 15);      // 냉기 취약
			this.SetResistance(ResistanceType.Poison, 35, 45);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Magery, 100.0, 110.0);

			this.VirtualArmor = 12;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 3;       
			this.MinTameSkill = 145.0;   // 200 상한 대비 중급 이상의 숙련도

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

        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }
        public FireDrake(Serial serial) : base(serial)
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
