using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ice elemental corpse")]
    public class IceElemental : BaseCreature, IAuraCreature
    {
        [Constructable]
        public IceElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an ice elemental";
            Body = 161;
            BaseSoundID = 268;

            /* Ice Elemental - Fame 9,000 / Karma -9,000 */
			/* [HP Calculation]
			   - Target HP: ~25,000
			   - Fame Bonus (9,000): ~21,850
			   - SetHits Required: 3,150 (Target - Bonus)
			*/
			this.SetStr(400, 550);       
			this.SetDex(150, 200);       
			this.SetInt(350, 500);       

			// [Hits] 최종 약 23,000 ~ 27,000 타겟
			this.SetHits(2150, 4150); 
			this.SetStam(150, 200);      
			this.SetMana(350, 500);      

			SetAttackSpeed(10.0);
			SetDamage(18, 28);     

			this.SetDamageType(ResistanceType.Cold, 100);

			// [Resistance] 정령다운 높은 저항, 화염에는 매우 취약
			this.SetResistance(ResistanceType.Physical, 50, 65);
			this.SetResistance(ResistanceType.Fire, -15, 0);    // 화염 약점
			this.SetResistance(ResistanceType.Cold, 75, 75);    // 냉기 면역 (Max 75%)
			this.SetResistance(ResistanceType.Poison, 60, 75);  // 독 면역 수준 (Max 75%)
			this.SetResistance(ResistanceType.Energy, 40, 55);

			this.SetSkill(SkillName.Magery, 95.0, 110.0);
			this.SetSkill(SkillName.EvalInt, 95.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);
			this.SetSkill(SkillName.Wrestling, 90.0, 105.0);

			this.VirtualArmor = 10;      
			this.Tamable = false;

			this.Fame = 9000;           
			this.Karma = -9000;

            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public IceElemental(Serial serial)
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

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
            AddLoot(LootPack.Gems, 2);
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
