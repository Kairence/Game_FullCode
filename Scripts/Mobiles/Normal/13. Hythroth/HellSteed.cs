using System;

namespace Server.Mobiles
{
    [CorpseName("a hellsteed corpse")]
    public class HellSteed : BaseMount, IElementalCreature
    {
        public ElementType ElementType { get { return ElementType.Chaos; } }

        [Constructable] 
        public HellSteed()
            : this("a hellsteed")
        {
        }

        [Constructable]
        public HellSteed(string name)
            : base(name, 793, 0x3EBB, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            /* Hell Steed - Fame 17,000 / Karma -17,000 */
			/* [HP Calculation]
			   - Target HP: ~65,000
			   - Fame Bonus (17,000): ~43,813
			   - SetHits Required: 21,187 (Target - Bonus)
			*/
			this.SetStr(700, 900);       
			this.SetDex(200, 300);       
			this.SetInt(300, 500);       

			// [Hits] 최종 약 60,000 ~ 70,000 타겟
			this.SetHits(16187, 26187); 
			this.SetStam(200, 300);      
			this.SetMana(300, 500);      

			this.SetAttackSpeed(1.8);    
			this.SetDamage(30, 45);      

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 55, 70); 
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 30, 45);     
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 언데드 성질 (Max 75%)

			this.SetSkill(SkillName.Wrestling, 115.0, 130.0);
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);

			this.VirtualArmor = 12;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       
			this.MinTameSkill = 190.0;   // 200 상한 서버의 초핵심 타겟 (매우 높음)

			this.Fame = 17000;           
			this.Karma = -17000;
            //SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public HellSteed(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune { get { return Poison.Lethal; } }

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
