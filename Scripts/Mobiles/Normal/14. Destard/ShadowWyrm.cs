using System;

namespace Server.Mobiles
{
    [CorpseName("a shadow wyrm corpse")]
    public class ShadowWyrm : BaseCreature
    {
        [Constructable]
        public ShadowWyrm()
            : base(AIType.AI_NecroMage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a shadow wyrm";
            Body = 106;
            BaseSoundID = 362;

            /* Shadow Wyrm - Fame 25,000 / Karma -25,000 */
			/* [HP Calculation]
			   - Target HP: ~155,000
			   - Fame Bonus (25,000): ~76,850
			   - SetHits Required: 78,150 (Target - Bonus)
			*/
			this.SetStr(1000, 1200);     
			this.SetDex(220, 320);       
			this.SetInt(950, 1250);      

			// [Hits] 명성 보너스 포함 최종 약 150,000 ~ 160,000 타겟
			this.SetHits(73150, 83150); 
			this.SetStam(220, 320);      
			this.SetMana(950, 1250);      

			SetAttackSpeed(2.5);
			SetDamage(95, 135);     

			this.SetDamageType(ResistanceType.Energy, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 55, 70);     
			this.SetResistance(ResistanceType.Cold, 75, 75);     
			this.SetResistance(ResistanceType.Energy, 75, 75);

			this.SetSkill(SkillName.Magery, 130.0, 150.0);
			this.SetSkill(SkillName.EvalInt, 130.0, 150.0);
			this.SetSkill(SkillName.MagicResist, 140.0, 160.0);

			this.VirtualArmor = 15;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 5;       
			this.MinTameSkill = 190.0;   

			this.Fame = 25000;           
			this.Karma = -25000;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public ShadowWyrm(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool ReacquireOnMovement { get { return !Controlled; } }
        public override bool AutoDispel { get { return !Controlled; } }
        public override Poison PoisonImmune { get { return Poison.Deadly; } }
        public override Poison HitPoison { get { return Poison.Deadly; } }
        public override int TreasureMapLevel { get { return 5; } }
        public override int Meat { get { return 19; } }
        public override int Hides { get { return 20; } }
        public override int Scales { get { return 10; } }
        public override ScaleType ScaleType { get { return ScaleType.Black; } }
        public override HideType HideType { get { return HideType.Barbed; } }
        public override bool CanFly { get { return true; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 5);
        }

        public override int GetIdleSound()
        {
            return 0x2D5;
        }

        public override int GetHurtSound()
        {
            return 0x2D1;
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
