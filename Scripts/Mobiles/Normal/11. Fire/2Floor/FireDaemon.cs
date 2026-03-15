using System;

namespace Server.Mobiles
{
    [CorpseName("a fire daemon corpse")]
    public class FireDaemon : BaseCreature, IAuraCreature
    {
        [Constructable]
        public FireDaemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fire daemon";
            Body = 102;
            BaseSoundID = 0x47D;

            /* Fire Daemon - Fame 20,000 / Karma -20,000 */
			/* [HP Calculation]
			   - Target HP: ~110,000
			   - Fame Bonus (20,000): ~55,550
			   - SetHits Required: 54,450 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(200, 300);       
			this.SetInt(800, 1000);      

			// [Hits] 최종 약 105,000 ~ 115,000 타겟
			this.SetHits(49450, 59450); 
			this.SetStam(200, 300);      
			this.SetMana(800, 1000);      

			SetAttackSpeed(2.5);
			SetDamage(75, 110);    

			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Fire, 60);

			this.SetResistance(ResistanceType.Physical, 65, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 75, 75);     // Max 75%
			this.SetResistance(ResistanceType.Cold, 15, 30);     
			this.SetResistance(ResistanceType.Poison, 60, 75);
			this.SetResistance(ResistanceType.Energy, 60, 75);

			this.SetSkill(SkillName.Magery, 115.0, 130.0);
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 20000;           
			this.Karma = -20000;

            //SetSpecialAbility(SpecialAbility.DragonBreath);
            SetAreaEffect(AreaEffect.AuraDamage);
        }        

        public FireDaemon(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override int TreasureMapLevel { get { return 4; } }
        public override int Meat { get { return 1; } }

        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
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
