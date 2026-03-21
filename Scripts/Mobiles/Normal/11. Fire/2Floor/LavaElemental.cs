using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a lava elemental corpse")]
    public class LavaElemental : BaseCreature, IAuraCreature
    {
        [Constructable]
        public LavaElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lava elemental";
            Body = 720; 

            /* Lava Elemental - Fame 9,500 / Karma -9,500 */
			/* [HP Calculation]
			   - Target HP: ~30,000
			   - Fame Bonus (9,500): ~22,950
			   - SetHits Required: 7,050 (Target - Bonus)
			*/
			this.SetStr(500, 650);       
			this.SetDex(120, 180);       
			this.SetInt(300, 450);       

			// [Hits] 최종 약 28,000 ~ 32,000 타겟
			this.SetHits(5050, 9050); 
			this.SetStam(120, 180);      

			SetAttackSpeed(10.0);
			SetDamage(15, 25);     

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 60, 70);
			this.SetResistance(ResistanceType.Fire, 75, 75);     // Max 75%
			this.SetResistance(ResistanceType.Cold, -15, 0);     // 냉기에 극도로 취약
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 독 면역 (Max 75%)
			this.SetResistance(ResistanceType.Energy, 30, 45);

			this.SetSkill(SkillName.Wrestling, 95.0, 110.0);
			this.SetSkill(SkillName.Tactics, 95.0, 110.0);
			this.SetSkill(SkillName.Magery, 90.0, 105.0);

			this.VirtualArmor = 12;      
			this.Tamable = false;

			this.Fame = 9500;           
			this.Karma = -9500;

            SetAreaEffect(AreaEffect.AuraDamage);
        }
        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }
        public LavaElemental(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 2);
            AddLoot(LootPack.MedScrolls);
        }

        public override int GetAttackSound() { return 0x60A; }
        public override int GetDeathSound() { return 0x60B; }
        public override int GetHurtSound() { return 0x60C; }
        public override int GetIdleSound() { return 0x60D; }

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