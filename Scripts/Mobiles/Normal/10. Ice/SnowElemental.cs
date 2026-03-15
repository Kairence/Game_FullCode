using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a snow elemental corpse")]
    public class SnowElemental : BaseCreature, IAuraCreature
    {
        [Constructable]
        public SnowElemental()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a snow elemental";
            Body = 163;
            BaseSoundID = 263;

            /* Snow Elemental - Fame 12,000 / Karma -12,000 */
			/* [HP Calculation]
			   - Target HP: ~35,000
			   - Fame Bonus (12,000): ~28,650
			   - SetHits Required: 6,350 (Target - Bonus)
			*/
			this.SetStr(450, 600);       
			this.SetDex(180, 250);       
			this.SetInt(600, 800);       

			// [Hits] 최종 약 32,000 ~ 38,000 타겟
			this.SetHits(3350, 9350); 
			this.SetStam(180, 250);      
			this.SetMana(600, 800);      

			SetAttackSpeed(4.5);
			SetDamage(55, 85);     

			this.SetDamageType(ResistanceType.Cold, 100);

			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, -5, 10);    
			this.SetResistance(ResistanceType.Cold, 75, 75);    // Max 75%
			this.SetResistance(ResistanceType.Poison, 65, 75);  // Max 75%
			this.SetResistance(ResistanceType.Energy, 45, 60);

			this.SetSkill(SkillName.Magery, 105.0, 120.0);
			this.SetSkill(SkillName.EvalInt, 105.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);

			this.VirtualArmor = 8;       
			this.Tamable = false;

			this.Fame = 12000;           
			this.Karma = -12000;

            PackItem(new BlackPearl(3));
            Item ore = new IronOre(3);
            ore.ItemID = 0x19B8;
            PackItem(ore);

            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public SnowElemental(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
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
                return 2;
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
            AddLoot(LootPack.Rich);
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
