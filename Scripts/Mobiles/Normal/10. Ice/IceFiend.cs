using System;

namespace Server.Mobiles
{
    [CorpseName("an ice fiend corpse")]
    public class IceFiend : BaseCreature, IAuraCreature
    {
        [Constructable]
        public IceFiend()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an ice fiend";
            Body = 43;
            BaseSoundID = 357;

            /* Ice Fiend - Fame 18,000 / Karma -18,000 */
			/* [HP Calculation]
			   - Target HP: ~90,000
			   - Fame Bonus (18,000): ~47,420
			   - SetHits Required: 42,580 (Target - Bonus)
			*/
			this.SetStr(600, 800);       
			this.SetDex(180, 280);       
			this.SetInt(700, 900);       

			// [Hits] 최종 약 85,000 ~ 95,000 타겟
			this.SetHits(37580, 47580); 
			this.SetStam(180, 280);      
			this.SetMana(700, 900);      

			SetAttackSpeed(2.5);
			SetDamage(70, 100);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Cold, 50);

			this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 20, 35);     
			this.SetResistance(ResistanceType.Cold, 75, 75);    // Max 75%
			this.SetResistance(ResistanceType.Poison, 55, 65);
			this.SetResistance(ResistanceType.Energy, 55, 65);

			this.SetSkill(SkillName.Magery, 110.0, 125.0);
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);

			this.VirtualArmor = 12;      
			this.Tamable = false;

			this.Fame = 18000;           
			this.Karma = -18000;

            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public IceFiend(Serial serial)
            : base(serial)
        {
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
                return 1;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Average);
            AddLoot(LootPack.MedScrolls, 2);
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
